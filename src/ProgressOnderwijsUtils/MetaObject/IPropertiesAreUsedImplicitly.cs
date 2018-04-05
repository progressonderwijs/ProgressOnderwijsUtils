using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace ProgressOnderwijsUtils
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public interface IPropertiesAreUsedImplicitly
    {
        /*
         * This doesn't really work well, since resharper thinks that objects with implicitly
         * used members are used themselves.
         * 
         * My suggestion is to remove the tags at the top of this interface, and instead demand
         * (by test) that all the properties of inheritors have the attribute used implicitly
         * (or similar)
         */
    }
}
