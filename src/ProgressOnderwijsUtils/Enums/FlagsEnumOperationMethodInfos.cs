using System;
using System.Reflection;

namespace ProgressOnderwijsUtils
{
    struct FlagsEnumOperationMethodInfos
    {
        public MethodInfo Or, HasFlag, HasFlagOverlap, ToInt64;
        public static readonly FlagsEnumOperationMethodInfos forInt32 = Init<int>(Int32Helpers.Or, Int32Helpers.HasFlag, Int32Helpers.HasFlagOverlap, Int32Helpers.ToInt64);
        public static readonly FlagsEnumOperationMethodInfos forInt64 = Init<long>(Int64Helpers.Or, Int64Helpers.HasFlag, Int64Helpers.HasFlagOverlap, Int64Helpers.ToInt64);

        static FlagsEnumOperationMethodInfos Init<T>(Func<T, T, T> or, Func<T, T, bool> hasFlag, Func<T, T, bool> overlaps, Func<T, long> toInt64)
        {
            return new FlagsEnumOperationMethodInfos {
                Or = or.Method,
                HasFlag = hasFlag.Method,
                HasFlagOverlap = overlaps.Method,
                ToInt64 = toInt64.Method,
            };
        }

        static class Int32Helpers
        {
            public static int Or(int a, int b) => a | b;
            public static bool HasFlag(int val, int flag) => (val & flag) == flag;
            public static bool HasFlagOverlap(int a, int b) => (a & b) != 0;
            public static long ToInt64(int a) => a;
        }

        static class Int64Helpers
        {
            public static long Or(long a, long b) => a | b;
            public static bool HasFlag(long val, long flag) => (val & flag) == flag;
            public static bool HasFlagOverlap(long a, long b) => (a & b) != 0L;
            public static long ToInt64(long a) => a;
        }

        public static FlagsEnumOperationMethodInfos GetFlagsMethods(Type underlyingIntegralType)
        {
            if (typeof(int) == underlyingIntegralType) {
                return forInt32;
            } else if (typeof(long) == underlyingIntegralType) {
                return forInt64;
            } else {
                throw new NotSupportedException($"flags operations not supported on {underlyingIntegralType}");
            }
        }
    }
}