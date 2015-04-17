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
        public static string ToStringInvariant<T>(this T val)
            where T : struct, IConvertible { return val.ToString(CultureInfo.InvariantCulture); }

        [Pure]
        public static string ToStringInvariant<T>(this T val, string format)
            where T : struct, IFormattable { return val.ToString(format, CultureInfo.InvariantCulture); }

        [Pure]
        public static string ToStringInvariant<T>(this T? val)
            where T : struct, IConvertible { return val == null ? "" : val.Value.ToString(CultureInfo.InvariantCulture); }

        [Pure]
        public static string ToStringInvariant<T>(this T? val, string format)
            where T : struct, IFormattable { return val == null ? "" : val.Value.ToString(format, CultureInfo.InvariantCulture); }

        [Pure]
        public static string ToStringInvariantOrNull<T>(this T? val)
            where T : struct, IConvertible { return val == null ? null : val.Value.ToString(CultureInfo.InvariantCulture); }

        [Pure]
        public static string ToStringInvariantOrNull<T>(this T? val, string format)
            where T : struct, IFormattable { return val == null ? null : val.Value.ToString(format, CultureInfo.InvariantCulture); }
    }
}
