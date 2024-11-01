namespace ProgressOnderwijsUtils.Collections;

public sealed class Maybe_Ok<TOk>
{
    public readonly TOk Value;

    public Maybe_Ok(TOk value)
        => Value = value;

    public Maybe<TOk, TError> AsMaybeWithoutError<TError>()
        => this;
}

public sealed class Maybe_Error<TError>
{
    public readonly TError Error;

    public Maybe_Error(TError error)
        => Error = error;

    public Maybe<TOk, TError> AsMaybeWithoutValue<TOk>()
        => this;
}

/// <summary>
/// A value or an error message describing why no value is present.
/// 
/// "Maybe" can be used to pass around possibly missing or erroneous values that aren't exceptional (e.g. that should not cause a crash).
/// See the utility functions  "WhenOk", "WhenOkTry" and "ExtractToValue" for ways to handily combine values of type Maybe.
/// 
/// "Maybe" is immutable and hence thread safe (assuming the wrapped value is thread safe).
/// </summary>
public readonly struct Maybe<TOk, TError>
{
    readonly object okOrError;

    Maybe(Maybe_Error<TError> error)
        => okOrError = error;

    Maybe(Maybe_Ok<TOk> ok)
        => okOrError = ok;

    /// <summary>
    /// Value: whether this Maybe is in the OK state.
    /// </summary>
    public bool IsOk()
        => IsOk(out _);

    /// <summary>
    /// Value: whether this Maybe is in the OK state.
    /// </summary>
    public bool IsOk([MaybeNullWhen(false)] out TOk okValueIfOk)
    {
        if (okOrError is Maybe_Ok<TOk> okValue) {
            okValueIfOk = okValue.Value;
            return true;
        }
        okValueIfOk = default(TOk);
        return false;
    }

    /// <summary>
    /// Value: whether this Maybe is in the Error state.
    /// </summary>
    public bool IsError()
        => IsError(out _);

    /// <summary>
    /// Value: whether this Maybe is in the Error state.
    /// </summary>
    public bool IsError([MaybeNullWhen(false)] out TError errorValueIfError)
    {
        if (okOrError is Maybe_Error<TError> errValue) {
            errorValueIfError = errValue.Error;
            return true;
        }
        errorValueIfError = default(TError);
        return false;
    }

    /// <summary>
    /// Extracts a value from the Maybe by calling either the ifOk function or the ifError function, depending on the state of the Maybe.
    /// </summary>
    [MustUseReturnValue]
    public TOut Extract<TOut>(Func<TOk, TOut> ifOk, Func<TError, TOut> ifError)
        => okOrError switch {
            Maybe_Ok<TOk> okValue => ifOk(okValue.Value),
            Maybe_Error<TError> errValue => ifError(errValue.Error),
            _ => throw new("Maybe is neither Ok nor Error."),
        };

    /// <summary>
    /// Converts an untyped error message into a specific type of failed Maybe.  This operator is a  workaround to make it easy to create an error message without redundant type info.
    /// </summary>
    [Pure]
    public static implicit operator Maybe<TOk, TError>(Maybe_Error<TError> err)
        => new(err);

    /// <summary>
    /// Converts an untyped error message into a specific type of failed Maybe.  This operator is a  workaround to make it easy to create an error message without redundant type info.
    /// </summary>
    [Pure]
    public static implicit operator Maybe<TOk, TError>(Maybe_Ok<TOk> err)
        => new(err);

    public bool TryGet([MaybeNullWhen(false)] out TOk okValueIfOk, [MaybeNullWhen(true)] out TError errorValueIfError)
    {
        switch (okOrError) {
            case Maybe_Ok<TOk> okValue:
                okValueIfOk = okValue.Value;
                errorValueIfError = default(TError);
                return true;
            case Maybe_Error<TError> errValue:
                okValueIfOk = default(TOk);
                errorValueIfError = errValue.Error;
                return false;
            default:
                throw new("Maybe is neither Ok nor Error.");
        }
    }

    public void Deconstruct(out bool isOk, out TOk? okValueIfOk, out TError? errorValueIfError)
        => isOk = TryGet(out okValueIfOk, out errorValueIfError);
}

public static class Maybe
{
    /// <summary>
    /// Creates a succesful Maybe that stores the provided value.
    /// </summary>
    [Pure]
    public static Maybe_Ok<T> Ok<T>(T val)
        => new(val);

    /// <summary>
    /// Creates a succesful Maybe value without a value.
    /// </summary>
    [Pure]
    public static Maybe_Ok<Unit> Ok()
        => new(Unit.Value);

    /// <summary>
    /// Create a failed maybe with an error state describing a failed operation.
    /// </summary>
    [Pure]
    public static Maybe_Error<TError> Error<TError>(TError error)
        => new(error);

    /// <summary>
    /// Create a failed maybe without any additional information about the error.
    /// </summary>
    [Pure]
    public static Maybe_Error<Unit> Error()
        => new(Unit.Value);

    public static Maybe<TOk, TError> Either<TOk, TError>(bool isOk, Func<TOk> whenOk, Func<TError> whenError)
        => isOk ? Ok(whenOk()).AsMaybeWithoutError<TError>() : Error(whenError());

    public static Maybe<Unit, TError> Either<TError>(bool isOk, Action whenOk, Func<TError> whenError)
        => isOk ? Ok(whenOk.ToUnitReturningFunc()()).AsMaybeWithoutError<TError>() : Error(whenError());

    public static Maybe<TOk, Unit> Either<TOk>(bool isOk, Func<TOk> whenOk, Action whenError)
        => isOk ? Ok(whenOk()).AsMaybeWithoutError<Unit>() : Error(whenError.ToUnitReturningFunc()());

    public static Maybe<TOk, TError> Either<TOk, TError>(bool isOk, TOk whenOk, TError whenError)
        => isOk ? Ok(whenOk).AsMaybeWithoutError<TError>() : Error(whenError);

    public static Maybe<Unit, TError> Verify<TError>(bool isOk, Func<TError> whenError)
        => isOk ? Ok().AsMaybeWithoutError<TError>() : Error(whenError());

    public static Maybe<Unit, TError> Verify<TError>(bool isOk, TError whenError)
        => isOk ? Ok().AsMaybeWithoutError<TError>() : Error(whenError);

    /// <summary>
    /// Converts a possibly null error to a Maybe&lt;Unit, TError&gt;. When the input is null; return OK, otherwise - returns error.
    /// </summary>
    [Pure]
    public static Maybe<Unit, TError> ErrorWhenNotNull<TError>(TError? val)
        where TError : class
        => val != null ? Error(val) : Ok(Unit.Value).AsMaybeWithoutError<TError>();

    /// <summary>
    /// Converts a possibly null error to a Maybe&lt;Unit, TError&gt;. When the input is null; return OK, otherwise - returns error.
    /// </summary>
    [Pure]
    public static Maybe<Unit, TError> ErrorWhenNotNull<TError>(TError? val)
        where TError : struct
        => val == null ? Ok(Unit.Value).AsMaybeWithoutError<TError>() : Error(val.Value);

    /// <summary>
    /// Converts a possibly null okValue to a Maybe&lt;TOk, Unit&gt;. When the input is null; return errors, otherwise returns ok.
    /// </summary>
    [Pure]
    public static Maybe<TOk, Unit> OkWhenNotNull<TOk>(TOk? val)
        where TOk : class
        => val is { } notNull ? Ok(notNull).AsMaybeWithoutError<Unit>() : Error(Unit.Value);

    /// <summary>
    /// Converts a possibly null okValue to a Maybe&lt;TOk, Unit&gt;. When the input is null; return errors, otherwise returns ok.
    /// </summary>
    [Pure]
    public static Maybe<TOk, Unit> OkWhenNotNull<TOk>(TOk? val)
        where TOk : struct
        => val != null ? Ok(val.Value).AsMaybeWithoutError<Unit>() : Error(Unit.Value);

    /// <summary>
    /// Usage: Maybe.Try( () => Some.Thing.That(Can.Fail())).Catch&lt;SomeException&gt;()
    /// </summary>
    public static MaybeTryBody Try(Action tryBody)
        => new(tryBody);

    /// <summary>
    /// Usage: Maybe.Try( () => Some.Thing.That(Can.Fail())).Catch&lt;SomeException&gt;()
    /// </summary>
    public static MaybeTryBody<TOk> Try<TOk>(Func<TOk> tryBody)
        => new(tryBody);
}

public readonly struct MaybeTryBody
{
    readonly Action tryBody;

    public MaybeTryBody(Action tryBody)
        => this.tryBody = tryBody;

    public Maybe<Unit, TError> Catch<TError>()
        where TError : Exception
    {
        try {
            tryBody();
            return Maybe.Ok();
        } catch (TError e) {
            return Maybe.Error(e);
        }
    }

    /// <summary>
    /// Executions a computation with reliable cleanup (like try...finally or using(...) {}).
    /// When both computation and cleanup throw exceptions, wraps both exceptions in an AggregateException.
    /// Instead of throwing, this method returns exceptions in a Maybe.Error().
    /// </summary>
    public Maybe<Unit, Exception> Finally(Action cleanup)
    {
        try {
            Utils.TryWithCleanup(tryBody, cleanup);
            return Maybe.Ok();
        } catch (Exception e) {
            return Maybe.Error(e);
        }
    }
}

public readonly struct MaybeTryBody<TOk>
{
    readonly Func<TOk> tryBody;

    public MaybeTryBody(Func<TOk> tryBody)
        => this.tryBody = tryBody;

    public Maybe<TOk, TError> Catch<TError>()
        where TError : Exception
    {
        try {
            return Maybe.Ok(tryBody());
        } catch (TError e) {
            return Maybe.Error(e);
        }
    }

    /// <summary>
    /// Executions a computation with reliable cleanup (like try...finally or using(...) {}).
    /// When both computation and cleanup throw exceptions, wraps both exceptions in an AggregateException.
    /// Instead of throwing, this method returns exceptions in a Maybe.Error().
    /// </summary>
    public Maybe<TOk, Exception> Finally(Action cleanup)
    {
        try {
            return Maybe.Ok(Utils.TryWithCleanup(tryBody, cleanup));
        } catch (Exception e) {
            return Maybe.Error(e);
        }
    }
}
