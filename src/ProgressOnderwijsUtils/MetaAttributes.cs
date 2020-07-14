using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [UsefulToKeep("library attribute")]
    public sealed class NoCodingStyleTestAttribute : Attribute
    {
        public NoCodingStyleTestAttribute([UsefulToKeep("for documentation")] string reason) { }
    }

    [AttributeUsage(AttributeTargets.All)]
    [UsefulToKeep("library attribute")]
    [MeansImplicitUse]
    public sealed class UsefulToKeepAttribute : Attribute
    {
        public UsefulToKeepAttribute([UsefulToKeep("for documentation")] string reason) { }
    }

    [AttributeUsage(AttributeTargets.All)]
    [UsefulToKeep("library attribute")]
    [MeansImplicitUse]
    public sealed class UsedImplicitlyBySerializationAttribute : Attribute { }
}
