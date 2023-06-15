using System.Buffers;

namespace ProgressOnderwijsUtils.Internal;

/// <summary>
/// faster than StringBuilder since we don't need insert-in-the-middle capability and can reuse this memory
/// </summary>
struct MutableShortStringBuilder
{
    public char[] CurrentCharacterBuffer;
    public int CurrentLength;

    public static MutableShortStringBuilder Create()
        => new() { CurrentCharacterBuffer = Allocate(4096), };

    static char[] Allocate(int length)
        => ArrayPool<char>.Shared.Rent(length);

    void Free()
        => ArrayPool<char>.Shared.Return(CurrentCharacterBuffer);

    public static MutableShortStringBuilder Create(int length)
        => new() { CurrentCharacterBuffer = Allocate(length), };

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
        CurrentCharacterBuffer = null!;
    }

    public string FinishBuilding()
    {
        var str = new string(CurrentCharacterBuffer, 0, CurrentLength);
        DiscardBuilder();
        return str;
    }
}
