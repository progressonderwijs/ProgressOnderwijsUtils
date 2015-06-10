using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ProgressOnderwijsUtils
{
    public static class SimplerHash
    {
        public static bool MD5VerifyHash(string plainText, string hashString) => hashString == MD5ComputeHash(plainText);

        public static string MD5ComputeHash(string plainText)
        {
            return MakeMD5(Encoding.UTF8.GetBytes(plainText));
        }

        static string MakeMD5(byte[] data)
        {
            using (var md5computer = MD5.Create()) {
                byte[] md5 = md5computer.ComputeHash(data);
                var sb = new StringBuilder();
                for (int i = 0; i < md5.Length; i++) {
                    sb.Append(md5[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
