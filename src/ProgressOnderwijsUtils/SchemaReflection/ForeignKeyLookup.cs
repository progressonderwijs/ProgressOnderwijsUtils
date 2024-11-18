namespace ProgressOnderwijsUtils.SchemaReflection;

public enum FkReferentialAction : byte
{
    //see https://docs.microsoft.com/en-us/sql/relational-databases/system-catalog-views/sys-foreign-keys-transact-sql?view=sql-server-2017 for docs
    NoAction = 0,
    Cascade = 1,
    SetNull = 2,
    SetDefault = 3,
}

public static class FkReferentialAction_AsSql
{
    public static ParameterizedSql AsSql(this FkReferentialAction action)
        => action switch {
            FkReferentialAction.NoAction => SQL($"no action"),
            FkReferentialAction.Cascade => SQL($"cascade"),
            FkReferentialAction.SetNull => SQL($"set null"),
            FkReferentialAction.SetDefault => SQL($"set default"),
            _ => throw new ArgumentOutOfRangeException(nameof(action), $"value {action} not recognized"),
        };
}

public struct ForeignKeySqlDefinition
{
    public string ConstraintName;
    public FkReferentialAction DeleteReferentialAction, UpdateReferentialAction;
    public DbObjectId ForeignKeyObjectId;
    public DbObjectId ReferencedParentTable;
    public DbObjectId ReferencingChildTable;
    public (DbColumnId ReferencedParentColumn, DbColumnId ReferencingChildColumn)[] Columns;
}

struct ForeignKeyColumnEntry : IWrittenImplicitly
{
    public DbObjectId ForeignKeyObjectId { get; init; }
    public FkReferentialAction DeleteReferentialAction { get; init; }
    public FkReferentialAction UpdateReferentialAction { get; init; }
    public DbObjectId ReferencingChildTable { get; init; }
    public DbObjectId ReferencedParentTable { get; init; }
    public DbColumnId ReferencedParentColumn { get; init; }
    public DbColumnId ReferencingChildColumn { get; init; }
    public string Name { get; init; }

    public static ForeignKeySqlDefinition[] LoadAll(SqlConnection conn)
    {
        var foreignKeys = SQL(
                $"""
                select 
                    ForeignKeyObjectId = fk.object_id
                    , DeleteReferentialAction = fk.delete_referential_action
                    , UpdateReferentialAction = fk.update_referential_action
                    , ReferencingChildTable = fk.parent_object_id
                    , ReferencedParentTable = fk.referenced_object_id
                    , ReferencedParentColumn = fkc.referenced_column_id
                    , ReferencingChildColumn = fkc.parent_column_id
                    , fk.name
                from sys.foreign_keys fk
                join sys.foreign_key_columns fkc on fkc.constraint_object_id = fk.object_id
                order by 
                    fk.object_id
                    , fkc.constraint_column_id
                """
            ).ReadPocos<ForeignKeyColumnEntry>(conn)
            .GroupBy(fkCol => fkCol.ForeignKeyObjectId)
            .Select(
                fk => {
                    var fkColEntry = fk.First();
                    return new ForeignKeySqlDefinition {
                        ForeignKeyObjectId = fk.Key,
                        ConstraintName = fkColEntry.Name,
                        DeleteReferentialAction = fkColEntry.DeleteReferentialAction,
                        UpdateReferentialAction = fkColEntry.UpdateReferentialAction,
                        ReferencingChildTable = fkColEntry.ReferencingChildTable,
                        ReferencedParentTable = fkColEntry.ReferencedParentTable,
                        Columns = fk.Select(c => (c.ReferencedParentColumn, c.ReferencingChildColumn)).ToArray(),
                    };
                }
            )
            .ToArray();

        return foreignKeys;
    }
}
