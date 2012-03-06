using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace ProgressOnderwijsUtils.Data
{
	/// <summary>
	/// Slaat de selectedkey, de currentpage en de datasource settings van een genericlijst op.
	/// </summary>
	[Serializable] public sealed class GenericLijstSettings
	{
		public SortedList SelectedKey;
		public int CurrentPage;
	}
}
