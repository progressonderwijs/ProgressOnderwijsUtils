using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ProgressOnderwijsUtils.Extensions
{
	public static class FileInfoExtension
	{
		public static string ReadToEnd(this FileInfo file)
		{
			using (var reader = file.OpenText())
				return reader.ReadToEnd();
		}	
	}
}
