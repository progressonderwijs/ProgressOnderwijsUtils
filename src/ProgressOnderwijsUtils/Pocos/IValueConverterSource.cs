using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// ReSharper disable once CheckNamespace
namespace ProgressOnderwijsUtils
{
    public interface IValueConverterSource<TModel, TProvider>
    {
        ValueConverter<TModel, TProvider> GetValueConverter();
    }

    public static class IConverterSourceExtensions
    {
        public static ValueConverter<TModel, TProvider> DefineConverter<TModel, TProvider>(
            // ReSharper disable once UnusedParameter.Global
            this IValueConverterSource<TModel, TProvider> _,
            Expression<Func<TModel, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TModel>> convertFromProviderExpression
        )
            => new(convertToProviderExpression, convertFromProviderExpression);
    }
}
