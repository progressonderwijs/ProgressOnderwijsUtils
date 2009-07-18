using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace ProgressOnderwijsUtils
{
	public class LineScanner
	{
		private string[] lines;
		int position = 0;

		public LineScanner(Stream stream)
		{
			StreamReader streamreader = new StreamReader(stream);
			lines = Regex.Split(streamreader.ReadToEnd(), "\r\n|\n");
		}

		public LineScanner(string s)
		{
			lines = Regex.Split(s, "\r\n|\n");
		}

		public string GetLine()
		{
			return position != lines.Length ? lines[position++] : null;
		}

		public void PushBack()
		{
			if (position != 0)
				--position;
		}

		public bool Eof()
		{
			return position == lines.Length;
		}
	}
}
