using System;

namespace ProgressOnderwijsUtils
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class CodeThatsOnlyUsedForTests : Attribute { }
}
