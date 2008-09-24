using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// Bevat gestuctureerde orderinginformatie
	/// </summary>
	[Serializable]public class SortOrder : List<SortColumn>
	{
		SortColumn basesortorder = null;
		public SortColumn BaseSortOrder { set { basesortorder = value; } get { return basesortorder; } }
		
		public SortOrder()
		{
		}

		public int OrderingIndex(string column, SortDirection direction)
		{
			return this.IndexOf(new SortColumn(column, direction));
		}

		public void AddReverseRemove(string kolomnaam)
		{
			/*			if (this.Contains(new SortColumn(kolomnaam, SortDirection.desc)))
							this.Remove(new SortColumn(kolomnaam, SortDirection.desc));
						else
						{
							if (this.Contains(new SortColumn(kolomnaam, SortDirection.asc)))
							{
								this.Remove(new SortColumn(kolomnaam, SortDirection.asc));
								this.Insert(0, new SortColumn(kolomnaam, SortDirection.desc));
							}
							else
								this.Insert(0, new SortColumn(kolomnaam, SortDirection.asc));
						}*/

			//even eenvoudig, alleen een kolom tegelijk, anders snapt niemand er meer iets van
			if (this.Contains(new SortColumn(kolomnaam, SortDirection.Asc)))
			{
				this.Clear();
				this.Add(new SortColumn(kolomnaam, SortDirection.Desc));
			}
			else
			{
				if (this.Contains(new SortColumn(kolomnaam, SortDirection.Desc)))
				{
					this.Clear();
				}
				else
				{
					this.Clear();
					this.Add(new SortColumn(kolomnaam, SortDirection.Asc));
				}
			}
		}
	}
	[Serializable] public class SortColumn : IEquatable<SortColumn>
	{
		string column;
		SortDirection direction;

		public string Column { get { return column; } }
		public SortDirection Direction { get { return direction; } }

		public SortColumn(string column, SortDirection direction)
		{
			this.column = column;
			this.direction = direction;
		}

		/*		public int CompareTo(SortColumn other)
				{
					return Column.CompareTo(other.Column);
				}*/

		public bool Equals(SortColumn other)
		{
			return this.Column == other.Column && this.Direction == other.Direction;
		}
	}
}
