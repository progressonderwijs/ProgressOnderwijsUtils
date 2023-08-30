namespace ProgressOnderwijsUtils.SchemaReflection;

public sealed record RawDatabaseDescription
{
    public sealed record ObjectDependency(DbObjectId referencing_id, DbObjectId referenced_id) : IWrittenImplicitly;

    public required string?[] Schemas { get; init; }
    public required DbNamedObjectId[] Tables { get; init; }
    public required DbNamedObjectId[] Views { get; init; }
    public required ObjectDependency[] Dependencies { get; init; }
    public required DbColumnMetaData[] Columns { get; init; }
    public required ForeignKeySqlDefinition[] ForeignKeys { get; init; }
    public required CheckConstraintSqlDefinition[] CheckConstraints { get; init; }
    public required TriggerSqlDefinition[] DmlTableTriggers { get; init; }
    public required DefaultValueConstraintSqlDefinition[] DefaultConstraints { get; init; }
    public required ComputedColumnSqlDefinition[] ComputedColumnDefinitions { get; init; }
    public required SequenceSqlDefinition[] Sequences { get; init; }
    public required DbObjectIndex[] Indexes { get; init; }
    public required DbObjectIndexColumn[] IndexColumns { get; init; }

    public static RawDatabaseDescription Load(SqlConnection conn)
        => new() {
            Schemas = Load_user_schemas(conn),
            Tables = DbNamedObjectId.LoadAllObjectsOfType(conn, "U"),
            Views = DbNamedObjectId.LoadAllObjectsOfType(conn, "V"),
            Dependencies = Load_sql_expression_dependencies(conn),
            Columns = DbColumnMetaData.LoadAll(conn),
            ForeignKeys = ForeignKeyColumnEntry.LoadAll(conn),
            CheckConstraints = CheckConstraintSqlDefinition.LoadAll(conn),
            DmlTableTriggers = TriggerSqlDefinition.LoadAllDmlTableTriggers(conn),
            DefaultConstraints = DefaultValueConstraintSqlDefinition.LoadAll(conn),
            ComputedColumnDefinitions = ComputedColumnSqlDefinition.LoadAll(conn),
            Sequences = SequenceSqlDefinition.LoadAll(conn),
            Indexes = DbObjectIndex.LoadAll(conn),
            IndexColumns = DbObjectIndexColumn.LoadAll(conn),
        };

    static string?[] Load_user_schemas(SqlConnection conn)
        => SQL(
            $"""
            select 
                s.name 
            from sys.schemas s
            where 1=1
                --no system schemas
                and 4 < s.schema_id
                and s.schema_id < 16384
            """
        ).ReadPlain<string>(conn);

    static ObjectDependency[] Load_sql_expression_dependencies(SqlConnection conn)
        => SQL(
            $"""
            select
                sed.referencing_id
                , sed.referenced_id
            from sys.sql_expression_dependencies sed
            where 1=1
                and sed.referenced_id is not null
            """
        ).ReadPocos<ObjectDependency>(conn);

    internal DatabaseDescriptionById IndexById()
        => new() {
            DefaultValues = DefaultConstraints.ToDictionary(o => (o.ParentObjectId, o.ParentColumnId)),
            ComputedColumns = ComputedColumnDefinitions.ToDictionary(o => (o.ObjectId, o.ColumnId)),
            CheckConstraints = CheckConstraints.ToGroupedDictionary(o => o.TableObjectId, (_, g) => g.ToArray()),
            Triggers = DmlTableTriggers.ToGroupedDictionary(o => o.TableObjectId, (_, g) => g.ToArray()),
            Columns = Columns.ToGroupedDictionary(col => col.DbObjectId, (_, cols) => cols.Order().ToArray()),
            SqlExpressionDependsOn = Dependencies.ToLookup(dep => dep.referencing_id, dep => dep.referenced_id),
            Indexes = Indexes.ToLookup(o => o.ObjectId),
            IndexColumns = IndexColumns.ToLookup(o => (o.ObjectId, o.IndexId)),
        };
}

sealed record DatabaseDescriptionById
{
    public required IReadOnlyDictionary<(DbObjectId ParentObjectId, DbColumnId ParentColumnId), DefaultValueConstraintSqlDefinition> DefaultValues { get; init; }
    public required IReadOnlyDictionary<(DbObjectId ObjectId, DbColumnId ColumnId), ComputedColumnSqlDefinition> ComputedColumns { get; init; }
    public required IReadOnlyDictionary<DbObjectId, CheckConstraintSqlDefinition[]> CheckConstraints { get; init; }
    public required IReadOnlyDictionary<DbObjectId, TriggerSqlDefinition[]> Triggers { get; init; }
    public required Dictionary<DbObjectId, DbColumnMetaData[]> Columns { get; init; }
    public required ILookup<DbObjectId, DbObjectId> SqlExpressionDependsOn { get; init; }
    public required ILookup<DbObjectId, DbObjectIndex> Indexes { get; init; }
    public required ILookup<(DbObjectId ObjectId, DbIndexId IndexId), DbObjectIndexColumn> IndexColumns { get; init; }
}
