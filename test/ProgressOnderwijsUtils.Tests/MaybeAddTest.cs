namespace ProgressOnderwijsUtils.Tests;

public sealed class MaybeAddTest
{
    sealed record A(int Id);

    sealed record B(int Id);

    sealed record C(int Id);

    sealed record D(int Id);

    [Fact]
    public void Collects_ok_values_in_tuple()
    {
        var a = new A(1);
        var b = new B(2);
        var c = new C(3);
        var d = new D(4);

        var sut = Maybe.Ok(a).AsMaybeWithoutError<string>()
            .ToMaybeAdd()
            .WhenOkTryAdd(Maybe.Either(true, b, "no B"))
            .WhenOkAdd(_ => c)
            .WhenOk(acc => Maybe.Either(ReferenceEquals(acc.Item1, a), Unit.Value, "missing A"))
            .WhenOkTryAdd(Maybe.Ok(d).AsMaybeWithoutError<string>())
            .ToMaybe(); // Maybe<(A, B, C, D), string>

        var ok = sut.AssertOk();
        PAssert.That(() => ReferenceEquals(ok.Item1, a));
        PAssert.That(() => ReferenceEquals(ok.Item2, b));
        PAssert.That(() => ReferenceEquals(ok.Item3, c));
        PAssert.That(() => ReferenceEquals(ok.Item4, d));
    }

}
