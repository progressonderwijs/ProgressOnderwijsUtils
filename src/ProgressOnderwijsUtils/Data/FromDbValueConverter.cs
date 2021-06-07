using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ProgressOnderwijsUtils
{
    public static class DbValueConverter
    {
        /// <summary>
        /// This method works just like a normal C# cast, with the following changed:
        /// - it treats DBNull.Value as if it were null
        /// - it ignores explicit and implicit cast operators
        /// - it supports casting from boxed int to nullable enum.
        /// - it supports casting fromDb when the target type is <see cref="IHasValueConverter"/>
        /// </summary>
        [Pure]
        [return: MaybeNull]
        public static T FromDb<T>(object? valueFromDb)
        {
            try {
                return FromDbHelper<T>.Convert(valueFromDb == DBNull.Value ? null : valueFromDb);
            } catch (Exception e) {
                var valTypeString = valueFromDb?.GetType().ToCSharpFriendlyTypeName() ?? "<null>";
                throw new InvalidCastException("Cannot cast " + valTypeString + " to type " + typeof(T).ToCSharpFriendlyTypeName(), e);
            }
        }

        /// <summary>
        /// This method works just like a normal C# cast, with the following changed:
        /// - it ignores explicit and implicit cast operators
        /// - it supports casting from boxed int to nullable enum.
        /// - it supports casting ToDb when the passed value is <see cref="IHasValueConverter"/>.
        /// </summary>
        [Pure]
        [return: NotNullIfNotNull("valueFromCode")]
        public static T ToDb<T>(object? valueFromCode)
        {
            try {
                return ToDbHelper<T>.Convert(valueFromCode);
            } catch (Exception e) {
                var valTypeString = valueFromCode?.GetType().ToCSharpFriendlyTypeName() ?? "<null>";
                throw new InvalidCastException("Cannot cast " + valTypeString + " to type " + typeof(T).ToCSharpFriendlyTypeName(), e);
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
        static class FromDbHelper<T>
        {
            public static readonly Func<object?, T> Convert = MakeConverter(typeof(T));

            static Func<object?, T> MakeConverter(Type type)
            {
                var converter = AutomaticValueConverters.GetOrNull(type);
                if (converter != null) {
                    return ForConvertible(type, converter);
                }
                if (!type.IsValueType) {
                    return obj => (T)obj!;
                }
                var nonnullableUnderlyingType = type.IfNullableGetNonNullableType();
                if (nonnullableUnderlyingType == null) {
                    return obj => (T)obj!;
                }

                return (Func<object?, T>)Delegate.CreateDelegate(typeof(Func<object, T>), extractNullableValueTypeMethod.MakeGenericMethod(nonnullableUnderlyingType));
            }

            static Func<object?, T> ForConvertible(Type type, ValueConverter converter)
            {
                if (type.IsNullableValueType() || !type.IsValueType) {
                    return obj => obj == null
                        ? default(T)!
                        : obj is T alreadyCast
                            ? alreadyCast
                            : (T)converter.ConvertFromProvider(obj);
                } else {
                    return obj => obj == null ? throw new InvalidCastException("Cannot convert null to " + type.ToCSharpFriendlyTypeName()) : (T)converter.ConvertFromProvider(obj);
                }
            }
        }

        static class ToDbHelper<T>
        {
            public static readonly Func<object?, T> Convert = MakeConverter(typeof(T));

            static Func<object?, T> MakeConverter(Type type)
            {
                if (!type.IsValueType) {
                    return obj => obj == null
                        ? default(T)!
                        : AutomaticValueConverters.GetOrNull(obj.GetType()) is { } converter
                            ? (T)converter.ConvertToProvider(obj)
                            : (T)obj;
                }
                var nonnullableUnderlyingType = type.IfNullableGetNonNullableType();
                if (nonnullableUnderlyingType == null) {
                    return obj =>
                        obj is null
                            ? throw new InvalidOperationException("Cannot convert null to non-nullable value type " + type.FriendlyName())
                            : AutomaticValueConverters.GetOrNull(obj.GetType()) is { } converter
                                ? (T)converter.ConvertToProvider(obj)
                                : (T)obj;
                }
                return obj =>
                    obj is null
                    ? default(T)!
                    : AutomaticValueConverters.GetOrNull(obj.GetType()) is { } converter
                        ? (T)converter.ConvertToProvider(obj)
                        : (T)obj;
            }
        }

        static readonly MethodInfo extractNullableValueTypeMethod = typeof(DbValueConverter).GetMethod(nameof(ExtractNullableValueType), BindingFlags.Static | BindingFlags.NonPublic).AssertNotNull();

        static TStruct? ExtractNullableValueType<TStruct>(object? obj)
            where TStruct : struct
        // ReSharper disable once MergeConditionalExpression //does not work for enums!
            => obj == null ? default(TStruct?) : (TStruct)obj;

        static readonly MethodInfo genericCastMethod = ((Func<object, int>)FromDb<int>).Method.GetGenericMethodDefinition();

        [Pure]
        public static object? DynamicCast(object? val, Type type)
        {
            if (type.IsInstanceOfType(val)) {
                return val;
            } else if (val == null || val == DBNull.Value) {
                if (type.IsValueType && !type.IsNullableValueType()) {
                    throw new InvalidCastException("Cannot cast (db)null to " + type.ToCSharpFriendlyTypeName());
                }
                return null;
            } else if (AutomaticValueConverters.GetOrNull(type) is { } targetConvertible) {
                return targetConvertible.ConvertFromProvider(val);
            } else if (AutomaticValueConverters.GetOrNull(val.GetType()) is { } sourceConvertible && sourceConvertible.ProviderClrType == type.GetNonNullableType()) {
                return sourceConvertible.ConvertToProvider(val);
            } else {
                return genericCastMethod.MakeGenericMethod(type).Invoke(null, new[] { val });
            }
        }

        [Pure]
        public static bool EqualsConvertingIntToEnum(object? val1, object? val2)
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
