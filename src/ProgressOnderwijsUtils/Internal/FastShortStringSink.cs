namespace ProgressOnderwijsUtils.Internal;

/// <summary>
/// Reference-typed wrapper around the mutable struct MutableShortStringBuilder
/// </summary>
class FastShortStringSink : IStringSink
{
    public FastShortStringSink(int initialBufferSize = MutableShortStringBuilder.InitialBufferSize)
        => Underlying = MutableShortStringBuilder.Create(initialBufferSize);

    public MutableShortStringBuilder Underlying;

    public void AppendText(ReadOnlySpan<char> text)
        => Underlying.AppendText(text);
}
