namespace ProgressOnderwijsUtils.Tests.Data;

public static class ParameterValuesForDebuggingExtensionTest
{
    [Fact]
    public static void ParameterlessSqlHasNoParameters()
    {
        var sql = SQL($"Hello");
        PAssert.That(() => sql.ParameterValuesForDebugging().None());
    }

    [Fact]
    public static void IntParametersCanBeRetrieved()
    {
        var sqlA = SQL($"Hello{1}");
        PAssert.That(() => sqlA.ParameterValuesForDebugging().SequenceEqual(new object[] { 1, }));
        var sqlB = SQL($"Hello{3}, {2}, {1}");
        PAssert.That(() => sqlB.ParameterValuesForDebugging().SequenceEqual(new object[] { 3, 2, 1, }));
    }

    [Fact]
    public static void DateTimeParametersCanBeRetrieved()
    {
        var sql = SQL($"Hello{new DateTime(2000, 1, 1)}");
        PAssert.That(() => sql.ParameterValuesForDebugging().SequenceEqual(new object[] { new DateTime(2000, 1, 1), }));
    }

    [Fact]
    public static void CurrentTimeTokenIsNotReplaced()
    {
        var sql = SQL($"Hello{CurrentTimeToken.Instance}");
        PAssert.That(() => sql.ParameterValuesForDebugging().SequenceEqual(new object[] { CurrentTimeToken.Instance, }));
    }
}
