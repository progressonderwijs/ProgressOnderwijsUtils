using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// Extensions for System.Data.DataTable.
    /// </summary>
    [Obsolete("This code is very slow and subtly buggy if a row value's ToString isn't identity preserving")]
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
                foreach (DataColumn column in columns) {
                    result &= row[column].Equals(other.row[column]);
                }
                return result;
            }

            public override int GetHashCode()
            {
                StringBuilder result = new StringBuilder();
                foreach (DataColumn column in columns) {
                    result.Append(row[column]);
                }
                return result.ToString().GetHashCode();
            }
        }

        /// <summary>
        /// Delete all duplicate records from this table that have the same key.
        /// </summary>
        /// <param name="key">The primary key of the tabel is set to this then unique key.</param>
        /// <param name="comparator">Optional delegate to choose the records to keep.</param>
        /// <param name="primary">Optional flag denoting whether to set the primary key to the key specified or not.</param>
        public static void DeleteDuplicates(this DataTable table, DataColumn[] key, Func<DataRow, DataRow, DataRow> comparator = null, bool primary = true)
        {
            var duplicates =
                from row in table.Rows.Cast<DataRow>()
                group row by new Key(key, row)
                into grp
                where grp.Count() > 1
                select grp;
            foreach (var duplicate in duplicates) {
                DataRow final = null;
                foreach (DataRow candidate in duplicate) {
                    if (final == null) {
                        final = candidate;
                    } else if (comparator != null) {
                        DataRow tmp = comparator(final, candidate);
                        if (ReferenceEquals(tmp, final)) {
                            candidate.Delete();
                        } else {
                            final.Delete();
                            final = candidate;
                        }
                    } else {
                        candidate.Delete();
                    }
                }
            }
            table.AcceptChanges();
            if (primary) {
                table.PrimaryKey = key;
            }
        }
    }

    [Continuous]
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

        static void SetUpRows(IEnumerable<int[]> rows, DataTable dataTable)
        {
            foreach (var data in rows) {
                var row = dataTable.NewRow();
                for (int i = 0; i < data.Length; ++i) {
                    row[i] = data[i];
                }
                dataTable.Rows.Add(row);
            }
        }

        static IEnumerable<TestCaseData> MakeUniqueData()
        {
            yield return new TestCaseData(0, new int[][] { });
            yield return new TestCaseData(
                1,
                new[] {
                    new[] { 1, 1 },
                }
            );
            yield return new TestCaseData(
                2,
                new[] {
                    new[] { 1, 1 },
                    new[] { 2, 2 },
                }
            );
            yield return new TestCaseData(
                1,
                new[] {
                    new[] { 1, 2 },
                    new[] { 1, 2 },
                }
            );
        }

        [Test, TestCaseSource(nameof(MakeUniqueData))]
        public void MakeUnique(int count, int[][] rows)
        {
            SetUpRows(rows, sut);
            sut.DeleteDuplicates(new[] { col1, col2 });
            Assert.That(sut.Rows.Count, Is.EqualTo(count));
        }

        [Test]
        public void MakeUniqueCompareOne()
        {
            SetUpRows(new[] { new[] { 1, 1 }, new[] { 1, 2 } }, sut);
            sut.DeleteDuplicates(new[] { col1 }, (one, other) => one);
            Assert.That(sut.Rows[0][1], Is.EqualTo(1));
        }

        [Test]
        public void MakeUniqueCompareOther()
        {
            SetUpRows(new[] { new[] { 1, 1 }, new[] { 1, 2 } }, sut);
            sut.DeleteDuplicates(new[] { col1 }, (one, other) => other);
            Assert.That(sut.Rows[0][1], Is.EqualTo(2));
        }
    }
}
