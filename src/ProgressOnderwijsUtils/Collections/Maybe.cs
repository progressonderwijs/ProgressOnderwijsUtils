using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
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
    public struct Maybe<TOk, TError>
    {
        readonly object okOrError;

        Maybe(Maybe_Error<TError> error)
            => okOrError = error;

        Maybe(Maybe_Ok<TOk> ok)
            => okOrError = ok;

        /// <summary>
        /// Consider "WhenOk",  "WhenOkTry" and "ExtractToValue": those often result in clearer code.
        /// 
        /// Value: whether this Maybe is in the OK state.
        /// </summary>
        public bool IsOk
            => okOrError is Maybe_Ok<TOk>;

        /// <summary>
        /// Extracts a value from the Maybe by calling either the ifOk function or the ifError function, depending on the state of the Maybe.
        /// </summary>
        [MustUseReturnValue]
        public TOut Extract<TOut>(Func<TOk, TOut> ifOk, Func<TError, TOut> ifError)
            => okOrError is Maybe_Ok<TOk> okWrapper ? ifOk(okWrapper.Value) : ifError(okOrError is Maybe_Error<TError> errorWrapper ? errorWrapper.Error : default(TError));

        /// <summary>
        /// Converts an untyped error message into a specific type of failed Maybe.  This operator is a  workaround to make it easy to create an error message without redundant type info.
        /// </summary>
        [Pure]
        public static implicit operator Maybe<TOk, TError>(Maybe_Error<TError> err)
            => new Maybe<TOk, TError>(err);

        /// <summary>
        /// Converts an untyped error message into a specific type of failed Maybe.  This operator is a  workaround to make it easy to create an error message without redundant type info.
        /// </summary>
        [Pure]
        public static implicit operator Maybe<TOk, TError>(Maybe_Ok<TOk> err)
            => new Maybe<TOk, TError>(err);

        public static explicit operator (bool isOk, TOk whenOk, TError whenError)(Maybe<TOk, TError> maybe)
        {
            switch (maybe.okOrError) {
                case Maybe_Ok<TOk> okValue:
                    return (true, okValue.Value,default(TError));
                case Maybe_Error<TError> errValue:
                    return (false,default(TOk),errValue.Error);
                default:
                    return (false, default(TOk), default(TError));
            }
        }
    }

    public static class Maybe
    {
        /// <summary>
        /// Creates a succesful Maybe that stores the provided value.
        /// </summary>
        [NotNull]
        [Pure]
        public static Maybe_Ok<T> Ok<T>(T val)
            => new Maybe_Ok<T>(val);

        /// <summary>
        /// Creates a succesful Maybe value without a value.
        /// </summary>
        [NotNull]
        [Pure]
        public static Maybe_Ok<Unit> Ok()
            => new Maybe_Ok<Unit>(Unit.Value);

        /// <summary>
        /// Create a failed maybe with an error state describing a failed operation.
        /// </summary>
        [NotNull]
        [Pure]
        public static Maybe_Error<TError> Error<TError>(TError error)
            => new Maybe_Error<TError>(error);

        /// <summary>
        /// Create a failed maybe without any additional information about the error.
        /// </summary>
        [NotNull]
        [Pure]
        public static Maybe_Error<Unit> Error()
            => new Maybe_Error<Unit>(Unit.Value);

        public static Maybe<TOk, TError> Either<TOk, TError>(bool isOk, Func<TOk> whenOk, Func<TError> whenError)
            => isOk ? Ok(whenOk()).AsMaybeWithoutError<TError>() : Error(whenError());

        public static Maybe<TOk, TError> Either<TOk, TError>(bool isOk, TOk whenOk, TError whenError)
            => isOk ? Ok(whenOk).AsMaybeWithoutError<TError>() : Error(whenError);

        /// <summary>
        /// Converts a error to a Maybe&lt;Unit, TError&gt;.  A null translatable represents success, any other value the error message to display.
        /// </summary>
        [Pure]
        public static Maybe<Unit, TError> ErrorWhenNotNull<TError>([CanBeNull] TError val)
            where TError : class
            => Either(val == null, Unit.Value, val);

        /// <summary>
        /// Usage: Maybe.Try( () => Some.Thing.That(Can.Fail())).Catch&lt;SomeException&gt;()
        /// </summary>
        public static MaybeTryBody Try(Action tryBody)
            => new MaybeTryBody(tryBody);

        /// <summary>
        /// Usage: Maybe.Try( () => Some.Thing.That(Can.Fail())).Catch&lt;SomeException&gt;()
        /// </summary>
        public static MaybeTryBody<TOk> Try<TOk>(Func<TOk> tryBody)
            => new MaybeTryBody<TOk>(tryBody);
    }

    public struct MaybeTryBody
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
    }

    public struct MaybeTryBody<TOk>
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
    }
}
