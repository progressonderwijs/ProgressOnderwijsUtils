using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// Marker interface to easily allow the detection of convertible types.  Never implement this type specifically; use the fully-specified IMetaObjectPropertyConvertible with TModel, TProvider, and TConverterSource
    /// </summary>
    public interface IMetaObjectPropertyConvertible { }

    /// <summary>
    /// Marker interface to easily allow the detection of convertible types.  Never implement this type specifically; use the fully-specified IMetaObjectPropertyConvertible with TModel, TProvider, and TConverterSource
    /// </summary>
    /// <typeparam name="TProvider"></typeparam>
    // ReSharper disable once UnusedTypeParameter
    public interface IMetaObjectPropertyConvertible<TProvider> : IMetaObjectPropertyConvertible { }

    public interface IMetaObjectPropertyConvertible<TModel, TProvider, [UsedImplicitly] TConverterSource> : IMetaObjectPropertyConvertible<TProvider>
        where TConverterSource : struct, IConverterSource<TModel, TProvider>
        where TModel : struct, IMetaObjectPropertyConvertible<TModel, TProvider, TConverterSource> { }
}
