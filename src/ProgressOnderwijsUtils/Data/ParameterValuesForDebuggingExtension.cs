namespace ProgressOnderwijsUtils;

public static class ParameterValuesForDebuggingExtension
{
    [Pure]
    public static object[] ParameterValuesForDebugging(this ParameterizedSql sql)
    {
        var collector = EquatableParameterValueCollector.Create();
        sql.AppendTo(ref collector);
        return collector.arguments.ToArray();
    }

    struct EquatableParameterValueCollector : ICommandFactory
    {
        public List<object> arguments;

        public string RegisterParameterAndGetName<T>(T o)
            where T : IQueryParameter
        {
            arguments.Add(o.EquatableValue);
            return "";
        }

        public void AppendSql(ReadOnlySpan<char> sql) { }

        public static EquatableParameterValueCollector Create()
            => new() { arguments = new(), };
    }
}
