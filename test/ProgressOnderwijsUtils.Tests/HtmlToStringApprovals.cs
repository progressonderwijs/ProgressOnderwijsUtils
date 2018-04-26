using ProgressOnderwijsUtils.Html;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    using static Tags;

    public class HtmlToStringApprovals
    {
        [Fact]
        public void BasicDocWorks() => ApprovalTest.Verify(basicDoc.SerializeToString());

        [Fact]
        public void BasicDocWorksToCSharp() => ApprovalTest.Verify(basicDoc.ToCSharp());

        static readonly HtmlTagKinds.HTML basicDoc =
            _html.Content(
                _head.Content(
                    _title.Content("Hello world!")
                    ),
                _body.Content(
                    _p.Content("Hello world!")
                    )
                );

        [Fact]
        public void DocWithEmptyClass() => ApprovalTest.Verify(docWithEmptyClass.SerializeToString());

        [Fact]
        public void DocWithEmptyClassToCSharp() => ApprovalTest.Verify(docWithEmptyClass.ToCSharp());

        static readonly HtmlTagKinds.HTML docWithEmptyClass =
            _html.Content(
                _head.Content(
                    _title.Content("Hello world!")
                    ),
                _body._class(null).Content(
                    _p.Content("Hello world!")
                    )
                );

        [Fact]
        public void DocWithOneClass() => ApprovalTest.Verify(docWithOneClass.SerializeToString());

        [Fact]
        public void DocWithOneClassToCSharp() => ApprovalTest.Verify(docWithOneClass.ToCSharp());

        static readonly HtmlTagKinds.HTML docWithOneClass =
            _html.Content(
                _head.Content(
                    _title.Content("Hello world!")
                    ),
                _body._class("aClass").Content(
                    _p.Content("Hello world!")
                    )
                );

        [Fact]
        public void DocWithTwoClasses() => ApprovalTest.Verify(docWithTwoClasses.SerializeToString());

        [Fact]
        public void DocWithTwoClassesToCSharp() => ApprovalTest.Verify(docWithTwoClasses.ToCSharp());

        static readonly HtmlTagKinds.HTML docWithTwoClasses =
            _html.Content(
                _head.Content(
                    _title.Content("Hello world!")
                    ),
                _body._class("aClass")._class("bClass").Content(
                    _p.Content("Hello world!")
                    )
                );

        [Fact]
        public void DocWithOddChars() => ApprovalTest.Verify(docWithOddChars.SerializeToString());

        [Fact]
        public void DocWithOddCharsToCSharp() => ApprovalTest.Verify(docWithOddChars.ToCSharp());

        static readonly HtmlTagKinds.HTML docWithOddChars =
            _html.Content(
                _head.Content(
                    _title.Content("Hello world!")
                    ),
                _body.Content(
                    _p.Content("Hello world: a < b & \"b\" > 'c'; ")
                    )
                );

        [Fact]
        public void DocWithOddCharsInAttribute() => ApprovalTest.Verify(docWithOddCharsInAttribute.SerializeToString());

        [Fact]
        public void DocWithOddCharsInAttributeToCSharp() => ApprovalTest.Verify(docWithOddCharsInAttribute.ToCSharp());

        static readonly HtmlTagKinds.HTML docWithOddCharsInAttribute =
            _html.Content(
                _head.Content(
                    _title.Content("Hello world!")
                    ),
                _body.Content(
                    _p._title("Hello world: a < b & \"b\" > 'c'; ").Content("no content")
                    )
                );

        [Fact]
        public void DocWithSelfClosingTags() => ApprovalTest.Verify(docWithSelfClosingTags.SerializeToString());

        [Fact]
        public void DocWithSelfClosingTagsToCSharp() => ApprovalTest.Verify(docWithSelfClosingTags.ToCSharp());

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
        public void DocWithEscapableRawText() => ApprovalTest.Verify(docWithEscapableRawText.SerializeToString());

        [Fact]
        public void DocWithEscapableRawTextToCSharp() => ApprovalTest.Verify(docWithEscapableRawText.ToCSharp());

        static readonly HtmlTagKinds.HTML docWithEscapableRawText =
            _html.Content(
                _head.Content(
                    _title.Content("Hello world: a < b & \"b\" > 'c'; ")
                    ),
                _body.Content(
                    _textarea.Content("Hello world: a < b & \"b\" > 'c'; ")
                    )
                );

        [Fact]
        public void DocWithTrulyRawText() => ApprovalTest.Verify(docWithTrulyRawText.SerializeToString());

        [Fact]
        public void DocWithTrulyRawTextToCSharp() => ApprovalTest.Verify(docWithTrulyRawText.ToCSharp());

        static readonly HtmlTagKinds.HTML docWithTrulyRawText =
            _html.Content(
                _head.Content(
                    _script._type("mytype").Content("Hello world: a < b & \"b\" > 'c'; </style> "),
                    _style._id("myId").Content("Hello world: a < b & \"b\" > 'c'; </script> ")
                    ),
                _body.Content(
                    _p.Content("Hello world!")
                    )
                );
    }
}
