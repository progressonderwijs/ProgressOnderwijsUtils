using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	[AttributeUsage(AttributeTargets.Class,Inherited=false)]
	public class NoCodingStyleTestAttribute : Attribute
	{
		public NoCodingStyleTestAttribute(string reason) { }
	}
}
