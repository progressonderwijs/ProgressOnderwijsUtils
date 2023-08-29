namespace ProgressOnderwijsUtils.Tests;

public sealed class RandomHelperTest
{
    static IEnumerable<int> Iter10K()
        => Enumerable.Repeat(0, 10000);

    [Fact]
    public void Check_AllNumbersHit()
    {
        var numTo37 = Enumerable.Range(0, 37).Select(i => (uint)i).ToHashSet();
        var randumNumTo37s = Iter10K().Select(i => RandomHelper.Secure.GetUInt32(37));
        PAssert.That(() => numTo37.SetEquals(randumNumTo37s)); //kans op fout ~= 37 * (1-1/37)^10000  < 10^-117
    }

    [Fact]
    public void CheckBasic_UInt32()
    {
        var uint32s = Iter10K().Select(i => RandomHelper.Secure.GetUInt32());
        PAssert.That(() => uint32s.Any(num => num > uint.MaxValue / 4 * 3));
        PAssert.That(() => uint32s.Any(num => num < uint.MaxValue / 4));
    }

    [Fact]
    public void CheckBasic_UInt64()
    {
        var uint64s = Iter10K().Select(i => RandomHelper.Secure.GetUInt64());
        PAssert.That(() => uint64s.Any(num => num > ulong.MaxValue / 4 * 3));
        PAssert.That(() => uint64s.Any(num => num < ulong.MaxValue / 4));
    }

    [Fact]
    public void CheckBasic_Int64()
    {
        var int64s = Iter10K().Select(i => RandomHelper.Secure.GetInt64());
        PAssert.That(() => int64s.Any(num => num > long.MaxValue / 4 * 3));
        PAssert.That(() => int64s.Any(num => num < long.MinValue / 4 * 3));
    }

    [Fact]
    public void CheckBasic_Int32()
    {
        var int64s = Iter10K().Select(i => RandomHelper.Secure.GetInt32());
        PAssert.That(() => int64s.Any(num => num > int.MaxValue / 4 * 3));
        PAssert.That(() => int64s.Any(num => num < int.MinValue / 4 * 3));
    }

    [Fact]
    public void Check_GetStringOfLatinLower()
    {
        for (var i = 0; i < 50; i++) {
            var len = (int)RandomHelper.Secure.GetUInt32(300);
            var str = RandomHelper.Secure.GetStringOfLatinLower(len);
            PAssert.That(() => str.Length == len);
            PAssert.That(() => str.AsEnumerable().None(c => c < 'a' || c > 'z'));
        }
    }

    [Fact]
    public void Check_GetStringOfLatinUpperOrLower()
    {
        for (var i = 0; i < 50; i++) {
            var len = (int)RandomHelper.Secure.GetUInt32(300);
            var StR = RandomHelper.Secure.GetStringOfLatinUpperOrLower(len);
            PAssert.That(() => StR.Length == len);
            PAssert.That(() => StR.AsEnumerable().None(c => (c < 'a' || c > 'z') && (c < 'A' || c > 'Z')));
        }
    }

    [Fact]
    public void CheckUriPrintable()
    {
        for (var i = 0; i < 50; i++) {
            var str = RandomHelper.Secure.GetStringOfUriPrintableCharacters(i);
            var escaped = Uri.EscapeDataString(str);
            PAssert.That(() => str == escaped);
            PAssert.That(() => str.Length == i);
            PAssert.That(() => Regex.IsMatch(str, "^[-_~0-9A-Za-z]*$"));
        }
    }

    [Fact]
    public void CheckStrings()
    {
        PAssert.That(() => Regex.IsMatch(RandomHelper.Secure.GetStringOfNumbers(10), "[0-9]{10}"));
        PAssert.That(() => Regex.IsMatch(RandomHelper.Secure.GetStringCapitalized(10), "[A-Z][a-z]{9}"));
        PAssert.That(() => Regex.IsMatch(RandomHelper.Secure.GetStringOfLatinLower(7), "[a-z]{7}"));
        PAssert.That(() => Regex.IsMatch(RandomHelper.Secure.GetStringOfLatinUpperOrLower(10), "[a-zA-Z]{10}"));
    }

    [Fact]
    public void ImplicitlyInsecure_Gedrag_is_deterministisch()
    {
        var randomHelper = RandomHelper.ImplicitlyInsecure();
        var pseudoRandomInteger = randomHelper.GetInt32();
        PAssert.That(() => pseudoRandomInteger == -20762718);
    }
}
