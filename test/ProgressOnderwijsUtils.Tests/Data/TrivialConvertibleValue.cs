using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ProgressOnderwijsUtils.Tests.Data
{

    public static class TrivialConvertibleValue
    {
        public static TrivialConvertibleValue<T> Create<T>(T value)
            => new TrivialConvertibleValue<T>(value);
    }

    public struct TrivialConvertibleValue<T> : IMetaObjectPropertyConvertible<TrivialConvertibleValue<T>, T, TrivialConvertibleValue<T>.CustomBlaStructConverter>
    {
        public struct CustomBlaStructConverter : IConverterSource<TrivialConvertibleValue<T>, T>
        {
            public ValueConverter<TrivialConvertibleValue<T>, T> GetValueConverter()
                => this.DefineConverter(v => v.Value, v => new TrivialConvertibleValue<T>(v));
        }

        public TrivialConvertibleValue(T value)
            => Value = value;

        public T Value { get; }

        [MetaObjectPropertyLoader]
        public static TrivialConvertibleValue<T> MethodWithIrrelevantName(T value)
            => new TrivialConvertibleValue<T>(value);
    }
}
