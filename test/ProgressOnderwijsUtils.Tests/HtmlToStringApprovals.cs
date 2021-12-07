namespace ProgressOnderwijsUtils.Tests;

using static Tags;

public sealed class HtmlToStringApprovals
{
    [Fact]
    public void BasicDocWorks()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(basicDoc.ToStringWithDoctype());

    [Fact]
    public void BasicDocWorksToCSharp()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(basicDoc.ToCSharp());

    static readonly HtmlTagKinds.HTML basicDoc =
        _html.Content(
            _head.Content(_title.Content("Hello world!")),
            _body.Content(_p.Content("Hello world!"))
        );

    [Fact]
    public void DocWithEmptyClass()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithEmptyClass.ToStringWithDoctype());

    [Fact]
    public void DocWithEmptyClassToCSharp()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithEmptyClass.ToCSharp());

    static readonly HtmlTagKinds.HTML docWithEmptyClass =
        _html.Content(
            _head.Content(_title.Content("Hello world!")),
            _body._class(default(string)).Content(_p.Content("Hello world!"))
        );

    [Fact]
    public void DocWithOneClass()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithOneClass.ToStringWithDoctype());

    [Fact]
    public void DocWithOneClassToCSharp()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithOneClass.ToCSharp());

    static readonly HtmlTagKinds.HTML docWithOneClass =
        _html.Content(
            _head.Content(_title.Content("Hello world!")),
            _body._class("aClass").Content(_p.Content("Hello world!"))
        );

    [Fact]
    public void DocWithTwoClasses()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithTwoClasses.ToStringWithDoctype());

    [Fact]
    public void DocWithTwoClassesToCSharp()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithTwoClasses.ToCSharp());

    static readonly HtmlTagKinds.HTML docWithTwoClasses =
        _html.Content(
            _head.Content(_title.Content("Hello world!")),
            _body._class("aClass")._class("bClass").Content(_p.Content("Hello world!"))
        );

    [Fact]
    public void DocWithOddChars()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithOddChars.ToStringWithDoctype());

    [Fact]
    public void DocWithOddCharsToCSharp()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithOddChars.ToCSharp());

    static readonly HtmlTagKinds.HTML docWithOddChars =
        _html.Content(
            _head.Content(_title.Content("Hello world!")),
            _body.Content(_p.Content("Hello world: a < b & \"b\" > 'c'; "))
        );

    [Fact]
    public void DocWithOddCharsInAttribute()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithOddCharsInAttribute.ToStringWithDoctype());

    [Fact]
    public void DocWithOddCharsInAttributeToCSharp()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithOddCharsInAttribute.ToCSharp());

    static readonly HtmlTagKinds.HTML docWithOddCharsInAttribute =
        _html.Content(
            _head.Content(_title.Content("Hello world!")),
            _body.Content(_p._title("Hello world: a < b & \"b\" > 'c'; ").Content("no content"))
        );

    [Fact]
    public void DocWithSelfClosingTags()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithSelfClosingTags.ToStringWithDoctype());

    [Fact]
    public void DocWithSelfClosingTagsToCSharp()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithSelfClosingTags.ToCSharp());

    static readonly HtmlTagKinds.HTML docWithSelfClosingTags =
        _html.Content(
            _head.Content(
                _title.Content("Hello world!"),
                _meta._hidden("hmm"),
                _link._rel("bla"),
                _base._href("nowhere")
            ),
            _body.Content(
                new HtmlFragment[] {
                    _area, _br, _col, _embed, _hr, _img, _input, _param, _source, _track, _wbr
                }.JoinHtml("\r\n")
            )
        );

    [Fact]
    public void DocWithEscapableRawText()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithEscapableRawText.ToStringWithDoctype());

    [Fact]
    public void DocWithEscapableRawTextToCSharp()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithEscapableRawText.ToCSharp());

    static readonly HtmlTagKinds.HTML docWithEscapableRawText =
        _html.Content(
            _head.Content(_title.Content("Hello world: a < b & \"b\" > 'c'; ")),
            _body.Content(_textarea.Content("Hello world: a < b & \"b\" > 'c'; "))
        );

    [Fact]
    public void DocWithTrulyRawText()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithTrulyRawText.ToStringWithDoctype());

    [Fact]
    public void DocWithTrulyRawTextToCSharp()
        => ApprovalTest.CreateHere().AssertUnchangedAndSave(docWithTrulyRawText.ToCSharp());

    static readonly HtmlTagKinds.HTML docWithTrulyRawText =
        _html.Content(
            _head.Content(
                _script._type("mytype").Content("Hello world: a < b & \"b\" > 'c'; </style> "),
                _style._id("myId").Content("Hello world: a < b & \"b\" > 'c'; </script> ")
            ),
            _body.Content(_p.Content("Hello world!"))
        );
}