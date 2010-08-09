using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Globalization;

namespace ProgressOnderwijsUtils
{
	public static class ParseHelper
	{

		//Om gebruikersinvoer te controleren, daarna kan parse plaatsvinden
		public static string TryParse(string s, Type t, CultureInfo culture)
		{
			if (t == typeof(int))
			{
				string foutkey = "";
				try { int dummy = int.Parse(s); }
				catch (ArgumentNullException) { foutkey = "geendata"; }
				catch (FormatException) { foutkey = "illegaledatainkolom"; }
				catch (OverflowException) { foutkey = "overflow"; }
				return foutkey;
			}
			if (t == typeof(int?)) if (s == "") return ""; else { return TryParse(s, typeof(int), culture ); }
			if (t == typeof(decimal))
			{
				string foutkey = "";
				try
				{
					ParseDecimal(s,culture);
				}
				catch (ArgumentNullException) { foutkey = "geendata"; }
				catch (FormatException) { foutkey = "illegaledatainkolom"; }
				catch (OverflowException) { foutkey = "overflow"; }
				return foutkey;
			}
			if (t == typeof(double))
			{
				string foutkey = "";
				try { double dummy = double.Parse(s, culture); }
				catch (ArgumentNullException) { foutkey = "geendata"; }
				catch (FormatException) { foutkey = "illegaledatainkolom"; }
				catch (OverflowException) { foutkey = "overflow"; }
				return foutkey;
			}
			if (t == typeof(decimal?)) if (s == "") return ""; else { return TryParse(s, typeof(decimal), culture ); }
			if (t == typeof(DateTime))
			{
				string foutkey = "";
				try
				{
					DateTime dummy = ParseDateTime(s, culture);
				}
				catch (FormatException)
				{
					foutkey = "foutdatumformaat";
				}
				return foutkey;
			}
			if (t == typeof(DateTime?)) if (s == "") return ""; else { return TryParse(s, typeof(DateTime),culture); }
			if (t == typeof(string)) return "";
			throw new Exception("TryParse nog niet geimplementeerd voor " + t.ToString());
		}

		//Deze functie gebruiken om gebruikersinvoer in vrije textvelden te converteren naar de juiste types, eerste TryParse uitvoeren!
		public static object Parse(string s, Type t, CultureInfo culture)
		{
			Type nonNullableType = t.GetNullableBaseType();
			bool canBeNull = nonNullableType != null || !t.IsValueType;
			Type fundamentalType = nonNullableType ?? t;

			if (canBeNull && s.Length == 0) return null; // dus s=="" levert null string op!
			//RW 24-04-2008 Moet wel zo, maar wat zijn de gevolgen?

			if (fundamentalType == typeof(int)) return int.Parse(s);
			if (fundamentalType == typeof(decimal)) return ParseDecimal(s, culture);		// Decimale punt ook ondersteunen - let wel, dus 100,000.0 != 100,000 zo
			if (fundamentalType == typeof(DateTime)) return ParseDateTime(s, culture); 
			if (fundamentalType == typeof(string)) return s;
			if (fundamentalType == typeof(bool)) return bool.Parse(s);
			throw new Exception("Parse nog niet geimplementeerd voor " + t.ToString());
		}

		private static DateTime ParseDateTime(string s, CultureInfo culture)		{
			// Controleer of de datum al dan geen delimiters bevat: 06-11-2007 vs 06112007
			// Als de delimiters ontbreken deze even zetten
			
			if (Regex.IsMatch(s, @"^\d{6,8}$"))
			{
				s = s.Substring(0, 2) + "-" +
						s.Substring(2, 2) + "-" +
						s.Substring(4, s.Length - 4);
			}

			return DateTime.Parse(s, culture);		
		}

		public static decimal ParseDecimal(string s, CultureInfo culture)
		{
			return s.LastIndexOf(',') < s.LastIndexOf('.') //when true, either only decimal point, or thousand sep commas with decimal point: that's the invariant format anyhow.
								? decimal.Parse(s, CultureInfo.InvariantCulture)
								: decimal.Parse(s, culture);
		}
	}
}
