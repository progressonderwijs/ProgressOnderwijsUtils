namespace ProgressOnderwijsUtils.Tests;

public sealed class xUnitUnescapeTest
{
    [Fact]
    public void PlainTextIsUnmolested()
        => PAssert.That(() => "foo bar" == XunitFormat.xUnitUnescapeString("foo bar"));

    [Fact]
    public void LineBreaksAreRestored()
        => PAssert.That(() => "foo\r\nbar\nlala" == XunitFormat.xUnitUnescapeString(@"foo\r\nbar\nlala"));

    [Fact]
    public void MixedBackslashLinebreakMessIsRestored()
        => PAssert.That(() => "foo \\n bar \r\\n la \\\r\n di \r\n da \\\\\twhee" == XunitFormat.xUnitUnescapeString(@"foo \\n bar \r\\n la \\\r\n di \r\n da \\\\\twhee"));
}
