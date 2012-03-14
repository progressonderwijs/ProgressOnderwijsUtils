using System;

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// A Marker interface to indicate that this class can be translated.
	/// </summary>
	public interface ITranslatable { 
		string GenerateUid();
		TextVal Translate(ITranslationKeyLookup connectionOrContext, int langId);
	}
}
