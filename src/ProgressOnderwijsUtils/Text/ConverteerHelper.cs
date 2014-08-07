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

		static IEnumerable<TranslateFunction> ResolveTranslator(object obj, string format)
		{
			yield return TryToString<ITranslatable>(obj, o => language =>
				o.Translate(language).Text);
			yield return TryToString<string>(obj, o => language =>
				o);
			yield return TryToString<int>(obj, o => language =>
				((long)o).ToString(format ?? "D", language.GetCulture()));
			yield return TryToString<Enum>(obj, o => language =>
				Converteer.TranslateEnum(o).Translate(language).Text);
			yield return TryToString<decimal>(obj, o => language =>
				o.ToString(format ?? GELD_EURO, language.GetCulture()));
			yield return TryToString<DateTime>(obj, o => language =>
				o.ToString(format ?? (o == o.Date ? ALLEEN_DATUM : DATUM_EN_TIJD_IN_MINUTEN), language.GetCulture()));
			yield return TryToString<TimeSpan>(obj, o => language =>
				o.ToString(format ?? "g", language.GetCulture()));
			yield return TryToString<char>(obj, o => language =>
				new string(o, 1));
			yield return TryToString<XhtmlData>(obj, o => language =>
				o.ToUiString());
			yield return TryToString<VariantData>(obj, o => language =>
				o.ToUiString());
			yield return TryToString<FileData>(obj, o => language =>
				o.ContainsFile ? string.Format("{0} ({1} KB)", o.FileName, o.Content.Length / 1000m) : "");
			yield return TryToString<double>(obj, o => language =>
				o.ToString(format ?? "0.##", language.GetCulture()));
			yield return TryToString<long>(obj, o => language =>
				o.ToString(format ?? "D", language.GetCulture()));
			yield return TryToString<bool>(obj, o => language => {
				switch (language)
				{
					case Taal.NL: return o ? "Ja" : "Nee";
					case Taal.EN: return o ? "Yes" : "No";
					case Taal.DU: return o ? "Ja" : "Nein";
					default: throw new ArgumentOutOfRangeException("language", "Taal niet bekend: " + language);
				}
			});
			yield return TryToString<IIdentifier>(obj, o => language =>
				o.Value.ToString(format ?? "D", language.GetCulture()));
			yield return TryToString<IEnumerable>(obj, o =>
				ArrayToStringHelper(o, format));
		}

		public static TranslateFunction ToStringDynamic(object obj, string format = null)
		{
			if (obj == null || obj == DBNull.Value)
				return language => "";

			foreach (var func in ResolveTranslator(obj, format))
				if (func != null)
					return func;

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
