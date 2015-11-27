using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    static class EnumLabelParser {
        public static IUntypedEnumLabelParser GetLabelParser(Type enumType) 
            => (IUntypedEnumLabelParser)Activator.CreateInstance(typeof(EnumLabelParser<>).MakeGenericType(enumType));
    }

    interface IUntypedEnumLabelParser
    {
        IEnumerable<Enum> UntypedLookup(string s, Taal taal);
    }

    struct EnumLabelParser<TEnum> : IUntypedEnumLabelParser
        where TEnum : struct, IConvertible, IComparable
    {
        static readonly Dictionary<Taal, ILookup<string, TEnum>> ParseLabels = EnumHelpers.GetValues<Taal>()
            .Where(t => t != Taal.None)
            .ToDictionary(taal => taal, taal => EnumHelpers.GetValues<TEnum>().ToLookup(e => EnumHelpers.GetLabel<TEnum>(e).Translate(taal).Text.Trim(), e => e, StringComparer.OrdinalIgnoreCase));

        public static IEnumerable<TEnum> Lookup(string s, Taal taal)
        {
            if (taal == Taal.None) {
                throw new ArgumentOutOfRangeException(nameof(taal), "Taal is niet gezet.  (== Taal.None)");
            }
            var cache = EnumMetaDataCache<TEnum>.Instance;
            return !cache.IsFlags
                ? ParseLabels[taal][s.Trim()]
                : new[] {
                    s.Split(',')
                        .Select(sub => sub.Trim())
                        .Where(sub => sub.Length > 0)
                        .Select(sub => ParseLabels[taal][sub].Single())
                        .Aggregate(default(TEnum), cache.AddFlag)
                };
        }

        public IEnumerable<Enum> UntypedLookup(string s, Taal taal) => Lookup(s, taal).Select(e => (Enum)(object)e);
    }
}