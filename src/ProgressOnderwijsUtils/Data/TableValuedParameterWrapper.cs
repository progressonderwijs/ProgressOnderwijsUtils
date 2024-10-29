using System.ComponentModel.DataAnnotations;

namespace ProgressOnderwijsUtils.Internal;

//public needed for auto-mapping
readonly struct TableValuedParameterWrapper<T> : IWrittenImplicitly, IOptionalObjectProjectionForDebugging, IReadImplicitly
{
    [Key]
    public T QueryTableValue { get; init; }

    public override string? ToString()
        => QueryTableValue is null ? "NULL" : QueryTableValue.ToString();

    public object? ProjectionForDebuggingOrNull()
        => QueryTableValue;
}


static class TableValuedParameterWrapperHelper
{
    /// <summary>
    /// Efficiently wraps an enumerable of objects in DbTableValuedParameterWrapper and materialized the sequence as array.
    /// Effectively it's like .Select(x => new DbTableValuedParameterWrapper { querytablevalue = x }).ToArray() but faster.
    /// </summary>
    public static TableValuedParameterWrapper<T>[] WrapPlainValueInSinglePropertyPoco<T>(IEnumerable<T> typedEnumerable)
    {
        if (typedEnumerable is T[] typedArray) {
            var projectedArray = new TableValuedParameterWrapper<T>[typedArray.Length];
            for (var i = 0; i < projectedArray.Length; i++) {
                projectedArray[i] = new() { QueryTableValue = typedArray[i], };
            }
            return projectedArray;
        }

        if (typedEnumerable is IReadOnlyList<T> typedList) {
            var projectedArray = new TableValuedParameterWrapper<T>[typedList.Count];
            for (var i = 0; i < projectedArray.Length; i++) {
                projectedArray[i] = new() { QueryTableValue = typedList[i], };
            }
            return projectedArray;
        }

        var arrayBuilder = new ArrayBuilder<TableValuedParameterWrapper<T>>();
        foreach (var item in typedEnumerable) {
            arrayBuilder.Add(new() { QueryTableValue = item, });
        }
        return arrayBuilder.ToArray();
    }
}
