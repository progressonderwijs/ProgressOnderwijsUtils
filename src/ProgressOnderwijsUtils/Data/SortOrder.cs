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
	public class SortOrder
	{
		readonly List<string> sortColumns = new List<string>();
		readonly Dictionary<string, SortDirection> sortDirections = new Dictionary<string, SortDirection>();

		public SortOrder() { }
		public SortOrder(IEnumerable<SortColumn> leastSignificantFirst) { foreach (var sc in leastSignificantFirst) AddMostSignificant(sc); }
		public SortOrder(params SortColumn[] leastSignificantFirst) { foreach (var sc in leastSignificantFirst) AddMostSignificant(sc); }


		public SortColumn BaseSortOrder { set; get; }


		/// <summary>
		/// The rank of the column in the sort order; 1 is most significant, higher numbers less so; null is not included in the sort order.
		/// </summary>
		public int? ColumnSortRank(string column) { return sortDirections.ContainsKey(column) ? sortColumns.Count - sortColumns.IndexOf(column) : default(int?); }

		public SortDirection? ColumnSortDirection(string column)
		{
			SortDirection retval;
			return sortDirections.TryGetValue(column, out retval) ? retval : default(SortDirection?);
		}

		static SortDirection FlipDirection(SortDirection dir) { return dir == SortDirection.Asc ? SortDirection.Desc : SortDirection.Asc; }

		public void AddOrReverse(string kolomnaam) //TODO:Emn:eigenlijk is dit niet logisch voor een click, want je wilt ook "ongesorteerd" weer kunnen geven, en als je alleen een kolom "als eerste" wil sorteren maar niet de richting wijzigen is dit nu lastig.
		{
			bool containedKolom = sortColumns.Remove(kolomnaam);
			sortDirections[kolomnaam] = containedKolom ? FlipDirection(sortDirections[kolomnaam]) : SortDirection.Desc;
			sortColumns.Add(kolomnaam);
		}

		public void AddMostSignificant(SortColumn kolom) { AddMostSignificant(kolom.Column, kolom.Direction); }
		public void AddMostSignificant(string kolomnaam, SortDirection dir = SortDirection.Desc)
		{
			sortDirections.Add(kolomnaam, dir);
			sortColumns.Add(kolomnaam);
		}


		public void AddLeastSignificant(SortColumn kolom) { AddLeastSignificant(kolom.Column, kolom.Direction); }
		public void AddLeastSignificant(string kolomnaam, SortDirection dir = SortDirection.Desc)
		{
			sortDirections.Add(kolomnaam, dir);
			sortColumns.Insert(0, kolomnaam);
		}



		public bool HasNonFallbackSorting { get { return sortColumns.Any(); } }

		public IEnumerable<SortColumn> CompleteSortOrder
		{
			get
			{
				return BaseSortOrder == null ? ManualLeastSignificantFirstOrder.Reverse() : ManualLeastSignificantFirstOrder.Reverse().Concat(new[] { BaseSortOrder });
			}
		}

		public IEnumerable<SortColumn> ManualLeastSignificantFirstOrder { get { return sortColumns.Select(sc => new SortColumn(sc, sortDirections[sc])); } }

		public void Clear()
		{
			sortDirections.Clear();
			sortColumns.Clear();
		}

		public int ManualCount { get { return sortColumns.Count; } }

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
