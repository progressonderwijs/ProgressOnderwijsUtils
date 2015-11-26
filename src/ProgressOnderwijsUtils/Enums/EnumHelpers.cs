using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
    public static class EnumHelpers
    {
        static readonly ConcurrentDictionary<Type, IEnumMetaDataCache> enumMetaCache = new ConcurrentDictionary<Type, IEnumMetaDataCache>();

        static IEnumMetaDataCache GetEnumMetaCache(Type enumType)
        {
            return enumMetaCache.GetOrAdd(enumType, type => {
                var specializedType = typeof(EnumMetaDataCache<>).MakeGenericType(type);
                var instanceField = specializedType.GetField(nameof(EnumMetaDataCache<DayOfWeek>.Instance), BindingFlags.Static | BindingFlags.Public);

                return (IEnumMetaDataCache)instanceField.GetValue(null);
            });
        }

        public static class GetAttrs<TAttr>
            where TAttr : Attribute
        {
            public static IEnumerable<TAttr> On<T>(T enumVal) where T : struct, IConvertible, IComparable
            {
                return EnumMetaDataCache<T>.Instance.AllAttributes(enumVal).OfType<TAttr>();
                //return EnumMetaCache<T>.AttrCache<TAttr>.EnumMemberAttributes[enumVal];
            }

            public static IEnumerable<T> From<T>(Func<TAttr, bool> pred) where T : struct, IConvertible, IComparable
            {
                return EnumMetaDataCache<T>.Instance
                    .AllValuesWithMetaData()
                    .Where(v => v.Attributes<TAttr>().Any(pred))
                    .Select(v => v.EnumValue);
            }
        }

        public static IEnumerable<Attribute> GetAttributes<TEnum>(TEnum enumValue)
            where TEnum : struct, IConvertible, IComparable
        {
            return MetaData(enumValue).Attributes<Attribute>();
        }

        public static IReadOnlyList<T> GetValues<T>() where T : struct, IConvertible, IComparable
        {
            return EnumMetaDataCache<T>.Instance.AllValues();
        }

        public static IReadOnlyList<IEnumMetaData> GetValuesWithMetaData(Type enumType) => GetEnumMetaCache(enumType).AllUntypedValuesWithMetaData();

        public static Func<TEnum, TEnum, TEnum> AddFlagsFunc<TEnum>() where TEnum : struct, IConvertible, IComparable
        {
            return EnumMetaDataCache<TEnum>.Instance.AddFlag;
        }

        public static Func<TEnum, TEnum, bool> HasFlagsFunc<TEnum>() where TEnum : struct, IConvertible, IComparable
        {
            return EnumMetaDataCache<TEnum>.Instance.HasFlag;
        }

        public static ITranslatable GetLabel<TEnum>(TEnum enumVal)
            where TEnum : struct, IConvertible, IComparable
            => MetaData(enumVal).Label;

        public static IEnumMetaData MetaData(Enum enumVal)
        {
            return GetEnumMetaCache(enumVal.GetType())
                .UntypedMetaData(enumVal);
        }

        public static IEnumMetaData MetaData<TEnum>(TEnum enumValue)
            where TEnum : struct, IConvertible, IComparable
        {
            return EnumMetaDataCache<TEnum>.Instance.MetaData(enumValue);
        }

        public static SelectItem<TEnum> GetSelectItem<TEnum>(TEnum f)
            where TEnum : struct, IConvertible, IComparable
        {
            return SelectItem.Create(f, GetLabel(f));
        }

        public static IReadOnlyList<SelectItem<TEnum>> CreateSelectItemList<TEnum>(this IEnumerable<TEnum> values)
            where TEnum : struct, IConvertible, IComparable
        {
            return values.Select(GetSelectItem).ToArray();
        }

        public static DataTable ToIntKoppelTabel<TEnum>(IEnumerable<TEnum> values, Taal taal)
            where TEnum : struct, IConvertible, IComparable
        {
            //TODO:EMN:improve this API.
            return values.CreateSelectItemList()
                .Select(v => new KoppelTabelEntry { Id = v.Value.ToInt32(null), Tekst = v.Label.Translate(taal).Text }
                ).ToDataTable();
        }

        public static DataTable ToIntKoppelTabel_OrderedByText<TEnum>(IEnumerable<TEnum> values, Taal taal)
            where TEnum : struct, IConvertible, IComparable
        {
            return values.Select(
                v =>
                    new KoppelTabelEntry { Id = v.ToInt32(null), Tekst = GetLabel(v).Translate(taal).Text }
                )
                .OrderBy(entry => entry.Tekst)
                .ToDataTable();
        }

        public static DataTable ToIntKoppelTabelExpandedText<TEnum>(IEnumerable<TEnum> values, Taal taal)
            where TEnum : struct, IConvertible, IComparable
        {
            return values.Select(
                v => {
                    var tv = GetLabel(v).Translate(taal);
                    return new KoppelTabelEntry { Id = v.ToInt32(null), Tekst = tv.Text + ": " + tv.ExtraText };
                }).ToDataTable();
        }

        public static SelectItem<TEnum?> GetSelectItem<TEnum>(TEnum? f)
            where TEnum : struct, IConvertible, IComparable
        {
            return SelectItem.Create(f, f == null ? Translatable.Empty : GetLabel(f.Value));
        }

        public static IReadOnlyList<SelectItem<int?>> ToIntSelectItemList<TEnum>(
            this IEnumerable<SelectItem<TEnum?>> enumSelectItemList)
            where TEnum : struct, IConvertible
        {
            return enumSelectItemList.Select(item => SelectItem.Create(item.Value.HasValue ? item.Value.Value.ToInt32(null) : default(int?), item.Label))
                .ToArray();
        }

        public static IReadOnlyList<SelectItem<int?>> ToIntSelectItemList<TEnum>(
            this IEnumerable<SelectItem<TEnum>> enumSelectItemList)
            where TEnum : struct, IConvertible
        {
            return enumSelectItemList.Select(item => SelectItem.Create((int?)item.Value.ToInt32(null), item.Label))
                .ToArray();
        }

        public static TEnum? TryParse<TEnum>(string s) where TEnum : struct, IConvertible
        {
            if (!typeof(TEnum).IsEnum) {
                throw new ArgumentException("type must be an enum, not " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(TEnum)));
            }

            TEnum retval;
            return Enum.TryParse(s, true, out retval) ? retval : default(TEnum?);
        }

        public static TEnum ParseCaseSensitively<TEnum>(string s) where TEnum : struct, IConvertible
        {
            var parsed = TryParseCaseSensitively<TEnum>(s);

            if (parsed.HasValue) {
                return parsed.Value;
            } else {
                throw new ArgumentException("Could not parse string as " + typeof(TEnum).Name);
            }
        }

        public static TEnum? TryParseCaseSensitively<TEnum>(string s) where TEnum : struct, IConvertible
        {
            if (!typeof(TEnum).IsEnum) {
                throw new ArgumentException("type must be an enum, not " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(TEnum)));
            }

            TEnum retval;

            if (Enum.TryParse(s, false, out retval)) {
                return retval;
            } else {
                return null;
            }
        }

        [CodeDieAlleenWordtGebruiktInTests]
        public static IEnumerable<TEnum> TryParseLabel<TEnum>(string s, Taal taal) where TEnum : struct, IConvertible, IComparable
        {
            return EnumLabelParser<TEnum>.Lookup(s, taal);
        }

        public static IEnumerable<Enum> TryParseLabel(Type enumType, string s, Taal taal)
        {
            if (!enumType.IsEnum) {
                throw new ArgumentException("enumType must be an enum, not " + ObjectToCode.GetCSharpFriendlyTypeName(enumType));
            }
            var parser = EnumLabelParser.GetLabelParser(enumType);
            return parser.UntypedLookup(s, taal);
        }

        public static bool IsDefined<TEnum>(TEnum enumval) where TEnum : struct, IConvertible, IComparable
        {
            return Enum.IsDefined(typeof(TEnum), enumval);
        }
    }
}
