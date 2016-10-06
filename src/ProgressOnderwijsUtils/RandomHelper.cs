﻿using System;
using System.Linq;
using System.Text;
using MoreLinq;

namespace ProgressOnderwijsUtils
{
    public abstract class RandomHelper
    {
        protected abstract byte[] GetBytes(int numBytes);

        [UsefulToKeep("library method")]
        public byte GetByte() => GetBytes(1)[0];

        [UsefulToKeep("library method")]
        public int GetNonNegativeInt32() => (int)GetUInt32((uint)int.MaxValue + 1);

        public int GetInt32() => BitConverter.ToInt32(GetBytes(sizeof(int)), 0);

        [UsefulToKeep("library method")]
        public long GetInt64() => BitConverter.ToInt64(GetBytes(sizeof(long)), 0);

        [CLSCompliant(false), UsefulToKeep("library method")]
        public uint GetUInt32()
        {
            return BitConverter.ToUInt32(GetBytes(sizeof(uint)), 0);
        }

        [CLSCompliant(false), UsefulToKeep("library method")]
        public ulong GetUInt64()
        {
            return BitConverter.ToUInt64(GetBytes(sizeof(ulong)), 0);
        }

        [CLSCompliant(false), UsefulToKeep("library method")]
        public uint GetUInt32(uint excludedBound)
        {
            // Proved in: http://www.google.com/url?q=http%3A%2F%2Fstackoverflow.com%2Fquestions%2F11758809%2Fwhat-is-the-optimal-algorithm-for-generating-an-unbiased-random-integer-within-a&sa=D&sntz=1&usg=AFQjCNEtQkf0HYEkTn6Npvmyu2TDKPQCxA
            uint modErr = (uint.MaxValue % excludedBound + 1) % excludedBound;
            uint safeIncBound = uint.MaxValue - modErr;

            while (true) {
                uint val = GetUInt32();
                if (val <= safeIncBound) {
                    return val % excludedBound;
                }
            }
        }

        [CLSCompliant(false), UsefulToKeep("library method")]
        public ulong GetUInt64(ulong bound)
        {
            ulong modErr = (ulong.MaxValue % bound + 1) % bound;
            ulong safeIncBound = ulong.MaxValue - modErr;

            while (true) {
                ulong val = GetUInt64();
                if (val <= safeIncBound) {
                    return val % bound;
                }
            }
        }

        public string GetStringOfLatinLower(int length) => GetString(length, 'a', 'z');
        public string GetStringCapitalized(int length) => GetString(1, 'A', 'Z') + GetString(length - 1, 'a', 'z');
        public string GetStringOfLatinUpperOrLower(int length) => GetStringUpperAndLower(length, 'a', 'z');
        public string GetStringOfNumbers(int length) => GetString(1, '1', '9') + GetString(length - 1, '0', '9');

        public string GetString(int length, char min, char max)
        {
            var letters = (uint)max - min + 1;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++) {
                sb.Append((char)(GetUInt32(letters) + min));
            }
            return sb.ToString();
        }

        public string GetStringUpperAndLower(int length, char min, char max)
        {
            var letters = (uint)max - min + 1;
            var MIN = char.ToUpper(min);
            var sb = new StringBuilder();
            for (var i = 0; i < length; i++) {
                sb.Append((char)(GetUInt32(letters) + (GetUInt32(100) < 50 ? min : MIN)));
            }
            return sb.ToString();
        }

        static readonly char[] UriPrintableCharacters =
            Enumerable.Range('A', 26).Concat(Enumerable.Range('a', 26)).Concat(Enumerable.Range('0', 10)).Select(i => (char)i).Concat("_-~").ToArray();

        public string GetStringOfUriPrintableCharacters(int length)
        {
            return new string(Enumerable.Range(0, length).Select(_ => UriPrintableCharacters[GetUInt32((uint)UriPrintableCharacters.Length)]).ToArray());
        }

        public static string GetPasswordString(int length)
        {
            return System.Web.Security.Membership.GeneratePassword(length, 0);
        }
    }
}
