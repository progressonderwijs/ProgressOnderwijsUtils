using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class ToStringInvariantExtension
    {
        [Pure]
        public static string ToStringInvariant<T>(this T val)
            where T : struct, IConvertible
            => val.ToString(CultureInfo.InvariantCulture);

        [Pure]
        public static string ToStringInvariant<T>(this T val, string format)
            where T : struct, IFormattable
            => val.ToString(format, CultureInfo.InvariantCulture);

        [Pure]
        public static string ToStringInvariant<T>(this T? val)
            where T : struct, IConvertible
            => val == null ? "" : val.Value.ToString(CultureInfo.InvariantCulture);

        [Pure]
        public static string ToStringInvariant<T>(this T? val, string format)
            where T : struct, IFormattable
            => val == null ? "" : val.Value.ToString(format, CultureInfo.InvariantCulture);

        [Pure]
        [return: NotNullIfNotNull("val")]
        public static string? ToStringInvariantOrNull<T>(this T? val)
            where T : struct, IConvertible
            => val == null ? null : val.Value.ToString(CultureInfo.InvariantCulture);

        [Pure]
        [UsefulToKeep("Library function, other overloads used")]
        [return: NotNullIfNotNull("val")]
        public static string? ToStringInvariantOrNull<T>(this T? val, string format)
            where T : struct, IFormattable
            => val == null ? null : val.Value.ToString(format, CultureInfo.InvariantCulture);
    }
}
