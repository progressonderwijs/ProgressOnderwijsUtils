using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils
{
	public static class EnumHelpers
	{
		interface IEnumValues
		{
			IReadOnlyList<Enum> Values();
			ITranslatable GetEnumLabel(Enum val);
		}

		static class Int32Helpers
		{
			public static int Or(int a, int b)
			{
				return a | b;
			}

			public static bool HasFlag(int val, int flag)
			{
				return (val & flag) == flag;
			}

			public static bool HasFlagOverlap(int a, int b)
			{
				return (a & b) != 0;
			}
			public static bool Equals(int a, int b)
			{
				return (a == b);
			}
			public static int GetHashcode(int a)
			{
				return a;
			}
			public static int Compare(int a, int b)
			{
				return a < b ? -1 : a == b ? 0 : 1;
			}
		}


		static class Int64Helpers
		{
			public static long Or(long a, long b)
			{
				return a | b;
			}

			public static bool HasFlag(long val, long flag)
			{
				return (val & flag) == flag;
			}

			public static bool HasFlagOverlap(long a, long b)
			{
				return (a & b) != 0L;
			}
			public static bool Equals(long a, long b)
			{
				return (a == b);
			}
			public static int GetHashcode(long a)
			{
				return (int)(a >> 32) ^ (int)a;
			}
			public static int Compare(long a, long b)
			{
				return a < b ? -1 : a == b ? 0 : 1;
			}
		}


		struct FlagOperationMethods
		{
			public static FlagOperationMethods Get<T>(Func<T, T, T> or, Func<T, T, bool> hasFlag, Func<T, T, bool> overlaps, Func<T, T, bool> equals, Func<T, int> getHashcode, Func<T, T, int> compare)
			{
				return new FlagOperationMethods
				{
					Or = or.Method,
					HasFlag = hasFlag.Method,
					HasFlagOverlap = overlaps.Method,
					EqualsMethod = equals.Method,
					GetHashcode = getHashcode.Method,
					Compare = compare.Method,
				};
			}

			public MethodInfo Or, HasFlag, HasFlagOverlap, EqualsMethod, GetHashcode, Compare;
		}

		static FlagOperationMethods forInt = FlagOperationMethods.Get<int>(Int32Helpers.Or, Int32Helpers.HasFlag, Int32Helpers.HasFlagOverlap, Int32Helpers.Equals, Int32Helpers.GetHashcode, Int32Helpers.Compare)

			, forLong = FlagOperationMethods.Get<long>(Int64Helpers.Or, Int64Helpers.HasFlag, Int64Helpers.HasFlagOverlap, Int64Helpers.Equals, Int64Helpers.GetHashcode, Int64Helpers.Compare);

		class LambdaEqualityComparer<T> : IEqualityComparer<T>
		{
			readonly Func<T, T, bool> equals;
			readonly Func<T, int> hash;
			public LambdaEqualityComparer(Func<T, T, bool> equals, Func<T, int> hash)
			{
				this.equals = equals;
				this.hash = hash;
			}

			public bool Equals(T x, T y)
			{
				return equals(x, y);
			}

			public int GetHashCode(T obj)
			{
				return hash(obj);
			}
		}

		static ITranslatable translatableComma = TextDefSimple.Create(", ");

		struct EnumMetaCache<TEnum> : IEnumValues where TEnum : struct, IConvertible, IComparable
		{
			public static readonly TEnum[] EnumValues;
			public static readonly bool IsFlags;
			static readonly FieldInfo[] enumFields;
			static readonly Type underlying;

			static EnumMetaCache()
			{
				if (!typeof(TEnum).IsEnum)
					throw new InvalidOperationException("EnumMetaCache werkt alleen met enums");
				underlying = typeof(TEnum).GetEnumUnderlyingType();

				enumFields = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static);

				//The following linq-based computation of EnumValues is several times slower:
				//EnumValues = enumFields.Select(f => (TEnum)f.GetValue(null)).Distinct().ToArray();

				EnumValues = new TEnum[enumFields.Length];
				int nextIndex = 0;
				foreach (var fieldInfo in enumFields)
				{
					bool duplicate = false;
					var value = (TEnum)fieldInfo.GetValue(null);
					for (int i = 0; i < nextIndex; i++)
						if (EnumValues[i].Equals(value))
						{
							duplicate = true;
							break;
						}
					if (!duplicate)
					{
						EnumValues[nextIndex] = value;
						nextIndex++;
					}
				}
				if (nextIndex != EnumValues.Length)
					Array.Resize(ref EnumValues, nextIndex);

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
					if (IsFlags)
					{

						FlagOperationMethods fastpathHelpers = default(FlagOperationMethods);
						if (typeof(int) == underlying)
						{
							fastpathHelpers = forInt;
						}
						else if (typeof(long) == underlying)
						{
							fastpathHelpers = forLong;
						}

						if (fastpathHelpers.Or != null)
						{
							CreateDelegate(out HasFlag, fastpathHelpers.HasFlag);
							CreateDelegate(out FlagsOverlap, fastpathHelpers.HasFlagOverlap);
							CreateDelegate(out AddFlag, fastpathHelpers.Or);
						}
						else
						{
							HasFlag = MakeHasFlag();
							FlagsOverlap = MakeFlagsOverlap();
							AddFlag = MakeAddFlag();
						}

						ValuesInOverlapOrder = (TEnum[])EnumValues.Clone();
						var overlapCount = new int[ValuesInOverlapOrder.Length];


						for (int i = 0; i < overlapCount.Length; i++)
						{
							foreach (var val in ValuesInOverlapOrder)
							{
								if (HasFlag(ValuesInOverlapOrder[i], val))
									overlapCount[i]++;
							}
						}

						var n = overlapCount.Length;
						while (true)
						{
							bool swapped = false;
							for (int i = 1; i < n; i++)
							{
								if (overlapCount[i - 1] > overlapCount[i])
								{
									swapped = true;
									var tmp = overlapCount[i];
									overlapCount[i] = overlapCount[i - 1];
									overlapCount[i - 1] = tmp;
									var tmp2 = ValuesInOverlapOrder[i];
									ValuesInOverlapOrder[i] = ValuesInOverlapOrder[i - 1];
									ValuesInOverlapOrder[i - 1] = tmp2;
								}
							}
							if (swapped)
								n--;
							else
								break;
						}
					}
				}


				static Func<TEnum, TEnum, bool> MakeHasFlag()
				{
					ParameterExpression valExpr = Expression.Parameter(typeof(TEnum));
					ParameterExpression flagExpr = Expression.Parameter(typeof(TEnum));

					return Expression.Lambda<Func<TEnum, TEnum, bool>>(
						Expression.Equal(
							flagExpr,
							Expression.ConvertChecked(Expression.And(
								Expression.ConvertChecked(flagExpr, underlying),
								Expression.ConvertChecked(valExpr, underlying)
								), typeof(TEnum))
							),
						valExpr, flagExpr
						).Compile();
				}

				static Func<TEnum, TEnum, bool> MakeFlagsOverlap()
				{
					ParameterExpression valExpr = Expression.Parameter(typeof(TEnum));
					ParameterExpression flagExpr = Expression.Parameter(typeof(TEnum));

					return Expression.Lambda<Func<TEnum, TEnum, bool>>(
						Expression.NotEqual(
							Expression.Default(underlying),
							Expression.And(
								Expression.ConvertChecked(flagExpr, underlying),
								Expression.ConvertChecked(valExpr, underlying)
								)
							),
						valExpr, flagExpr
						).Compile();
				}

				static Func<TEnum, TEnum, TEnum> MakeAddFlag()
				{
					ParameterExpression valExpr = Expression.Parameter(typeof(TEnum));
					ParameterExpression flagExpr = Expression.Parameter(typeof(TEnum));

					return Expression.Lambda<Func<TEnum, TEnum, TEnum>>(
						Expression.ConvertChecked(
							Expression.Or(
								Expression.ConvertChecked(flagExpr, underlying),
								Expression.ConvertChecked(valExpr, underlying)
								), typeof(TEnum)
							), valExpr, flagExpr).Compile();
				}
			}
			static void CreateDelegate<TFunc>(out TFunc func, MethodInfo method)
			{
				func = (TFunc)(object)Delegate.CreateDelegate(typeof(TFunc), method);
			}


			struct AttrEntry
			{
				public TEnum Value;
				public object[] Attrs;
				public ITranslatable Label;
			}


			static AttrEntry[] sortedAttrs;
			static Comparison<TEnum> comp;


			static int IdxAfterLastLtNode(TEnum needle)
			{
				int start = 0, end = sortedAttrs.Length;
				//invariant: only LT nodes before start
				//invariant: only GTE nodes at or past end
				while (end != start)
				{
					int midpoint = end + start >> 1;
					// start <= midpoint < end
					if (comp(sortedAttrs[midpoint].Value, needle) < 0)
					{
						start = midpoint + 1;//i.e. midpoint < start1 so start0 < start1
					}
					else
					{
						end = midpoint;//i.e end1 = midpoint so end1 < end0
					}
				}
				return end;
			}

			public static object[] Attributes(TEnum value)
			{
				if (sortedAttrs == null)
					InitAttrCache();
				int idx = IdxAfterLastLtNode(value);
				if (idx < sortedAttrs.Length && comp(sortedAttrs[idx].Value, value) == 0)
					return sortedAttrs[idx].Attrs;
				else return ArrayExtensions.Empty<object>();
			}

			static void InitAttrCache()
			{
				if (typeof(int) == underlying)
				{
					CreateDelegate(out comp, forInt.Compare);
				}
				else
				{
					comp = (a, b) => a.CompareTo(b);
				}

				var entries = new AttrEntry[EnumValues.Length];
				int nextIdx = 0;
				foreach (var field in enumFields)
				{
					var customAttributes = field.GetCustomAttributes(typeof(Attribute), false);
					if (customAttributes.Length == 0)
						continue;
					var value = (TEnum)field.GetValue(null);
					int insertIdx = nextIdx - 1;
					while (true)
					{

						var comparison = insertIdx == -1 ? 1 : comp(value, entries[insertIdx].Value);
						if (comparison < 0)
						{
							insertIdx--;
						}
						else if (comparison == 0)
						{

							var oldLength = entries[insertIdx].Attrs.Length;
							Array.Resize(ref entries[insertIdx].Attrs, oldLength + customAttributes.Length);
							int i = oldLength;
							foreach (var attr in customAttributes)
								entries[insertIdx].Attrs[i++] = attr;
							break;
						}
						else
						{
							insertIdx++;
							for (int j = nextIdx - 1; j >= insertIdx; j--)
								entries[j + 1] = entries[j];
							entries[insertIdx] = new AttrEntry
							{
								Value = value,
								Attrs = customAttributes
							};
							nextIdx++;
							break;
						}
					}
				}
				if (nextIdx < entries.Length)
					Array.Resize(ref entries, nextIdx);
				sortedAttrs = entries;
			}

			public IReadOnlyList<Enum> Values()
			{
				return EnumValues.SelectIndexable(e => (Enum)(object)e);
			}

			public ITranslatable GetEnumLabel(Enum val)
			{
				return GetLabel((TEnum)(object)val);
			}

			public static ITranslatable GetLabel(TEnum val)
			{
				if (IsFlags)
					return GetFlagsLabel(val);
				else
					return GetSingleLabel(val);
			}

			static ITranslatable GetFlagsLabel(TEnum val)
			{
				var values = FlagEnumHelpers.ValuesInOverlapOrder;
				var matched = new List<TEnum>(values.Length);
				TEnum covered = default(TEnum);

				int i = values.Length;
				while (i != 0)
				{
					i--;
					var flag = values[i];
					if (!FlagEnumHelpers.FlagsOverlap(covered, flag) && FlagEnumHelpers.HasFlag(val, flag) && !Equals(flag, default(TEnum)))
					{
						covered = FlagEnumHelpers.AddFlag(covered, flag);
						matched.Add(flag);
					}
				}

				if (!Equals(covered, val))
				{
					foreach (var flag in values)
					{
						if (!FlagEnumHelpers.HasFlag(covered, flag) && FlagEnumHelpers.HasFlag(val, flag))
						{
							covered = FlagEnumHelpers.AddFlag(covered, flag);
							matched.Add(flag);
						}
					}
					if (!Equals(covered, val))
						throw new ArgumentOutOfRangeException("Enum Value " + val + " is not a combination of flags in type " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(TEnum)));
				}
				if (matched.Count == 1)
					return GetSingleLabel(matched[0]);
				return matched.Select(GetSingleLabel).Reverse().JoinTexts(translatableComma);
			}

			static ITranslatable GetSingleLabel(TEnum f)
			{
				if (sortedAttrs == null)
					InitAttrCache();
				var idx = IdxAfterLastLtNode(f);
				bool validIdx = idx < sortedAttrs.Length && comp(sortedAttrs[idx].Value, f) == 0;


				if (validIdx && sortedAttrs[idx].Label != null)
					return sortedAttrs[idx].Label;

				var attrs = validIdx ? sortedAttrs[idx].Attrs : ArrayExtensions.Empty<object>();

				var translatedlabel = attrs.OfType<MpLabelAttribute>().SingleOrDefault();
				var untranslatedlabel = attrs.OfType<MpLabelUntranslatedAttribute>().SingleOrDefault();
				if (translatedlabel != null && untranslatedlabel != null)
					throw new Exception("Cannot define both an untranslated and a translated label on the same enum: " + f);

				var tooltip = attrs.OfType<MpTooltipAttribute>().SingleOrDefault();
				//if (translatedlabel == null && untranslatedlabel == null && tooltip == null && !EnumMembers.Contains(f))
				//throw new ArgumentOutOfRangeException("Enum Value " + f + " does not exist in type " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(TEnum)));

				var translatable =
					translatedlabel != null ? translatedlabel.ToTranslatable()
						: untranslatedlabel != null ? untranslatedlabel.ToTranslatable()
							: Converteer.ToText(StringUtils.PrettyCapitalizedPrintCamelCased(f.ToString(CultureInfo.InvariantCulture)));

				if (tooltip != null)
					translatable = translatable.ReplaceTooltipWithText(Translatable.Literal(tooltip.NL, tooltip.EN, tooltip.DE));

				if (validIdx)
					sortedAttrs[idx].Label = translatable;
				return translatable;
			}
		}


		interface ILabelLookup
		{
			IEnumerable<Enum> Lookup(string s, Taal taal);
		}


		struct EnumLabelLookup<TEnum> : ILabelLookup where TEnum : struct, IConvertible, IComparable
		{
			static readonly Dictionary<Taal, ILookup<string, TEnum>> ParseLabels = GetValues<Taal>().Where(t => t != Taal.None).ToDictionary(taal => taal, taal => GetValues<TEnum>().ToLookup(e => GetLabel(e).Translate(taal).Text.Trim(), e => e, StringComparer.OrdinalIgnoreCase));

			public static IEnumerable<TEnum> Lookup(string s, Taal taal)
			{
				if (taal == Taal.None)
					throw new ArgumentOutOfRangeException("taal", "Taal is niet gezet.  (== Taal.None)");
				return !EnumMetaCache<TEnum>.IsFlags ? ParseLabels[taal][s.Trim()] :
					new[]
					{
						s.Split(',')
							.Select(sub => sub.Trim())
							.Where(sub => sub.Length > 0)
							.Select(sub => ParseLabels[taal][sub].Single())
							.Aggregate(default(TEnum), EnumMetaCache<TEnum>.FlagEnumHelpers.AddFlag)
					};
			}

			IEnumerable<Enum> ILabelLookup.Lookup(string s, Taal taal)
			{
				return Lookup(s, taal).Select(e => (Enum)(object)e);
			}
		}


		public static class GetAttrs<TAttr> where TAttr : Attribute
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
			if (memberInfo != null)
			{
				var attribute = (T)memberInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();
				return attribute;
			}
			return null;
		}

		public static IReadOnlyList<T> GetValues<T>() where T : struct, IConvertible, IComparable
		{
			return EnumMetaCache<T>.EnumValues;
		}

		public static IReadOnlyList<Enum> GetValues(Type enumType)
		{
			if (!enumType.IsEnum)
				throw new ArgumentException("enumType must be an enum, not " + ObjectToCode.GetCSharpFriendlyTypeName(enumType));
			var values = (IEnumValues)Activator.CreateInstance(typeof(EnumMetaCache<>).MakeGenericType(enumType));
			return values.Values();
		}

		public static Func<TEnum, TEnum, TEnum> AddFlagsFunc<TEnum>() where TEnum : struct, IConvertible, IComparable
		{
			return EnumMetaCache<TEnum>.FlagEnumHelpers.AddFlag;
		}

		public static Func<TEnum, TEnum, bool> HasFlagsFunc<TEnum>() where TEnum : struct, IConvertible, IComparable
		{
			return EnumMetaCache<TEnum>.FlagEnumHelpers.HasFlag;
		}

		public static ITranslatable GetLabel<TEnum>(TEnum f) where TEnum : struct, IConvertible, IComparable
		{
			return EnumMetaCache<TEnum>.GetLabel(f);
		}

		public static ITranslatable GetLabel(Enum enumVal)
		{
			if (enumVal == null)
				throw new ArgumentNullException("enumVal");
			var type = enumVal.GetType();
			if (!type.IsEnum)
				throw new ArgumentException("enumVal must be an enum value, not of type " + ObjectToCode.GetCSharpFriendlyTypeName(type));
			var labeller = (IEnumValues)Activator.CreateInstance(typeof(EnumMetaCache<>).MakeGenericType(type));
			return labeller.GetEnumLabel(enumVal);
		}

		public static SelectItem<TEnum> GetSelectItem<TEnum>(TEnum f)
			where TEnum : struct, IConvertible, IComparable
		{
			return SelectItem.Create(f, GetLabel(f));
		}

		public static DataTable ToIntKoppelTabel<TEnum>(IEnumerable<TEnum> values, Taal taal)
			where TEnum : struct, IConvertible, IComparable
		{
			return values.Select(v =>
				new KoppelTabelEntry { Id = v.ToInt32(null), Tekst = GetLabel(v).Translate(taal).Text }
				).ToDataTable();
		}

		public static DataTable ToIntKoppelTabel_OrderedByText<TEnum>(IEnumerable<TEnum> values, Taal taal)
			where TEnum : struct, IConvertible, IComparable
		{
			return values.Select(v =>
				new KoppelTabelEntry { Id = v.ToInt32(null), Tekst = GetLabel(v).Translate(taal).Text }
				)
				.OrderBy(entry => entry.Tekst)
				.ToDataTable();
		}

		public static DataTable ToIntKoppelTabelExpandedText<TEnum>(IEnumerable<TEnum> values, Taal taal)
			where TEnum : struct, IConvertible, IComparable
		{
			return values.Select(v =>
				new KoppelTabelEntry { Id = v.ToInt32(null), Tekst = GetLabel(v).Translate(taal).ExtraText }
				)
				.ToDataTable();
		}

		public static SelectItem<TEnum?> GetSelectItem<TEnum>(TEnum? f)
			where TEnum : struct, IConvertible, IComparable
		{
			return SelectItem.Create(f, f == null ? TextDefSimple.EmptyText : GetLabel(f.Value));
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
			if (!typeof(TEnum).IsEnum)
				throw new ArgumentException("type must be an enum, not " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(TEnum)));

			TEnum retval;
			return Enum.TryParse(s, true, out retval) ? retval : default(TEnum?);
		}

		public static IEnumerable<TEnum> TryParseLabel<TEnum>(string s, Taal taal) where TEnum : struct, IConvertible, IComparable
		{
			return EnumLabelLookup<TEnum>.Lookup(s, taal);
		}

		public static IEnumerable<Enum> TryParseLabel(Type enumType, string s, Taal taal)
		{
			if (!enumType.IsEnum)
				throw new ArgumentException("enumType must be an enum, not " + ObjectToCode.GetCSharpFriendlyTypeName(enumType));
			var parser = (ILabelLookup)Activator.CreateInstance(typeof(EnumLabelLookup<>).MakeGenericType(enumType));
			return parser.Lookup(s, taal);
		}

		public static bool IsDefined<TEnum>(TEnum enumval) where TEnum : struct, IConvertible, IComparable
		{
			return Enum.IsDefined(typeof(TEnum), enumval);
		}

		public static IEnumerable<T> AllCombinations<T>() where T : struct, IConvertible, IComparable
		{
			// Construct a function for OR-ing together two enums
			var orFunction = AddFlagsFunc<T>();

			var initalValues = (T[])Enum.GetValues(typeof(T));
			var discoveredCombinations = new HashSet<T>(initalValues);
			var queue = new Queue<T>(initalValues);

			// Try OR-ing every inital value to each value in the queue
			while (queue.Count > 0)
			{
				T a = queue.Dequeue();
				foreach (T b in initalValues)
				{
					T combo = orFunction(a, b);
					if (discoveredCombinations.Add(combo))
						queue.Enqueue(combo);
				}
			}

			return discoveredCombinations;
		}
	}
}
