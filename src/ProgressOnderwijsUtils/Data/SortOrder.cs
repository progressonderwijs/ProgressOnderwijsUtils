using MoreLinq;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// Bevat gestuctureerde orderinginformatie
	/// </summary>
	[Serializable]
	public class SortOrder : List<SortColumn>
	{
		public SortColumn BaseSortOrder { set; get; }

		public int OrderingIndex(string column, SortDirection direction)
		{
			return IndexOf(new SortColumn(column, direction));
		}

		public void AddReverse(string kolomnaam)
		{
			if (Remove(new SortColumn(kolomnaam, SortDirection.Desc)))
				Insert(0, new SortColumn(kolomnaam, SortDirection.Asc));
			else
			{
				Remove(new SortColumn(kolomnaam, SortDirection.Asc));
				Insert(0, new SortColumn(kolomnaam, SortDirection.Desc));
			}
		}

		public IEnumerable<SortColumn> CompleteSortOrder
		{
			get
			{
				foreach (var sc in this) yield return sc;
				if (BaseSortOrder != null) yield return BaseSortOrder;
			}
		}
	}

	[Serializable]
	public class SortColumn : IEquatable<SortColumn>
	{
		readonly string column;
		readonly SortDirection direction;

		public string Column { get { return column; } }
		public SortDirection Direction { get { return direction; } }
		public string SqlSortString { get { return column + " " + direction; } }

		public SortColumn(string column, SortDirection direction) { this.column = column; this.direction = direction; }

		public bool Equals(SortColumn other) { return Column == other.Column && Direction == other.Direction; }
		public override bool Equals(object obj) { return obj is SortColumn && Equals((SortColumn)obj); }
		public override int GetHashCode() { return column.GetHashCode() + 51 * direction.GetHashCode(); }
	}
}
