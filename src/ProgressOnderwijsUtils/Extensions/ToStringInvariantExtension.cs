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
        {
            return val.ToString(CultureInfo.InvariantCulture);
        }

        [NotNull]
        [Pure]
        public static string ToStringInvariant<T>(this T val, string format)
            where T : struct, IFormattable
        {
            return val.ToString(format, CultureInfo.InvariantCulture);
        }

        [NotNull]
        [Pure]
        public static string ToStringInvariant<T>(this T? val)
            where T : struct, IConvertible
        {
            return val == null ? "" : val.Value.ToString(CultureInfo.InvariantCulture);
        }

        [NotNull]
        [Pure]
        public static string ToStringInvariant<T>(this T? val, string format)
            where T : struct, IFormattable
        {
            return val == null ? "" : val.Value.ToString(format, CultureInfo.InvariantCulture);
        }

        [CanBeNull]
        [Pure]
        public static string ToStringInvariantOrNull<T>(this T? val)
            where T : struct, IConvertible
        {
            return val == null ? null : val.Value.ToString(CultureInfo.InvariantCulture);
        }

        [CanBeNull]
        [Pure]
        [UsefulToKeep("Library function, other overloads used")]
        public static string ToStringInvariantOrNull<T>(this T? val, string format)
            where T : struct, IFormattable
        {
            return val == null ? null : val.Value.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
