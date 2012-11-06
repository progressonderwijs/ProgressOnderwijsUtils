using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
	public static class EnumHelpers
	{
		static class EnumMetaCache<TEnum> where TEnum : struct
		{
			public static readonly ReadOnlyCollection<TEnum> EnumValues = new ReadOnlyCollection<TEnum>((TEnum[])Enum.GetValues(typeof(TEnum)));
			public static readonly Dictionary<TEnum, MemberInfo> EnumMembers = EnumValues.ToDictionary(v => v, v => typeof(TEnum).GetMember(v.ToString()).Single());

			public static class AttrCache<TAttr> where TAttr : Attribute
			{
				public static readonly ILookup<TEnum, TAttr> EnumMemberAttributes =
					(
						from kv in EnumMembers
						from attr in kv.Value.GetCustomAttributes<TAttr>()
						select new { EnumValue = kv.Key, Attr = attr }
						).ToLookup(x => x.EnumValue, x => x.Attr);
			}
		}
		interface ILabelLookup
		{
			IEnumerable<Enum> Lookup(string s, Taal taal);
		}
		sealed class EnumLabelLookup<TEnum> : ILabelLookup where TEnum : struct
		{
			public static readonly Dictionary<Taal, ILookup<string, TEnum>> ParseLabels = GetValues<Taal>().ToDictionary(taal => taal, taal => GetValues<TEnum>().ToLookup(e => GetLabel(e).Translate(taal).Text, e => e, StringComparer.OrdinalIgnoreCase));
			public static IEnumerable<TEnum> Lookup(string s, Taal taal) { return ParseLabels[taal][s]; }
			IEnumerable<Enum> ILabelLookup.Lookup(string s, Taal taal)
			{
				return Lookup(s, taal).Select(e => (Enum)(object)e);
			}
		}


		public static class GetAttrs<TAttr> where TAttr : Attribute
		{
			public static IEnumerable<TAttr> On<T>(T enumVal) where T : struct
			{
				return EnumMetaCache<T>.AttrCache<TAttr>.EnumMemberAttributes[enumVal];
			}
		}

		public static ReadOnlyCollection<T> GetValues<T>() where T : struct
		{
			return EnumMetaCache<T>.EnumValues;
		}

		public static TEnum? TryParse<TEnum>(string s) where TEnum : struct
		{
			if (!typeof(TEnum).IsEnum)
				throw new ArgumentException("type must be an enum, not " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(TEnum)));

			TEnum retval;
			return Enum.TryParse(s, true, out retval) ? retval : default(TEnum?);
		}

		public static IEnumerable<TEnum> TryParseLabel<TEnum>(string s, Taal taal) where TEnum : struct
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


		public static ITranslatable GetLabel<TEnum>(TEnum f) where TEnum : struct
		{

			var label = GetAttrs<MpLabelAttribute>.On(f).SingleOrDefault();
			var tooltip = GetAttrs<MpTooltipAttribute>.On(f).SingleOrDefault();
			if (label == null && tooltip == null && !EnumMetaCache<TEnum>.EnumMembers.ContainsKey(f))
				throw new ArgumentOutOfRangeException("Enum Value " + f + " does not exist in type " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(TEnum)));

			var translatable = label != null ? label.ToTranslatable()
				: Converteer.ToText(StringUtils.PrettyPrintCamelCased(f.ToString()));
			if (tooltip != null)
				translatable = translatable.ReplaceTooltipWithText(Translatable.Literal(tooltip.NL, tooltip.EN, tooltip.DE));

			return translatable;
		}


		public static SelectItem<TEnum> GetSelectItem<TEnum>(TEnum f) where TEnum : struct { return SelectItem.Create(f, GetLabel(f)); }


	}
}
