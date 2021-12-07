namespace ProgressOnderwijsUtils.Tests.Data;

public static class ParameterValuesForDebuggingExtensionTest
{
    [Fact]
    public static void ParameterlessSqlHasNoParameters()
        => PAssert.That(() => SQL($"Hello").ParameterValuesForDebugging().None());

    [Fact]
    public static void IntParametersCanBeRetrieved()
    {
        PAssert.That(() => SQL($"Hello{1}").ParameterValuesForDebugging().SequenceEqual(new object[] { 1, }));
        PAssert.That(() => SQL($"Hello{3}, {2}, {1}").ParameterValuesForDebugging().SequenceEqual(new object[] { 3, 2, 1, }));
    }

    [Fact]
    public static void DateTimeParametersCanBeRetrieved()
        => PAssert.That(() => SQL($"Hello{new DateTime(2000, 1, 1)}").ParameterValuesForDebugging().SequenceEqual(new object[] { new DateTime(2000, 1, 1), }));

    [Fact]
    public static void CurrentTimeTokenIsNotReplaced()
        => PAssert.That(() => SQL($"Hello{CurrentTimeToken.Instance}").ParameterValuesForDebugging().SequenceEqual(new object[] { CurrentTimeToken.Instance, }));
}
