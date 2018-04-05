using System;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public static class ParameterValuesForDebuggingExtensionTest
    {
        [Fact]
        public static void ParameterlessSqlHasNoParameters()
        {
            PAssert.That(() => SafeSql.SQL($"Hello").ParameterValuesForDebugging().None());
        }

        [Fact]
        public static void IntParametersCanBeRetrieved()
        {
            PAssert.That(() => SafeSql.SQL($"Hello{1}").ParameterValuesForDebugging().SequenceEqual(new object[] { 1 }));
            PAssert.That(() => SafeSql.SQL($"Hello{3}, {2}, {1}").ParameterValuesForDebugging().SequenceEqual(new object[] { 3, 2, 1 }));
        }

        [Fact]
        public static void DateTimeParametersCanBeRetrieved()
        {
            PAssert.That(() => SafeSql.SQL($"Hello{new DateTime(2000, 1, 1)}").ParameterValuesForDebugging().SequenceEqual(new object[] { new DateTime(2000, 1, 1) }));
        }

        [Fact]
        public static void CurrentTimeTokenIsNotReplaced()
        {
            PAssert.That(() => SafeSql.SQL($"Hello{CurrentTimeToken.Instance}").ParameterValuesForDebugging().SequenceEqual(new object[] { CurrentTimeToken.Instance }));
        }
    }
}