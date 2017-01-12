namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// A toggle that is off by default, and can be turned on/off.  
    /// The current state is on whenever it has been enabled more often than it has been disabled; so if you "enable" multiple times you must "disable" at least that often to turn it off again.
    /// </summary>
    public sealed class NestableToggle
    {
        int EnabledHowOften;
        public bool IsCurrentlyEnabled => EnabledHowOften > 0;
        public void Enable() => EnabledHowOften++;
        public void Disable() => EnabledHowOften--;
    }
}