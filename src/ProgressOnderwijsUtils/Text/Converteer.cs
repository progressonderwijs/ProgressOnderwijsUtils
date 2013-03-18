using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
			{ DatumFormaat.DatumEnTijdInMilliseconden, Tuple.Create("dd-MM-yyyy HH:mm:ss.fff", (string)null) },
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
		static readonly DateTime dtWithManyDigits = new DateTime(2000, 1, 1) - TimeSpan.FromDays(1) - TimeSpan.FromHours(1) - TimeSpan.FromMinutes(1) - TimeSpan.FromSeconds(1) - TimeSpan.FromMilliseconds(1) - TimeSpan.FromTicks(1);
		static readonly Dictionary<DatumFormaat, int> formaatLengte = DATE_FORMATS.Keys.ToDictionary(k => k, k => new[] { Taal.NL, Taal.EN, Taal.DU }.Select(t => dtWithManyDigits.ToString(DATE_FORMATS[k].Item1, Translator.GetCulture(t)).Length).Max());

		public static int DateTimeStringLengthForFormat(DatumFormaat formaat) { return formaatLengte[formaat]; }

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
		public static string ToString(object obj, string format, Taal language)
		{
			return ConverteerHelper.ToStringDynamic(obj, format)(language);
		}
		public static string ToString(object obj) { return ToString(obj, null, Taal.NL); }
		public static string ToString(object obj, Taal language) { return ToString(obj, null, language); }

		/// <summary>
		/// Utility functie die de DatumFormaat enum vertaald naar de overeenkomstige format string.
		/// </summary>
		public static string ToString(DateTime? dt, DatumFormaat format, Taal language = Taal.NL)
		{
			return ConverteerHelper.ToStringDynamic(dt, DATE_FORMATS[format].Item1)(language);
		}

		#endregion

		#region ToText

		static readonly MethodInfo enumToTranslatableGeneric = ((Func<DatabaseVersion, ITranslatable>)EnumHelpers.GetLabel).Method.GetGenericMethodDefinition();
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
			else if (obj is Enum)
				return TranslateEnum((Enum)obj);
			else if (string.IsNullOrEmpty(extraformat))
				return Translatable.CreateTranslatable(ConverteerHelper.ToStringDynamic(obj, format));
			else
				return Translatable.CreateTranslatable(ConverteerHelper.ToStringDynamic(obj, format), ConverteerHelper.ToStringDynamic(obj, extraformat));
		}

		public static ITranslatable TranslateEnum(Enum obj) { return (ITranslatable)enumToTranslatableGeneric.MakeGenericMethod(obj.GetType()).Invoke(null, new object[] { obj }); }

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
		public static object Parse(string s, Type t, Taal taal)
		{
			return TryParse(s, t, taal).Value;
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
								? decimal.Parse(s, CultureInfo.InvariantCulture)
								: decimal.Parse(s, CultureNL);
		}


		// string naar datum conversie
		public static DateTime? ToDateTime(string s, DatumFormaat formaat, Taal taal = Taal.NL) //TODO:alleen VerwInfo en MT940 worden gebruikt?
		{
			if (string.IsNullOrEmpty(s))
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

			ParseResult parseResult = TryParse(datum, typeof(DateTime), taal);
			if (parseResult.IsOk)
				return (DateTime)parseResult.Value;
			return null;
		}

		public struct ParseResult
		{
			static readonly object Nonsense = new object();
			public readonly ParseState State;
			readonly object value;

			public bool IsOk { get { return State == ParseState.Ok; } }
			public object Value
			{
				get
				{
					if (!IsOk)
					{
						if (State == ParseState.Geendata)
							throw new ArgumentNullException("Parse mislukt met status " + State);
						if (State == ParseState.Malformed || State == ParseState.Datumfout)
							throw new FormatException("Parse mislukt met status " + State);
						if (State == ParseState.Overflow)
							throw new OverflowException("Parse mislukt met status " + State);
						throw new InvalidOperationException("Parse mislukt met status " + State);
					}
					return value;
				}
			}

			public ITranslatable ErrorMessage { get { if (IsOk)return null; else return (ITranslatable)value; } }

			ParseResult(ParseState state, object value)
			{
				State = state;
				this.value = value;
			}
			public static ParseResult Ok(object value) { return new ParseResult(ParseState.Ok, value); }
			public static ParseResult CreateError(ParseState state, ITranslatable error)
			{
				if (state == ParseState.Ok)
					throw new InvalidOperationException("Cannot set error: OK is not an error");
				return new ParseResult(state, error);
			}
			public static ParseResult Malformed(Type type, string s) { return CreateError(ParseState.Malformed, Texts.GenericEdit.MalformedData(Texts.ClrTypeNames.UserReadable(type, false)).Append(Translatable.Literal(" niet ", " not ", " nicht "), Converteer.ToText(" niet \"" + s + "\"."))); }
			public static ParseResult Overflow { get { return CreateError(ParseState.Overflow, Texts.GenericEdit.Overflow); } }
			public static ParseResult Geendata { get { return CreateError(ParseState.Geendata, Texts.GenericEdit.GeenData); } }
			public static ParseResult Datumfout { get { return CreateError(ParseState.Datumfout, Texts.GenericEdit.FoutDatumFormaat); } }
			public static ParseResult TijdFout { get { return CreateError(ParseState.TijdFout, Texts.GenericEdit.FoutTijdFormaat); } }
		}



		public enum ParseState { Undefined, Ok, Malformed, Overflow, Geendata, Datumfout, TijdFout }
		//Om gebruikersinvoer te controleren, daarna kan parse plaatsvinden
		public static ParseResult TryParse(string s, Type t, Taal taal)
		{
			if (t == null) throw new ArgumentNullException("t");
			if (s == null) return ParseResult.Geendata;

			Type nonNullableType = t.IfNullableGetNonNullableType();
			bool canBeNull = !t.IsArray && (nonNullableType != null || !t.IsValueType);
			Type fundamentalType = nonNullableType ?? t;

			if (canBeNull && s.Length == 0) return ParseResult.Ok(null); // dus s=="" levert null string op! - en dit vangt ook lege arrays, maar dat is ok; bij het parsen worden die niet-null
			else if (fundamentalType == typeof(bool))
			{
				bool value;
				return Boolean.TryParse(s, out value) ? ParseResult.Ok(value) : ParseResult.Malformed(t, s);
			}
			else if (fundamentalType == typeof(DateTime))
			{
				DateTime parsedDate;
				bool canParse = DateTime.TryParse(DateStringCheck(s), CultureNL, DateTimeStyles.None, out parsedDate);
				return !canParse ? ParseResult.Datumfout
					: parsedDate.Year < YearMinimum || parsedDate.Year >= YearMaximum ? ParseResult.Overflow
					: ParseResult.Ok(parsedDate);
			}
			else if (fundamentalType == typeof(TimeSpan))
			{
				TimeSpan result;
				return TimeSpan.TryParse(s, CultureNL, out result) ? ParseResult.Ok(result) : ParseResult.TijdFout;
			}
			else if (fundamentalType == typeof(long))
				try { return ParseResult.Ok(long.Parse(s)); }
				catch (FormatException) { return ParseResult.Malformed(t, s); }
				catch (OverflowException) { return ParseResult.Overflow; }
			else if (fundamentalType == typeof(int))
				try { return ParseResult.Ok(int.Parse(s)); }
				catch (FormatException) { return ParseResult.Malformed(t, s); }
				catch (OverflowException) { return ParseResult.Overflow; }
			else if (fundamentalType == typeof(decimal))
				try { return ParseResult.Ok(ParseDecimal(s)); }
				catch (FormatException) { return ParseResult.Malformed(t, s); }
				catch (OverflowException) { return ParseResult.Overflow; }
			else if (fundamentalType.IsEnum)
			{
				var options = EnumHelpers.TryParseLabel(fundamentalType, s, taal).ToArray();
				return options.Length == 0 ? ParseResult.Malformed(t, s) :
					options.Length == 1 ? ParseResult.Ok(options[0]) :
					ParseResult.CreateError(ParseState.Malformed, Translatable.Literal("Kan waarde '", "Value '").AppendAuto(s).Append(Translatable.Literal("' niet uniek interpreteren!", "' cannot be interpreted unambiguously!")));
			}
			/*catch (OverflowException) { return ParseResult.Overflow; }*/
			else if (fundamentalType == typeof(double))
				try { return ParseResult.Ok((double)ParseDecimal(s)); }
				catch (FormatException) { return ParseResult.Malformed(t, s); }
				catch (OverflowException) { return ParseResult.Overflow; }
			else if (fundamentalType == typeof(string))
				return ParseResult.Ok(s);
			else if (fundamentalType == typeof(XhtmlData))
			{
				var res = XhtmlData.TryParseAndSanitize(s);
				return res.HasValue ? ParseResult.Ok(res.Value) : ParseResult.Malformed(t, s);
			}
			else if (fundamentalType.IsArray)
			{
				var elementType = fundamentalType.GetElementType();
				if (elementType.IsArray) throw new InvalidOperationException("Cannot parse jagged arrays");
				if (fundamentalType.GetArrayRank() != 1) throw new InvalidOperationException("Can only parse arrays of rank 1");
				var components = s.Split(WHITESPACE, StringSplitOptions.RemoveEmptyEntries);
				var retval = Array.CreateInstance(elementType, components.Length);
				for (int i = 0; i < components.Length; i++)
				{
					var elementParseResult = TryParse(components[i], elementType, taal);
					if (elementParseResult.IsOk)
						retval.SetValue(elementParseResult.Value, i);
					else
						return elementParseResult;
				}
				return ParseResult.Ok(retval); ;
			}
			throw new ConverteerException("TryParse nog niet geimplementeerd voor " + t);
		}
		static readonly char[] WHITESPACE = new[] { ' ', '\r', '\n' };

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
	}

}
