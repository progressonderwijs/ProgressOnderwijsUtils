using System;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// A piece of data that can be translated.
    /// </summary>
    public interface ITranslatable
    {
        [Pure]
        string GenerateUid();

        [Pure]
        TextVal Translate(Taal lang);
    }
}
