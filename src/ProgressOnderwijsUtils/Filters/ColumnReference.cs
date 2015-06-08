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
                throw new ArgumentNullException("colname");
            } else if (!IsOkName.IsMatch(colname)) {
                throw new ArgumentException("Geen valide kolomnaam " + colname, "colname");
            }
            ColumnName = colname;
        }

        public bool Equals(ColumnReference other) { return ColumnName == other.ColumnName; }
        public override bool Equals(object obj) { return obj is ColumnReference && Equals((ColumnReference)obj); }
        public override int GetHashCode() => 1 + ColumnName.GetHashCode();
        public static bool operator ==(ColumnReference a, ColumnReference b) { return ReferenceEquals(a, b) || a != null && b != null && a.Equals(b); }
        public static bool operator !=(ColumnReference a, ColumnReference b) { return !ReferenceEquals(a, b) && (a == null || b == null || !a.Equals(b)); }
    }

    [Serializable]
    public sealed class LiteralSqlInt : IEquatable<LiteralSqlInt>
    {
        public readonly int Value;
        public LiteralSqlInt(int val) { Value = val; }
        public bool Equals(LiteralSqlInt other) { return Value == other.Value; }
        public override bool Equals(object obj) { return obj is LiteralSqlInt && Equals((LiteralSqlInt)obj); }
        public override int GetHashCode() => 27 + Value.GetHashCode();
        public static bool operator ==(LiteralSqlInt a, LiteralSqlInt b) { return ReferenceEquals(a, b) || a != null && b != null && a.Equals(b); }
        public static bool operator !=(LiteralSqlInt a, LiteralSqlInt b) { return !ReferenceEquals(a, b) && (a == null || b == null || !a.Equals(b)); }
        public static LiteralSqlInt Create(int p) { return new LiteralSqlInt(p); }
    }
}
