using System;
using System.Collections.Generic;
using System.Text;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
	public interface ITranslationKeyLookup
	{
		TextVal Lookup(Taal taal, string uid);
	}

	public interface ITextDefKey : ITranslatable
	{
		string WebModule { get;}
		string Sleutel { get; }
	}
}
