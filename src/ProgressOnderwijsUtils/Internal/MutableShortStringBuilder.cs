using System.Buffers;

namespace ProgressOnderwijsUtils.Internal;

/// <summary>
/// faster than StringBuilder since we don't need insert-in-the-middle capability and can reuse this memory
/// </summary>
struct MutableShortStringBuilder
{
    public const int InitialBufferSize = 4096;
    public char[] CurrentCharacterBuffer;
    public int CurrentLength;

    public static MutableShortStringBuilder Create(int length = InitialBufferSize)
        => new() { CurrentCharacterBuffer = Allocate(length), };

    static char[] Allocate(int length)
        => ArrayPool<char>.Shared.Rent(length);

    void Free()
        => ArrayPool<char>.Shared.Return(CurrentCharacterBuffer);

    public void AppendText(ReadOnlySpan<char> text)
    {
        checked {
            if (CurrentCharacterBuffer.Length < CurrentLength + text.Length) {
                var newLen = Math.Max(CurrentCharacterBuffer.Length * 2, CurrentLength + text.Length);
                var newArray = Allocate(newLen);
                Array.Copy(CurrentCharacterBuffer, newArray, CurrentLength);
                Free();
                CurrentCharacterBuffer = newArray;
            }
            text.CopyTo(CurrentCharacterBuffer.AsSpan(CurrentLength));
            CurrentLength += text.Length;
        }
    }

    public void DiscardBuilder()
    {
        Free();
        // ReSharper disable once NullableWarningSuppressionIsUsed
        CurrentCharacterBuffer = null!; // intentionally corrupt state on dispose so that invalid use-after-dispose fails fast
    }

    public string FinishBuilding()
    {
        var str = new string(CurrentCharacterBuffer, 0, CurrentLength);
        DiscardBuilder();
        return str;
    }
}
