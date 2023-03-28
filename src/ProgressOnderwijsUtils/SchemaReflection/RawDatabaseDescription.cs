namespace ProgressOnderwijsUtils.SchemaReflection;

public sealed record RawDatabaseDescription
{
    public sealed record ObjectDependency(DbObjectId referencing_id, DbObjectId referenced_id) : IWrittenImplicitly;

    public required DbNamedObjectId[] tables { get; init; }
    public required DbNamedObjectId[] views { get; init; }
    public required ILookup<DbObjectId, DbObjectId> dependencies { get; init; }
    public required Dictionary<DbObjectId, DbColumnMetaData[]> columns { get; init; }
    public required ForeignKeySqlDefinition[] foreignKeys { get; init; }
    public required CheckConstraintSqlDefinition[] checkConstraints { get; init; }
    public required DmlTableTriggerSqlDefinition[] dmlTableTriggers { get; init; }
    public required DefaultValueConstraintSqlDefinition[] defaultConstraints { get; init; }
    public required ComputedColumnSqlDefinition[] computedColumnDefinitions { get; init; }
    public required SequenceSqlDefinition[] sequences { get; init; }

    public static RawDatabaseDescription Load(SqlConnection conn)
    {
        var tables = DbNamedObjectId.LoadAllObjectsOfType(conn, "U");
        var views = DbNamedObjectId.LoadAllObjectsOfType(conn, "V");
        var dependencies = SQL(
            $@"
                select
                    sed.referencing_id
                    , sed.referenced_id
                from sys.sql_expression_dependencies sed
                where 1=1
                    and sed.referenced_id is not null
            "
        ).ReadPocos<ObjectDependency>(conn).ToLookup(dep => dep.referencing_id, dep => dep.referenced_id);

        var rawDescription = new RawDatabaseDescription {
            tables = tables,
            views = views,
            dependencies = dependencies,
            columns = DbColumnMetaData.LoadAll(conn),
            foreignKeys = ForeignKeyColumnEntry.LoadAll(conn),
            checkConstraints = CheckConstraintSqlDefinition.LoadAll(conn),
            dmlTableTriggers = DmlTableTriggerSqlDefinition.LoadAll(conn),
            defaultConstraints = DefaultValueConstraintSqlDefinition.LoadAll(conn),
            computedColumnDefinitions = ComputedColumnSqlDefinition.LoadAll(conn),
            sequences = SequenceSqlDefinition.LoadAll(conn),
        };
        return rawDescription;
    }
}
