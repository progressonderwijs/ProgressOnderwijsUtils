using System;
using System.Collections.Generic;
using System.Data;
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
			ITranslatable GetLabel(Enum val);
		}

		struct EnumMetaCache<TEnum> : IEnumValues where TEnum : struct, IConvertible
		{
			public static readonly TEnum[] EnumValues = (TEnum[])Enum.GetValues(typeof(TEnum));
			public static readonly Dictionary<TEnum, MemberInfo> EnumMembers = EnumValues.ToDictionary(v => v, v => typeof(TEnum).GetMember(v.ToString()).Single());
			public static readonly bool IsFlags = typeof(TEnum).GetCustomAttributes(typeof(FlagsAttribute)).Any();
			public static readonly Func<TEnum, TEnum, bool> HasFlag;
			public static readonly Func<TEnum, TEnum, TEnum> AddFlag;
			static readonly Type underlying = Enum.GetUnderlyingType(typeof(TEnum));

			static EnumMetaCache()
			{
				if (!typeof(TEnum).IsEnum)
					throw new InvalidOperationException("EnumMetaCache werkt alleen met enums");
				if (IsFlags)
				{
					HasFlag = MakeHasFlag();
					AddFlag = MakeAddFlag();
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

			//
			public static class AttrCache<TAttr> where TAttr : Attribute
			{
				public static readonly ILookup<TEnum, TAttr> EnumMemberAttributes =
					(
						from kv in EnumMembers
						from attr in kv.Value.GetCustomAttributes<TAttr>()
						select new { EnumValue = kv.Key, Attr = attr }
						).ToLookup(x => x.EnumValue, x => x.Attr);
			}


			public IReadOnlyList<Enum> Values() { return EnumValues.SelectIndexable(e => (Enum)(object)e); }
			public ITranslatable GetLabel(Enum val) { return GetLabel((TEnum)(object)val); }

			public static ITranslatable GetLabel(TEnum val)
			{
				if (IsFlags)
					return GetFlagsLabel(val);
				else
					return GetSingleLabel(val);
			}

			static ITranslatable GetFlagsLabel(TEnum val)
			{
				var matched = new List<TEnum>(EnumValues.Length);
				TEnum covered = default(TEnum);
				//decode flags like .NET: use as few flags as possible, greedily.
				int i = EnumValues.Length;
				while (i != 0)
				{
					i--;
					var flag = EnumValues[i];
					if (HasFlag(covered, flag) || !HasFlag(val, flag)) continue;
					covered = AddFlag(covered, flag);
					matched.Add(flag);
				}

				if (!Equals(covered, val))
					throw new ArgumentOutOfRangeException("Enum Value " + val + " is not a combination of flags in type " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(TEnum)));

				return matched.Select(GetSingleLabel).Reverse().JoinTexts(TextDefSimple.Create(", "));
			}

			static ITranslatable GetSingleLabel(TEnum f)
			{
				var label = GetAttrs<MpLabelAttribute>.On(f).SingleOrDefault();
				var tooltip = GetAttrs<MpTooltipAttribute>.On(f).SingleOrDefault();
				if (label == null && tooltip == null && !EnumMembers.ContainsKey(f))
					throw new ArgumentOutOfRangeException("Enum Value " + f + " does not exist in type " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(TEnum)));

				var translatable = label != null ? label.ToTranslatable()
					: Converteer.ToText(StringUtils.PrettyPrintCamelCased(f.ToString()));
				if (tooltip != null)
					translatable = translatable.ReplaceTooltipWithText(Translatable.Literal(tooltip.NL, tooltip.EN, tooltip.DE));

				return translatable;
			}
		}



		interface ILabelLookup
		{
			IEnumerable<Enum> Lookup(string s, Taal taal);
		}


		struct EnumLabelLookup<TEnum> : ILabelLookup where TEnum : struct, IConvertible
		{
			public static readonly Dictionary<Taal, ILookup<string, TEnum>> ParseLabels = GetValues<Taal>().ToDictionary(taal => taal, taal => GetValues<TEnum>().ToLookup(e => GetLabel(e).Translate(taal).Text, e => e, StringComparer.OrdinalIgnoreCase));
			public static IEnumerable<TEnum> Lookup(string s, Taal taal) { return ParseLabels[taal][s]; }
			IEnumerable<Enum> ILabelLookup.Lookup(string s, Taal taal) { return Lookup(s, taal).Select(e => (Enum)(object)e); }
		}


		public static class GetAttrs<TAttr> where TAttr : Attribute
		{
			public static IEnumerable<TAttr> On<T>(T enumVal) where T : struct, IConvertible { return EnumMetaCache<T>.AttrCache<TAttr>.EnumMemberAttributes[enumVal]; }
		}


		public static IReadOnlyList<T> GetValues<T>() where T : struct, IConvertible { return EnumMetaCache<T>.EnumValues; }

		public static IReadOnlyList<Enum> GetValues(Type enumType)
		{
			if (!enumType.IsEnum)
				throw new ArgumentException("enumType must be an enum, not " + ObjectToCode.GetCSharpFriendlyTypeName(enumType));
			var values = (IEnumValues)Activator.CreateInstance(typeof(EnumMetaCache<>).MakeGenericType(enumType));
			return values.Values();
		}

		public static Func<TEnum, TEnum, TEnum> AddFlagsFunc<TEnum>() where TEnum : struct,IConvertible { return EnumMetaCache<TEnum>.AddFlag; }
		public static Func<TEnum, TEnum, bool> HasFlagsFunc<TEnum>() where TEnum : struct,IConvertible { return EnumMetaCache<TEnum>.HasFlag; }

		public static ITranslatable GetLabel<TEnum>(TEnum f) where TEnum : struct, IConvertible { return EnumMetaCache<TEnum>.GetLabel(f); }

		public static ITranslatable GetLabel(Enum enumVal)
		{
			if (enumVal == null)
				throw new ArgumentNullException("enumVal");
			var type = enumVal.GetType();
			if (!type.IsEnum)
				throw new ArgumentException("enumVal must be an enum value, not of type " + ObjectToCode.GetCSharpFriendlyTypeName(type));
			var labeller = (IEnumValues)Activator.CreateInstance(typeof(EnumMetaCache<>).MakeGenericType(type));
			return labeller.GetLabel(enumVal);
		}

		public static SelectItem<TEnum> GetSelectItem<TEnum>(TEnum f)
			where TEnum : struct, IConvertible
		{
			return SelectItem.Create(f, GetLabel(f));
		}



		public static DataTable ToIntKoppelTabel<TEnum>(IEnumerable<TEnum> values, Taal taal)
			where TEnum : struct,IConvertible
		{
			var dt = new DataTable { Columns = { { "id", typeof(int) }, { "tekst", typeof(string) } } };
			foreach (var item in values.Select(GetSelectItem))
				dt.Rows.Add(item.Value.ToInt32(null), item.Label.Translate(taal).Text);
			return dt;
		}

		public static SelectItem<TEnum?> GetSelectItem<TEnum>(TEnum? f)
			where TEnum : struct, IConvertible
		{
			return SelectItem.Create(f, f == null ? TextDefSimple.EmptyText : GetLabel(f.Value));
		}


		public static TEnum? TryParse<TEnum>(string s) where TEnum : struct, IConvertible
		{
			if (!typeof(TEnum).IsEnum)
				throw new ArgumentException("type must be an enum, not " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(TEnum)));

			TEnum retval;
			return Enum.TryParse(s, true, out retval) ? retval : default(TEnum?);
		}

		public static IEnumerable<TEnum> TryParseLabel<TEnum>(string s, Taal taal) where TEnum : struct, IConvertible { return EnumLabelLookup<TEnum>.Lookup(s, taal); }

		public static IEnumerable<Enum> TryParseLabel(Type enumType, string s, Taal taal)
		{
			if (!enumType.IsEnum)
				throw new ArgumentException("enumType must be an enum, not " + ObjectToCode.GetCSharpFriendlyTypeName(enumType));
			var parser = (ILabelLookup)Activator.CreateInstance(typeof(EnumLabelLookup<>).MakeGenericType(enumType));
			return parser.Lookup(s, taal);
		}

		public static bool IsDefined<TEnum>(TEnum enumval) where TEnum : struct,IConvertible
		{
			return Enum.IsDefined(typeof(TEnum), enumval);
		}
	}
}