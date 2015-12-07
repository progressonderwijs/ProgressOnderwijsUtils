using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class ConverteerHelper
    {
        public const string GELD_EURO = "N02";

        static Func<Taal, string> TryToString<T>(object obj, Func<T, Func<Taal, string>> translator)
        {
            return obj is T ? translator((T)obj) : null;
        }

        [Pure]
        static IEnumerable<Func<Taal, string>> ResolveTranslator(object obj, string format)
        {
            yield return TryToString<ITranslatable>(
                obj,
                o => language =>
                    o.Translate(language).Text);
            yield return TryToString<string>(
                obj,
                o => language =>
                    o);
            yield return TryToString<int>(
                obj,
                o => language =>
                    o.ToString(format ?? "D", language.GetCulture()));
            yield return TryToString<Enum>(
                obj,
                o => language =>
                    EnumHelpers.MetaData(o)
                        .Label.Translate(language).Text);
            yield return TryToString<decimal>(
                obj,
                o => language =>
                    o.ToString(format ?? GELD_EURO, language.GetCulture()));
            yield return TryToString<DateTime>(
                obj,
                o => language =>
                    o.ToString(
                        format ?? (o == o.Date
                            ? Converteer.DateFormatStrings(DatumFormaat.AlleenDatum, language).Text
                            : Converteer.DateFormatStrings(DatumFormaat.DatumEnTijdInMinuten, language).Text),
                        language.GetCulture()));
            yield return TryToString<TimeSpan>(
                obj,
                o => language =>
                    o.ToString(format ?? "g", language.GetCulture()));
            yield return TryToString<bool>(
                obj,
                o => language => {
                    switch (language) {
                        case Taal.NL:
                            return o ? "Ja" : "Nee";
                        case Taal.EN:
                            return o ? "Yes" : "No";
                        case Taal.DU:
                            return o ? "Ja" : "Nein";
                        default:
                            throw new ArgumentOutOfRangeException(nameof(language), "Taal niet bekend: " + language);
                    }
                });
            yield return TryToString<char>(
                obj,
                o => language =>
                    new string(o, 1));
            yield return TryToString<XhtmlData>(
                obj,
                o => language =>
                    o.ToUiString());
            yield return TryToString<VariantData>(
                obj,
                o => language =>
                    o.ToString());
            yield return TryToString<FileData>(
                obj,
                o => language =>
                    o.ContainsFile ? $"{o.FileName} ({o.Content.Length / 1000m} KB)" : "");
            yield return TryToString<double>(
                obj,
                o => language =>
                    o.ToString(format ?? "0.##", language.GetCulture()));
            yield return TryToString<float>(
                obj,
                o => language =>
                    o.ToString(format ?? "0.##", language.GetCulture()));
            yield return TryToString<long>(
                obj,
                o => language =>
                    o.ToString(format ?? "D", language.GetCulture()));
            yield return TryToString<ushort>(
                obj,
                o => language =>
                    o.ToString(format ?? "D", language.GetCulture()));
            yield return TryToString<uint>(
                obj,
                o => language =>
                    o.ToString(format ?? "D", language.GetCulture()));
            yield return TryToString<short>(
                obj,
                o => language =>
                    o.ToString(format ?? "D", language.GetCulture()));
            yield return TryToString<sbyte>(
                obj,
                o => language =>
                    o.ToString(format ?? "D", language.GetCulture()));
            yield return TryToString<byte>(
                obj,
                o => language =>
                    o.ToString(format ?? "D", language.GetCulture()));
            yield return TryToString<IEnumerable>(
                obj,
                o =>
                    ArrayToStringHelper(o, format));
            yield return TryToString<SmartEnum>(
                obj,
                o => language =>
                    o.Text.Translate(language).Text);
        }

        [Pure]
        public static Func<Taal, string> ToStringDynamic(object obj, string format = null)
        {
            if (obj == null || obj == DBNull.Value) {
                return language => "";
            }

            foreach (var func in ResolveTranslator(obj, format)) {
                if (func != null) {
                    return func;
                }
            }

            throw new ConverteerException("unknown type " + obj.GetType() + " to stringify");
        }

        [Pure]
        static Func<Taal, string> ArrayToStringHelper(IEnumerable o, string format)
        {
            var subtrans = o.Cast<object>().Select(
                elem =>
                    ToStringDynamic(elem, format)).ToArray();

            return language => subtrans.Select(f => f(language)).JoinStrings("\n");
        }
    }
}
