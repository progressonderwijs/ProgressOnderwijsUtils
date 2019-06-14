using System.Collections.Generic;
using System.Data.SqlClient;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.SchemaReflection;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// Contains extension methods to insert metaobjects (POCOs) into database tables using SqlBulkCopy.
    /// </summary>
    public static class MetaObjectBulkCopy
    {
        /// <summary>
        /// Performs a bulk insert.  Maps columns based on name, not order (unlike SqlBulkCopy by default) and checks constraints.
        /// For more fine-grained control, create a BulkInsertTarget instance instead of using DatabaseDescription.Table.
        /// </summary>
        public static void BulkCopyToSqlServer<[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
            T>([NotNull] this IEnumerable<T> metaObjects, [NotNull] SqlConnection sqlConn, [NotNull] DatabaseDescription.Table table, CommandTimeout timeout)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
            => BulkCopyToSqlServer(metaObjects, sqlConn, BulkInsertTarget.FromDatabaseDescription(table), timeout);

        /// <summary>
        /// Performs a bulk insert.  Maps columns based on name, not order (unlike SqlBulkCopy by default) and checks constraints.
        /// For more fine-grained control, create a BulkInsertTarget instance instead of using DatabaseDescription.Table.
        /// </summary>
        public static void BulkCopyToSqlServer<[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
            T>([NotNull] this IEnumerable<T> metaObjects, [NotNull] SqlConnection sqlConn, [NotNull] DatabaseDescription.Table table)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
            => BulkCopyToSqlServer(metaObjects, sqlConn, table, CommandTimeout.DeferToConnectionDefault);

        /// <summary>
        /// Performs a bulk insert.  Maps columns based on name, not order (unlike SqlBulkCopy by default).
        /// </summary>
        public static void BulkCopyToSqlServer<[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
            T>([NotNull] this IEnumerable<T> metaObjects, SqlConnection sqlConn, [NotNull] BulkInsertTarget target)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
            => BulkCopyToSqlServer(metaObjects, sqlConn, target, CommandTimeout.DeferToConnectionDefault);

        /// <summary>
        /// Performs a bulk insert.  Maps columns based on name, not order (unlike SqlBulkCopy by default).
        /// </summary>
        public static void BulkCopyToSqlServer<[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
            T>([NotNull] this IEnumerable<T> metaObjects, SqlConnection sqlConn, [NotNull] BulkInsertTarget target, CommandTimeout timeout)
            where T : IMetaObject, IPropertiesAreUsedImplicitly
            => target.BulkInsert(sqlConn, metaObjects, timeout);
    }
}
