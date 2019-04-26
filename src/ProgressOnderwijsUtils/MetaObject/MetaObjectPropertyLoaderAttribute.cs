using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// ReSharper disable once CheckNamespace
namespace ProgressOnderwijsUtils
{
    public sealed class MetaObjectPropertyLoaderAttribute : Attribute, IPropertiesAreUsedImplicitly { }

    public interface IConverterSource<TModel, TProvider>
    {
        ValueConverter<TModel, TProvider> GetValueConverter();
    }

    public interface IMetaObjectPropertyConvertible<TModel, TProvider, [UsedImplicitly] TConverterSource>
        where TConverterSource : struct, IConverterSource<TModel, TProvider>
        where TModel : struct, IMetaObjectPropertyConvertible<TModel, TProvider, TConverterSource>
    { }
}
