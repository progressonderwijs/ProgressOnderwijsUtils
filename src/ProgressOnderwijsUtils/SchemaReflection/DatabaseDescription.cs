using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.SchemaReflection
{
    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class DbIdEnumAttribute : Attribute, IEnumShouldBeParameterizedInSqlAttribute { }

    [DbIdEnum]
    public enum DbObjectId { }

    /// <summary>
    /// This id is 1-based and may contain gaps due to dropping of columns.
    /// </summary>
    [DbIdEnum]
    public enum DbColumnId { }

    public struct DbNamedObjectId : IWrittenImplicitly
    {
        public DbObjectId ObjectId { get; init; }
        public string QualifiedName { get; init; }

        public static DbNamedObjectId[] LoadAllObjectsOfType(SqlConnection conn, string type)
            => SQL(
                $@"
                    select
                        ObjectId = o.object_id
                        , QualifiedName = schema_name(o.schema_id)+'.'+o.name
                    from sys.objects o
                    where o.type = {type}
                "
            ).ReadPocos<DbNamedObjectId>(conn);
    }

    public sealed class DatabaseDescription
    {
        readonly IReadOnlyDictionary<DbObjectId, Table> tableById;
        readonly IReadOnlyDictionary<DbObjectId, View> viewById;
        readonly ILookup<DbObjectId, CheckConstraint> checkConstraintsByTableId;
        readonly ILookup<DbObjectId, DmlTableTrigger> triggersByTableId;
        readonly IReadOnlyDictionary<string, Table> tableByQualifiedName;
        public readonly ILookup<string, ForeignKey> ForeignKeyConstraintsByUnqualifiedName;
        readonly ILookup<DbObjectId, ForeignKey> fksByReferencedParentObjectId;
        readonly ILookup<DbObjectId, ForeignKey> fksByReferencingChildObjectId;

        public DatabaseDescription(DbNamedObjectId[] tables, DbNamedObjectId[] views, Dictionary<DbObjectId, DbColumnMetaData[]> columns, DbForeignKey[] foreignKeys, CheckConstraintEntry[] checkConstraints, DmlTableTrigger[] dmlTableTriggers)
        {
            tableById = tables.ToDictionary(o => o.ObjectId, o => new Table(this, o, columns.GetOrDefault(o.ObjectId).EmptyIfNull()));
            viewById = views.ToDictionary(o => o.ObjectId, o => new View(o, columns.GetOrDefault(o.ObjectId).EmptyIfNull()));
            var fkObjects = foreignKeys.ArraySelect(o => new ForeignKey(o, tableById));
            fksByReferencedParentObjectId = fkObjects.ToLookup(fk => fk.ReferencedParentTable.ObjectId);
            fksByReferencingChildObjectId = fkObjects.ToLookup(fk => fk.ReferencingChildTable.ObjectId);
            checkConstraintsByTableId = checkConstraints.ToLookup(o => o.TableObjectId, o => new CheckConstraint(o, tableById[o.TableObjectId]));
            triggersByTableId = dmlTableTriggers.ToLookup(o => o.TableObjectId);
            tableByQualifiedName = tableById.Values.ToDictionary(o => o.QualifiedName, StringComparer.OrdinalIgnoreCase);
            ForeignKeyConstraintsByUnqualifiedName = fkObjects.ToLookup(o => o.UnqualifiedName, StringComparer.OrdinalIgnoreCase);
        }

        public static DatabaseDescription LoadFromSchemaTables(SqlConnection conn)
        {
            var tables = DbNamedObjectId.LoadAllObjectsOfType(conn, "U");
            var views = DbNamedObjectId.LoadAllObjectsOfType(conn, "V");
            var columnsByTableId = DbColumnMetaData.LoadAll(conn);
            return new(tables, views, columnsByTableId, ForeignKeyColumnEntry.LoadAll(conn), CheckConstraintEntry.LoadAll(conn), DmlTableTrigger.LoadAll(conn));
        }

        public IEnumerable<Table> AllTables
            => tableById.Values;

        public IEnumerable<View> AllViews
            => viewById.Values;

        public IEnumerable<CheckConstraint> AllCheckConstraints
            => checkConstraintsByTableId.SelectMany(c => c);

        public Table GetTableByName(string qualifiedName)
            => TryGetTableByName(qualifiedName) ?? throw new ArgumentException($"Unknown table '{qualifiedName}'.", nameof(qualifiedName));

        public Table? TryGetTableByName(string qualifiedName)
            => tableByQualifiedName.TryGetValue(qualifiedName, out var id) ? id : null;

        public Table? TryGetTableById(DbObjectId id)
            => tableById.GetOrDefaultR(id);

        public sealed record ForeignKeyColumn (TableColumn ReferencedParentColumn, TableColumn ReferencingChildColumn);

        public sealed class ForeignKey
        {
            public readonly Table ReferencedParentTable;
            public readonly Table ReferencingChildTable;
            public readonly IReadOnlyList<ForeignKeyColumn> Columns;
            public readonly string UnqualifiedName;
            public readonly FkReferentialAction DeleteReferentialAction;
            public readonly FkReferentialAction UpdateReferentialAction;

            internal ForeignKey(DbForeignKey o, IReadOnlyDictionary<DbObjectId, Table> tablesById)
            {
                ReferencedParentTable = tablesById[o.ReferencedParentTable];
                ReferencingChildTable = tablesById[o.ReferencingChildTable];
                Columns = o.Columns.ArraySelect(pair => new ForeignKeyColumn(ReferencedParentTable.GetByColumnIndex(pair.ReferencedParentColumn), ReferencingChildTable.GetByColumnIndex(pair.ReferencingChildColumn)));
                UnqualifiedName = o.ConstraintName;
                DeleteReferentialAction = o.DeleteReferentialAction;
                UpdateReferentialAction = o.UpdateReferentialAction;
            }

            public string QualifiedName
                => ReferencingChildTable.SchemaName + "." + UnqualifiedName;

            public ParameterizedSql ScriptToAddConstraint()
                => SQL(
                    $@"
                    alter table {ReferencingChildTable.QualifiedNameSql}
                    add constraint {ParameterizedSql.CreateDynamic(UnqualifiedName)}
                        foreign key ({ParameterizedSql.CreateDynamic(Columns.Select(fkc => fkc.ReferencingChildColumn.ColumnName).JoinStrings(", "))}) 
                        references {ReferencedParentTable.QualifiedNameSql}
                            ({ParameterizedSql.CreateDynamic(Columns.Select(fkc => fkc.ReferencedParentColumn.ColumnName).JoinStrings(", "))})
                        on delete {DeleteReferentialAction.AsSql()}
                        on update {UpdateReferentialAction.AsSql()};
                    "
                );

            public ParameterizedSql ScriptToDropConstraint()
                => SQL($"alter table {ReferencingChildTable.QualifiedNameSql} drop constraint {ParameterizedSql.CreateDynamic(UnqualifiedName)};\r\n");
        }

        public sealed class TableColumn
        {
            public readonly Table Table;
            public readonly DbColumnMetaData ColumnMetaData;

            public TableColumn(Table table, DbColumnMetaData columnMetaData)
            {
                ColumnMetaData = columnMetaData;
                Table = table;
            }

            public DbColumnId ColumnId
                => ColumnMetaData.ColumnId;

            public string ColumnName
                => ColumnMetaData.ColumnName;

            public bool IsPrimaryKey
                => ColumnMetaData.IsPrimaryKey;

            public bool IsRowVersion
                => ColumnMetaData.IsRowVersion;

            public bool IsComputed
                => ColumnMetaData.IsComputed;

            public bool IsNullable
                => ColumnMetaData.IsNullable;

            public SqlXType UserTypeId
                => ColumnMetaData.UserTypeId;

            public ParameterizedSql SqlColumnName()
                => ColumnMetaData.SqlColumnName();

            public bool IsString
                => ColumnMetaData.IsString;

            public bool IsUnicode
                => ColumnMetaData.IsUnicode;
        }

        public sealed class Table
        {
            public readonly DatabaseDescription db;
            readonly DbNamedObjectId NamedTableId;

            internal Table(DatabaseDescription db, DbNamedObjectId namedTableId, DbColumnMetaData[] columns)
            {
                this.db = db;
                NamedTableId = namedTableId;
                Columns = columns.ArraySelect(col => new TableColumn(this, col));
            }

            public DbObjectId ObjectId
                => NamedTableId.ObjectId;

            public string QualifiedName
                => NamedTableId.QualifiedName;

            public readonly TableColumn[] Columns;

            public string SchemaName
                => DbQualifiedNameUtils.SchemaFromQualifiedName(QualifiedName);

            public string UnqualifiedName
                => DbQualifiedNameUtils.UnqualifiedTableName(QualifiedName);

            public ParameterizedSql QualifiedNameSql
                => ParameterizedSql.CreateDynamic(QualifiedName);

            public IEnumerable<TableColumn> PrimaryKey
                => Columns.Where(c => c.IsPrimaryKey);

            public IEnumerable<Table> AllDependantTables
                => Utils.TransitiveClosure(
                    new[] { ObjectId },
                    reachable => db.fksByReferencedParentObjectId[reachable].Select(fk => fk.ReferencingChildTable.ObjectId)
                ).Select(id => db.tableById[id]);

            public IEnumerable<ForeignKey> KeysToReferencedParents
                => db.fksByReferencingChildObjectId[ObjectId];

            public IEnumerable<ForeignKey> KeysFromReferencingChildren
                => db.fksByReferencedParentObjectId[ObjectId];

            public IEnumerable<CheckConstraint> CheckConstraints
                => db.checkConstraintsByTableId[ObjectId];

            public IEnumerable<DmlTableTrigger> Triggers
                => db.triggersByTableId[ObjectId];

            public ForeignKeyInfo[] ChildColumnsReferencingColumn(string pkColumn)
                => KeysFromReferencingChildren
                    .SelectMany(
                        fk =>
                            fk.Columns
                                .Where(fkCol => fkCol.ReferencedParentColumn.ColumnName.EqualsOrdinalCaseInsensitive(pkColumn))
                                .Select(fkCol => new ForeignKeyInfo(fk.ReferencingChildTable.QualifiedName, fkCol.ReferencingChildColumn.ColumnName))
                    ).ToArray();

            public TableColumn GetByColumnIndex(DbColumnId columnId)
            {
                var guess = (int)columnId - 1;
                if (guess >= 0 && guess < Columns.Length && Columns[guess].ColumnId == columnId) {
                    //optimized guess: most tables aren't missing column-ids, and sql indexes are 1-based, so usually this will be where we can find a column:
                    return Columns[guess];
                }

                //...but brute force is fine too!
                foreach (var col in Columns) {
                    if (col.ColumnId == columnId) {
                        return col;
                    }
                }
                throw new ArgumentOutOfRangeException(nameof(columnId), "column index " + columnId + " not found");
            }
        }

        public sealed class View
        {
            readonly DbNamedObjectId view;
            public readonly DbColumnMetaData[] Columns;

            public View(DbNamedObjectId view, DbColumnMetaData[] columns)
            {
                this.view = view;
                Columns = columns;
            }

            public DbObjectId ObjectId
                => view.ObjectId;

            public string QualifiedName
                => view.QualifiedName;

            public string SchemaName
                => DbQualifiedNameUtils.SchemaFromQualifiedName(QualifiedName);

            public string UnqualifiedName
                => DbQualifiedNameUtils.UnqualifiedTableName(QualifiedName);

            public ParameterizedSql QualifiedNameSql
                => ParameterizedSql.CreateDynamic(QualifiedName);
        }
    }
}
