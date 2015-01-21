using System;
using NUnit.Framework;

namespace ProgressOnderwijsUtils.Test
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ContinuousAttribute : CategoryAttribute { }

    /// <summary>
    /// Deze tests gaan over de database structuur zelf, en wil je dus alleen draaien in omgevingen waar de DB schema zeker netjes op orde is.
    /// In 't bijzonder, ze kunnen locaal misgaan mits sqlbuider en/of de restore out of date is, en in feature builds.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DbTestAttribute : CategoryAttribute { }
}
