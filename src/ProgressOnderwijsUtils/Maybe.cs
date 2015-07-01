using System;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// This type represents "nothing". Its only purpose is to satisfy the compiler; so e.g. when it expects a Func from A to B, and you only have an Action on A; you might pass a Func from A to Void.
    /// </summary>
    public struct Unit
    {
        public static Unit Value => default(Unit);
    }

    /// <summary>
    /// A value or an error message describing why no value is present.
    /// 
    /// "Maybe" can be used to pass around possibly missing or erroneous values that aren't exceptional (e.g. that should not cause a crash).
    /// See the utility functions  "WhenOk", "WhenOkTry" and "ExtractToValue" for ways to handily combine values of type Maybe. 
    /// 
    /// "Maybe" is immutable and hence thread safe (assuming the wrapped value is thread safe).
    /// </summary>
    public abstract class Maybe<T>
    {
        /// <summary>
        /// Use this sparingly!  Often "WhenOk",  "WhenOkTry" and "ExtractToValue" result in clearer code.
        /// 
        /// Value: whether this Maybe is in the OK state.
        /// </summary>
        public abstract bool IsOk { get; }

        /// <summary>
        /// Returns whether this maybe contains this value. Returns false if the maybe is not ok
        /// </summary>
        public abstract bool Contains(T value);

        /// <summary>
        /// Gets the value of this Maybe if it is OK; throws an Exception if called when this Maybe is not OK.
        /// </summary>
        public abstract T GetValue();

        /// <summary>
        /// Gets the error message of this Maybe if present; throws an Exception if called when this Maybe is OK.
        /// </summary>
        public abstract ITranslatable GetError();

        /// <summary>
        /// Extracts a value from the Maybe by calling either the ifOk function or the ifError function, depending on the state of the Maybe.
        /// </summary>
        public abstract TOut ExtractToValue<TOut>(Func<T, TOut> ifOk, Func<ITranslatable, TOut> ifError);

        /// <summary>
        /// 
        /// </summary>
        public abstract void If(Action<T> ifOk, Action<ITranslatable> ifError);

        /// <summary>
        /// Converts an untyped error message into a specific type of failed Maybe.  This operator is a  workaround to make it easy to create an error message without redundant type info.
        /// </summary>
        public static implicit operator Maybe<T>(Maybe.ErrorValue err) { return Maybe.Error<T>(err.ErrorMessage); }

        Maybe() { }

        public sealed class ErrorValue : Maybe<T>
        {
            readonly ITranslatable error;

            public ErrorValue(ITranslatable error)
            {
                if (error == null) {
                    throw new ArgumentNullException(nameof(error));
                }
                this.error = error;
            }

            public override bool IsOk => false;
            public override bool Contains(T value) { return false; }
            public override T GetValue() { throw new InvalidOperationException("Cannot get value; in error state: " + error.Translate(Taal.NL)); }
            public override ITranslatable GetError() => error;
            public override TOut ExtractToValue<TOut>(Func<T, TOut> ifOk, Func<ITranslatable, TOut> ifError) { return ifError(error); }
            public override void If(Action<T> ifOk, Action<ITranslatable> ifError) { ifError(error); }
            public override string ToString() { return base.ToString() + $"({error})"; }

        }

        public sealed class OkValue : Maybe<T>
        {
            readonly T val;
            public OkValue(T val) { this.val = val; }
            public override bool IsOk => true;
            public override bool Contains(T value) { return Equals(value, val); }
            public override T GetValue() => val;
            public override ITranslatable GetError() { throw new InvalidOperationException("No error: cannot get error message!"); }
            public override TOut ExtractToValue<TOut>(Func<T, TOut> ifOk, Func<ITranslatable, TOut> ifError) { return ifOk(val); }
            public override void If(Action<T> ifOk, Action<ITranslatable> ifError) { ifOk(val); }

            public override string ToString() { return base.ToString() + $"({val})"; }
        }
    }

    public static class Maybe
    {
        public struct ErrorValue
        {
            public readonly ITranslatable ErrorMessage;

            public ErrorValue(ITranslatable errorMessage)
            {
                if (errorMessage == null) {
                    throw new ArgumentNullException(nameof(errorMessage));
                }
                ErrorMessage = errorMessage;
            }
        }

        /// <summary>
        /// Creates a succesful Maybe that stores the provided value.
        /// </summary>
        public static Maybe<T> Ok<T>(T val) { return new Maybe<T>.OkValue(val); }

        /// <summary>
        /// Creates a succesful Maybe value without a value.
        /// </summary>
        public static Maybe<Unit> Ok() => new Maybe<Unit>.OkValue(Unit.Value);

        /// <summary>
        /// Creates a failed Maybe value with the specified error message.
        /// </summary>
        public static Maybe<T> Error<T>(ITranslatable val) { return new Maybe<T>.ErrorValue(val); }

        /// <summary>
        /// Create an error message to show to the user describing a failed operation.
        /// </summary>
        /// <param name="val">The text of the error message</param>
        /// <returns>An ErrorValue (this is implicitly cast to whatever type of Maybe&lt;T&gt; you use.</returns>
        public static ErrorValue Error(ITranslatable val) => new ErrorValue(val);

        /// <summary>
        /// Create an error message to show to the user describing a failed operation.
        /// </summary>
        /// <param name="val">The text of the error message</param>
        /// <returns>An ErrorValue (this is implicitly cast to whatever type of Maybe&lt;T&gt; you use.</returns>
        public static ErrorValue Error(string val) => Error(Translatable.Literal(val));

        /// <summary>
        /// Converts an ITranslatable to a Maybe&lt;Unit&gt;.  A null translatable represents success, any other value the error message to display.
        /// </summary>
        public static Maybe<Unit> ErrorWhenNotNull(this ITranslatable val) => val == null ? Ok() : new ErrorValue(val);

        public static ITranslatable ErrorOrNull<T>(this Maybe<T> state) { return state.ExtractToValue(v => null, e => e); }

        /// <summary>
        /// Maps a possibly failed value to a new value.
        /// When the input state is failed, the output state is also failed (with the same message).  If the input is OK, the output is OK and is mapped
        /// using the provided function.  The function is eagerly evaluated, i.e. not like Enumerable.Select, but like Enumerable.ToArray.
        /// </summary>
        public static Maybe<TOut> WhenOk<T, TOut>(this Maybe<T> state, Func<T, TOut> map) { return state.ExtractToValue(v => Ok(map(v)), Error<TOut>); }
        
        /// <summary>
        /// Maps a possibly failed value to a new value.
        /// When the input state is failed, the output state is also failed (with the same message).  If the input is OK, the output is OK and is mapped
        /// using the provided function.  The function is eagerly evaluated, i.e. not like Enumerable.Select, but like Enumerable.ToArray.
        /// </summary>
        [UsefulToKeep("Library Function")]
        public static Maybe<TOut> WhenOk<TOut>(this Maybe<Unit> state, Func<TOut> map) { return state.ExtractToValue(v => Ok(map()), Error<TOut>); }

        /// <summary>
        /// Processes a possibly failed value.  
        /// When the input state is failed, the output state is also failed (with the same message).  If the input is OK, the output is OK, but the output
        /// has no value (value of type Unit).  The function is eagerly evaluated, i.e. not like Enumerable.Select, but like Enumerable.ToArray.
        /// </summary>
        public static Maybe<Unit> WhenOk<T>(this Maybe<T> state, Action<T> map)
        {
            return state.ExtractToValue(
                v => {
                    map(v);
                    return Ok();
                },
                Error<Unit>);
        }

        /// <summary>
        /// Processes a possibly failed value.  
        /// When the input state is failed, the output state is also failed (with the same message).  If the input is OK, the output is OK, but the output
        /// has no value (value of type Unit).  The function is eagerly evaluated, i.e. not like Enumerable.Select, but like Enumerable.ToArray.
        /// </summary>
        public static Maybe<Unit> WhenOk(this Maybe<Unit> state, Action map) {
            return state.ExtractToValue(
                v => {
                    map();
                    return Ok();
                },
                Error<Unit>);
        }

        /// <summary>
        /// Processes a possibly failed value.  
        /// When the input state is failed, the provided error handler is called and the output state is also failed (with the same message).
        /// If the input is OK, the output is OK with the same value.
        /// </summary>
        public static Maybe<T> WhenError<T>(this Maybe<T> state, Action<ITranslatable> handler)
        {
            return state.ExtractToValue(
                Ok,
                err => {
                    handler(err);
                    return Error(err);
                });
        }

        /// <summary>
        /// Maps a possibly failed value to a new value using a mapping function that itself can fail.
        /// When the input state is failed, the output state is also failed (with the same message).  If the input is OK, it is transformed using the
        /// provided "map" function (which may itself report an error).   The function is eagerly evaluated, i.e. not like Enumerable.Select, but like
        /// Enumerable.ToArray.
        /// </summary>
        public static Maybe<TOut> WhenOkTry<T, TOut>(this Maybe<T> state, Func<T, Maybe<TOut>> map) { return state.ExtractToValue(map, Error<TOut>); }
    }
}
