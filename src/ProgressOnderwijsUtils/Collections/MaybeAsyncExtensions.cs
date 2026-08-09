using System.Threading.Tasks;

#pragma warning disable VSTHRD200 // Use "Async" suffix — intentionally omitted for sync-lambda overloads on Task<Maybe>
#pragma warning disable VSTHRD003 // Avoid awaiting foreign tasks — these are extension methods on Task<T> by design

namespace ProgressOnderwijsUtils.Collections;

public static class MaybeAsyncExtensions
{
    /// <summary>
    /// Sync WhenOk with a side-effect-only action on an awaitable Maybe. Returns Maybe&lt;Unit, TError&gt;.
    /// </summary>
    public static async Task<Maybe<Unit, TError>> WhenOk<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask, Action<TOk> action)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.WhenOk(action);
    }

    /// <summary>
    /// Sync WhenOk on an awaitable Maybe.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOkOut, TError>> WhenOk<TOk, TError, TOkOut>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, TOkOut> map)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.WhenOk(map);
    }

    /// <summary>
    /// Async WhenOk with a side-effect-only action. Returns Maybe&lt;Unit, TError&gt;.
    /// </summary>
    public static async Task<Maybe<Unit, TError>> WhenOkAsync<TOk, TError>(this Maybe<TOk, TError> state, Func<TOk, Task> action)
    {
        if (state.TryGet(out var okValue, out var error)) {
            await action(okValue).ConfigureAwait(false);
            return Maybe.Ok(Unit.Value);
        }
        return Maybe.Error(error);
    }

    /// <summary>
    /// Async version of WhenOk: maps a possibly failed value to a new value using an async mapping function.
    /// When the input state is failed, the output state is also failed (with the same message).
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOkOut, TError>> WhenOkAsync<TOk, TError, TOkOut>(this Maybe<TOk, TError> state, Func<TOk, Task<TOkOut>> map)
        => state.TryGet(out var okValue, out var error) ? Maybe.Ok(await map(okValue).ConfigureAwait(false)) : Maybe.Error(error).AsMaybeWithoutValue<TOkOut>();

    /// <summary>
    /// Async WhenOk with a side-effect-only action on an awaitable Maybe. Returns Maybe&lt;Unit, TError&gt;.
    /// </summary>
    public static async Task<Maybe<Unit, TError>> WhenOkAsync<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, Task> action)
    {
        var state = await stateTask.ConfigureAwait(false);
        if (state.TryGet(out var okValue, out var error)) {
            await action(okValue).ConfigureAwait(false);
            return Maybe.Ok(Unit.Value);
        }
        return Maybe.Error(error);
    }

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
    public static async Task<Maybe<TOut, TError>> WhenOkTry<TOk, TError, TOut>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, Maybe<TOut, TError>> map)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.WhenOkTry(map);
    }

    /// <summary>
    /// Sync WhenOkTry for Unit ok values on an awaitable Maybe.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOut, TError>> WhenOkTry<TError, TOut>(this Task<Maybe<Unit, TError>> stateTask, Func<Maybe<TOut, TError>> map)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.WhenOkTry(map);
    }

    /// <summary>
    /// Async version of Extract: extracts a value by calling either the async ifOk or ifError function.
    /// </summary>
    [Pure]
    public static async Task<TOut> ExtractAsync<TOk, TError, TOut>(this Maybe<TOk, TError> state, Func<TOk, Task<TOut>> ifOk, Func<TError, Task<TOut>> ifError)
        => state.TryGet(out var okValue, out var error) ? await ifOk(okValue).ConfigureAwait(false) : await ifError(error).ConfigureAwait(false);

    /// <summary>
    /// Extracts a value by calling either the async ifOk or sync ifError function.
    /// </summary>
    [Pure]
    public static async Task<TOut> ExtractAsync<TOk, TError, TOut>(this Maybe<TOk, TError> state, Func<TOk, Task<TOut>> ifOk, Func<TError, TOut> ifError)
        => state.TryGet(out var okValue, out var error) ? await ifOk(okValue).ConfigureAwait(false) : ifError(error);

    /// <summary>
    /// Extracts a value by calling either the sync ifOk or async ifError function.
    /// </summary>
    [Pure]
    public static async Task<TOut> ExtractAsync<TOk, TError, TOut>(this Maybe<TOk, TError> state, Func<TOk, TOut> ifOk, Func<TError, Task<TOut>> ifError)
        => state.TryGet(out var okValue, out var error) ? ifOk(okValue) : await ifError(error).ConfigureAwait(false);

    /// <summary>
    /// Sync Extract on an awaitable Maybe.
    /// </summary>
    [Pure]
    public static async Task<TOut> Extract<TOk, TError, TOut>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, TOut> ifOk, Func<TError, TOut> ifError)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.Extract(ifOk, ifError);
    }

    /// <summary>
    /// Async version of Extract on an awaitable Maybe with async selector functions.
    /// </summary>
    [Pure]
    public static async Task<TOut> ExtractAsync<TOk, TError, TOut>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, Task<TOut>> ifOk, Func<TError, Task<TOut>> ifError)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.TryGet(out var okValue, out var error) ? await ifOk(okValue).ConfigureAwait(false) : await ifError(error).ConfigureAwait(false);
    }

    /// <summary>
    /// Extracts a value from an awaitable Maybe by calling either the async ifOk or sync ifError function.
    /// </summary>
    [Pure]
    public static async Task<TOut> ExtractAsync<TOk, TError, TOut>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, Task<TOut>> ifOk, Func<TError, TOut> ifError)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.TryGet(out var okValue, out var error) ? await ifOk(okValue).ConfigureAwait(false) : ifError(error);
    }

    /// <summary>
    /// Extracts a value from an awaitable Maybe by calling either the sync ifOk or async ifError function.
    /// </summary>
    [Pure]
    public static async Task<TOut> ExtractAsync<TOk, TError, TOut>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, TOut> ifOk, Func<TError, Task<TOut>> ifError)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.TryGet(out var okValue, out var error) ? ifOk(okValue) : await ifError(error).ConfigureAwait(false);
    }

    /// <summary>
    /// Sync WhenError on an awaitable Maybe.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOk, TErrorResult>> WhenError<TOk, TError, TErrorResult>(this Task<Maybe<TOk, TError>> stateTask, Func<TError, TErrorResult> map)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.WhenError(map);
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

    /// <summary>
    /// Async WhenError with a side-effect-only action. Returns Maybe&lt;TOk, Unit&gt;.
    /// </summary>
    public static async Task<Maybe<TOk, Unit>> WhenErrorAsync<TOk, TError>(this Maybe<TOk, TError> state, Func<TError, Task> action)
    {
        if (state.TryGet(out var okValue, out var error)) {
            return Maybe.Ok(okValue);
        }
        await action(error).ConfigureAwait(false);
        return Maybe.Error();
    }

    /// <summary>
    /// Async WhenError with a side-effect-only action on an awaitable Maybe. Returns Maybe&lt;TOk, Unit&gt;.
    /// </summary>
    public static async Task<Maybe<TOk, Unit>> WhenErrorAsync<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask, Func<TError, Task> action)
    {
        var state = await stateTask.ConfigureAwait(false);
        if (state.TryGet(out var okValue, out var error)) {
            return Maybe.Ok(okValue);
        }
        await action(error).ConfigureAwait(false);
        return Maybe.Error(Unit.Value);
    }

    /// <summary>
    /// Sync WhenError with a side-effect-only action on an awaitable Maybe. Returns Maybe&lt;TOk, Unit&gt;.
    /// </summary>
    public static async Task<Maybe<TOk, Unit>> WhenError<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask, Action<TError> action)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.WhenError(action);
    }

    /// <summary>
    /// Discard the Ok value of an awaitable Maybe, keeping only success/error status.
    /// </summary>
    [Pure]
    public static async Task<Maybe<Unit, TError>> DiscardValue<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.DiscardValue();
    }

    /// <summary>
    /// Asserts that an awaitable Maybe is Ok, throwing if it is in an error state.
    /// </summary>
    public static async Task AssertOk<TError>(this Task<Maybe<Unit, TError>> stateTask)
    {
        var state = await stateTask.ConfigureAwait(false);
        state.AssertOk();
    }

    /// <summary>
    /// Asserts that an awaitable Maybe is Ok, returning the Ok value or throwing if it is in an error state.
    /// </summary>
    public static async Task<TOk> AssertOk<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.AssertOk();
    }

    /// <summary>
    /// Asserts that an awaitable Maybe is Error, returning the error value or throwing if it is in an Ok state.
    /// </summary>
    public static async Task AssertError<TOk>(this Task<Maybe<TOk, Unit>> stateTask)
    {
        var state = await stateTask.ConfigureAwait(false);
        state.AssertError();
    }

    /// <summary>
    /// Asserts that an awaitable Maybe is Error, returning the error value or throwing if it is in an Ok state.
    /// </summary>
    public static async Task<TError> AssertError<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.AssertError();
    }

    /// <summary>
    /// Calls the provided ifOk delegate only when the awaitable Maybe is in the OK state.
    /// </summary>
    public static async Task IfOk<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask, Action<TOk> ifOk)
    {
        var state = await stateTask.ConfigureAwait(false);
        state.IfOk(ifOk);
    }

    /// <summary>
    /// Calls the provided async ifOk delegate only when the awaitable Maybe is in the OK state.
    /// </summary>
    public static async Task IfOkAsync<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, Task> ifOk)
    {
        var state = await stateTask.ConfigureAwait(false);
        if (state.TryGet(out var okValue, out _)) {
            await ifOk(okValue).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Calls the provided ifError delegate only when the awaitable Maybe is in the Error state.
    /// </summary>
    public static async Task IfError<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask, Action<TError> ifError)
    {
        var state = await stateTask.ConfigureAwait(false);
        state.IfError(ifError);
    }

    /// <summary>
    /// Calls the provided async ifError delegate only when the awaitable Maybe is in the Error state.
    /// </summary>
    public static async Task IfErrorAsync<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask, Func<TError, Task> ifError)
    {
        var state = await stateTask.ConfigureAwait(false);
        if (!state.TryGet(out _, out var error)) {
            await ifError(error).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Calls the provided ifOk or ifError delegate depending on the state of the awaitable Maybe.
    /// </summary>
    public static async Task If<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask, Action<TOk> ifOk, Action<TError> ifError)
    {
        var state = await stateTask.ConfigureAwait(false);
        state.If(ifOk, ifError);
    }

    /// <summary>
    /// Calls the provided async ifOk or sync ifError delegate depending on the state of the awaitable Maybe.
    /// </summary>
    public static async Task IfAsync<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, Task> ifOk, Action<TError> ifError)
    {
        var state = await stateTask.ConfigureAwait(false);
        if (state.TryGet(out var okValue, out var error)) {
            await ifOk(okValue).ConfigureAwait(false);
        } else {
            ifError(error);
        }
    }

    /// <summary>
    /// Calls the provided sync ifOk or async ifError delegate depending on the state of the awaitable Maybe.
    /// </summary>
    public static async Task IfAsync<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask, Action<TOk> ifOk, Func<TError, Task> ifError)
    {
        var state = await stateTask.ConfigureAwait(false);
        if (state.TryGet(out var okValue, out var error)) {
            ifOk(okValue);
        } else {
            await ifError(error).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Calls the provided async ifOk or ifError delegate depending on the state of the awaitable Maybe.
    /// </summary>
    public static async Task IfAsync<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask, Func<TOk, Task> ifOk, Func<TError, Task> ifError)
    {
        var state = await stateTask.ConfigureAwait(false);
        if (state.TryGet(out var okValue, out var error)) {
            await ifOk(okValue).ConfigureAwait(false);
        } else {
            await ifError(error).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns the error value if the awaitable Maybe is in an error state, or null if it is ok.
    /// </summary>
    [Pure]
    public static async Task<TError?> ErrorOrNull<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask)
        where TError : class?
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.ErrorOrNull();
    }

    /// <summary>
    /// Returns the error value if the awaitable Maybe is in an error state, or null if it is ok.
    /// </summary>
    [Pure]
    public static async Task<TError?> ErrorOrNull<TOk, TError>(this Task<Maybe<TOk, TError?>> stateTask)
        where TError : struct
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.ErrorOrNull();
    }

    /// <summary>
    /// Returns the error value as a nullable if the awaitable Maybe is in an error state, or null if it is ok.
    /// </summary>
    [Pure]
    public static async Task<TError?> ErrorOrNullable<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask)
        where TError : struct
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.ErrorOrNullable();
    }

    /// <summary>
    /// Returns the ok value if the awaitable Maybe is in an ok state, or null if it is an error.
    /// </summary>
    [Pure]
    public static async Task<TOk?> ValueOrNull<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask)
        where TOk : class?
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.ValueOrNull();
    }

    /// <summary>
    /// Returns the ok value if the awaitable Maybe is in an ok state, or null if it is an error.
    /// </summary>
    [Pure]
    public static async Task<TOk?> ValueOrNull<TOk, TError>(this Task<Maybe<TOk?, TError>> stateTask)
        where TOk : struct
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.ValueOrNull();
    }

    /// <summary>
    /// Returns the ok value as a nullable if the awaitable Maybe is in an ok state, or null if it is an error.
    /// </summary>
    [Pure]
    public static async Task<TOk?> ValueOrNullable<TOk, TError>(this Task<Maybe<TOk, TError>> stateTask)
        where TOk : struct
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.ValueOrNullable();
    }

    /// <summary>
    /// Awaits all tasks and returns Ok with all ok values, or Error with all error values if any failed.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOk[], TError[]>> WhenAllOk<TOk, TError>(this IEnumerable<Task<Maybe<TOk, TError>>> tasks)
    {
        var maybes = await Task.WhenAll(tasks).ConfigureAwait(false);
        return maybes.WhenAllOk();
    }

    /// <summary>
    /// Awaits all tasks and returns Ok if all succeeded, or Error with all error values if any failed.
    /// </summary>
    [Pure]
    public static async Task<Maybe<Unit, TError[]>> WhenAllOk<TError>(this IEnumerable<Task<Maybe<Unit, TError>>> tasks)
    {
        var maybes = await Task.WhenAll(tasks).ConfigureAwait(false);
        return maybes.WhenAllOk();
    }

    /// <summary>
    /// Async version of WhenErrorTry: recovers from a failed Maybe using an async mapping function that itself can fail.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOk, TErrorOut>> WhenErrorTryAsync<TOk, TError, TErrorOut>(this Maybe<TOk, TError> state, Func<TError, Task<Maybe<TOk, TErrorOut>>> map)
        => state.TryGet(out var okValue, out var error) ? Maybe.Ok(okValue) : await map(error).ConfigureAwait(false);

    /// <summary>
    /// Async version of WhenErrorTry for Unit errors: recovers from a failed Maybe using an async mapping function that itself can fail.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOk, TErrorOut>> WhenErrorTryAsync<TOk, TErrorOut>(this Maybe<TOk, Unit> state, Func<Task<Maybe<TOk, TErrorOut>>> map)
        => state.TryGet(out var okValue, out _) ? Maybe.Ok(okValue) : await map().ConfigureAwait(false);

    /// <summary>
    /// Async version of WhenErrorTry on an awaitable Maybe: recovers from a failed Maybe using an async mapping function that itself can fail.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOk, TErrorOut>> WhenErrorTryAsync<TOk, TError, TErrorOut>(this Task<Maybe<TOk, TError>> stateTask, Func<TError, Task<Maybe<TOk, TErrorOut>>> map)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.TryGet(out var okValue, out var error) ? Maybe.Ok(okValue) : await map(error).ConfigureAwait(false);
    }

    /// <summary>
    /// Sync WhenErrorTry on an awaitable Maybe.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOk, TErrorOut>> WhenErrorTry<TOk, TError, TErrorOut>(this Task<Maybe<TOk, TError>> stateTask, Func<TError, Maybe<TOk, TErrorOut>> map)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.WhenErrorTry(map);
    }

    /// <summary>
    /// Async version of WhenErrorTry for Unit errors on an awaitable Maybe.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOk, TErrorOut>> WhenErrorTryAsync<TOk, TErrorOut>(this Task<Maybe<TOk, Unit>> stateTask, Func<Task<Maybe<TOk, TErrorOut>>> map)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.TryGet(out var okValue, out _) ? Maybe.Ok(okValue) : await map().ConfigureAwait(false);
    }

    /// <summary>
    /// Sync WhenErrorTry for Unit errors on an awaitable Maybe.
    /// </summary>
    [Pure]
    public static async Task<Maybe<TOk, TErrorOut>> WhenErrorTry<TOk, TErrorOut>(this Task<Maybe<TOk, Unit>> stateTask, Func<Maybe<TOk, TErrorOut>> map)
    {
        var state = await stateTask.ConfigureAwait(false);
        return state.WhenErrorTry(map);
    }
}
