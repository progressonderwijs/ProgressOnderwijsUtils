using System.Runtime.CompilerServices;
using ProgressOnderwijsUtils.Html;
using Xunit;

namespace ProgressOnderwijsUtilsTests
{
    using static Tags;

    public class HtmlToStringApprovals
    {
        [Fact]
        public void BasicDocWorks()
        {
            ApprovalTest.Verify(
                _html.Content(
                    _head.Content(
                        _title.Content("Hello world!")
                        ),
                    _body.Content(
                        _p.Content("Hello world!")
                        )
                    ).AsFragment().SerializeToString()
                );
        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DocWithEmptyClass()
        {
            ApprovalTest.Verify(
                _html.Content(
                    _head.Content(
                        _title.Content("Hello world!")
                        ),
                    _body._class(null).Content(
                        _p.Content("Hello world!")
                        )
                    ).AsFragment().SerializeToString()
                );
        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DocWithOneClass()
        {
            ApprovalTest.Verify(
                _html.Content(
                    _head.Content(
                        _title.Content("Hello world!")
                        ),
                    _body._class("aClass").Content(
                        _p.Content("Hello world!")
                        )
                    ).AsFragment().SerializeToString()
                );
        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DocWithTwoClasses()
        {
            ApprovalTest.Verify(
                _html.Content(
                    _head.Content(
                        _title.Content("Hello world!")
                        ),
                    _body._class("aClass")._class("bClass").Content(
                        _p.Content("Hello world!")
                        )
                    ).AsFragment().SerializeToString()
                );
        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DocWithOddChars()
        {
            ApprovalTest.Verify(
                _html.Content(
                    _head.Content(
                        _title.Content("Hello world!")
                        ),
                    _body.Content(
                        _p.Content("Hello world: a < b & \"b\" > 'c'; ")
                        )
                    ).AsFragment().SerializeToString()
                );
        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DocWithOddCharsInAttribute()
        {
            ApprovalTest.Verify(
                _html.Content(
                    _head.Content(
                        _title.Content("Hello world!")
                        ),
                    _body.Content(
                        _p._title("Hello world: a < b & \"b\" > 'c'; ").Content("no content")
                        )
                    ).AsFragment().SerializeToString()
                );
        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DocWithSelfClosingTags()
        {
            ApprovalTest.Verify(
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
                    ).AsFragment().SerializeToString()
                );
        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DocWithEscapableRawText()
        {
            ApprovalTest.Verify(
                _html.Content(
                    _head.Content(
                        _title.Content("Hello world: a < b & \"b\" > 'c'; ")
                        ),
                    _body.Content(
                        _textarea.Content("Hello world: a < b & \"b\" > 'c'; ")
                        )
                    ).AsFragment().SerializeToString()
                );
        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DocWithTrulyRawText()
        {
            ApprovalTest.Verify(
                _html.Content(
                    _head.Content(
                        _script._type("mytype").Content("Hello world: a < b & \"b\" > 'c'; </style> "),
                        _style._type("mytype").Content("Hello world: a < b & \"b\" > 'c'; </script> ")
                        ),
                    _body.Content(
                        _p.Content("Hello world!")
                        )
                    ).AsFragment().SerializeToString()
                );
        }
    }
}
