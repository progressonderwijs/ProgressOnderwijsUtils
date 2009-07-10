using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProgressOnderwijsUtils.Enums.Support;

namespace ProgressOnderwijsUtils.Enums
{
	public class DbCodeAttribute : Attribute, IHasLabel<int>
	{
		public DbCodeAttribute(int code) { Label = code; }
		public int Label { get; private set; }
	}
}
