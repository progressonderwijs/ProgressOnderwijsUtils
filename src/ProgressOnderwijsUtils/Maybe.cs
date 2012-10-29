using System;
using System.Web;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// This type represents "nothing". Its only purpose is to satisfy the compiler; so e.g. when it expects a Func from A to B, and you only have an Action on A; you might pass a Func from A to Void.
	/// </summary>
	public struct Unit { public static Unit Value { get { return default(Unit); } } }

	public abstract class Maybe<T>
	{
		public abstract bool IsOk { get; }
		public abstract T GetValue();
		public abstract ITranslatable GetError();
		public abstract TOut ExtractToValue<TOut>(Func<T, TOut> ifOk, Func<ITranslatable, TOut> ifError);
		public static implicit operator Maybe<T>(Maybe.ErrorValue err) { return Maybe.Error<T>(err.ErrorMessage); }

		public static bool operator true(Maybe<T> state) { return state.IsOk; }
		public static bool operator false(Maybe<T> state) { return !state.IsOk; }
		public static Maybe<T> operator &(Maybe<Unit> maybe1, Maybe<T> maybe2) { return maybe1.ExtractToValue(_ => maybe2, Maybe.Error<T>); }
		//public static MaybeError<T> operator |(MaybeError<T> a, MaybeError<T> b) { return a.IsOk ? a : b; }

		private Maybe() { }

		public sealed class ErrorValue : Maybe<T>
		{
			readonly ITranslatable error;
			public ErrorValue(ITranslatable error)
			{
				if (error == null) throw new ArgumentNullException("error");
				this.error = error;
			}
			public override bool IsOk { get { return false; } }
			public override T GetValue() { throw new InvalidOperationException("Cannot get value; in error state: " + error.Translate(Taal.NL)); }
			public override ITranslatable GetError() { return error; }
			public override TOut ExtractToValue<TOut>(Func<T, TOut> ifOk, Func<ITranslatable, TOut> ifError) { return ifError(error); }
		}


		public sealed class OkValue : Maybe<T>
		{
			readonly T val;
			public OkValue(T val) { this.val = val; }
			public override bool IsOk { get { return true; } }
			public override T GetValue() { return val; }
			public override ITranslatable GetError() { throw new InvalidOperationException("No error: cannot get error message!"); }
			public override TOut ExtractToValue<TOut>(Func<T, TOut> ifOk, Func<ITranslatable, TOut> ifError) { return ifOk(val); }
		}
	}

	public static class Maybe
	{
		public struct ErrorValue
		{
			public readonly ITranslatable ErrorMessage;
			public ErrorValue(ITranslatable errorMessage)
			{
				if (errorMessage == null) throw new ArgumentNullException("errorMessage"); ErrorMessage = errorMessage;
			}
		}

		public static Maybe<T> Ok<T>(T val) { return new Maybe<T>.OkValue(val); }
		public static Maybe<Unit> Ok() { return new Maybe<Unit>.OkValue(Unit.Value); }
		public static Maybe<T> Error<T>(ITranslatable val) { return new Maybe<T>.ErrorValue(val); }
		public static ErrorValue Error(ITranslatable val) { return new ErrorValue(val); }


		public static Maybe<Unit> ErrorWhenNotNull(this ITranslatable val)
		{
			return val == null ? Ok() : new ErrorValue(val);
		}

		public static Maybe<TOut> WhenOk<T, TOut>(this Maybe<T> state, Func<T, TOut> map) { return state.ExtractToValue(v => Ok(map(v)), Error<TOut>); }
		public static Maybe<Unit> WhenOk<T>(this Maybe<T> state, Action<T> map) { return state.ExtractToValue(v => { map(v); return Ok(); }, Error<Unit>); }
		public static Maybe<TOut> WhenOkTry<T, TOut>(this Maybe<T> state, Func<T, Maybe<TOut>> map) { return state.ExtractToValue(map, Error<TOut>); }
	}
}