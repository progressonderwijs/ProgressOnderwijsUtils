namespace ProgressOnderwijsUtils.Tests;

public sealed class XmlMinimizerTest
{
    [Fact]
    public void TrivialDocIsNotChanged()
    {
        var doc = XDocument.Parse("<test/>");
        XmlMinimizer.CleanupNamespaces(doc);
        var output = doc.ToString(SaveOptions.DisableFormatting);
        PAssert.That(() => output == "<test />");
    }

    [Fact]
    public void CommentsCanBeRemoves()
    {
        var doc = XDocument.Parse("<test><!--This is a comment --></test>");
        XmlMinimizer.RemoveComments(doc);
        var output = doc.ToString(SaveOptions.DisableFormatting);
        PAssert.That(() => output == "<test />");
    }

    [Fact]
    public void DocumentWithUnusedRootNamespaceHasNamespaceRemoved()
    {
        var doc = XDocument.Parse("<test xmlns:bla='bla'/>");
        XmlMinimizer.CleanupNamespaces(doc);
        var output = doc.ToString(SaveOptions.DisableFormatting);
        PAssert.That(() => output == "<test />");
    }

    [Fact]
    public void DocumentWithDefaultNsIsUnchanged()
    {
        var doc = XDocument.Parse("<test xmlns='bla'/>");
        XmlMinimizer.CleanupNamespaces(doc);
        var output = doc.ToString(SaveOptions.DisableFormatting);
        PAssert.That(() => output == "<test xmlns=\"bla\" />");
    }

    [Fact]
    public void XmlDeclarationHasNoImpact()
    {
        var doc = XDocument.Parse("<?xml version=\"1.0\"?><test xmlns='bla'/>");
        XmlMinimizer.CleanupNamespaces(doc);
        var output = doc.ToString(SaveOptions.DisableFormatting);
        PAssert.That(() => output == "<test xmlns=\"bla\" />");
    }

    [Fact]
    public void DocumentWithChildNamespaceIsUnchanged()
    {
        var doc = XDocument.Parse("<test xmlns='bla'><hmm:this xmlns:hmm='foo'/></test>");
        XmlMinimizer.CleanupNamespaces(doc);
        var output = doc.ToString(SaveOptions.DisableFormatting);
        PAssert.That(() => output == "<test xmlns=\"bla\"><hmm:this xmlns:hmm=\"foo\" /></test>");
    }

    [Fact]
    public void DocumentWithNamespaceUsedInDescendantIsUnchanged()
    {
        var doc = XDocument.Parse("<test xmlns:hmm='bla'><hmm:this /></test>");
        XmlMinimizer.CleanupNamespaces(doc);
        var output = doc.ToString(SaveOptions.DisableFormatting);
        PAssert.That(() => output == "<test xmlns:hmm=\"bla\"><hmm:this /></test>");
    }

    [Fact]
    public void DocumentWithNamespaceUsedInDescendantAttributeIsUnchanged()
    {
        var doc = XDocument.Parse("<test xmlns:hmm='bla'><this><that hmm:id='' /></this></test>");
        XmlMinimizer.CleanupNamespaces(doc);
        var output = doc.ToString(SaveOptions.DisableFormatting);
        PAssert.That(() => output == "<test xmlns:hmm=\"bla\"><this><that hmm:id=\"\" /></this></test>");
    }

    [Fact]
    public void UnusualAliasesAreNormalized()
    {
        var doc = XDocument.Parse("<test xmlns:notxsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:notxsi=\"http://www.w3.org/2001/XMLSchema-instance\"><this notxsd:foo='' notxsi:nil='true' /></test>");
        XmlMinimizer.CleanupNamespaces(doc);
        var output = doc.ToString(SaveOptions.DisableFormatting);
        PAssert.That(() => output == "<test xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><this xsd:foo=\"\" xsi:nil=\"true\" /></test>");
    }

    [Fact]
    public void UnusedAreRemoved()
    {
        var doc = XDocument.Parse("<test xmlns:notxsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:notxsi=\"http://www.w3.org/2001/XMLSchema-instance\"><this notxsi:nil='true' /></test>");
        XmlMinimizer.CleanupNamespaces(doc);
        var output = doc.ToString(SaveOptions.DisableFormatting);
        PAssert.That(() => output == "<test xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><this xsi:nil=\"true\" /></test>");
    }

    static readonly Encoding UTF8 = Encoding.UTF8;

    [Fact]
    public void SaveToUtf8ContainsNoByteOrderMark()
    {
        var doc = XDocument.Parse("<test>Ƒϕϕ</test>");
        var bytes = XmlMinimizer.ToUtf8(doc);
        PAssert.That(() => bytes[0] == (byte)'<');
    }

    [Fact]
    public void SaveToUtf8IsUtf8()
    {
        var doc = XDocument.Parse("<test>Ƒϕϕ</test>");
        var bytes = XmlMinimizer.ToUtf8(doc);
        var str = UTF8.GetString(bytes);

        PAssert.That(() => str == "<test>Ƒϕϕ</test>");
    }

    [Fact]
    public void SaveToUtf8CanRoundTrip()
    {
        var doc = XDocument.Parse("<test>Ƒϕϕ</test>");
        var bytes = XmlMinimizer.ToUtf8(doc);
        var reloaded = XmlMinimizer.FromUtf8(bytes);

        var str = reloaded.ToString();

        PAssert.That(() => str == "<test>Ƒϕϕ</test>");
    }

    [Fact]
    public void SaveToUtf8ExcludesXmlDeclaration()
    {
        var doc = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf16\"?><test>Ƒϕϕ</test>");

        var bytes = XmlMinimizer.ToUtf8(doc);
        var str = UTF8.GetString(bytes);

        PAssert.That(() => str == "<test>Ƒϕϕ</test>");
    }

    [Fact]
    public void SaveToUtf8ExcludesIndentation()
    {
        var doc = XDocument.Parse(
            @"<test>
    <nested>
        <elements>
            <here>
                Ƒϕϕ
            </here>
        </elements>
    </nested>
</test>"
        );

        var utf8BytesFromXml = XmlMinimizer.ToUtf8(doc);
        var stringFromBytes = UTF8.GetString(utf8BytesFromXml);

        //XDocument.Parse/Serialize appears to sometimes lose CR's
        //The behavior differs at least between net462 on windows and netcoreapp20 on linux
        PAssert.That(
            () => stringFromBytes.Replace("\r", "") == @"<test><nested><elements><here>
                Ƒϕϕ
            </here></elements></nested></test>".Replace("\r", "")
        );
    }
}
