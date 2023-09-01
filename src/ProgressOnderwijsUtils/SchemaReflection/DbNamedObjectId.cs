namespace ProgressOnderwijsUtils.SchemaReflection;

[DbIdEnum]
public enum DbObjectId { }

public interface IDbNamedObject
{
    DbObjectId ObjectId { get; }
    string QualifiedName { get; }
}

public struct DbNamedObjectId : IWrittenImplicitly, IDbNamedObject
{
    public DbObjectId ObjectId { get; init; }
    public string QualifiedName { get; init; }

    public static DbNamedObjectId[] LoadAllObjectsOfType(SqlConnection conn, string type)
        => SQL(
            $@"
                    select
                        ObjectId = o.object_id
                        , QualifiedName = schema_name(o.schema_id) + '.' + o.name
                    from sys.objects o
                    where 1=1
                        and o.type = {type}
                        and o.is_ms_shipped = {false} -- to filter out the dbo.dtproperties system table
                "
        ).ReadPocos<DbNamedObjectId>(conn);

    public static DbNamedObjectId[] LoadAllTableTypes(SqlConnection conn)
        => SQL(
            $"""
            select
                ObjectId = tt.type_table_object_id
                , QualifiedName = schema_name(tt.schema_id) + '.' + tt.name
            from sys.table_types tt
            """
        ).ReadPocos<DbNamedObjectId>(conn);
}
