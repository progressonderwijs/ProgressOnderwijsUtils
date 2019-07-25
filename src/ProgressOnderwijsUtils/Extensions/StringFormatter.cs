#nullable disable
using System;
using System.Globalization;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class StringFormatter
    {
        [NotNull]
        public static string FormatString(this CultureInfo culture, [NotNull] FormattableString interpolatedString)
            => interpolatedString.ToString(culture);
    }
}
