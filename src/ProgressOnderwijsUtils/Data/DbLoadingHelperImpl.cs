﻿using System;
using System.Data;

namespace ProgressOnderwijsUtils
{
    public static class DbLoadingHelperImpl
    {
        //called via reflection from DataReaderSpecialization
        public static byte[] GetBytes(this IDataRecord row, int colIndex)
        {
            var byteCount = row.GetBytes(
                colIndex,
                0L,
                null!, // Incorrect annotation? https://github.com/dotnet/runtime/blob/35e535e6fa50b8284a44428d6b03b0180bc9b499/src/libraries/System.Data.Common/src/System/Data/Common/DataRecordInternal.cs#L123
                0,
                0
            );
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

        //called via reflection from DataReaderSpecialization
        public static char[] GetChars(this IDataRecord row, int colIndex)
        {
            var charCount = row.GetChars(
                colIndex,
                0L,
                null!, // Incorrect annotation? https://github.com/dotnet/runtime/blob/35e535e6fa50b8284a44428d6b03b0180bc9b499/src/libraries/System.Data.Common/src/System/Data/Common/DataRecordInternal.cs#L194
                0,
                0
            );
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
