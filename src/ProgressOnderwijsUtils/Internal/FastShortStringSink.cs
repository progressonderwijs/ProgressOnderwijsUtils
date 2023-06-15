namespace ProgressOnderwijsUtils.Internal;

/// <summary>
/// Reference-typed wrapper around the mutable struct MutableShortStringBuilder
/// </summary>
class FastShortStringSink : IStringSink
{
    public MutableShortStringBuilder Underlying = MutableShortStringBuilder.Create();

    public void AppendText(ReadOnlySpan<char> text)
        => Underlying.AppendText(text);
}
