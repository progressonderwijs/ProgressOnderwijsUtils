using System;
using System.Collections.Generic;
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
        public static ForeignKeyLookup LoadAll(SqlCommandCreationContext conn)
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
            ").ReadMetaObjects<ForeignKeyColumnEntry>(conn)
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

    struct ForeignKeyColumnEntry : IMetaObject
    {
        public DbObjectId ForeignKeyObjectId { get; set; }
        public FkReferentialAction DeleteReferentialAction { get; set; }
        public FkReferentialAction UpdateReferentialAction { get; set; }
        public DbObjectId ReferencingChildTable { get; set; }
        public DbObjectId ReferencedParentTable { get; set; }
        public DbColumnId ReferencedParentColumn { get; set; }
        public DbColumnId ReferencingChildColumn { get; set; }
        public string Name { get; set; }
    }
}
