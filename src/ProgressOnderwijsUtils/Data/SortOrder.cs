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
		public SortColumn BaseSortOrder { set;get;}

		public SortOrder() { }

		public int OrderingIndex(string column, SortDirection direction)
		{
			return this.IndexOf(new SortColumn(column, direction));
		}

		public void AddReverse(string kolomnaam)
		{
			if (this.Contains(new SortColumn(kolomnaam, SortDirection.Desc)))
			{
				this.Remove(new SortColumn(kolomnaam, SortDirection.Desc));
				this.Insert(0, new SortColumn(kolomnaam, SortDirection.Asc));
			}
			else if (this.Contains(new SortColumn(kolomnaam, SortDirection.Asc)))
			{
				this.Remove(new SortColumn(kolomnaam, SortDirection.Asc));
				this.Insert(0, new SortColumn(kolomnaam, SortDirection.Desc));
			}
			else
				this.Insert(0, new SortColumn(kolomnaam, SortDirection.Desc));
			//Oude strategie
			/*			if (this.Contains(new SortColumn(kolomnaam, SortDirection.Desc)))
							this.Remove(new SortColumn(kolomnaam, SortDirection.Desc));
						else
						{
							if (this.Contains(new SortColumn(kolomnaam, SortDirection.Asc)))
							{
								this.Remove(new SortColumn(kolomnaam, SortDirection.Asc));
								this.Insert(0, new SortColumn(kolomnaam, SortDirection.Desc));
							}
							else
								this.Insert(0, new SortColumn(kolomnaam, SortDirection.Desc));
						}*/
			//Eenvoudige strategie
			/*			if (this.Contains(new SortColumn(kolomnaam, SortDirection.Asc)))
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
						}*/
		}

		/*public void RemoveAll()
		{
			this.Clear();
		}*/
	}

	[Serializable]
	public class SortColumn : IEquatable<SortColumn>
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
