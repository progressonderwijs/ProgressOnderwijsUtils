namespace ProgressOnderwijsUtils.Tests;

public sealed class UnitTests
{
    [Fact]
    public void TwoInstancesAreEqual()
    {
        // ReSharper disable once EqualExpressionComparison
        PAssert.That(() => Unit.Value.Equals(Unit.Value));
        PAssert.That(() => Unit.Value.Equals(default(Unit)));
        PAssert.That(() => Unit.Value.Equals(new Unit()));

        var action = () => { };
        PAssert.That(() => Unit.Value.Equals(Unit.SideEffect(action)));
    }

    [Fact]
    public void ReturnIsTheIdentityFunction()
        => PAssert.That(() => Unit.Value.Return(3) == 3);
}
