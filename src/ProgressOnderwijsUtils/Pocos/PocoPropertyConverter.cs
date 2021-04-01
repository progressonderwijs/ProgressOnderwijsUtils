using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FastExpressionCompiler;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ProgressOnderwijsUtils
{
    public sealed class PocoPropertyConverter
    {
        public Type ModelType { get; }
        public Type DbType { get; }
        public Delegate CompiledConverterToDb { get; }
        public Delegate CompiledConverterFromDb { get; }
        public readonly Func<object?, object?> ConvertToDb;
        public readonly Func<object?, object?> ConvertFromDb;

        public PocoPropertyConverter(ValueConverter converter)
        {
            ModelType = converter.ModelClrType;
            DbType = converter.ProviderClrType;
            ConvertToDb = converter.ConvertToProvider;
            ConvertFromDb = converter.ConvertFromProvider;
            CompiledConverterToDb = converter.ConvertToProviderExpression.CompileFast();
            CompiledConverterFromDb = converter.ConvertFromProviderExpression.CompileFast();
        }

        static PocoPropertyConverter Define<TModel, TProvider>(
            Expression<Func<TModel, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TModel>> convertFromProviderExpression)
            => new(new ValueConverter<TModel, TProvider>(convertToProviderExpression, convertFromProviderExpression));

        static ValueConverter CreateValueConverter<TModel, TProvider, [UsedImplicitly] TConverterSource>()
            where TConverterSource : struct, IConverterSource<TModel, TProvider>
            where TModel : struct, IPocoConvertibleProperty<TModel, TProvider, TConverterSource>
            => new TConverterSource().GetValueConverter();

        internal static readonly MethodInfo CreateValueConverter_OpenGenericMethod = ((Func<ValueConverter>)CreateValueConverter<UnusedTypeTemplate1, int, UnusedTypeTemplate2>).Method.GetGenericMethodDefinition();

        struct UnusedTypeTemplate1 : IPocoConvertibleProperty<UnusedTypeTemplate1, int, UnusedTypeTemplate2> { }

        struct UnusedTypeTemplate2 : IConverterSource<UnusedTypeTemplate1, int>
        {
            public ValueConverter<UnusedTypeTemplate1, int> GetValueConverter()
                => throw new NotImplementedException();
        }

        static readonly ConcurrentDictionary<Type, PocoPropertyConverter?> propertyConverterCache = new();

        static readonly Func<Type, PocoPropertyConverter?> cachedFactoryDelegate = type => {
            if (type == typeof(ulong)) {
                return Define<ulong, byte[]>(codeVal => QueryScalarParameterComponent.UInt64ToSqlBinary(codeVal), dbVal => ParameterizedSqlObjectMapper.SqlBinaryToUInt64(dbVal));
            } else if (type == typeof(uint)) {
                return Define<uint, byte[]>(codeVal => QueryScalarParameterComponent.UInt32ToSqlBinary(codeVal), dbVal => ParameterizedSqlObjectMapper.SqlBinaryToUInt32(dbVal));
            }

            return type.GetNonNullableUnderlyingType()
                .GetInterfaces()
                .Where(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IPocoConvertibleProperty<,,>))
                .Select(interfaceType => {
                    var typeArgs = interfaceType.GenericTypeArguments;
                    var valueConverterFactoryMethodInfo = CreateValueConverter_OpenGenericMethod.MakeGenericMethod(typeArgs[0], typeArgs[1], typeArgs[2]);
                    var valueConverterFactory = valueConverterFactoryMethodInfo.CreateDelegate<Func<ValueConverter>>();
                    return new PocoPropertyConverter(valueConverterFactory());
                })
                .SingleOrNull();
        };

        public static PocoPropertyConverter? GetOrNull(Type propertyType)
            => propertyConverterCache.GetOrAdd(propertyType, cachedFactoryDelegate);
    }
}
