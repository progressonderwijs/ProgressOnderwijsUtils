using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils.Converteer
{
	public static class SmartUnbox
	{
		/// <summary>
		/// This method works just like a normal C# cast, with the following changed:
		///  - it doesn't support custom casts, just built-in casts
		///  - it supports casting from boxed int to nullable enum.
		/// </summary>
		public static T Cast<T>(object obj)
		{
			try
			{
				return UnboxHelperClass<T>.Extractor(obj);
			}
			catch (Exception e)
			{
				string valStr =
					obj == null ? "<null>" :
					obj == DBNull.Value ? "<dbnull>"
					: obj.GetType().FullName + " value";
				throw new InvalidCastException("Cannot cast " + valStr + " to type " + typeof(T).FullName, e);
			}
		}

		static class UnboxHelperClass<T>
		{
			public static readonly Func<object, T> Extractor;
			static UnboxHelperClass()
			{
				Type type = typeof(T);
				Type nullableBase = type.GetNullableBaseType();
				if (nullableBase != null && nullableBase.IsEnum)
				{
					Extractor = (Func<object, T>)Delegate.CreateDelegate(typeof(Func<object, T>),
																		 typeof(SmartUnbox).GetMethod("ExtractNullableEnum", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(nullableBase));
				}
				else
				{
					Extractor = ExtractNormalTypes;
				}
			}
			static T ExtractNormalTypes(object obj) { return (T)obj; }
		}

		// ReSharper disable UnusedMember.Local
		static TEnum? ExtractNullableEnum<TEnum>(object obj) where TEnum : struct
		// ReSharper restore UnusedMember.Local
		{
			return obj == null ? default(TEnum?) : (TEnum)obj;
		}
	}
}
