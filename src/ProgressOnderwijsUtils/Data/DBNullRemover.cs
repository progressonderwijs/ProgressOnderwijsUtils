using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProgressOnderwijsUtils
{
    public static class DBNullRemover
    {
        /// <summary>
        /// This method works just like a normal C# cast, with the following changed:
        ///  - it treats DBNull.Value as if it were null
        ///  - it doesn't support custom casts, just built-in casts
        ///  - it supports casting from boxed int to nullable enum.
        /// </summary>
        public static T Cast<T>(object fromdatabase)
        {
            try {
                return FieldHelperClass<T>.Extractor(fromdatabase);
            } catch (Exception e) {
                string valStr =
                    fromdatabase == null
                        ? "<null>"
                        : fromdatabase == DBNull.Value
                            ? "<dbnull>"
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
                if (type.IsValueType) {
                    Type nullableBase = type.IfNullableGetNonNullableType();
                    if (nullableBase == null) {
                        Extractor = ExtractValueField;
                    } else if (!nullableBase.IsValueType) {
                        Extractor = ExtractClassOrNullableField;
                    } else {
                        Extractor = (Func<object, T>)Delegate.CreateDelegate(
                            typeof(Func<object, T>),
                            typeof(DBNullRemover).GetMethod("ExtractNullableStruct", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(nullableBase));
                    }
                } else if (typeof(IIdentifier).IsAssignableFrom(type)) {
                    Extractor = ExtractIdentifier;
                } else {
                    Extractor = ExtractClassOrNullableField;
                }
            }

            static T ExtractClassOrNullableField(object obj) { return obj == DBNull.Value ? default(T) : (T)obj; }
            static T ExtractValueField(object obj) { return (T)obj; }

            static T ExtractIdentifier(object obj)
            {
                if (obj == DBNull.Value) {
                    return default(T);
                }

                if (obj == null) {
                    return default(T);
                }

                var r = (T)Activator.CreateInstance(typeof(T), null);
                var i = r as IIdentifier;
                // ReSharper disable PossibleNullReferenceException
                i.SetValue((int)obj);
                // ReSharper restore PossibleNullReferenceException

                return r;
            }
        }

        // ReSharper disable UnusedMember.Local
        static TStruct? ExtractNullableStruct<TStruct>(object obj) where TStruct : struct
            // ReSharper restore UnusedMember.Local
        { return obj == DBNull.Value || obj == null ? default(TStruct?) : (TStruct)obj; }
    }
}
