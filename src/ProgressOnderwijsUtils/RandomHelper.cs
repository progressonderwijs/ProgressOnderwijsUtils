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
        fillWithRandomBytes(bytes);
        return bytes;
    }

    public byte GetByte()
        => GetBytes(1)[0];

    public int GetNonNegativeInt32()
        => (int)GetUInt32((uint)int.MaxValue + 1);

    public int GetInt32()
        => BitConverter.ToInt32(GetBytes(sizeof(int)), 0);

    public long GetInt64()
        => BitConverter.ToInt64(GetBytes(sizeof(long)), 0);

    public uint GetUInt32()
        => BitConverter.ToUInt32(GetBytes(sizeof(uint)), 0);

    public ulong GetUInt64()
        => BitConverter.ToUInt64(GetBytes(sizeof(ulong)), 0);

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
    {
        var letters = (uint)max - min + 1;
        var sb = new StringBuilder();
        for (var i = 0; i < length; i++) {
            _ = sb.Append((char)(GetUInt32(letters) + min));
        }
        return sb.ToString();
    }

    public string GetStringUpperAndLower(int length, char min, char max)
    {
        var letters = (uint)max - min + 1;
        var MIN = char.ToUpper(min);
        var sb = new StringBuilder();
        for (var i = 0; i < length; i++) {
            _ = sb.Append((char)(GetUInt32(letters) + (GetUInt32(100) < 50 ? min : MIN)));
        }
        return sb.ToString();
    }

    static readonly char[] UriPrintableCharacters =
        Enumerable.Range('A', 26).Concat(Enumerable.Range('a', 26)).Concat(Enumerable.Range('0', 10)).Select(i => (char)i).Concat("_-~").ToArray();

    public string GetStringOfUriPrintableCharacters(int length)
        => new(Enumerable.Range(0, length).Select(_ => UriPrintableCharacters[GetUInt32((uint)UriPrintableCharacters.Length)]).ToArray());
}
