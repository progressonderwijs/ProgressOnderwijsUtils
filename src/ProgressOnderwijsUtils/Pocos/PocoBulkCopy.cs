using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.SchemaReflection;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// Contains extension methods to insert POCOs into database tables using SqlBulkCopy.
    /// </summary>
    public static class PocoBulkCopy
    {
        /// <summary>
        /// Performs a bulk insert.  Maps columns based on name, not order (unlike SqlBulkCopy by default) and checks constraints.
        /// For more fine-grained control, create a BulkInsertTarget instance instead of using DatabaseDescription.Table.
        /// </summary>
        public static void BulkCopyToSqlServer<[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)] T>(this IEnumerable<T> pocos, SqlConnection sqlConn, DatabaseDescription.Table table, CommandTimeout timeout = new())
            where T : IReadImplicitly
            => BulkCopyToSqlServer(pocos, sqlConn, BulkInsertTarget.FromDatabaseDescription(table), timeout);

        /// <summary>
        /// Performs a bulk insert.  Maps columns based on name, not order (unlike SqlBulkCopy by default).
        /// </summary>
        public static void BulkCopyToSqlServer<[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)] T>(this IEnumerable<T> pocos, SqlConnection sqlConn, BulkInsertTarget target, CommandTimeout timeout = new())
            where T : IReadImplicitly
            => target.BulkInsert(sqlConn, pocos, timeout);
    }
}
