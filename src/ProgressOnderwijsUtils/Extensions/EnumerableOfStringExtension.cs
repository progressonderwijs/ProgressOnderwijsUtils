﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public static class EnumerableOfStringExtension
	{
		public static string Join(this IEnumerable<string> strings)
		{
			StringBuilder sb = new StringBuilder();
			foreach (string s in strings)
				sb.Append(s);
			return sb.ToString();
		}
		public static string Join(this IEnumerable<string> strings, string separator)
		{
			StringBuilder sb = new StringBuilder();
			bool addsep = false;
			foreach (string s in strings)
			{
				if (addsep)
					sb.Append(separator);
				else 
					addsep = true;
				sb.Append(s);
			}
			return sb.ToString();
		}

	}
}
