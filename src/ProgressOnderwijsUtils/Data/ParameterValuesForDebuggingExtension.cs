using System.Collections.Generic;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class ParameterValuesForDebuggingExtension
    {
        [NotNull]
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

            [NotNull]
            public string RegisterParameterAndGetName<T>([NotNull] T o)
                where T : IQueryParameter
            {
                arguments.Add(o.EquatableValue);
                return "";
            }

            public void AppendSql(string sql, int startIndex, int length) { }

            public static EquatableParameterValueCollector Create()
                => new EquatableParameterValueCollector { arguments = new List<object>() };
        }
    }
}
