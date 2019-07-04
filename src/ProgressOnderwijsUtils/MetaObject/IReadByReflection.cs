using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Class)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
    public sealed class ReadByReflectionAttribute : Attribute { }

    [ReadByReflection]
    public interface IReadByReflection { }
}
