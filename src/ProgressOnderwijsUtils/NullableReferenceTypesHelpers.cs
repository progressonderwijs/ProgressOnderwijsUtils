namespace ProgressOnderwijsUtils;

public static class NullableReferenceTypesHelpers
{
    [MustUseReturnValue]
    public static T AssertNotNull<T>(this T? t)
        where T : class
        => t ?? throw new($"{typeof(T)} is null!");

    [MustUseReturnValue]
    public static T AssertNotNull<T>(this T? t)
        where T : struct
        => t ?? throw new($"{typeof(T)} is null!");

    [MustUseReturnValue]
    // ReSharper disable once ReturnTypeCanBeNotNullable
    public static T? PretendNullable<T>(this T t)
        where T : class
        => t;
}
