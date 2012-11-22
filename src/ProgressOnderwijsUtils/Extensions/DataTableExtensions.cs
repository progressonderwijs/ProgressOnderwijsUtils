using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// Extensions for System.Data.DataTable.
	/// </summary>
	public static class DataTableExtensions
	{
		sealed class Key : IEquatable<Key>
		{
			readonly DataColumn[] columns;
			readonly DataRow row;

			public Key(DataColumn[] columns, DataRow row)
			{
				this.columns = columns;
				this.row = row;
			}

			public bool Equals(Key other)
			{
				bool result = true;
				foreach (DataColumn column in columns)
				{
					result &= row[column].Equals(other.row[column]);
				}
				return result;
			}

			public override int GetHashCode()
			{
				StringBuilder result = new StringBuilder();
				foreach (DataColumn column in columns)
				{
					result.Append(row[column]);
				}
				return result.ToString().GetHashCode();
			}
		}

		/// <summary>
		/// Delegate declaration to determine which record must be chosen.
		/// </summary>
		/// <param name="one"></param>
		/// <param name="other"></param>
		/// <param name="data">Extra data object passed to the MakeUnique method.</param>
		/// <returns>The chosen record.</returns>
		public delegate DataRow Comparator(DataRow one, DataRow other, object data);

		/// <summary>
		/// Delete all duplicate records from this table that have the same key.
		/// </summary>
		/// <param name="table"></param>
		/// <param name="key">The primary key of the tabel is set to this then unique key.</param>
		/// <param name="comparator">Optional delegate to choose the records to delete.</param>
		/// <param name="data">Optional object passed transparently to the comparator when called.</param>
		/// <param name="primary">Optional flag denoting whether to set the primary key to the key specified or not.</param>
		public static void MakeUnique(this DataTable table, DataColumn[] key, Comparator comparator = null, object data = null, bool primary = true)
		{
			var duplicates =
				from row in table.Rows.Cast<DataRow>()
				group row by new Key(key, row) into grp
				where grp.Count() > 1
				select grp;
			foreach (var duplicate in duplicates)
			{
				DataRow final = null;
				foreach (DataRow candidate in duplicate)
				{
					if (final == null)
					{
						final = candidate;
					}
					else if (comparator != null)
					{
						DataRow tmp = comparator(final, candidate, data);
						if (ReferenceEquals(tmp, final))
						{
							candidate.Delete();
						}
						else
						{
							final.Delete();
							final = candidate;
						}
					}
					else
					{
						candidate.Delete();
					}
				}
			}
			table.AcceptChanges();
			if (primary)
			{
				table.PrimaryKey = key;
			}
		}
	}

	[TestFixture]
	public class DataTableExtensionsTest
	{
		DataTable sut;
		DataColumn col1;
		DataColumn col2;

		[SetUp]
		public void SetUp()
		{
			sut = new DataTable();
			col1 = new DataColumn("one", typeof(int));
			col2 = new DataColumn("two", typeof(int));
			sut.Columns.Add(col1);
			sut.Columns.Add(col2);
		}

		void SetUpRows(IEnumerable<int[]> rows)
		{
			foreach (int[] data in rows)
			{
				DataRow row = sut.NewRow();
				for (int i = 0; i < data.Length; ++i)
				{
					row[i] = data[i];
				}
				sut.Rows.Add(row);
			}
		}

		IEnumerable<object[]> MakeUniqueData()
		{
			yield return new object[] { 0, new int[][] { } };
			yield return new object[] { 1, new[]
			{ 
				new[] { 1, 1 },
			} };
			yield return new object[] { 2, new[]
			{ 
				new[] { 1, 1 },
				new[] { 2, 2 },
			} };
			yield return new object[] { 1, new[]
			{ 
				new[] { 1, 2 },
				new[] { 1, 2 },
			} };
		}

		[Test, TestCaseSource("MakeUniqueData")]
		public void MakeUnique(int count, int[][] rows)
		{
			SetUpRows(rows);
			sut.MakeUnique(new[] { col1, col2 }, null);
			Assert.That(sut.Rows.Count, Is.EqualTo(count));
		}

		[Test]
		public void MakeUniqueCompareOne()
		{
			SetUpRows(new[] { new[] { 1, 1 }, new[] { 1, 2 } });
			sut.MakeUnique(new[] { col1 }, delegate(DataRow one, DataRow other, object data)
			{
				Assert.That(data, Is.Null);
				return one;
			});
			Assert.That(sut.Rows[0][1], Is.EqualTo(1));
		}

		[Test]
		public void MakeUniqueCompareOther()
		{
			SetUpRows(new[] { new[] { 1, 1 }, new[] { 1, 2 } });
			sut.MakeUnique(new[] { col1 }, delegate(DataRow one, DataRow other, object data)
			{
				Assert.That((int)data == 1);
				return other;
			}, 1);
			Assert.That(sut.Rows[0][1], Is.EqualTo(2));
		}
	}
}
