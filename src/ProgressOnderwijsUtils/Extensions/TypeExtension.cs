using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public static class TypeExtension
	{
		/// <summary>
		/// If type is Nullable&lt;T&gt;, returns typeof(T).  For non-Nullable&lt;&gt; types, returns null;
		/// </summary>
		public static Type GetNullableBaseType(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
				? type.GetGenericArguments()[0]
				: null;
		}

		public static bool CanBeNull(this Type type)
		{
			return !type.IsValueType || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}
		public static IEnumerable<Type> BaseTypes(this Type type)
		{
			if (null == type) yield break;
			var baseType = type.BaseType;
			while (baseType != null)
			{
				yield return baseType;
				baseType = baseType.BaseType;
			}
		}
	}
}
