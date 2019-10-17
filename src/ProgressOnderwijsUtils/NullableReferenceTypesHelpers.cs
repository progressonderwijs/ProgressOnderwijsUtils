using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class NullableReferenceTypesHelpers
    {
        [MustUseReturnValue]
        public static T AssertNotNull<T>(this T? t)
            where T : class
            => t ?? throw new Exception(typeof(T) + " is null!");

        [MustUseReturnValue]
        public static T? PretendNullable<T>(this T t)
            where T : class
            => t;
    }
}
