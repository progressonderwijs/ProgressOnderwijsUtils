﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	public static class TypeExtension
	{ 
		/// <summary>
		/// If type is Nullable&lt;T&gt;, returns typeof(T).  For non-Nullable&lt;&gt; types, returns null;
		/// </summary>
		public static Type IfNullableGetNonNullableType(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
				? type.GetGenericArguments()[0]
				: null;
		}

		/// <summary>
		/// If type is Nullable&lt;T&gt;, returns typeof(T).  For non-Nullable&lt;&gt; types, returns the type itself - this might also be a reference type, so the resulting type may still permit the value null.
		/// </summary>
		public static Type GetNonNullableType(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
				? type.GetGenericArguments()[0]
				: type;
		}

		/// <summary>
		/// For enums, nullable types and nullable enums, return non-nullable underlying type; otherwise return original type.
		/// e.g. typeof(MyEnum?) => typeof(int)
		/// e.g. typeof(string) => typeof(string)
		/// 
		/// </summary>
		public static Type GetNonNullableUnderlyingType(this Type type)
		{
			var nonNullableType = type.GetNonNullableType();
			if (nonNullableType.IsEnum)
				return nonNullableType.GetEnumUnderlyingType();
			if (typeof(IIdentifier).IsAssignableFrom(type.BaseType))
				return Identifier.BaseType;
			return nonNullableType;
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
