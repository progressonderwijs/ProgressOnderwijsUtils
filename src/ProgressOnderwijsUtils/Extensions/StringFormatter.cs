using System;
using System.Globalization;

namespace ProgressOnderwijsUtils
{
    public static class StringFormatter
    {
        public static string FormatString(this CultureInfo culture, FormattableString interpolatedString)
            => interpolatedString.ToString(culture);
    }
}
