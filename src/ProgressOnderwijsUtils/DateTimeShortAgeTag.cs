using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	public static class DateTimeShortAgeTag
	{
		static readonly char[] trimStart = new[] { 'A' };
		static readonly char[] trimEnd = new[] { '=' };
		/// <summary>
		/// The purpose of an AgeTag is to provide an unlikely-to-collide unique token based on modification time.
		/// A tick is 10^-7 seconds; so Ticks>>23 ~ 0.84 seconds
		/// now, we take 0xffffff of those pseudo seconds, or 2^24*2^23*10^-7 seconds, i.e. 162.9 days, or about 0.5 years.
		/// after this time, the AgeTag will loop.  So our Tag is granular to the second, and unique within a half year.
		/// </summary>
		public static string ToAgeTag(DateTime datetime)
		{
			uint granularTicks = (uint)(datetime.Ticks >> 23) & 0xffffff;
			byte[] bytes = BitConverter.GetBytes(granularTicks).Reverse().ToArray();//reverse so most significant first.
			return Convert.ToBase64String(bytes).TrimStart(trimStart).TrimEnd(trimEnd); //we trim off unneeded preceding "zero" ('A') and trailing padding chars ('=')
		}
		/// <summary>
		/// The purpose of an AgeTag is to provide an unlikely-to-collide unique token based on modification time.
		/// A tick is 10^-7 seconds; so Ticks>>23 ~ 0.84 seconds
		/// now, we take 0xffffff of those pseudo seconds, or 2^24*2^23*10^-7 seconds, i.e. 162.9 days, or about 0.5 years.
		/// after this time, the AgeTag will loop.  So our Tag is granular to the second, and unique within a half year.
		/// </summary>
		public static string ToAgeTagCaseInsensitive(DateTime datetime)
		{
			uint granularTicks = (uint)(datetime.Ticks >> 23) & 0xffffff; //3byte code
			
			return granularTicks.ToString("x2",CultureInfo.InvariantCulture);
		}
	}
}
