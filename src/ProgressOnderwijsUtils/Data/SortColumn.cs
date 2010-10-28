using System;

namespace ProgressOnderwijsUtils
{

	[Serializable]
	public class SortColumn : IEquatable<SortColumn>
	{
		readonly string column;
		readonly SortDirection direction;

		public string Column { get { return column; } }
		public SortDirection Direction { get { return direction; } }
		public string SqlSortString { get { return column + " " + direction; } }

		public SortColumn(string column, SortDirection direction) { this.column = column; this.direction = direction; }

		public SortColumn WithReverseDirection { get { return new SortColumn(column, FlipDirection(direction)); } }
		public SortColumn WithDifferentName(string newColumn) { return new SortColumn(newColumn, direction); }


		static SortDirection FlipDirection(SortDirection dir) { return dir == SortDirection.Asc ? SortDirection.Desc : SortDirection.Asc; }

		public bool Equals(SortColumn other) { return Column == other.Column && Direction == other.Direction; }
		public override bool Equals(object obj) { return obj is SortColumn && Equals((SortColumn)obj); }
		public override int GetHashCode() { return column.GetHashCode() + 51 * direction.GetHashCode(); }
	}
}
