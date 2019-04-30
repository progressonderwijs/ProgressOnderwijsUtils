using System;
using System.Linq;
using System.Reflection;
using FastExpressionCompiler;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ProgressOnderwijsUtils
{
    sealed class MetaObjectPropertyConverter
    {
        public Type ModelType { get; }
        public Type DbType { get; }
        public Delegate CompiledConverter { get; }

        public MetaObjectPropertyConverter(Type converterDefinition)
        {
            ModelType = converterDefinition.GenericTypeArguments[0];
            DbType = converterDefinition.GenericTypeArguments[1];
            var converterType = converterDefinition.GenericTypeArguments[2];
            var converter = ((Func<ValueConverter>)CreateValueConverter_OpenGenericMethod.MakeGenericMethod(ModelType, DbType, converterType).CreateDelegate(typeof(Func<ValueConverter>)))();
            CompiledConverter = converter.ConvertToProviderExpression.CompileFast();
        }

        static ValueConverter CreateValueConverter<TModel, TProvider, [UsedImplicitly] TConverterSource>()
            where TConverterSource : struct, IConverterSource<TModel, TProvider>
            where TModel : struct, IMetaObjectPropertyConvertible<TModel, TProvider, TConverterSource>
            => new TConverterSource().GetValueConverter();

        internal static readonly MethodInfo CreateValueConverter_OpenGenericMethod = ((Func<ValueConverter>)CreateValueConverter<UnusedTypeTemplate1, int, UnusedTypeTemplate2>).Method.GetGenericMethodDefinition();

        struct UnusedTypeTemplate1 : IMetaObjectPropertyConvertible<UnusedTypeTemplate1, int, UnusedTypeTemplate2> { }

        struct UnusedTypeTemplate2 : IConverterSource<UnusedTypeTemplate1, int>
        {
            public ValueConverter<UnusedTypeTemplate1, int> GetValueConverter()
                => throw new NotImplementedException();
        }

        public static MetaObjectPropertyConverter GetOrNull(Type type)
            => type
                .GetInterfaces()
                .Where(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IMetaObjectPropertyConvertible<,,>))
                .Select(i => new MetaObjectPropertyConverter(i))
                .SingleOrNull();
    }
}
