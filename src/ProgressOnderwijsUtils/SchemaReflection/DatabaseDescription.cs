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
        var rawSchemaById = new DatabaseDescriptionById {
            DefaultValues = rawDescription.DefaultConstraints.ToDictionary(o => (o.ParentObjectId, o.ParentColumnId)),
            ComputedColumns = rawDescription.ComputedColumnDefinitions.ToDictionary(o => (o.ObjectId, o.ColumnId)),
            CheckConstraints = rawDescription.CheckConstraints.ToGroupedDictionary(o => o.TableObjectId, (_, g) => g.ToArray()),
            Triggers = rawDescription.DmlTableTriggers.ToGroupedDictionary(o => o.TableObjectId, (_, g) => g.ToArray()),
            Columns = rawDescription.Columns.ToGroupedDictionary(col => col.DbObjectId, (_, cols) => cols.Order().ToArray()),
            SqlExpressionDependsOn = rawDescription.Dependencies.ToLookup(dep => dep.referencing_id, dep => dep.referenced_id),
            Indexes = rawDescription.Indexes.ToLookup(o => o.ObjectId),
            IndexColumns = rawDescription.IndexColumns.ToLookup(o => (o.ObjectId, o.IndexId)),
        };

        Sequences = rawDescription.Sequences.ToDictionary(s => s.QualifiedName, StringComparer.OrdinalIgnoreCase);
        tableById = rawDescription.Tables.ToDictionary(o => o.ObjectId, o => new Table(o, rawSchemaById, this));
        viewById = rawDescription.Views.ToDictionary(o => o.ObjectId, o => new View(o, rawSchemaById, this));
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
        public required ILookup<DbObjectId, DbObjectIndex> Indexes { get; init; }
        public required ILookup<(DbObjectId ObjectId, DbIndexId IndexId), DbObjectIndexColumn> IndexColumns { get; init; }
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

    public sealed record ForeignKeyColumn(Column<Table> ReferencedParentColumn, Column<Table> ReferencingChildColumn);

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
            Columns = fkDef.Columns.ArraySelect(pair => new ForeignKeyColumn(ReferencedParentTable.ColumnsById[pair.ReferencedParentColumn], ReferencingChildTable.ColumnsById[pair.ReferencingChildColumn]));
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

    public sealed class Column<TObject> : IDbColumn
        where TObject : IDbNamedObject
    {
        public readonly TObject ContainingObject;
        public DbColumnMetaData ColumnMetaData { get; }
        public readonly DefaultValueConstraintSqlDefinition? DefaultValueConstraint;
        public readonly ComputedColumnSqlDefinition? ComputedAs;

        internal Column(TObject containingObject, DbColumnMetaData columnMetaData, DatabaseDescriptionById rawSchemaById)
        {
            ColumnMetaData = columnMetaData;
            ContainingObject = containingObject;
            DefaultValueConstraint = rawSchemaById.DefaultValues.GetValueOrDefault((columnMetaData.DbObjectId, columnMetaData.ColumnId));
            ComputedAs = rawSchemaById.ComputedColumns.GetValueOrDefault((columnMetaData.DbObjectId, columnMetaData.ColumnId));
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

        public short MaxLength
            => ColumnMetaData.MaxLength;

        public byte Precision
            => ColumnMetaData.Precision;

        public byte Scale
            => ColumnMetaData.Scale;

        public bool HasAutoIncrementIdentity
            => ColumnMetaData.HasAutoIncrementIdentity;
    }

    static Column<TObject> DefineColumn<TObject>(TObject containingObject, DatabaseDescriptionById rawSchemaById, DbColumnMetaData col)
        where TObject : IDbNamedObject
        => new(containingObject, col, rawSchemaById);

    public interface IObjectWithColumns<TObject> : IDbNamedObject
        where TObject : IObjectWithColumns<TObject>
    {
        Column<TObject>[] Columns { get; }
        IReadOnlyDictionary<DbColumnId, Column<TObject>> ColumnsById { get; }
    }

    public sealed class Table : IObjectWithColumns<Table>
    {
        public Column<Table>[] Columns { get; }
        public IReadOnlyDictionary<DbColumnId, Column<Table>> ColumnsById { get; }
        public readonly DmlTableTriggerSqlDefinition[] Triggers;
        public readonly CheckConstraintSqlDefinition[] CheckConstraints;
        readonly DbNamedObjectId NamedTableId;
        public DatabaseDescription Database { get; }

        internal Table(DbNamedObjectId namedTableId, DatabaseDescriptionById rawSchemaById, DatabaseDescription database)
        {
            Database = database;
            NamedTableId = namedTableId;
            Columns = rawSchemaById.Columns.GetValueOrDefault(namedTableId.ObjectId).EmptyIfNull().ArraySelect(col => DefineColumn(this, rawSchemaById, col));
            ColumnsById = Columns.ToDictionary(o => o.ColumnId);
            Triggers = rawSchemaById.Triggers.GetValueOrDefault(ObjectId).EmptyIfNull();
            CheckConstraints = rawSchemaById.CheckConstraints.GetValueOrDefault(ObjectId).EmptyIfNull();
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

        public IEnumerable<Column<Table>> PrimaryKey
            => Columns.Where(c => c.IsPrimaryKey);

        public IEnumerable<Table> AllDependantTables
            => Utils.TransitiveClosure(
                new[] { ObjectId, },
                reachable => Database.fksByReferencedParentObjectId[reachable].Select(fk => fk.ReferencingChildTable.ObjectId)
            ).Select(id => Database.tableById[id]);

        public IEnumerable<ForeignKey> KeysToReferencedParents
            => Database.fksByReferencingChildObjectId[ObjectId];

        public IEnumerable<ForeignKey> KeysFromReferencingChildren
            => Database.fksByReferencedParentObjectId[ObjectId];

        public ForeignKeyInfo[] ChildColumnsReferencingColumn(string pkColumn)
            => KeysFromReferencingChildren
                .SelectMany(
                    fk =>
                        fk.Columns
                            .Where(fkCol => fkCol.ReferencedParentColumn.ColumnName.EqualsOrdinalCaseInsensitive(pkColumn))
                            .Select(fkCol => new ForeignKeyInfo(fk.ReferencingChildTable.QualifiedName, fkCol.ReferencingChildColumn.ColumnName))
                ).ToArray();
    }

    public sealed class View : IObjectWithColumns<View>
    {
        readonly DbNamedObjectId NamedObject;
        public DatabaseDescription Database { get; }
        public Column<View>[] Columns { get; }
        public readonly Table[] ReferencedTables;

        internal View(DbNamedObjectId namedObject, DatabaseDescriptionById rawSchemaById, DatabaseDescription db)
        {
            NamedObject = namedObject;
            Database = db;
            Columns = rawSchemaById.Columns.GetValueOrDefault(namedObject.ObjectId).EmptyIfNull().ArraySelect(col => DefineColumn(this, rawSchemaById, col));
            ReferencedTables = rawSchemaById.SqlExpressionDependsOn[namedObject.ObjectId].Select(db.TryGetTableById).WhereNotNull().ToArray();
            ColumnsById = Columns.ToDictionary(o => o.ColumnId);
        }

        public IReadOnlyDictionary<DbColumnId, Column<View>> ColumnsById { get; }

        public DbObjectId ObjectId
            => NamedObject.ObjectId;

        public string QualifiedName
            => NamedObject.QualifiedName;

        public string SchemaName
            => DbQualifiedNameUtils.SchemaFromQualifiedName(QualifiedName);

        public string UnqualifiedName
            => DbQualifiedNameUtils.UnqualifiedTableName(QualifiedName);

        public ParameterizedSql QualifiedNameSql
            => ParameterizedSql.CreateDynamic(QualifiedName);
    }
}
