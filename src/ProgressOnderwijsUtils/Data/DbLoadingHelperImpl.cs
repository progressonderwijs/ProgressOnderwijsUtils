using System;
using System.Data;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class DbLoadingHelperImpl
    {
        [NotNull]
        //called via reflection from DataReaderSpecialization
        public static byte[] GetBytes([NotNull] this IDataRecord row, int colIndex)
        {
            var byteCount = row.GetBytes(colIndex, 0L, null, 0, 0);
            if (byteCount > int.MaxValue) {
                throw new NotSupportedException("Array too large!");
            }
            var arr = new byte[byteCount];
            long offset = 0;
            while (offset < byteCount) {
                offset += row.GetBytes(colIndex, offset, arr, (int)offset, (int)byteCount);
            }
            return arr;
        }

        [NotNull]
        //called via reflection from DataReaderSpecialization
        public static char[] GetChars([NotNull] this IDataRecord row, int colIndex)
        {
            var charCount = row.GetChars(colIndex, 0L, null, 0, 0);
            if (charCount > int.MaxValue) {
                throw new NotSupportedException("Array too large!");
            }
            var arr = new char[charCount];
            long offset = 0;
            while (offset < charCount) {
                offset += row.GetChars(colIndex, offset, arr, (int)offset, (int)charCount);
            }
            return arr;
        }
    }
}
