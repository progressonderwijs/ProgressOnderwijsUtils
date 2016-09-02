﻿using System;
using System.IO;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    [CodeThatsOnlyUsedForTests]
    public static class FileInfoExtension
    {
        [Pure]
        [CodeThatsOnlyUsedForTests]
        public static string ReadToEnd(this FileInfo file)
        {
            using (var reader = file.OpenText())
                return reader.ReadToEnd();
        }

        /// <summary>
        /// Tests whether this file has the same contents as another file.
        /// </summary>
        [Pure]
        [CodeThatsOnlyUsedForTests]
        public static bool SameContents(this FileInfo one, FileInfo other)
        {
            if (other == null) {
                throw new ArgumentNullException(nameof(other));
            }

            bool result = true;
            if (one.FullName == other.FullName) { } else if (one.Length != other.Length) {
                result = false;
            } else {
                using (FileStream fs1 = one.OpenRead())
                using (FileStream fs2 = other.OpenRead()) {
                    while (result && (fs1.Position < fs1.Length)) {
                        result = fs1.ReadByte() == fs2.ReadByte();
                    }
                }
            }

            return result;
        }
    }
}
