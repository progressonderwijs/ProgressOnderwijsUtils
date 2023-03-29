namespace ProgressOnderwijsUtils.SchemaReflection;

public sealed class DatabaseDescription
{
    public readonly IReadOnlyDictionary<string, SequenceSqlDefinition> Sequences;
    readonly IReadOnlyDictionary<DbObjectId, Table> tableById;
    readonly IReadOnlyDictionary<DbObjectId, View> viewById;
    readonly IReadOnlyDictionary<string, Table> tableByQualifiedName;
    public readonly ILookup<string, ForeignKey> ForeignKeyConstraintsByUnqualifiedName;
    readonly ILookup<DbObjectId, ForeignKey> fksByReferencedParentObjectId;
    readonly ILookup<DbObjectId, ForeignKey> fksByReferencingChildObjectId;

    public DatabaseDescription(RawDatabaseDescription rawDescription)
    {

        Sequences = rawDescription.Sequences.ToDictionary(s => s.QualifiedName, StringComparer.OrdinalIgnoreCase);

        var dataByTableId = new DatabaseDescriptionById {
            DefaultValues = rawDescription.DefaultConstraints.ToDictionary(o => (o.ParentObjectId, o.ParentColumnId)),
            ComputedColumns = rawDescription.ComputedColumnDefinitions.ToDictionary(o => (o.ObjectId, o.ColumnId)),
            CheckConstraints = rawDescription.CheckConstraints.ToGroupedDictionary(o => o.TableObjectId, (_, g) => g.ToArray()),
            Triggers = rawDescription.DmlTableTriggers.ToGroupedDictionary(o => o.TableObjectId, (_, g) => g.ToArray()),
            Columns = rawDescription.Columns.ToGroupedDictionary(col => col.DbObjectId, (_, cols) => cols.Order().ToArray()),
            SqlExpressionDependsOn = rawDescription.Dependencies.ToLookup(dep => dep.referencing_id, dep => dep.referenced_id),
        };

        tableById = rawDescription.Tables.ToDictionary(o => o.ObjectId, o => new Table(this, o, dataByTableId.Columns.GetValueOrDefault(o.ObjectId).EmptyIfNull(), dataByTableId));
        viewById = rawDescription.Views.ToDictionary(o => o.ObjectId, o => new View(o, dataByTableId.Columns.GetValueOrDefault(o.ObjectId).EmptyIfNull(), dataByTableId.SqlExpressionDependsOn[o.ObjectId].Select(dep => tableById.GetValueOrDefault(dep)).WhereNotNull().ToArray()));
        var fkObjects = rawDescription.ForeignKeys.ArraySelect(o => new ForeignKey(o, tableById));
        fksByReferencedParentObjectId = fkObjects.ToLookup(fk => fk.ReferencedParentTable.ObjectId);
        fksByReferencingChildObjectId = fkObjects.ToLookup(fk => fk.ReferencingChildTable.ObjectId);
        tableByQualifiedName = tableById.Values.ToDictionary(o => o.QualifiedName, StringComparer.OrdinalIgnoreCase);
        ForeignKeyConstraintsByUnqualifiedName = fkObjects.ToLookup(o => o.UnqualifiedName, StringComparer.OrdinalIgnoreCase);
    }

    internal sealed record DatabaseDescriptionById
    {
        public required IReadOnlyDictionary<(DbObjectId ParentObjectId, DbColumnId ParentColumnId), DefaultValueConstraintSqlDefinition> DefaultValues { get; init; }
        public required IReadOnlyDictionary<(DbObjectId ObjectId, DbColumnId ColumnId), ComputedColumnSqlDefinition> ComputedColumns { get; init; }
        public required IReadOnlyDictionary<DbObjectId, CheckConstraintSqlDefinition[]> CheckConstraints { get; init; }
        public required IReadOnlyDictionary<DbObjectId, DmlTableTriggerSqlDefinition[]> Triggers { get; init; }
        public required Dictionary<DbObjectId, DbColumnMetaData[]> Columns { get; init; }
        public required ILookup<DbObjectId, DbObjectId> SqlExpressionDependsOn { get; init; }
    }

    public static DatabaseDescription LoadFromSchemaTables(SqlConnection conn)
        => new(RawDatabaseDescription.Load(conn));

    public IEnumerable<Table> AllTables
        => tableById.Values;

    public IEnumerable<View> AllViews
        => viewById.Values;

    public IEnumerable<CheckConstraintSqlDefinition> AllCheckConstraints
        => AllTables.SelectMany(t => t.CheckConstraints);

    public Table GetTableByName(string qualifiedName)
        => TryGetTableByName(qualifiedName) ?? throw new ArgumentException($"Unknown table '{qualifiedName}'.", nameof(qualifiedName));

    public Table? TryGetTableByName(string qualifiedName)
        => tableByQualifiedName.TryGetValue(qualifiedName, out var id) ? id : null;

    public Table? TryGetTableById(DbObjectId id)
        => tableById.GetValueOrDefault(id);

    public sealed record ForeignKeyColumn(TableColumn ReferencedParentColumn, TableColumn ReferencingChildColumn);

    public sealed class ForeignKey
    {
        public readonly Table ReferencedParentTable;
        public readonly Table ReferencingChildTable;
        public readonly IReadOnlyList<ForeignKeyColumn> Columns;
        public readonly string UnqualifiedName;
        public readonly FkReferentialAction DeleteReferentialAction;
        public readonly FkReferentialAction UpdateReferentialAction;

        internal ForeignKey(ForeignKeySqlDefinition fkDef, IReadOnlyDictionary<DbObjectId, Table> tablesById)
        {
            ReferencedParentTable = tablesById[fkDef.ReferencedParentTable];
            ReferencingChildTable = tablesById[fkDef.ReferencingChildTable];
            Columns = fkDef.Columns.ArraySelect(pair => new ForeignKeyColumn(ReferencedParentTable.GetByColumnIndex(pair.ReferencedParentColumn), ReferencingChildTable.GetByColumnIndex(pair.ReferencingChildColumn)));
            UnqualifiedName = fkDef.ConstraintName;
            DeleteReferentialAction = fkDef.DeleteReferentialAction;
            UpdateReferentialAction = fkDef.UpdateReferentialAction;
        }

        public string QualifiedName
            => $"{ReferencingChildTable.SchemaName}.{UnqualifiedName}";

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
            => SQL($"alter table {ReferencingChildTable.QualifiedNameSql} drop constraint {ParameterizedSql.CreateDynamic(UnqualifiedName)};\n");
    }

    public sealed class TableColumn
    {
        public readonly Table Table;
        public readonly DbColumnMetaData ColumnMetaData;
        public readonly DefaultValueConstraintSqlDefinition? DefaultValueConstraint;
        public readonly ComputedColumnSqlDefinition? ComputedAs;

        internal TableColumn(Table table, DbColumnMetaData columnMetaData, DatabaseDescriptionById dataByTableId)
        {
            ColumnMetaData = columnMetaData;
            Table = table;
            DefaultValueConstraint = dataByTableId.DefaultValues.GetValueOrDefault((columnMetaData.DbObjectId, columnMetaData.ColumnId));
            ComputedAs = dataByTableId.ComputedColumns.GetValueOrDefault((columnMetaData.DbObjectId, columnMetaData.ColumnId));
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

        public SqlSystemTypeId UserTypeId
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
        public readonly TableColumn[] Columns;
        public readonly DmlTableTriggerSqlDefinition[] Triggers;
        public readonly CheckConstraintSqlDefinition[] CheckConstraints;
        readonly DbNamedObjectId NamedTableId;
        public readonly DatabaseDescription db;

        internal Table(DatabaseDescription db, DbNamedObjectId namedTableId, DbColumnMetaData[] columns, DatabaseDescriptionById dataByTableId)
        {
            this.db = db;
            NamedTableId = namedTableId;
            Columns = columns.ArraySelect(col => new TableColumn(this, col, dataByTableId));
            Triggers = dataByTableId.Triggers.GetValueOrDefault(ObjectId).EmptyIfNull();
            CheckConstraints = dataByTableId.CheckConstraints.GetValueOrDefault(ObjectId).EmptyIfNull();
        }

        public DbObjectId ObjectId
            => NamedTableId.ObjectId;

        public string QualifiedName
            => NamedTableId.QualifiedName;

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
                new[] { ObjectId, },
                reachable => db.fksByReferencedParentObjectId[reachable].Select(fk => fk.ReferencingChildTable.ObjectId)
            ).Select(id => db.tableById[id]);

        public IEnumerable<ForeignKey> KeysToReferencedParents
            => db.fksByReferencingChildObjectId[ObjectId];

        public IEnumerable<ForeignKey> KeysFromReferencingChildren
            => db.fksByReferencedParentObjectId[ObjectId];

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
            throw new ArgumentOutOfRangeException(nameof(columnId), $"column index {columnId} not found");
        }
    }

    public sealed class View
    {
        readonly DbNamedObjectId view;
        public readonly DbColumnMetaData[] Columns;
        public readonly Table[] ReferencedTables;

        public View(DbNamedObjectId view, DbColumnMetaData[] columns, Table[] referencedTables)
        {
            this.view = view;
            Columns = columns;
            ReferencedTables = referencedTables;
        }

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
