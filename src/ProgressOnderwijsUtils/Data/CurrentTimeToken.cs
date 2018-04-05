using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public sealed class CurrentTimeToken
    {
        CurrentTimeToken() { }
        public static readonly CurrentTimeToken Instance = new CurrentTimeToken();

        public static CurrentTimeToken Parse([NotNull] string s)
        {
            if (s != "") {
                throw new ArgumentException("Can only parse empty string as current time token!");
            }
            return Instance;
        }
    }
}
