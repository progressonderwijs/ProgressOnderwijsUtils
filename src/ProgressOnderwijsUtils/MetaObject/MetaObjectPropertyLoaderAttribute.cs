using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// ReSharper disable once CheckNamespace
namespace ProgressOnderwijsUtils
{
    public sealed class MetaObjectPropertyLoaderAttribute : Attribute, IPropertiesAreUsedImplicitly { }

    public interface IConverterSource<TModel, TProvider>
    {
        ValueConverter<TModel, TProvider> GetValueConverter();
    }
}
