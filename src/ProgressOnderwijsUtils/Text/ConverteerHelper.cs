using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace ProgressOnderwijsUtils
{
	public delegate string TranslateFunction(Taal taal = Taal.NL);


	public static class ConverteerHelper
	{
		public const string GELD_EURO = "N02";
		public const string ALLEEN_DATUM = "dd-MM-yyyy";
		public const string DATUM_EN_TIJD_IN_MINUTEN = "dd-MM-yyyy HH:mm";

		static TranslateFunction TryToString<T>(object obj, Func<T, TranslateFunction> translator)
		{
			return obj is T ? translator((T)obj) : null;
		}

		static TranslateFunction OrToString<T>(this TranslateFunction f, object obj, Func<T, TranslateFunction> translator)
		{
			return f ?? TryToString(obj, translator);
		}

		public static TranslateFunction ToStringDynamic(object obj, string format = null)
		{
			if (obj == null || obj == DBNull.Value)
				return language => "";
			//perf critical: We avoid the overhead of dynamic in common cases by adding
			//these redundant if statements.  Removing them means (in particular) that
			//CriteriumFilter.ToString relies on dynamic code, which slows down test
			//discovery.

			var retval = TryToString<ITranslatable>(obj, o => language => o.Translate(language).Text)
				.OrToString<string>(obj, o => language =>
					o)
				.OrToString<int>(obj, o => language =>
					((long)o).ToString(format ?? "D", language.GetCulture()))
				.OrToString<Enum>(obj, o => language => Converteer.TranslateEnum(o).Translate(language).Text)
				.OrToString<decimal>(obj, o => language =>
					o.ToString(format ?? GELD_EURO, language.GetCulture()))
				.OrToString<DateTime>(obj, o => language =>
					o.ToString(format ?? (o == o.Date ? ALLEEN_DATUM : DATUM_EN_TIJD_IN_MINUTEN), language.GetCulture()))
				.OrToString<TimeSpan>(obj, o => language =>
					o.ToString(format ?? "g", language.GetCulture()))
				.OrToString<char>(obj, o => language =>
					new string(o, 1))
				.OrToString<XhtmlData>(obj, o => language =>
					o.ToUiString())
				.OrToString<VariantData>(obj, o => language =>
					o.ToUiString())
				.OrToString<FileData>(obj, o => language =>
					o.ContainsFile ? string.Format("{0} ({1} KB)", o.FileName, o.Content.Length / 1000m) : "")
				.OrToString<double>(obj, o => language =>
					o.ToString(format ?? "0.##", language.GetCulture()))
				.OrToString<long>(obj, o => language =>
					o.ToString(format ?? "D", language.GetCulture()))
				.OrToString<IIdentifier>(obj, o => language =>
					o.Value.ToString(format ?? "D", language.GetCulture()))
				.OrToString<IEnumerable>(obj, o => ArrayToStringHelper(o, format))
				;
			if (retval != null)
				return retval;
			else
				throw new ConverteerException("unknown type " + obj.GetType() + " to stringify");
		}

		static TranslateFunction ArrayToStringHelper(IEnumerable o, string format)
		{
			var subtrans = o.Cast<object>().Select(elem =>
				ToStringDynamic(elem, format)).ToArray();

			return language => subtrans.Select(f => f(language)).JoinStrings("\n");
		}
	}
}
