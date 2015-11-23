using System;
using System.Text.RegularExpressions;

namespace ProgressOnderwijsUtils
{
    [Serializable]
    public sealed class ColumnReference : IEquatable<ColumnReference>
    {
        public static readonly Regex IsOkName = new Regex(@"^(\w+|getdate\(\))$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        public readonly string ColumnName;

        public ColumnReference(string colname)
        {
            if (colname == null) {
                throw new ArgumentNullException(nameof(colname));
            } else if (!IsOkName.IsMatch(colname)) {
                throw new ArgumentException("Geen valide kolomnaam " + colname, nameof(colname));
            }
            ColumnName = colname;
        }

        public bool Equals(ColumnReference other) => ColumnName == other.ColumnName;
        public override bool Equals(object obj) => obj is ColumnReference && Equals((ColumnReference)obj);
        public override int GetHashCode() => 1 + ColumnName.GetHashCode();

        public static bool operator ==(ColumnReference a, ColumnReference b)
        {
            return ReferenceEquals(a, b) || a != null && b != null && a.Equals(b);
        }

        public static bool operator !=(ColumnReference a, ColumnReference b)
        {
            return !ReferenceEquals(a, b) && (a == null || b == null || !a.Equals(b));
        }
    }
}
