using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ProgressOnderwijsUtils.Converteer
{
	public class ParseResult<T> : IEquatable<ParseResult<T>>
	{
		private readonly T value;
		private readonly string errorCode;
		public bool Success { get { return errorCode == null; } }
		public bool Failed { get { return !Success; } }
		public string ErrorCode { get { return errorCode; } }
		public T Value { get { if (Success) return value; throw new ConverteerException(); } }

		private ParseResult(T value, string errorCode) { this.value = value; this.errorCode = errorCode; }
		public static ParseResult<T> SuccessfulParse(T value) { return new ParseResult<T>(value, null); }
		public static ParseResult<T> FailedParse(string errorCode) { return new ParseResult<T>(default(T), errorCode); }



		public static implicit operator T(ParseResult<T> parseResult) { return parseResult.Value; }
		//public static implicit operator ParseResult<T>(T value) { return SuccessfulParse(value); }


		#region Equals + GetHashCode implementations.

		public bool Equals(ParseResult<T> other) { return errorCode == other.errorCode && (errorCode != null || object.Equals(value, other.value)); }
		public override bool Equals(object obj)
		{
			return
				obj is ParseResult<T>
				? Equals((ParseResult<T>)obj)
				: errorCode == null && object.Equals(value, obj);

			//see GetHashCode for consequences of this implementation.
		}

		//note that since a parseresult is equal to an error-free value, it must have the same hashcode as that error-free value!
		public override int GetHashCode() { return errorCode == null ? value.GetHashCode() : errorCode.GetHashCode() + 137; }

		//I override == to ensure that two different ParseResults with the same values are considered equal.  This is the right semantics for basic parseresults (i.e. like for int& datetime),
		//but may not be for complex types where you might be meaning to ask whether the parseresults refer to the same mutable object - but that's not something you want to do anyhow.
		public static bool operator ==(ParseResult<T> a, ParseResult<T> b) { return object.ReferenceEquals(a, b) || a.Equals(b); }
		public static bool operator !=(ParseResult<T> a, ParseResult<T> b) { return !(a == b); } //required by compiler.
		#endregion
	}

	public static class ParseResult
	{
		public static ParseResult<T> SuccessfulParse<T>(T value) { return ParseResult<T>.SuccessfulParse(value); }
		public static ParseResult<T> FailedParse<T>(string errorCode) { return ParseResult<T>.FailedParse(errorCode); }
	}
}
