using JetBrains.Annotations;

namespace ProgressOnderwijsUtils {
    public interface IMetaObjectPropertyConvertible<TModel, TProvider, [UsedImplicitly] TConverterSource>
        where TConverterSource : struct, IConverterSource<TModel, TProvider>
        where TModel : struct, IMetaObjectPropertyConvertible<TModel, TProvider, TConverterSource>
    { }
}