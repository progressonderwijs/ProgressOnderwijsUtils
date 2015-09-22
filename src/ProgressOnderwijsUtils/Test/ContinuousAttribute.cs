using System;
using NUnit.Framework;

namespace ProgressOnderwijsUtils.Test
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ContinuousAttribute : CategoryAttribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false),
        UsefulToKeep("We gaan continuous hiermee vervangen")]
    public sealed class ExcludeFromContinuousAttribute : CategoryAttribute { }
}
