using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// Marker interface to easily allow the detection of convertible types.  Never implement this type specifically; use the fully-specified IPocoConvertibleProperty with TModel, TProvider, and TConverterSource
    /// </summary>
    public interface IPocoConvertibleProperty { }

    /// <summary>
    /// Marker interface to easily allow the detection of convertible types.  Never implement this type specifically; use the fully-specified IPocoConvertibleProperty with TModel, TProvider, and TConverterSource
    /// </summary>
    /// <typeparam name="TProvider"></typeparam>
    // ReSharper disable once UnusedTypeParameter
    public interface IPocoConvertibleProperty<TProvider> : IPocoConvertibleProperty { }

    public interface IPocoConvertibleProperty<TModel, TProvider, [UsedImplicitly] TConverterSource> : IPocoConvertibleProperty<TProvider>
        where TConverterSource : struct, IConverterSource<TModel, TProvider>
        where TModel : struct, IPocoConvertibleProperty<TModel, TProvider, TConverterSource> { }
}
