using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
    interface IEnumMetaDataCache
    {
        IReadOnlyList<IEnumMetaData> AllUntypedValuesWithMetaData();
        IEnumMetaData UntypedMetaData(Enum val);
    }

    interface IEnumMetaDataCache<TEnum> : IEnumMetaDataCache
        where TEnum : struct, IConvertible, IComparable
    {
        IReadOnlyList<EnumMetaData<TEnum>> AllValuesWithMetaData();
        EnumMetaData<TEnum> MetaData(TEnum val);
    }

    static class EnumMetaDataStaticHelpers
    {
        //this class is separate from EnumMetaDataCache itself to avoid multiple copies of this static fields
        public static readonly ITranslatable translatableComma = Converteer.ToText(", ");
    }

    class EnumMetaDataCache<TEnum> : IEnumMetaDataCache<TEnum>
        where TEnum : struct, IConvertible, IComparable
    {
        public static readonly EnumMetaDataCache<TEnum> Instance = new EnumMetaDataCache<TEnum>();
        readonly TEnum[] EnumValues;
        public readonly bool IsFlags;
        readonly FieldInfo[] enumFields;
        readonly Type underlying;
        readonly TEnum[] ValuesInOverlapOrder;
        public readonly Func<TEnum, TEnum, TEnum> AddFlag;
        public readonly Func<TEnum, TEnum, bool> HasFlag;
        readonly Func<TEnum, TEnum, bool> FlagsOverlap;
        Func<TEnum, long> toInt64;
        AttrEntry[] sortedAttrs;
        public IReadOnlyList<IEnumMetaData> AllUntypedValuesWithMetaData() => EnumValues.SelectIndexable(e => (IEnumMetaData)new EnumMetaData<TEnum>(e));
        public IReadOnlyList<EnumMetaData<TEnum>> AllValuesWithMetaData() => EnumValues.SelectIndexable(e => new EnumMetaData<TEnum>(e));
        public IEnumMetaData UntypedMetaData(Enum val) => new EnumMetaData<TEnum>((TEnum)(object)val);
        public EnumMetaData<TEnum> MetaData(TEnum val) => new EnumMetaData<TEnum>(val);
        public IReadOnlyList<TEnum> AllValues() => EnumValues;
        public ITranslatable GetLabel(TEnum val) => IsFlags ? GetFlagsLabel(val) : GetSingleLabel(val);

        public object[] AllAttributes(TEnum value)
        {
            EnsureAttrCacheInitialized();
            var key = toInt64(value);
            var idx = IdxAfterLastLtNode(key);
            if (idx < sortedAttrs.Length && sortedAttrs[idx].Value == key) {
                return sortedAttrs[idx].Attrs;
            } else {
                return ArrayExtensions.Empty<object>();
            }
        }

        EnumMetaDataCache()
        {
            if (!typeof(TEnum).IsEnum) {
                throw new InvalidOperationException($"{typeof(EnumMetaDataCache<>).Name} werkt alleen met enums; niet ${ObjectToCode.GetCSharpFriendlyTypeName(typeof(TEnum))}");
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

            if (IsFlags) {
                var fastpathHelpers = FlagsEnumOperationMethodInfos.GetFlagsMethods(underlying);

                if (fastpathHelpers.Or != null) {
                    HasFlag = CreateDelegate<Func<TEnum, TEnum, bool>>(fastpathHelpers.HasFlag);
                    FlagsOverlap = CreateDelegate<Func<TEnum, TEnum, bool>>(fastpathHelpers.HasFlagOverlap);
                    AddFlag = CreateDelegate<Func<TEnum, TEnum, TEnum>>(fastpathHelpers.Or);
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

        Func<TEnum, TEnum, bool> MakeHasFlag()
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

        Func<TEnum, TEnum, bool> MakeFlagsOverlap()
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

        Func<TEnum, TEnum, TEnum> MakeAddFlag()
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

        static TFunc CreateDelegate<TFunc>(MethodInfo method)
        {
            return (TFunc)(object)Delegate.CreateDelegate(typeof(TFunc), method);
        }

        struct AttrEntry
        {
            public long Value;
            public object[] Attrs;
            public ITranslatable Label;
        }

        int IdxAfterLastLtNode(long needle)
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

        void EnsureAttrCacheInitialized()
        {
            if (sortedAttrs != null) {
                return;
            }

            if (typeof(int) == underlying) {
                toInt64 = CreateDelegate<Func<TEnum, long>>(FlagsEnumOperationMethodInfos.forInt32.ToInt64);
            } else if (typeof(long) == underlying) {
                toInt64 = CreateDelegate<Func<TEnum, long>>(FlagsEnumOperationMethodInfos.forInt64.ToInt64);
            } else {
                toInt64 = v => v.ToInt64(null);//slow fallback
            }

            //TODO: some documentation of invariants here might be in order.
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

        ITranslatable GetFlagsLabel(TEnum val)
        {
            var values = ValuesInOverlapOrder;
            var matched = new List<TEnum>(values.Length);
            var covered = default(TEnum);

            var i = values.Length;
            while (i != 0) {
                i--;
                var flag = values[i];
                if (!FlagsOverlap(covered, flag) && HasFlag(val, flag) && !Equals(flag, default(TEnum))) {
                    covered = AddFlag(covered, flag);
                    matched.Add(flag);
                }
            }

            if (!Equals(covered, val)) {
                foreach (var flag in values) {
                    if (!HasFlag(covered, flag) && HasFlag(val, flag)) {
                        covered = AddFlag(covered, flag);
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
            return matched.Select(GetSingleLabel).Reverse().JoinTexts(EnumMetaDataStaticHelpers.translatableComma);
        }

        ITranslatable GetSingleLabel(TEnum f)
        {
            EnsureAttrCacheInitialized();
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
                throw new Exception("Cannot define both an untranslated and a translated label on the same enum member: " + f);
            }

            var tooltip = attrs.OfType<MpTooltipAttribute>().SingleOrDefault();
            var untranslatedTooltip = attrs.OfType<MpTooltipUntranslatedAttribute>().SingleOrDefault();
            if (tooltip != null && untranslatedTooltip != null) {
                throw new Exception("Cannot define both an untranslated and a translated tooltip on the same enum member: " + f);
            }

            var translatable =
                translatedlabel != null
                    ? translatedlabel.ToTranslatable()
                    : untranslatedlabel != null
                        ? untranslatedlabel.ToTranslatable()
                        : Converteer.ToText(StringUtils.PrettyCapitalizedPrintCamelCased(f.ToString(CultureInfo.InvariantCulture)));

            if (tooltip != null) {
                translatable = translatable.ReplaceTooltipWithText(Translatable.Literal(tooltip.NL, tooltip.EN, tooltip.DE));
            } else if (untranslatedTooltip != null) {
                translatable = translatable.ReplaceTooltipWithText(Converteer.ToText(untranslatedTooltip.Tooltip));
            }

            if (validIdx) {
                sortedAttrs[idx].Label = translatable;
            }
            return translatable;
        }
    }
}
