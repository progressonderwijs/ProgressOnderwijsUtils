using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Class)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
    public sealed class ReadImplicitlyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Class)]
    [MeansImplicitUse(ImplicitUseKindFlags.Assign | ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.Members)]
    public sealed class WrittenImplicitlyAttribute : Attribute { }

    public interface IPoco { }

    [ReadImplicitly]
    public interface IReadImplicitly : IPoco { }

    [WrittenImplicitly]
    public interface IWrittenImplicitly : IPoco { }

    public interface IUsedImplicitly : IReadImplicitly, IWrittenImplicitly { }

    [AttributeUsage(AttributeTargets.All)]
    [UsefulToKeep("library attribute")]
    [MeansImplicitUse]
    public sealed class UsefulToKeepAttribute : Attribute
    {
        public UsefulToKeepAttribute([UsefulToKeep("for documentation")] string reason)
        {
            Reason = reason;
        }

        public string Reason { get; }
    }

    [AttributeUsage(AttributeTargets.All)]
    [UsefulToKeep("library attribute")]
    [MeansImplicitUse(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers)]
    public sealed class UsedImplicitlyBySerializationAttribute : Attribute { }
}
