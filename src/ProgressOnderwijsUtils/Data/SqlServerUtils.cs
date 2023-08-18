namespace ProgressOnderwijsUtils;

public static class SqlServerUtils
{
    /// <summary>
    /// If the catalog in question does not exist, this method does nothing.
    /// </summary>
    public static void KillOtherUserProcessesOnDb(SqlConnection sqlContext, string catalog)
    {
        try {
            SQL(
                $@"
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
                "
            ).ExecuteNonQuery(sqlContext);
        } catch (Exception e) when (IsSpidAlreadyDeadException(e)) {
            //the spid may already be dead by the time we get around to killing it, which throws an error.  We ignore that error.
        }
    }

    static bool IsSpidAlreadyDeadException(Exception exception)
        => exception.Message.PretendNullable() != null && exception.Message.Contains("is not an active process ID.", StringComparison.Ordinal)
            || exception.InnerException != null && IsSpidAlreadyDeadException(exception.InnerException);

    public static string PrettifySqlExpression(string sql)
    {
        var alomstPretty = PrettifySqlExpressionLeaveParens(sql);
        var withoutPointlessParens = Regex.Replace(alomstPretty, @"\((\w+)\)", m => m.Value[1..^1]);
        return withoutPointlessParens;
    }

    public static string PrettifySqlExpressionLeaveParens(string sql)
    {
        var uncappedKeyWords = Regex.Replace(sql, @"\b(NEXT|VALUE|FOR|IS|NOT|NULL|OR|AND|CONVERT|TRY_CAST|AS)\b", m => m.Value.ToLowerInvariant());
        var withoutPointlessBrackets = Regex.Replace(uncappedKeyWords, @"\[(\w+)\]", m => m.Value[1..^1]);
        return withoutPointlessBrackets;
    }
}
