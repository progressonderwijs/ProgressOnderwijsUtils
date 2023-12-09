namespace ProgressOnderwijsUtils;

public sealed class ParameterizedSqlExecutionException : Exception
{
    public ParameterizedSqlExecutionException(string msg)
        : base(msg) { }

    public ParameterizedSqlExecutionException() { }

    public ParameterizedSqlExecutionException(string msg, Exception? inner)
        : base(msg, inner) { }
}
