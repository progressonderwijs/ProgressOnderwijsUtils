namespace ProgressOnderwijsUtils.SchemaReflection;

public sealed record RawDatabaseDescription
{
    public sealed record ObjectDependency(DbObjectId referencing_id, DbObjectId referenced_id) : IWrittenImplicitly;

    public required DbNamedObjectId[] tables { get; init; }
    public required DbNamedObjectId[] views { get; init; }
    public required ObjectDependency[] dependencies { get; init; }
    public required Dictionary<DbObjectId, DbColumnMetaData[]> columns { get; init; }
    public required ForeignKeySqlDefinition[] foreignKeys { get; init; }
    public required CheckConstraintSqlDefinition[] checkConstraints { get; init; }
    public required DmlTableTriggerSqlDefinition[] dmlTableTriggers { get; init; }
    public required DefaultValueConstraintSqlDefinition[] defaultConstraints { get; init; }
    public required ComputedColumnSqlDefinition[] computedColumnDefinitions { get; init; }
    public required SequenceSqlDefinition[] sequences { get; init; }

    public static RawDatabaseDescription Load(SqlConnection conn)
        => new() {
            tables = DbNamedObjectId.LoadAllObjectsOfType(conn, "U"),
            views = DbNamedObjectId.LoadAllObjectsOfType(conn, "V"),
            dependencies = Load_sql_expression_dependencies(conn),
            columns = DbColumnMetaData.LoadAll(conn),
            foreignKeys = ForeignKeyColumnEntry.LoadAll(conn),
            checkConstraints = CheckConstraintSqlDefinition.LoadAll(conn),
            dmlTableTriggers = DmlTableTriggerSqlDefinition.LoadAll(conn),
            defaultConstraints = DefaultValueConstraintSqlDefinition.LoadAll(conn),
            computedColumnDefinitions = ComputedColumnSqlDefinition.LoadAll(conn),
            sequences = SequenceSqlDefinition.LoadAll(conn),
        };

    static ObjectDependency[] Load_sql_expression_dependencies(SqlConnection conn)
        => SQL(
            $@"
                select
                    sed.referencing_id
                    , sed.referenced_id
                from sys.sql_expression_dependencies sed
                where 1=1
                    and sed.referenced_id is not null
            "
        ).ReadPocos<ObjectDependency>(conn);
}
