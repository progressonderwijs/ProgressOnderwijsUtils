﻿using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false), UsefulToKeep("library attribute")]
    public class NoCodingStyleTestAttribute : Attribute
    {
        public NoCodingStyleTestAttribute([UsefulToKeep("for documentation")] string reason) { }
    }

    [AttributeUsage(AttributeTargets.All), UsefulToKeep("library attribute"), MeansImplicitUse]
    public class UsefulToKeepAttribute : Attribute
    {
        public UsefulToKeepAttribute(string reason) { }
    }
}
