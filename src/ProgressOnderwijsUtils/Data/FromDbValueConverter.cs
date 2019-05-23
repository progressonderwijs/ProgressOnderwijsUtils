using System;
using System.Reflection;
using ExpressionToCodeLib;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class FromDbValueConverter
    {
        /// <summary>
        /// This method works just like a normal C# cast, with the following changed:
        ///  - it treats DBNull.Value as if it were null
        ///  - it doesn't support custom casts, just built-in casts
        ///  - it supports casting from boxed int to nullable enum.
        /// </summary>
        [Pure]
        public static T Cast<T>(object fromdatabase)
        {
            try {
                return FieldHelperClass<T>.Extractor(fromdatabase);
            } catch (Exception e) {
                var valStr =
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
            public static readonly Func<object, T> Extractor = GetExtractor(typeof(T));

            [NotNull]
            static Func<object, T> GetExtractor([NotNull] Type type)
            {
                var converter = MetaObjectPropertyConverter.GetOrNull(type);
                if (converter != null) {
                    return ExtractorFromConverter(type, converter);
                }
                if (!type.IsValueType) {
                    return obj => obj == DBNull.Value ? default(T) : (T)obj;
                }
                var nonnullableUnderlyingType = type.IfNullableGetNonNullableType();
                if (nonnullableUnderlyingType == null) {
                    return obj => (T)obj;
                }

                return (Func<object, T>)Delegate.CreateDelegate(typeof(Func<object, T>), extractNullableValueTypeMethod.MakeGenericMethod(nonnullableUnderlyingType));
            }

            static Func<object, T> ExtractorFromConverter(Type type, MetaObjectPropertyConverter converter)
            {
                if (type.IsNullableValueType() || !type.IsValueType) {
                    return obj => obj == DBNull.Value || obj == null ? default(T) : obj is T alreadyCast ? alreadyCast : (T)converter.ConvertFromDb(obj);
                } else {
                    return obj => obj == DBNull.Value || obj == null ? throw new InvalidCastException("Cannot convert null to " + type.ToCSharpFriendlyTypeName()) : (T)converter.ConvertFromDb(obj);
                }
            }
        }

        static readonly MethodInfo extractNullableValueTypeMethod = typeof(FromDbValueConverter).GetMethod(nameof(ExtractNullableValueType), BindingFlags.Static | BindingFlags.NonPublic);

        static TStruct? ExtractNullableValueType<TStruct>([CanBeNull] object obj)
            where TStruct : struct
            => obj == DBNull.Value || obj == null ? default(TStruct?) : (TStruct)obj;

        static readonly MethodInfo genericCastMethod = ((Func<object, int>)Cast<int>).Method.GetGenericMethodDefinition();

        [Pure]
        public static object DynamicCast(object val, Type type)
            => genericCastMethod.MakeGenericMethod(type).Invoke(null, new[] { val });

        [Pure]
        public static bool EqualsConvertingIntToEnum([CanBeNull] object val1, [CanBeNull] object val2)
        {
            if (val1 is Enum && val2 != null && !(val2 is Enum)) {
                return Equals(val1, Enum.ToObject(val1.GetType(), val2));
            }
            if (val1 != null && !(val1 is Enum) && val2 is Enum) {
                return Equals(Enum.ToObject(val2.GetType(), val1), val2);
            }
            return Equals(val1, val2);
        }
    }
}
