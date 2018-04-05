using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class SimplerHash
    {
        public static bool MD5VerifyHash([NotNull] string plainText, string hashString) => hashString == MD5ComputeHash(plainText);

        [NotNull]
        public static string MD5ComputeHash([NotNull] string plainText)
        {
            return MakeMD5(Encoding.UTF8.GetBytes(plainText));
        }

        [NotNull]
        static string MakeMD5([NotNull] byte[] data)
        {
            using (var md5computer = MD5.Create()) {
                var md5 = md5computer.ComputeHash(data);
                var sb = new StringBuilder();
                for (var i = 0; i < md5.Length; i++) {
                    sb.Append(md5[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
