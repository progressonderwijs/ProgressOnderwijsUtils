﻿using System;
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


		/// <summary>
		/// If type is non-Nullable value type T, returns typeof(Nullable&lt;T&gt;).  For Nullable&lt;&gt; or reference types, returns null;
		/// </summary>
		public static Type MakeNullableType(this Type type)
		{
			return !type.IsValueType || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
				? null
				: typeof(Nullable<>).MakeGenericType(type);
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

		public static string GetNonGenericName(this Type type)
		{
			var typename = type.FullName;
			// ReSharper disable PossibleNullReferenceException
			int backtickIdx = typename.IndexOf('`');
			// ReSharper restore PossibleNullReferenceException
			return backtickIdx == -1 ? typename : typename.Substring(0, backtickIdx);
		}
	}
}
