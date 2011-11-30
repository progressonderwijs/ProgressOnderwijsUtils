using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Xml;

namespace ProgressOnderwijsUtils
{
	public static class ToStringInvariantExtension
	{
		public static string ToStringInvariant(this long val) { return val.ToString(CultureInfo.InvariantCulture); }
		public static string ToStringInvariant(this int val) { return val.ToString(CultureInfo.InvariantCulture); }
		public static string ToStringInvariant(this short val) { return val.ToString(CultureInfo.InvariantCulture); }
		public static string ToStringInvariant(this ulong val) { return val.ToString(CultureInfo.InvariantCulture); }
		public static string ToStringInvariant(this uint val) { return val.ToString(CultureInfo.InvariantCulture); }
		public static string ToStringInvariant(this ushort val) { return val.ToString(CultureInfo.InvariantCulture); }
		public static string ToStringInvariant(this double val) { return val.ToString(CultureInfo.InvariantCulture); }
		public static string ToStringInvariant(this decimal val) { return val.ToString(CultureInfo.InvariantCulture); }
		public static string ToStringInvariant(this float val) { return val.ToString(CultureInfo.InvariantCulture); }
		public static string ToStringInvariant(this bool val) { return val.ToString(CultureInfo.InvariantCulture); }
		public static string ToStringInvariant(this DateTime val) { return val.ToString(CultureInfo.InvariantCulture); }
	}
}
