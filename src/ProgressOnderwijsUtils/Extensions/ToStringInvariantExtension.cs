using System;
using System.Globalization;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class ToStringInvariantExtension
    {
        [NotNull]
        [Pure]
        public static string ToStringInvariant<T>(this T val)
            where T : struct, IConvertible
            => val.ToString(CultureInfo.InvariantCulture);

        [NotNull]
        [Pure]
        public static string ToStringInvariant<T>(this T val, string format)
            where T : struct, IFormattable
            => val.ToString(format, CultureInfo.InvariantCulture);

        [NotNull]
        [Pure]
        public static string ToStringInvariant<T>(this T? val)
            where T : struct, IConvertible
            => val == null ? "" : val.Value.ToString(CultureInfo.InvariantCulture);

        [NotNull]
        [Pure]
        public static string ToStringInvariant<T>(this T? val, string format)
            where T : struct, IFormattable
            => val == null ? "" : val.Value.ToString(format, CultureInfo.InvariantCulture);

        [CanBeNull]
        [Pure]
        public static string ToStringInvariantOrNull<T>(this T? val)
            where T : struct, IConvertible
            => val == null ? null : val.Value.ToString(CultureInfo.InvariantCulture);

        [CanBeNull]
        [Pure]
        [UsefulToKeep("Library function, other overloads used")]
        public static string ToStringInvariantOrNull<T>(this T? val, string format)
            where T : struct, IFormattable
            => val == null ? null : val.Value.ToString(format, CultureInfo.InvariantCulture);
    }
}
