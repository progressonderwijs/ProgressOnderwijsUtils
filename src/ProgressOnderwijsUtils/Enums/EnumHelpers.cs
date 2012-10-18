using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

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
			TEnum retval;
			return Enum.TryParse(s, true, out retval) ? retval : default(TEnum?);
		}
	}
}
