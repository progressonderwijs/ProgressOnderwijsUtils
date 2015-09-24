using System;
using NUnit.Framework;

namespace ProgressOnderwijsUtils.Test
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ContinuousAttribute : CategoryAttribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false),
        UsefulToKeep("We gaan continuous hiermee vervangen")]
    public sealed class ExcludeFromContinuousAttribute : CategoryAttribute { }

    /// <summary>
    /// Bij het 'uitzetten' van de betaalplannen in de code zijn er een 100-tal test cases die nog wel uitgaan van een betaalplan in het opzetten van hun scenario.
    /// Deze test cases zetten we nu even tijdelijk uit, maar moeten zo snel mogelijk of gefixed worden of verwijderd worden indien niet meer van toepassing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class MissendeBetaalplanAttribute : CategoryAttribute
    {
    }
}
