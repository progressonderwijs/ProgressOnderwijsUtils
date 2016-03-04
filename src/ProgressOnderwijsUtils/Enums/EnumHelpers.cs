using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
