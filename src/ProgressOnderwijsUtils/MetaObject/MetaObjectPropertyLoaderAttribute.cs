using System;

// ReSharper disable once CheckNamespace
namespace ProgressOnderwijsUtils
{
    public sealed class MetaObjectPropertyLoaderAttribute : Attribute, IPropertiesAreUsedImplicitly { }
    public sealed class MetaObjectPropertyConvertibleAttribute : Attribute, IPropertiesAreUsedImplicitly { }
}
