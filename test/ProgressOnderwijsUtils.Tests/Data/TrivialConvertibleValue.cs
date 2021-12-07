using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ProgressOnderwijsUtils.Tests.Data;

public static class TrivialConvertibleValue
{
    public static TrivialValue<T> Create<T>(T value)
        => new(value);
}

public struct TrivialValue<T> : IHasValueConverter<TrivialValue<T>, T, TrivialValue<T>.CustomBlaStructConverter>
{
    public struct CustomBlaStructConverter : IValueConverterSource<TrivialValue<T>, T>
    {
        public ValueConverter<TrivialValue<T>, T> GetValueConverter()
            => this.DefineConverter(v => v.Value, v => new(v));
    }

    public TrivialValue(T value)
        => Value = value;

    public T Value { get; }
}
