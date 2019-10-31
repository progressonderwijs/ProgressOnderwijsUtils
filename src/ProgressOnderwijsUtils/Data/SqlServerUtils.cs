﻿using System;
using Microsoft.Data.SqlClient;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils
{
    public static class SqlServerUtils
    {
        /// <summary>
        /// If the catalog in question does not exist, this method does nothing.
        /// </summary>
        public static void KillOtherUserProcessesOnDb(SqlConnection sqlContext, string catalog)
        {
            try {
                SQL($@"
                    declare @query as nvarchar(max) = isnull((
                            select string_agg(N'kill ' + cast(s.session_id as nvarchar(max)) + N'; ', nchar(10))
                            from sys.dm_exec_sessions s
                            where 1=1
                                and s.database_id  = db_id({catalog})
                                and s.security_id <> 1
                                and s.session_id <> @@spid
                                and s.status in ('running', 'sleeping')
                        ),'');

                    exec(@query);
                ").ExecuteNonQuery(sqlContext);
            } catch (Exception e) when (IsSpidAlreadyDeadException(e)) {
                //the spid may already be dead by the time we get around to killing it, which throws an error.  We ignore that error.
            }
        }

        static bool IsSpidAlreadyDeadException(Exception exception)
            => exception.Message?.Contains("is not an active process ID.", StringComparison.Ordinal) == true
                || exception.InnerException != null && IsSpidAlreadyDeadException(exception.InnerException);
    }
}