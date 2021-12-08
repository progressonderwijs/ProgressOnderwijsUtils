namespace ProgressOnderwijsUtils.Tests;

public sealed class StreamExtensionsTestClass
{
    [Fact]
    public void ChecksEOF()
    {
        using var stream = new MemoryStream(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        _ = Assert.Throws<EndOfStreamException>(() => stream.ReadUntil(257));
        _ = stream.Seek(0, SeekOrigin.Begin);
        PAssert.That(() => stream.ReadUntil(256).SequenceEqual(Enumerable.Range(0, 256).Select(i => (byte)i)));
    }
}
