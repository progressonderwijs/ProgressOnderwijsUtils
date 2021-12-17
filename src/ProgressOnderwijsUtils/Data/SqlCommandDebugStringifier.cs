namespace ProgressOnderwijsUtils;

public static class SqlCommandDebugStringifier
{
    public static string DebugFriendlyCommandText(SqlCommand sqlCommand, SqlTracerAgumentInclusion includeSensitiveInfo)
        => CommandParamStringOrEmpty(sqlCommand, includeSensitiveInfo) + sqlCommand.CommandText;

    static string CommandParamStringOrEmpty(SqlCommand sqlCommand, SqlTracerAgumentInclusion includeSensitiveInfo)
    {
        if (includeSensitiveInfo == SqlTracerAgumentInclusion.IncludingArgumentValues) {
            return CommandParamString(sqlCommand);
        } else {
            return "";
        }
    }

    static string CommandParamString(SqlCommand sqlCommand)
        => sqlCommand.Parameters.Cast<SqlParameter>().Select(DeclareParameter).JoinStrings();

    static string DeclareParameter(SqlParameter par)
    {
        bool isStructured;
        string pseudoSqlType;

        try {
            isStructured = par.SqlDbType == SqlDbType.Structured;
            pseudoSqlType = isStructured
                ? par.TypeName
                : par.SqlDbType + (par.SqlDbType == SqlDbType.NVarChar ? "(max)" : "");
        } catch {
            isStructured = false;
            pseudoSqlType = $"[Unmappable; C#:{par.Value?.GetType().ToCSharpFriendlyTypeName()}]";
            //in debug scenarios, I don't want this to throw
        }
        var declareVariable = $"DECLARE {par.ParameterName} AS {pseudoSqlType}";
        if (!isStructured) {
            return $"{declareVariable} = {InsecureSqlDebugString(par.Value, true)};\n";
        } else {
            return declareVariable
                + "/*" + par.Value.AssertNotNull().GetType().ToCSharpFriendlyTypeName() + "*/;\n"
                + "insert into " + par.ParameterName + " values "
                + ValuesClauseForTableValuedParameter((par.Value as IOptionalObjectListForDebugging)?.ContentsForDebuggingOrNull());
        }
    }

    static string ValuesClauseForTableValuedParameter(IReadOnlyList<object>? tableValue)
    {
        if (tableValue == null) {
            return "(/* UNKNOWN? */);\n";
        }
        const int maxValuesToInsert = 20;
        var valuesString = tableValue.Take(maxValuesToInsert).Select(v => $"({InsecureSqlDebugString(v, false)})").JoinStrings(", ");
        var valueCountCommentIfNecessary = $"{(tableValue.Count <= maxValuesToInsert ? "" : $"\n    /* ... and more; {tableValue.Count} total */")};\n";
        return valuesString + valueCountCommentIfNecessary;
    }

    public static string InsecureSqlDebugString(object? p, bool includeReadableEnumValue)
    {
        if (p is DBNull || p == null) {
            return "NULL";
        } else if (p is DateTime dateTime) {
            return $"'{dateTime.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", CultureInfo.InvariantCulture)}'";
        } else if (p is string s) {
            return $"'{s.Replace("'", "''")}'";
        } else if (p is long l) {
            return l.ToStringInvariant();
        } else if (p is int i) {
            return i.ToStringInvariant();
        } else if (p is bool b) {
            return b ? "1" : "0";
        } else if (p is Enum) {
            return ((IConvertible)p).ToInt64(null).ToStringInvariant() + (includeReadableEnumValue ? $"/*{ObjectToCode.PlainObjectToCode(p)}*/" : "");
        } else if (p is IFormattable formattable) {
            return formattable.ToString(null, CultureInfo.InvariantCulture);
        } else {
            try {
                return $"{{!{p}!}}";
            } catch (Exception e) {
                return $"[[Exception in {nameof(SqlCommandTracer)}.{nameof(InsecureSqlDebugString)}: {e.Message}]]";
            }
        }
    }

    public static ParameterizedSqlExecutionException ExceptionWithTextAndArguments(string message, SqlCommand failedCommand, Exception? innerException)
    {
        var debugFriendlyCommandText = DebugFriendlyCommandText(failedCommand, SqlTracerAgumentInclusion.IncludingArgumentValues);
        var timeoutMessage = innerException.IsSqlTimeoutException() ? $"\nCOMMAND TIMEOUT: {failedCommand.CommandTimeout}s" : "";
        return new($"{message}{timeoutMessage}\n\nSqlCommandText:\n{debugFriendlyCommandText}", innerException);
    }
}
