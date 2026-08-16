using System.Threading.Tasks;

#pragma warning disable VSTHRD200 // Use "Async" suffix — intentionally omitted for sync-lambda overloads on Task<Maybe>

namespace ProgressOnderwijsUtils.Collections;

public sealed record MaybeAddState<T1, E>
{
    readonly Maybe<T1, E> maybe;

    public MaybeAddState(Maybe<T1, E> maybe)
        => this.maybe = maybe;

    public Maybe<T1, E> ToMaybe()
        => maybe;

    public MaybeAddState<T1, T2, E> WhenOkTryAdd<T2>(Maybe<T2, E> value)
        => new(maybe.WhenOkTry(v1 => value.WhenOk(v2 => (v1, v2))));

    public MaybeAddState<T1, T2, E> WhenOkTryAdd<T2>(Func<T1, Maybe<T2, E>> selector)
        => new(maybe.WhenOkTry(v1 => selector(v1).WhenOk(v2 => (v1, v2))));

    public MaybeAddState<T1, T2, E> WhenOkAdd<T2>(Func<T1, T2> selector)
        => new(maybe.WhenOk(v1 => (v1, selector(v1))));

    public MaybeAddState<T1, E> WhenOk(Func<T1, Maybe<Unit, E>> check)
        => new(maybe.WhenOkTry(v1 => check(v1).WhenOk(_ => v1)));

    public async Task<MaybeAddState<T1, T2, E>> WhenOkTryAddAsync<T2>(Func<T1, Task<Maybe<T2, E>>> selector)
        => new(await maybe.WhenOkTryAsync(async v1 => (await selector(v1).ConfigureAwait(false)).WhenOk(v2 => (v1, v2))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, E>> WhenOkAddAsync<T2>(Func<T1, Task<T2>> selector)
        => new(await maybe.WhenOkAsync(async v1 => (v1, await selector(v1).ConfigureAwait(false))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, E>> WhenOkAsync(Func<T1, Task<Maybe<Unit, E>>> check)
        => new(await maybe.WhenOkTryAsync(async v1 => (await check(v1).ConfigureAwait(false)).WhenOk(_ => v1)).ConfigureAwait(false));

    public MaybeAddState<T1, F> WhenError<F>(Func<E, F> selector)
        => new(maybe.WhenError(selector));

    public async Task<MaybeAddState<T1, F>> WhenErrorAsync<F>(Func<E, Task<F>> selector)
        => new(await maybe.WhenErrorAsync(selector).ConfigureAwait(false));
}

public sealed record MaybeAddState<T1, T2, E>
{
    readonly Maybe<(T1, T2), E> maybe;

    public MaybeAddState(Maybe<(T1, T2), E> maybe)
        => this.maybe = maybe;

    public Maybe<(T1, T2), E> ToMaybe()
        => maybe;

    public MaybeAddState<T1, T2, T3, E> WhenOkTryAdd<T3>(Maybe<T3, E> value)
        => new(maybe.WhenOkTry(a => value.WhenOk(v3 => (a.Item1, a.Item2, v3))));

    public MaybeAddState<T1, T2, T3, E> WhenOkTryAdd<T3>(Func<(T1, T2), Maybe<T3, E>> selector)
        => new(maybe.WhenOkTry(a => selector(a).WhenOk(v3 => (a.Item1, a.Item2, v3))));

    public MaybeAddState<T1, T2, T3, E> WhenOkAdd<T3>(Func<(T1, T2), T3> selector)
        => new(maybe.WhenOk(a => (a.Item1, a.Item2, selector(a))));

    public MaybeAddState<T1, T2, E> WhenOk(Func<(T1, T2), Maybe<Unit, E>> check)
        => new(maybe.WhenOkTry(a => check(a).WhenOk(_ => a)));

    public async Task<MaybeAddState<T1, T2, T3, E>> WhenOkTryAddAsync<T3>(Func<(T1, T2), Task<Maybe<T3, E>>> selector)
        => new(await maybe.WhenOkTryAsync(async a => (await selector(a).ConfigureAwait(false)).WhenOk(v3 => (a.Item1, a.Item2, v3))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, E>> WhenOkAddAsync<T3>(Func<(T1, T2), Task<T3>> selector)
        => new(await maybe.WhenOkAsync(async a => (a.Item1, a.Item2, await selector(a).ConfigureAwait(false))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, E>> WhenOkAsync(Func<(T1, T2), Task<Maybe<Unit, E>>> check)
        => new(await maybe.WhenOkTryAsync(async a => (await check(a).ConfigureAwait(false)).WhenOk(_ => a)).ConfigureAwait(false));

    public MaybeAddState<T1, T2, F> WhenError<F>(Func<E, F> selector)
        => new(maybe.WhenError(selector));

    public async Task<MaybeAddState<T1, T2, F>> WhenErrorAsync<F>(Func<E, Task<F>> selector)
        => new(await maybe.WhenErrorAsync(selector).ConfigureAwait(false));
}

public sealed record MaybeAddState<T1, T2, T3, E>
{
    readonly Maybe<(T1, T2, T3), E> maybe;

    public MaybeAddState(Maybe<(T1, T2, T3), E> maybe)
        => this.maybe = maybe;

    public Maybe<(T1, T2, T3), E> ToMaybe()
        => maybe;

    public MaybeAddState<T1, T2, T3, T4, E> WhenOkTryAdd<T4>(Maybe<T4, E> value)
        => new(maybe.WhenOkTry(a => value.WhenOk(v4 => (a.Item1, a.Item2, a.Item3, v4))));

    public MaybeAddState<T1, T2, T3, T4, E> WhenOkTryAdd<T4>(Func<(T1, T2, T3), Maybe<T4, E>> selector)
        => new(maybe.WhenOkTry(a => selector(a).WhenOk(v4 => (a.Item1, a.Item2, a.Item3, v4))));

    public MaybeAddState<T1, T2, T3, T4, E> WhenOkAdd<T4>(Func<(T1, T2, T3), T4> selector)
        => new(maybe.WhenOk(a => (a.Item1, a.Item2, a.Item3, selector(a))));

    public MaybeAddState<T1, T2, T3, E> WhenOk(Func<(T1, T2, T3), Maybe<Unit, E>> check)
        => new(maybe.WhenOkTry(a => check(a).WhenOk(_ => a)));

    public async Task<MaybeAddState<T1, T2, T3, T4, E>> WhenOkTryAddAsync<T4>(Func<(T1, T2, T3), Task<Maybe<T4, E>>> selector)
        => new(await maybe.WhenOkTryAsync(async a => (await selector(a).ConfigureAwait(false)).WhenOk(v4 => (a.Item1, a.Item2, a.Item3, v4))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, T4, E>> WhenOkAddAsync<T4>(Func<(T1, T2, T3), Task<T4>> selector)
        => new(await maybe.WhenOkAsync(async a => (a.Item1, a.Item2, a.Item3, await selector(a).ConfigureAwait(false))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, E>> WhenOkAsync(Func<(T1, T2, T3), Task<Maybe<Unit, E>>> check)
        => new(await maybe.WhenOkTryAsync(async a => (await check(a).ConfigureAwait(false)).WhenOk(_ => a)).ConfigureAwait(false));

    public MaybeAddState<T1, T2, T3, F> WhenError<F>(Func<E, F> selector)
        => new(maybe.WhenError(selector));

    public async Task<MaybeAddState<T1, T2, T3, F>> WhenErrorAsync<F>(Func<E, Task<F>> selector)
        => new(await maybe.WhenErrorAsync(selector).ConfigureAwait(false));
}

public sealed record MaybeAddState<T1, T2, T3, T4, E>
{
    readonly Maybe<(T1, T2, T3, T4), E> maybe;

    public MaybeAddState(Maybe<(T1, T2, T3, T4), E> maybe)
        => this.maybe = maybe;

    public Maybe<(T1, T2, T3, T4), E> ToMaybe()
        => maybe;

    public MaybeAddState<T1, T2, T3, T4, T5, E> WhenOkTryAdd<T5>(Maybe<T5, E> value)
        => new(maybe.WhenOkTry(a => value.WhenOk(v5 => (a.Item1, a.Item2, a.Item3, a.Item4, v5))));

    public MaybeAddState<T1, T2, T3, T4, T5, E> WhenOkTryAdd<T5>(Func<(T1, T2, T3, T4), Maybe<T5, E>> selector)
        => new(maybe.WhenOkTry(a => selector(a).WhenOk(v5 => (a.Item1, a.Item2, a.Item3, a.Item4, v5))));

    public MaybeAddState<T1, T2, T3, T4, T5, E> WhenOkAdd<T5>(Func<(T1, T2, T3, T4), T5> selector)
        => new(maybe.WhenOk(a => (a.Item1, a.Item2, a.Item3, a.Item4, selector(a))));

    public MaybeAddState<T1, T2, T3, T4, E> WhenOk(Func<(T1, T2, T3, T4), Maybe<Unit, E>> check)
        => new(maybe.WhenOkTry(a => check(a).WhenOk(_ => a)));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, E>> WhenOkTryAddAsync<T5>(Func<(T1, T2, T3, T4), Task<Maybe<T5, E>>> selector)
        => new(await maybe.WhenOkTryAsync(async a => (await selector(a).ConfigureAwait(false)).WhenOk(v5 => (a.Item1, a.Item2, a.Item3, a.Item4, v5))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, E>> WhenOkAddAsync<T5>(Func<(T1, T2, T3, T4), Task<T5>> selector)
        => new(await maybe.WhenOkAsync(async a => (a.Item1, a.Item2, a.Item3, a.Item4, await selector(a).ConfigureAwait(false))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, T4, E>> WhenOkAsync(Func<(T1, T2, T3, T4), Task<Maybe<Unit, E>>> check)
        => new(await maybe.WhenOkTryAsync(async a => (await check(a).ConfigureAwait(false)).WhenOk(_ => a)).ConfigureAwait(false));

    public MaybeAddState<T1, T2, T3, T4, F> WhenError<F>(Func<E, F> selector)
        => new(maybe.WhenError(selector));

    public async Task<MaybeAddState<T1, T2, T3, T4, F>> WhenErrorAsync<F>(Func<E, Task<F>> selector)
        => new(await maybe.WhenErrorAsync(selector).ConfigureAwait(false));
}

public sealed record MaybeAddState<T1, T2, T3, T4, T5, E>
{
    readonly Maybe<(T1, T2, T3, T4, T5), E> maybe;

    public MaybeAddState(Maybe<(T1, T2, T3, T4, T5), E> maybe)
        => this.maybe = maybe;

    public Maybe<(T1, T2, T3, T4, T5), E> ToMaybe()
        => maybe;

    public MaybeAddState<T1, T2, T3, T4, T5, T6, E> WhenOkTryAdd<T6>(Maybe<T6, E> value)
        => new(maybe.WhenOkTry(a => value.WhenOk(v6 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, v6))));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, E> WhenOkTryAdd<T6>(Func<(T1, T2, T3, T4, T5), Maybe<T6, E>> selector)
        => new(maybe.WhenOkTry(a => selector(a).WhenOk(v6 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, v6))));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, E> WhenOkAdd<T6>(Func<(T1, T2, T3, T4, T5), T6> selector)
        => new(maybe.WhenOk(a => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, selector(a))));

    public MaybeAddState<T1, T2, T3, T4, T5, E> WhenOk(Func<(T1, T2, T3, T4, T5), Maybe<Unit, E>> check)
        => new(maybe.WhenOkTry(a => check(a).WhenOk(_ => a)));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> WhenOkTryAddAsync<T6>(Func<(T1, T2, T3, T4, T5), Task<Maybe<T6, E>>> selector)
        => new(await maybe.WhenOkTryAsync(async a => (await selector(a).ConfigureAwait(false)).WhenOk(v6 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, v6))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> WhenOkAddAsync<T6>(Func<(T1, T2, T3, T4, T5), Task<T6>> selector)
        => new(await maybe.WhenOkAsync(async a => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, await selector(a).ConfigureAwait(false))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, E>> WhenOkAsync(Func<(T1, T2, T3, T4, T5), Task<Maybe<Unit, E>>> check)
        => new(await maybe.WhenOkTryAsync(async a => (await check(a).ConfigureAwait(false)).WhenOk(_ => a)).ConfigureAwait(false));

    public MaybeAddState<T1, T2, T3, T4, T5, F> WhenError<F>(Func<E, F> selector)
        => new(maybe.WhenError(selector));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, F>> WhenErrorAsync<F>(Func<E, Task<F>> selector)
        => new(await maybe.WhenErrorAsync(selector).ConfigureAwait(false));
}

public sealed record MaybeAddState<T1, T2, T3, T4, T5, T6, E>
{
    readonly Maybe<(T1, T2, T3, T4, T5, T6), E> maybe;

    public MaybeAddState(Maybe<(T1, T2, T3, T4, T5, T6), E> maybe)
        => this.maybe = maybe;

    public Maybe<(T1, T2, T3, T4, T5, T6), E> ToMaybe()
        => maybe;

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E> WhenOkTryAdd<T7>(Maybe<T7, E> value)
        => new(maybe.WhenOkTry(a => value.WhenOk(v7 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, v7))));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E> WhenOkTryAdd<T7>(Func<(T1, T2, T3, T4, T5, T6), Maybe<T7, E>> selector)
        => new(maybe.WhenOkTry(a => selector(a).WhenOk(v7 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, v7))));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E> WhenOkAdd<T7>(Func<(T1, T2, T3, T4, T5, T6), T7> selector)
        => new(maybe.WhenOk(a => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, selector(a))));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, E> WhenOk(Func<(T1, T2, T3, T4, T5, T6), Maybe<Unit, E>> check)
        => new(maybe.WhenOkTry(a => check(a).WhenOk(_ => a)));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> WhenOkTryAddAsync<T7>(Func<(T1, T2, T3, T4, T5, T6), Task<Maybe<T7, E>>> selector)
        => new(await maybe.WhenOkTryAsync(async a => (await selector(a).ConfigureAwait(false)).WhenOk(v7 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, v7))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> WhenOkAddAsync<T7>(Func<(T1, T2, T3, T4, T5, T6), Task<T7>> selector)
        => new(await maybe.WhenOkAsync(async a => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, await selector(a).ConfigureAwait(false))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> WhenOkAsync(Func<(T1, T2, T3, T4, T5, T6), Task<Maybe<Unit, E>>> check)
        => new(await maybe.WhenOkTryAsync(async a => (await check(a).ConfigureAwait(false)).WhenOk(_ => a)).ConfigureAwait(false));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, F> WhenError<F>(Func<E, F> selector)
        => new(maybe.WhenError(selector));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, F>> WhenErrorAsync<F>(Func<E, Task<F>> selector)
        => new(await maybe.WhenErrorAsync(selector).ConfigureAwait(false));
}

public sealed record MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>
{
    readonly Maybe<(T1, T2, T3, T4, T5, T6, T7), E> maybe;

    public MaybeAddState(Maybe<(T1, T2, T3, T4, T5, T6, T7), E> maybe)
        => this.maybe = maybe;

    public Maybe<(T1, T2, T3, T4, T5, T6, T7), E> ToMaybe()
        => maybe;

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E> WhenOkTryAdd<T8>(Maybe<T8, E> value)
        => new(maybe.WhenOkTry(a => value.WhenOk(v8 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, v8))));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E> WhenOkTryAdd<T8>(Func<(T1, T2, T3, T4, T5, T6, T7), Maybe<T8, E>> selector)
        => new(maybe.WhenOkTry(a => selector(a).WhenOk(v8 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, v8))));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E> WhenOkAdd<T8>(Func<(T1, T2, T3, T4, T5, T6, T7), T8> selector)
        => new(maybe.WhenOk(a => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, selector(a))));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E> WhenOk(Func<(T1, T2, T3, T4, T5, T6, T7), Maybe<Unit, E>> check)
        => new(maybe.WhenOkTry(a => check(a).WhenOk(_ => a)));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> WhenOkTryAddAsync<T8>(Func<(T1, T2, T3, T4, T5, T6, T7), Task<Maybe<T8, E>>> selector)
        => new(await maybe.WhenOkTryAsync(async a => (await selector(a).ConfigureAwait(false)).WhenOk(v8 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, v8))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> WhenOkAddAsync<T8>(Func<(T1, T2, T3, T4, T5, T6, T7), Task<T8>> selector)
        => new(await maybe.WhenOkAsync(async a => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, await selector(a).ConfigureAwait(false))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> WhenOkAsync(Func<(T1, T2, T3, T4, T5, T6, T7), Task<Maybe<Unit, E>>> check)
        => new(await maybe.WhenOkTryAsync(async a => (await check(a).ConfigureAwait(false)).WhenOk(_ => a)).ConfigureAwait(false));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, F> WhenError<F>(Func<E, F> selector)
        => new(maybe.WhenError(selector));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, F>> WhenErrorAsync<F>(Func<E, Task<F>> selector)
        => new(await maybe.WhenErrorAsync(selector).ConfigureAwait(false));
}

public sealed record MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>
{
    readonly Maybe<(T1, T2, T3, T4, T5, T6, T7, T8), E> maybe;

    public MaybeAddState(Maybe<(T1, T2, T3, T4, T5, T6, T7, T8), E> maybe)
        => this.maybe = maybe;

    public Maybe<(T1, T2, T3, T4, T5, T6, T7, T8), E> ToMaybe()
        => maybe;

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E> WhenOkTryAdd<T9>(Maybe<T9, E> value)
        => new(maybe.WhenOkTry(a => value.WhenOk(v9 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, a.Item8, v9))));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E> WhenOkTryAdd<T9>(Func<(T1, T2, T3, T4, T5, T6, T7, T8), Maybe<T9, E>> selector)
        => new(maybe.WhenOkTry(a => selector(a).WhenOk(v9 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, a.Item8, v9))));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E> WhenOkAdd<T9>(Func<(T1, T2, T3, T4, T5, T6, T7, T8), T9> selector)
        => new(maybe.WhenOk(a => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, a.Item8, selector(a))));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E> WhenOk(Func<(T1, T2, T3, T4, T5, T6, T7, T8), Maybe<Unit, E>> check)
        => new(maybe.WhenOkTry(a => check(a).WhenOk(_ => a)));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> WhenOkTryAddAsync<T9>(Func<(T1, T2, T3, T4, T5, T6, T7, T8), Task<Maybe<T9, E>>> selector)
        => new(await maybe.WhenOkTryAsync(async a => (await selector(a).ConfigureAwait(false)).WhenOk(v9 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, a.Item8, v9))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> WhenOkAddAsync<T9>(Func<(T1, T2, T3, T4, T5, T6, T7, T8), Task<T9>> selector)
        => new(await maybe.WhenOkAsync(async a => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, a.Item8, await selector(a).ConfigureAwait(false))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> WhenOkAsync(Func<(T1, T2, T3, T4, T5, T6, T7, T8), Task<Maybe<Unit, E>>> check)
        => new(await maybe.WhenOkTryAsync(async a => (await check(a).ConfigureAwait(false)).WhenOk(_ => a)).ConfigureAwait(false));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, F> WhenError<F>(Func<E, F> selector)
        => new(maybe.WhenError(selector));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, F>> WhenErrorAsync<F>(Func<E, Task<F>> selector)
        => new(await maybe.WhenErrorAsync(selector).ConfigureAwait(false));
}

public sealed record MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>
{
    readonly Maybe<(T1, T2, T3, T4, T5, T6, T7, T8, T9), E> maybe;

    public MaybeAddState(Maybe<(T1, T2, T3, T4, T5, T6, T7, T8, T9), E> maybe)
        => this.maybe = maybe;

    public Maybe<(T1, T2, T3, T4, T5, T6, T7, T8, T9), E> ToMaybe()
        => maybe;

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E> WhenOkTryAdd<T10>(Maybe<T10, E> value)
        => new(maybe.WhenOkTry(a => value.WhenOk(v10 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, a.Item8, a.Item9, v10))));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E> WhenOkTryAdd<T10>(Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Maybe<T10, E>> selector)
        => new(maybe.WhenOkTry(a => selector(a).WhenOk(v10 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, a.Item8, a.Item9, v10))));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E> WhenOkAdd<T10>(Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9), T10> selector)
        => new(maybe.WhenOk(a => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, a.Item8, a.Item9, selector(a))));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E> WhenOk(Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Maybe<Unit, E>> check)
        => new(maybe.WhenOkTry(a => check(a).WhenOk(_ => a)));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> WhenOkTryAddAsync<T10>(Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Task<Maybe<T10, E>>> selector)
        => new(await maybe.WhenOkTryAsync(async a => (await selector(a).ConfigureAwait(false)).WhenOk(v10 => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, a.Item8, a.Item9, v10))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> WhenOkAddAsync<T10>(Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Task<T10>> selector)
        => new(await maybe.WhenOkAsync(async a => (a.Item1, a.Item2, a.Item3, a.Item4, a.Item5, a.Item6, a.Item7, a.Item8, a.Item9, await selector(a).ConfigureAwait(false))).ConfigureAwait(false));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> WhenOkAsync(Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Task<Maybe<Unit, E>>> check)
        => new(await maybe.WhenOkTryAsync(async a => (await check(a).ConfigureAwait(false)).WhenOk(_ => a)).ConfigureAwait(false));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, F> WhenError<F>(Func<E, F> selector)
        => new(maybe.WhenError(selector));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, F>> WhenErrorAsync<F>(Func<E, Task<F>> selector)
        => new(await maybe.WhenErrorAsync(selector).ConfigureAwait(false));
}

public sealed record MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>
{
    readonly Maybe<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10), E> maybe;

    public MaybeAddState(Maybe<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10), E> maybe)
        => this.maybe = maybe;

    public Maybe<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10), E> ToMaybe()
        => maybe;

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E> WhenOk(Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10), Maybe<Unit, E>> check)
        => new(maybe.WhenOkTry(a => check(a).WhenOk(_ => a)));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> WhenOkAsync(Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10), Task<Maybe<Unit, E>>> check)
        => new(await maybe.WhenOkTryAsync(async a => (await check(a).ConfigureAwait(false)).WhenOk(_ => a)).ConfigureAwait(false));

    public MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, F> WhenError<F>(Func<E, F> selector)
        => new(maybe.WhenError(selector));

    public async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, F>> WhenErrorAsync<F>(Func<E, Task<F>> selector)
        => new(await maybe.WhenErrorAsync(selector).ConfigureAwait(false));
}

#pragma warning disable VSTHRD003 // Awaiting task passed in as parameter is intentional
public static class AsyncMaybeAddExtensions
{
    public static MaybeAddState<T1, E> ToMaybeAdd<T1, E>(this Maybe<T1, E> value)
        => new(value);

    // ── Task<MaybeAddState<T1, E>> ───────────────────────────────────────────────
    public static async Task<MaybeAddState<T1, T2, E>> WhenOkTryAdd<T1, T2, E>(this Task<MaybeAddState<T1, E>> step, Maybe<T2, E> value)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(value);

    public static async Task<MaybeAddState<T1, T2, E>> WhenOkTryAdd<T1, T2, E>(this Task<MaybeAddState<T1, E>> step, Func<T1, Maybe<T2, E>> selector)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(selector);

    public static async Task<MaybeAddState<T1, T2, E>> WhenOkAdd<T1, T2, E>(this Task<MaybeAddState<T1, E>> step, Func<T1, T2> selector)
        => (await step.ConfigureAwait(false)).WhenOkAdd(selector);

    public static async Task<MaybeAddState<T1, T2, E>> WhenOkAddAsync<T1, T2, E>(this Task<MaybeAddState<T1, E>> step, Func<T1, Task<T2>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, E>> WhenOk<T1, E>(this Task<MaybeAddState<T1, E>> step, Func<T1, Maybe<Unit, E>> check)
        => (await step.ConfigureAwait(false)).WhenOk(check);

    public static async Task<MaybeAddState<T1, T2, E>> WhenOkTryAddAsync<T1, T2, E>(this Task<MaybeAddState<T1, E>> step, Func<T1, Task<Maybe<T2, E>>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkTryAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, E>> WhenOkAsync<T1, E>(this Task<MaybeAddState<T1, E>> step, Func<T1, Task<Maybe<Unit, E>>> check)
        => await (await step.ConfigureAwait(false)).WhenOkAsync(check).ConfigureAwait(false);

    public static async Task<Maybe<T1, E>> ToMaybe<T1, E>(this Task<MaybeAddState<T1, E>> step)
        => (await step.ConfigureAwait(false)).ToMaybe();

    // ── Task<MaybeAddState<T1, T2, E>> ──────────────────────────────────────────
    public static async Task<MaybeAddState<T1, T2, T3, E>> WhenOkTryAdd<T1, T2, T3, E>(this Task<MaybeAddState<T1, T2, E>> step, Maybe<T3, E> value)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(value);

    public static async Task<MaybeAddState<T1, T2, T3, E>> WhenOkTryAdd<T1, T2, T3, E>(this Task<MaybeAddState<T1, T2, E>> step, Func<(T1, T2), Maybe<T3, E>> selector)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, E>> WhenOkAdd<T1, T2, T3, E>(this Task<MaybeAddState<T1, T2, E>> step, Func<(T1, T2), T3> selector)
        => (await step.ConfigureAwait(false)).WhenOkAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, E>> WhenOkAddAsync<T1, T2, T3, E>(this Task<MaybeAddState<T1, T2, E>> step, Func<(T1, T2), Task<T3>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, E>> WhenOk<T1, T2, E>(this Task<MaybeAddState<T1, T2, E>> step, Func<(T1, T2), Maybe<Unit, E>> check)
        => (await step.ConfigureAwait(false)).WhenOk(check);

    public static async Task<MaybeAddState<T1, T2, T3, E>> WhenOkTryAddAsync<T1, T2, T3, E>(this Task<MaybeAddState<T1, T2, E>> step, Func<(T1, T2), Task<Maybe<T3, E>>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkTryAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, E>> WhenOkAsync<T1, T2, E>(this Task<MaybeAddState<T1, T2, E>> step, Func<(T1, T2), Task<Maybe<Unit, E>>> check)
        => await (await step.ConfigureAwait(false)).WhenOkAsync(check).ConfigureAwait(false);

    public static async Task<Maybe<(T1, T2), E>> ToMaybe<T1, T2, E>(this Task<MaybeAddState<T1, T2, E>> step)
        => (await step.ConfigureAwait(false)).ToMaybe();

    // ── Task<MaybeAddState<T1, T2, T3, E>> ──────────────────────────────────────
    public static async Task<MaybeAddState<T1, T2, T3, T4, E>> WhenOkTryAdd<T1, T2, T3, T4, E>(this Task<MaybeAddState<T1, T2, T3, E>> step, Maybe<T4, E> value)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(value);

    public static async Task<MaybeAddState<T1, T2, T3, T4, E>> WhenOkTryAdd<T1, T2, T3, T4, E>(this Task<MaybeAddState<T1, T2, T3, E>> step, Func<(T1, T2, T3), Maybe<T4, E>> selector)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, E>> WhenOkAdd<T1, T2, T3, T4, E>(this Task<MaybeAddState<T1, T2, T3, E>> step, Func<(T1, T2, T3), T4> selector)
        => (await step.ConfigureAwait(false)).WhenOkAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, E>> WhenOkAddAsync<T1, T2, T3, T4, E>(this Task<MaybeAddState<T1, T2, T3, E>> step, Func<(T1, T2, T3), Task<T4>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, E>> WhenOk<T1, T2, T3, E>(this Task<MaybeAddState<T1, T2, T3, E>> step, Func<(T1, T2, T3), Maybe<Unit, E>> check)
        => (await step.ConfigureAwait(false)).WhenOk(check);

    public static async Task<MaybeAddState<T1, T2, T3, T4, E>> WhenOkTryAddAsync<T1, T2, T3, T4, E>(this Task<MaybeAddState<T1, T2, T3, E>> step, Func<(T1, T2, T3), Task<Maybe<T4, E>>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkTryAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, E>> WhenOkAsync<T1, T2, T3, E>(this Task<MaybeAddState<T1, T2, T3, E>> step, Func<(T1, T2, T3), Task<Maybe<Unit, E>>> check)
        => await (await step.ConfigureAwait(false)).WhenOkAsync(check).ConfigureAwait(false);

    public static async Task<Maybe<(T1, T2, T3), E>> ToMaybe<T1, T2, T3, E>(this Task<MaybeAddState<T1, T2, T3, E>> step)
        => (await step.ConfigureAwait(false)).ToMaybe();

    // ── Task<MaybeAddState<T1, T2, T3, T4, E>> ──────────────────────────────────
    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, E>> WhenOkTryAdd<T1, T2, T3, T4, T5, E>(this Task<MaybeAddState<T1, T2, T3, T4, E>> step, Maybe<T5, E> value)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(value);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, E>> WhenOkTryAdd<T1, T2, T3, T4, T5, E>(this Task<MaybeAddState<T1, T2, T3, T4, E>> step, Func<(T1, T2, T3, T4), Maybe<T5, E>> selector)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, E>> WhenOkAdd<T1, T2, T3, T4, T5, E>(this Task<MaybeAddState<T1, T2, T3, T4, E>> step, Func<(T1, T2, T3, T4), T5> selector)
        => (await step.ConfigureAwait(false)).WhenOkAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, E>> WhenOkAddAsync<T1, T2, T3, T4, T5, E>(this Task<MaybeAddState<T1, T2, T3, T4, E>> step, Func<(T1, T2, T3, T4), Task<T5>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, E>> WhenOk<T1, T2, T3, T4, E>(this Task<MaybeAddState<T1, T2, T3, T4, E>> step, Func<(T1, T2, T3, T4), Maybe<Unit, E>> check)
        => (await step.ConfigureAwait(false)).WhenOk(check);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, E>> WhenOkTryAddAsync<T1, T2, T3, T4, T5, E>(this Task<MaybeAddState<T1, T2, T3, T4, E>> step, Func<(T1, T2, T3, T4), Task<Maybe<T5, E>>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkTryAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, E>> WhenOkAsync<T1, T2, T3, T4, E>(this Task<MaybeAddState<T1, T2, T3, T4, E>> step, Func<(T1, T2, T3, T4), Task<Maybe<Unit, E>>> check)
        => await (await step.ConfigureAwait(false)).WhenOkAsync(check).ConfigureAwait(false);

    public static async Task<Maybe<(T1, T2, T3, T4), E>> ToMaybe<T1, T2, T3, T4, E>(this Task<MaybeAddState<T1, T2, T3, T4, E>> step)
        => (await step.ConfigureAwait(false)).ToMaybe();

    // ── Task<MaybeAddState<T1, T2, T3, T4, T5, E>> ───────────────────────────────────
    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> WhenOkTryAdd<T1, T2, T3, T4, T5, T6, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, E>> step, Maybe<T6, E> value)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(value);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> WhenOkTryAdd<T1, T2, T3, T4, T5, T6, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, E>> step, Func<(T1, T2, T3, T4, T5), Maybe<T6, E>> selector)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> WhenOkAdd<T1, T2, T3, T4, T5, T6, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, E>> step, Func<(T1, T2, T3, T4, T5), T6> selector)
        => (await step.ConfigureAwait(false)).WhenOkAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> WhenOkAddAsync<T1, T2, T3, T4, T5, T6, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, E>> step, Func<(T1, T2, T3, T4, T5), Task<T6>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, E>> WhenOk<T1, T2, T3, T4, T5, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, E>> step, Func<(T1, T2, T3, T4, T5), Maybe<Unit, E>> check)
        => (await step.ConfigureAwait(false)).WhenOk(check);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> WhenOkTryAddAsync<T1, T2, T3, T4, T5, T6, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, E>> step, Func<(T1, T2, T3, T4, T5), Task<Maybe<T6, E>>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkTryAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, E>> WhenOkAsync<T1, T2, T3, T4, T5, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, E>> step, Func<(T1, T2, T3, T4, T5), Task<Maybe<Unit, E>>> check)
        => await (await step.ConfigureAwait(false)).WhenOkAsync(check).ConfigureAwait(false);

    public static async Task<Maybe<(T1, T2, T3, T4, T5), E>> ToMaybe<T1, T2, T3, T4, T5, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, E>> step)
        => (await step.ConfigureAwait(false)).ToMaybe();

    // ── Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> ──
    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> WhenOkTryAdd<T1, T2, T3, T4, T5, T6, T7, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> step, Maybe<T7, E> value)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(value);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> WhenOkTryAdd<T1, T2, T3, T4, T5, T6, T7, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> step, Func<(T1, T2, T3, T4, T5, T6), Maybe<T7, E>> selector)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> WhenOkAdd<T1, T2, T3, T4, T5, T6, T7, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> step, Func<(T1, T2, T3, T4, T5, T6), T7> selector)
        => (await step.ConfigureAwait(false)).WhenOkAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> WhenOkAddAsync<T1, T2, T3, T4, T5, T6, T7, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> step, Func<(T1, T2, T3, T4, T5, T6), Task<T7>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> WhenOk<T1, T2, T3, T4, T5, T6, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> step, Func<(T1, T2, T3, T4, T5, T6), Maybe<Unit, E>> check)
        => (await step.ConfigureAwait(false)).WhenOk(check);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> WhenOkTryAddAsync<T1, T2, T3, T4, T5, T6, T7, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> step, Func<(T1, T2, T3, T4, T5, T6), Task<Maybe<T7, E>>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkTryAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> WhenOkAsync<T1, T2, T3, T4, T5, T6, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> step, Func<(T1, T2, T3, T4, T5, T6), Task<Maybe<Unit, E>>> check)
        => await (await step.ConfigureAwait(false)).WhenOkAsync(check).ConfigureAwait(false);

    public static async Task<Maybe<(T1, T2, T3, T4, T5, T6), E>> ToMaybe<T1, T2, T3, T4, T5, T6, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> step)
        => (await step.ConfigureAwait(false)).ToMaybe();

    // ── Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> ──
    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> WhenOkTryAdd<T1, T2, T3, T4, T5, T6, T7, T8, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> step, Maybe<T8, E> value)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(value);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> WhenOkTryAdd<T1, T2, T3, T4, T5, T6, T7, T8, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7), Maybe<T8, E>> selector)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> WhenOkAdd<T1, T2, T3, T4, T5, T6, T7, T8, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7), T8> selector)
        => (await step.ConfigureAwait(false)).WhenOkAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> WhenOkAddAsync<T1, T2, T3, T4, T5, T6, T7, T8, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7), Task<T8>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> WhenOk<T1, T2, T3, T4, T5, T6, T7, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7), Maybe<Unit, E>> check)
        => (await step.ConfigureAwait(false)).WhenOk(check);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> WhenOkTryAddAsync<T1, T2, T3, T4, T5, T6, T7, T8, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7), Task<Maybe<T8, E>>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkTryAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> WhenOkAsync<T1, T2, T3, T4, T5, T6, T7, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7), Task<Maybe<Unit, E>>> check)
        => await (await step.ConfigureAwait(false)).WhenOkAsync(check).ConfigureAwait(false);

    public static async Task<Maybe<(T1, T2, T3, T4, T5, T6, T7), E>> ToMaybe<T1, T2, T3, T4, T5, T6, T7, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> step)
        => (await step.ConfigureAwait(false)).ToMaybe();

    // ── Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> ──
    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> WhenOkTryAdd<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> step, Maybe<T9, E> value)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(value);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> WhenOkTryAdd<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7, T8), Maybe<T9, E>> selector)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> WhenOkAdd<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7, T8), T9> selector)
        => (await step.ConfigureAwait(false)).WhenOkAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> WhenOkAddAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7, T8), Task<T9>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> WhenOk<T1, T2, T3, T4, T5, T6, T7, T8, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7, T8), Maybe<Unit, E>> check)
        => (await step.ConfigureAwait(false)).WhenOk(check);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> WhenOkTryAddAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7, T8), Task<Maybe<T9, E>>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkTryAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> WhenOkAsync<T1, T2, T3, T4, T5, T6, T7, T8, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7, T8), Task<Maybe<Unit, E>>> check)
        => await (await step.ConfigureAwait(false)).WhenOkAsync(check).ConfigureAwait(false);

    public static async Task<Maybe<(T1, T2, T3, T4, T5, T6, T7, T8), E>> ToMaybe<T1, T2, T3, T4, T5, T6, T7, T8, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> step)
        => (await step.ConfigureAwait(false)).ToMaybe();

    // ── Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> ──
    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> WhenOkTryAdd<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> step, Maybe<T10, E> value)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(value);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> WhenOkTryAdd<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Maybe<T10, E>> selector)
        => (await step.ConfigureAwait(false)).WhenOkTryAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> WhenOkAdd<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9), T10> selector)
        => (await step.ConfigureAwait(false)).WhenOkAdd(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> WhenOkAddAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Task<T10>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> WhenOk<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Maybe<Unit, E>> check)
        => (await step.ConfigureAwait(false)).WhenOk(check);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> WhenOkTryAddAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Task<Maybe<T10, E>>> selector)
        => await (await step.ConfigureAwait(false)).WhenOkTryAddAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> WhenOkAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9), Task<Maybe<Unit, E>>> check)
        => await (await step.ConfigureAwait(false)).WhenOkAsync(check).ConfigureAwait(false);

    public static async Task<Maybe<(T1, T2, T3, T4, T5, T6, T7, T8, T9), E>> ToMaybe<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> step)
        => (await step.ConfigureAwait(false)).ToMaybe();

    // ── Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> (terminal) ──
    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> WhenOk<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10), Maybe<Unit, E>> check)
        => (await step.ConfigureAwait(false)).WhenOk(check);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> WhenOkAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> step, Func<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10), Task<Maybe<Unit, E>>> check)
        => await (await step.ConfigureAwait(false)).WhenOkAsync(check).ConfigureAwait(false);

    public static async Task<Maybe<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10), E>> ToMaybe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> step)
        => (await step.ConfigureAwait(false)).ToMaybe();

    // ── WhenError on Task<MaybeAddState<...>> ──
    public static async Task<MaybeAddState<T1, F>> WhenError<T1, E, F>(this Task<MaybeAddState<T1, E>> step, Func<E, F> selector)
        => (await step.ConfigureAwait(false)).WhenError(selector);

    public static async Task<MaybeAddState<T1, F>> WhenErrorAsync<T1, E, F>(this Task<MaybeAddState<T1, E>> step, Func<E, Task<F>> selector)
        => await (await step.ConfigureAwait(false)).WhenErrorAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, F>> WhenError<T1, T2, E, F>(this Task<MaybeAddState<T1, T2, E>> step, Func<E, F> selector)
        => (await step.ConfigureAwait(false)).WhenError(selector);

    public static async Task<MaybeAddState<T1, T2, F>> WhenErrorAsync<T1, T2, E, F>(this Task<MaybeAddState<T1, T2, E>> step, Func<E, Task<F>> selector)
        => await (await step.ConfigureAwait(false)).WhenErrorAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, F>> WhenError<T1, T2, T3, E, F>(this Task<MaybeAddState<T1, T2, T3, E>> step, Func<E, F> selector)
        => (await step.ConfigureAwait(false)).WhenError(selector);

    public static async Task<MaybeAddState<T1, T2, T3, F>> WhenErrorAsync<T1, T2, T3, E, F>(this Task<MaybeAddState<T1, T2, T3, E>> step, Func<E, Task<F>> selector)
        => await (await step.ConfigureAwait(false)).WhenErrorAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, F>> WhenError<T1, T2, T3, T4, E, F>(this Task<MaybeAddState<T1, T2, T3, T4, E>> step, Func<E, F> selector)
        => (await step.ConfigureAwait(false)).WhenError(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, F>> WhenErrorAsync<T1, T2, T3, T4, E, F>(this Task<MaybeAddState<T1, T2, T3, T4, E>> step, Func<E, Task<F>> selector)
        => await (await step.ConfigureAwait(false)).WhenErrorAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, F>> WhenError<T1, T2, T3, T4, T5, E, F>(this Task<MaybeAddState<T1, T2, T3, T4, T5, E>> step, Func<E, F> selector)
        => (await step.ConfigureAwait(false)).WhenError(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, F>> WhenErrorAsync<T1, T2, T3, T4, T5, E, F>(this Task<MaybeAddState<T1, T2, T3, T4, T5, E>> step, Func<E, Task<F>> selector)
        => await (await step.ConfigureAwait(false)).WhenErrorAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, F>> WhenError<T1, T2, T3, T4, T5, T6, E, F>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> step, Func<E, F> selector)
        => (await step.ConfigureAwait(false)).WhenError(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, F>> WhenErrorAsync<T1, T2, T3, T4, T5, T6, E, F>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, E>> step, Func<E, Task<F>> selector)
        => await (await step.ConfigureAwait(false)).WhenErrorAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, F>> WhenError<T1, T2, T3, T4, T5, T6, T7, E, F>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> step, Func<E, F> selector)
        => (await step.ConfigureAwait(false)).WhenError(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, F>> WhenErrorAsync<T1, T2, T3, T4, T5, T6, T7, E, F>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, E>> step, Func<E, Task<F>> selector)
        => await (await step.ConfigureAwait(false)).WhenErrorAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, F>> WhenError<T1, T2, T3, T4, T5, T6, T7, T8, E, F>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> step, Func<E, F> selector)
        => (await step.ConfigureAwait(false)).WhenError(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, F>> WhenErrorAsync<T1, T2, T3, T4, T5, T6, T7, T8, E, F>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, E>> step, Func<E, Task<F>> selector)
        => await (await step.ConfigureAwait(false)).WhenErrorAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, F>> WhenError<T1, T2, T3, T4, T5, T6, T7, T8, T9, E, F>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> step, Func<E, F> selector)
        => (await step.ConfigureAwait(false)).WhenError(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, F>> WhenErrorAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, E, F>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, E>> step, Func<E, Task<F>> selector)
        => await (await step.ConfigureAwait(false)).WhenErrorAsync(selector).ConfigureAwait(false);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, F>> WhenError<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E, F>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> step, Func<E, F> selector)
        => (await step.ConfigureAwait(false)).WhenError(selector);

    public static async Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, F>> WhenErrorAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E, F>(this Task<MaybeAddState<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, E>> step, Func<E, Task<F>> selector)
        => await (await step.ConfigureAwait(false)).WhenErrorAsync(selector).ConfigureAwait(false);
#pragma warning restore VSTHRD003
}
