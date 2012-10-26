using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	public static class ToStringInvariantExtension
	{
		[Pure]
		public static string ToStringInvariant(this long val) { return val.ToString(CultureInfo.InvariantCulture); }
		[Pure]
		public static string ToStringInvariant(this int val) { return val.ToString(CultureInfo.InvariantCulture); }
		[Pure]
		public static string ToStringInvariant(this short val) { return val.ToString(CultureInfo.InvariantCulture); }
		[Pure]
		public static string ToStringInvariant(this ulong val) { return val.ToString(CultureInfo.InvariantCulture); }
		[Pure]
		public static string ToStringInvariant(this uint val) { return val.ToString(CultureInfo.InvariantCulture); }
		[Pure]
		public static string ToStringInvariant(this ushort val) { return val.ToString(CultureInfo.InvariantCulture); }
		[Pure]
		public static string ToStringInvariant(this double val) { return val.ToString(CultureInfo.InvariantCulture); }
		[Pure]
		public static string ToStringInvariant(this decimal val) { return val.ToString(CultureInfo.InvariantCulture); }
		[Pure]
		public static string ToStringInvariant(this float val) { return val.ToString(CultureInfo.InvariantCulture); }
		[Pure]
		public static string ToStringInvariant(this bool val) { return val.ToString(CultureInfo.InvariantCulture); }
		[Pure]
		public static string ToStringInvariant(this DateTime val) { return val.ToString(CultureInfo.InvariantCulture); }

		[Pure]
		static TR Lift<T, TR>(T? nullable, Func<T, TR> f)
			where T : struct
			where TR : class
		{ return nullable == null ? default(TR) : f(nullable.Value); }

		[Pure]
		public static string ToStringInvariantOrNull(this long? val) { return Lift(val, ToStringInvariant); }
		[Pure]
		public static string ToStringInvariantOrNull(this int? val) { return Lift(val, ToStringInvariant); }
		[Pure]
		public static string ToStringInvariantOrNull(this short? val) { return Lift(val, ToStringInvariant); }
		[Pure]
		public static string ToStringInvariantOrNull(this ulong? val) { return Lift(val, ToStringInvariant); }
		[Pure]
		public static string ToStringInvariantOrNull(this uint? val) { return Lift(val, ToStringInvariant); }
		[Pure]
		public static string ToStringInvariantOrNull(this ushort? val) { return Lift(val, ToStringInvariant); }
		[Pure]
		public static string ToStringInvariantOrNull(this double? val) { return Lift(val, ToStringInvariant); }
		[Pure]
		public static string ToStringInvariantOrNull(this decimal? val) { return Lift(val, ToStringInvariant); }
		[Pure]
		public static string ToStringInvariantOrNull(this float? val) { return Lift(val, ToStringInvariant); }
		[Pure]
		public static string ToStringInvariantOrNull(this bool? val) { return Lift(val, ToStringInvariant); }
		[Pure]
		public static string ToStringInvariantOrNull(this DateTime? val) { return Lift(val, ToStringInvariant); }
	}
}
