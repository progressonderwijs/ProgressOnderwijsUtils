using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    using AttributeNameInterfaces;

    public static class HtmlTagKinds
    {
        public struct A : IHtmlTagAllowingContent<A>, IHasAttr_href, IHasAttr_target, IHasAttr_download, IHasAttr_ping, IHasAttr_rel, IHasAttr_hreflang, IHasAttr_type, IHasAttr_referrerpolicy
        {
            public string TagName => "a";
            string IHtmlTag.TagStart => "<a";
            string IHtmlTag.EndTag => "</a>";
            HtmlAttributes attrs;
            A IHtmlTag<A>.WithAttributes(HtmlAttributes replacementAttributes) => new A { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            A IHtmlTagAllowingContent<A>.WithContents(HtmlFragment[] replacementContents) => new A { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(A tag) => tag.AsFragment();
        }
        public struct ABBR : IHtmlTagAllowingContent<ABBR>
        {
            public string TagName => "abbr";
            string IHtmlTag.TagStart => "<abbr";
            string IHtmlTag.EndTag => "</abbr>";
            HtmlAttributes attrs;
            ABBR IHtmlTag<ABBR>.WithAttributes(HtmlAttributes replacementAttributes) => new ABBR { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            ABBR IHtmlTagAllowingContent<ABBR>.WithContents(HtmlFragment[] replacementContents) => new ABBR { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(ABBR tag) => tag.AsFragment();
        }
        public struct ADDRESS : IHtmlTagAllowingContent<ADDRESS>
        {
            public string TagName => "address";
            string IHtmlTag.TagStart => "<address";
            string IHtmlTag.EndTag => "</address>";
            HtmlAttributes attrs;
            ADDRESS IHtmlTag<ADDRESS>.WithAttributes(HtmlAttributes replacementAttributes) => new ADDRESS { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            ADDRESS IHtmlTagAllowingContent<ADDRESS>.WithContents(HtmlFragment[] replacementContents) => new ADDRESS { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(ADDRESS tag) => tag.AsFragment();
        }
        public struct AREA : IHtmlTag<AREA>, IHasAttr_alt, IHasAttr_coords, IHasAttr_shape, IHasAttr_href, IHasAttr_target, IHasAttr_download, IHasAttr_ping, IHasAttr_rel, IHasAttr_referrerpolicy
        {
            public string TagName => "area";
            string IHtmlTag.TagStart => "<area";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            AREA IHtmlTag<AREA>.WithAttributes(HtmlAttributes replacementAttributes) => new AREA { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(AREA tag) => tag.AsFragment();
        }
        public struct ARTICLE : IHtmlTagAllowingContent<ARTICLE>
        {
            public string TagName => "article";
            string IHtmlTag.TagStart => "<article";
            string IHtmlTag.EndTag => "</article>";
            HtmlAttributes attrs;
            ARTICLE IHtmlTag<ARTICLE>.WithAttributes(HtmlAttributes replacementAttributes) => new ARTICLE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            ARTICLE IHtmlTagAllowingContent<ARTICLE>.WithContents(HtmlFragment[] replacementContents) => new ARTICLE { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(ARTICLE tag) => tag.AsFragment();
        }
        public struct ASIDE : IHtmlTagAllowingContent<ASIDE>
        {
            public string TagName => "aside";
            string IHtmlTag.TagStart => "<aside";
            string IHtmlTag.EndTag => "</aside>";
            HtmlAttributes attrs;
            ASIDE IHtmlTag<ASIDE>.WithAttributes(HtmlAttributes replacementAttributes) => new ASIDE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            ASIDE IHtmlTagAllowingContent<ASIDE>.WithContents(HtmlFragment[] replacementContents) => new ASIDE { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(ASIDE tag) => tag.AsFragment();
        }
        public struct AUDIO : IHtmlTagAllowingContent<AUDIO>, IHasAttr_src, IHasAttr_crossorigin, IHasAttr_preload, IHasAttr_autoplay, IHasAttr_loop, IHasAttr_muted, IHasAttr_controls
        {
            public string TagName => "audio";
            string IHtmlTag.TagStart => "<audio";
            string IHtmlTag.EndTag => "</audio>";
            HtmlAttributes attrs;
            AUDIO IHtmlTag<AUDIO>.WithAttributes(HtmlAttributes replacementAttributes) => new AUDIO { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            AUDIO IHtmlTagAllowingContent<AUDIO>.WithContents(HtmlFragment[] replacementContents) => new AUDIO { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(AUDIO tag) => tag.AsFragment();
        }
        public struct B : IHtmlTagAllowingContent<B>
        {
            public string TagName => "b";
            string IHtmlTag.TagStart => "<b";
            string IHtmlTag.EndTag => "</b>";
            HtmlAttributes attrs;
            B IHtmlTag<B>.WithAttributes(HtmlAttributes replacementAttributes) => new B { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            B IHtmlTagAllowingContent<B>.WithContents(HtmlFragment[] replacementContents) => new B { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(B tag) => tag.AsFragment();
        }
        public struct BASE : IHtmlTag<BASE>, IHasAttr_href, IHasAttr_target
        {
            public string TagName => "base";
            string IHtmlTag.TagStart => "<base";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            BASE IHtmlTag<BASE>.WithAttributes(HtmlAttributes replacementAttributes) => new BASE { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(BASE tag) => tag.AsFragment();
        }
        public struct BDI : IHtmlTagAllowingContent<BDI>
        {
            public string TagName => "bdi";
            string IHtmlTag.TagStart => "<bdi";
            string IHtmlTag.EndTag => "</bdi>";
            HtmlAttributes attrs;
            BDI IHtmlTag<BDI>.WithAttributes(HtmlAttributes replacementAttributes) => new BDI { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            BDI IHtmlTagAllowingContent<BDI>.WithContents(HtmlFragment[] replacementContents) => new BDI { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(BDI tag) => tag.AsFragment();
        }
        public struct BDO : IHtmlTagAllowingContent<BDO>
        {
            public string TagName => "bdo";
            string IHtmlTag.TagStart => "<bdo";
            string IHtmlTag.EndTag => "</bdo>";
            HtmlAttributes attrs;
            BDO IHtmlTag<BDO>.WithAttributes(HtmlAttributes replacementAttributes) => new BDO { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            BDO IHtmlTagAllowingContent<BDO>.WithContents(HtmlFragment[] replacementContents) => new BDO { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(BDO tag) => tag.AsFragment();
        }
        public struct BLOCKQUOTE : IHtmlTagAllowingContent<BLOCKQUOTE>, IHasAttr_cite
        {
            public string TagName => "blockquote";
            string IHtmlTag.TagStart => "<blockquote";
            string IHtmlTag.EndTag => "</blockquote>";
            HtmlAttributes attrs;
            BLOCKQUOTE IHtmlTag<BLOCKQUOTE>.WithAttributes(HtmlAttributes replacementAttributes) => new BLOCKQUOTE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            BLOCKQUOTE IHtmlTagAllowingContent<BLOCKQUOTE>.WithContents(HtmlFragment[] replacementContents) => new BLOCKQUOTE { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(BLOCKQUOTE tag) => tag.AsFragment();
        }
        public struct BODY : IHtmlTagAllowingContent<BODY>, IHasAttr_onafterprint, IHasAttr_onbeforeprint, IHasAttr_onbeforeunload, IHasAttr_onhashchange, IHasAttr_onlanguagechange, IHasAttr_onmessage, IHasAttr_onmessageerror, IHasAttr_onoffline, IHasAttr_ononline, IHasAttr_onpagehide, IHasAttr_onpageshow, IHasAttr_onpopstate, IHasAttr_onrejectionhandled, IHasAttr_onstorage, IHasAttr_onunhandledrejection, IHasAttr_onunload
        {
            public string TagName => "body";
            string IHtmlTag.TagStart => "<body";
            string IHtmlTag.EndTag => "</body>";
            HtmlAttributes attrs;
            BODY IHtmlTag<BODY>.WithAttributes(HtmlAttributes replacementAttributes) => new BODY { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            BODY IHtmlTagAllowingContent<BODY>.WithContents(HtmlFragment[] replacementContents) => new BODY { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(BODY tag) => tag.AsFragment();
        }
        public struct BR : IHtmlTag<BR>
        {
            public string TagName => "br";
            string IHtmlTag.TagStart => "<br";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            BR IHtmlTag<BR>.WithAttributes(HtmlAttributes replacementAttributes) => new BR { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(BR tag) => tag.AsFragment();
        }
        public struct BUTTON : IHtmlTagAllowingContent<BUTTON>, IHasAttr_autofocus, IHasAttr_disabled, IHasAttr_form, IHasAttr_formaction, IHasAttr_formenctype, IHasAttr_formmethod, IHasAttr_formnovalidate, IHasAttr_formtarget, IHasAttr_name, IHasAttr_type, IHasAttr_value
        {
            public string TagName => "button";
            string IHtmlTag.TagStart => "<button";
            string IHtmlTag.EndTag => "</button>";
            HtmlAttributes attrs;
            BUTTON IHtmlTag<BUTTON>.WithAttributes(HtmlAttributes replacementAttributes) => new BUTTON { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            BUTTON IHtmlTagAllowingContent<BUTTON>.WithContents(HtmlFragment[] replacementContents) => new BUTTON { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(BUTTON tag) => tag.AsFragment();
        }
        public struct CANVAS : IHtmlTagAllowingContent<CANVAS>, IHasAttr_width, IHasAttr_height
        {
            public string TagName => "canvas";
            string IHtmlTag.TagStart => "<canvas";
            string IHtmlTag.EndTag => "</canvas>";
            HtmlAttributes attrs;
            CANVAS IHtmlTag<CANVAS>.WithAttributes(HtmlAttributes replacementAttributes) => new CANVAS { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            CANVAS IHtmlTagAllowingContent<CANVAS>.WithContents(HtmlFragment[] replacementContents) => new CANVAS { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(CANVAS tag) => tag.AsFragment();
        }
        public struct CAPTION : IHtmlTagAllowingContent<CAPTION>
        {
            public string TagName => "caption";
            string IHtmlTag.TagStart => "<caption";
            string IHtmlTag.EndTag => "</caption>";
            HtmlAttributes attrs;
            CAPTION IHtmlTag<CAPTION>.WithAttributes(HtmlAttributes replacementAttributes) => new CAPTION { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            CAPTION IHtmlTagAllowingContent<CAPTION>.WithContents(HtmlFragment[] replacementContents) => new CAPTION { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(CAPTION tag) => tag.AsFragment();
        }
        public struct CITE : IHtmlTagAllowingContent<CITE>
        {
            public string TagName => "cite";
            string IHtmlTag.TagStart => "<cite";
            string IHtmlTag.EndTag => "</cite>";
            HtmlAttributes attrs;
            CITE IHtmlTag<CITE>.WithAttributes(HtmlAttributes replacementAttributes) => new CITE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            CITE IHtmlTagAllowingContent<CITE>.WithContents(HtmlFragment[] replacementContents) => new CITE { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(CITE tag) => tag.AsFragment();
        }
        public struct CODE : IHtmlTagAllowingContent<CODE>
        {
            public string TagName => "code";
            string IHtmlTag.TagStart => "<code";
            string IHtmlTag.EndTag => "</code>";
            HtmlAttributes attrs;
            CODE IHtmlTag<CODE>.WithAttributes(HtmlAttributes replacementAttributes) => new CODE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            CODE IHtmlTagAllowingContent<CODE>.WithContents(HtmlFragment[] replacementContents) => new CODE { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(CODE tag) => tag.AsFragment();
        }
        public struct COL : IHtmlTag<COL>, IHasAttr_span
        {
            public string TagName => "col";
            string IHtmlTag.TagStart => "<col";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            COL IHtmlTag<COL>.WithAttributes(HtmlAttributes replacementAttributes) => new COL { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(COL tag) => tag.AsFragment();
        }
        public struct COLGROUP : IHtmlTagAllowingContent<COLGROUP>, IHasAttr_span
        {
            public string TagName => "colgroup";
            string IHtmlTag.TagStart => "<colgroup";
            string IHtmlTag.EndTag => "</colgroup>";
            HtmlAttributes attrs;
            COLGROUP IHtmlTag<COLGROUP>.WithAttributes(HtmlAttributes replacementAttributes) => new COLGROUP { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            COLGROUP IHtmlTagAllowingContent<COLGROUP>.WithContents(HtmlFragment[] replacementContents) => new COLGROUP { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(COLGROUP tag) => tag.AsFragment();
        }
        public struct DATA : IHtmlTagAllowingContent<DATA>, IHasAttr_value
        {
            public string TagName => "data";
            string IHtmlTag.TagStart => "<data";
            string IHtmlTag.EndTag => "</data>";
            HtmlAttributes attrs;
            DATA IHtmlTag<DATA>.WithAttributes(HtmlAttributes replacementAttributes) => new DATA { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            DATA IHtmlTagAllowingContent<DATA>.WithContents(HtmlFragment[] replacementContents) => new DATA { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(DATA tag) => tag.AsFragment();
        }
        public struct DATALIST : IHtmlTagAllowingContent<DATALIST>
        {
            public string TagName => "datalist";
            string IHtmlTag.TagStart => "<datalist";
            string IHtmlTag.EndTag => "</datalist>";
            HtmlAttributes attrs;
            DATALIST IHtmlTag<DATALIST>.WithAttributes(HtmlAttributes replacementAttributes) => new DATALIST { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            DATALIST IHtmlTagAllowingContent<DATALIST>.WithContents(HtmlFragment[] replacementContents) => new DATALIST { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(DATALIST tag) => tag.AsFragment();
        }
        public struct DD : IHtmlTagAllowingContent<DD>
        {
            public string TagName => "dd";
            string IHtmlTag.TagStart => "<dd";
            string IHtmlTag.EndTag => "</dd>";
            HtmlAttributes attrs;
            DD IHtmlTag<DD>.WithAttributes(HtmlAttributes replacementAttributes) => new DD { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            DD IHtmlTagAllowingContent<DD>.WithContents(HtmlFragment[] replacementContents) => new DD { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(DD tag) => tag.AsFragment();
        }
        public struct DEL : IHtmlTagAllowingContent<DEL>, IHasAttr_cite, IHasAttr_datetime
        {
            public string TagName => "del";
            string IHtmlTag.TagStart => "<del";
            string IHtmlTag.EndTag => "</del>";
            HtmlAttributes attrs;
            DEL IHtmlTag<DEL>.WithAttributes(HtmlAttributes replacementAttributes) => new DEL { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            DEL IHtmlTagAllowingContent<DEL>.WithContents(HtmlFragment[] replacementContents) => new DEL { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(DEL tag) => tag.AsFragment();
        }
        public struct DETAILS : IHtmlTagAllowingContent<DETAILS>, IHasAttr_open
        {
            public string TagName => "details";
            string IHtmlTag.TagStart => "<details";
            string IHtmlTag.EndTag => "</details>";
            HtmlAttributes attrs;
            DETAILS IHtmlTag<DETAILS>.WithAttributes(HtmlAttributes replacementAttributes) => new DETAILS { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            DETAILS IHtmlTagAllowingContent<DETAILS>.WithContents(HtmlFragment[] replacementContents) => new DETAILS { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(DETAILS tag) => tag.AsFragment();
        }
        public struct DFN : IHtmlTagAllowingContent<DFN>
        {
            public string TagName => "dfn";
            string IHtmlTag.TagStart => "<dfn";
            string IHtmlTag.EndTag => "</dfn>";
            HtmlAttributes attrs;
            DFN IHtmlTag<DFN>.WithAttributes(HtmlAttributes replacementAttributes) => new DFN { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            DFN IHtmlTagAllowingContent<DFN>.WithContents(HtmlFragment[] replacementContents) => new DFN { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(DFN tag) => tag.AsFragment();
        }
        public struct DIALOG : IHtmlTagAllowingContent<DIALOG>, IHasAttr_open
        {
            public string TagName => "dialog";
            string IHtmlTag.TagStart => "<dialog";
            string IHtmlTag.EndTag => "</dialog>";
            HtmlAttributes attrs;
            DIALOG IHtmlTag<DIALOG>.WithAttributes(HtmlAttributes replacementAttributes) => new DIALOG { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            DIALOG IHtmlTagAllowingContent<DIALOG>.WithContents(HtmlFragment[] replacementContents) => new DIALOG { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(DIALOG tag) => tag.AsFragment();
        }
        public struct DIV : IHtmlTagAllowingContent<DIV>
        {
            public string TagName => "div";
            string IHtmlTag.TagStart => "<div";
            string IHtmlTag.EndTag => "</div>";
            HtmlAttributes attrs;
            DIV IHtmlTag<DIV>.WithAttributes(HtmlAttributes replacementAttributes) => new DIV { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            DIV IHtmlTagAllowingContent<DIV>.WithContents(HtmlFragment[] replacementContents) => new DIV { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(DIV tag) => tag.AsFragment();
        }
        public struct DL : IHtmlTagAllowingContent<DL>
        {
            public string TagName => "dl";
            string IHtmlTag.TagStart => "<dl";
            string IHtmlTag.EndTag => "</dl>";
            HtmlAttributes attrs;
            DL IHtmlTag<DL>.WithAttributes(HtmlAttributes replacementAttributes) => new DL { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            DL IHtmlTagAllowingContent<DL>.WithContents(HtmlFragment[] replacementContents) => new DL { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(DL tag) => tag.AsFragment();
        }
        public struct DT : IHtmlTagAllowingContent<DT>
        {
            public string TagName => "dt";
            string IHtmlTag.TagStart => "<dt";
            string IHtmlTag.EndTag => "</dt>";
            HtmlAttributes attrs;
            DT IHtmlTag<DT>.WithAttributes(HtmlAttributes replacementAttributes) => new DT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            DT IHtmlTagAllowingContent<DT>.WithContents(HtmlFragment[] replacementContents) => new DT { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(DT tag) => tag.AsFragment();
        }
        public struct EM : IHtmlTagAllowingContent<EM>
        {
            public string TagName => "em";
            string IHtmlTag.TagStart => "<em";
            string IHtmlTag.EndTag => "</em>";
            HtmlAttributes attrs;
            EM IHtmlTag<EM>.WithAttributes(HtmlAttributes replacementAttributes) => new EM { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            EM IHtmlTagAllowingContent<EM>.WithContents(HtmlFragment[] replacementContents) => new EM { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(EM tag) => tag.AsFragment();
        }
        public struct EMBED : IHtmlTag<EMBED>, IHasAttr_src, IHasAttr_type, IHasAttr_width, IHasAttr_height
        {
            public string TagName => "embed";
            string IHtmlTag.TagStart => "<embed";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            EMBED IHtmlTag<EMBED>.WithAttributes(HtmlAttributes replacementAttributes) => new EMBED { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(EMBED tag) => tag.AsFragment();
        }
        public struct FIELDSET : IHtmlTagAllowingContent<FIELDSET>, IHasAttr_disabled, IHasAttr_form, IHasAttr_name
        {
            public string TagName => "fieldset";
            string IHtmlTag.TagStart => "<fieldset";
            string IHtmlTag.EndTag => "</fieldset>";
            HtmlAttributes attrs;
            FIELDSET IHtmlTag<FIELDSET>.WithAttributes(HtmlAttributes replacementAttributes) => new FIELDSET { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            FIELDSET IHtmlTagAllowingContent<FIELDSET>.WithContents(HtmlFragment[] replacementContents) => new FIELDSET { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(FIELDSET tag) => tag.AsFragment();
        }
        public struct FIGCAPTION : IHtmlTagAllowingContent<FIGCAPTION>
        {
            public string TagName => "figcaption";
            string IHtmlTag.TagStart => "<figcaption";
            string IHtmlTag.EndTag => "</figcaption>";
            HtmlAttributes attrs;
            FIGCAPTION IHtmlTag<FIGCAPTION>.WithAttributes(HtmlAttributes replacementAttributes) => new FIGCAPTION { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            FIGCAPTION IHtmlTagAllowingContent<FIGCAPTION>.WithContents(HtmlFragment[] replacementContents) => new FIGCAPTION { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(FIGCAPTION tag) => tag.AsFragment();
        }
        public struct FIGURE : IHtmlTagAllowingContent<FIGURE>
        {
            public string TagName => "figure";
            string IHtmlTag.TagStart => "<figure";
            string IHtmlTag.EndTag => "</figure>";
            HtmlAttributes attrs;
            FIGURE IHtmlTag<FIGURE>.WithAttributes(HtmlAttributes replacementAttributes) => new FIGURE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            FIGURE IHtmlTagAllowingContent<FIGURE>.WithContents(HtmlFragment[] replacementContents) => new FIGURE { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(FIGURE tag) => tag.AsFragment();
        }
        public struct FOOTER : IHtmlTagAllowingContent<FOOTER>
        {
            public string TagName => "footer";
            string IHtmlTag.TagStart => "<footer";
            string IHtmlTag.EndTag => "</footer>";
            HtmlAttributes attrs;
            FOOTER IHtmlTag<FOOTER>.WithAttributes(HtmlAttributes replacementAttributes) => new FOOTER { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            FOOTER IHtmlTagAllowingContent<FOOTER>.WithContents(HtmlFragment[] replacementContents) => new FOOTER { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(FOOTER tag) => tag.AsFragment();
        }
        public struct FORM : IHtmlTagAllowingContent<FORM>, IHasAttr_accept_charset, IHasAttr_action, IHasAttr_autocomplete, IHasAttr_enctype, IHasAttr_method, IHasAttr_name, IHasAttr_novalidate, IHasAttr_target
        {
            public string TagName => "form";
            string IHtmlTag.TagStart => "<form";
            string IHtmlTag.EndTag => "</form>";
            HtmlAttributes attrs;
            FORM IHtmlTag<FORM>.WithAttributes(HtmlAttributes replacementAttributes) => new FORM { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            FORM IHtmlTagAllowingContent<FORM>.WithContents(HtmlFragment[] replacementContents) => new FORM { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(FORM tag) => tag.AsFragment();
        }
        public struct H1 : IHtmlTagAllowingContent<H1>
        {
            public string TagName => "h1";
            string IHtmlTag.TagStart => "<h1";
            string IHtmlTag.EndTag => "</h1>";
            HtmlAttributes attrs;
            H1 IHtmlTag<H1>.WithAttributes(HtmlAttributes replacementAttributes) => new H1 { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            H1 IHtmlTagAllowingContent<H1>.WithContents(HtmlFragment[] replacementContents) => new H1 { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(H1 tag) => tag.AsFragment();
        }
        public struct H2 : IHtmlTagAllowingContent<H2>
        {
            public string TagName => "h2";
            string IHtmlTag.TagStart => "<h2";
            string IHtmlTag.EndTag => "</h2>";
            HtmlAttributes attrs;
            H2 IHtmlTag<H2>.WithAttributes(HtmlAttributes replacementAttributes) => new H2 { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            H2 IHtmlTagAllowingContent<H2>.WithContents(HtmlFragment[] replacementContents) => new H2 { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(H2 tag) => tag.AsFragment();
        }
        public struct H3 : IHtmlTagAllowingContent<H3>
        {
            public string TagName => "h3";
            string IHtmlTag.TagStart => "<h3";
            string IHtmlTag.EndTag => "</h3>";
            HtmlAttributes attrs;
            H3 IHtmlTag<H3>.WithAttributes(HtmlAttributes replacementAttributes) => new H3 { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            H3 IHtmlTagAllowingContent<H3>.WithContents(HtmlFragment[] replacementContents) => new H3 { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(H3 tag) => tag.AsFragment();
        }
        public struct H4 : IHtmlTagAllowingContent<H4>
        {
            public string TagName => "h4";
            string IHtmlTag.TagStart => "<h4";
            string IHtmlTag.EndTag => "</h4>";
            HtmlAttributes attrs;
            H4 IHtmlTag<H4>.WithAttributes(HtmlAttributes replacementAttributes) => new H4 { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            H4 IHtmlTagAllowingContent<H4>.WithContents(HtmlFragment[] replacementContents) => new H4 { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(H4 tag) => tag.AsFragment();
        }
        public struct H5 : IHtmlTagAllowingContent<H5>
        {
            public string TagName => "h5";
            string IHtmlTag.TagStart => "<h5";
            string IHtmlTag.EndTag => "</h5>";
            HtmlAttributes attrs;
            H5 IHtmlTag<H5>.WithAttributes(HtmlAttributes replacementAttributes) => new H5 { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            H5 IHtmlTagAllowingContent<H5>.WithContents(HtmlFragment[] replacementContents) => new H5 { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(H5 tag) => tag.AsFragment();
        }
        public struct H6 : IHtmlTagAllowingContent<H6>
        {
            public string TagName => "h6";
            string IHtmlTag.TagStart => "<h6";
            string IHtmlTag.EndTag => "</h6>";
            HtmlAttributes attrs;
            H6 IHtmlTag<H6>.WithAttributes(HtmlAttributes replacementAttributes) => new H6 { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            H6 IHtmlTagAllowingContent<H6>.WithContents(HtmlFragment[] replacementContents) => new H6 { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(H6 tag) => tag.AsFragment();
        }
        public struct HEAD : IHtmlTagAllowingContent<HEAD>
        {
            public string TagName => "head";
            string IHtmlTag.TagStart => "<head";
            string IHtmlTag.EndTag => "</head>";
            HtmlAttributes attrs;
            HEAD IHtmlTag<HEAD>.WithAttributes(HtmlAttributes replacementAttributes) => new HEAD { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            HEAD IHtmlTagAllowingContent<HEAD>.WithContents(HtmlFragment[] replacementContents) => new HEAD { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(HEAD tag) => tag.AsFragment();
        }
        public struct HEADER : IHtmlTagAllowingContent<HEADER>
        {
            public string TagName => "header";
            string IHtmlTag.TagStart => "<header";
            string IHtmlTag.EndTag => "</header>";
            HtmlAttributes attrs;
            HEADER IHtmlTag<HEADER>.WithAttributes(HtmlAttributes replacementAttributes) => new HEADER { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            HEADER IHtmlTagAllowingContent<HEADER>.WithContents(HtmlFragment[] replacementContents) => new HEADER { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(HEADER tag) => tag.AsFragment();
        }
        public struct HGROUP : IHtmlTagAllowingContent<HGROUP>
        {
            public string TagName => "hgroup";
            string IHtmlTag.TagStart => "<hgroup";
            string IHtmlTag.EndTag => "</hgroup>";
            HtmlAttributes attrs;
            HGROUP IHtmlTag<HGROUP>.WithAttributes(HtmlAttributes replacementAttributes) => new HGROUP { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            HGROUP IHtmlTagAllowingContent<HGROUP>.WithContents(HtmlFragment[] replacementContents) => new HGROUP { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(HGROUP tag) => tag.AsFragment();
        }
        public struct HR : IHtmlTag<HR>
        {
            public string TagName => "hr";
            string IHtmlTag.TagStart => "<hr";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            HR IHtmlTag<HR>.WithAttributes(HtmlAttributes replacementAttributes) => new HR { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(HR tag) => tag.AsFragment();
        }
        public struct HTML : IHtmlTagAllowingContent<HTML>, IHasAttr_manifest
        {
            public string TagName => "html";
            string IHtmlTag.TagStart => "<html";
            string IHtmlTag.EndTag => "</html>";
            HtmlAttributes attrs;
            HTML IHtmlTag<HTML>.WithAttributes(HtmlAttributes replacementAttributes) => new HTML { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            HTML IHtmlTagAllowingContent<HTML>.WithContents(HtmlFragment[] replacementContents) => new HTML { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(HTML tag) => tag.AsFragment();
        }
        public struct I : IHtmlTagAllowingContent<I>
        {
            public string TagName => "i";
            string IHtmlTag.TagStart => "<i";
            string IHtmlTag.EndTag => "</i>";
            HtmlAttributes attrs;
            I IHtmlTag<I>.WithAttributes(HtmlAttributes replacementAttributes) => new I { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            I IHtmlTagAllowingContent<I>.WithContents(HtmlFragment[] replacementContents) => new I { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(I tag) => tag.AsFragment();
        }
        public struct IFRAME : IHtmlTagAllowingContent<IFRAME>, IHasAttr_src, IHasAttr_srcdoc, IHasAttr_name, IHasAttr_sandbox, IHasAttr_allowfullscreen, IHasAttr_allowpaymentrequest, IHasAttr_allowusermedia, IHasAttr_width, IHasAttr_height, IHasAttr_referrerpolicy
        {
            public string TagName => "iframe";
            string IHtmlTag.TagStart => "<iframe";
            string IHtmlTag.EndTag => "</iframe>";
            HtmlAttributes attrs;
            IFRAME IHtmlTag<IFRAME>.WithAttributes(HtmlAttributes replacementAttributes) => new IFRAME { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            IFRAME IHtmlTagAllowingContent<IFRAME>.WithContents(HtmlFragment[] replacementContents) => new IFRAME { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(IFRAME tag) => tag.AsFragment();
        }
        public struct IMG : IHtmlTag<IMG>, IHasAttr_alt, IHasAttr_src, IHasAttr_srcset, IHasAttr_crossorigin, IHasAttr_usemap, IHasAttr_ismap, IHasAttr_width, IHasAttr_height, IHasAttr_referrerpolicy
        {
            public string TagName => "img";
            string IHtmlTag.TagStart => "<img";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            IMG IHtmlTag<IMG>.WithAttributes(HtmlAttributes replacementAttributes) => new IMG { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(IMG tag) => tag.AsFragment();
        }
        public struct INPUT : IHtmlTag<INPUT>, IHasAttr_accept, IHasAttr_alt, IHasAttr_autocomplete, IHasAttr_autofocus, IHasAttr_checked, IHasAttr_dirname, IHasAttr_disabled, IHasAttr_form, IHasAttr_formaction, IHasAttr_formenctype, IHasAttr_formmethod, IHasAttr_formnovalidate, IHasAttr_formtarget, IHasAttr_height, IHasAttr_inputmode, IHasAttr_list, IHasAttr_max, IHasAttr_maxlength, IHasAttr_min, IHasAttr_minlength, IHasAttr_multiple, IHasAttr_name, IHasAttr_pattern, IHasAttr_placeholder, IHasAttr_readonly, IHasAttr_required, IHasAttr_size, IHasAttr_src, IHasAttr_step, IHasAttr_type, IHasAttr_value, IHasAttr_width
        {
            public string TagName => "input";
            string IHtmlTag.TagStart => "<input";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            INPUT IHtmlTag<INPUT>.WithAttributes(HtmlAttributes replacementAttributes) => new INPUT { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(INPUT tag) => tag.AsFragment();
        }
        public struct INS : IHtmlTagAllowingContent<INS>, IHasAttr_cite, IHasAttr_datetime
        {
            public string TagName => "ins";
            string IHtmlTag.TagStart => "<ins";
            string IHtmlTag.EndTag => "</ins>";
            HtmlAttributes attrs;
            INS IHtmlTag<INS>.WithAttributes(HtmlAttributes replacementAttributes) => new INS { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            INS IHtmlTagAllowingContent<INS>.WithContents(HtmlFragment[] replacementContents) => new INS { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(INS tag) => tag.AsFragment();
        }
        public struct KBD : IHtmlTagAllowingContent<KBD>
        {
            public string TagName => "kbd";
            string IHtmlTag.TagStart => "<kbd";
            string IHtmlTag.EndTag => "</kbd>";
            HtmlAttributes attrs;
            KBD IHtmlTag<KBD>.WithAttributes(HtmlAttributes replacementAttributes) => new KBD { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            KBD IHtmlTagAllowingContent<KBD>.WithContents(HtmlFragment[] replacementContents) => new KBD { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(KBD tag) => tag.AsFragment();
        }
        public struct LABEL : IHtmlTagAllowingContent<LABEL>, IHasAttr_for
        {
            public string TagName => "label";
            string IHtmlTag.TagStart => "<label";
            string IHtmlTag.EndTag => "</label>";
            HtmlAttributes attrs;
            LABEL IHtmlTag<LABEL>.WithAttributes(HtmlAttributes replacementAttributes) => new LABEL { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            LABEL IHtmlTagAllowingContent<LABEL>.WithContents(HtmlFragment[] replacementContents) => new LABEL { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(LABEL tag) => tag.AsFragment();
        }
        public struct LEGEND : IHtmlTagAllowingContent<LEGEND>
        {
            public string TagName => "legend";
            string IHtmlTag.TagStart => "<legend";
            string IHtmlTag.EndTag => "</legend>";
            HtmlAttributes attrs;
            LEGEND IHtmlTag<LEGEND>.WithAttributes(HtmlAttributes replacementAttributes) => new LEGEND { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            LEGEND IHtmlTagAllowingContent<LEGEND>.WithContents(HtmlFragment[] replacementContents) => new LEGEND { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(LEGEND tag) => tag.AsFragment();
        }
        public struct LI : IHtmlTagAllowingContent<LI>, IHasAttr_value
        {
            public string TagName => "li";
            string IHtmlTag.TagStart => "<li";
            string IHtmlTag.EndTag => "</li>";
            HtmlAttributes attrs;
            LI IHtmlTag<LI>.WithAttributes(HtmlAttributes replacementAttributes) => new LI { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            LI IHtmlTagAllowingContent<LI>.WithContents(HtmlFragment[] replacementContents) => new LI { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(LI tag) => tag.AsFragment();
        }
        public struct LINK : IHtmlTag<LINK>, IHasAttr_href, IHasAttr_crossorigin, IHasAttr_rel, IHasAttr_as, IHasAttr_media, IHasAttr_hreflang, IHasAttr_type, IHasAttr_sizes, IHasAttr_referrerpolicy, IHasAttr_nonce, IHasAttr_integrity
        {
            public string TagName => "link";
            string IHtmlTag.TagStart => "<link";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            LINK IHtmlTag<LINK>.WithAttributes(HtmlAttributes replacementAttributes) => new LINK { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(LINK tag) => tag.AsFragment();
        }
        public struct MAIN : IHtmlTagAllowingContent<MAIN>
        {
            public string TagName => "main";
            string IHtmlTag.TagStart => "<main";
            string IHtmlTag.EndTag => "</main>";
            HtmlAttributes attrs;
            MAIN IHtmlTag<MAIN>.WithAttributes(HtmlAttributes replacementAttributes) => new MAIN { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            MAIN IHtmlTagAllowingContent<MAIN>.WithContents(HtmlFragment[] replacementContents) => new MAIN { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(MAIN tag) => tag.AsFragment();
        }
        public struct MAP : IHtmlTagAllowingContent<MAP>, IHasAttr_name
        {
            public string TagName => "map";
            string IHtmlTag.TagStart => "<map";
            string IHtmlTag.EndTag => "</map>";
            HtmlAttributes attrs;
            MAP IHtmlTag<MAP>.WithAttributes(HtmlAttributes replacementAttributes) => new MAP { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            MAP IHtmlTagAllowingContent<MAP>.WithContents(HtmlFragment[] replacementContents) => new MAP { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(MAP tag) => tag.AsFragment();
        }
        public struct MARK : IHtmlTagAllowingContent<MARK>
        {
            public string TagName => "mark";
            string IHtmlTag.TagStart => "<mark";
            string IHtmlTag.EndTag => "</mark>";
            HtmlAttributes attrs;
            MARK IHtmlTag<MARK>.WithAttributes(HtmlAttributes replacementAttributes) => new MARK { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            MARK IHtmlTagAllowingContent<MARK>.WithContents(HtmlFragment[] replacementContents) => new MARK { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(MARK tag) => tag.AsFragment();
        }
        public struct MENU : IHtmlTagAllowingContent<MENU>, IHasAttr_type, IHasAttr_label
        {
            public string TagName => "menu";
            string IHtmlTag.TagStart => "<menu";
            string IHtmlTag.EndTag => "</menu>";
            HtmlAttributes attrs;
            MENU IHtmlTag<MENU>.WithAttributes(HtmlAttributes replacementAttributes) => new MENU { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            MENU IHtmlTagAllowingContent<MENU>.WithContents(HtmlFragment[] replacementContents) => new MENU { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(MENU tag) => tag.AsFragment();
        }
        public struct MENUITEM : IHtmlTagAllowingContent<MENUITEM>, IHasAttr_type, IHasAttr_label, IHasAttr_icon, IHasAttr_disabled, IHasAttr_checked, IHasAttr_radiogroup, IHasAttr_default
        {
            public string TagName => "menuitem";
            string IHtmlTag.TagStart => "<menuitem";
            string IHtmlTag.EndTag => "</menuitem>";
            HtmlAttributes attrs;
            MENUITEM IHtmlTag<MENUITEM>.WithAttributes(HtmlAttributes replacementAttributes) => new MENUITEM { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            MENUITEM IHtmlTagAllowingContent<MENUITEM>.WithContents(HtmlFragment[] replacementContents) => new MENUITEM { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(MENUITEM tag) => tag.AsFragment();
        }
        public struct META : IHtmlTag<META>, IHasAttr_name, IHasAttr_http_equiv, IHasAttr_content, IHasAttr_charset
        {
            public string TagName => "meta";
            string IHtmlTag.TagStart => "<meta";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            META IHtmlTag<META>.WithAttributes(HtmlAttributes replacementAttributes) => new META { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(META tag) => tag.AsFragment();
        }
        public struct METER : IHtmlTagAllowingContent<METER>, IHasAttr_value, IHasAttr_min, IHasAttr_max, IHasAttr_low, IHasAttr_high, IHasAttr_optimum
        {
            public string TagName => "meter";
            string IHtmlTag.TagStart => "<meter";
            string IHtmlTag.EndTag => "</meter>";
            HtmlAttributes attrs;
            METER IHtmlTag<METER>.WithAttributes(HtmlAttributes replacementAttributes) => new METER { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            METER IHtmlTagAllowingContent<METER>.WithContents(HtmlFragment[] replacementContents) => new METER { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(METER tag) => tag.AsFragment();
        }
        public struct NAV : IHtmlTagAllowingContent<NAV>
        {
            public string TagName => "nav";
            string IHtmlTag.TagStart => "<nav";
            string IHtmlTag.EndTag => "</nav>";
            HtmlAttributes attrs;
            NAV IHtmlTag<NAV>.WithAttributes(HtmlAttributes replacementAttributes) => new NAV { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            NAV IHtmlTagAllowingContent<NAV>.WithContents(HtmlFragment[] replacementContents) => new NAV { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(NAV tag) => tag.AsFragment();
        }
        public struct NOSCRIPT : IHtmlTagAllowingContent<NOSCRIPT>
        {
            public string TagName => "noscript";
            string IHtmlTag.TagStart => "<noscript";
            string IHtmlTag.EndTag => "</noscript>";
            HtmlAttributes attrs;
            NOSCRIPT IHtmlTag<NOSCRIPT>.WithAttributes(HtmlAttributes replacementAttributes) => new NOSCRIPT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            NOSCRIPT IHtmlTagAllowingContent<NOSCRIPT>.WithContents(HtmlFragment[] replacementContents) => new NOSCRIPT { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(NOSCRIPT tag) => tag.AsFragment();
        }
        public struct OBJECT : IHtmlTagAllowingContent<OBJECT>, IHasAttr_data, IHasAttr_type, IHasAttr_typemustmatch, IHasAttr_name, IHasAttr_usemap, IHasAttr_form, IHasAttr_width, IHasAttr_height
        {
            public string TagName => "object";
            string IHtmlTag.TagStart => "<object";
            string IHtmlTag.EndTag => "</object>";
            HtmlAttributes attrs;
            OBJECT IHtmlTag<OBJECT>.WithAttributes(HtmlAttributes replacementAttributes) => new OBJECT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            OBJECT IHtmlTagAllowingContent<OBJECT>.WithContents(HtmlFragment[] replacementContents) => new OBJECT { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(OBJECT tag) => tag.AsFragment();
        }
        public struct OL : IHtmlTagAllowingContent<OL>, IHasAttr_reversed, IHasAttr_start, IHasAttr_type
        {
            public string TagName => "ol";
            string IHtmlTag.TagStart => "<ol";
            string IHtmlTag.EndTag => "</ol>";
            HtmlAttributes attrs;
            OL IHtmlTag<OL>.WithAttributes(HtmlAttributes replacementAttributes) => new OL { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            OL IHtmlTagAllowingContent<OL>.WithContents(HtmlFragment[] replacementContents) => new OL { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(OL tag) => tag.AsFragment();
        }
        public struct OPTGROUP : IHtmlTagAllowingContent<OPTGROUP>, IHasAttr_disabled, IHasAttr_label
        {
            public string TagName => "optgroup";
            string IHtmlTag.TagStart => "<optgroup";
            string IHtmlTag.EndTag => "</optgroup>";
            HtmlAttributes attrs;
            OPTGROUP IHtmlTag<OPTGROUP>.WithAttributes(HtmlAttributes replacementAttributes) => new OPTGROUP { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            OPTGROUP IHtmlTagAllowingContent<OPTGROUP>.WithContents(HtmlFragment[] replacementContents) => new OPTGROUP { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(OPTGROUP tag) => tag.AsFragment();
        }
        public struct OPTION : IHtmlTagAllowingContent<OPTION>, IHasAttr_disabled, IHasAttr_label, IHasAttr_selected, IHasAttr_value
        {
            public string TagName => "option";
            string IHtmlTag.TagStart => "<option";
            string IHtmlTag.EndTag => "</option>";
            HtmlAttributes attrs;
            OPTION IHtmlTag<OPTION>.WithAttributes(HtmlAttributes replacementAttributes) => new OPTION { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            OPTION IHtmlTagAllowingContent<OPTION>.WithContents(HtmlFragment[] replacementContents) => new OPTION { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(OPTION tag) => tag.AsFragment();
        }
        public struct OUTPUT : IHtmlTagAllowingContent<OUTPUT>, IHasAttr_for, IHasAttr_form, IHasAttr_name
        {
            public string TagName => "output";
            string IHtmlTag.TagStart => "<output";
            string IHtmlTag.EndTag => "</output>";
            HtmlAttributes attrs;
            OUTPUT IHtmlTag<OUTPUT>.WithAttributes(HtmlAttributes replacementAttributes) => new OUTPUT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            OUTPUT IHtmlTagAllowingContent<OUTPUT>.WithContents(HtmlFragment[] replacementContents) => new OUTPUT { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(OUTPUT tag) => tag.AsFragment();
        }
        public struct P : IHtmlTagAllowingContent<P>
        {
            public string TagName => "p";
            string IHtmlTag.TagStart => "<p";
            string IHtmlTag.EndTag => "</p>";
            HtmlAttributes attrs;
            P IHtmlTag<P>.WithAttributes(HtmlAttributes replacementAttributes) => new P { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            P IHtmlTagAllowingContent<P>.WithContents(HtmlFragment[] replacementContents) => new P { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(P tag) => tag.AsFragment();
        }
        public struct PARAM : IHtmlTag<PARAM>, IHasAttr_name, IHasAttr_value
        {
            public string TagName => "param";
            string IHtmlTag.TagStart => "<param";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            PARAM IHtmlTag<PARAM>.WithAttributes(HtmlAttributes replacementAttributes) => new PARAM { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(PARAM tag) => tag.AsFragment();
        }
        public struct PICTURE : IHtmlTagAllowingContent<PICTURE>
        {
            public string TagName => "picture";
            string IHtmlTag.TagStart => "<picture";
            string IHtmlTag.EndTag => "</picture>";
            HtmlAttributes attrs;
            PICTURE IHtmlTag<PICTURE>.WithAttributes(HtmlAttributes replacementAttributes) => new PICTURE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            PICTURE IHtmlTagAllowingContent<PICTURE>.WithContents(HtmlFragment[] replacementContents) => new PICTURE { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(PICTURE tag) => tag.AsFragment();
        }
        public struct PRE : IHtmlTagAllowingContent<PRE>
        {
            public string TagName => "pre";
            string IHtmlTag.TagStart => "<pre";
            string IHtmlTag.EndTag => "</pre>";
            HtmlAttributes attrs;
            PRE IHtmlTag<PRE>.WithAttributes(HtmlAttributes replacementAttributes) => new PRE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            PRE IHtmlTagAllowingContent<PRE>.WithContents(HtmlFragment[] replacementContents) => new PRE { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(PRE tag) => tag.AsFragment();
        }
        public struct PROGRESS : IHtmlTagAllowingContent<PROGRESS>, IHasAttr_value, IHasAttr_max
        {
            public string TagName => "progress";
            string IHtmlTag.TagStart => "<progress";
            string IHtmlTag.EndTag => "</progress>";
            HtmlAttributes attrs;
            PROGRESS IHtmlTag<PROGRESS>.WithAttributes(HtmlAttributes replacementAttributes) => new PROGRESS { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            PROGRESS IHtmlTagAllowingContent<PROGRESS>.WithContents(HtmlFragment[] replacementContents) => new PROGRESS { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(PROGRESS tag) => tag.AsFragment();
        }
        public struct Q : IHtmlTagAllowingContent<Q>, IHasAttr_cite
        {
            public string TagName => "q";
            string IHtmlTag.TagStart => "<q";
            string IHtmlTag.EndTag => "</q>";
            HtmlAttributes attrs;
            Q IHtmlTag<Q>.WithAttributes(HtmlAttributes replacementAttributes) => new Q { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            Q IHtmlTagAllowingContent<Q>.WithContents(HtmlFragment[] replacementContents) => new Q { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(Q tag) => tag.AsFragment();
        }
        public struct RP : IHtmlTagAllowingContent<RP>
        {
            public string TagName => "rp";
            string IHtmlTag.TagStart => "<rp";
            string IHtmlTag.EndTag => "</rp>";
            HtmlAttributes attrs;
            RP IHtmlTag<RP>.WithAttributes(HtmlAttributes replacementAttributes) => new RP { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            RP IHtmlTagAllowingContent<RP>.WithContents(HtmlFragment[] replacementContents) => new RP { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(RP tag) => tag.AsFragment();
        }
        public struct RT : IHtmlTagAllowingContent<RT>
        {
            public string TagName => "rt";
            string IHtmlTag.TagStart => "<rt";
            string IHtmlTag.EndTag => "</rt>";
            HtmlAttributes attrs;
            RT IHtmlTag<RT>.WithAttributes(HtmlAttributes replacementAttributes) => new RT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            RT IHtmlTagAllowingContent<RT>.WithContents(HtmlFragment[] replacementContents) => new RT { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(RT tag) => tag.AsFragment();
        }
        public struct RUBY : IHtmlTagAllowingContent<RUBY>
        {
            public string TagName => "ruby";
            string IHtmlTag.TagStart => "<ruby";
            string IHtmlTag.EndTag => "</ruby>";
            HtmlAttributes attrs;
            RUBY IHtmlTag<RUBY>.WithAttributes(HtmlAttributes replacementAttributes) => new RUBY { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            RUBY IHtmlTagAllowingContent<RUBY>.WithContents(HtmlFragment[] replacementContents) => new RUBY { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(RUBY tag) => tag.AsFragment();
        }
        public struct S : IHtmlTagAllowingContent<S>
        {
            public string TagName => "s";
            string IHtmlTag.TagStart => "<s";
            string IHtmlTag.EndTag => "</s>";
            HtmlAttributes attrs;
            S IHtmlTag<S>.WithAttributes(HtmlAttributes replacementAttributes) => new S { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            S IHtmlTagAllowingContent<S>.WithContents(HtmlFragment[] replacementContents) => new S { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(S tag) => tag.AsFragment();
        }
        public struct SAMP : IHtmlTagAllowingContent<SAMP>
        {
            public string TagName => "samp";
            string IHtmlTag.TagStart => "<samp";
            string IHtmlTag.EndTag => "</samp>";
            HtmlAttributes attrs;
            SAMP IHtmlTag<SAMP>.WithAttributes(HtmlAttributes replacementAttributes) => new SAMP { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            SAMP IHtmlTagAllowingContent<SAMP>.WithContents(HtmlFragment[] replacementContents) => new SAMP { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(SAMP tag) => tag.AsFragment();
        }
        public struct SCRIPT : IHtmlTagAllowingContent<SCRIPT>, IHasAttr_src, IHasAttr_type, IHasAttr_charset, IHasAttr_async, IHasAttr_defer, IHasAttr_crossorigin, IHasAttr_nonce, IHasAttr_integrity
        {
            public string TagName => "script";
            string IHtmlTag.TagStart => "<script";
            string IHtmlTag.EndTag => "</script>";
            HtmlAttributes attrs;
            SCRIPT IHtmlTag<SCRIPT>.WithAttributes(HtmlAttributes replacementAttributes) => new SCRIPT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            SCRIPT IHtmlTagAllowingContent<SCRIPT>.WithContents(HtmlFragment[] replacementContents) => new SCRIPT { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(SCRIPT tag) => tag.AsFragment();
        }
        public struct SECTION : IHtmlTagAllowingContent<SECTION>
        {
            public string TagName => "section";
            string IHtmlTag.TagStart => "<section";
            string IHtmlTag.EndTag => "</section>";
            HtmlAttributes attrs;
            SECTION IHtmlTag<SECTION>.WithAttributes(HtmlAttributes replacementAttributes) => new SECTION { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            SECTION IHtmlTagAllowingContent<SECTION>.WithContents(HtmlFragment[] replacementContents) => new SECTION { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(SECTION tag) => tag.AsFragment();
        }
        public struct SELECT : IHtmlTagAllowingContent<SELECT>, IHasAttr_autocomplete, IHasAttr_autofocus, IHasAttr_disabled, IHasAttr_form, IHasAttr_multiple, IHasAttr_name, IHasAttr_required, IHasAttr_size
        {
            public string TagName => "select";
            string IHtmlTag.TagStart => "<select";
            string IHtmlTag.EndTag => "</select>";
            HtmlAttributes attrs;
            SELECT IHtmlTag<SELECT>.WithAttributes(HtmlAttributes replacementAttributes) => new SELECT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            SELECT IHtmlTagAllowingContent<SELECT>.WithContents(HtmlFragment[] replacementContents) => new SELECT { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(SELECT tag) => tag.AsFragment();
        }
        public struct SLOT : IHtmlTagAllowingContent<SLOT>, IHasAttr_name
        {
            public string TagName => "slot";
            string IHtmlTag.TagStart => "<slot";
            string IHtmlTag.EndTag => "</slot>";
            HtmlAttributes attrs;
            SLOT IHtmlTag<SLOT>.WithAttributes(HtmlAttributes replacementAttributes) => new SLOT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            SLOT IHtmlTagAllowingContent<SLOT>.WithContents(HtmlFragment[] replacementContents) => new SLOT { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(SLOT tag) => tag.AsFragment();
        }
        public struct SMALL : IHtmlTagAllowingContent<SMALL>
        {
            public string TagName => "small";
            string IHtmlTag.TagStart => "<small";
            string IHtmlTag.EndTag => "</small>";
            HtmlAttributes attrs;
            SMALL IHtmlTag<SMALL>.WithAttributes(HtmlAttributes replacementAttributes) => new SMALL { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            SMALL IHtmlTagAllowingContent<SMALL>.WithContents(HtmlFragment[] replacementContents) => new SMALL { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(SMALL tag) => tag.AsFragment();
        }
        public struct SOURCE : IHtmlTag<SOURCE>, IHasAttr_src, IHasAttr_sizes, IHasAttr_media
        {
            public string TagName => "source";
            string IHtmlTag.TagStart => "<source";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            SOURCE IHtmlTag<SOURCE>.WithAttributes(HtmlAttributes replacementAttributes) => new SOURCE { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(SOURCE tag) => tag.AsFragment();
        }
        public struct SPAN : IHtmlTagAllowingContent<SPAN>
        {
            public string TagName => "span";
            string IHtmlTag.TagStart => "<span";
            string IHtmlTag.EndTag => "</span>";
            HtmlAttributes attrs;
            SPAN IHtmlTag<SPAN>.WithAttributes(HtmlAttributes replacementAttributes) => new SPAN { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            SPAN IHtmlTagAllowingContent<SPAN>.WithContents(HtmlFragment[] replacementContents) => new SPAN { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(SPAN tag) => tag.AsFragment();
        }
        public struct STRONG : IHtmlTagAllowingContent<STRONG>
        {
            public string TagName => "strong";
            string IHtmlTag.TagStart => "<strong";
            string IHtmlTag.EndTag => "</strong>";
            HtmlAttributes attrs;
            STRONG IHtmlTag<STRONG>.WithAttributes(HtmlAttributes replacementAttributes) => new STRONG { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            STRONG IHtmlTagAllowingContent<STRONG>.WithContents(HtmlFragment[] replacementContents) => new STRONG { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(STRONG tag) => tag.AsFragment();
        }
        public struct STYLE : IHtmlTagAllowingContent<STYLE>, IHasAttr_media, IHasAttr_nonce, IHasAttr_type
        {
            public string TagName => "style";
            string IHtmlTag.TagStart => "<style";
            string IHtmlTag.EndTag => "</style>";
            HtmlAttributes attrs;
            STYLE IHtmlTag<STYLE>.WithAttributes(HtmlAttributes replacementAttributes) => new STYLE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            STYLE IHtmlTagAllowingContent<STYLE>.WithContents(HtmlFragment[] replacementContents) => new STYLE { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(STYLE tag) => tag.AsFragment();
        }
        public struct SUB : IHtmlTagAllowingContent<SUB>
        {
            public string TagName => "sub";
            string IHtmlTag.TagStart => "<sub";
            string IHtmlTag.EndTag => "</sub>";
            HtmlAttributes attrs;
            SUB IHtmlTag<SUB>.WithAttributes(HtmlAttributes replacementAttributes) => new SUB { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            SUB IHtmlTagAllowingContent<SUB>.WithContents(HtmlFragment[] replacementContents) => new SUB { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(SUB tag) => tag.AsFragment();
        }
        public struct SUMMARY : IHtmlTagAllowingContent<SUMMARY>
        {
            public string TagName => "summary";
            string IHtmlTag.TagStart => "<summary";
            string IHtmlTag.EndTag => "</summary>";
            HtmlAttributes attrs;
            SUMMARY IHtmlTag<SUMMARY>.WithAttributes(HtmlAttributes replacementAttributes) => new SUMMARY { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            SUMMARY IHtmlTagAllowingContent<SUMMARY>.WithContents(HtmlFragment[] replacementContents) => new SUMMARY { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(SUMMARY tag) => tag.AsFragment();
        }
        public struct SUP : IHtmlTagAllowingContent<SUP>
        {
            public string TagName => "sup";
            string IHtmlTag.TagStart => "<sup";
            string IHtmlTag.EndTag => "</sup>";

            HtmlAttributes attrs;
            SUP IHtmlTag<SUP>.WithAttributes(HtmlAttributes replacementAttributes) => new SUP { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            SUP IHtmlTagAllowingContent<SUP>.WithContents(HtmlFragment[] replacementContents) => new SUP { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(SUP tag) => tag.AsFragment();
        }
        public struct TABLE : IHtmlTagAllowingContent<TABLE>
        {
            public string TagName => "table";
            string IHtmlTag.TagStart => "<table";
            string IHtmlTag.EndTag => "</table>";
            HtmlAttributes attrs;
            TABLE IHtmlTag<TABLE>.WithAttributes(HtmlAttributes replacementAttributes) => new TABLE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            TABLE IHtmlTagAllowingContent<TABLE>.WithContents(HtmlFragment[] replacementContents) => new TABLE { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(TABLE tag) => tag.AsFragment();
        }
        public struct TBODY : IHtmlTagAllowingContent<TBODY>
        {
            public string TagName => "tbody";
            string IHtmlTag.TagStart => "<tbody";
            string IHtmlTag.EndTag => "</tbody>";
            HtmlAttributes attrs;
            TBODY IHtmlTag<TBODY>.WithAttributes(HtmlAttributes replacementAttributes) => new TBODY { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            TBODY IHtmlTagAllowingContent<TBODY>.WithContents(HtmlFragment[] replacementContents) => new TBODY { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(TBODY tag) => tag.AsFragment();
        }
        public struct TD : IHtmlTagAllowingContent<TD>, IHasAttr_colspan, IHasAttr_rowspan, IHasAttr_headers
        {
            public string TagName => "td";
            string IHtmlTag.TagStart => "<td";
            string IHtmlTag.EndTag => "</td>";
            HtmlAttributes attrs;
            TD IHtmlTag<TD>.WithAttributes(HtmlAttributes replacementAttributes) => new TD { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            TD IHtmlTagAllowingContent<TD>.WithContents(HtmlFragment[] replacementContents) => new TD { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(TD tag) => tag.AsFragment();
        }
        public struct TEMPLATE : IHtmlTag<TEMPLATE>
        {
            public string TagName => "template";
            string IHtmlTag.TagStart => "<template";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            TEMPLATE IHtmlTag<TEMPLATE>.WithAttributes(HtmlAttributes replacementAttributes) => new TEMPLATE { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(TEMPLATE tag) => tag.AsFragment();
        }
        public struct TEXTAREA : IHtmlTagAllowingContent<TEXTAREA>, IHasAttr_autofocus, IHasAttr_cols, IHasAttr_dirname, IHasAttr_disabled, IHasAttr_form, IHasAttr_inputmode, IHasAttr_maxlength, IHasAttr_minlength, IHasAttr_name, IHasAttr_placeholder, IHasAttr_readonly, IHasAttr_required, IHasAttr_rows, IHasAttr_wrap
        {
            public string TagName => "textarea";
            string IHtmlTag.TagStart => "<textarea";
            string IHtmlTag.EndTag => "</textarea>";
            HtmlAttributes attrs;
            TEXTAREA IHtmlTag<TEXTAREA>.WithAttributes(HtmlAttributes replacementAttributes) => new TEXTAREA { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            TEXTAREA IHtmlTagAllowingContent<TEXTAREA>.WithContents(HtmlFragment[] replacementContents) => new TEXTAREA { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(TEXTAREA tag) => tag.AsFragment();
        }
        public struct TFOOT : IHtmlTagAllowingContent<TFOOT>
        {
            public string TagName => "tfoot";
            string IHtmlTag.TagStart => "<tfoot";
            string IHtmlTag.EndTag => "</tfoot>";
            HtmlAttributes attrs;
            TFOOT IHtmlTag<TFOOT>.WithAttributes(HtmlAttributes replacementAttributes) => new TFOOT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            TFOOT IHtmlTagAllowingContent<TFOOT>.WithContents(HtmlFragment[] replacementContents) => new TFOOT { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(TFOOT tag) => tag.AsFragment();
        }
        public struct TH : IHtmlTagAllowingContent<TH>, IHasAttr_colspan, IHasAttr_rowspan, IHasAttr_headers, IHasAttr_scope, IHasAttr_abbr
        {
            public string TagName => "th";
            string IHtmlTag.TagStart => "<th";
            string IHtmlTag.EndTag => "</th>";
            HtmlAttributes attrs;
            TH IHtmlTag<TH>.WithAttributes(HtmlAttributes replacementAttributes) => new TH { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            TH IHtmlTagAllowingContent<TH>.WithContents(HtmlFragment[] replacementContents) => new TH { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(TH tag) => tag.AsFragment();
        }
        public struct THEAD : IHtmlTagAllowingContent<THEAD>
        {
            public string TagName => "thead";
            string IHtmlTag.TagStart => "<thead";
            string IHtmlTag.EndTag => "</thead>";
            HtmlAttributes attrs;
            THEAD IHtmlTag<THEAD>.WithAttributes(HtmlAttributes replacementAttributes) => new THEAD { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            THEAD IHtmlTagAllowingContent<THEAD>.WithContents(HtmlFragment[] replacementContents) => new THEAD { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(THEAD tag) => tag.AsFragment();
        }
        public struct TIME : IHtmlTagAllowingContent<TIME>, IHasAttr_datetime
        {
            public string TagName => "time";
            string IHtmlTag.TagStart => "<time";
            string IHtmlTag.EndTag => "</time>";
            HtmlAttributes attrs;
            TIME IHtmlTag<TIME>.WithAttributes(HtmlAttributes replacementAttributes) => new TIME { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            TIME IHtmlTagAllowingContent<TIME>.WithContents(HtmlFragment[] replacementContents) => new TIME { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(TIME tag) => tag.AsFragment();
        }
        public struct TITLE : IHtmlTagAllowingContent<TITLE>
        {
            public string TagName => "title";
            string IHtmlTag.TagStart => "<title";
            string IHtmlTag.EndTag => "</title>";
            HtmlAttributes attrs;
            TITLE IHtmlTag<TITLE>.WithAttributes(HtmlAttributes replacementAttributes) => new TITLE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            TITLE IHtmlTagAllowingContent<TITLE>.WithContents(HtmlFragment[] replacementContents) => new TITLE { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(TITLE tag) => tag.AsFragment();
        }
        public struct TR : IHtmlTagAllowingContent<TR>
        {
            public string TagName => "tr";
            string IHtmlTag.TagStart => "<tr";
            string IHtmlTag.EndTag => "</tr>";
            HtmlAttributes attrs;
            TR IHtmlTag<TR>.WithAttributes(HtmlAttributes replacementAttributes) => new TR { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            TR IHtmlTagAllowingContent<TR>.WithContents(HtmlFragment[] replacementContents) => new TR { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(TR tag) => tag.AsFragment();
        }
        public struct TRACK : IHtmlTag<TRACK>, IHasAttr_default, IHasAttr_kind, IHasAttr_label, IHasAttr_src, IHasAttr_srclang
        {
            public string TagName => "track";
            string IHtmlTag.TagStart => "<track";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            TRACK IHtmlTag<TRACK>.WithAttributes(HtmlAttributes replacementAttributes) => new TRACK { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(TRACK tag) => tag.AsFragment();
        }
        public struct U : IHtmlTagAllowingContent<U>
        {
            public string TagName => "u";
            string IHtmlTag.TagStart => "<u";
            string IHtmlTag.EndTag => "</u>";
            HtmlAttributes attrs;
            U IHtmlTag<U>.WithAttributes(HtmlAttributes replacementAttributes) => new U { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            U IHtmlTagAllowingContent<U>.WithContents(HtmlFragment[] replacementContents) => new U { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(U tag) => tag.AsFragment();
        }
        public struct UL : IHtmlTagAllowingContent<UL>
        {
            public string TagName => "ul";
            string IHtmlTag.TagStart => "<ul";
            string IHtmlTag.EndTag => "</ul>";
            HtmlAttributes attrs;
            UL IHtmlTag<UL>.WithAttributes(HtmlAttributes replacementAttributes) => new UL { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            UL IHtmlTagAllowingContent<UL>.WithContents(HtmlFragment[] replacementContents) => new UL { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(UL tag) => tag.AsFragment();
        }
        public struct VAR : IHtmlTagAllowingContent<VAR>
        {
            public string TagName => "var";
            string IHtmlTag.TagStart => "<var";
            string IHtmlTag.EndTag => "</var>";
            HtmlAttributes attrs;
            VAR IHtmlTag<VAR>.WithAttributes(HtmlAttributes replacementAttributes) => new VAR { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            VAR IHtmlTagAllowingContent<VAR>.WithContents(HtmlFragment[] replacementContents) => new VAR { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(VAR tag) => tag.AsFragment();
        }
        public struct VIDEO : IHtmlTagAllowingContent<VIDEO>, IHasAttr_src, IHasAttr_crossorigin, IHasAttr_poster, IHasAttr_preload, IHasAttr_autoplay, IHasAttr_playsinline, IHasAttr_loop, IHasAttr_muted, IHasAttr_controls, IHasAttr_width, IHasAttr_height
        {
            public string TagName => "video";
            string IHtmlTag.TagStart => "<video";
            string IHtmlTag.EndTag => "</video>";
            HtmlAttributes attrs;
            VIDEO IHtmlTag<VIDEO>.WithAttributes(HtmlAttributes replacementAttributes) => new VIDEO { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            VIDEO IHtmlTagAllowingContent<VIDEO>.WithContents(HtmlFragment[] replacementContents) => new VIDEO { attrs = attrs, children = replacementContents };
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(VIDEO tag) => tag.AsFragment();
        }
        public struct WBR : IHtmlTag<WBR>
        {
            public string TagName => "wbr";
            string IHtmlTag.TagStart => "<wbr";
            string IHtmlTag.EndTag => "";
            HtmlAttributes attrs;
            WBR IHtmlTag<WBR>.WithAttributes(HtmlAttributes replacementAttributes) => new WBR { attrs = replacementAttributes };
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment(WBR tag) => tag.AsFragment();
        }
    }

    public static class Tags
    {

        ///<summary>Hyperlink. See: <a href="https://html.spec.whatwg.org/#the-a-element">https://html.spec.whatwg.org/#the-a-element</a><br /></summary>
        public static readonly HtmlTagKinds.A _a = new HtmlTagKinds.A();

        ///<summary>Abbreviation. See: <a href="https://html.spec.whatwg.org/#the-abbr-element">https://html.spec.whatwg.org/#the-abbr-element</a><br /></summary>
        public static readonly HtmlTagKinds.ABBR _abbr = new HtmlTagKinds.ABBR();

        ///<summary>Contact information for a page or article element. See: <a href="https://html.spec.whatwg.org/#the-address-element">https://html.spec.whatwg.org/#the-address-element</a><br /></summary>
        public static readonly HtmlTagKinds.ADDRESS _address = new HtmlTagKinds.ADDRESS();

        ///<summary>Hyperlink or dead area on an image map. See: <a href="https://html.spec.whatwg.org/#the-area-element">https://html.spec.whatwg.org/#the-area-element</a><br /></summary>
        public static readonly HtmlTagKinds.AREA _area = new HtmlTagKinds.AREA();

        ///<summary>Self-contained syndicatable or reusable composition. See: <a href="https://html.spec.whatwg.org/#the-article-element">https://html.spec.whatwg.org/#the-article-element</a><br /></summary>
        public static readonly HtmlTagKinds.ARTICLE _article = new HtmlTagKinds.ARTICLE();

        ///<summary>Sidebar for tangentially related content. See: <a href="https://html.spec.whatwg.org/#the-aside-element">https://html.spec.whatwg.org/#the-aside-element</a><br /></summary>
        public static readonly HtmlTagKinds.ASIDE _aside = new HtmlTagKinds.ASIDE();

        ///<summary>Audio player. See: <a href="https://html.spec.whatwg.org/#the-audio-element">https://html.spec.whatwg.org/#the-audio-element</a><br /></summary>
        public static readonly HtmlTagKinds.AUDIO _audio = new HtmlTagKinds.AUDIO();

        ///<summary>Keywords. See: <a href="https://html.spec.whatwg.org/#the-b-element">https://html.spec.whatwg.org/#the-b-element</a><br /></summary>
        public static readonly HtmlTagKinds.B _b = new HtmlTagKinds.B();

        ///<summary>Base URL and default target browsing context for hyperlinks and forms. See: <a href="https://html.spec.whatwg.org/#the-base-element">https://html.spec.whatwg.org/#the-base-element</a><br /></summary>
        public static readonly HtmlTagKinds.BASE _base = new HtmlTagKinds.BASE();

        ///<summary>Text directionality isolation. See: <a href="https://html.spec.whatwg.org/#the-bdi-element">https://html.spec.whatwg.org/#the-bdi-element</a><br /></summary>
        public static readonly HtmlTagKinds.BDI _bdi = new HtmlTagKinds.BDI();

        ///<summary>Text directionality formatting. See: <a href="https://html.spec.whatwg.org/#the-bdo-element">https://html.spec.whatwg.org/#the-bdo-element</a><br /></summary>
        public static readonly HtmlTagKinds.BDO _bdo = new HtmlTagKinds.BDO();

        ///<summary>A section quoted from another source. See: <a href="https://html.spec.whatwg.org/#the-blockquote-element">https://html.spec.whatwg.org/#the-blockquote-element</a><br /></summary>
        public static readonly HtmlTagKinds.BLOCKQUOTE _blockquote = new HtmlTagKinds.BLOCKQUOTE();

        ///<summary>Document body. See: <a href="https://html.spec.whatwg.org/#the-body-element">https://html.spec.whatwg.org/#the-body-element</a><br /></summary>
        public static readonly HtmlTagKinds.BODY _body = new HtmlTagKinds.BODY();

        ///<summary>Line break, e.g. in poem or postal address. See: <a href="https://html.spec.whatwg.org/#the-br-element">https://html.spec.whatwg.org/#the-br-element</a><br /></summary>
        public static readonly HtmlTagKinds.BR _br = new HtmlTagKinds.BR();

        ///<summary>Button control. See: <a href="https://html.spec.whatwg.org/#the-button-element">https://html.spec.whatwg.org/#the-button-element</a><br /></summary>
        public static readonly HtmlTagKinds.BUTTON _button = new HtmlTagKinds.BUTTON();

        ///<summary>Scriptable bitmap canvas. See: <a href="https://html.spec.whatwg.org/#the-canvas-element">https://html.spec.whatwg.org/#the-canvas-element</a><br /></summary>
        public static readonly HtmlTagKinds.CANVAS _canvas = new HtmlTagKinds.CANVAS();

        ///<summary>Table caption. See: <a href="https://html.spec.whatwg.org/#the-caption-element">https://html.spec.whatwg.org/#the-caption-element</a><br /></summary>
        public static readonly HtmlTagKinds.CAPTION _caption = new HtmlTagKinds.CAPTION();

        ///<summary>Title of a work. See: <a href="https://html.spec.whatwg.org/#the-cite-element">https://html.spec.whatwg.org/#the-cite-element</a><br /></summary>
        public static readonly HtmlTagKinds.CITE _cite = new HtmlTagKinds.CITE();

        ///<summary>Computer code. See: <a href="https://html.spec.whatwg.org/#the-code-element">https://html.spec.whatwg.org/#the-code-element</a><br /></summary>
        public static readonly HtmlTagKinds.CODE _code = new HtmlTagKinds.CODE();

        ///<summary>Table column. See: <a href="https://html.spec.whatwg.org/#the-col-element">https://html.spec.whatwg.org/#the-col-element</a><br /></summary>
        public static readonly HtmlTagKinds.COL _col = new HtmlTagKinds.COL();

        ///<summary>Group of columns in a table. See: <a href="https://html.spec.whatwg.org/#the-colgroup-element">https://html.spec.whatwg.org/#the-colgroup-element</a><br /></summary>
        public static readonly HtmlTagKinds.COLGROUP _colgroup = new HtmlTagKinds.COLGROUP();

        ///<summary>Machine-readable equivalent. See: <a href="https://html.spec.whatwg.org/#the-data-element">https://html.spec.whatwg.org/#the-data-element</a><br /></summary>
        public static readonly HtmlTagKinds.DATA _data = new HtmlTagKinds.DATA();

        ///<summary>Container for options for combo box control. See: <a href="https://html.spec.whatwg.org/#the-datalist-element">https://html.spec.whatwg.org/#the-datalist-element</a><br /></summary>
        public static readonly HtmlTagKinds.DATALIST _datalist = new HtmlTagKinds.DATALIST();

        ///<summary>Content for corresponding dt element(s). See: <a href="https://html.spec.whatwg.org/#the-dd-element">https://html.spec.whatwg.org/#the-dd-element</a><br /></summary>
        public static readonly HtmlTagKinds.DD _dd = new HtmlTagKinds.DD();

        ///<summary>A removal from the document. See: <a href="https://html.spec.whatwg.org/#the-del-element">https://html.spec.whatwg.org/#the-del-element</a><br /></summary>
        public static readonly HtmlTagKinds.DEL _del = new HtmlTagKinds.DEL();

        ///<summary>Disclosure control for hiding details. See: <a href="https://html.spec.whatwg.org/#the-details-element">https://html.spec.whatwg.org/#the-details-element</a><br /></summary>
        public static readonly HtmlTagKinds.DETAILS _details = new HtmlTagKinds.DETAILS();

        ///<summary>Defining instance. See: <a href="https://html.spec.whatwg.org/#the-dfn-element">https://html.spec.whatwg.org/#the-dfn-element</a><br /></summary>
        public static readonly HtmlTagKinds.DFN _dfn = new HtmlTagKinds.DFN();

        ///<summary>Dialog box or window. See: <a href="https://html.spec.whatwg.org/#the-dialog-element">https://html.spec.whatwg.org/#the-dialog-element</a><br /></summary>
        public static readonly HtmlTagKinds.DIALOG _dialog = new HtmlTagKinds.DIALOG();

        ///<summary>Generic flow container, or container for name-value groups in dl elements. See: <a href="https://html.spec.whatwg.org/#the-div-element">https://html.spec.whatwg.org/#the-div-element</a><br /></summary>
        public static readonly HtmlTagKinds.DIV _div = new HtmlTagKinds.DIV();

        ///<summary>Association list consisting of zero or more name-value groups. See: <a href="https://html.spec.whatwg.org/#the-dl-element">https://html.spec.whatwg.org/#the-dl-element</a><br /></summary>
        public static readonly HtmlTagKinds.DL _dl = new HtmlTagKinds.DL();

        ///<summary>Legend for corresponding dd element(s). See: <a href="https://html.spec.whatwg.org/#the-dt-element">https://html.spec.whatwg.org/#the-dt-element</a><br /></summary>
        public static readonly HtmlTagKinds.DT _dt = new HtmlTagKinds.DT();

        ///<summary>Stress emphasis. See: <a href="https://html.spec.whatwg.org/#the-em-element">https://html.spec.whatwg.org/#the-em-element</a><br /></summary>
        public static readonly HtmlTagKinds.EM _em = new HtmlTagKinds.EM();

        ///<summary>Plugin. See: <a href="https://html.spec.whatwg.org/#the-embed-element">https://html.spec.whatwg.org/#the-embed-element</a><br /></summary>
        public static readonly HtmlTagKinds.EMBED _embed = new HtmlTagKinds.EMBED();

        ///<summary>Group of form controls. See: <a href="https://html.spec.whatwg.org/#the-fieldset-element">https://html.spec.whatwg.org/#the-fieldset-element</a><br /></summary>
        public static readonly HtmlTagKinds.FIELDSET _fieldset = new HtmlTagKinds.FIELDSET();

        ///<summary>Caption for figure. See: <a href="https://html.spec.whatwg.org/#the-figcaption-element">https://html.spec.whatwg.org/#the-figcaption-element</a><br /></summary>
        public static readonly HtmlTagKinds.FIGCAPTION _figcaption = new HtmlTagKinds.FIGCAPTION();

        ///<summary>Figure with optional caption. See: <a href="https://html.spec.whatwg.org/#the-figure-element">https://html.spec.whatwg.org/#the-figure-element</a><br /></summary>
        public static readonly HtmlTagKinds.FIGURE _figure = new HtmlTagKinds.FIGURE();

        ///<summary>Footer for a page or section. See: <a href="https://html.spec.whatwg.org/#the-footer-element">https://html.spec.whatwg.org/#the-footer-element</a><br /></summary>
        public static readonly HtmlTagKinds.FOOTER _footer = new HtmlTagKinds.FOOTER();

        ///<summary>User-submittable form. See: <a href="https://html.spec.whatwg.org/#the-form-element">https://html.spec.whatwg.org/#the-form-element</a><br /></summary>
        public static readonly HtmlTagKinds.FORM _form = new HtmlTagKinds.FORM();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlTagKinds.H1 _h1 = new HtmlTagKinds.H1();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlTagKinds.H2 _h2 = new HtmlTagKinds.H2();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlTagKinds.H3 _h3 = new HtmlTagKinds.H3();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlTagKinds.H4 _h4 = new HtmlTagKinds.H4();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlTagKinds.H5 _h5 = new HtmlTagKinds.H5();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlTagKinds.H6 _h6 = new HtmlTagKinds.H6();

        ///<summary>Container for document metadata. See: <a href="https://html.spec.whatwg.org/#the-head-element">https://html.spec.whatwg.org/#the-head-element</a><br /></summary>
        public static readonly HtmlTagKinds.HEAD _head = new HtmlTagKinds.HEAD();

        ///<summary>Introductory or navigational aids for a page or section. See: <a href="https://html.spec.whatwg.org/#the-header-element">https://html.spec.whatwg.org/#the-header-element</a><br /></summary>
        public static readonly HtmlTagKinds.HEADER _header = new HtmlTagKinds.HEADER();

        ///<summary>heading group. See: <a href="https://html.spec.whatwg.org/#the-hgroup-element">https://html.spec.whatwg.org/#the-hgroup-element</a><br /></summary>
        public static readonly HtmlTagKinds.HGROUP _hgroup = new HtmlTagKinds.HGROUP();

        ///<summary>Thematic break. See: <a href="https://html.spec.whatwg.org/#the-hr-element">https://html.spec.whatwg.org/#the-hr-element</a><br /></summary>
        public static readonly HtmlTagKinds.HR _hr = new HtmlTagKinds.HR();

        ///<summary>Root element. See: <a href="https://html.spec.whatwg.org/#the-html-element">https://html.spec.whatwg.org/#the-html-element</a><br /></summary>
        public static readonly HtmlTagKinds.HTML _html = new HtmlTagKinds.HTML();

        ///<summary>Alternate voice. See: <a href="https://html.spec.whatwg.org/#the-i-element">https://html.spec.whatwg.org/#the-i-element</a><br /></summary>
        public static readonly HtmlTagKinds.I _i = new HtmlTagKinds.I();

        ///<summary>Nested browsing context. See: <a href="https://html.spec.whatwg.org/#the-iframe-element">https://html.spec.whatwg.org/#the-iframe-element</a><br /></summary>
        public static readonly HtmlTagKinds.IFRAME _iframe = new HtmlTagKinds.IFRAME();

        ///<summary>Image. See: <a href="https://html.spec.whatwg.org/#the-img-element">https://html.spec.whatwg.org/#the-img-element</a><br /></summary>
        public static readonly HtmlTagKinds.IMG _img = new HtmlTagKinds.IMG();

        ///<summary>Form control. See: <a href="https://html.spec.whatwg.org/#the-input-element">https://html.spec.whatwg.org/#the-input-element</a><br /></summary>
        public static readonly HtmlTagKinds.INPUT _input = new HtmlTagKinds.INPUT();

        ///<summary>An addition to the document. See: <a href="https://html.spec.whatwg.org/#the-ins-element">https://html.spec.whatwg.org/#the-ins-element</a><br /></summary>
        public static readonly HtmlTagKinds.INS _ins = new HtmlTagKinds.INS();

        ///<summary>User input. See: <a href="https://html.spec.whatwg.org/#the-kbd-element">https://html.spec.whatwg.org/#the-kbd-element</a><br /></summary>
        public static readonly HtmlTagKinds.KBD _kbd = new HtmlTagKinds.KBD();

        ///<summary>Caption for a form control. See: <a href="https://html.spec.whatwg.org/#the-label-element">https://html.spec.whatwg.org/#the-label-element</a><br /></summary>
        public static readonly HtmlTagKinds.LABEL _label = new HtmlTagKinds.LABEL();

        ///<summary>Caption for fieldset. See: <a href="https://html.spec.whatwg.org/#the-legend-element">https://html.spec.whatwg.org/#the-legend-element</a><br /></summary>
        public static readonly HtmlTagKinds.LEGEND _legend = new HtmlTagKinds.LEGEND();

        ///<summary>List item. See: <a href="https://html.spec.whatwg.org/#the-li-element">https://html.spec.whatwg.org/#the-li-element</a><br /></summary>
        public static readonly HtmlTagKinds.LI _li = new HtmlTagKinds.LI();

        ///<summary>Link metadata. See: <a href="https://html.spec.whatwg.org/#the-link-element">https://html.spec.whatwg.org/#the-link-element</a><br /></summary>
        public static readonly HtmlTagKinds.LINK _link = new HtmlTagKinds.LINK();

        ///<summary>Container for the dominant contents of another element. See: <a href="https://html.spec.whatwg.org/#the-main-element">https://html.spec.whatwg.org/#the-main-element</a><br /></summary>
        public static readonly HtmlTagKinds.MAIN _main = new HtmlTagKinds.MAIN();

        ///<summary>Image map. See: <a href="https://html.spec.whatwg.org/#the-map-element">https://html.spec.whatwg.org/#the-map-element</a><br /></summary>
        public static readonly HtmlTagKinds.MAP _map = new HtmlTagKinds.MAP();

        ///<summary>Highlight. See: <a href="https://html.spec.whatwg.org/#the-mark-element">https://html.spec.whatwg.org/#the-mark-element</a><br /></summary>
        public static readonly HtmlTagKinds.MARK _mark = new HtmlTagKinds.MARK();

        ///<summary>Menu of commands. See: <a href="https://html.spec.whatwg.org/#the-menu-element">https://html.spec.whatwg.org/#the-menu-element</a><br /></summary>
        public static readonly HtmlTagKinds.MENU _menu = new HtmlTagKinds.MENU();

        ///<summary>Menu command. See: <a href="https://html.spec.whatwg.org/#the-menuitem-element">https://html.spec.whatwg.org/#the-menuitem-element</a><br /></summary>
        public static readonly HtmlTagKinds.MENUITEM _menuitem = new HtmlTagKinds.MENUITEM();

        ///<summary>Text metadata. See: <a href="https://html.spec.whatwg.org/#the-meta-element">https://html.spec.whatwg.org/#the-meta-element</a><br /></summary>
        public static readonly HtmlTagKinds.META _meta = new HtmlTagKinds.META();

        ///<summary>Gauge. See: <a href="https://html.spec.whatwg.org/#the-meter-element">https://html.spec.whatwg.org/#the-meter-element</a><br /></summary>
        public static readonly HtmlTagKinds.METER _meter = new HtmlTagKinds.METER();

        ///<summary>Section with navigational links. See: <a href="https://html.spec.whatwg.org/#the-nav-element">https://html.spec.whatwg.org/#the-nav-element</a><br /></summary>
        public static readonly HtmlTagKinds.NAV _nav = new HtmlTagKinds.NAV();

        ///<summary>Fallback content for script. See: <a href="https://html.spec.whatwg.org/#the-noscript-element">https://html.spec.whatwg.org/#the-noscript-element</a><br /></summary>
        public static readonly HtmlTagKinds.NOSCRIPT _noscript = new HtmlTagKinds.NOSCRIPT();

        ///<summary>Image, nested browsing context, or plugin. See: <a href="https://html.spec.whatwg.org/#the-object-element">https://html.spec.whatwg.org/#the-object-element</a><br /></summary>
        public static readonly HtmlTagKinds.OBJECT _object = new HtmlTagKinds.OBJECT();

        ///<summary>Ordered list. See: <a href="https://html.spec.whatwg.org/#the-ol-element">https://html.spec.whatwg.org/#the-ol-element</a><br /></summary>
        public static readonly HtmlTagKinds.OL _ol = new HtmlTagKinds.OL();

        ///<summary>Group of options in a list box. See: <a href="https://html.spec.whatwg.org/#the-optgroup-element">https://html.spec.whatwg.org/#the-optgroup-element</a><br /></summary>
        public static readonly HtmlTagKinds.OPTGROUP _optgroup = new HtmlTagKinds.OPTGROUP();

        ///<summary>Option in a list box or combo box control. See: <a href="https://html.spec.whatwg.org/#the-option-element">https://html.spec.whatwg.org/#the-option-element</a><br /></summary>
        public static readonly HtmlTagKinds.OPTION _option = new HtmlTagKinds.OPTION();

        ///<summary>Calculated output value. See: <a href="https://html.spec.whatwg.org/#the-output-element">https://html.spec.whatwg.org/#the-output-element</a><br /></summary>
        public static readonly HtmlTagKinds.OUTPUT _output = new HtmlTagKinds.OUTPUT();

        ///<summary>Paragraph. See: <a href="https://html.spec.whatwg.org/#the-p-element">https://html.spec.whatwg.org/#the-p-element</a><br /></summary>
        public static readonly HtmlTagKinds.P _p = new HtmlTagKinds.P();

        ///<summary>Parameter for object. See: <a href="https://html.spec.whatwg.org/#the-param-element">https://html.spec.whatwg.org/#the-param-element</a><br /></summary>
        public static readonly HtmlTagKinds.PARAM _param = new HtmlTagKinds.PARAM();

        ///<summary>Image. See: <a href="https://html.spec.whatwg.org/#the-picture-element">https://html.spec.whatwg.org/#the-picture-element</a><br /></summary>
        public static readonly HtmlTagKinds.PICTURE _picture = new HtmlTagKinds.PICTURE();

        ///<summary>Block of preformatted text. See: <a href="https://html.spec.whatwg.org/#the-pre-element">https://html.spec.whatwg.org/#the-pre-element</a><br /></summary>
        public static readonly HtmlTagKinds.PRE _pre = new HtmlTagKinds.PRE();

        ///<summary>Progress bar. See: <a href="https://html.spec.whatwg.org/#the-progress-element">https://html.spec.whatwg.org/#the-progress-element</a><br /></summary>
        public static readonly HtmlTagKinds.PROGRESS _progress = new HtmlTagKinds.PROGRESS();

        ///<summary>Quotation. See: <a href="https://html.spec.whatwg.org/#the-q-element">https://html.spec.whatwg.org/#the-q-element</a><br /></summary>
        public static readonly HtmlTagKinds.Q _q = new HtmlTagKinds.Q();

        ///<summary>Parenthesis for ruby annotation text. See: <a href="https://html.spec.whatwg.org/#the-rp-element">https://html.spec.whatwg.org/#the-rp-element</a><br /></summary>
        public static readonly HtmlTagKinds.RP _rp = new HtmlTagKinds.RP();

        ///<summary>Ruby annotation text. See: <a href="https://html.spec.whatwg.org/#the-rt-element">https://html.spec.whatwg.org/#the-rt-element</a><br /></summary>
        public static readonly HtmlTagKinds.RT _rt = new HtmlTagKinds.RT();

        ///<summary>Ruby annotation(s). See: <a href="https://html.spec.whatwg.org/#the-ruby-element">https://html.spec.whatwg.org/#the-ruby-element</a><br /></summary>
        public static readonly HtmlTagKinds.RUBY _ruby = new HtmlTagKinds.RUBY();

        ///<summary>Inaccurate text. See: <a href="https://html.spec.whatwg.org/#the-s-element">https://html.spec.whatwg.org/#the-s-element</a><br /></summary>
        public static readonly HtmlTagKinds.S _s = new HtmlTagKinds.S();

        ///<summary>Computer output. See: <a href="https://html.spec.whatwg.org/#the-samp-element">https://html.spec.whatwg.org/#the-samp-element</a><br /></summary>
        public static readonly HtmlTagKinds.SAMP _samp = new HtmlTagKinds.SAMP();

        ///<summary>Embedded script. See: <a href="https://html.spec.whatwg.org/#the-script-element">https://html.spec.whatwg.org/#the-script-element</a><br /></summary>
        public static readonly HtmlTagKinds.SCRIPT _script = new HtmlTagKinds.SCRIPT();

        ///<summary>Generic document or application section. See: <a href="https://html.spec.whatwg.org/#the-section-element">https://html.spec.whatwg.org/#the-section-element</a><br /></summary>
        public static readonly HtmlTagKinds.SECTION _section = new HtmlTagKinds.SECTION();

        ///<summary>List box control. See: <a href="https://html.spec.whatwg.org/#the-select-element">https://html.spec.whatwg.org/#the-select-element</a><br /></summary>
        public static readonly HtmlTagKinds.SELECT _select = new HtmlTagKinds.SELECT();

        ///<summary>Shadow tree slot. See: <a href="https://html.spec.whatwg.org/#the-slot-element">https://html.spec.whatwg.org/#the-slot-element</a><br /></summary>
        public static readonly HtmlTagKinds.SLOT _slot = new HtmlTagKinds.SLOT();

        ///<summary>Side comment. See: <a href="https://html.spec.whatwg.org/#the-small-element">https://html.spec.whatwg.org/#the-small-element</a><br /></summary>
        public static readonly HtmlTagKinds.SMALL _small = new HtmlTagKinds.SMALL();

        ///<summary>Image source for img or media source for video or audio. See: <a href="https://html.spec.whatwg.org/#the-source-element">https://html.spec.whatwg.org/#the-source-element</a><br /></summary>
        public static readonly HtmlTagKinds.SOURCE _source = new HtmlTagKinds.SOURCE();

        ///<summary>Generic phrasing container. See: <a href="https://html.spec.whatwg.org/#the-span-element">https://html.spec.whatwg.org/#the-span-element</a><br /></summary>
        public static readonly HtmlTagKinds.SPAN _span = new HtmlTagKinds.SPAN();

        ///<summary>Importance. See: <a href="https://html.spec.whatwg.org/#the-strong-element">https://html.spec.whatwg.org/#the-strong-element</a><br /></summary>
        public static readonly HtmlTagKinds.STRONG _strong = new HtmlTagKinds.STRONG();

        ///<summary>Embedded styling information. See: <a href="https://html.spec.whatwg.org/#the-style-element">https://html.spec.whatwg.org/#the-style-element</a><br /></summary>
        public static readonly HtmlTagKinds.STYLE _style = new HtmlTagKinds.STYLE();

        ///<summary>Subscript. See: <a href="https://html.spec.whatwg.org/#the-sub-and-sup-elements">https://html.spec.whatwg.org/#the-sub-and-sup-elements</a><br /></summary>
        public static readonly HtmlTagKinds.SUB _sub = new HtmlTagKinds.SUB();

        ///<summary>Caption for details. See: <a href="https://html.spec.whatwg.org/#the-summary-element">https://html.spec.whatwg.org/#the-summary-element</a><br /></summary>
        public static readonly HtmlTagKinds.SUMMARY _summary = new HtmlTagKinds.SUMMARY();

        ///<summary>Superscript. See: <a href="https://html.spec.whatwg.org/#the-sub-and-sup-elements">https://html.spec.whatwg.org/#the-sub-and-sup-elements</a><br /></summary>
        public static readonly HtmlTagKinds.SUP _sup = new HtmlTagKinds.SUP();

        ///<summary>Table. See: <a href="https://html.spec.whatwg.org/#the-table-element">https://html.spec.whatwg.org/#the-table-element</a><br /></summary>
        public static readonly HtmlTagKinds.TABLE _table = new HtmlTagKinds.TABLE();

        ///<summary>Group of rows in a table. See: <a href="https://html.spec.whatwg.org/#the-tbody-element">https://html.spec.whatwg.org/#the-tbody-element</a><br /></summary>
        public static readonly HtmlTagKinds.TBODY _tbody = new HtmlTagKinds.TBODY();

        ///<summary>Table cell. See: <a href="https://html.spec.whatwg.org/#the-td-element">https://html.spec.whatwg.org/#the-td-element</a><br /></summary>
        public static readonly HtmlTagKinds.TD _td = new HtmlTagKinds.TD();

        ///<summary>Template. See: <a href="https://html.spec.whatwg.org/#the-template-element">https://html.spec.whatwg.org/#the-template-element</a><br /></summary>
        public static readonly HtmlTagKinds.TEMPLATE _template = new HtmlTagKinds.TEMPLATE();

        ///<summary>Multiline text controls. See: <a href="https://html.spec.whatwg.org/#the-textarea-element">https://html.spec.whatwg.org/#the-textarea-element</a><br /></summary>
        public static readonly HtmlTagKinds.TEXTAREA _textarea = new HtmlTagKinds.TEXTAREA();

        ///<summary>Group of footer rows in a table. See: <a href="https://html.spec.whatwg.org/#the-tfoot-element">https://html.spec.whatwg.org/#the-tfoot-element</a><br /></summary>
        public static readonly HtmlTagKinds.TFOOT _tfoot = new HtmlTagKinds.TFOOT();

        ///<summary>Table header cell. See: <a href="https://html.spec.whatwg.org/#the-th-element">https://html.spec.whatwg.org/#the-th-element</a><br /></summary>
        public static readonly HtmlTagKinds.TH _th = new HtmlTagKinds.TH();

        ///<summary>Group of heading rows in a table. See: <a href="https://html.spec.whatwg.org/#the-thead-element">https://html.spec.whatwg.org/#the-thead-element</a><br /></summary>
        public static readonly HtmlTagKinds.THEAD _thead = new HtmlTagKinds.THEAD();

        ///<summary>Machine-readable equivalent of date- or time-related data. See: <a href="https://html.spec.whatwg.org/#the-time-element">https://html.spec.whatwg.org/#the-time-element</a><br /></summary>
        public static readonly HtmlTagKinds.TIME _time = new HtmlTagKinds.TIME();

        ///<summary>Document title. See: <a href="https://html.spec.whatwg.org/#the-title-element">https://html.spec.whatwg.org/#the-title-element</a><br /></summary>
        public static readonly HtmlTagKinds.TITLE _title = new HtmlTagKinds.TITLE();

        ///<summary>Table row. See: <a href="https://html.spec.whatwg.org/#the-tr-element">https://html.spec.whatwg.org/#the-tr-element</a><br /></summary>
        public static readonly HtmlTagKinds.TR _tr = new HtmlTagKinds.TR();

        ///<summary>Timed text track. See: <a href="https://html.spec.whatwg.org/#the-track-element">https://html.spec.whatwg.org/#the-track-element</a><br /></summary>
        public static readonly HtmlTagKinds.TRACK _track = new HtmlTagKinds.TRACK();

        ///<summary>Keywords. See: <a href="https://html.spec.whatwg.org/#the-u-element">https://html.spec.whatwg.org/#the-u-element</a><br /></summary>
        public static readonly HtmlTagKinds.U _u = new HtmlTagKinds.U();

        ///<summary>List. See: <a href="https://html.spec.whatwg.org/#the-ul-element">https://html.spec.whatwg.org/#the-ul-element</a><br /></summary>
        public static readonly HtmlTagKinds.UL _ul = new HtmlTagKinds.UL();

        ///<summary>Variable. See: <a href="https://html.spec.whatwg.org/#the-var-element">https://html.spec.whatwg.org/#the-var-element</a><br /></summary>
        public static readonly HtmlTagKinds.VAR _var = new HtmlTagKinds.VAR();

        ///<summary>Video player. See: <a href="https://html.spec.whatwg.org/#the-video-element">https://html.spec.whatwg.org/#the-video-element</a><br /></summary>
        public static readonly HtmlTagKinds.VIDEO _video = new HtmlTagKinds.VIDEO();

        ///<summary>Line breaking opportunity. See: <a href="https://html.spec.whatwg.org/#the-wbr-element">https://html.spec.whatwg.org/#the-wbr-element</a><br /></summary>
        public static readonly HtmlTagKinds.WBR _wbr = new HtmlTagKinds.WBR();
    }

    namespace AttributeNameInterfaces
    {
        public interface IHasAttr_href { }
        public interface IHasAttr_target { }
        public interface IHasAttr_download { }
        public interface IHasAttr_ping { }
        public interface IHasAttr_rel { }
        public interface IHasAttr_hreflang { }
        public interface IHasAttr_type { }
        public interface IHasAttr_referrerpolicy { }
        public interface IHasAttr_alt { }
        public interface IHasAttr_coords { }
        public interface IHasAttr_shape { }
        public interface IHasAttr_src { }
        public interface IHasAttr_crossorigin { }
        public interface IHasAttr_preload { }
        public interface IHasAttr_autoplay { }
        public interface IHasAttr_loop { }
        public interface IHasAttr_muted { }
        public interface IHasAttr_controls { }
        public interface IHasAttr_cite { }
        public interface IHasAttr_onafterprint { }
        public interface IHasAttr_onbeforeprint { }
        public interface IHasAttr_onbeforeunload { }
        public interface IHasAttr_onhashchange { }
        public interface IHasAttr_onlanguagechange { }
        public interface IHasAttr_onmessage { }
        public interface IHasAttr_onmessageerror { }
        public interface IHasAttr_onoffline { }
        public interface IHasAttr_ononline { }
        public interface IHasAttr_onpagehide { }
        public interface IHasAttr_onpageshow { }
        public interface IHasAttr_onpopstate { }
        public interface IHasAttr_onrejectionhandled { }
        public interface IHasAttr_onstorage { }
        public interface IHasAttr_onunhandledrejection { }
        public interface IHasAttr_onunload { }
        public interface IHasAttr_autofocus { }
        public interface IHasAttr_disabled { }
        public interface IHasAttr_form { }
        public interface IHasAttr_formaction { }
        public interface IHasAttr_formenctype { }
        public interface IHasAttr_formmethod { }
        public interface IHasAttr_formnovalidate { }
        public interface IHasAttr_formtarget { }
        public interface IHasAttr_name { }
        public interface IHasAttr_value { }
        public interface IHasAttr_width { }
        public interface IHasAttr_height { }
        public interface IHasAttr_span { }
        public interface IHasAttr_datetime { }
        public interface IHasAttr_open { }
        public interface IHasAttr_accept_charset { }
        public interface IHasAttr_action { }
        public interface IHasAttr_autocomplete { }
        public interface IHasAttr_enctype { }
        public interface IHasAttr_method { }
        public interface IHasAttr_novalidate { }
        public interface IHasAttr_manifest { }
        public interface IHasAttr_srcdoc { }
        public interface IHasAttr_sandbox { }
        public interface IHasAttr_allowfullscreen { }
        public interface IHasAttr_allowpaymentrequest { }
        public interface IHasAttr_allowusermedia { }
        public interface IHasAttr_srcset { }
        public interface IHasAttr_usemap { }
        public interface IHasAttr_ismap { }
        public interface IHasAttr_accept { }
        public interface IHasAttr_checked { }
        public interface IHasAttr_dirname { }
        public interface IHasAttr_inputmode { }
        public interface IHasAttr_list { }
        public interface IHasAttr_max { }
        public interface IHasAttr_maxlength { }
        public interface IHasAttr_min { }
        public interface IHasAttr_minlength { }
        public interface IHasAttr_multiple { }
        public interface IHasAttr_pattern { }
        public interface IHasAttr_placeholder { }
        public interface IHasAttr_readonly { }
        public interface IHasAttr_required { }
        public interface IHasAttr_size { }
        public interface IHasAttr_step { }
        public interface IHasAttr_for { }
        public interface IHasAttr_as { }
        public interface IHasAttr_media { }
        public interface IHasAttr_sizes { }
        public interface IHasAttr_nonce { }
        public interface IHasAttr_integrity { }
        public interface IHasAttr_label { }
        public interface IHasAttr_icon { }
        public interface IHasAttr_radiogroup { }
        public interface IHasAttr_default { }
        public interface IHasAttr_http_equiv { }
        public interface IHasAttr_content { }
        public interface IHasAttr_charset { }
        public interface IHasAttr_low { }
        public interface IHasAttr_high { }
        public interface IHasAttr_optimum { }
        public interface IHasAttr_data { }
        public interface IHasAttr_typemustmatch { }
        public interface IHasAttr_reversed { }
        public interface IHasAttr_start { }
        public interface IHasAttr_selected { }
        public interface IHasAttr_async { }
        public interface IHasAttr_defer { }
        public interface IHasAttr_colspan { }
        public interface IHasAttr_rowspan { }
        public interface IHasAttr_headers { }
        public interface IHasAttr_cols { }
        public interface IHasAttr_rows { }
        public interface IHasAttr_wrap { }
        public interface IHasAttr_scope { }
        public interface IHasAttr_abbr { }
        public interface IHasAttr_kind { }
        public interface IHasAttr_srclang { }
        public interface IHasAttr_poster { }
        public interface IHasAttr_playsinline { }
    }

    public static class AttributeConstructionMethods
    {
        public static THtmlTag _accesskey<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("accesskey", attrValue);
        public static THtmlTag _class<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("class", attrValue);
        public static THtmlTag _contenteditable<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("contenteditable", attrValue);
        public static THtmlTag _contextmenu<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("contextmenu", attrValue);
        public static THtmlTag _dir<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("dir", attrValue);
        public static THtmlTag _draggable<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("draggable", attrValue);
        public static THtmlTag _hidden<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("hidden", attrValue);
        public static THtmlTag _id<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("id", attrValue);
        public static THtmlTag _is<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("is", attrValue);
        public static THtmlTag _itemid<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("itemid", attrValue);
        public static THtmlTag _itemprop<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("itemprop", attrValue);
        public static THtmlTag _itemref<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("itemref", attrValue);
        public static THtmlTag _itemscope<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("itemscope", attrValue);
        public static THtmlTag _itemtype<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("itemtype", attrValue);
        public static THtmlTag _lang<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("lang", attrValue);
        public static THtmlTag _slot<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("slot", attrValue);
        public static THtmlTag _spellcheck<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("spellcheck", attrValue);
        public static THtmlTag _style<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("style", attrValue);
        public static THtmlTag _tabindex<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("tabindex", attrValue);
        public static THtmlTag _title<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("title", attrValue);
        public static THtmlTag _translate<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("translate", attrValue);
        public static THtmlTag _href<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_href, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("href", attrValue);
        public static THtmlTag _target<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_target, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("target", attrValue);
        public static THtmlTag _download<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_download, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("download", attrValue);
        public static THtmlTag _ping<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_ping, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("ping", attrValue);
        public static THtmlTag _rel<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_rel, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("rel", attrValue);
        public static THtmlTag _hreflang<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_hreflang, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("hreflang", attrValue);
        public static THtmlTag _type<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_type, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("type", attrValue);
        public static THtmlTag _referrerpolicy<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_referrerpolicy, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("referrerpolicy", attrValue);
        public static THtmlTag _alt<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_alt, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("alt", attrValue);
        public static THtmlTag _coords<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_coords, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("coords", attrValue);
        public static THtmlTag _shape<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_shape, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("shape", attrValue);
        public static THtmlTag _src<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_src, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("src", attrValue);
        public static THtmlTag _crossorigin<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_crossorigin, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("crossorigin", attrValue);
        public static THtmlTag _preload<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_preload, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("preload", attrValue);
        public static THtmlTag _autoplay<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_autoplay, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("autoplay", attrValue);
        public static THtmlTag _loop<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_loop, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("loop", attrValue);
        public static THtmlTag _muted<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_muted, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("muted", attrValue);
        public static THtmlTag _controls<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_controls, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("controls", attrValue);
        public static THtmlTag _cite<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_cite, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("cite", attrValue);
        public static THtmlTag _onafterprint<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onafterprint, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onafterprint", attrValue);
        public static THtmlTag _onbeforeprint<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onbeforeprint, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onbeforeprint", attrValue);
        public static THtmlTag _onbeforeunload<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onbeforeunload, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onbeforeunload", attrValue);
        public static THtmlTag _onhashchange<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onhashchange, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onhashchange", attrValue);
        public static THtmlTag _onlanguagechange<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onlanguagechange, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onlanguagechange", attrValue);
        public static THtmlTag _onmessage<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onmessage, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onmessage", attrValue);
        public static THtmlTag _onmessageerror<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onmessageerror, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onmessageerror", attrValue);
        public static THtmlTag _onoffline<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onoffline, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onoffline", attrValue);
        public static THtmlTag _ononline<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_ononline, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("ononline", attrValue);
        public static THtmlTag _onpagehide<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onpagehide, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onpagehide", attrValue);
        public static THtmlTag _onpageshow<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onpageshow, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onpageshow", attrValue);
        public static THtmlTag _onpopstate<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onpopstate, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onpopstate", attrValue);
        public static THtmlTag _onrejectionhandled<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onrejectionhandled, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onrejectionhandled", attrValue);
        public static THtmlTag _onstorage<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onstorage, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onstorage", attrValue);
        public static THtmlTag _onunhandledrejection<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onunhandledrejection, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onunhandledrejection", attrValue);
        public static THtmlTag _onunload<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onunload, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("onunload", attrValue);
        public static THtmlTag _autofocus<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_autofocus, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("autofocus", attrValue);
        public static THtmlTag _disabled<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_disabled, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("disabled", attrValue);
        public static THtmlTag _form<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_form, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("form", attrValue);
        public static THtmlTag _formaction<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_formaction, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("formaction", attrValue);
        public static THtmlTag _formenctype<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_formenctype, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("formenctype", attrValue);
        public static THtmlTag _formmethod<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_formmethod, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("formmethod", attrValue);
        public static THtmlTag _formnovalidate<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_formnovalidate, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("formnovalidate", attrValue);
        public static THtmlTag _formtarget<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_formtarget, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("formtarget", attrValue);
        public static THtmlTag _name<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_name, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("name", attrValue);
        public static THtmlTag _value<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_value, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("value", attrValue);
        public static THtmlTag _width<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_width, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("width", attrValue);
        public static THtmlTag _height<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_height, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("height", attrValue);
        public static THtmlTag _span<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_span, IHtmlTag<THtmlTag>

            => htmlTagExpr.Attribute("span", attrValue);
        public static THtmlTag _datetime<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_datetime, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("datetime", attrValue);
        public static THtmlTag _open<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_open, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("open", attrValue);
        public static THtmlTag _accept_charset<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_accept_charset, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("accept-charset", attrValue);
        public static THtmlTag _action<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_action, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("action", attrValue);
        public static THtmlTag _autocomplete<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_autocomplete, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("autocomplete", attrValue);
        public static THtmlTag _enctype<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_enctype, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("enctype", attrValue);
        public static THtmlTag _method<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_method, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("method", attrValue);
        public static THtmlTag _novalidate<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_novalidate, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("novalidate", attrValue);
        public static THtmlTag _manifest<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_manifest, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("manifest", attrValue);
        public static THtmlTag _srcdoc<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_srcdoc, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("srcdoc", attrValue);
        public static THtmlTag _sandbox<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_sandbox, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("sandbox", attrValue);
        public static THtmlTag _allowfullscreen<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_allowfullscreen, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("allowfullscreen", attrValue);
        public static THtmlTag _allowpaymentrequest<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_allowpaymentrequest, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("allowpaymentrequest", attrValue);
        public static THtmlTag _allowusermedia<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_allowusermedia, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("allowusermedia", attrValue);
        public static THtmlTag _srcset<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_srcset, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("srcset", attrValue);
        public static THtmlTag _usemap<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_usemap, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("usemap", attrValue);
        public static THtmlTag _ismap<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_ismap, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("ismap", attrValue);
        public static THtmlTag _accept<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_accept, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("accept", attrValue);
        public static THtmlTag _checked<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_checked, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("checked", attrValue);
        public static THtmlTag _dirname<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_dirname, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("dirname", attrValue);
        public static THtmlTag _inputmode<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_inputmode, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("inputmode", attrValue);
        public static THtmlTag _list<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_list, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("list", attrValue);
        public static THtmlTag _max<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_max, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("max", attrValue);
        public static THtmlTag _maxlength<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_maxlength, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("maxlength", attrValue);
        public static THtmlTag _min<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_min, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("min", attrValue);
        public static THtmlTag _minlength<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_minlength, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("minlength", attrValue);
        public static THtmlTag _multiple<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_multiple, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("multiple", attrValue);
        public static THtmlTag _pattern<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_pattern, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("pattern", attrValue);
        public static THtmlTag _placeholder<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_placeholder, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("placeholder", attrValue);
        public static THtmlTag _readonly<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_readonly, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("readonly", attrValue);
        public static THtmlTag _required<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_required, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("required", attrValue);
        public static THtmlTag _size<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_size, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("size", attrValue);
        public static THtmlTag _step<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_step, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("step", attrValue);
        public static THtmlTag _for<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_for, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("for", attrValue);
        public static THtmlTag _as<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_as, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("as", attrValue);
        public static THtmlTag _media<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_media, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("media", attrValue);
        public static THtmlTag _sizes<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_sizes, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("sizes", attrValue);
        public static THtmlTag _nonce<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_nonce, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("nonce", attrValue);
        public static THtmlTag _integrity<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_integrity, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("integrity", attrValue);
        public static THtmlTag _label<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_label, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("label", attrValue);
        public static THtmlTag _icon<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_icon, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("icon", attrValue);
        public static THtmlTag _radiogroup<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_radiogroup, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("radiogroup", attrValue);
        public static THtmlTag _default<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_default, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("default", attrValue);
        public static THtmlTag _http_equiv<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_http_equiv, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("http-equiv", attrValue);
        public static THtmlTag _content<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_content, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("content", attrValue);
        public static THtmlTag _charset<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_charset, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("charset", attrValue);
        public static THtmlTag _low<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_low, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("low", attrValue);
        public static THtmlTag _high<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_high, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("high", attrValue);
        public static THtmlTag _optimum<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_optimum, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("optimum", attrValue);
        public static THtmlTag _data<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_data, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("data", attrValue);
        public static THtmlTag _typemustmatch<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_typemustmatch, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("typemustmatch", attrValue);
        public static THtmlTag _reversed<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_reversed, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("reversed", attrValue);
        public static THtmlTag _start<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_start, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("start", attrValue);
        public static THtmlTag _selected<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_selected, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("selected", attrValue);
        public static THtmlTag _async<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_async, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("async", attrValue);
        public static THtmlTag _defer<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_defer, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("defer", attrValue);
        public static THtmlTag _colspan<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_colspan, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("colspan", attrValue);
        public static THtmlTag _rowspan<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_rowspan, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("rowspan", attrValue);
        public static THtmlTag _headers<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_headers, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("headers", attrValue);
        public static THtmlTag _cols<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_cols, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("cols", attrValue);
        public static THtmlTag _rows<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_rows, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("rows", attrValue);
        public static THtmlTag _wrap<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_wrap, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("wrap", attrValue);
        public static THtmlTag _scope<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_scope, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("scope", attrValue);
        public static THtmlTag _abbr<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_abbr, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("abbr", attrValue);
        public static THtmlTag _kind<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_kind, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("kind", attrValue);
        public static THtmlTag _srclang<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_srclang, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("srclang", attrValue);
        public static THtmlTag _poster<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_poster, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("poster", attrValue);
        public static THtmlTag _playsinline<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_playsinline, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute("playsinline", attrValue);
    }
}
