namespace ProgressOnderwijsUtils.Tests;

public sealed class DictionaryExtensionsTests
{
    [Fact]
    public void GetOrLazyDefault()
    {
        var sut = new Dictionary<int, int> { { 0, 0 }, };
        PAssert.That(() => sut.GetOrLazyDefault(0, () => 1) == 0);
        PAssert.That(() => sut.GetOrLazyDefault(1, () => 2) == 2);
        PAssert.That(() => !sut.ContainsKey(1));
    }

    [Fact]
    public void GetOrAdd()
    {
        var sut = new Dictionary<int, int> { { 0, 0 }, };
        PAssert.That(() => sut.GetOrAdd(0, 1) == 0);
        PAssert.That(() => sut.GetOrAdd(1, 2) == 2);
        PAssert.That(() => sut.ContainsKey(1));
    }
}
