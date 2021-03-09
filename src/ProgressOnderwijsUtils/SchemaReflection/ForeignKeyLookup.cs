using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.SchemaReflection
{
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
            => action == FkReferentialAction.NoAction ? SQL($"no action")
                : action == FkReferentialAction.Cascade ? SQL($"cascade")
                : action == FkReferentialAction.SetNull ? SQL($"set null")
                : action == FkReferentialAction.SetDefault ? SQL($"set default")
                : throw new ArgumentOutOfRangeException(nameof(action), "value " + action + " not recognized");
    }

    public struct DbForeignKey
    {
        public string ConstraintName;
        public FkReferentialAction DeleteReferentialAction, UpdateReferentialAction;
        public DbObjectId ReferencedParentTable;
        public DbObjectId ReferencingChildTable;
        public (DbColumnId ReferencedParentColumn, DbColumnId ReferencingChildColumn)[] Columns;
    }

    public sealed class ForeignKeyLookup
    {
        public static ForeignKeyLookup LoadAll(SqlConnection conn)
        {
            var foreignKeys = SQL($@"
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
            ").ReadPocos<ForeignKeyColumnEntry>(conn)
                .GroupBy(fkCol => fkCol.ForeignKeyObjectId)
                .Select(fk => {
                    var fkColEntry = fk.First();
                    return new DbForeignKey {
                        ConstraintName = fkColEntry.Name,
                        DeleteReferentialAction = fkColEntry.DeleteReferentialAction,
                        UpdateReferentialAction = fkColEntry.UpdateReferentialAction,
                        ReferencingChildTable = fkColEntry.ReferencingChildTable,
                        ReferencedParentTable = fkColEntry.ReferencedParentTable,
                        Columns = fk.Select(c => (c.ReferencedParentColumn, c.ReferencingChildColumn)).ToArray()
                    };
                })
                .ToArray();

            return new ForeignKeyLookup(foreignKeys);
        }

        public readonly ILookup<DbObjectId, DbForeignKey> KeysByReferencedParentTable;
        public readonly ILookup<DbObjectId, DbForeignKey> KeysByReferencingChildTable;

        public ForeignKeyLookup(DbForeignKey[] foreignKeys)
        {
            KeysByReferencedParentTable = foreignKeys.EmptyIfNull().ToLookup(fk => fk.ReferencedParentTable);
            KeysByReferencingChildTable = foreignKeys.EmptyIfNull().ToLookup(fk => fk.ReferencingChildTable);
        }

        public HashSet<DbObjectId> AllDependantTables(DbObjectId table)
            => Utils.TransitiveClosure(new[] { table }, reachable => KeysByReferencedParentTable[reachable].Select(fk => fk.ReferencingChildTable));
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
    }
}
