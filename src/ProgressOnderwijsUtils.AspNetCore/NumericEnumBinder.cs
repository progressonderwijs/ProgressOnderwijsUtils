using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ProgressOnderwijsUtils.AspNetCore;

public sealed class NumericEnumBinder : IModelBinder
{
    readonly Type EnumType;
    readonly bool IsNullable;

    public NumericEnumBinder(Type enumType)
    {
        EnumType = enumType.IsGenericType && enumType.GetGenericTypeDefinition() == typeof(Nullable<>) ? enumType.GetGenericArguments()[0] : enumType;
        IsNullable = EnumType != enumType;
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult != ValueProviderResult.None) {
            if (IsNullable && valueProviderResult.FirstValue == "") {
                bindingContext.Result = ModelBindingResult.Success(null);
            } else if (long.TryParse(valueProviderResult.FirstValue, out var number)) {
                bindingContext.Result = ModelBindingResult.Success(Enum.ToObject(EnumType, number));
            } else {
                _ = bindingContext.ModelState.TryAddModelError(modelName, "must be an integer.");
            }
        }

        return Task.CompletedTask;
    }
}
