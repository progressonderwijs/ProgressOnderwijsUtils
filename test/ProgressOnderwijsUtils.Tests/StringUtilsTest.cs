namespace ProgressOnderwijsUtils.Tests;

public sealed class StringUtilsTest
{
    [Fact]
    public void IsUpperAscii()
    {
        PAssert.That(() => !StringUtils.IsUpperAscii("  as"));
        PAssert.That(() => StringUtils.IsUpperAscii("XYZ"));
        PAssert.That(() => !StringUtils.IsUpperAscii("XYZ "));
        PAssert.That(() => !StringUtils.IsUpperAscii("b"));
    }

    [Fact]
    public void SepaTekensetEnModificaties()
    {
        PAssert.That(() => StringUtils.SepaTekensetEnModificaties("Antonín Dvořák") == "Antonin Dvorak");
        PAssert.That(() => StringUtils.SepaTekensetEnModificaties("@ßẞ!#$!@SDfef9{ɚ0 df0o4[[") == "ssssSDfef90 df0o4[[");
        PAssert.That(() => StringUtils.SepaTekensetEnModificaties("ΝΕΣΧΑΑΣΗ ΘΤΡ ΡΗΣΔΙΟ") == null);
        PAssert.That(() => StringUtils.SepaTekensetEnModificaties(null) == null);
    }

    [Fact]
    public void TryParseInt32()
    {
        PAssert.That(() => " 10 00 ".TryParseInt32() == null);
        PAssert.That(() => " 1000 ".TryParseInt32() == 1000);
        PAssert.That(() => default(string?).TryParseInt32() == null);
    }

    [Fact]
    public void TryParseInt64()
    {
        PAssert.That(() => " 10 00 ".TryParseInt64() == null);
        PAssert.That(() => " 1000 ".TryParseInt64() == 1000);
        PAssert.That(() => default(string?).TryParseInt64() == null);
    }
}
