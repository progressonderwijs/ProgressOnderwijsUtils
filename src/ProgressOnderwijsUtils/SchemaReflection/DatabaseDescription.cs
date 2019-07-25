#nullable disable
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.SchemaReflection
{
    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class DbIdEnumAttribute : Attribute, IEnumShouldBeParameterizedInSqlAttribute { }

    [DbIdEnum]
    public enum DbObjectId { }

    [DbIdEnum]
    public enum DbColumnId { }

    public struct DbNamedTableId : IWrittenImplicitly
    {
        public DbObjectId ObjectId { get; set; }
        public string QualifiedName { get; set; }

        public static DbNamedTableId[] LoadAll(SqlConnection conn)
            => SQL($@"
                select
                    ObjectId = t.object_id
                    , QualifiedName = schema_name(t.schema_id) + N'.' + object_name(t.object_id)
                from sys.tables t
            ").ReadPocos<DbNamedTableId>(conn);
    }

    public sealed class DatabaseDescription
    {
        readonly IReadOnlyDictionary<DbObjectId, Table> tableById;
        readonly ForeignKeyLookup foreignKeyLookup;
        readonly Lazy<Dictionary<string, Table>> tableByQualifiedName;

        public DatabaseDescription(DbNamedTableId[] tables, Dictionary<DbObjectId, DbColumnMetaData[]> columns, ForeignKeyLookup foreignKeys)
        {
            foreignKeyLookup = foreignKeys;
            tableById = tables.ToDictionary(o => o.ObjectId, o => new Table(this, o, columns.GetOrDefault(o.ObjectId) ?? Array.Empty<DbColumnMetaData>()));
            tableByQualifiedName = Utils.Lazy(() => tableById.Values.ToDictionary(o => o.QualifiedName, StringComparer.OrdinalIgnoreCase));
        }

        public static DatabaseDescription LoadFromSchemaTables(SqlConnection conn)
        {
            var columnsByTableId = DbColumnMetaData.LoadAll(conn);
            return new DatabaseDescription(DbNamedTableId.LoadAll(conn), columnsByTableId, ForeignKeyLookup.LoadAll(conn));
        }

        public IEnumerable<Table> AllTables
            => tableById.Values;

        [NotNull]
        public Table GetTableByName(string qualifiedName)
            => TryGetTableByName(qualifiedName) ?? throw new ArgumentException($"Unknown table '{qualifiedName}'.", nameof(qualifiedName));

        [CanBeNull]
        public Table TryGetTableByName(string qualifiedName)
            => tableByQualifiedName.Value.TryGetValue(qualifiedName, out var id) ? id : null;

        [CanBeNull]
        public Table TryGetTableById(DbObjectId id)
            => tableById.GetOrDefaultR(id);

        public sealed class ForeignKey
        {
            public FkReferentialAction DeleteReferentialAction;
            public FkReferentialAction UpdateReferentialAction;
            public Table ReferencedParentTable { get; set; }
            public Table ReferencingChildTable { get; set; }
            public (TableColumn ReferencedParentColumn, TableColumn ReferencingChildColumn)[] Columns { get; set; }
            public string ForeignKeyConstraintName { get; set; }

            public static ForeignKey Create(DatabaseDescription db, DbForeignKey fk)
            {
                var parentTable = db.TryGetTableById(fk.ReferencedParentTable);
                var childTable = db.TryGetTableById(fk.ReferencingChildTable);
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
        }

        public sealed class Table
        {
            public readonly DatabaseDescription db;
            readonly DbNamedTableId NamedTableId;

            public Table(DatabaseDescription db, DbNamedTableId namedTableId, DbColumnMetaData[] columns)
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
                => db.foreignKeyLookup.AllDependantTables(ObjectId).Select(id => db.TryGetTableById(id));

            public IEnumerable<ForeignKey> KeysToReferencedParents
                => db.foreignKeyLookup.KeysByReferencingChildTable[ObjectId].Select(fk => ForeignKey.Create(db, fk));

            public IEnumerable<ForeignKey> KeysFromReferencingChildren
                => db.foreignKeyLookup.KeysByReferencedParentTable[ObjectId].Select(fk => ForeignKey.Create(db, fk));

            public ForeignKeyInfo[] ChildColumnsReferencingColumn(string pkColumn)
                => KeysFromReferencingChildren
                    .SelectMany(fk =>
                        fk.Columns
                            .Where(fkCol => fkCol.ReferencedParentColumn.ColumnName.EqualsOrdinalCaseInsensitive(pkColumn))
                            .Select(fkCol => new ForeignKeyInfo {
                                TableName = fk.ReferencingChildTable.QualifiedName,
                                ColumnName = fkCol.ReferencingChildColumn.ColumnName
                            })
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
    }
}
