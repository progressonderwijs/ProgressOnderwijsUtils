using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.Text
{
	public delegate string TranslateFunction(Taal taal = Taal.NL);
	public static class ConverteerHelper
	{
		public static TranslateFunction ToString(long l, string format) { return language => l.ToString(format ?? "D", Translator.GetCulture(language)); }
		public static TranslateFunction ToString(double d, string format) { return language => d.ToString(format ?? "0.0##########################", Translator.GetCulture(language)); }
		public static TranslateFunction ToString(decimal d, string format) { return language => d.ToString(format ?? GELD_EURO, Translator.GetCulture(language)); }
		public static TranslateFunction ToString(TimeSpan ts, string format) { return language => ts.ToString(format ?? "g", Translator.GetCulture(language)); }
		public static TranslateFunction ToString(DateTime dt, string format) { return language => dt.ToString(format ?? (dt == dt.Date ? ALLEEN_DATUM : DATUM_EN_TIJD_IN_MINUTEN), Translator.GetCulture(language)); }
		public static TranslateFunction ToString(FileData obj, string format) { return language => obj.ToUiString(); }
		public static TranslateFunction ToString(VariantData obj, string format) { return language => obj.ToUiString(); }
		public static TranslateFunction ToString(string obj, string format) { return language => obj; }
		public static TranslateFunction ToString(char obj, string format) { return language => new string((char)obj, 1); }
		public static TranslateFunction ToString(XHtmlData obj, string format) { return language => obj.ToUiString(); }
		public static TranslateFunction ToString<T>(T[] arr, string format) {
			var subtrans=arr.Select(obj => (TranslateFunction)ConverteerHelper.ToString((dynamic)obj, format)).ToArray();

			return language => subtrans.Select(f => f(language)).JoinStrings("\n"); 
		}


		public static TranslateFunction ToString(bool obj, string format) { 
			return language => {
				switch (language) {
					case Taal.NL: return obj ? "Ja" : "Nee";
					case Taal.EN: return obj ? "Yes" : "No";
					case Taal.DU: return obj ? "Ja" : "Nein";
					default: throw new ArgumentOutOfRangeException("language", "Taal niet bekend: " + language);
				}
			};
		}

		public const string GELD_EURO = "N02";
		public const string ALLEEN_DATUM = "dd-MM-yyyy";
		public const string DATUM_EN_TIJD_IN_MINUTEN = "dd-MM-yyyy HH:mm";

		public static TranslateFunction ToStringDynamic(object obj, string format = null)
		{
			if (obj == null || obj == DBNull.Value) return language => "";
			try
			{
				return ConverteerHelper.ToString((dynamic)obj, format);
			}
			catch (Exception e)
			{
				throw new ConverteerException("unknown type " + obj.GetType() + " to stringify", e);
			}
		}
	}
}
