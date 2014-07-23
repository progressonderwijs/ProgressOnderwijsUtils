using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace ProgressOnderwijsUtils
{
	public delegate string TranslateFunction(Taal taal = Taal.NL);
	public static class ConverteerHelper
	{
		#region ToString implementations for use from dynamic code
		// ReSharper disable MemberCanBePrivate.Global
		// ReSharper disable UnusedParameter.Global
		//used in dynamic code:
		public static TranslateFunction ToString(IIdentifier i, string format) { return language => i.Value.ToString(format ?? "D", language.GetCulture()); }
		public static TranslateFunction ToString(long l, string format) { return language => l.ToString(format ?? "D", language.GetCulture()); }
		public static TranslateFunction ToString(double d, string format) { return language => d.ToString(format ?? "0.##", language.GetCulture()); }
		public static TranslateFunction ToString(decimal d, string format) { return language => d.ToString(format ?? GELD_EURO, language.GetCulture()); }
		public static TranslateFunction ToString(TimeSpan ts, string format) { return language => ts.ToString(format ?? "g", language.GetCulture()); }
		public static TranslateFunction ToString(DateTime dt, string format) { return language => dt.ToString(format ?? (dt == dt.Date ? ALLEEN_DATUM : DATUM_EN_TIJD_IN_MINUTEN), language.GetCulture()); }
		public static TranslateFunction ToString(FileData obj, string format) { return language => obj.ContainsFile ? string.Format("{0} ({1} KB)", obj.FileName, obj.Content.Length / 1000m) : ""; }
		public static TranslateFunction ToString(VariantData obj, string format) { return language => obj.ToUiString(); }
		public static TranslateFunction ToString(string str, string format) { return language => str; }
		public static TranslateFunction ToString(char c, string format) { return language => new string(c, 1); }
		public static TranslateFunction ToString(XhtmlData obj, string format) { return language => obj.ToUiString(); }
		public static TranslateFunction ToString(ITranslatable obj, string format) { return language => obj.Translate(language).Text; }
		public static TranslateFunction ToString(Enum obj, string format) { return ToString(Converteer.TranslateEnum(obj), format); }
		public static TranslateFunction ToString<T>(T[] arr, string format)
		{
			var subtrans = arr.Select(obj => (TranslateFunction)ConverteerHelper.ToString((dynamic)obj, format)).ToArray();

			return language => subtrans.Select(f => f(language)).JoinStrings("\n");
		}


		public static TranslateFunction ToString(bool b, string format)
		{
			return language => {
				switch (language)
				{
					case Taal.NL: return b ? "Ja" : "Nee";
					case Taal.EN: return b ? "Yes" : "No";
					case Taal.DU: return b ? "Ja" : "Nein";
					default: throw new ArgumentOutOfRangeException("language", "Taal niet bekend: " + language);
				}
			};
		}
		// ReSharper restore UnusedParameter.Global
		// ReSharper restore MemberCanBePrivate.Global
		#endregion
		public const string GELD_EURO = "N02";
		public const string ALLEEN_DATUM = "dd-MM-yyyy";
		public const string DATUM_EN_TIJD_IN_MINUTEN = "dd-MM-yyyy HH:mm";

		public static TranslateFunction ToStringDynamic(object obj, string format = null)
		{
			if (obj == null || obj == DBNull.Value) return language => "";
			//perf critical: We avoid the overhead of dynamic in common cases by adding
			//these redundant if statements.  Removing them means (in particular) that
			//CriteriumFilter.ToString relies on dynamic code, which slows down test
			//discovery.

			if (obj is ITranslatable)
				return ToString((ITranslatable)obj, format);
			if (obj is string)
				return ToString((string)obj, format);
			if (obj is int)
				return ToString((int)obj, format);
			if (obj is Enum)
				return ToString((Enum)obj, format);
			if (obj is decimal)
				return ToString((decimal)obj, format);
			if (obj is DateTime)
				return ToString((DateTime)obj, format);
			if (obj is TimeSpan)
				return ToString((TimeSpan)obj, format);
			if (obj is char)
				return ToString((char)obj, format);
			if (obj is XhtmlData)
				return ToString((XhtmlData)obj, format);
			if (obj is VariantData)
				return ToString((VariantData)obj, format);
			if (obj is FileData)
				return ToString((FileData)obj, format);
			if (obj is double)
				return ToString((double)obj, format);
			if (obj is long)
				return ToString((long)obj, format);
			if (obj is IIdentifier)
				return ToString((IIdentifier)obj, format);
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
