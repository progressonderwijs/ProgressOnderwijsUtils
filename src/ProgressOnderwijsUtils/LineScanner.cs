using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public sealed class LineScanner
    {
        readonly string[] lines;
        int position;

        public LineScanner([NotNull] string s)
        {
            lines = Regex.Split(s, "\r\n|\n");
        }

        [CanBeNull]
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
