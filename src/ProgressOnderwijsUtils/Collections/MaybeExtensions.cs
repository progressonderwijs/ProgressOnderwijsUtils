using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Resources;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ProgressOnderwijsUtils.Collections
{
    public static class MaybeExtensions
    {
        [return: MaybeNull]
        [Pure]
        public static TError ErrorOrNull<TOk, TError>(this Maybe<TOk, TError> state)
            where TError : class?
            => state.TryGet(out _, out var whenError) ? null : whenError;

        [Pure]
        public static TError? ErrorOrNull<TOk, TError>(this Maybe<TOk, TError?> state)
            where TError : struct
            => state.TryGet(out _, out var whenError) ? null : whenError;

        [Pure]
        public static TError? ErrorOrNullable<TOk, TError>(this Maybe<TOk, TError> state)
            where TError : struct
            => state.TryGet(out _, out var whenError) ? default(TError?) : whenError;

        [return: MaybeNull]
        [Pure]
        public static TOk ValueOrNull<TOk, TError>(this Maybe<TOk, TError> state)
            where TOk : class?
            => state.TryGet(out var whenOk, out _) ? whenOk : null;

        [Pure]
        public static TOk? ValueOrNull<TOk, TError>(this Maybe<TOk?, TError> state)
            where TOk : struct
            => state.TryGet(out var okValue, out _) ? okValue : null;

        [Pure]
        public static TOk? ValueOrNullable<TOk, TError>(this Maybe<TOk, TError> state)
            where TOk : struct
            => state.TryGet(out var okValue, out _) ? okValue : default(TOk?);

        public static TOk AssertOk<TOk, TError>(this Maybe<TOk, TError> state)
            => state.TryGet(out var okValue, out var error) ? okValue : throw new Exception("Assertion that Maybe is Ok failed; error state: " + error);

        public static TOk AssertOk<TOk, TError>(this Maybe<TOk, TError> state, Func<TError, Exception?> exceptionWhenError)
            => state.TryGet(out var okValue, out var error) ? okValue : throw exceptionWhenError(error) ?? new Exception("Assertion that Maybe is Ok failed; error state: " + error);

        public static TError AssertError<TOk, TError>(this Maybe<TOk, TError> state)
            => state.TryGet(out var okValue, out var error) ? throw new Exception("Assertion that Maybe is Error failed; ok state: " + okValue) : error;

        /// <summary>
        /// Maps a possibly failed value to a new value.
        /// When the input state is failed, the output state is also failed (with the same message).  If the input is OK, the output is OK and is mapped
        /// using the provided function.  The function is eagerly evaluated, i.e. not like Enumerable.Select, but like Enumerable.ToArray.
        /// </summary>
        [Pure]
        public static Maybe<TOkOut, TError> WhenOk<TOk, TError, TOkOut>(this Maybe<TOk, TError> state, Func<TOk, TOkOut> map)
            => state.TryGet(out var okValue, out var error) ? Maybe.Ok(map(okValue)) : Maybe.Error(error).AsMaybeWithoutValue<TOkOut>();

        /// <summary>
        /// Maps a possibly failed value to a new value.
        /// When the input state is failed, the output state is also failed (with the same message).  If the input is OK, the output is OK and is mapped
        /// using the provided function.  The function is eagerly evaluated, i.e. not like Enumerable.Select, but like Enumerable.ToArray.
        /// </summary>
        [Pure]
        public static Maybe<Unit, TError> WhenOk<TOk, TError>(this Maybe<TOk, TError> state, Action<TOk> actOnValue)
        {
            if (state.TryGet(out var okValue, out var error)) {
                actOnValue(okValue);
                return Maybe.Ok(Unit.Value);
            } else {
                return Maybe.Error(error);
            }
        }

        [Obsolete("When the state of a Maybe is a nested Maybe, the error of that nested state can easily be ignored.")]
        public static Maybe<Maybe<TOkOut, TIgnoredError>, TError> WhenOk<TOk, TError, TOkOut, TIgnoredError>(this Maybe<TOk, TError> state, Func<TOk, Maybe<TOkOut, TIgnoredError>> map)
            => state.TryGet(out var okValue, out var error) ? Maybe.Ok(map(okValue)) : Maybe.Error(error).AsMaybeWithoutValue<Maybe<TOkOut, TIgnoredError>>();

        /// <summary>
        /// Maps a possibly failed value to a new value.
        /// When the input state is failed, the output state is also failed (with the same message).  If the input is OK, the output is OK and is mapped
        /// using the provided function.  The function is eagerly evaluated, i.e. not like Enumerable.Select, but like Enumerable.ToArray.
        /// </summary>
        [Pure]
        public static Maybe<TOkOut, TError> WhenOk<TOkOut, TError>(this Maybe<Unit, TError> state, Func<TOkOut> map)
        {
            if (state.TryGet(out _, out var error)) {
                return Maybe.Ok(map());
            } else {
                return Maybe.Error(error);
            }
        }

        /// <summary>
        /// Maps an error state (when failed) into a new error state.
        /// When the input state is failed, the output state is also failed and is mapped using the provided function.
        /// If the input is OK, the output is OK with the same value.
        /// The function is eagerly evaluated, i.e. not like Enumerable.Select, but like Enumerable.ToArray.
        /// </summary>
        [UsefulToKeep("Library Function")]
        [Pure]
        public static Maybe<TOk, TErrorResult> WhenError<TOk, TError, TErrorResult>(this Maybe<TOk, TError> state, Func<TError, TErrorResult> map)
        {
            if (state.TryGet(out var okValue, out var error)) {
                return Maybe.Ok(okValue);
            } else {
                return Maybe.Error(map(error));
            }
        }

        /// <summary>
        /// Maps an error state (when failed) into a new error state.
        /// When the input state is failed, the output state is also failed and is mapped using the provided function.
        /// If the input is OK, the output is OK with the same value.
        /// The function is eagerly evaluated, i.e. not like Enumerable.Select, but like Enumerable.ToArray.
        /// </summary>
        [UsefulToKeep("Library Function")]
        [Pure]
        public static Maybe<TOk, Unit> WhenError<TOk, TError>(this Maybe<TOk, TError> state, Action<TError> map)
        {
            if (state.TryGet(out var okValue, out var error)) {
                return Maybe.Ok(okValue);
            } else {
                map(error);
                return Maybe.Error();
            }
        }

        /// <summary>
        /// Maps an error state (when failed) into a new error state.
        /// When the input state is failed, the output state is also failed and is mapped using the provided function.
        /// If the input is OK, the output is OK with the same value.
        /// The function is eagerly evaluated, i.e. not like Enumerable.Select, but like Enumerable.ToArray.
        /// </summary>
        [UsefulToKeep("Library Function")]
        [Pure]
        public static Maybe<TOk, TErrorResult> WhenError<TOk, TErrorResult>(this Maybe<TOk, Unit> state, Func<TErrorResult> map)
        {
            if (state.TryGet(out var okValue, out _)) {
                return Maybe.Ok(okValue);
            } else {
                return Maybe.Error(map());
            }
        }

        /// <summary>
        /// Calls the provided ifOk delegate only when the maybe is in the OK state.
        /// </summary>
        public static void IfOk<TOk, TError>(this Maybe<TOk, TError> state, Action<TOk> ifOk)
        {
            if (state.TryGet(out var okValue, out _)) {
                ifOk(okValue);
            }
        }

        /// <summary>
        /// Calls the provided ifOk delegate only when the maybe is in the OK state.
        /// </summary>
        public static void IfOk<TError>(this Maybe<Unit, TError> state, Action ifOk)
        {
            if (state.TryGet(out _, out _)) {
                ifOk();
            }
        }

        /// <summary>
        /// Calls the provided ifOk delegate only when the maybe is in the OK state, and the provided ifError delegate otherwise.
        /// </summary>
        public static void If<TOk, TError>(this Maybe<TOk, TError> state, Action<TOk> ifOk, Action<TError> ifError)
        {
            if (state.TryGet(out var okValue, out var error)) {
                ifOk(okValue);
            } else {
                ifError(error);
            }
        }

        /// <summary>
        /// Calls the provided ifOk delegate only when the maybe is in the OK state, and the provided ifError delegate otherwise.
        /// </summary>
        public static void If<TError>(this Maybe<Unit, TError> state, Action ifOk, Action<TError> ifError)
        {
            if (state.TryGet(out _, out var error)) {
                ifOk();
            } else {
                ifError(error);
            }
        }

        /// <summary>
        /// Calls the provided ifOk delegate only when the maybe is in the OK state, and the provided ifError delegate otherwise.
        /// </summary>
        public static void If<TOk>(this Maybe<TOk, Unit> state, Action<TOk> ifOk, Action ifError)
        {
            if (state.TryGet(out var okValue, out _)) {
                ifOk(okValue);
            } else {
                ifError();
            }
        }

        /// <summary>
        /// Calls the provided ifOk delegate only when the maybe is in the OK state, and the provided ifError delegate otherwise.
        /// </summary>
        public static void If(this Maybe<Unit, Unit> state, Action ifOk, Action ifError)
        {
            if (state.TryGet(out _, out _)) {
                ifOk();
            } else {
                ifError();
            }
        }

        /// <summary>
        /// Calls the provided ifError delegate only when the maybe is in the Error state.
        /// </summary>
        public static void IfError<TOk, TError>(this Maybe<TOk, TError> state, Action<TError> ifError)
        {
            if (!state.TryGet(out _, out var error)) {
                ifError(error);
            }
        }

        /// <summary>
        /// Calls the provided ifError delegate only when the maybe is in the Error state.
        /// </summary>
        public static void IfError<TOk>(this Maybe<TOk, Unit> state, Action ifError)
        {
            var isOk = state.TryGet(out _, out _);
            if (!isOk) {
                ifError();
            }
        }

        /// <summary>
        /// Returns whether this maybe contains this value. Returns false if the maybe is not ok
        /// </summary>
        [Pure]
        public static bool Contains<TOk, TError>(this Maybe<TOk, TError> state, TOk value)
        {
            var isOk = state.TryGet(out var okValue, out _);
            return isOk && Equals(okValue, value);
        }

        /// <summary>
        /// Returns whether this maybe contains a value matching the predicate. Returns false if the maybe is not ok
        /// </summary>
        [Pure]
        public static bool Contains<TOk, TError>(this Maybe<TOk, TError> state, Predicate<TOk> predicate)
            => state.TryGet(out var okValue, out _) && predicate(okValue);

        /// <summary>
        /// Returns whether this maybe contains this error value. Returns false if the maybe is ok
        /// </summary>
        [Pure]
        public static bool ContainsError<TOk, TError>(this Maybe<TOk, TError> state, TError value)
        {
            var isOk = state.TryGet(out _, out var error);
            return !isOk && Equals(error, value);
        }

        /// <summary>
        /// Returns whether this maybe contains an error value matching the predicate. Returns false if the maybe is ok
        /// </summary>
        [Pure]
        public static bool ContainsError<TOk, TError>(this Maybe<TOk, TError> state, Predicate<TError> predicate)
            => !state.TryGet(out _, out var error) && predicate(error);

        [Pure]
        public static Maybe<TOk[], TError[]> WhenAllOk<TOk, TError>(this IEnumerable<Maybe<TOk, TError>> maybes)
        {
            var (okValues, errValues) = maybes.Partition();
            return errValues.Any() ? (Maybe<TOk[], TError[]>)Maybe.Error(errValues) : Maybe.Ok(okValues).AsMaybeWithoutError<TError[]>();
        }

        [Pure]
        public static IEnumerable<TOk> WhereOk<TOk, TError>(this IEnumerable<Maybe<TOk, TError>> maybes)
        {
            foreach (var state in maybes) {
                if (state.TryGet(out var okValue, out _)) {
                    yield return okValue;
                }
            }
        }

        [Pure]
        public static IEnumerable<TError> WhereError<TOk, TError>(this IEnumerable<Maybe<TOk, TError>> maybes)
        {
            foreach (var state in maybes) {
                if (!state.TryGet(out _, out var error)) {
                    yield return error;
                }
            }
        }

        [Pure]
        public static (TOk[] okValues, TError[] errorValues) Partition<TOk, TError>(this IEnumerable<Maybe<TOk, TError>> maybes)
        {
            var okValues = new List<TOk>();
            var errValues = new List<TError>();
            foreach (var state in maybes) {
                if (state.TryGet(out var okValue, out var error)) {
                    okValues.Add(okValue);
                } else {
                    errValues.Add(error);
                }
            }
            return (okValues: okValues.ToArray(), errorValues: errValues.ToArray());
        }

        /// <summary>
        /// Maps a possibly failed value to a new value using a mapping function that itself can fail.
        /// When the input state is failed, the output state is also failed (with the same message).  If the input is OK, it is transformed using the
        /// provided "map" function (which may itself report an error).   The function is eagerly evaluated, i.e. not like Enumerable.Select, but like
        /// Enumerable.ToArray.
        /// </summary>
        [Pure]
        public static Maybe<TOut, TError> WhenOkTry<TOk, TError, TOut>(this Maybe<TOk, TError> state, Func<TOk, Maybe<TOut, TError>> map)
        {
            if (state.TryGet(out var okValue, out var error)) {
                return map(okValue);
            } else {
                return Maybe.Error(error);
            }
        }

        /// <summary>
        /// Maps a possibly failed value to a new value using a mapping function that itself can fail.
        /// When the input state is failed, the output state is also failed (with the same message).  If the input is OK, it is transformed using the
        /// provided "map" function (which may itself report an error).   The function is eagerly evaluated, i.e. not like Enumerable.Select, but like
        /// Enumerable.ToArray.
        /// </summary>
        [Pure]
        public static Maybe<TOut, TError> WhenOkTry<TError, TOut>(this Maybe<Unit, TError> state, Func<Maybe<TOut, TError>> map)
        {
            if (state.TryGet(out _, out var error)) {
                return map();
            } else {
                return Maybe.Error(error);
            }
        }

        /// <summary>
        /// Maps a possibly failed value to a new value using a mapping function that itself can fail.
        /// When the input state is failed, the output state is also failed (with the same message).  If the input is OK, it is transformed using the
        /// provided "map" function (which may itself report an error).   The function is eagerly evaluated, i.e. not like Enumerable.Select, but like
        /// Enumerable.ToArray.
        /// </summary>
        [Pure]
        public static Maybe<TOk, TErrorOut> WhenErrorTry<TOk, TError, TErrorOut>(this Maybe<TOk, TError> state, Func<TError, Maybe<TOk, TErrorOut>> map)
        {
            if (state.TryGet(out var okValue, out var error)) {
                return Maybe.Ok(okValue);
            } else {
                return map(error);
            }
        }

        /// <summary>
        /// Maps a possibly failed value to a new value using a mapping function that itself can fail.
        /// When the input state is failed, the output state is also failed (with the same message).  If the input is OK, it is transformed using the
        /// provided "map" function (which may itself report an error).   The function is eagerly evaluated, i.e. not like Enumerable.Select, but like
        /// Enumerable.ToArray.
        /// </summary>
        [Pure]
        public static Maybe<TOk, TErrorOut> WhenErrorTry<TOk, TErrorOut>(this Maybe<TOk, Unit> state, Func<Maybe<TOk, TErrorOut>> map)
        {
            if (state.TryGet(out var okValue, out _)) {
                return Maybe.Ok(okValue);
            } else {
                return map();
            }
        }

        /// <summary>
        /// Conceptually equivalent to WhenOkTry (for linq compatibility).
        /// This is necessary for query expressions; see https://msdn.microsoft.com/en-us/library/bb308966.aspx#csharp3.0overview_topic19
        /// </summary>
        [Pure]
        public static Maybe<TOkResult, TError> SelectMany<TOk, TError, TOkTemp, TOkResult>(this Maybe<TOk, TError> state, Func<TOk, Maybe<TOkTemp, TError>> tempSelector, Func<TOk, TOkTemp, TOkResult> resultSelector)
        {
            if (!state.TryGet(out var okValue, out var error)) {
                return Maybe.Error(error);
            }
            if (tempSelector(okValue).TryGet(out var tempValue, out var tempError)) {
                return Maybe.Ok(resultSelector(okValue, tempValue));
            } else {
                return Maybe.Error(tempError);
            }
        }

        //=> state.Extract(source => collectionSelector(source).Select(collection => resultSelector(source, collection)), Maybe.Error<TOkResult>);
        /// <summary>
        /// Equivalent to WhenOkTry (for linq compatibility)
        /// </summary>
        [UsefulToKeep("This is the standard overload of the query-expression required SelectMany")]
        [Pure]
        public static Maybe<TOkResult, TError> SelectMany<TOk, TError, TOkResult>(this Maybe<TOk, TError> state, Func<TOk, Maybe<TOkResult, TError>> map)
        {
            if (state.TryGet(out var okValue, out var error)) {
                return map(okValue);
            } else {
                return Maybe.Error(error);
            }
        }

        /// <summary>
        /// Equivalent to WhenOk (for linq compatibility)
        /// </summary>
        [Pure]
        public static Maybe<TOkResult, TError> Select<TOk, TError, TOkResult>(this Maybe<TOk, TError> state, Func<TOk, TOkResult> map)
        {
            if (state.TryGet(out var okValue, out var error)) {
                return Maybe.Ok(map(okValue));
            } else {
                return Maybe.Error(error);
            }
        }
    }
}
