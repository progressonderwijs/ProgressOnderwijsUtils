namespace ProgressOnderwijsUtils;

/// <summary>
/// Marker interface to easily allow the detection of convertible types.  Never implement this type specifically; use the fully-specified IPocoConvertibleProperty with TModel, TProvider, and TConverterSource
/// </summary>
public interface IHasValueConverter { }

/// <summary>
/// Marker interface to easily allow the detection of convertible types.  Never implement this type specifically; use the fully-specified IPocoConvertibleProperty with TModel, TProvider, and TConverterSource
/// </summary>
/// <typeparam name="TProvider"></typeparam>
// ReSharper disable once UnusedTypeParameter
public interface IHasValueConverter<TProvider> : IHasValueConverter { }

public interface IHasValueConverter<TModel, TProvider, [UsedImplicitly] TValueConverterSource> : IHasValueConverter<TProvider>
    where TValueConverterSource : struct, IValueConverterSource<TModel, TProvider>
    where TModel : IHasValueConverter<TModel, TProvider, TValueConverterSource> { }