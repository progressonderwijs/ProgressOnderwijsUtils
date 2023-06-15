namespace ProgressOnderwijsUtils.Internal;

interface IStringSink
{
    void AppendText(ReadOnlySpan<char> text);
}
