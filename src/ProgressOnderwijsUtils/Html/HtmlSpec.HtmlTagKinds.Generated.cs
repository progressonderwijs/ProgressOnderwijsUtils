#nullable enable
using ProgressOnderwijsUtils.Html.AttributeNameInterfaces;

namespace ProgressOnderwijsUtils.Html;

public static class HtmlTagKinds
{
    public struct A : IHtmlElementAllowingContent<A>, IHasAttr_href, IHasAttr_target, IHasAttr_download, IHasAttr_ping, IHasAttr_rel, IHasAttr_hreflang, IHasAttr_type, IHasAttr_referrerpolicy
    {
        public string TagName => "a";
        string IHtmlElement.TagStart => "<a";
        string IHtmlElement.EndTag => "</a>";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<a"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</a>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<abbr"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</abbr>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<address"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</address>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<area"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => ""u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<article"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</article>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<aside"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</aside>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<audio"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</audio>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<b"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</b>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<base"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => ""u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<bdi"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</bdi>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<bdo"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</bdo>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<blockquote"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</blockquote>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<body"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</body>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<br"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => ""u8;
        HtmlAttributes attrs;
        BR IHtmlElement<BR>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new BR { attrs = replacementAttributes };
        HtmlAttributes IHtmlElement.Attributes => attrs;
        IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
        [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
        public static implicit operator HtmlFragment(BR tag) => tag.AsFragment();
        public static HtmlFragment operator +(BR head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
        public static HtmlFragment operator +(string head, BR tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
    }
    public struct BUTTON : IHtmlElementAllowingContent<BUTTON>, IHasAttr_disabled, IHasAttr_form, IHasAttr_formaction, IHasAttr_formenctype, IHasAttr_formmethod, IHasAttr_formnovalidate, IHasAttr_formtarget, IHasAttr_name, IHasAttr_popovertarget, IHasAttr_popovertargetaction, IHasAttr_type, IHasAttr_value
    {
        public string TagName => "button";
        string IHtmlElement.TagStart => "<button";
        string IHtmlElement.EndTag => "</button>";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<button"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</button>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<canvas"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</canvas>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<caption"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</caption>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<cite"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</cite>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<code"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</code>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<col"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => ""u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<colgroup"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</colgroup>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<data"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</data>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<datalist"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</datalist>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<dd"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</dd>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<del"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</del>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<details"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</details>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<dfn"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</dfn>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<dialog"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</dialog>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<div"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</div>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<dl"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</dl>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<dt"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</dt>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<em"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</em>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<embed"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => ""u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<fieldset"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</fieldset>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<figcaption"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</figcaption>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<figure"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</figure>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<footer"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</footer>"u8;
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
    public struct FORM : IHtmlElementAllowingContent<FORM>, IHasAttr_accept_charset, IHasAttr_action, IHasAttr_autocomplete, IHasAttr_enctype, IHasAttr_method, IHasAttr_name, IHasAttr_novalidate, IHasAttr_rel, IHasAttr_target
    {
        public string TagName => "form";
        string IHtmlElement.TagStart => "<form";
        string IHtmlElement.EndTag => "</form>";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<form"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</form>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<h1"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</h1>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<h2"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</h2>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<h3"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</h3>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<h4"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</h4>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<h5"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</h5>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<h6"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</h6>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<head"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</head>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<header"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</header>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<hgroup"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</hgroup>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<hr"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => ""u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<html"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</html>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<i"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</i>"u8;
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
    public struct IFRAME : IHtmlElementAllowingContent<IFRAME>, IHasAttr_src, IHasAttr_srcdoc, IHasAttr_name, IHasAttr_sandbox, IHasAttr_allow, IHasAttr_allowfullscreen, IHasAttr_width, IHasAttr_height, IHasAttr_referrerpolicy, IHasAttr_loading
    {
        public string TagName => "iframe";
        string IHtmlElement.TagStart => "<iframe";
        string IHtmlElement.EndTag => "</iframe>";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<iframe"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</iframe>"u8;
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
    public struct IMG : IHtmlElement<IMG>, IHasAttr_alt, IHasAttr_src, IHasAttr_srcset, IHasAttr_sizes, IHasAttr_crossorigin, IHasAttr_usemap, IHasAttr_ismap, IHasAttr_width, IHasAttr_height, IHasAttr_referrerpolicy, IHasAttr_decoding, IHasAttr_loading, IHasAttr_fetchpriority
    {
        public string TagName => "img";
        string IHtmlElement.TagStart => "<img";
        string IHtmlElement.EndTag => "";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<img"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => ""u8;
        HtmlAttributes attrs;
        IMG IHtmlElement<IMG>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new IMG { attrs = replacementAttributes };
        HtmlAttributes IHtmlElement.Attributes => attrs;
        IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
        [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
        public static implicit operator HtmlFragment(IMG tag) => tag.AsFragment();
        public static HtmlFragment operator +(IMG head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
        public static HtmlFragment operator +(string head, IMG tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
    }
    public struct INPUT : IHtmlElement<INPUT>, IHasAttr_accept, IHasAttr_alt, IHasAttr_autocomplete, IHasAttr_checked, IHasAttr_dirname, IHasAttr_disabled, IHasAttr_form, IHasAttr_formaction, IHasAttr_formenctype, IHasAttr_formmethod, IHasAttr_formnovalidate, IHasAttr_formtarget, IHasAttr_height, IHasAttr_list, IHasAttr_max, IHasAttr_maxlength, IHasAttr_min, IHasAttr_minlength, IHasAttr_multiple, IHasAttr_name, IHasAttr_pattern, IHasAttr_placeholder, IHasAttr_popovertarget, IHasAttr_popovertargetaction, IHasAttr_readonly, IHasAttr_required, IHasAttr_size, IHasAttr_src, IHasAttr_step, IHasAttr_type, IHasAttr_value, IHasAttr_width
    {
        public string TagName => "input";
        string IHtmlElement.TagStart => "<input";
        string IHtmlElement.EndTag => "";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<input"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => ""u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<ins"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</ins>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<kbd"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</kbd>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<label"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</label>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<legend"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</legend>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<li"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</li>"u8;
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
    public struct LINK : IHtmlElement<LINK>, IHasAttr_href, IHasAttr_crossorigin, IHasAttr_rel, IHasAttr_as, IHasAttr_media, IHasAttr_hreflang, IHasAttr_type, IHasAttr_sizes, IHasAttr_imagesrcset, IHasAttr_imagesizes, IHasAttr_referrerpolicy, IHasAttr_integrity, IHasAttr_blocking, IHasAttr_color, IHasAttr_disabled, IHasAttr_fetchpriority
    {
        public string TagName => "link";
        string IHtmlElement.TagStart => "<link";
        string IHtmlElement.EndTag => "";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<link"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => ""u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<main"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</main>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<map"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</map>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<mark"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</mark>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<menu"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</menu>"u8;
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
    public struct META : IHtmlElement<META>, IHasAttr_name, IHasAttr_http_equiv, IHasAttr_content, IHasAttr_charset, IHasAttr_media
    {
        public string TagName => "meta";
        string IHtmlElement.TagStart => "<meta";
        string IHtmlElement.EndTag => "";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<meta"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => ""u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<meter"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</meter>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<nav"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</nav>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<noscript"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</noscript>"u8;
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
    public struct OBJECT : IHtmlElementAllowingContent<OBJECT>, IHasAttr_data, IHasAttr_type, IHasAttr_name, IHasAttr_form, IHasAttr_width, IHasAttr_height
    {
        public string TagName => "object";
        string IHtmlElement.TagStart => "<object";
        string IHtmlElement.EndTag => "</object>";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<object"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</object>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<ol"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</ol>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<optgroup"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</optgroup>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<option"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</option>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<output"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</output>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<p"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</p>"u8;
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
    public struct PICTURE : IHtmlElementAllowingContent<PICTURE>
    {
        public string TagName => "picture";
        string IHtmlElement.TagStart => "<picture";
        string IHtmlElement.EndTag => "</picture>";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<picture"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</picture>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<pre"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</pre>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<progress"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</progress>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<q"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</q>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<rp"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</rp>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<rt"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</rt>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<ruby"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</ruby>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<s"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</s>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<samp"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</samp>"u8;
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
    public struct SCRIPT : IHtmlElementAllowingContent<SCRIPT>, IHasAttr_src, IHasAttr_type, IHasAttr_nomodule, IHasAttr_async, IHasAttr_defer, IHasAttr_crossorigin, IHasAttr_integrity, IHasAttr_referrerpolicy, IHasAttr_blocking, IHasAttr_fetchpriority
    {
        public string TagName => "script";
        string IHtmlElement.TagStart => "<script";
        string IHtmlElement.EndTag => "</script>";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<script"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</script>"u8;
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
    public struct SEARCH : IHtmlElementAllowingContent<SEARCH>
    {
        public string TagName => "search";
        string IHtmlElement.TagStart => "<search";
        string IHtmlElement.EndTag => "</search>";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<search"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</search>"u8;
        HtmlAttributes attrs;
        SEARCH IHtmlElement<SEARCH>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new SEARCH { attrs = replacementAttributes, children = children };
        HtmlAttributes IHtmlElement.Attributes => attrs;
        HtmlFragment children;
        SEARCH IHtmlElementAllowingContent<SEARCH>.ReplaceContentWith(HtmlFragment replacementContents) => new SEARCH { attrs = attrs, children = replacementContents };
        HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
        IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
        [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
        public static implicit operator HtmlFragment(SEARCH tag) => tag.AsFragment();
        public static HtmlFragment operator +(SEARCH head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
        public static HtmlFragment operator +(string head, SEARCH tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
    }
    public struct SECTION : IHtmlElementAllowingContent<SECTION>
    {
        public string TagName => "section";
        string IHtmlElement.TagStart => "<section";
        string IHtmlElement.EndTag => "</section>";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<section"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</section>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<select"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</select>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<slot"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</slot>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<small"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</small>"u8;
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
    public struct SOURCE : IHtmlElement<SOURCE>, IHasAttr_type, IHasAttr_media, IHasAttr_src, IHasAttr_srcset, IHasAttr_sizes, IHasAttr_width, IHasAttr_height
    {
        public string TagName => "source";
        string IHtmlElement.TagStart => "<source";
        string IHtmlElement.EndTag => "";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<source"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => ""u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<span"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</span>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<strong"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</strong>"u8;
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
    public struct STYLE : IHtmlElementAllowingContent<STYLE>, IHasAttr_media, IHasAttr_blocking
    {
        public string TagName => "style";
        string IHtmlElement.TagStart => "<style";
        string IHtmlElement.EndTag => "</style>";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<style"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</style>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<sub"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</sub>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<summary"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</summary>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<sup"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</sup>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<table"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</table>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<tbody"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</tbody>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<td"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</td>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<template"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</template>"u8;
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
    public struct TEXTAREA : IHtmlElementAllowingContent<TEXTAREA>, IHasAttr_autocomplete, IHasAttr_cols, IHasAttr_dirname, IHasAttr_disabled, IHasAttr_form, IHasAttr_maxlength, IHasAttr_minlength, IHasAttr_name, IHasAttr_placeholder, IHasAttr_readonly, IHasAttr_required, IHasAttr_rows, IHasAttr_wrap
    {
        public string TagName => "textarea";
        string IHtmlElement.TagStart => "<textarea";
        string IHtmlElement.EndTag => "</textarea>";
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<textarea"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</textarea>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<tfoot"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</tfoot>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<th"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</th>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<thead"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</thead>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<time"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</time>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<title"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</title>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<tr"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</tr>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<track"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => ""u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<u"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</u>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<ul"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</ul>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<var"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</var>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<video"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => "</video>"u8;
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
        ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<wbr"u8;
        ReadOnlySpan<byte> IHtmlElement.EndTagUtf8  => ""u8;
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
