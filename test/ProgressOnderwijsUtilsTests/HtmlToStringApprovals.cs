using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ApprovalTests;
using NUnit.Framework;
using ProgressOnderwijsUtils.Html;

namespace ProgressOnderwijsUtilsTests
{
    using static Tags;

    public class HtmlToStringApprovals
    {
        [Test, MethodImpl(MethodImplOptions.NoInlining)]
        public void BasicDocWorks()
        {
            Approvals.Verify(
                _html.Content(
                    _head.Content(
                        _title.Content("Hello world!")
                        ),
                    _body.Content(
                        _p.Content("Hello world!")
                        )
                    ).ToFragment().SerializeToString()
                );
        }

        [Test, MethodImpl(MethodImplOptions.NoInlining)]
        public void DocWithOddChars()
        {
            Approvals.Verify(
                _html.Content(
                    _head.Content(
                        _title.Content("Hello world!")
                        ),
                    _body.Content(
                        _p.Content("Hello world: a < b & \"b\" > 'c'; ")
                        )
                    ).ToFragment().SerializeToString()
                );
        }

        [Test, MethodImpl(MethodImplOptions.NoInlining)]
        public void DocWithOddCharsInAttribute()
        {
            Approvals.Verify(
                _html.Content(
                    _head.Content(
                        _title.Content("Hello world!")
                        ),
                    _body.Content(
                        _p._title("Hello world: a < b & \"b\" > 'c'; ").Content("no content")
                        )
                    ).ToFragment().SerializeToString()
                );
        }

        [Test, MethodImpl(MethodImplOptions.NoInlining)]
        public void DocWithSelfClosingTags()
        {
            Approvals.Verify(
                _html.Content(
                    _head.Content(
                        _title.Content("Hello world!"),
                        _meta._hidden("hmm"),
                        _link._rel("bla"),
                        _base._href("nowhere")
                        ),
                    _body.Content(
                        _area, _br, _col, _embed, _hr, _img, _input, _keygen, _menuitem, _param, _source, _track, _wbr
                        )
                    ).ToFragment().SerializeToString()
                );
        }

        [Test, MethodImpl(MethodImplOptions.NoInlining)]
        public void DocWithEscapableRawText()
        {
            Approvals.Verify(
                _html.Content(
                    _head.Content(
                        _title.Content("Hello world: a < b & \"b\" > 'c'; ")
                        ),
                    _body.Content(
                        _textarea.Content("Hello world: a < b & \"b\" > 'c'; ")
                        )
                    ).ToFragment().SerializeToString()
                );
        }
        [Test, MethodImpl(MethodImplOptions.NoInlining)]
        public void DocWithTrulyRawText()
        {
            Approvals.Verify(
                _html.Content(
                    _head.Content(
                        _script._type("mytype").Content("Hello world: a < b & \"b\" > 'c'; </style> "),
                        _style._type("mytype").Content("Hello world: a < b & \"b\" > 'c'; </script> ")
                        ),
                    _body.Content(
                        _p.Content("Hello world!")
                        )
                    ).ToFragment().SerializeToString()
                );
        }
    }
}
