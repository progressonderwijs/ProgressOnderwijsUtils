using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProgressOnderwijsUtils
{
    public class LineScanner
    {
        readonly string[] lines;
        int position;
        public LineScanner(string s) { lines = Regex.Split(s, "\r\n|\n"); }
        public string GetLine() => position != lines.Length ? lines[position++] : null;

        public void PushBack()
        {
            if (position != 0) {
                --position;
            }
        }

        public bool Eof() => position == lines.Length;
    }
}
