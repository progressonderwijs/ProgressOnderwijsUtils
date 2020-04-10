using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    using AttributeNameInterfaces;

    public static class HtmlTagKinds
    {
        public struct A : IHtmlElementAllowingContent<A>, IHasAttr_href, IHasAttr_target, IHasAttr_download, IHasAttr_ping, IHasAttr_rel, IHasAttr_hreflang, IHasAttr_type, IHasAttr_referrerpolicy
        {
            public string TagName => "a";
            string IHtmlElement.TagStart => "<a";
            string IHtmlElement.EndTag => "</a>";
            HtmlAttributes attrs;
            A IHtmlElement<A>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new A { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            A IHtmlElementAllowingContent<A>.ReplaceContentWith(HtmlFragment replacementContents) => new A { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(A tag) => tag.AsFragment();
            public static HtmlFragment operator +(A head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, A tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct ABBR : IHtmlElementAllowingContent<ABBR>
        {
            public string TagName => "abbr";
            string IHtmlElement.TagStart => "<abbr";
            string IHtmlElement.EndTag => "</abbr>";
            HtmlAttributes attrs;
            ABBR IHtmlElement<ABBR>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new ABBR { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            ABBR IHtmlElementAllowingContent<ABBR>.ReplaceContentWith(HtmlFragment replacementContents) => new ABBR { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(ABBR tag) => tag.AsFragment();
            public static HtmlFragment operator +(ABBR head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, ABBR tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct ADDRESS : IHtmlElementAllowingContent<ADDRESS>
        {
            public string TagName => "address";
            string IHtmlElement.TagStart => "<address";
            string IHtmlElement.EndTag => "</address>";
            HtmlAttributes attrs;
            ADDRESS IHtmlElement<ADDRESS>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new ADDRESS { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            ADDRESS IHtmlElementAllowingContent<ADDRESS>.ReplaceContentWith(HtmlFragment replacementContents) => new ADDRESS { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(ADDRESS tag) => tag.AsFragment();
            public static HtmlFragment operator +(ADDRESS head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, ADDRESS tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct AREA : IHtmlElement<AREA>, IHasAttr_alt, IHasAttr_coords, IHasAttr_shape, IHasAttr_href, IHasAttr_target, IHasAttr_download, IHasAttr_ping, IHasAttr_rel, IHasAttr_referrerpolicy
        {
            public string TagName => "area";
            string IHtmlElement.TagStart => "<area";
            string IHtmlElement.EndTag => "";
            HtmlAttributes attrs;
            AREA IHtmlElement<AREA>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new AREA { attrs = replacementAttributes };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(AREA tag) => tag.AsFragment();
            public static HtmlFragment operator +(AREA head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, AREA tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct ARTICLE : IHtmlElementAllowingContent<ARTICLE>
        {
            public string TagName => "article";
            string IHtmlElement.TagStart => "<article";
            string IHtmlElement.EndTag => "</article>";
            HtmlAttributes attrs;
            ARTICLE IHtmlElement<ARTICLE>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new ARTICLE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            ARTICLE IHtmlElementAllowingContent<ARTICLE>.ReplaceContentWith(HtmlFragment replacementContents) => new ARTICLE { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(ARTICLE tag) => tag.AsFragment();
            public static HtmlFragment operator +(ARTICLE head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, ARTICLE tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct ASIDE : IHtmlElementAllowingContent<ASIDE>
        {
            public string TagName => "aside";
            string IHtmlElement.TagStart => "<aside";
            string IHtmlElement.EndTag => "</aside>";
            HtmlAttributes attrs;
            ASIDE IHtmlElement<ASIDE>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new ASIDE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            ASIDE IHtmlElementAllowingContent<ASIDE>.ReplaceContentWith(HtmlFragment replacementContents) => new ASIDE { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(ASIDE tag) => tag.AsFragment();
            public static HtmlFragment operator +(ASIDE head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, ASIDE tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct AUDIO : IHtmlElementAllowingContent<AUDIO>, IHasAttr_src, IHasAttr_crossorigin, IHasAttr_preload, IHasAttr_autoplay, IHasAttr_loop, IHasAttr_muted, IHasAttr_controls
        {
            public string TagName => "audio";
            string IHtmlElement.TagStart => "<audio";
            string IHtmlElement.EndTag => "</audio>";
            HtmlAttributes attrs;
            AUDIO IHtmlElement<AUDIO>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new AUDIO { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            AUDIO IHtmlElementAllowingContent<AUDIO>.ReplaceContentWith(HtmlFragment replacementContents) => new AUDIO { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(AUDIO tag) => tag.AsFragment();
            public static HtmlFragment operator +(AUDIO head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, AUDIO tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct B : IHtmlElementAllowingContent<B>
        {
            public string TagName => "b";
            string IHtmlElement.TagStart => "<b";
            string IHtmlElement.EndTag => "</b>";
            HtmlAttributes attrs;
            B IHtmlElement<B>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new B { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            B IHtmlElementAllowingContent<B>.ReplaceContentWith(HtmlFragment replacementContents) => new B { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(B tag) => tag.AsFragment();
            public static HtmlFragment operator +(B head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, B tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct BASE : IHtmlElement<BASE>, IHasAttr_href, IHasAttr_target
        {
            public string TagName => "base";
            string IHtmlElement.TagStart => "<base";
            string IHtmlElement.EndTag => "";
            HtmlAttributes attrs;
            BASE IHtmlElement<BASE>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new BASE { attrs = replacementAttributes };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(BASE tag) => tag.AsFragment();
            public static HtmlFragment operator +(BASE head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, BASE tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct BDI : IHtmlElementAllowingContent<BDI>
        {
            public string TagName => "bdi";
            string IHtmlElement.TagStart => "<bdi";
            string IHtmlElement.EndTag => "</bdi>";
            HtmlAttributes attrs;
            BDI IHtmlElement<BDI>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new BDI { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            BDI IHtmlElementAllowingContent<BDI>.ReplaceContentWith(HtmlFragment replacementContents) => new BDI { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(BDI tag) => tag.AsFragment();
            public static HtmlFragment operator +(BDI head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, BDI tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct BDO : IHtmlElementAllowingContent<BDO>
        {
            public string TagName => "bdo";
            string IHtmlElement.TagStart => "<bdo";
            string IHtmlElement.EndTag => "</bdo>";
            HtmlAttributes attrs;
            BDO IHtmlElement<BDO>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new BDO { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            BDO IHtmlElementAllowingContent<BDO>.ReplaceContentWith(HtmlFragment replacementContents) => new BDO { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(BDO tag) => tag.AsFragment();
            public static HtmlFragment operator +(BDO head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, BDO tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct BLOCKQUOTE : IHtmlElementAllowingContent<BLOCKQUOTE>, IHasAttr_cite
        {
            public string TagName => "blockquote";
            string IHtmlElement.TagStart => "<blockquote";
            string IHtmlElement.EndTag => "</blockquote>";
            HtmlAttributes attrs;
            BLOCKQUOTE IHtmlElement<BLOCKQUOTE>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new BLOCKQUOTE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            BLOCKQUOTE IHtmlElementAllowingContent<BLOCKQUOTE>.ReplaceContentWith(HtmlFragment replacementContents) => new BLOCKQUOTE { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(BLOCKQUOTE tag) => tag.AsFragment();
            public static HtmlFragment operator +(BLOCKQUOTE head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, BLOCKQUOTE tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct BODY : IHtmlElementAllowingContent<BODY>, IHasAttr_onafterprint, IHasAttr_onbeforeprint, IHasAttr_onbeforeunload, IHasAttr_onhashchange, IHasAttr_onlanguagechange, IHasAttr_onmessage, IHasAttr_onmessageerror, IHasAttr_onoffline, IHasAttr_ononline, IHasAttr_onpagehide, IHasAttr_onpageshow, IHasAttr_onpopstate, IHasAttr_onrejectionhandled, IHasAttr_onstorage, IHasAttr_onunhandledrejection, IHasAttr_onunload
        {
            public string TagName => "body";
            string IHtmlElement.TagStart => "<body";
            string IHtmlElement.EndTag => "</body>";
            HtmlAttributes attrs;
            BODY IHtmlElement<BODY>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new BODY { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            BODY IHtmlElementAllowingContent<BODY>.ReplaceContentWith(HtmlFragment replacementContents) => new BODY { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(BODY tag) => tag.AsFragment();
            public static HtmlFragment operator +(BODY head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, BODY tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct BR : IHtmlElement<BR>
        {
            public string TagName => "br";
            string IHtmlElement.TagStart => "<br";
            string IHtmlElement.EndTag => "";
            HtmlAttributes attrs;
            BR IHtmlElement<BR>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new BR { attrs = replacementAttributes };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(BR tag) => tag.AsFragment();
            public static HtmlFragment operator +(BR head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, BR tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct BUTTON : IHtmlElementAllowingContent<BUTTON>, IHasAttr_disabled, IHasAttr_form, IHasAttr_formaction, IHasAttr_formenctype, IHasAttr_formmethod, IHasAttr_formnovalidate, IHasAttr_formtarget, IHasAttr_name, IHasAttr_type, IHasAttr_value
        {
            public string TagName => "button";
            string IHtmlElement.TagStart => "<button";
            string IHtmlElement.EndTag => "</button>";
            HtmlAttributes attrs;
            BUTTON IHtmlElement<BUTTON>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new BUTTON { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            BUTTON IHtmlElementAllowingContent<BUTTON>.ReplaceContentWith(HtmlFragment replacementContents) => new BUTTON { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(BUTTON tag) => tag.AsFragment();
            public static HtmlFragment operator +(BUTTON head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, BUTTON tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct CANVAS : IHtmlElementAllowingContent<CANVAS>, IHasAttr_width, IHasAttr_height
        {
            public string TagName => "canvas";
            string IHtmlElement.TagStart => "<canvas";
            string IHtmlElement.EndTag => "</canvas>";
            HtmlAttributes attrs;
            CANVAS IHtmlElement<CANVAS>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new CANVAS { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            CANVAS IHtmlElementAllowingContent<CANVAS>.ReplaceContentWith(HtmlFragment replacementContents) => new CANVAS { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(CANVAS tag) => tag.AsFragment();
            public static HtmlFragment operator +(CANVAS head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, CANVAS tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct CAPTION : IHtmlElementAllowingContent<CAPTION>
        {
            public string TagName => "caption";
            string IHtmlElement.TagStart => "<caption";
            string IHtmlElement.EndTag => "</caption>";
            HtmlAttributes attrs;
            CAPTION IHtmlElement<CAPTION>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new CAPTION { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            CAPTION IHtmlElementAllowingContent<CAPTION>.ReplaceContentWith(HtmlFragment replacementContents) => new CAPTION { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(CAPTION tag) => tag.AsFragment();
            public static HtmlFragment operator +(CAPTION head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, CAPTION tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct CITE : IHtmlElementAllowingContent<CITE>
        {
            public string TagName => "cite";
            string IHtmlElement.TagStart => "<cite";
            string IHtmlElement.EndTag => "</cite>";
            HtmlAttributes attrs;
            CITE IHtmlElement<CITE>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new CITE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            CITE IHtmlElementAllowingContent<CITE>.ReplaceContentWith(HtmlFragment replacementContents) => new CITE { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(CITE tag) => tag.AsFragment();
            public static HtmlFragment operator +(CITE head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, CITE tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct CODE : IHtmlElementAllowingContent<CODE>
        {
            public string TagName => "code";
            string IHtmlElement.TagStart => "<code";
            string IHtmlElement.EndTag => "</code>";
            HtmlAttributes attrs;
            CODE IHtmlElement<CODE>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new CODE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            CODE IHtmlElementAllowingContent<CODE>.ReplaceContentWith(HtmlFragment replacementContents) => new CODE { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(CODE tag) => tag.AsFragment();
            public static HtmlFragment operator +(CODE head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, CODE tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct COL : IHtmlElement<COL>, IHasAttr_span
        {
            public string TagName => "col";
            string IHtmlElement.TagStart => "<col";
            string IHtmlElement.EndTag => "";
            HtmlAttributes attrs;
            COL IHtmlElement<COL>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new COL { attrs = replacementAttributes };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(COL tag) => tag.AsFragment();
            public static HtmlFragment operator +(COL head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, COL tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct COLGROUP : IHtmlElementAllowingContent<COLGROUP>, IHasAttr_span
        {
            public string TagName => "colgroup";
            string IHtmlElement.TagStart => "<colgroup";
            string IHtmlElement.EndTag => "</colgroup>";
            HtmlAttributes attrs;
            COLGROUP IHtmlElement<COLGROUP>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new COLGROUP { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            COLGROUP IHtmlElementAllowingContent<COLGROUP>.ReplaceContentWith(HtmlFragment replacementContents) => new COLGROUP { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(COLGROUP tag) => tag.AsFragment();
            public static HtmlFragment operator +(COLGROUP head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, COLGROUP tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct DATA : IHtmlElementAllowingContent<DATA>, IHasAttr_value
        {
            public string TagName => "data";
            string IHtmlElement.TagStart => "<data";
            string IHtmlElement.EndTag => "</data>";
            HtmlAttributes attrs;
            DATA IHtmlElement<DATA>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new DATA { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            DATA IHtmlElementAllowingContent<DATA>.ReplaceContentWith(HtmlFragment replacementContents) => new DATA { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(DATA tag) => tag.AsFragment();
            public static HtmlFragment operator +(DATA head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, DATA tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct DATALIST : IHtmlElementAllowingContent<DATALIST>
        {
            public string TagName => "datalist";
            string IHtmlElement.TagStart => "<datalist";
            string IHtmlElement.EndTag => "</datalist>";
            HtmlAttributes attrs;
            DATALIST IHtmlElement<DATALIST>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new DATALIST { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            DATALIST IHtmlElementAllowingContent<DATALIST>.ReplaceContentWith(HtmlFragment replacementContents) => new DATALIST { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(DATALIST tag) => tag.AsFragment();
            public static HtmlFragment operator +(DATALIST head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, DATALIST tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct DD : IHtmlElementAllowingContent<DD>
        {
            public string TagName => "dd";
            string IHtmlElement.TagStart => "<dd";
            string IHtmlElement.EndTag => "</dd>";
            HtmlAttributes attrs;
            DD IHtmlElement<DD>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new DD { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            DD IHtmlElementAllowingContent<DD>.ReplaceContentWith(HtmlFragment replacementContents) => new DD { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(DD tag) => tag.AsFragment();
            public static HtmlFragment operator +(DD head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, DD tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct DEL : IHtmlElementAllowingContent<DEL>, IHasAttr_cite, IHasAttr_datetime
        {
            public string TagName => "del";
            string IHtmlElement.TagStart => "<del";
            string IHtmlElement.EndTag => "</del>";
            HtmlAttributes attrs;
            DEL IHtmlElement<DEL>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new DEL { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            DEL IHtmlElementAllowingContent<DEL>.ReplaceContentWith(HtmlFragment replacementContents) => new DEL { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(DEL tag) => tag.AsFragment();
            public static HtmlFragment operator +(DEL head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, DEL tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct DETAILS : IHtmlElementAllowingContent<DETAILS>, IHasAttr_open
        {
            public string TagName => "details";
            string IHtmlElement.TagStart => "<details";
            string IHtmlElement.EndTag => "</details>";
            HtmlAttributes attrs;
            DETAILS IHtmlElement<DETAILS>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new DETAILS { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            DETAILS IHtmlElementAllowingContent<DETAILS>.ReplaceContentWith(HtmlFragment replacementContents) => new DETAILS { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(DETAILS tag) => tag.AsFragment();
            public static HtmlFragment operator +(DETAILS head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, DETAILS tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct DFN : IHtmlElementAllowingContent<DFN>
        {
            public string TagName => "dfn";
            string IHtmlElement.TagStart => "<dfn";
            string IHtmlElement.EndTag => "</dfn>";
            HtmlAttributes attrs;
            DFN IHtmlElement<DFN>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new DFN { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            DFN IHtmlElementAllowingContent<DFN>.ReplaceContentWith(HtmlFragment replacementContents) => new DFN { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(DFN tag) => tag.AsFragment();
            public static HtmlFragment operator +(DFN head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, DFN tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct DIALOG : IHtmlElementAllowingContent<DIALOG>, IHasAttr_open
        {
            public string TagName => "dialog";
            string IHtmlElement.TagStart => "<dialog";
            string IHtmlElement.EndTag => "</dialog>";
            HtmlAttributes attrs;
            DIALOG IHtmlElement<DIALOG>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new DIALOG { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            DIALOG IHtmlElementAllowingContent<DIALOG>.ReplaceContentWith(HtmlFragment replacementContents) => new DIALOG { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(DIALOG tag) => tag.AsFragment();
            public static HtmlFragment operator +(DIALOG head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, DIALOG tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct DIV : IHtmlElementAllowingContent<DIV>
        {
            public string TagName => "div";
            string IHtmlElement.TagStart => "<div";
            string IHtmlElement.EndTag => "</div>";
            HtmlAttributes attrs;
            DIV IHtmlElement<DIV>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new DIV { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            DIV IHtmlElementAllowingContent<DIV>.ReplaceContentWith(HtmlFragment replacementContents) => new DIV { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(DIV tag) => tag.AsFragment();
            public static HtmlFragment operator +(DIV head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, DIV tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct DL : IHtmlElementAllowingContent<DL>
        {
            public string TagName => "dl";
            string IHtmlElement.TagStart => "<dl";
            string IHtmlElement.EndTag => "</dl>";
            HtmlAttributes attrs;
            DL IHtmlElement<DL>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new DL { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            DL IHtmlElementAllowingContent<DL>.ReplaceContentWith(HtmlFragment replacementContents) => new DL { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(DL tag) => tag.AsFragment();
            public static HtmlFragment operator +(DL head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, DL tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct DT : IHtmlElementAllowingContent<DT>
        {
            public string TagName => "dt";
            string IHtmlElement.TagStart => "<dt";
            string IHtmlElement.EndTag => "</dt>";
            HtmlAttributes attrs;
            DT IHtmlElement<DT>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new DT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            DT IHtmlElementAllowingContent<DT>.ReplaceContentWith(HtmlFragment replacementContents) => new DT { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(DT tag) => tag.AsFragment();
            public static HtmlFragment operator +(DT head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, DT tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct EM : IHtmlElementAllowingContent<EM>
        {
            public string TagName => "em";
            string IHtmlElement.TagStart => "<em";
            string IHtmlElement.EndTag => "</em>";
            HtmlAttributes attrs;
            EM IHtmlElement<EM>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new EM { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            EM IHtmlElementAllowingContent<EM>.ReplaceContentWith(HtmlFragment replacementContents) => new EM { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(EM tag) => tag.AsFragment();
            public static HtmlFragment operator +(EM head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, EM tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct EMBED : IHtmlElement<EMBED>, IHasAttr_src, IHasAttr_type, IHasAttr_width, IHasAttr_height
        {
            public string TagName => "embed";
            string IHtmlElement.TagStart => "<embed";
            string IHtmlElement.EndTag => "";
            HtmlAttributes attrs;
            EMBED IHtmlElement<EMBED>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new EMBED { attrs = replacementAttributes };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(EMBED tag) => tag.AsFragment();
            public static HtmlFragment operator +(EMBED head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, EMBED tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct FIELDSET : IHtmlElementAllowingContent<FIELDSET>, IHasAttr_disabled, IHasAttr_form, IHasAttr_name
        {
            public string TagName => "fieldset";
            string IHtmlElement.TagStart => "<fieldset";
            string IHtmlElement.EndTag => "</fieldset>";
            HtmlAttributes attrs;
            FIELDSET IHtmlElement<FIELDSET>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new FIELDSET { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            FIELDSET IHtmlElementAllowingContent<FIELDSET>.ReplaceContentWith(HtmlFragment replacementContents) => new FIELDSET { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(FIELDSET tag) => tag.AsFragment();
            public static HtmlFragment operator +(FIELDSET head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, FIELDSET tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct FIGCAPTION : IHtmlElementAllowingContent<FIGCAPTION>
        {
            public string TagName => "figcaption";
            string IHtmlElement.TagStart => "<figcaption";
            string IHtmlElement.EndTag => "</figcaption>";
            HtmlAttributes attrs;
            FIGCAPTION IHtmlElement<FIGCAPTION>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new FIGCAPTION { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            FIGCAPTION IHtmlElementAllowingContent<FIGCAPTION>.ReplaceContentWith(HtmlFragment replacementContents) => new FIGCAPTION { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(FIGCAPTION tag) => tag.AsFragment();
            public static HtmlFragment operator +(FIGCAPTION head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, FIGCAPTION tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct FIGURE : IHtmlElementAllowingContent<FIGURE>
        {
            public string TagName => "figure";
            string IHtmlElement.TagStart => "<figure";
            string IHtmlElement.EndTag => "</figure>";
            HtmlAttributes attrs;
            FIGURE IHtmlElement<FIGURE>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new FIGURE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            FIGURE IHtmlElementAllowingContent<FIGURE>.ReplaceContentWith(HtmlFragment replacementContents) => new FIGURE { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(FIGURE tag) => tag.AsFragment();
            public static HtmlFragment operator +(FIGURE head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, FIGURE tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct FOOTER : IHtmlElementAllowingContent<FOOTER>
        {
            public string TagName => "footer";
            string IHtmlElement.TagStart => "<footer";
            string IHtmlElement.EndTag => "</footer>";
            HtmlAttributes attrs;
            FOOTER IHtmlElement<FOOTER>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new FOOTER { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            FOOTER IHtmlElementAllowingContent<FOOTER>.ReplaceContentWith(HtmlFragment replacementContents) => new FOOTER { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(FOOTER tag) => tag.AsFragment();
            public static HtmlFragment operator +(FOOTER head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, FOOTER tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct FORM : IHtmlElementAllowingContent<FORM>, IHasAttr_accept_charset, IHasAttr_action, IHasAttr_autocomplete, IHasAttr_enctype, IHasAttr_method, IHasAttr_name, IHasAttr_novalidate, IHasAttr_target
        {
            public string TagName => "form";
            string IHtmlElement.TagStart => "<form";
            string IHtmlElement.EndTag => "</form>";
            HtmlAttributes attrs;
            FORM IHtmlElement<FORM>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new FORM { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            FORM IHtmlElementAllowingContent<FORM>.ReplaceContentWith(HtmlFragment replacementContents) => new FORM { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(FORM tag) => tag.AsFragment();
            public static HtmlFragment operator +(FORM head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, FORM tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct H1 : IHtmlElementAllowingContent<H1>
        {
            public string TagName => "h1";
            string IHtmlElement.TagStart => "<h1";
            string IHtmlElement.EndTag => "</h1>";
            HtmlAttributes attrs;
            H1 IHtmlElement<H1>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new H1 { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            H1 IHtmlElementAllowingContent<H1>.ReplaceContentWith(HtmlFragment replacementContents) => new H1 { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(H1 tag) => tag.AsFragment();
            public static HtmlFragment operator +(H1 head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, H1 tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct H2 : IHtmlElementAllowingContent<H2>
        {
            public string TagName => "h2";
            string IHtmlElement.TagStart => "<h2";
            string IHtmlElement.EndTag => "</h2>";
            HtmlAttributes attrs;
            H2 IHtmlElement<H2>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new H2 { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            H2 IHtmlElementAllowingContent<H2>.ReplaceContentWith(HtmlFragment replacementContents) => new H2 { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(H2 tag) => tag.AsFragment();
            public static HtmlFragment operator +(H2 head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, H2 tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct H3 : IHtmlElementAllowingContent<H3>
        {
            public string TagName => "h3";
            string IHtmlElement.TagStart => "<h3";
            string IHtmlElement.EndTag => "</h3>";
            HtmlAttributes attrs;
            H3 IHtmlElement<H3>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new H3 { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            H3 IHtmlElementAllowingContent<H3>.ReplaceContentWith(HtmlFragment replacementContents) => new H3 { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(H3 tag) => tag.AsFragment();
            public static HtmlFragment operator +(H3 head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, H3 tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct H4 : IHtmlElementAllowingContent<H4>
        {
            public string TagName => "h4";
            string IHtmlElement.TagStart => "<h4";
            string IHtmlElement.EndTag => "</h4>";
            HtmlAttributes attrs;
            H4 IHtmlElement<H4>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new H4 { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            H4 IHtmlElementAllowingContent<H4>.ReplaceContentWith(HtmlFragment replacementContents) => new H4 { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(H4 tag) => tag.AsFragment();
            public static HtmlFragment operator +(H4 head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, H4 tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct H5 : IHtmlElementAllowingContent<H5>
        {
            public string TagName => "h5";
            string IHtmlElement.TagStart => "<h5";
            string IHtmlElement.EndTag => "</h5>";
            HtmlAttributes attrs;
            H5 IHtmlElement<H5>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new H5 { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            H5 IHtmlElementAllowingContent<H5>.ReplaceContentWith(HtmlFragment replacementContents) => new H5 { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(H5 tag) => tag.AsFragment();
            public static HtmlFragment operator +(H5 head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, H5 tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct H6 : IHtmlElementAllowingContent<H6>
        {
            public string TagName => "h6";
            string IHtmlElement.TagStart => "<h6";
            string IHtmlElement.EndTag => "</h6>";
            HtmlAttributes attrs;
            H6 IHtmlElement<H6>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new H6 { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            H6 IHtmlElementAllowingContent<H6>.ReplaceContentWith(HtmlFragment replacementContents) => new H6 { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(H6 tag) => tag.AsFragment();
            public static HtmlFragment operator +(H6 head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, H6 tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct HEAD : IHtmlElementAllowingContent<HEAD>
        {
            public string TagName => "head";
            string IHtmlElement.TagStart => "<head";
            string IHtmlElement.EndTag => "</head>";
            HtmlAttributes attrs;
            HEAD IHtmlElement<HEAD>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new HEAD { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            HEAD IHtmlElementAllowingContent<HEAD>.ReplaceContentWith(HtmlFragment replacementContents) => new HEAD { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(HEAD tag) => tag.AsFragment();
            public static HtmlFragment operator +(HEAD head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, HEAD tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct HEADER : IHtmlElementAllowingContent<HEADER>
        {
            public string TagName => "header";
            string IHtmlElement.TagStart => "<header";
            string IHtmlElement.EndTag => "</header>";
            HtmlAttributes attrs;
            HEADER IHtmlElement<HEADER>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new HEADER { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            HEADER IHtmlElementAllowingContent<HEADER>.ReplaceContentWith(HtmlFragment replacementContents) => new HEADER { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(HEADER tag) => tag.AsFragment();
            public static HtmlFragment operator +(HEADER head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, HEADER tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct HGROUP : IHtmlElementAllowingContent<HGROUP>
        {
            public string TagName => "hgroup";
            string IHtmlElement.TagStart => "<hgroup";
            string IHtmlElement.EndTag => "</hgroup>";
            HtmlAttributes attrs;
            HGROUP IHtmlElement<HGROUP>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new HGROUP { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            HGROUP IHtmlElementAllowingContent<HGROUP>.ReplaceContentWith(HtmlFragment replacementContents) => new HGROUP { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(HGROUP tag) => tag.AsFragment();
            public static HtmlFragment operator +(HGROUP head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, HGROUP tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct HR : IHtmlElement<HR>
        {
            public string TagName => "hr";
            string IHtmlElement.TagStart => "<hr";
            string IHtmlElement.EndTag => "";
            HtmlAttributes attrs;
            HR IHtmlElement<HR>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new HR { attrs = replacementAttributes };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(HR tag) => tag.AsFragment();
            public static HtmlFragment operator +(HR head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, HR tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct HTML : IHtmlElementAllowingContent<HTML>, IHasAttr_manifest
        {
            public string TagName => "html";
            string IHtmlElement.TagStart => "<html";
            string IHtmlElement.EndTag => "</html>";
            HtmlAttributes attrs;
            HTML IHtmlElement<HTML>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new HTML { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            HTML IHtmlElementAllowingContent<HTML>.ReplaceContentWith(HtmlFragment replacementContents) => new HTML { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(HTML tag) => tag.AsFragment();
            public static HtmlFragment operator +(HTML head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, HTML tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct I : IHtmlElementAllowingContent<I>
        {
            public string TagName => "i";
            string IHtmlElement.TagStart => "<i";
            string IHtmlElement.EndTag => "</i>";
            HtmlAttributes attrs;
            I IHtmlElement<I>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new I { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            I IHtmlElementAllowingContent<I>.ReplaceContentWith(HtmlFragment replacementContents) => new I { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(I tag) => tag.AsFragment();
            public static HtmlFragment operator +(I head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, I tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct IFRAME : IHtmlElementAllowingContent<IFRAME>, IHasAttr_src, IHasAttr_srcdoc, IHasAttr_name, IHasAttr_sandbox, IHasAttr_allow, IHasAttr_allowfullscreen, IHasAttr_allowpaymentrequest, IHasAttr_width, IHasAttr_height, IHasAttr_referrerpolicy
        {
            public string TagName => "iframe";
            string IHtmlElement.TagStart => "<iframe";
            string IHtmlElement.EndTag => "</iframe>";
            HtmlAttributes attrs;
            IFRAME IHtmlElement<IFRAME>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new IFRAME { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;

            HtmlFragment children;
            IFRAME IHtmlElementAllowingContent<IFRAME>.ReplaceContentWith(HtmlFragment replacementContents) => new IFRAME { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(IFRAME tag) => tag.AsFragment();
            public static HtmlFragment operator +(IFRAME head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, IFRAME tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct IMG : IHtmlElement<IMG>, IHasAttr_alt, IHasAttr_src, IHasAttr_srcset, IHasAttr_sizes, IHasAttr_crossorigin, IHasAttr_usemap, IHasAttr_ismap, IHasAttr_width, IHasAttr_height, IHasAttr_referrerpolicy, IHasAttr_decoding, IHasAttr_loading
        {
            public string TagName => "img";
            string IHtmlElement.TagStart => "<img";
            string IHtmlElement.EndTag => "";
            HtmlAttributes attrs;
            IMG IHtmlElement<IMG>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new IMG { attrs = replacementAttributes };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(IMG tag) => tag.AsFragment();
            public static HtmlFragment operator +(IMG head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, IMG tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct INPUT : IHtmlElement<INPUT>, IHasAttr_accept, IHasAttr_alt, IHasAttr_autocomplete, IHasAttr_checked, IHasAttr_dirname, IHasAttr_disabled, IHasAttr_form, IHasAttr_formaction, IHasAttr_formenctype, IHasAttr_formmethod, IHasAttr_formnovalidate, IHasAttr_formtarget, IHasAttr_height, IHasAttr_list, IHasAttr_max, IHasAttr_maxlength, IHasAttr_min, IHasAttr_minlength, IHasAttr_multiple, IHasAttr_name, IHasAttr_pattern, IHasAttr_placeholder, IHasAttr_readonly, IHasAttr_required, IHasAttr_size, IHasAttr_src, IHasAttr_step, IHasAttr_type, IHasAttr_value, IHasAttr_width
        {
            public string TagName => "input";
            string IHtmlElement.TagStart => "<input";
            string IHtmlElement.EndTag => "";
            HtmlAttributes attrs;
            INPUT IHtmlElement<INPUT>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new INPUT { attrs = replacementAttributes };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(INPUT tag) => tag.AsFragment();
            public static HtmlFragment operator +(INPUT head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, INPUT tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct INS : IHtmlElementAllowingContent<INS>, IHasAttr_cite, IHasAttr_datetime
        {
            public string TagName => "ins";
            string IHtmlElement.TagStart => "<ins";
            string IHtmlElement.EndTag => "</ins>";
            HtmlAttributes attrs;
            INS IHtmlElement<INS>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new INS { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            INS IHtmlElementAllowingContent<INS>.ReplaceContentWith(HtmlFragment replacementContents) => new INS { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(INS tag) => tag.AsFragment();
            public static HtmlFragment operator +(INS head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, INS tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct KBD : IHtmlElementAllowingContent<KBD>
        {
            public string TagName => "kbd";
            string IHtmlElement.TagStart => "<kbd";
            string IHtmlElement.EndTag => "</kbd>";
            HtmlAttributes attrs;
            KBD IHtmlElement<KBD>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new KBD { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            KBD IHtmlElementAllowingContent<KBD>.ReplaceContentWith(HtmlFragment replacementContents) => new KBD { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(KBD tag) => tag.AsFragment();
            public static HtmlFragment operator +(KBD head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, KBD tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct LABEL : IHtmlElementAllowingContent<LABEL>, IHasAttr_for
        {
            public string TagName => "label";
            string IHtmlElement.TagStart => "<label";
            string IHtmlElement.EndTag => "</label>";
            HtmlAttributes attrs;
            LABEL IHtmlElement<LABEL>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new LABEL { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            LABEL IHtmlElementAllowingContent<LABEL>.ReplaceContentWith(HtmlFragment replacementContents) => new LABEL { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(LABEL tag) => tag.AsFragment();
            public static HtmlFragment operator +(LABEL head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, LABEL tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct LEGEND : IHtmlElementAllowingContent<LEGEND>
        {
            public string TagName => "legend";
            string IHtmlElement.TagStart => "<legend";
            string IHtmlElement.EndTag => "</legend>";
            HtmlAttributes attrs;
            LEGEND IHtmlElement<LEGEND>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new LEGEND { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            LEGEND IHtmlElementAllowingContent<LEGEND>.ReplaceContentWith(HtmlFragment replacementContents) => new LEGEND { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(LEGEND tag) => tag.AsFragment();
            public static HtmlFragment operator +(LEGEND head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, LEGEND tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct LI : IHtmlElementAllowingContent<LI>, IHasAttr_value
        {
            public string TagName => "li";
            string IHtmlElement.TagStart => "<li";
            string IHtmlElement.EndTag => "</li>";
            HtmlAttributes attrs;
            LI IHtmlElement<LI>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new LI { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            LI IHtmlElementAllowingContent<LI>.ReplaceContentWith(HtmlFragment replacementContents) => new LI { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(LI tag) => tag.AsFragment();
            public static HtmlFragment operator +(LI head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, LI tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct LINK : IHtmlElement<LINK>, IHasAttr_href, IHasAttr_crossorigin, IHasAttr_rel, IHasAttr_as, IHasAttr_media, IHasAttr_hreflang, IHasAttr_type, IHasAttr_sizes, IHasAttr_imagesrcset, IHasAttr_imagesizes, IHasAttr_referrerpolicy, IHasAttr_integrity
        {
            public string TagName => "link";
            string IHtmlElement.TagStart => "<link";
            string IHtmlElement.EndTag => "";
            HtmlAttributes attrs;
            LINK IHtmlElement<LINK>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new LINK { attrs = replacementAttributes };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(LINK tag) => tag.AsFragment();
            public static HtmlFragment operator +(LINK head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, LINK tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct MAIN : IHtmlElementAllowingContent<MAIN>
        {
            public string TagName => "main";
            string IHtmlElement.TagStart => "<main";
            string IHtmlElement.EndTag => "</main>";
            HtmlAttributes attrs;
            MAIN IHtmlElement<MAIN>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new MAIN { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            MAIN IHtmlElementAllowingContent<MAIN>.ReplaceContentWith(HtmlFragment replacementContents) => new MAIN { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(MAIN tag) => tag.AsFragment();
            public static HtmlFragment operator +(MAIN head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, MAIN tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct MAP : IHtmlElementAllowingContent<MAP>, IHasAttr_name
        {
            public string TagName => "map";
            string IHtmlElement.TagStart => "<map";
            string IHtmlElement.EndTag => "</map>";
            HtmlAttributes attrs;
            MAP IHtmlElement<MAP>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new MAP { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            MAP IHtmlElementAllowingContent<MAP>.ReplaceContentWith(HtmlFragment replacementContents) => new MAP { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(MAP tag) => tag.AsFragment();
            public static HtmlFragment operator +(MAP head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, MAP tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct MARK : IHtmlElementAllowingContent<MARK>
        {
            public string TagName => "mark";
            string IHtmlElement.TagStart => "<mark";
            string IHtmlElement.EndTag => "</mark>";
            HtmlAttributes attrs;
            MARK IHtmlElement<MARK>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new MARK { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            MARK IHtmlElementAllowingContent<MARK>.ReplaceContentWith(HtmlFragment replacementContents) => new MARK { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(MARK tag) => tag.AsFragment();
            public static HtmlFragment operator +(MARK head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, MARK tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct MENU : IHtmlElementAllowingContent<MENU>
        {
            public string TagName => "menu";
            string IHtmlElement.TagStart => "<menu";
            string IHtmlElement.EndTag => "</menu>";
            HtmlAttributes attrs;
            MENU IHtmlElement<MENU>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new MENU { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            MENU IHtmlElementAllowingContent<MENU>.ReplaceContentWith(HtmlFragment replacementContents) => new MENU { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(MENU tag) => tag.AsFragment();
            public static HtmlFragment operator +(MENU head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, MENU tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct META : IHtmlElement<META>, IHasAttr_name, IHasAttr_http_equiv, IHasAttr_content, IHasAttr_charset
        {
            public string TagName => "meta";
            string IHtmlElement.TagStart => "<meta";
            string IHtmlElement.EndTag => "";
            HtmlAttributes attrs;
            META IHtmlElement<META>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new META { attrs = replacementAttributes };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(META tag) => tag.AsFragment();
            public static HtmlFragment operator +(META head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, META tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct METER : IHtmlElementAllowingContent<METER>, IHasAttr_value, IHasAttr_min, IHasAttr_max, IHasAttr_low, IHasAttr_high, IHasAttr_optimum
        {
            public string TagName => "meter";
            string IHtmlElement.TagStart => "<meter";
            string IHtmlElement.EndTag => "</meter>";
            HtmlAttributes attrs;
            METER IHtmlElement<METER>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new METER { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            METER IHtmlElementAllowingContent<METER>.ReplaceContentWith(HtmlFragment replacementContents) => new METER { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(METER tag) => tag.AsFragment();
            public static HtmlFragment operator +(METER head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, METER tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct NAV : IHtmlElementAllowingContent<NAV>
        {
            public string TagName => "nav";
            string IHtmlElement.TagStart => "<nav";
            string IHtmlElement.EndTag => "</nav>";
            HtmlAttributes attrs;
            NAV IHtmlElement<NAV>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new NAV { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            NAV IHtmlElementAllowingContent<NAV>.ReplaceContentWith(HtmlFragment replacementContents) => new NAV { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(NAV tag) => tag.AsFragment();
            public static HtmlFragment operator +(NAV head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, NAV tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct NOSCRIPT : IHtmlElementAllowingContent<NOSCRIPT>
        {
            public string TagName => "noscript";
            string IHtmlElement.TagStart => "<noscript";
            string IHtmlElement.EndTag => "</noscript>";
            HtmlAttributes attrs;
            NOSCRIPT IHtmlElement<NOSCRIPT>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new NOSCRIPT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            NOSCRIPT IHtmlElementAllowingContent<NOSCRIPT>.ReplaceContentWith(HtmlFragment replacementContents) => new NOSCRIPT { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(NOSCRIPT tag) => tag.AsFragment();
            public static HtmlFragment operator +(NOSCRIPT head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, NOSCRIPT tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct OBJECT : IHtmlElementAllowingContent<OBJECT>, IHasAttr_data, IHasAttr_type, IHasAttr_name, IHasAttr_usemap, IHasAttr_form, IHasAttr_width, IHasAttr_height
        {
            public string TagName => "object";
            string IHtmlElement.TagStart => "<object";
            string IHtmlElement.EndTag => "</object>";
            HtmlAttributes attrs;
            OBJECT IHtmlElement<OBJECT>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new OBJECT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            OBJECT IHtmlElementAllowingContent<OBJECT>.ReplaceContentWith(HtmlFragment replacementContents) => new OBJECT { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(OBJECT tag) => tag.AsFragment();
            public static HtmlFragment operator +(OBJECT head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, OBJECT tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct OL : IHtmlElementAllowingContent<OL>, IHasAttr_reversed, IHasAttr_start, IHasAttr_type
        {
            public string TagName => "ol";
            string IHtmlElement.TagStart => "<ol";
            string IHtmlElement.EndTag => "</ol>";
            HtmlAttributes attrs;
            OL IHtmlElement<OL>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new OL { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            OL IHtmlElementAllowingContent<OL>.ReplaceContentWith(HtmlFragment replacementContents) => new OL { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(OL tag) => tag.AsFragment();
            public static HtmlFragment operator +(OL head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, OL tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct OPTGROUP : IHtmlElementAllowingContent<OPTGROUP>, IHasAttr_disabled, IHasAttr_label
        {
            public string TagName => "optgroup";
            string IHtmlElement.TagStart => "<optgroup";
            string IHtmlElement.EndTag => "</optgroup>";
            HtmlAttributes attrs;
            OPTGROUP IHtmlElement<OPTGROUP>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new OPTGROUP { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            OPTGROUP IHtmlElementAllowingContent<OPTGROUP>.ReplaceContentWith(HtmlFragment replacementContents) => new OPTGROUP { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(OPTGROUP tag) => tag.AsFragment();
            public static HtmlFragment operator +(OPTGROUP head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, OPTGROUP tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct OPTION : IHtmlElementAllowingContent<OPTION>, IHasAttr_disabled, IHasAttr_label, IHasAttr_selected, IHasAttr_value
        {
            public string TagName => "option";
            string IHtmlElement.TagStart => "<option";
            string IHtmlElement.EndTag => "</option>";
            HtmlAttributes attrs;
            OPTION IHtmlElement<OPTION>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new OPTION { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            OPTION IHtmlElementAllowingContent<OPTION>.ReplaceContentWith(HtmlFragment replacementContents) => new OPTION { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(OPTION tag) => tag.AsFragment();
            public static HtmlFragment operator +(OPTION head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, OPTION tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct OUTPUT : IHtmlElementAllowingContent<OUTPUT>, IHasAttr_for, IHasAttr_form, IHasAttr_name
        {
            public string TagName => "output";
            string IHtmlElement.TagStart => "<output";
            string IHtmlElement.EndTag => "</output>";
            HtmlAttributes attrs;
            OUTPUT IHtmlElement<OUTPUT>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new OUTPUT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            OUTPUT IHtmlElementAllowingContent<OUTPUT>.ReplaceContentWith(HtmlFragment replacementContents) => new OUTPUT { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(OUTPUT tag) => tag.AsFragment();
            public static HtmlFragment operator +(OUTPUT head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, OUTPUT tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct P : IHtmlElementAllowingContent<P>
        {
            public string TagName => "p";
            string IHtmlElement.TagStart => "<p";
            string IHtmlElement.EndTag => "</p>";
            HtmlAttributes attrs;
            P IHtmlElement<P>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new P { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            P IHtmlElementAllowingContent<P>.ReplaceContentWith(HtmlFragment replacementContents) => new P { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(P tag) => tag.AsFragment();
            public static HtmlFragment operator +(P head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, P tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct PARAM : IHtmlElement<PARAM>, IHasAttr_name, IHasAttr_value
        {
            public string TagName => "param";
            string IHtmlElement.TagStart => "<param";
            string IHtmlElement.EndTag => "";
            HtmlAttributes attrs;
            PARAM IHtmlElement<PARAM>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new PARAM { attrs = replacementAttributes };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(PARAM tag) => tag.AsFragment();
            public static HtmlFragment operator +(PARAM head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, PARAM tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct PICTURE : IHtmlElementAllowingContent<PICTURE>
        {
            public string TagName => "picture";
            string IHtmlElement.TagStart => "<picture";
            string IHtmlElement.EndTag => "</picture>";
            HtmlAttributes attrs;
            PICTURE IHtmlElement<PICTURE>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new PICTURE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            PICTURE IHtmlElementAllowingContent<PICTURE>.ReplaceContentWith(HtmlFragment replacementContents) => new PICTURE { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(PICTURE tag) => tag.AsFragment();
            public static HtmlFragment operator +(PICTURE head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, PICTURE tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct PRE : IHtmlElementAllowingContent<PRE>
        {
            public string TagName => "pre";
            string IHtmlElement.TagStart => "<pre";
            string IHtmlElement.EndTag => "</pre>";
            HtmlAttributes attrs;
            PRE IHtmlElement<PRE>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new PRE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            PRE IHtmlElementAllowingContent<PRE>.ReplaceContentWith(HtmlFragment replacementContents) => new PRE { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(PRE tag) => tag.AsFragment();
            public static HtmlFragment operator +(PRE head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, PRE tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct PROGRESS : IHtmlElementAllowingContent<PROGRESS>, IHasAttr_value, IHasAttr_max
        {
            public string TagName => "progress";
            string IHtmlElement.TagStart => "<progress";
            string IHtmlElement.EndTag => "</progress>";
            HtmlAttributes attrs;
            PROGRESS IHtmlElement<PROGRESS>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new PROGRESS { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            PROGRESS IHtmlElementAllowingContent<PROGRESS>.ReplaceContentWith(HtmlFragment replacementContents) => new PROGRESS { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(PROGRESS tag) => tag.AsFragment();
            public static HtmlFragment operator +(PROGRESS head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, PROGRESS tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct Q : IHtmlElementAllowingContent<Q>, IHasAttr_cite
        {
            public string TagName => "q";
            string IHtmlElement.TagStart => "<q";
            string IHtmlElement.EndTag => "</q>";
            HtmlAttributes attrs;
            Q IHtmlElement<Q>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new Q { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            Q IHtmlElementAllowingContent<Q>.ReplaceContentWith(HtmlFragment replacementContents) => new Q { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(Q tag) => tag.AsFragment();
            public static HtmlFragment operator +(Q head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, Q tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct RP : IHtmlElementAllowingContent<RP>
        {
            public string TagName => "rp";
            string IHtmlElement.TagStart => "<rp";
            string IHtmlElement.EndTag => "</rp>";
            HtmlAttributes attrs;
            RP IHtmlElement<RP>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new RP { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            RP IHtmlElementAllowingContent<RP>.ReplaceContentWith(HtmlFragment replacementContents) => new RP { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(RP tag) => tag.AsFragment();
            public static HtmlFragment operator +(RP head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, RP tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct RT : IHtmlElementAllowingContent<RT>
        {
            public string TagName => "rt";
            string IHtmlElement.TagStart => "<rt";
            string IHtmlElement.EndTag => "</rt>";
            HtmlAttributes attrs;
            RT IHtmlElement<RT>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new RT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            RT IHtmlElementAllowingContent<RT>.ReplaceContentWith(HtmlFragment replacementContents) => new RT { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(RT tag) => tag.AsFragment();
            public static HtmlFragment operator +(RT head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, RT tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct RUBY : IHtmlElementAllowingContent<RUBY>
        {
            public string TagName => "ruby";
            string IHtmlElement.TagStart => "<ruby";
            string IHtmlElement.EndTag => "</ruby>";
            HtmlAttributes attrs;
            RUBY IHtmlElement<RUBY>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new RUBY { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            RUBY IHtmlElementAllowingContent<RUBY>.ReplaceContentWith(HtmlFragment replacementContents) => new RUBY { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(RUBY tag) => tag.AsFragment();
            public static HtmlFragment operator +(RUBY head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, RUBY tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct S : IHtmlElementAllowingContent<S>
        {
            public string TagName => "s";
            string IHtmlElement.TagStart => "<s";
            string IHtmlElement.EndTag => "</s>";
            HtmlAttributes attrs;
            S IHtmlElement<S>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new S { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            S IHtmlElementAllowingContent<S>.ReplaceContentWith(HtmlFragment replacementContents) => new S { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(S tag) => tag.AsFragment();
            public static HtmlFragment operator +(S head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, S tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct SAMP : IHtmlElementAllowingContent<SAMP>
        {
            public string TagName => "samp";
            string IHtmlElement.TagStart => "<samp";
            string IHtmlElement.EndTag => "</samp>";
            HtmlAttributes attrs;
            SAMP IHtmlElement<SAMP>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new SAMP { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            SAMP IHtmlElementAllowingContent<SAMP>.ReplaceContentWith(HtmlFragment replacementContents) => new SAMP { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(SAMP tag) => tag.AsFragment();
            public static HtmlFragment operator +(SAMP head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, SAMP tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct SCRIPT : IHtmlElementAllowingContent<SCRIPT>, IHasAttr_src, IHasAttr_type, IHasAttr_async, IHasAttr_defer, IHasAttr_crossorigin, IHasAttr_integrity, IHasAttr_referrerpolicy
        {
            public string TagName => "script";
            string IHtmlElement.TagStart => "<script";
            string IHtmlElement.EndTag => "</script>";
            HtmlAttributes attrs;
            SCRIPT IHtmlElement<SCRIPT>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new SCRIPT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            SCRIPT IHtmlElementAllowingContent<SCRIPT>.ReplaceContentWith(HtmlFragment replacementContents) => new SCRIPT { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(SCRIPT tag) => tag.AsFragment();
            public static HtmlFragment operator +(SCRIPT head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, SCRIPT tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct SECTION : IHtmlElementAllowingContent<SECTION>
        {
            public string TagName => "section";
            string IHtmlElement.TagStart => "<section";
            string IHtmlElement.EndTag => "</section>";
            HtmlAttributes attrs;
            SECTION IHtmlElement<SECTION>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new SECTION { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            SECTION IHtmlElementAllowingContent<SECTION>.ReplaceContentWith(HtmlFragment replacementContents) => new SECTION { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(SECTION tag) => tag.AsFragment();
            public static HtmlFragment operator +(SECTION head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, SECTION tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct SELECT : IHtmlElementAllowingContent<SELECT>, IHasAttr_autocomplete, IHasAttr_disabled, IHasAttr_form, IHasAttr_multiple, IHasAttr_name, IHasAttr_required, IHasAttr_size
        {
            public string TagName => "select";
            string IHtmlElement.TagStart => "<select";
            string IHtmlElement.EndTag => "</select>";
            HtmlAttributes attrs;
            SELECT IHtmlElement<SELECT>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new SELECT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            SELECT IHtmlElementAllowingContent<SELECT>.ReplaceContentWith(HtmlFragment replacementContents) => new SELECT { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(SELECT tag) => tag.AsFragment();
            public static HtmlFragment operator +(SELECT head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, SELECT tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct SLOT : IHtmlElementAllowingContent<SLOT>, IHasAttr_name
        {
            public string TagName => "slot";
            string IHtmlElement.TagStart => "<slot";
            string IHtmlElement.EndTag => "</slot>";
            HtmlAttributes attrs;
            SLOT IHtmlElement<SLOT>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new SLOT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            SLOT IHtmlElementAllowingContent<SLOT>.ReplaceContentWith(HtmlFragment replacementContents) => new SLOT { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(SLOT tag) => tag.AsFragment();
            public static HtmlFragment operator +(SLOT head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, SLOT tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct SMALL : IHtmlElementAllowingContent<SMALL>
        {
            public string TagName => "small";
            string IHtmlElement.TagStart => "<small";
            string IHtmlElement.EndTag => "</small>";
            HtmlAttributes attrs;
            SMALL IHtmlElement<SMALL>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new SMALL { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            SMALL IHtmlElementAllowingContent<SMALL>.ReplaceContentWith(HtmlFragment replacementContents) => new SMALL { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(SMALL tag) => tag.AsFragment();
            public static HtmlFragment operator +(SMALL head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, SMALL tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct SOURCE : IHtmlElement<SOURCE>, IHasAttr_src, IHasAttr_type, IHasAttr_srcset, IHasAttr_sizes, IHasAttr_media
        {
            public string TagName => "source";
            string IHtmlElement.TagStart => "<source";
            string IHtmlElement.EndTag => "";
            HtmlAttributes attrs;
            SOURCE IHtmlElement<SOURCE>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new SOURCE { attrs = replacementAttributes };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(SOURCE tag) => tag.AsFragment();
            public static HtmlFragment operator +(SOURCE head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, SOURCE tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct SPAN : IHtmlElementAllowingContent<SPAN>
        {
            public string TagName => "span";
            string IHtmlElement.TagStart => "<span";
            string IHtmlElement.EndTag => "</span>";
            HtmlAttributes attrs;
            SPAN IHtmlElement<SPAN>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new SPAN { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            SPAN IHtmlElementAllowingContent<SPAN>.ReplaceContentWith(HtmlFragment replacementContents) => new SPAN { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(SPAN tag) => tag.AsFragment();
            public static HtmlFragment operator +(SPAN head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, SPAN tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct STRONG : IHtmlElementAllowingContent<STRONG>
        {
            public string TagName => "strong";
            string IHtmlElement.TagStart => "<strong";
            string IHtmlElement.EndTag => "</strong>";
            HtmlAttributes attrs;
            STRONG IHtmlElement<STRONG>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new STRONG { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            STRONG IHtmlElementAllowingContent<STRONG>.ReplaceContentWith(HtmlFragment replacementContents) => new STRONG { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(STRONG tag) => tag.AsFragment();
            public static HtmlFragment operator +(STRONG head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, STRONG tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct STYLE : IHtmlElementAllowingContent<STYLE>, IHasAttr_media
        {
            public string TagName => "style";
            string IHtmlElement.TagStart => "<style";
            string IHtmlElement.EndTag => "</style>";
            HtmlAttributes attrs;
            STYLE IHtmlElement<STYLE>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new STYLE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            STYLE IHtmlElementAllowingContent<STYLE>.ReplaceContentWith(HtmlFragment replacementContents) => new STYLE { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(STYLE tag) => tag.AsFragment();
            public static HtmlFragment operator +(STYLE head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, STYLE tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct SUB : IHtmlElementAllowingContent<SUB>
        {
            public string TagName => "sub";
            string IHtmlElement.TagStart => "<sub";
            string IHtmlElement.EndTag => "</sub>";
            HtmlAttributes attrs;
            SUB IHtmlElement<SUB>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new SUB { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            SUB IHtmlElementAllowingContent<SUB>.ReplaceContentWith(HtmlFragment replacementContents) => new SUB { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(SUB tag) => tag.AsFragment();
            public static HtmlFragment operator +(SUB head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, SUB tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct SUMMARY : IHtmlElementAllowingContent<SUMMARY>
        {
            public string TagName => "summary";
            string IHtmlElement.TagStart => "<summary";
            string IHtmlElement.EndTag => "</summary>";
            HtmlAttributes attrs;
            SUMMARY IHtmlElement<SUMMARY>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new SUMMARY { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            SUMMARY IHtmlElementAllowingContent<SUMMARY>.ReplaceContentWith(HtmlFragment replacementContents) => new SUMMARY { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(SUMMARY tag) => tag.AsFragment();
            public static HtmlFragment operator +(SUMMARY head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, SUMMARY tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct SUP : IHtmlElementAllowingContent<SUP>
        {
            public string TagName => "sup";
            string IHtmlElement.TagStart => "<sup";
            string IHtmlElement.EndTag => "</sup>";
            HtmlAttributes attrs;
            SUP IHtmlElement<SUP>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new SUP { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            SUP IHtmlElementAllowingContent<SUP>.ReplaceContentWith(HtmlFragment replacementContents) => new SUP { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(SUP tag) => tag.AsFragment();
            public static HtmlFragment operator +(SUP head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, SUP tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct TABLE : IHtmlElementAllowingContent<TABLE>
        {
            public string TagName => "table";
            string IHtmlElement.TagStart => "<table";
            string IHtmlElement.EndTag => "</table>";
            HtmlAttributes attrs;
            TABLE IHtmlElement<TABLE>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new TABLE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            TABLE IHtmlElementAllowingContent<TABLE>.ReplaceContentWith(HtmlFragment replacementContents) => new TABLE { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(TABLE tag) => tag.AsFragment();
            public static HtmlFragment operator +(TABLE head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, TABLE tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct TBODY : IHtmlElementAllowingContent<TBODY>
        {
            public string TagName => "tbody";
            string IHtmlElement.TagStart => "<tbody";
            string IHtmlElement.EndTag => "</tbody>";
            HtmlAttributes attrs;
            TBODY IHtmlElement<TBODY>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new TBODY { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            TBODY IHtmlElementAllowingContent<TBODY>.ReplaceContentWith(HtmlFragment replacementContents) => new TBODY { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(TBODY tag) => tag.AsFragment();
            public static HtmlFragment operator +(TBODY head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, TBODY tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct TD : IHtmlElementAllowingContent<TD>, IHasAttr_colspan, IHasAttr_rowspan, IHasAttr_headers
        {
            public string TagName => "td";
            string IHtmlElement.TagStart => "<td";
            string IHtmlElement.EndTag => "</td>";
            HtmlAttributes attrs;
            TD IHtmlElement<TD>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new TD { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            TD IHtmlElementAllowingContent<TD>.ReplaceContentWith(HtmlFragment replacementContents) => new TD { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(TD tag) => tag.AsFragment();
            public static HtmlFragment operator +(TD head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, TD tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct TEMPLATE : IHtmlElementAllowingContent<TEMPLATE>
        {
            public string TagName => "template";
            string IHtmlElement.TagStart => "<template";
            string IHtmlElement.EndTag => "</template>";
            HtmlAttributes attrs;
            TEMPLATE IHtmlElement<TEMPLATE>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new TEMPLATE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            TEMPLATE IHtmlElementAllowingContent<TEMPLATE>.ReplaceContentWith(HtmlFragment replacementContents) => new TEMPLATE { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(TEMPLATE tag) => tag.AsFragment();
            public static HtmlFragment operator +(TEMPLATE head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, TEMPLATE tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct TEXTAREA : IHtmlElementAllowingContent<TEXTAREA>, IHasAttr_cols, IHasAttr_dirname, IHasAttr_disabled, IHasAttr_form, IHasAttr_maxlength, IHasAttr_minlength, IHasAttr_name, IHasAttr_placeholder, IHasAttr_readonly, IHasAttr_required, IHasAttr_rows, IHasAttr_wrap
        {
            public string TagName => "textarea";
            string IHtmlElement.TagStart => "<textarea";
            string IHtmlElement.EndTag => "</textarea>";
            HtmlAttributes attrs;
            TEXTAREA IHtmlElement<TEXTAREA>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new TEXTAREA { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            TEXTAREA IHtmlElementAllowingContent<TEXTAREA>.ReplaceContentWith(HtmlFragment replacementContents) => new TEXTAREA { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(TEXTAREA tag) => tag.AsFragment();
            public static HtmlFragment operator +(TEXTAREA head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, TEXTAREA tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct TFOOT : IHtmlElementAllowingContent<TFOOT>
        {
            public string TagName => "tfoot";
            string IHtmlElement.TagStart => "<tfoot";
            string IHtmlElement.EndTag => "</tfoot>";
            HtmlAttributes attrs;
            TFOOT IHtmlElement<TFOOT>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new TFOOT { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            TFOOT IHtmlElementAllowingContent<TFOOT>.ReplaceContentWith(HtmlFragment replacementContents) => new TFOOT { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;

            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(TFOOT tag) => tag.AsFragment();
            public static HtmlFragment operator +(TFOOT head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, TFOOT tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct TH : IHtmlElementAllowingContent<TH>, IHasAttr_colspan, IHasAttr_rowspan, IHasAttr_headers, IHasAttr_scope, IHasAttr_abbr
        {
            public string TagName => "th";
            string IHtmlElement.TagStart => "<th";
            string IHtmlElement.EndTag => "</th>";
            HtmlAttributes attrs;
            TH IHtmlElement<TH>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new TH { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            TH IHtmlElementAllowingContent<TH>.ReplaceContentWith(HtmlFragment replacementContents) => new TH { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(TH tag) => tag.AsFragment();
            public static HtmlFragment operator +(TH head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, TH tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct THEAD : IHtmlElementAllowingContent<THEAD>
        {
            public string TagName => "thead";
            string IHtmlElement.TagStart => "<thead";
            string IHtmlElement.EndTag => "</thead>";
            HtmlAttributes attrs;
            THEAD IHtmlElement<THEAD>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new THEAD { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            THEAD IHtmlElementAllowingContent<THEAD>.ReplaceContentWith(HtmlFragment replacementContents) => new THEAD { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(THEAD tag) => tag.AsFragment();
            public static HtmlFragment operator +(THEAD head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, THEAD tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct TIME : IHtmlElementAllowingContent<TIME>, IHasAttr_datetime
        {
            public string TagName => "time";
            string IHtmlElement.TagStart => "<time";
            string IHtmlElement.EndTag => "</time>";
            HtmlAttributes attrs;
            TIME IHtmlElement<TIME>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new TIME { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            TIME IHtmlElementAllowingContent<TIME>.ReplaceContentWith(HtmlFragment replacementContents) => new TIME { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(TIME tag) => tag.AsFragment();
            public static HtmlFragment operator +(TIME head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, TIME tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct TITLE : IHtmlElementAllowingContent<TITLE>
        {
            public string TagName => "title";
            string IHtmlElement.TagStart => "<title";
            string IHtmlElement.EndTag => "</title>";
            HtmlAttributes attrs;
            TITLE IHtmlElement<TITLE>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new TITLE { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            TITLE IHtmlElementAllowingContent<TITLE>.ReplaceContentWith(HtmlFragment replacementContents) => new TITLE { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(TITLE tag) => tag.AsFragment();
            public static HtmlFragment operator +(TITLE head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, TITLE tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct TR : IHtmlElementAllowingContent<TR>
        {
            public string TagName => "tr";
            string IHtmlElement.TagStart => "<tr";
            string IHtmlElement.EndTag => "</tr>";
            HtmlAttributes attrs;
            TR IHtmlElement<TR>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new TR { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            TR IHtmlElementAllowingContent<TR>.ReplaceContentWith(HtmlFragment replacementContents) => new TR { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(TR tag) => tag.AsFragment();
            public static HtmlFragment operator +(TR head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, TR tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct TRACK : IHtmlElement<TRACK>, IHasAttr_default, IHasAttr_kind, IHasAttr_label, IHasAttr_src, IHasAttr_srclang
        {
            public string TagName => "track";
            string IHtmlElement.TagStart => "<track";
            string IHtmlElement.EndTag => "";
            HtmlAttributes attrs;
            TRACK IHtmlElement<TRACK>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new TRACK { attrs = replacementAttributes };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(TRACK tag) => tag.AsFragment();
            public static HtmlFragment operator +(TRACK head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, TRACK tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct U : IHtmlElementAllowingContent<U>
        {
            public string TagName => "u";
            string IHtmlElement.TagStart => "<u";
            string IHtmlElement.EndTag => "</u>";
            HtmlAttributes attrs;
            U IHtmlElement<U>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new U { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            U IHtmlElementAllowingContent<U>.ReplaceContentWith(HtmlFragment replacementContents) => new U { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(U tag) => tag.AsFragment();
            public static HtmlFragment operator +(U head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, U tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct UL : IHtmlElementAllowingContent<UL>
        {
            public string TagName => "ul";
            string IHtmlElement.TagStart => "<ul";
            string IHtmlElement.EndTag => "</ul>";
            HtmlAttributes attrs;
            UL IHtmlElement<UL>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new UL { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            UL IHtmlElementAllowingContent<UL>.ReplaceContentWith(HtmlFragment replacementContents) => new UL { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(UL tag) => tag.AsFragment();
            public static HtmlFragment operator +(UL head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, UL tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct VAR : IHtmlElementAllowingContent<VAR>
        {
            public string TagName => "var";
            string IHtmlElement.TagStart => "<var";
            string IHtmlElement.EndTag => "</var>";
            HtmlAttributes attrs;
            VAR IHtmlElement<VAR>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new VAR { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            VAR IHtmlElementAllowingContent<VAR>.ReplaceContentWith(HtmlFragment replacementContents) => new VAR { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(VAR tag) => tag.AsFragment();
            public static HtmlFragment operator +(VAR head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, VAR tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct VIDEO : IHtmlElementAllowingContent<VIDEO>, IHasAttr_src, IHasAttr_crossorigin, IHasAttr_poster, IHasAttr_preload, IHasAttr_autoplay, IHasAttr_playsinline, IHasAttr_loop, IHasAttr_muted, IHasAttr_controls, IHasAttr_width, IHasAttr_height
        {
            public string TagName => "video";
            string IHtmlElement.TagStart => "<video";
            string IHtmlElement.EndTag => "</video>";
            HtmlAttributes attrs;
            VIDEO IHtmlElement<VIDEO>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new VIDEO { attrs = replacementAttributes, children = children };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            HtmlFragment children;
            VIDEO IHtmlElementAllowingContent<VIDEO>.ReplaceContentWith(HtmlFragment replacementContents) => new VIDEO { attrs = attrs, children = replacementContents };
            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(VIDEO tag) => tag.AsFragment();
            public static HtmlFragment operator +(VIDEO head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, VIDEO tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
        }
        public struct WBR : IHtmlElement<WBR>
        {
            public string TagName => "wbr";
            string IHtmlElement.TagStart => "<wbr";
            string IHtmlElement.EndTag => "";
            HtmlAttributes attrs;
            WBR IHtmlElement<WBR>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new WBR { attrs = replacementAttributes };
            HtmlAttributes IHtmlElement.Attributes => attrs;
            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
            public static implicit operator HtmlFragment(WBR tag) => tag.AsFragment();
            public static HtmlFragment operator +(WBR head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
            public static HtmlFragment operator +(string head, WBR tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
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

        ///<summary>Container for the dominant contents of the document. See: <a href="https://html.spec.whatwg.org/#the-main-element">https://html.spec.whatwg.org/#the-main-element</a><br /></summary>
        public static readonly HtmlTagKinds.MAIN _main = new HtmlTagKinds.MAIN();

        ///<summary>Image map. See: <a href="https://html.spec.whatwg.org/#the-map-element">https://html.spec.whatwg.org/#the-map-element</a><br /></summary>
        public static readonly HtmlTagKinds.MAP _map = new HtmlTagKinds.MAP();

        ///<summary>Highlight. See: <a href="https://html.spec.whatwg.org/#the-mark-element">https://html.spec.whatwg.org/#the-mark-element</a><br /></summary>
        public static readonly HtmlTagKinds.MARK _mark = new HtmlTagKinds.MARK();

        ///<summary>Menu of commands. See: <a href="https://html.spec.whatwg.org/#the-menu-element">https://html.spec.whatwg.org/#the-menu-element</a><br /></summary>
        public static readonly HtmlTagKinds.MENU _menu = new HtmlTagKinds.MENU();

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

        ///<summary>Unarticulated annotation. See: <a href="https://html.spec.whatwg.org/#the-u-element">https://html.spec.whatwg.org/#the-u-element</a><br /></summary>
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
        public interface IHasAttr_allow { }
        public interface IHasAttr_allowfullscreen { }
        public interface IHasAttr_allowpaymentrequest { }
        public interface IHasAttr_srcset { }
        public interface IHasAttr_sizes { }
        public interface IHasAttr_usemap { }
        public interface IHasAttr_ismap { }
        public interface IHasAttr_decoding { }
        public interface IHasAttr_loading { }
        public interface IHasAttr_accept { }
        public interface IHasAttr_checked { }
        public interface IHasAttr_dirname { }
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
        public interface IHasAttr_imagesrcset { }
        public interface IHasAttr_imagesizes { }
        public interface IHasAttr_integrity { }
        public interface IHasAttr_http_equiv { }
        public interface IHasAttr_content { }
        public interface IHasAttr_charset { }
        public interface IHasAttr_low { }
        public interface IHasAttr_high { }
        public interface IHasAttr_optimum { }
        public interface IHasAttr_data { }
        public interface IHasAttr_reversed { }
        public interface IHasAttr_start { }
        public interface IHasAttr_label { }
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
        public interface IHasAttr_default { }
        public interface IHasAttr_kind { }
        public interface IHasAttr_srclang { }
        public interface IHasAttr_poster { }
        public interface IHasAttr_playsinline { }
    }

    public static class AttributeConstructionMethods
    {
        public static THtmlTag _accesskey<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("accesskey", attrValue);
        public static THtmlTag _autocapitalize<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("autocapitalize", attrValue);
        public static THtmlTag _autofocus<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("autofocus", attrValue);
        public static THtmlTag _class<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("class", attrValue);
        public static THtmlTag _contenteditable<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("contenteditable", attrValue);
        public static THtmlTag _dir<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("dir", attrValue);
        public static THtmlTag _draggable<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("draggable", attrValue);
        public static THtmlTag _enterkeyhint<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("enterkeyhint", attrValue);
        public static THtmlTag _hidden<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("hidden", attrValue);
        public static THtmlTag _id<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("id", attrValue);
        public static THtmlTag _inputmode<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("inputmode", attrValue);
        public static THtmlTag _is<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("is", attrValue);
        public static THtmlTag _itemid<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("itemid", attrValue);
        public static THtmlTag _itemprop<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("itemprop", attrValue);
        public static THtmlTag _itemref<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("itemref", attrValue);
        public static THtmlTag _itemscope<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("itemscope", attrValue);
        public static THtmlTag _itemtype<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("itemtype", attrValue);
        public static THtmlTag _lang<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("lang", attrValue);
        public static THtmlTag _nonce<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("nonce", attrValue);
        public static THtmlTag _slot<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("slot", attrValue);
        public static THtmlTag _spellcheck<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("spellcheck", attrValue);
        public static THtmlTag _style<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("style", attrValue);
        public static THtmlTag _tabindex<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("tabindex", attrValue);
        public static THtmlTag _title<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("title", attrValue);
        public static THtmlTag _translate<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("translate", attrValue);
        public static THtmlTag _href<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_href, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("href", attrValue);
        public static THtmlTag _target<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_target, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("target", attrValue);
        public static THtmlTag _download<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_download, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("download", attrValue);
        public static THtmlTag _ping<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_ping, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("ping", attrValue);
        public static THtmlTag _rel<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_rel, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("rel", attrValue);
        public static THtmlTag _hreflang<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_hreflang, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("hreflang", attrValue);
        public static THtmlTag _type<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_type, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("type", attrValue);
        public static THtmlTag _referrerpolicy<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_referrerpolicy, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("referrerpolicy", attrValue);
        public static THtmlTag _alt<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_alt, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("alt", attrValue);
        public static THtmlTag _coords<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_coords, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("coords", attrValue);
        public static THtmlTag _shape<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_shape, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("shape", attrValue);
        public static THtmlTag _src<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_src, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("src", attrValue);
        public static THtmlTag _crossorigin<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_crossorigin, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("crossorigin", attrValue);
        public static THtmlTag _preload<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_preload, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("preload", attrValue);
        public static THtmlTag _autoplay<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_autoplay, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("autoplay", attrValue);
        public static THtmlTag _loop<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_loop, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("loop", attrValue);
        public static THtmlTag _muted<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_muted, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("muted", attrValue);
        public static THtmlTag _controls<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_controls, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("controls", attrValue);
        public static THtmlTag _cite<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_cite, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("cite", attrValue);
        public static THtmlTag _onafterprint<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onafterprint, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onafterprint", attrValue);
        public static THtmlTag _onbeforeprint<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onbeforeprint, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onbeforeprint", attrValue);
        public static THtmlTag _onbeforeunload<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onbeforeunload, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onbeforeunload", attrValue);
        public static THtmlTag _onhashchange<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onhashchange, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onhashchange", attrValue);
        public static THtmlTag _onlanguagechange<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onlanguagechange, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onlanguagechange", attrValue);
        public static THtmlTag _onmessage<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onmessage, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onmessage", attrValue);
        public static THtmlTag _onmessageerror<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onmessageerror, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onmessageerror", attrValue);
        public static THtmlTag _onoffline<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onoffline, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onoffline", attrValue);
        public static THtmlTag _ononline<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_ononline, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("ononline", attrValue);
        public static THtmlTag _onpagehide<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onpagehide, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onpagehide", attrValue);
        public static THtmlTag _onpageshow<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onpageshow, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onpageshow", attrValue);
        public static THtmlTag _onpopstate<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onpopstate, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onpopstate", attrValue);
        public static THtmlTag _onrejectionhandled<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onrejectionhandled, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onrejectionhandled", attrValue);
        public static THtmlTag _onstorage<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onstorage, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onstorage", attrValue);
        public static THtmlTag _onunhandledrejection<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onunhandledrejection, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onunhandledrejection", attrValue);
        public static THtmlTag _onunload<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_onunload, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("onunload", attrValue);
        public static THtmlTag _disabled<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_disabled, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("disabled", attrValue);
        public static THtmlTag _form<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_form, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("form", attrValue);
        public static THtmlTag _formaction<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_formaction, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("formaction", attrValue);
        public static THtmlTag _formenctype<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_formenctype, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("formenctype", attrValue);
        public static THtmlTag _formmethod<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_formmethod, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("formmethod", attrValue);
        public static THtmlTag _formnovalidate<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_formnovalidate, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("formnovalidate", attrValue);
        public static THtmlTag _formtarget<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_formtarget, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("formtarget", attrValue);
        public static THtmlTag _name<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_name, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("name", attrValue);
        public static THtmlTag _value<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_value, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("value", attrValue);
        public static THtmlTag _width<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_width, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("width", attrValue);
        public static THtmlTag _height<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_height, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("height", attrValue);
        public static THtmlTag _span<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_span, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("span", attrValue);
        public static THtmlTag _datetime<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_datetime, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("datetime", attrValue);
        public static THtmlTag _open<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_open, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("open", attrValue);
        public static THtmlTag _accept_charset<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_accept_charset, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("accept-charset", attrValue);
        public static THtmlTag _action<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_action, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("action", attrValue);
        public static THtmlTag _autocomplete<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_autocomplete, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("autocomplete", attrValue);
        public static THtmlTag _enctype<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_enctype, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("enctype", attrValue);
        public static THtmlTag _method<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_method, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("method", attrValue);
        public static THtmlTag _novalidate<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_novalidate, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("novalidate", attrValue);
        public static THtmlTag _manifest<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_manifest, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("manifest", attrValue);
        public static THtmlTag _srcdoc<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_srcdoc, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("srcdoc", attrValue);
        public static THtmlTag _sandbox<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_sandbox, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("sandbox", attrValue);
        public static THtmlTag _allow<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_allow, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("allow", attrValue);
        public static THtmlTag _allowfullscreen<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_allowfullscreen, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("allowfullscreen", attrValue);
        public static THtmlTag _allowpaymentrequest<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_allowpaymentrequest, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("allowpaymentrequest", attrValue);
        public static THtmlTag _srcset<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_srcset, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("srcset", attrValue);
        public static THtmlTag _sizes<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_sizes, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("sizes", attrValue);
        public static THtmlTag _usemap<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_usemap, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("usemap", attrValue);
        public static THtmlTag _ismap<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_ismap, IHtmlElement<THtmlTag>

            => htmlTagExpr.Attribute("ismap", attrValue);
        public static THtmlTag _decoding<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_decoding, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("decoding", attrValue);
        public static THtmlTag _loading<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_loading, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("loading", attrValue);
        public static THtmlTag _accept<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_accept, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("accept", attrValue);
        public static THtmlTag _checked<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_checked, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("checked", attrValue);
        public static THtmlTag _dirname<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_dirname, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("dirname", attrValue);
        public static THtmlTag _list<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_list, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("list", attrValue);
        public static THtmlTag _max<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_max, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("max", attrValue);
        public static THtmlTag _maxlength<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_maxlength, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("maxlength", attrValue);
        public static THtmlTag _min<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_min, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("min", attrValue);
        public static THtmlTag _minlength<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_minlength, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("minlength", attrValue);
        public static THtmlTag _multiple<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_multiple, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("multiple", attrValue);
        public static THtmlTag _pattern<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_pattern, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("pattern", attrValue);
        public static THtmlTag _placeholder<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_placeholder, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("placeholder", attrValue);
        public static THtmlTag _readonly<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_readonly, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("readonly", attrValue);
        public static THtmlTag _required<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_required, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("required", attrValue);
        public static THtmlTag _size<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_size, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("size", attrValue);
        public static THtmlTag _step<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_step, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("step", attrValue);
        public static THtmlTag _for<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_for, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("for", attrValue);
        public static THtmlTag _as<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_as, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("as", attrValue);
        public static THtmlTag _media<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_media, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("media", attrValue);
        public static THtmlTag _imagesrcset<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_imagesrcset, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("imagesrcset", attrValue);
        public static THtmlTag _imagesizes<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_imagesizes, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("imagesizes", attrValue);
        public static THtmlTag _integrity<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_integrity, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("integrity", attrValue);
        public static THtmlTag _http_equiv<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_http_equiv, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("http-equiv", attrValue);
        public static THtmlTag _content<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_content, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("content", attrValue);
        public static THtmlTag _charset<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_charset, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("charset", attrValue);
        public static THtmlTag _low<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_low, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("low", attrValue);
        public static THtmlTag _high<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_high, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("high", attrValue);
        public static THtmlTag _optimum<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_optimum, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("optimum", attrValue);
        public static THtmlTag _data<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_data, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("data", attrValue);
        public static THtmlTag _reversed<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_reversed, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("reversed", attrValue);
        public static THtmlTag _start<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_start, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("start", attrValue);
        public static THtmlTag _label<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_label, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("label", attrValue);
        public static THtmlTag _selected<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_selected, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("selected", attrValue);
        public static THtmlTag _async<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_async, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("async", attrValue);
        public static THtmlTag _defer<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_defer, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("defer", attrValue);
        public static THtmlTag _colspan<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_colspan, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("colspan", attrValue);
        public static THtmlTag _rowspan<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_rowspan, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("rowspan", attrValue);
        public static THtmlTag _headers<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_headers, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("headers", attrValue);
        public static THtmlTag _cols<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_cols, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("cols", attrValue);
        public static THtmlTag _rows<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_rows, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("rows", attrValue);
        public static THtmlTag _wrap<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_wrap, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("wrap", attrValue);
        public static THtmlTag _scope<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_scope, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("scope", attrValue);
        public static THtmlTag _abbr<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_abbr, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("abbr", attrValue);
        public static THtmlTag _default<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_default, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("default", attrValue);
        public static THtmlTag _kind<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_kind, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("kind", attrValue);
        public static THtmlTag _srclang<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_srclang, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("srclang", attrValue);
        public static THtmlTag _poster<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_poster, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("poster", attrValue);
        public static THtmlTag _playsinline<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
            where THtmlTag : struct, IHasAttr_playsinline, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute("playsinline", attrValue);
    }
}
