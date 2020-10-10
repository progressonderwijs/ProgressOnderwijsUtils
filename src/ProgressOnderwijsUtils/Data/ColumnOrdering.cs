using System;
using System.Data;

namespace ProgressOnderwijsUtils
{
    readonly struct ColumnOrdering : IEquatable<ColumnOrdering>
    {
        readonly int cachedHash;
        public readonly string[] Cols;

        ColumnOrdering(int _cachedHash, string[] _cols)
            => (cachedHash, Cols) = (_cachedHash, _cols);

        public static ColumnOrdering FromReader(IDataReader reader)
        {
            var cols = PooledSmallBufferAllocator<string>.GetByLength(reader.FieldCount);
            var hashCode = new HashCode();
            for (var i = 0; i < cols.Length; i++) {
                var name = reader.GetName(i);
                cols[i] = name;
                var caseInsensitiveHash = CaseInsensitiveHash(name);
                hashCode.Add((int)caseInsensitiveHash);
                hashCode.Add((int)(caseInsensitiveHash >> 32));
            }
            return new ColumnOrdering(hashCode.ToHashCode(), cols);
        }

        public bool Equals(ColumnOrdering other)
        {
            var oCols = other.Cols;
            if (cachedHash != other.cachedHash || Cols.Length != oCols.Length) {
                return false;
            }
            for (var i = 0; i < Cols.Length; i++) {
                if (!CaseInsensitiveEquality(Cols[i], oCols[i])) {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
            => cachedHash;

        public override bool Equals(object? obj)
            => obj is ColumnOrdering columnOrdering && Equals(columnOrdering);

        public static ulong CaseInsensitiveHash(string s)
        {
            //Much faster than StringComparer.OrdinalIgnoreCase.GetHashCode(...)
            //Based on java's String.hashCode(): http://docs.oracle.com/javase/6/docs/api/java/lang/String.html#hashCode%28%29
            //In particularly, need not produce great hash quality since column names are non-hostile (and it's good enough for the largest language on the planet...)

            var hash = 0ul;
            foreach (var c in s) {
                var code = c > 'Z' || c < 'A' ? c : c + (uint)AsciiUpperToLowerDiff;
                hash = (hash << 5) - hash + code;
            }
            return hash;
        }

        public static bool CaseInsensitiveEquality(string a, string b)
        {
            //Much faster than StringComparer.OrdinalIgnoreCase.Equals(a,b)
            //optimized for strings that are equal, because that's the expected use case.
            if (a.Length != b.Length) {
                return false;
            }
            for (var i = 0; i < a.Length; i++) {
                int aChar = a[i];
                int bChar = b[i];
                if (aChar != bChar) {
                    //although comparison is case insensitve, exact equality implies case insensitive equality
                    //exact equality is commonly true and faster, so we test that first.
                    var aCode = aChar > 'Z' || aChar < 'A' ? aChar : aChar + AsciiUpperToLowerDiff;
                    var bCode = bChar > 'Z' || bChar < 'A' ? bChar : bChar + AsciiUpperToLowerDiff;

                    if (aCode != bCode) {
                        return false;
                    }
                }
            }
            return true;
        }

        const int AsciiUpperToLowerDiff = 'a' - 'A';
    }
}
