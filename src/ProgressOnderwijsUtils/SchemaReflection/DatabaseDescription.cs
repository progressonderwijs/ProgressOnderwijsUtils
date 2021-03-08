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
        public DbObjectId ObjectId { get; set; }
        public string QualifiedName { get; set; }

        public static DbNamedObjectId[] LoadAllObjectsOfType(SqlConnection conn, string type)
            => SQL($@"
                    select
                        ObjectId = o.object_id
                        , QualifiedName = schema_name(o.schema_id)+'.'+o.name
                    from sys.objects o
                    where o.type = {type}
                ").ReadPocos<DbNamedObjectId>(conn);
    }

    public sealed class DatabaseDescription
    {
        readonly IReadOnlyDictionary<DbObjectId, Table> tableById;
        readonly IReadOnlyDictionary<DbObjectId, View> viewById;
        readonly ILookup<DbObjectId, CheckConstraint> checkConstraintsByTableId;
        readonly ForeignKeyLookup foreignKeyLookup;
        readonly Lazy<Dictionary<string, Table>> tableByQualifiedName;

        public DatabaseDescription(DbNamedObjectId[] tables, DbNamedObjectId[] views, Dictionary<DbObjectId, DbColumnMetaData[]> columns, ForeignKeyLookup foreignKeys, CheckConstraintEntry[] checkConstraints)
        {
            foreignKeyLookup = foreignKeys;
            tableById = tables.ToDictionary(o => o.ObjectId, o => new Table(this, o, columns.GetOrDefault(o.ObjectId) ?? Array.Empty<DbColumnMetaData>()));
            viewById = views.ToDictionary(o => o.ObjectId, o => new View(o, columns.GetOrDefault(o.ObjectId) ?? Array.Empty<DbColumnMetaData>()));
            checkConstraintsByTableId = checkConstraints.ToLookup(o => o.TableObjectId, o => new CheckConstraint(o, tableById[o.TableObjectId]));
            tableByQualifiedName = Utils.Lazy(() => tableById.Values.ToDictionary(o => o.QualifiedName, StringComparer.OrdinalIgnoreCase));
        }

        public static DatabaseDescription LoadFromSchemaTables(SqlConnection conn)
        {
            var tables = DbNamedObjectId.LoadAllObjectsOfType(conn, "U");
            var views = DbNamedObjectId.LoadAllObjectsOfType(conn, "V");
            var columnsByTableId = DbColumnMetaData.LoadAll(conn);
            return new DatabaseDescription(tables, views, columnsByTableId, ForeignKeyLookup.LoadAll(conn), CheckConstraintEntry.LoadAll(conn));
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
            => tableByQualifiedName.Value.TryGetValue(qualifiedName, out var id) ? id : null;

        public Table? TryGetTableById(DbObjectId id)
            => tableById.GetOrDefaultR(id);

        public sealed class ForeignKey
        {
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
            public FkReferentialAction DeleteReferentialAction;
            public FkReferentialAction UpdateReferentialAction;
            public Table ReferencedParentTable { get; set; }
            public Table ReferencingChildTable { get; set; }
            public (TableColumn ReferencedParentColumn, TableColumn ReferencingChildColumn)[] Columns { get; set; }
            public string ForeignKeyConstraintName { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

            public static ForeignKey Create(DatabaseDescription db, DbForeignKey fk)
            {
                // Let's trust callers not to make up object ids
                var parentTable = db.TryGetTableById(fk.ReferencedParentTable)!;
                var childTable = db.TryGetTableById(fk.ReferencingChildTable)!;
                return new ForeignKey {
                    ReferencedParentTable = parentTable,
                    ReferencingChildTable = childTable,
                    Columns = fk.Columns.ArraySelect(pair => (parentTable.GetByColumnIndex(pair.ReferencedParentColumn), childTable.GetByColumnIndex(pair.ReferencingChildColumn))),
                    ForeignKeyConstraintName = fk.ConstraintName,
                    DeleteReferentialAction = fk.DeleteReferentialAction,
                    UpdateReferentialAction = fk.UpdateReferentialAction,
                };
            }

            public ParameterizedSql ScriptToAddConstraint()
                => SQL($@"
                    alter table {ReferencingChildTable.QualifiedNameSql}
                    add constraint {ParameterizedSql.CreateDynamic(ForeignKeyConstraintName)}
                        foreign key ({ParameterizedSql.CreateDynamic(Columns.Select(fkc => fkc.ReferencingChildColumn.ColumnName).JoinStrings(", "))}) 
                        references {ReferencedParentTable.QualifiedNameSql}
                            ({ParameterizedSql.CreateDynamic(Columns.Select(fkc => fkc.ReferencedParentColumn.ColumnName).JoinStrings(", "))})
                        on delete {DeleteReferentialAction.AsSql()}
                        on update {UpdateReferentialAction.AsSql()};
                    ");

            public ParameterizedSql ScriptToDropConstraint()
                => SQL($"alter table {ReferencingChildTable.QualifiedNameSql} drop constraint {ParameterizedSql.CreateDynamic(ForeignKeyConstraintName)};\r\n");
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

            public bool Is_Primary_Key
                => ColumnMetaData.IsPrimaryKey;

            public bool Is_RowVersion
                => ColumnMetaData.IsRowVersion;

            public bool Is_Computed
                => ColumnMetaData.IsComputed;

            public bool Is_Nullable
                => ColumnMetaData.IsNullable;

            public SqlXType User_Type_Id
                => ColumnMetaData.UserTypeId;

            public ParameterizedSql SqlColumnName()
                => ColumnMetaData.SqlColumnName();

            public bool Is_String
                => ColumnMetaData.IsString;
            public bool Is_Unicode
                => ColumnMetaData.IsUnicode;
        }

        public sealed class Table
        {
            public readonly DatabaseDescription db;
            readonly DbNamedObjectId NamedTableId;

            public Table(DatabaseDescription db, DbNamedObjectId namedTableId, DbColumnMetaData[] columns)
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
                => Columns.Where(c => c.Is_Primary_Key);

            public IEnumerable<Table> AllDependantTables
                => db.foreignKeyLookup.AllDependantTables(ObjectId).Select(id => db.TryGetTableById(id)!);

            public IEnumerable<ForeignKey> KeysToReferencedParents
                => db.foreignKeyLookup.KeysByReferencingChildTable[ObjectId].Select(fk => ForeignKey.Create(db, fk));

            public IEnumerable<ForeignKey> KeysFromReferencingChildren
                => db.foreignKeyLookup.KeysByReferencedParentTable[ObjectId].Select(fk => ForeignKey.Create(db, fk));

            public IEnumerable<CheckConstraint> CheckConstraints
                => db.checkConstraintsByTableId[ObjectId];

            public ForeignKeyInfo[] ChildColumnsReferencingColumn(string pkColumn)
                => KeysFromReferencingChildren
                    .SelectMany(fk =>
                        fk.Columns
                            .Where(fkCol => fkCol.ReferencedParentColumn.ColumnName.EqualsOrdinalCaseInsensitive(pkColumn))
                            .Select(fkCol => new ForeignKeyInfo(fk.ReferencingChildTable.QualifiedName,fkCol.ReferencingChildColumn.ColumnName))
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
