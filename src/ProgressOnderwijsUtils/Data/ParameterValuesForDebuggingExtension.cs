using System;
using System.Collections.Generic;
using static ProgressOnderwijsUtils.SafeSql;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using Xunit;

namespace ProgressOnderwijsUtils
{
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

            public string RegisterParameterAndGetName<T>(T o) where T : IQueryParameter
            {
                arguments.Add(o.EquatableValue);
                return "";
            }

            public void AppendSql(string sql, int startIndex, int length) { }
            public static EquatableParameterValueCollector Create() => new EquatableParameterValueCollector { arguments = new List<object>() };
        }
    }

    public static class ParameterValuesForDebuggingExtensionTest
    {
        [Fact]
        public static void ParameterlessSqlHasNoParameters()
        {
            PAssert.That(() => SQL($"Hello").ParameterValuesForDebugging().None());
        }

        [Fact]
        public static void IntParametersCanBeRetrieved()
        {
            PAssert.That(() => SQL($"Hello{1}").ParameterValuesForDebugging().SequenceEqual(new object[] { 1 }));
            PAssert.That(() => SQL($"Hello{3}, {2}, {1}").ParameterValuesForDebugging().SequenceEqual(new object[] { 3, 2, 1 }));
        }

        [Fact]
        public static void DateTimeParametersCanBeRetrieved()
        {
            PAssert.That(() => SQL($"Hello{new DateTime(2000, 1, 1)}").ParameterValuesForDebugging().SequenceEqual(new object[] { new DateTime(2000, 1, 1) }));
        }

        [Fact]
        public static void CurrentTimeTokenIsNotReplaced()
        {
            PAssert.That(() => SQL($"Hello{CurrentTimeToken.Instance}").ParameterValuesForDebugging().SequenceEqual(new object[] { CurrentTimeToken.Instance }));
        }
    }
}
