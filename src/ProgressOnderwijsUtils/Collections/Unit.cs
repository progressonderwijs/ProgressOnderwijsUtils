using System;

namespace ProgressOnderwijsUtils.Collections
{
    /// <summary>
    /// This type represents "nothing". Its only purpose is to satisfy the compiler; so e.g. when it expects a Func from A to B, and you only have an Action on A; you might pass a Func from A to Void.
    /// </summary>
    public struct Unit
    {
        public static Unit Value
            => default(Unit);

        public static Unit SideEffect(Action action)
        {
            action();
            return Value;
        }

        public T Return<T>(T val)
            => val;
    }
}
