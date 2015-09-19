using System;
using System.Globalization;

namespace ProgressOnderwijsUtils
{
    public static class StringFormatter
    {
        public static string FormatWithInvariantCulture(FormattableString interpolatedString)
        {
            return CultureInfo.InvariantCulture.FormatString(interpolatedString);
        }

        public static string FormatString(this CultureInfo culture, FormattableString interpolatedString)
        {
            return interpolatedString.ToString(culture);
        }
    }
}
