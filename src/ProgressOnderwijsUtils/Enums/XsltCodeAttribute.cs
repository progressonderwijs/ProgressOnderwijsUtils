using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProgressOnderwijsUtils.Enums.Support;

namespace ProgressOnderwijsUtils.Enums
{
	public class XsltCodeAttribute : Attribute, IHasLabel<string>
	{
		public XsltCodeAttribute(string code) { Label = code; }
		public string Label { get; private set; }
	}
}
