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
    public static class PocoPropertyConverter
    {
        static ValueConverter<TModel, TProvider> Define<TModel, TProvider>(
            Expression<Func<TModel, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TModel>> convertFromProviderExpression)
            => new(convertToProviderExpression, convertFromProviderExpression);

        static ValueConverter CreateValueConverter<TModel, TProvider, [UsedImplicitly] TConverterSource>()
            where TConverterSource : struct, IConverterSource<TModel, TProvider>
            where TModel : struct, IPocoConvertibleProperty<TModel, TProvider, TConverterSource>
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

        struct UnusedTypeTemplate1 : IPocoConvertibleProperty<UnusedTypeTemplate1, int, UnusedTypeTemplate2> { }

        struct UnusedTypeTemplate2 : IConverterSource<UnusedTypeTemplate1, int>
        {
            public ValueConverter<UnusedTypeTemplate1, int> GetValueConverter()
                => throw new NotImplementedException();
        }

        static readonly ValueConverter<ulong, byte[]> ulongConverter = Define<ulong, byte[]>(codeVal => QueryScalarParameterComponent.UInt64ToSqlBinary(codeVal), dbVal => ParameterizedSqlObjectMapper.SqlBinaryToUInt64(dbVal));
        static readonly ValueConverter<uint, byte[]> uintConverter = Define<uint, byte[]>(codeVal => QueryScalarParameterComponent.UInt32ToSqlBinary(codeVal), dbVal => ParameterizedSqlObjectMapper.SqlBinaryToUInt32(dbVal));
        static readonly ConcurrentDictionary<Type, ValueConverter?> propertyConverterCache = new();

        static readonly Func<Type, ValueConverter?> cachedFactoryDelegate = type => {
            if (type == typeof(ulong)) {
                return ulongConverter;
            } else if (type == typeof(uint)) {
                return uintConverter;
            } else if (type.IsEnum) {
                var underlyingType = type.GetEnumUnderlyingType();
                if (underlyingType == typeof(ulong)) {
                    return (ValueConverter)LiftToEnum_OpenGenericMethod.MakeGenericMethod(type, underlyingType, ulongConverter.ProviderClrType).Invoke(null, new object[] { ulongConverter }).AssertNotNull();
                } else if (underlyingType == typeof(uint)) {
                    return (ValueConverter)LiftToEnum_OpenGenericMethod.MakeGenericMethod(type, underlyingType, uintConverter.ProviderClrType).Invoke(null, new object[] { uintConverter }).AssertNotNull();
                }
            }

            return type.GetNonNullableUnderlyingType()
                .GetInterfaces()
                .Where(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IPocoConvertibleProperty<,,>))
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

        public static ValueConverter? GetOrNull(Type propertyType)
            => propertyConverterCache.GetOrAdd(propertyType, cachedFactoryDelegate);
    }
}
