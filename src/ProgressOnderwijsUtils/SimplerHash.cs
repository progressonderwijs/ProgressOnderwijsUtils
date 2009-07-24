using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace ProgressOnderwijsUtils
{
	public static class SimplerHash
	{
		public static bool MD5VerifyHash(string plainText, string hashString)
		{
			return hashString == MD5ComputeHash(plainText);
		}

		public static string MD5ComputeHash(string plainText)
		{
			using (var md5provider = new MD5CryptoServiceProvider())
				return string.Join("",
					md5provider.ComputeHash(Encoding.UTF8.GetBytes(plainText))
					.Select(byteVal => byteVal.ToString("x2"))
					.ToArray());
		}
	}
}
