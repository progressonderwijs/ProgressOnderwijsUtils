using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ProgressOnderwijsUtils
{
	public static class ETagMD5Helper
	{

		const uint IRRELEVANT_SALT = 0xa910d726;
		public static string UniqueETag(int pageToken)
		{
			using(MemoryStream ms = new MemoryStream())
			using (BinaryWriter bw = new BinaryWriter(ms))
			{
				bw.Write(pageToken);
				bw.Write(DateTime.Now.Ticks);
				bw.Write(IRRELEVANT_SALT);
				bw.Flush();
				byte[] hash = System.Security.Cryptography.MD5.Create().ComputeHash(ms.ToArray());
				return Convert.ToBase64String(hash);
			}
		}
	}
}
