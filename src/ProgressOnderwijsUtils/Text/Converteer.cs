using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MoreLinq;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Text;

namespace ProgressOnderwijsUtils
{

	/// <summary>
	/// De plek om de waarde van een object te representeren in een string voor een bepaalde taal.
	/// 
	/// Er is een generieke ToString functie die de waarde van een object representeerd in een string voor een bepaalde taal.
	/// En er is een generieke ToText functie die de waarde van een object representeerd in een translatable voor alle talen.
	/// De ToText functie maakt gebruik van de ToString functie om de strings voor de verschillende talen te achterhalen.
	/// 
	/// Elk type heeft zijn eigen default formatering die we in het algemeen willen gebruiken in Progress.NET.
	/// Deze default formatering is echter te overrulen middels standaard/custom .NET formaterings strings.
	/// 
	/// De volgende types worden op dit moment ondersteund:
	/// - bool (niet bij ToString)
	/// - sbyte
	/// - byte
	/// - short
	/// - ushort
	/// - int
	/// - uint
	/// - long
	/// - decimal
	/// - char
	/// - string
	/// - TimeSpan
	/// - DateTime
	/// - FileData
	/// - VariantData
	/// 
	/// De volgende talen worden op dit moment ondersteund:
	/// - Nederlands
	/// - Engels
	/// </summary>
	public static class Converteer
	{
		public static readonly CultureInfo CultureNL = new CultureInfo("nl-NL", false) { DateTimeFormat = { ShortDatePattern = "dd-MM-yyyy" } };

		#region Configuration

		public static readonly IDictionary<DatumFormaat, Tuple<string, string>> DATE_FORMATS = new Dictionary<DatumFormaat, Tuple<string, string>>
		{
			{ DatumFormaat.AlleenDatum, Tuple.Create(ConverteerHelper.ALLEEN_DATUM, (string)null) },
			{ DatumFormaat.AlleenTijd, Tuple.Create("HH:mm", (string)null) },
			{ DatumFormaat.DatumEnTijdInMinuten, Tuple.Create(ConverteerHelper.DATUM_EN_TIJD_IN_MINUTEN, (string)null) },
			{ DatumFormaat.DatumEnTijdInSeconden, Tuple.Create("dd-MM-yyyy HH:mm:ss", (string)null) },
			{ DatumFormaat.DatumEnTijdInMilliSeconden, Tuple.Create("dd-MM-yyyy HH:mm:ss.fff", (string)null) },
			{ DatumFormaat.DatumToolTipTijd, Tuple.Create("dd-MM-yyyy", "dd-MM-yyyy HH:mm:ss.fff") },
			{ DatumFormaat.JaarToolTipDatum, Tuple.Create("yyyy", "dd-MM-yyyy") },
			{ DatumFormaat.DatumZonderJaar, Tuple.Create("dd-MM", "dd-MM-yyyy") },
			{ DatumFormaat.SMDatum, Tuple.Create("yyyyMMdd", (string)null) },
			{ DatumFormaat.SMDatumTijd, Tuple.Create("yyyyMMddHHmmss", (string)null) },
			{ DatumFormaat.ClieopDatum, Tuple.Create("ddMMyy", (string)null) },
			{ DatumFormaat.MT940Datum, Tuple.Create("yyMMdd", (string)null) },
			{ DatumFormaat.VerwInfoDatum, Tuple.Create("yyMMdd", (string)null) },
			{ DatumFormaat.ISODate, Tuple.Create("yyyy-MM-dd", (string)null) },
			{ DatumFormaat.ISODateTime, Tuple.Create("yyyy-MM-ddTHH:mm:ss", (string)null) },
		};


		static readonly IDictionary<NummerFormaat, string> DECIMAL_FORMATS = new Dictionary<NummerFormaat, string>
		{
			{ NummerFormaat.GeldEuro, ConverteerHelper.GELD_EURO },
			{ NummerFormaat.Afgerond, "#" },
			{ NummerFormaat.AfgerondOp1Decimaal, "#.#" },
			{ NummerFormaat.AfgerondOp2Decimaal,  "#.##" },
			{ NummerFormaat.GeldEuroGrootboek, "#,##0.00 D;#,##0.00 C;0.00  "}
		};

		#endregion

		#region ToString

		/// <summary>
		/// Generieke string-efy functie die de default Progress.NET formatering en culture-info gebruikt.
		/// </summary>
		/// <param name="obj">De waarde te string-efy</param>
		/// <param name="format">Optionele formatering string om de default Progress.NET formatering te overrulen 
		/// 	(zie .Net documentatie voor details)</param>
		/// <param name="language">De taal die de te gebruiken culture aangeeft</param>
		/// <returns>De geformateerde string die de waarde representeerd</returns>
		public static string ToString(object obj, string format = null, Taal language = Taal.NL)
		{
			return ConverteerHelper.ToStringDynamic(obj, format)(language);
		}

		/// <summary>
		/// Utility functie die de DatumFormaat enum vertaald naar de overeenkomstige format string.
		/// </summary>
		public static string ToString(DateTime? dt, DatumFormaat format, Taal language = Taal.NL)
		{
			return ConverteerHelper.ToStringDynamic(dt, DATE_FORMATS[format].Item1)(language);
		}

		#endregion

		#region ToText

		/// <summary>
		/// Generieke text-efy functie die de default Progress.NET formatering en culture-info gebruikt om een waarde een vertaalbare string representatie te geven.
		/// </summary>
		/// <param name="obj">De waarde te text-efy</param>
		/// <param name="format">Optionele formatering string om de default Progress.NET formatering voor het tekst gedeelte te overrulen 
		/// (zie de .NET documentatie voor formatstring details)</param>
		/// <param name="extraformat">Extra formatering string om de default Progress.NET formatering voor het tooltip gedeelte te overrulen 
		/// (zie de .NET documentatie voor formatstring details)</param>
		/// <returns>De geformateerde translatable die de waarde representeerd in de Nederlands en Engelse taal.</returns>
		public static ITranslatable ToText(object obj, string format = null, string extraformat = null)
		{
			if (obj == null || obj == DBNull.Value)
				return TextDefSimple.EmptyText;
			else if (obj is ITranslatable)
				return (ITranslatable)obj;
			else if (obj is TextVal)
				return new TextDefSimple((TextVal)obj);
			else if (String.IsNullOrEmpty(extraformat))
				return Translatable.CreateTranslatable(ConverteerHelper.ToStringDynamic(obj, format));
			else
				return Translatable.CreateTranslatable(ConverteerHelper.ToStringDynamic(obj, format), ConverteerHelper.ToStringDynamic(obj, extraformat));
		}
		/// <summary>
		/// Utility functie die de NummerFormaat enum vertaald naar de overeenkomstige format string.
		/// </summary>
		public static ITranslatable ToText(decimal? d, NummerFormaat format) { return ToText(d, DECIMAL_FORMATS[format]); }

		/// <summary>
		/// Utility functie die de DatumFormaat enum vertaald naar de overeenkomstige format string.
		/// </summary>
		public static ITranslatable ToText(DateTime? dt, DatumFormaat format)
		{
			return ToText(dt, DATE_FORMATS[format].Item1, DATE_FORMATS[format].Item2);
		}


		#endregion

		#region Parse

		//Deze functie gebruiken om gebruikersinvoer in vrije textvelden te converteren naar de juiste types, eerste TryParse uitvoeren!
		public static object Parse(string s, Type t)
		{
			if (s == null) throw new ArgumentNullException("s");
			if (t == null) throw new ArgumentNullException("t");

			Type nonNullableType = t.IfNullableGetCoreType();
			bool canBeNull = nonNullableType != null || !t.IsValueType;
			Type fundamentalType = nonNullableType ?? t;

			if (canBeNull && s.Length == 0) return null; // dus s=="" levert null string op! --RW 24-04-2008 Moet wel zo, maar wat zijn de gevolgen?

			if (fundamentalType == typeof(int)) return Int32.Parse(s);
			if (fundamentalType == typeof(long)) return Int64.Parse(s);
			if (fundamentalType == typeof(decimal)) return ParseDecimal(s);		// Decimale punt ook ondersteunen - let wel, dus 100,000.0 != 100,000 zo
			if (fundamentalType == typeof(double)) return (double)ParseDecimal(s);//Emn: dit is  misschien niet de ideale oplossing, maar eventjes snel...
			if (fundamentalType == typeof(DateTime)) return ParseDateTime(s);
			if (fundamentalType == typeof(TimeSpan)) return TimeSpan.Parse(s, CultureNL);
			if (fundamentalType == typeof(string)) return s;
			if (fundamentalType == typeof(bool)) return Boolean.Parse(s);
			if (fundamentalType == typeof(XHtmlData)) return XHtmlData.Parse(s);
			throw new ConverteerException("Parse nog niet geimplementeerd voor " + t);
		}

		private const int YearMinimum = 1900, YearMaximum = 2100;

		static DateTime ParseDateTime(string s)
		{
			var retval = DateTime.Parse(DateStringCheck(s), CultureNL);
			if (retval.Year < YearMinimum || retval.Year >= YearMaximum) throw new ArgumentOutOfRangeException("Suspicious date " + retval + " not accepted as input.");
			return retval;
		}

		static decimal ParseDecimal(string s)
		{
			return s.LastIndexOf(',') < s.LastIndexOf('.') //when true, either only decimal point, or thousand sep commas with decimal point
								? Decimal.Parse(s, CultureInfo.InvariantCulture)
								: decimal.Parse(s, CultureNL);
		}


		// String naar datum conversie
		public static DateTime? ToDateTime(string s, DatumFormaat formaat) //TODO:alleen VerwInfo en MT940 worden gebruikt?
		{
			if (String.IsNullOrEmpty(s))
				return null;

			string datum = DateStringCheck(s);

			switch (formaat)
			{
				case DatumFormaat.ClieopDatum:
					if (s.Length == 6)
						datum = DateTime.Now.Year.ToStringInvariant().Substring(0, 2) +
								s.Substring(4, 2) + "-" +
								s.Substring(2, 2) + "-" +
								s.Substring(0, 2);
					break;
				case DatumFormaat.VerwInfoDatum:
				case DatumFormaat.MT940Datum:
					if (s.Length == 6)
						datum = DateTime.Now.Year.ToStringInvariant().Substring(0, 2) +
								s.Substring(0, 2) + "-" +
								s.Substring(2, 2) + "-" +
								s.Substring(4, 2);
					break;
				case DatumFormaat.SMDatum:
					if (s.Length == 8)
						datum = s.Substring(0, 4) + "-" +
								s.Substring(4, 2) + "-" +
								s.Substring(6, 2);
					break;
				case DatumFormaat.SMDatumTijd:
					if (s.Length == 14)
						datum = s.Substring(0, 4) + "-" +
							s.Substring(4, 2) + "-" +
							s.Substring(6, 2) + " " +
							s.Substring(8, 2) + ":" +
							s.Substring(10, 2) + ":" +
							s.Substring(12, 2);
					break;
			}

			if (TryParse(datum, typeof(DateTime)).IsOk())
				return DateTime.Parse(datum, CultureNL);
			return null;
		}

		public enum ParseState { OK, MALFORMED, OVERFLOW, GEENDATA, DATUMFOUT, TIJDFOUT }
		//Om gebruikersinvoer te controleren, daarna kan parse plaatsvinden
		public static ParseState TryParse(string s, Type t)
		{
			if (t == null) throw new ArgumentNullException("t");
			if (s == null) return ParseState.GEENDATA;

			Type nonNullableType = t.IfNullableGetCoreType();
			bool canBeNull = nonNullableType != null || !t.IsValueType;
			Type fundamentalType = nonNullableType ?? t;

			if (canBeNull && s.Length == 0) return ParseState.OK; // dus s=="" levert null string op!
			else if (fundamentalType == typeof(bool))
			{
				bool ignore;
				return Boolean.TryParse(s, out ignore) ? ParseState.OK : ParseState.MALFORMED;
			}
			else if (fundamentalType == typeof(DateTime))
			{
				DateTime parsedDate;
				bool canParse = DateTime.TryParse(DateStringCheck(s), CultureNL, DateTimeStyles.None, out parsedDate);
				return !canParse ? ParseState.DATUMFOUT : parsedDate.Year < YearMinimum || parsedDate.Year >= YearMaximum ? ParseState.OVERFLOW : ParseState.OK;
			}
			else if (fundamentalType == typeof(TimeSpan))
			{
				TimeSpan ignore;
				return TimeSpan.TryParse(s, CultureNL, out ignore) ? ParseState.OK : ParseState.TIJDFOUT;
			}
			else if (fundamentalType == typeof(long))
				// ReSharper disable ReturnValueOfPureMethodIsNotUsed
				try { Int64.Parse(s); return ParseState.OK; }
				// ReSharper restore ReturnValueOfPureMethodIsNotUsed
				catch (FormatException) { return ParseState.MALFORMED; }
				catch (OverflowException) { return ParseState.OVERFLOW; }
			else if (fundamentalType == typeof(int))
				// ReSharper disable ReturnValueOfPureMethodIsNotUsed
				try { Int32.Parse(s); return ParseState.OK; }
				// ReSharper restore ReturnValueOfPureMethodIsNotUsed
				catch (FormatException) { return ParseState.MALFORMED; }
				catch (OverflowException) { return ParseState.OVERFLOW; }
			else if (fundamentalType == typeof(decimal) || fundamentalType == typeof(double))
				try { ParseDecimal(s); return ParseState.OK; }
				catch (FormatException) { return ParseState.MALFORMED; }
				catch (OverflowException) { return ParseState.OVERFLOW; }
			else if (fundamentalType == typeof(string))
				return ParseState.OK;
			else if (fundamentalType == typeof(XHtmlData))
				return XhtmlCleaner.TryParse(s) != null ? ParseState.OK : ParseState.MALFORMED;
			throw new ConverteerException("TryParse nog niet geimplementeerd voor " + t);
		}
		public static ITranslatable ToDbCode(this ParseState state)
		{
			switch (state)
			{
				case ParseState.GEENDATA: return Texts.GenericEdit.Geendata;
				case ParseState.OK: return null;
				case ParseState.MALFORMED: return Texts.GenericEdit.Illegaledatainkolom;
				case ParseState.OVERFLOW: return Texts.GenericEdit.Overflow;
				case ParseState.DATUMFOUT: return Texts.GenericEdit.Foutdatumformaat;
				case ParseState.TIJDFOUT: return Texts.GenericEdit.Fouttijdformaat;
				default: throw new InvalidOperationException("Invalid parse state");
			}
		}
		public static bool IsOk(this ParseState state) { return state == ParseState.OK; }


		static string DateStringCheck(string datum)
		{
			// Controleer of de datum al dan geen delimiters bevat: 06-11-2007 vs 06112007
			// Als de delimiters ontbreken deze even zetten
			return Regex.IsMatch(datum, @"^\d{6,8}$")
					? datum.Substring(0, 2) + "-" +
					  datum.Substring(2, 2) + "-" +
					  datum.Substring(4, datum.Length - 4)
					: datum;
		}

		#endregion

		/// <summary>
		/// Handig om Nullables en Strings direct uit de datatable te halen waarbij de DBNull wordt vertaalt naar de null
		/// </summary>
		/// <param name="obj">Een object dat potentieel DBNull.Value mag zijn.</param>
		/// <returns>hetzelfde object of "null" mits het oorspronkelijke object DBNull of null was.</returns>
		public static object ToNullable(object obj) //TODO:Remove: DbNullRemover makes this obsolete
		{
			return obj == DBNull.Value ? null : obj;
		}
	}

}
