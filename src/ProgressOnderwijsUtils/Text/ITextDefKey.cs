using System;
using System.Collections.Generic;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
	public interface ITextDefKey : ITranslatable
	{
		string WebModule { get;}
		string Sleutel { get; }
	}
}
