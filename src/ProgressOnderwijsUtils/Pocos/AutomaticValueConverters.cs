using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ProgressOnderwijsUtils
{
    public static class AutomaticValueConverters
    {
        static ValueConverter<TModel, TProvider> Define<TModel, TProvider>(
            Expression<Func<TModel, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TModel>> convertFromProviderExpression)
            => new(convertToProviderExpression, convertFromProviderExpression);

        static ValueConverter CreateValueConverter<TModel, TProvider, [UsedImplicitly] TConverterSource>()
            where TConverterSource : struct, IValueConverterSource<TModel, TProvider>
            where TModel : struct, IHasValueConverter<TModel, TProvider, TConverterSource>
            => new TConverterSource().GetValueConverter();

        internal static readonly MethodInfo CreateValueConverter_OpenGenericMethod = ((Func<ValueConverter>)CreateValueConverter<UnusedTypeTemplate1, int, UnusedTypeTemplate2>).Method.GetGenericMethodDefinition();

        static ValueConverter<TEnum, TProvider> LiftToEnum<TEnum, TOrigModel, TProvider>(ValueConverter<TOrigModel, TProvider> baseConverter)
        {
            var newModelArg = Expression.Parameter(typeof(TEnum), "modelVal");
            var origModelArg = baseConverter.ConvertToProviderExpression.Parameters.Single();
            var newToProviderBody = ReplacingExpressionVisitor.Replace(origModelArg, Expression.Convert(newModelArg, origModelArg.Type), baseConverter.ConvertToProviderExpression.Body);
            var toProviderLambda = Expression.Lambda<Func<TEnum, TProvider>>(newToProviderBody, newModelArg);
            var fromProviderLambda = Expression.Lambda<Func<TProvider, TEnum>>(Expression.Convert(baseConverter.ConvertFromProviderExpression.Body, typeof(TEnum)), baseConverter.ConvertFromProviderExpression.Parameters.Single());
            return new(toProviderLambda, fromProviderLambda);
        }

        internal static readonly MethodInfo LiftToEnum_OpenGenericMethod = ((Func<ValueConverter<int, int>, ValueConverter<int, int>>)LiftToEnum<int, int, int>).Method.GetGenericMethodDefinition();

        struct UnusedTypeTemplate1 : IHasValueConverter<UnusedTypeTemplate1, int, UnusedTypeTemplate2> { }

        struct UnusedTypeTemplate2 : IValueConverterSource<UnusedTypeTemplate1, int>
        {
            public ValueConverter<UnusedTypeTemplate1, int> GetValueConverter()
                => throw new NotImplementedException();
        }

        static readonly ValueConverter<ulong, byte[]> ulongConverter = Define<ulong, byte[]>(codeVal => QueryScalarParameterComponent.UInt64ToSqlBinary(codeVal), dbVal => ParameterizedSqlObjectMapper.SqlBinaryToUInt64(dbVal));
        static readonly ValueConverter<uint, byte[]> uintConverter = Define<uint, byte[]>(codeVal => QueryScalarParameterComponent.UInt32ToSqlBinary(codeVal), dbVal => ParameterizedSqlObjectMapper.SqlBinaryToUInt32(dbVal));
        static readonly ValueConverter<int, int> intPassThroughConverter = Define<int, int>(codeVal => codeVal, dbVal => dbVal);
        static readonly ConcurrentDictionary<Type, ValueConverter?> propertyConverterCache = new();

        static readonly Func<Type, ValueConverter?> cachedFactoryDelegate = type => {
            if (GetLiftedNullableConverter(type) is { } valueConverter) {
                return valueConverter;
            } else if (type == typeof(ulong)) {
                return ulongConverter;
            } else if (type == typeof(uint)) {
                return uintConverter;
            } else if (type.IsEnum) {
                var underlyingType = type.GetEnumUnderlyingType();
                if (underlyingType == typeof(ulong)) {
                    return (ValueConverter)LiftToEnum_OpenGenericMethod.MakeGenericMethod(type, underlyingType, ulongConverter.ProviderClrType).Invoke(null, new object[] { ulongConverter }).AssertNotNull();
                } else if (underlyingType == typeof(uint)) {
                    return (ValueConverter)LiftToEnum_OpenGenericMethod.MakeGenericMethod(type, underlyingType, uintConverter.ProviderClrType).Invoke(null, new object[] { uintConverter }).AssertNotNull();
                } else if (underlyingType == typeof(int)) {
                    return (ValueConverter)LiftToEnum_OpenGenericMethod.MakeGenericMethod(type, underlyingType, intPassThroughConverter.ProviderClrType).Invoke(null, new object[] { intPassThroughConverter }).AssertNotNull();
                }
            }

            return type
                .GetInterfaces()
                .Where(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IHasValueConverter<,,>))
                .Select(
                    interfaceType => {
                        var typeArgs = interfaceType.GenericTypeArguments;
                        var valueConverterFactoryMethodInfo = CreateValueConverter_OpenGenericMethod.MakeGenericMethod(typeArgs[0], typeArgs[1], typeArgs[2]);
                        var valueConverterFactory = valueConverterFactoryMethodInfo.CreateDelegate<Func<ValueConverter>>();
                        return valueConverterFactory();
                    }
                )
                .SingleOrNull();
        };

        static ValueConverter? GetLiftedNullableConverter(Type nullableModelType)
        {
            if (nullableModelType.IfNullableGetNonNullableType() is not { } nonNullableModelType || GetOrNull(nonNullableModelType) is not { } nonNullableValueTypeConverter) {
                return null;
            }
            var providerType = nonNullableValueTypeConverter.ProviderClrType;
            var possiblyNullableProviderType = providerType.MakeNullableType() ?? providerType;

            var nullableModelArg = Expression.Parameter(nullableModelType, "nullableModelVal");
            var origModelArg = nonNullableValueTypeConverter.ConvertToProviderExpression.Parameters.Single();
            var nullableModelToProviderBody = Expression.Condition(
                IsExpressionNonNull(nullableModelArg),
                Expression.Convert(ReplacingExpressionVisitor.Replace(origModelArg, Expression.Convert(nullableModelArg, origModelArg.Type), nonNullableValueTypeConverter.ConvertToProviderExpression.Body), possiblyNullableProviderType),
                Expression.Default(possiblyNullableProviderType)
            );
            var modelToProviderFuncType = typeof(Func<,>).MakeGenericType(nullableModelType, possiblyNullableProviderType);
            var modelToProviderLambda = Expression.Lambda(modelToProviderFuncType, nullableModelToProviderBody, nullableModelArg);

            var nullableProviderArg = Expression.Parameter(possiblyNullableProviderType, "nullableProviderVal");
            var origProviderArg = nonNullableValueTypeConverter.ConvertFromProviderExpression.Parameters.Single();
            var nullableProviderToModelBody = Expression.Condition(
                IsExpressionNonNull(nullableProviderArg),
                Expression.Convert(ReplacingExpressionVisitor.Replace(origProviderArg, Expression.Convert(nullableProviderArg, origProviderArg.Type), nonNullableValueTypeConverter.ConvertFromProviderExpression.Body), nullableModelType),
                Expression.Default(nullableModelType)
            );
            var providerToModelFuncType = typeof(Func<,>).MakeGenericType(possiblyNullableProviderType, nullableModelType);
            var providerToModelLambda = Expression.Lambda(providerToModelFuncType, nullableProviderToModelBody, nullableProviderArg);

            return (ValueConverter)Activator.CreateInstance(typeof(ValueConverter<,>).MakeGenericType(nullableModelType, possiblyNullableProviderType), modelToProviderLambda, providerToModelLambda, null).AssertNotNull();
        }

        public static ValueConverter? GetOrNull(Type propertyType)
            => propertyConverterCache.GetOrAdd(propertyType, cachedFactoryDelegate);

        public static Expression IsExpressionNonNull(Expression propertyValue)
            => propertyValue.Type.IsNullableValueType()
                ? Expression.Property(propertyValue, nameof(Nullable<int>.HasValue))
                : propertyValue.Type.IsValueType
                    ? Expression.Constant(false)
                    : Expression.NotEqual(Expression.Default(typeof(object)), Expression.Convert(propertyValue, typeof(object)));
    }
}
