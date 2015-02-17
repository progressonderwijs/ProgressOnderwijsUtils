using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class EnumHelpers
    {
        interface IEnumMetaCache
        {
            IReadOnlyList<Enum> Values();
            ITranslatable GetEnumLabel(Enum val);
        }

        static class Int32Helpers
        {
            public static int Or(int a, int b) { return a | b; }
            public static bool HasFlag(int val, int flag) { return (val & flag) == flag; }
            public static bool HasFlagOverlap(int a, int b) { return (a & b) != 0; }
            public static long ToInt64(int a) { return a; }
        }

        static class Int64Helpers
        {
            public static long Or(long a, long b) { return a | b; }
            public static bool HasFlag(long val, long flag) { return (val & flag) == flag; }
            public static bool HasFlagOverlap(long a, long b) { return (a & b) != 0L; }
            public static long ToInt64(long a) { return a; }
        }

        struct FlagOperationMethods
        {
            public static FlagOperationMethods Get<T>(Func<T, T, T> or, Func<T, T, bool> hasFlag, Func<T, T, bool> overlaps, Func<T, long> toInt64)
            {
                return new FlagOperationMethods {
                    Or = or.Method,
                    HasFlag = hasFlag.Method,
                    HasFlagOverlap = overlaps.Method,
                    ToInt64 = toInt64.Method,
                };
            }

            public MethodInfo Or, HasFlag, HasFlagOverlap, ToInt64;
        }

        static readonly FlagOperationMethods forInt = FlagOperationMethods.Get<int>(Int32Helpers.Or, Int32Helpers.HasFlag, Int32Helpers.HasFlagOverlap, Int32Helpers.ToInt64)
            ,
            forLong = FlagOperationMethods.Get<long>(Int64Helpers.Or, Int64Helpers.HasFlag, Int64Helpers.HasFlagOverlap, Int64Helpers.ToInt64);

        static readonly ITranslatable translatableComma = Translatable.Raw(", ");
        static readonly ConcurrentDictionary<Type, IEnumMetaCache> enumMetaCache = new ConcurrentDictionary<Type, IEnumMetaCache>();

        static IEnumMetaCache GetEnumMetaCache(Type enumType)
        {
            return enumMetaCache.GetOrAdd(enumType, type => (IEnumMetaCache)Activator.CreateInstance(typeof(EnumMetaCache<>).MakeGenericType(type)));
        }

        struct EnumMetaCache<TEnum> : IEnumMetaCache
            where TEnum : struct, IConvertible, IComparable
        {
            public static readonly TEnum[] EnumValues;
            public static readonly bool IsFlags;
            static readonly FieldInfo[] enumFields;
            static readonly Type underlying;

            static EnumMetaCache()
            {
                if (!typeof(TEnum).IsEnum) {
                    throw new InvalidOperationException("EnumMetaCache werkt alleen met enums");
                }
                underlying = typeof(TEnum).GetEnumUnderlyingType();

                enumFields = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static);

                //The following linq-based computation of EnumValues is several times slower:
                //EnumValues = enumFields.Select(f => (TEnum)f.GetValue(null)).Distinct().ToArray();

                EnumValues = new TEnum[enumFields.Length];
                int nextIndex = 0;
                foreach (var fieldInfo in enumFields) {
                    bool duplicate = false;
                    var value = (TEnum)fieldInfo.GetValue(null);
                    for (int i = 0; i < nextIndex; i++) {
                        if (EnumValues[i].Equals(value)) {
                            duplicate = true;
                            break;
                        }
                    }
                    if (!duplicate) {
                        EnumValues[nextIndex] = value;
                        nextIndex++;
                    }
                }
                if (nextIndex != EnumValues.Length) {
                    Array.Resize(ref EnumValues, nextIndex);
                }

                IsFlags = typeof(TEnum).GetCustomAttributes(typeof(FlagsAttribute)).Any();
            }

            public static class FlagEnumHelpers
            {
                public static readonly TEnum[] ValuesInOverlapOrder;
                public static readonly Func<TEnum, TEnum, TEnum> AddFlag;
                public static readonly Func<TEnum, TEnum, bool> HasFlag;
                public static readonly Func<TEnum, TEnum, bool> FlagsOverlap;

                static FlagEnumHelpers()
                {
                    if (IsFlags) {
                        var fastpathHelpers = default(FlagOperationMethods);
                        if (typeof(int) == underlying) {
                            fastpathHelpers = forInt;
                        } else if (typeof(long) == underlying) {
                            fastpathHelpers = forLong;
                        }

                        if (fastpathHelpers.Or != null) {
                            CreateDelegate(out HasFlag, fastpathHelpers.HasFlag);
                            CreateDelegate(out FlagsOverlap, fastpathHelpers.HasFlagOverlap);
                            CreateDelegate(out AddFlag, fastpathHelpers.Or);
                        } else {
                            HasFlag = MakeHasFlag();
                            FlagsOverlap = MakeFlagsOverlap();
                            AddFlag = MakeAddFlag();
                        }

                        ValuesInOverlapOrder = (TEnum[])EnumValues.Clone();
                        var overlapCount = new int[ValuesInOverlapOrder.Length];

                        for (var i = 0; i < overlapCount.Length; i++) {
                            foreach (var val in ValuesInOverlapOrder) {
                                if (HasFlag(ValuesInOverlapOrder[i], val)) {
                                    overlapCount[i]++;
                                }
                            }
                        }

                        var n = overlapCount.Length;
                        while (true) {
                            var swapped = false;
                            for (var i = 1; i < n; i++) {
                                if (overlapCount[i - 1] > overlapCount[i]) {
                                    swapped = true;
                                    var tmp = overlapCount[i];
                                    overlapCount[i] = overlapCount[i - 1];
                                    overlapCount[i - 1] = tmp;
                                    var tmp2 = ValuesInOverlapOrder[i];
                                    ValuesInOverlapOrder[i] = ValuesInOverlapOrder[i - 1];
                                    ValuesInOverlapOrder[i - 1] = tmp2;
                                }
                            }
                            if (swapped) {
                                n--;
                            } else {
                                break;
                            }
                        }
                    }
                }

                static Func<TEnum, TEnum, bool> MakeHasFlag()
                {
                    var valExpr = Expression.Parameter(typeof(TEnum));
                    var flagExpr = Expression.Parameter(typeof(TEnum));

                    return Expression.Lambda<Func<TEnum, TEnum, bool>>(
                        Expression.Equal(
                            flagExpr,
                            Expression.ConvertChecked(
                                Expression.And(
                                    Expression.ConvertChecked(flagExpr, underlying),
                                    Expression.ConvertChecked(valExpr, underlying)
                                    ),
                                typeof(TEnum))
                            ),
                        valExpr,
                        flagExpr
                        ).Compile();
                }

                static Func<TEnum, TEnum, bool> MakeFlagsOverlap()
                {
                    var valExpr = Expression.Parameter(typeof(TEnum));
                    var flagExpr = Expression.Parameter(typeof(TEnum));

                    return Expression.Lambda<Func<TEnum, TEnum, bool>>(
                        Expression.NotEqual(
                            Expression.Default(underlying),
                            Expression.And(
                                Expression.ConvertChecked(flagExpr, underlying),
                                Expression.ConvertChecked(valExpr, underlying)
                                )
                            ),
                        valExpr,
                        flagExpr
                        ).Compile();
                }

                static Func<TEnum, TEnum, TEnum> MakeAddFlag()
                {
                    var valExpr = Expression.Parameter(typeof(TEnum));
                    var flagExpr = Expression.Parameter(typeof(TEnum));

                    return Expression.Lambda<Func<TEnum, TEnum, TEnum>>(
                        Expression.ConvertChecked(
                            Expression.Or(
                                Expression.ConvertChecked(flagExpr, underlying),
                                Expression.ConvertChecked(valExpr, underlying)
                                ),
                            typeof(TEnum)
                            ),
                        valExpr,
                        flagExpr).Compile();
                }
            }

            static void CreateDelegate<TFunc>(out TFunc func, MethodInfo method) { func = (TFunc)(object)Delegate.CreateDelegate(typeof(TFunc), method); }

            struct AttrEntry
            {
                public long Value;
                public object[] Attrs;
                public ITranslatable Label;
            }

            static AttrEntry[] sortedAttrs;
            static Func<TEnum, long> toInt64;

            static int IdxAfterLastLtNode(long needle)
            {
                //based on https://github.com/EamonNerbonne/a-vs-an/blob/master/A-vs-An/A-vs-An-DotNet/Internals/Node.cs
                int start = 0, end = sortedAttrs.Length;
                //invariant: only LT nodes before start
                //invariant: only GTE nodes at or past end
                while (end != start) {
                    var midpoint = end + start >> 1;
                    // start <= midpoint < end
                    if (sortedAttrs[midpoint].Value < needle) {
                        start = midpoint + 1; //i.e. midpoint < start1 so start0 < start1
                    } else {
                        end = midpoint; //i.e end1 = midpoint so end1 < end0
                    }
                }
                return end;
            }

            public static object[] Attributes(TEnum value)
            {
                if (sortedAttrs == null) {
                    InitAttrCache();
                }
                var key = toInt64(value);
                var idx = IdxAfterLastLtNode(key);
                if (idx < sortedAttrs.Length && sortedAttrs[idx].Value == key) {
                    return sortedAttrs[idx].Attrs;
                } else {
                    return ArrayExtensions.Empty<object>();
                }
            }

            static void InitAttrCache()
            {
                if (typeof(int) == underlying) {
                    CreateDelegate(out toInt64, forInt.ToInt64);
                } else {
                    toInt64 = a => a.ToInt64(null);
                }

                var entries = new AttrEntry[EnumValues.Length];
                var nextIdx = 0;
                foreach (var field in enumFields) {
                    var customAttributes = field.GetCustomAttributes(typeof(Attribute), false);
                    if (customAttributes.Length == 0) {
                        continue;
                    }
                    var value = (TEnum)field.GetValue(null);
                    var key = toInt64(value);
                    var insertIdx = nextIdx - 1;
                    while (true) {
                        if (insertIdx == -1 || key > entries[insertIdx].Value) {
                            insertIdx++;
                            for (var j = nextIdx - 1; j >= insertIdx; j--) {
                                entries[j + 1] = entries[j];
                            }
                            entries[insertIdx] = new AttrEntry {
                                Value = key,
                                Attrs = customAttributes
                            };
                            nextIdx++;
                            break;
                        } else if (key == entries[insertIdx].Value) {
                            var oldLength = entries[insertIdx].Attrs.Length;
                            Array.Resize(ref entries[insertIdx].Attrs, oldLength + customAttributes.Length);
                            var i = oldLength;
                            foreach (var attr in customAttributes) {
                                entries[insertIdx].Attrs[i++] = attr;
                            }
                            break;
                        } else {
                            insertIdx--;
                        }
                    }
                }
                if (nextIdx < entries.Length) {
                    Array.Resize(ref entries, nextIdx);
                }
                sortedAttrs = entries;
            }

            public IReadOnlyList<Enum> Values() { return EnumValues.SelectIndexable(e => (Enum)(object)e); }
            public ITranslatable GetEnumLabel(Enum val) { return GetLabel((TEnum)(object)val); }

            public static ITranslatable GetLabel(TEnum val)
            {
                if (IsFlags) {
                    return GetFlagsLabel(val);
                } else {
                    return GetSingleLabel(val);
                }
            }

            static ITranslatable GetFlagsLabel(TEnum val)
            {
                var values = FlagEnumHelpers.ValuesInOverlapOrder;
                var matched = new List<TEnum>(values.Length);
                var covered = default(TEnum);

                var i = values.Length;
                while (i != 0) {
                    i--;
                    var flag = values[i];
                    if (!FlagEnumHelpers.FlagsOverlap(covered, flag) && FlagEnumHelpers.HasFlag(val, flag) && !Equals(flag, default(TEnum))) {
                        covered = FlagEnumHelpers.AddFlag(covered, flag);
                        matched.Add(flag);
                    }
                }

                if (!Equals(covered, val)) {
                    foreach (var flag in values) {
                        if (!FlagEnumHelpers.HasFlag(covered, flag) && FlagEnumHelpers.HasFlag(val, flag)) {
                            covered = FlagEnumHelpers.AddFlag(covered, flag);
                            matched.Add(flag);
                        }
                    }
                    if (!Equals(covered, val)) {
                        throw new ArgumentOutOfRangeException(
                            "Enum Value " + val + " is not a combination of flags in type " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(TEnum)));
                    }
                }
                if (matched.Count == 1) {
                    return GetSingleLabel(matched[0]);
                }
                return matched.Select(GetSingleLabel).Reverse().JoinTexts(translatableComma);
            }

            static ITranslatable GetSingleLabel(TEnum f)
            {
                if (sortedAttrs == null) {
                    InitAttrCache();
                }
                var key = toInt64(f);
                var idx = IdxAfterLastLtNode(key);
                var validIdx = idx < sortedAttrs.Length && sortedAttrs[idx].Value == key;

                if (validIdx && sortedAttrs[idx].Label != null) {
                    return sortedAttrs[idx].Label;
                }

                var attrs = validIdx ? sortedAttrs[idx].Attrs : ArrayExtensions.Empty<object>();

                var translatedlabel = attrs.OfType<MpLabelAttribute>().SingleOrDefault();
                var untranslatedlabel = attrs.OfType<MpLabelUntranslatedAttribute>().SingleOrDefault();
                if (translatedlabel != null && untranslatedlabel != null) {
                    throw new Exception("Cannot define both an untranslated and a translated label on the same enum: " + f);
                }

                var tooltip = attrs.OfType<MpTooltipAttribute>().SingleOrDefault();

                var translatable =
                    translatedlabel != null
                        ? translatedlabel.ToTranslatable()
                        : untranslatedlabel != null
                            ? untranslatedlabel.ToTranslatable()
                            : Converteer.ToText(StringUtils.PrettyCapitalizedPrintCamelCased(f.ToString(CultureInfo.InvariantCulture)));

                if (tooltip != null) {
                    translatable = translatable.ReplaceTooltipWithText(Translatable.Literal(tooltip.NL, tooltip.EN, tooltip.DE));
                }

                if (validIdx) {
                    sortedAttrs[idx].Label = translatable;
                }
                return translatable;
            }
        }

        interface ILabelLookup
        {
            IEnumerable<Enum> Lookup(string s, Taal taal);
        }

        struct EnumLabelLookup<TEnum> : ILabelLookup
            where TEnum : struct, IConvertible, IComparable
        {
            static readonly Dictionary<Taal, ILookup<string, TEnum>> ParseLabels = GetValues<Taal>()
                .Where(t => t != Taal.None)
                .ToDictionary(taal => taal, taal => GetValues<TEnum>().ToLookup(e => GetLabel(e).Translate(taal).Text.Trim(), e => e, StringComparer.OrdinalIgnoreCase));

            public static IEnumerable<TEnum> Lookup(string s, Taal taal)
            {
                if (taal == Taal.None) {
                    throw new ArgumentOutOfRangeException("taal", "Taal is niet gezet.  (== Taal.None)");
                }
                return !EnumMetaCache<TEnum>.IsFlags
                    ? ParseLabels[taal][s.Trim()]
                    : new[] {
                        s.Split(',')
                            .Select(sub => sub.Trim())
                            .Where(sub => sub.Length > 0)
                            .Select(sub => ParseLabels[taal][sub].Single())
                            .Aggregate(default(TEnum), EnumMetaCache<TEnum>.FlagEnumHelpers.AddFlag)
                    };
            }

            IEnumerable<Enum> ILabelLookup.Lookup(string s, Taal taal) { return Lookup(s, taal).Select(e => (Enum)(object)e); }
        }

        public static class GetAttrs<TAttr>
            where TAttr : Attribute
        {
            public static IEnumerable<TAttr> On<T>(T enumVal) where T : struct, IConvertible, IComparable
            {
                return EnumMetaCache<T>.Attributes(enumVal).OfType<TAttr>();
                //return EnumMetaCache<T>.AttrCache<TAttr>.EnumMemberAttributes[enumVal];
            }

            public static IEnumerable<T> From<T>(Func<TAttr, bool> pred) where T : struct, IConvertible, IComparable
            {
                return EnumMetaCache<T>.EnumValues.Where(v => On(v).Any(pred));
            }
        }

        public static T GetAttributeValue<T>(this Enum enumValue) where T : Attribute
        {
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();
            if (memberInfo != null) {
                var attribute = (T)memberInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();
                return attribute;
            }
            return null;
        }

        public static IReadOnlyList<T> GetValues<T>() where T : struct, IConvertible, IComparable { return EnumMetaCache<T>.EnumValues; }
        public static IReadOnlyList<Enum> GetValues(Type enumType) { return GetEnumMetaCache(enumType).Values(); }
        public static Func<TEnum, TEnum, TEnum> AddFlagsFunc<TEnum>() where TEnum : struct, IConvertible, IComparable { return EnumMetaCache<TEnum>.FlagEnumHelpers.AddFlag; }
        public static Func<TEnum, TEnum, bool> HasFlagsFunc<TEnum>() where TEnum : struct, IConvertible, IComparable { return EnumMetaCache<TEnum>.FlagEnumHelpers.HasFlag; }
        public static ITranslatable GetLabel<TEnum>(TEnum f) where TEnum : struct, IConvertible, IComparable { return EnumMetaCache<TEnum>.GetLabel(f); }

        public static ITranslatable GetLabel(Enum enumVal)
        {
            return GetEnumMetaCache(enumVal.GetType())
                .GetEnumLabel(enumVal);
        }

        public static SelectItem<TEnum> GetSelectItem<TEnum>(TEnum f)
            where TEnum : struct, IConvertible, IComparable
        {
            return SelectItem.Create(f, GetLabel(f));
        }

        public static DataTable ToIntKoppelTabel<TEnum>(IEnumerable<TEnum> values, Taal taal)
            where TEnum : struct, IConvertible, IComparable
        {
            return values.Select(
                v =>
                    new KoppelTabelEntry { Id = v.ToInt32(null), Tekst = GetLabel(v).Translate(taal).Text }
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
            if (!typeof(TEnum).IsEnum) {
                throw new ArgumentException("type must be an enum, not " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(TEnum)));
            }

            TEnum retval;

            if (Enum.TryParse(s, false, out retval)) {
                return retval;
            } else {
                throw new ArgumentException("Could not parse string as " + typeof(TEnum).Name);
            }
        }

        public static IEnumerable<TEnum> TryParseLabel<TEnum>(string s, Taal taal) where TEnum : struct, IConvertible, IComparable
        {
            return EnumLabelLookup<TEnum>.Lookup(s, taal);
        }

        public static IEnumerable<Enum> TryParseLabel(Type enumType, string s, Taal taal)
        {
            if (!enumType.IsEnum) {
                throw new ArgumentException("enumType must be an enum, not " + ObjectToCode.GetCSharpFriendlyTypeName(enumType));
            }
            var parser = (ILabelLookup)Activator.CreateInstance(typeof(EnumLabelLookup<>).MakeGenericType(enumType));
            return parser.Lookup(s, taal);
        }

        public static bool IsDefined<TEnum>(TEnum enumval) where TEnum : struct, IConvertible, IComparable { return Enum.IsDefined(typeof(TEnum), enumval); }

        public static IEnumerable<T> AllCombinations<T>() where T : struct, IConvertible, IComparable
        {
            // Construct a function for OR-ing together two enums
            var orFunction = AddFlagsFunc<T>();

            var initalValues = (T[])Enum.GetValues(typeof(T));
            var discoveredCombinations = new HashSet<T>(initalValues);
            var queue = new Queue<T>(initalValues);

            // Try OR-ing every inital value to each value in the queue
            while (queue.Count > 0) {
                var a = queue.Dequeue();
                foreach (T b in initalValues) {
                    T combo = orFunction(a, b);
                    if (discoveredCombinations.Add(combo)) {
                        queue.Enqueue(combo);
                    }
                }
            }

            return discoveredCombinations;
        }
    }
}
