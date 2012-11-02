using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MoreLinq;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
	public static class Translator
	{
		static readonly IDictionary<Taal, CultureInfo> CULTURES = new Dictionary<Taal, CultureInfo>
		{
			{ Taal.NL, new CultureInfo("nl-NL", false) },		
			{ Taal.EN, new CultureInfo("en-GB", false) },		
			{ Taal.DU, new CultureInfo("de-DE", false) },		
		};

		static Translator()
		{
			// TODO: ...
			CULTURES[Taal.NL].DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";
			CULTURES[Taal.EN].DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
			CULTURES[Taal.EN].NumberFormat.CurrencySymbol = CULTURES[Taal.NL].NumberFormat.CurrencySymbol;
			CULTURES[Taal.DU].DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
		}

		public static CultureInfo GetCulture(this Taal language)
		{
			return CULTURES[language];
		}
	}
}
