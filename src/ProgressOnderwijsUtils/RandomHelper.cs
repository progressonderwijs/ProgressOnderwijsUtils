using System.Runtime.InteropServices;

namespace ProgressOnderwijsUtils;

public sealed class RandomHelper
{
    delegate void FillBytes(Span<byte> bytes);

    public static readonly RandomHelper Secure = new(RandomNumberGenerator.Fill);

    public static RandomHelper Insecure(int seed)
        => new(new Random(seed).NextBytes);

    public static RandomHelper ImplicitlyInsecure([CallerLineNumber] int linenumber = -1, [CallerFilePath] string filepath = "", [CallerMemberName] string membername = "")
        => Insecure(GetNaiveHashCode(Path.GetFileName(filepath)) + 1337 * GetNaiveHashCode(membername));

    static int GetNaiveHashCode(string str)
        => (int)ColumnOrdering.CaseInsensitiveHash(str);

    readonly FillBytes fillWithRandomBytes;

    RandomHelper(FillBytes fillWithRandomBytes)
        => this.fillWithRandomBytes = fillWithRandomBytes;

    public byte[] GetBytes(int numBytes)
    {
        var bytes = new byte[numBytes];
        GetBytes(bytes);
        return bytes;
    }

    public void GetBytes(Span<byte> bytes)
        => fillWithRandomBytes(bytes);

    public byte GetByte()
        => GetSimpleType<byte>();

    public int GetNonNegativeInt32()
        => (int)GetUInt32((uint)int.MaxValue + 1);

    public int GetInt32()
        => GetSimpleType<int>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    T GetSimpleType<T>()
        where T : struct
    {
        var num = default(T);
        var span = new Span<T>(ref num);
        var bytes = MemoryMarshal.AsBytes(span);
        fillWithRandomBytes(bytes);
        return num;
    }

    public long GetInt64()
        => GetSimpleType<long>();

    public uint GetUInt32()
        => GetSimpleType<uint>();

    public ulong GetUInt64()
        => GetSimpleType<ulong>();

    public uint GetUInt32(uint excludedBound)
    {
        // Proved in: http://www.google.com/url?q=http%3A%2F%2Fstackoverflow.com%2Fquestions%2F11758809%2Fwhat-is-the-optimal-algorithm-for-generating-an-unbiased-random-integer-within-a&sa=D&sntz=1&usg=AFQjCNEtQkf0HYEkTn6Npvmyu2TDKPQCxA
        // ReSharper disable once UselessBinaryOperation Resharper bug: https://youtrack.jetbrains.com/issue/RSRP-486301
        var modErr = (uint.MaxValue % excludedBound + 1) % excludedBound;
        var safeIncBound = uint.MaxValue - modErr;

        while (true) {
            var val = GetUInt32();
            if (val <= safeIncBound) {
                return val % excludedBound;
            }
        }
    }

    public ulong GetUInt64(ulong bound)
    {
        var modErr = (ulong.MaxValue % bound + 1) % bound;
        var safeIncBound = ulong.MaxValue - modErr;

        while (true) {
            var val = GetUInt64();
            if (val <= safeIncBound) {
                return val % bound;
            }
        }
    }

    public string GetStringOfLatinLower(int length)
        => GetString(length, 'a', 'z');

    public string GetStringCapitalized(int length)
        => GetString(1, 'A', 'Z') + GetString(length - 1, 'a', 'z');

    public string GetStringOfLatinUpperOrLower(int length)
        => GetStringUpperAndLower(length, 'a', 'z');

    public string GetStringOfNumbers(int length)
        => GetString(1, '1', '9') + GetString(length - 1, '0', '9');

    public string GetString(int length, char min, char max)
        => string.Create(
            length,
            (min, max, this),
            static (buffer, o) => {
                var (min, max, rnd) = o;
                var letters = (uint)max - min + 1;
                foreach (ref var c in buffer) {
                    c = (char)(rnd.GetUInt32(letters) + min);
                }
            }
        );

    public string GetStringUpperAndLower(int length, char min, char max)
        => string.Create(
            length,
            (min, max, this),
            static (buffer, o) => {
                var (min, max, rnd) = o;
                var letters = (uint)max - min + 1;
                var MIN = char.ToUpper(min);
                foreach (ref var c in buffer) {
                    c = (char)(rnd.GetUInt32(letters) + (rnd.GetUInt32(100) < 50 ? min : MIN));
                }
            }
        );

    static readonly char[] UriPrintableCharacters =
        Enumerable.Range('A', 26).Concat(Enumerable.Range('a', 26)).Concat(Enumerable.Range('0', 10)).Select(i => (char)i).Concat("_-~").ToArray();

    public string GetStringOfUriPrintableCharacters(int length)
        => string.Create(
            length,
            this,
            static (buffer, rnd) => {
                foreach (ref var c in buffer) {
                    c = UriPrintableCharacters[rnd.GetUInt32((uint)UriPrintableCharacters.Length)];
                }
            }
        );
}
