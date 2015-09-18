using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

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
        static readonly SortedList<DatumFormaat, TextVal> DATE_FORMATS = new SortedList<DatumFormaat, TextVal> {
            { DatumFormaat.AlleenDatum, new TextVal("dd-MM-yyyy", null) },
            { DatumFormaat.AlleenTijd, new TextVal("HH:mm", null) },
            { DatumFormaat.DatumEnTijdInMinuten, new TextVal("dd-MM-yyyy HH:mm", null) },
            { DatumFormaat.DatumEnTijdInSeconden, new TextVal("dd-MM-yyyy HH:mm:ss", null) },
            { DatumFormaat.DatumEnTijdInMilliseconden, new TextVal("dd-MM-yyyy HH:mm:ss.fff", null) },
            { DatumFormaat.DatumToolTipTijd, new TextVal("dd-MM-yyyy", "dd-MM-yyyy HH:mm:ss.fff") },
            { DatumFormaat.JaarToolTipDatum, new TextVal("yyyy", "dd-MM-yyyy") },
            { DatumFormaat.SMDatum, new TextVal("yyyyMMdd", null) },
            { DatumFormaat.SMDatumTijd, new TextVal("yyyyMMddHHmmss", null) },
            { DatumFormaat.ClieopDatum, new TextVal("ddMMyy", null) },
            { DatumFormaat.MT940Datum, new TextVal("yyMMdd", null) },
            { DatumFormaat.DatumZonderJaar, new TextVal("dd-MM", "dd-MM-yyyy") },
            { DatumFormaat.ISODate, new TextVal("yyyy-MM-dd", null) },
            { DatumFormaat.ISODateTime, new TextVal("yyyy-MM-ddTHH:mm:ss", null) },
        };

        static readonly DateTime DT_WITH_MANY_DIGITS = new DateTime(2000, 1, 1) - TimeSpan.FromDays(1) - TimeSpan.FromHours(1) - TimeSpan.FromMinutes(1)
            - TimeSpan.FromSeconds(1) - TimeSpan.FromMilliseconds(1) - TimeSpan.FromTicks(1);

        static readonly SortedList<DatumFormaat, int> FORMAAT_LENGTE;

        static Converteer()
        {
            var talen = EnumHelpers.GetValues<Taal>().Where(t => t != Taal.None).ToArray();
            FORMAAT_LENGTE = DATE_FORMATS.Keys.ToSortedList(
                k => k,
                k => talen.Max(
                    taal => {
                        var formatString = DateFormatStrings(k, taal).Text;
                        return DT_WITH_MANY_DIGITS.ToString(formatString, taal.GetCulture()).Length;
                    }));
        }

        [Pure]
        public static TextVal DateFormatStrings(DatumFormaat formaat, Taal taal = Taal.NL)
        {
            if (taal == Taal.EN) {
                if (formaat == DatumFormaat.AlleenDatum) {
                    return new TextVal("dd/MM/yyyy", null);
                }
                if (formaat == DatumFormaat.AlleenTijd) {
                    return new TextVal("HH:mm", null);
                }
                if (formaat == DatumFormaat.DatumEnTijdInMinuten) {
                    return new TextVal("dd/MM/yyyy HH:mm", null);
                }
                if (formaat == DatumFormaat.DatumEnTijdInSeconden) {
                    return new TextVal("dd/MM/yyyy HH:mm:ss", null);
                }
                if (formaat == DatumFormaat.DatumEnTijdInMilliseconden) {
                    return new TextVal("dd/MM/yyyy HH:mm:ss.fff", null);
                }
                if (formaat == DatumFormaat.DatumToolTipTijd) {
                    return new TextVal("dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss.fff");
                }
                if (formaat == DatumFormaat.JaarToolTipDatum) {
                    return new TextVal("yyyy", "dd/MM/yyyy");
                }
                if (formaat == DatumFormaat.DatumZonderJaar) {
                    return new TextVal("dd/MM", "dd/MM/yyyy");
                }
            } else if (taal == Taal.DU) {
                if (formaat == DatumFormaat.AlleenDatum) {
                    return new TextVal("dd.MM.yyyy", null);
                }
                if (formaat == DatumFormaat.AlleenTijd) {
                    return new TextVal("HH:mm", null);
                }
                if (formaat == DatumFormaat.DatumEnTijdInMinuten) {
                    return new TextVal("dd.MM.yyyy HH:mm", null);
                }
                if (formaat == DatumFormaat.DatumEnTijdInSeconden) {
                    return new TextVal("dd.MM.yyyy HH:mm:ss", null);
                }
                if (formaat == DatumFormaat.DatumEnTijdInMilliseconden) {
                    return new TextVal("dd.MM.yyyy HH:mm:ss.fff", null);
                }
                if (formaat == DatumFormaat.DatumToolTipTijd) {
                    return new TextVal("dd.MM.yyyy", "dd.MM.yyyy HH:mm:ss.fff");
                }
                if (formaat == DatumFormaat.JaarToolTipDatum) {
                    return new TextVal("yyyy", "dd.MM.yyyy");
                }
                if (formaat == DatumFormaat.DatumZonderJaar) {
                    return new TextVal("dd.MM", "dd.MM.yyyy");
                }
            }
            //fallback:
            return DATE_FORMATS[formaat];
        }

        [Pure]
        public static int DateTimeStringLengthForFormat(DatumFormaat formaat) => FORMAAT_LENGTE[formaat];

        /// <summary>
        /// Generieke string-efy functie die de default Progress.NET formatering en culture-info gebruikt.
        /// </summary>
        /// <param name="obj">De waarde te string-efy</param>
        /// <param name="format">Optionele formatering string om de default Progress.NET formatering te overrulen 
        /// 	(zie .Net documentatie voor details)</param>
        /// <param name="language">De taal die de te gebruiken culture aangeeft</param>
        /// <returns>De geformateerde string die de waarde representeerd</returns>
        [Pure]
        public static string ToString(object obj, string format, Taal language) => ConverteerHelper.ToStringDynamic(obj, format)(language);

        [Pure]
        public static string ToString(object obj) => ToString(obj, null, Taal.NL);

        [Pure]
        public static string ToString(object obj, Taal language) => ToString(obj, null, language);

        /// <summary>
        /// Utility functie die de DatumFormaat enum vertaald naar de overeenkomstige format string.
        /// </summary>
        [Pure]
        public static string ToString(DateTime? dt, DatumFormaat format, Taal language = Taal.NL)
            => ConverteerHelper.ToStringDynamic(dt, DateFormatStrings(format, language).Text)(language);

        /// <summary>
        /// Generieke text-efy functie die de default Progress.NET formatering en culture-info gebruikt om een waarde een vertaalbare string representatie te geven.
        /// </summary>
        /// <param name="obj">De waarde te text-efy</param>
        /// <param name="format">Optionele formatering string om de default Progress.NET formatering voor het tekst gedeelte te overrulen 
        /// (zie de .NET documentatie voor formatstring details)</param>
        /// <param name="extraformat">Extra formatering string om de default Progress.NET formatering voor het tooltip gedeelte te overrulen 
        /// (zie de .NET documentatie voor formatstring details)</param>
        /// <returns>De geformateerde translatable die de waarde representeerd in de Nederlands en Engelse taal.</returns>
        [Pure]
        public static ITranslatable ToText(object obj, string format = null, string extraformat = null)
        {
            if (obj == null || obj == DBNull.Value) {
                return Translatable.Empty;
            } else if (obj is ITranslatable) {
                return (ITranslatable)obj;
            } else if (obj is TextVal) {
                return Translatable.Raw((TextVal)obj);
            } else if (obj is Enum) {
                return EnumHelpers.GetLabel((Enum)obj); // TranslateEnum((Enum)obj);
            } else if (string.IsNullOrEmpty(extraformat)) {
                return Translatable.CreateTranslatable(ConverteerHelper.ToStringDynamic(obj, format));
            } else {
                return Translatable.CreateTranslatable(ConverteerHelper.ToStringDynamic(obj, format), ConverteerHelper.ToStringDynamic(obj, extraformat));
            }
        }

        /// <summary>
        /// Utility functie die de NummerFormaat enum vertaald naar de overeenkomstige format string.
        /// </summary>
        [Pure]
        public static ITranslatable ToText(decimal? d, NummerFormaat format)
        {
            string formatString;
            if (format == NummerFormaat.GeldEuro) {
                formatString = ConverteerHelper.GELD_EURO;
            } else if (format == NummerFormaat.Afgerond) {
                formatString = "#";
            } else if (format == NummerFormaat.AfgerondOp1Decimaal) {
                formatString = "#.#";
            } else if (format == NummerFormaat.AfgerondOp2Decimaal) {
                formatString = "#.##";
            } else if (format == NummerFormaat.GeldEuroGrootboek) {
                formatString = "#,##0.00 D;#,##0.00 C;0.00  ";
            } else {
                throw new ArgumentException("Unknown format " + format);
            }

            return ToText(d, formatString);
        }

        [Pure]
        public static LiteralTranslatable ToCheckmarkOrEmpty(bool value) => value ? Translatable.Literal("✔") : Translatable.Empty;

        /// <summary>
        /// Utility functie die de DatumFormaat enum vertaald naar de overeenkomstige format string.
        /// </summary>
        [Pure]
        public static ITranslatable ToText(DateTime? dt, DatumFormaat format)
        {
            if (string.IsNullOrEmpty(DATE_FORMATS[format].ExtraText)) {
                return Translatable.CreateTranslatable(taal => ConverteerHelper.ToStringDynamic(dt, DateFormatStrings(format, taal).Text)(taal));
            } else {
                return Translatable.CreateTranslatable(
                    taal => ConverteerHelper.ToStringDynamic(dt, DateFormatStrings(format, taal).Text)(taal),
                    taal => ConverteerHelper.ToStringDynamic(dt, DateFormatStrings(format, taal).ExtraText)(taal));
            }
        }

        //Deze functie gebruiken om gebruikersinvoer in vrije textvelden te converteren naar de juiste types, eerste TryParse uitvoeren!
        [Pure]
        public static object Parse(string s, Type t, Taal taal) => TryParse(s, t, taal).GetValue();

        [Pure]
        public static T Parse<T>(string s, Taal taal)
        {
            return (T)TryParse(s, typeof(T), taal).GetValue();
        }

        const int YearMinimum = 1900, YearMaximum = 2100;

        [Pure]
        static decimal ParseDecimal(string s)
        {
            return s.LastIndexOf(',') < s.LastIndexOf('.') //when true, either only decimal point, or thousand sep commas with decimal point
                ? decimal.Parse(s, CultureInfo.InvariantCulture)
                : decimal.Parse(s, Taal.NL.GetCulture());
        }

        // string naar datum conversie
        [Pure]
        public static DateTime? ToDateTime(string s, DatumFormaat formaat, Taal taal = Taal.NL) //TODO:alleen VerwInfo en MT940 worden gebruikt?
        {
            if (string.IsNullOrEmpty(s)) {
                return null;
            }

            var datum = DateStringCheck(s);

            switch (formaat) {
                case DatumFormaat.ClieopDatum:
                    if (s.Length == 6) {
                        datum = DateTime.Now.Year.ToStringInvariant().Substring(0, 2) +
                            s.Substring(4, 2) + "-" +
                            s.Substring(2, 2) + "-" +
                            s.Substring(0, 2);
                    }
                    break;
                case DatumFormaat.MT940Datum:
                    if (s.Length == 6) {
                        datum = DateTime.Now.Year.ToStringInvariant().Substring(0, 2) +
                            s.Substring(0, 2) + "-" +
                            s.Substring(2, 2) + "-" +
                            s.Substring(4, 2);
                    }
                    break;
                case DatumFormaat.SMDatum:
                    if (s.Length == 8) {
                        datum = s.Substring(0, 4) + "-" +
                            s.Substring(4, 2) + "-" +
                            s.Substring(6, 2);
                    }
                    break;
                case DatumFormaat.SMDatumTijd:
                    if (s.Length == 14) {
                        datum = s.Substring(0, 4) + "-" +
                            s.Substring(4, 2) + "-" +
                            s.Substring(6, 2) + " " +
                            s.Substring(8, 2) + ":" +
                            s.Substring(10, 2) + ":" +
                            s.Substring(12, 2);
                    }
                    break;
            }

            var parseResult = TryParse(datum, typeof(DateTime), taal);
            if (parseResult.IsOk) {
                return (DateTime)parseResult.GetValue();
            }
            return null;
        }

        public struct ParseResult
        {
            public readonly ParseState State;
            readonly object value;
            public bool IsOk => State == ParseState.Ok;

            /// <summary>
            /// Gets the value as parsed by parse if the parse was successful.  Throws an exception if used in a non-OK state.
            /// </summary>
            public object GetValue()
            {
                if (!IsOk) {
                    if (State == ParseState.Geendata) {
                        throw new ArgumentNullException("Parse mislukt met status " + State);
                    }
                    if (State == ParseState.Malformed || State == ParseState.Datumfout) {
                        throw new FormatException("Parse mislukt met status " + State);
                    }
                    if (State == ParseState.Overflow) {
                        throw new OverflowException("Parse mislukt met status " + State);
                    }
                    throw new InvalidOperationException("Parse mislukt met status " + State);
                }
                return value;
            }

            public ITranslatable ErrorMessage
            {
                get
                {
                    if (IsOk) {
                        return null;
                    } else {
                        return (ITranslatable)value;
                    }
                }
            }

            ParseResult(ParseState state, object value)
            {
                State = state;
                this.value = value;
            }

            [Pure]
            public static ParseResult Ok(object value) => new ParseResult(ParseState.Ok, value);

            [Pure]
            public static ParseResult CreateError(ParseState state, ITranslatable error)
            {
                if (state == ParseState.Ok) {
                    throw new InvalidOperationException("Cannot set error: OK is not an error");
                }
                return new ParseResult(state, error);
            }

            [Pure]
            public static ParseResult Malformed(Type type, string s)
            {
                return CreateError(
                    ParseState.Malformed,
                    GenericEditText.MalformedData(ClrTypeNamesText.UserReadable(type, false))
                        .Append(Translatable.Literal(" niet ", " not ", " nicht "), ToText("\"" + s + "\".")));
            }

            public static ParseResult Overflow => CreateError(ParseState.Overflow, GenericEditText.Overflow);
            public static ParseResult Geendata => CreateError(ParseState.Geendata, GenericEditText.GeenData);
            public static ParseResult Datumfout => CreateError(ParseState.Datumfout, GenericEditText.FoutDatumFormaat);
            public static ParseResult TijdFout => CreateError(ParseState.TijdFout, GenericEditText.FoutTijdFormaat);
        }

        public enum ParseState
        {
            // ReSharper disable UnusedMember.Global
            Undefined,
            Ok,
            Malformed,
            Overflow,
            Geendata,
            Datumfout,
            TijdFout
            // ReSharper restore UnusedMember.Global
        }

        //Om gebruikersinvoer te controleren, daarna kan parse plaatsvinden
        [Pure]
        public static ParseResult TryParse(string s, Type t, Taal taal)
        {
            if (t == null) {
                throw new ArgumentNullException(nameof(t));
            }
            if (s == null) {
                return ParseResult.Geendata;
            }

            var nonNullableType = t.IfNullableGetNonNullableType();
            var canBeNull = !t.IsArray && (nonNullableType != null || !t.IsValueType);
            var fundamentalType = nonNullableType ?? t;

            if (canBeNull && s.Length == 0) {
                return ParseResult.Ok(null); // dus s=="" levert null string op! - en dit vangt ook lege arrays, maar dat is ok; bij het parsen worden die niet-null
            } else if (fundamentalType == typeof(bool)) {
                bool value;
                return bool.TryParse(s, out value) ? ParseResult.Ok(value) : ParseResult.Malformed(t, s);
            } else if (fundamentalType == typeof(DateTime)) {
                DateTime parsedDate;
                var canParse = DateTime.TryParse(DateStringCheck(s), Taal.NL.GetCulture(), DateTimeStyles.None, out parsedDate);
                return !canParse
                    ? ParseResult.Datumfout
                    : parsedDate.Year < YearMinimum || parsedDate.Year >= YearMaximum
                        ? ParseResult.Overflow
                        : ParseResult.Ok(parsedDate);
            } else if (fundamentalType == typeof(TimeSpan)) {
                TimeSpan result;
                return TimeSpan.TryParse(s, Taal.NL.GetCulture(), out result) ? ParseResult.Ok(result) : ParseResult.TijdFout;
            } else if (fundamentalType == typeof(long)) {
                try {
                    return ParseResult.Ok(long.Parse(s));
                } catch (FormatException) {
                    return ParseResult.Malformed(t, s);
                } catch (OverflowException) {
                    return ParseResult.Overflow;
                }
            } else if (fundamentalType == typeof(int)) {
                try {
                    return ParseResult.Ok(int.Parse(s));
                } catch (FormatException) {
                    return ParseResult.Malformed(t, s);
                } catch (OverflowException) {
                    return ParseResult.Overflow;
                }
            } else if (fundamentalType == typeof(decimal)) {
                try {
                    return ParseResult.Ok(ParseDecimal(s));
                } catch (FormatException) {
                    return ParseResult.Malformed(t, s);
                } catch (OverflowException) {
                    return ParseResult.Overflow;
                }
            } else if (fundamentalType.IsEnum) {
                var options = EnumHelpers.TryParseLabel(fundamentalType, s, taal).ToArray();
                return options.Length == 0
                    ? ParseResult.Malformed(t, s)
                    : options.Length == 1
                        ? ParseResult.Ok(options[0])
                        : ParseResult.CreateError(
                            ParseState.Malformed,
                            Translatable.Literal("Kan waarde '", "Value '")
                                .AppendAuto(s)
                                .Append(Translatable.Literal("' niet uniek interpreteren!", "' cannot be interpreted unambiguously!")));
            }
            /*catch (OverflowException) { return ParseResult.Overflow; }*/
            else if (fundamentalType == typeof(double)) {
                try {
                    return ParseResult.Ok((double)ParseDecimal(s));
                } catch (FormatException) {
                    return ParseResult.Malformed(t, s);
                } catch (OverflowException) {
                    return ParseResult.Overflow;
                }
            } else if (fundamentalType == typeof(string)) {
                return ParseResult.Ok(s);
            } else if (fundamentalType == typeof(XhtmlData)) {
                var res = XhtmlData.TryParseAndSanitize(s);
                return res.HasValue ? ParseResult.Ok(res.Value) : ParseResult.Malformed(t, s);
            } else if (fundamentalType.IsArray) {
                var elementType = fundamentalType.GetElementType();
                if (elementType.IsArray) {
                    throw new InvalidOperationException("Cannot parse jagged arrays");
                }
                if (fundamentalType.GetArrayRank() != 1) {
                    throw new InvalidOperationException("Can only parse arrays of rank 1");
                }
                var components = s.Split(WHITESPACE, StringSplitOptions.RemoveEmptyEntries);
                var retval = Array.CreateInstance(elementType, components.Length);
                for (var i = 0; i < components.Length; i++) {
                    var elementParseResult = TryParse(components[i], elementType, taal);
                    if (elementParseResult.IsOk) {
                        retval.SetValue(elementParseResult.GetValue(), i);
                    } else {
                        return elementParseResult;
                    }
                }
                return ParseResult.Ok(retval);
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
    }
}
