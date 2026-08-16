namespace ProgressOnderwijsUtils.Tests;

public sealed class MaybeAddTest
{
    sealed record A { }

    sealed record B { }

    sealed record C { }

    sealed record D { }

    [Fact]
    public void FooWithMaybe()
    {
        var sut = Maybe.Ok(new A()).AsMaybeWithoutError<string>()
            .ToMaybeAdd()
            .WhenOkTryAdd(Maybe.Either(true, new B(), "no B"))
            .WhenOkAdd(_ => new C())
            .WhenOk(acc => Maybe.Either(acc.Item1 is not null, Unit.Value, "missing A"))
            .WhenOkTryAdd((Maybe<D, string>)Maybe.Ok(new D()))
            .ToMaybe(); // Maybe<(A, B, C, D), string>

        PAssert.That(() => sut.IsOk());
    }

}
