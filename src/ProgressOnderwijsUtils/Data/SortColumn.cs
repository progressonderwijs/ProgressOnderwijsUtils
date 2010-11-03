﻿using System;
using NUnit.Framework;

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

	[TestFixture]
	public class SortColumnTest 
	{
		[Test]
		public void CheckEquals()
		{
			Assert.AreNotEqual(new SortColumn("test", SortDirection.Asc), new SortColumn("ziggy", SortDirection.Asc));
			Assert.AreEqual(new SortColumn("test", SortDirection.Asc), new SortColumn("ziggy", SortDirection.Asc).WithDifferentName("test"));
			Assert.AreNotEqual(new SortColumn("test", SortDirection.Asc), new SortColumn("Test", SortDirection.Asc));

			Assert.AreNotSame(new SortColumn("abc", SortDirection.Asc), new SortColumn("abc", SortDirection.Asc));
			Assert.AreEqual(new SortColumn("abc", SortDirection.Asc), new SortColumn("abc", SortDirection.Asc));

			Assert.AreNotEqual(new SortColumn("abc", SortDirection.Asc), new SortColumn("abc", SortDirection.Desc));
			Assert.AreEqual(new SortColumn("abc", SortDirection.Asc), new SortColumn("abc", SortDirection.Desc).WithReverseDirection);
		}
		[Test]
		public void CheckSqlSortString()
		{
			Assert.AreEqual(new SortColumn("ziggy", SortDirection.Asc).SqlSortString, "ziggy Asc");
			Assert.AreEqual(new SortColumn("ziggy", SortDirection.Asc).WithDifferentName("test").SqlSortString, "test Asc");
		}
	}
}
