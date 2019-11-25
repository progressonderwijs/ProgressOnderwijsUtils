using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
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

        public PocoPropertyConverter(Type converterDefinition)
        {
            ModelType = converterDefinition.GenericTypeArguments[0];
            DbType = converterDefinition.GenericTypeArguments[1];
            var converterType = converterDefinition.GenericTypeArguments[2];
            var converter = ((Func<ValueConverter>)CreateValueConverter_OpenGenericMethod.MakeGenericMethod(ModelType, DbType, converterType).CreateDelegate(typeof(Func<ValueConverter>)))();

            ConvertToDb = converter.ConvertToProvider;
            ConvertFromDb = converter.ConvertFromProvider;
            CompiledConverterToDb = converter.ConvertToProviderExpression.Compile();
            CompiledConverterFromDb = converter.ConvertFromProviderExpression.Compile();
        }

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

        static readonly ConcurrentDictionary<Type, PocoPropertyConverter> propertyConverterCache = new ConcurrentDictionary<Type, PocoPropertyConverter>();

        static readonly Func<Type, PocoPropertyConverter> cachedFactoryDelegate = type =>
            type.GetNonNullableUnderlyingType()
                .GetInterfaces()
                .Where(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IPocoConvertibleProperty<,,>))
                .Select(i => new PocoPropertyConverter(i))
                .SingleOrNull();

        public static PocoPropertyConverter GetOrNull(Type propertyType)
            => propertyConverterCache.GetOrAdd(propertyType, cachedFactoryDelegate);
    }
}
