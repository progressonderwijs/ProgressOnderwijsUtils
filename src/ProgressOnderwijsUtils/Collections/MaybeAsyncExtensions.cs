using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Collections;

public static class MaybeAsyncExtensions
{
    /// <summary>
    /// Async version of WhenOk: maps a possibly failed value to a new value using an async mapping function.
    /// When the input state is failed, the output state is also failed (with the same message).
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOkOut, TError>> WhenOkAsync<TOk, TError, TOkOut>(this Maybe<TOk, TError> state, Func<TOk, Task<TOkOut>> map)
        => state.TryGet(out var okValue, out var error) ? Maybe.Ok(await map(okValue).ConfigureAwait(false)) : Maybe.Error(error).AsMaybeWithoutValue<TOkOut>();

    /// <summary>
    /// Async version of WhenOk for Unit ok values.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOkOut, TError>> WhenOkAsync<TOkOut, TError>(this Maybe<Unit, TError> state, Func<Task<TOkOut>> map)
        => state.TryGet(out _, out var error) ? Maybe.Ok(await map().ConfigureAwait(false)) : Maybe.Error(error).AsMaybeWithoutValue<TOkOut>();

    /// <summary>
    /// Async version of WhenOkTry: maps a possibly failed value using an async function that itself can fail.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOut, TError>> WhenOkTryAsync<TOk, TError, TOut>(this Maybe<TOk, TError> state, Func<TOk, Task<Maybe<TOut, TError>>> map)
        => state.TryGet(out var okValue, out var error) ? await map(okValue).ConfigureAwait(false) : Maybe.Error(error);

    /// <summary>
    /// Async version of WhenOkTry for Unit ok values.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOut, TError>> WhenOkTryAsync<TError, TOut>(this Maybe<Unit, TError> state, Func<Task<Maybe<TOut, TError>>> map)
        => state.TryGet(out _, out var error) ? await map().ConfigureAwait(false) : Maybe.Error(error);

    /// <summary>
    /// Async version of WhenOk: maps a possibly failed value to a new value using an async mapping function.
    /// Operates on an awaitable Maybe.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOkOut, TError>> WhenOkAsync<TOk, TError, TOkOut>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, Task<TOkOut>> map)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.TryGet(out var okValue, out var error) ? Maybe.Ok(await map(okValue).ConfigureAwait(false)) : Maybe.Error(error).AsMaybeWithoutValue<TOkOut>();
    }

    /// <summary>
    /// Sync WhenOk on an awaitable Maybe.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOkOut, TError>> WhenOkAsync<TOk, TError, TOkOut>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, TOkOut> map)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.TryGet(out var okValue, out var error) ? Maybe.Ok(map(okValue)) : Maybe.Error(error).AsMaybeWithoutValue<TOkOut>();
    }

    /// <summary>
    /// Async version of WhenOkTry: maps a possibly failed value using an async function that itself can fail.
    /// Operates on an awaitable Maybe.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOut, TError>> WhenOkTryAsync<TOk, TError, TOut>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, Task<Maybe<TOut, TError>>> map)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.TryGet(out var okValue, out var error) ? await map(okValue).ConfigureAwait(false) : Maybe.Error(error);
    }

    /// <summary>
    /// Sync WhenOkTry on an awaitable Maybe.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOut, TError>> WhenOkTryAsync<TOk, TError, TOut>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, Maybe<TOut, TError>> map)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.TryGet(out var okValue, out var error) ? map(okValue) : Maybe.Error(error);
    }

    /// <summary>
    /// Async version of Extract: extracts a value by calling either the async ifOk or ifError function.
    /// </summary>
    [Pure]
    public static async Task<TOut> ExtractAsync<TOk, TError, TOut>(this Maybe<TOk, TError> state, Func<TOk, Task<TOut>> ifOk, Func<TError, Task<TOut>> ifError)
        => state.TryGet(out var okValue, out var error) ? await ifOk(okValue).ConfigureAwait(false) : await ifError(error).ConfigureAwait(false);

    /// <summary>
    /// Async version of Extract on an awaitable Maybe.
    /// </summary>
    [Pure]
    public static async Task<TOut> ExtractAsync<TOk, TError, TOut>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, TOut> ifOk, Func<TError, TOut> ifError)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.Extract(ifOk, ifError);
    }

    /// <summary>
    /// Sync WhenError on an awaitable Maybe.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOk, TErrorResult>> WhenErrorAsync<TOk, TError, TErrorResult>(this Task<Maybe<TOk, TError>> stateTask, Func<TError, TErrorResult> map)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.TryGet(out var okValue, out var error) ? Maybe.Ok(okValue) : Maybe.Error(map(error));
    }

    /// <summary>
    /// Async version of WhenError: maps an error state using an async mapping function.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOk, TErrorResult>> WhenErrorAsync<TOk, TError, TErrorResult>(this Maybe<TOk, TError> state, Func<TError, Task<TErrorResult>> map)
        => state.TryGet(out var okValue, out var error) ? Maybe.Ok(okValue) : Maybe.Error(await map(error).ConfigureAwait(false));

    /// <summary>
    /// Async WhenError on an awaitable Maybe with an async mapping function.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOk, TErrorResult>> WhenErrorAsync<TOk, TError, TErrorResult>(this Task<Maybe<TOk, TError>> stateTask, Func<TError, Task<TErrorResult>> map)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.TryGet(out var okValue, out var error) ? Maybe.Ok(okValue) : Maybe.Error(await map(error).ConfigureAwait(false));
    }
}
