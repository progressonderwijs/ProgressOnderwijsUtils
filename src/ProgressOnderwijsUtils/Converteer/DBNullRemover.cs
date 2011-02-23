using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils.Converteer
{
	public static class DBNullRemover
	{
		public static T Cast<T>(object fromdatabase)
		{
			try
			{
				return FieldHelperClass<T>.Extractor(fromdatabase);
			}
			catch (Exception e)
			{
				string valStr =
					fromdatabase == null ? "<null>" :
					fromdatabase == DBNull.Value ? "<dbnull>"
					: fromdatabase.GetType().FullName + " value";
				throw new InvalidCastException("Cannot cast " + valStr + " to type " + typeof(T).FullName, e);
			}
		}

		/// <summary>
		/// This class is essentially a static lookup table to get the right cast-delegate from object to T.
		/// Previously, casting was simply done as:
		/// (T)(obj == DBNull.Value ? null : obj)
		/// That works for casting e.g. boxed int to int? or strings-as-objects to string,
		/// and it works for casting boxed int to Enums too - but *not* for casting boxed int to nullable enum;
		/// to get that to work, I needed this workaround.
		/// </summary>
		static class FieldHelperClass<T>
		{
			public static readonly Func<object, T> Extractor;
			static FieldHelperClass()
			{
				Type type = typeof(T);
				if (type.IsValueType)
				{
					Type nullableBase = type.GetNullableBaseType();
					if (nullableBase == null)
						Extractor = ExtractValueField;
					else if (!nullableBase.IsEnum)
						Extractor = ExtractClassOrNullableField;
					else
					{
						Extractor = (Func<object, T>)Delegate.CreateDelegate(typeof(Func<object, T>),
							typeof(FieldEnumHelperClass).GetMethod("ExtractNullableEnum").MakeGenericMethod(nullableBase));
					}
				}
				else
					Extractor = ExtractClassOrNullableField;

			}
			static T ExtractClassOrNullableField(object obj) { return obj == DBNull.Value ? default(T) : (T)obj; }
			static T ExtractValueField(object obj) { return (T)obj; }
			static class FieldEnumHelperClass
			{
				// ReSharper disable UnusedMember.Local
				public static TEnum? ExtractNullableEnum<TEnum>(object obj) where TEnum : struct
				// ReSharper restore UnusedMember.Local
				{
					return obj == DBNull.Value || obj == null ? default(TEnum?) : (TEnum)obj;
				}
			}
		}
	}
}
