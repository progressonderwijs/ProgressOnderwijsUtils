using System;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// A piece of data that can be translated.
    /// </summary>
    public interface ITranslatable
    {
        string GenerateUid();
        TextVal Translate(Taal lang);
    }
}
