namespace ProgressOnderwijsUtils.Html
{
    using AttributeNameInterfaces;

    public static class TagNames
    {
        public struct A : IHtmlTag, IHasAttr_href, IHasAttr_target, IHasAttr_download, IHasAttr_ping, IHasAttr_rel, IHasAttr_hreflang, IHasAttr_type, IHasAttr_referrerpolicy, IHtmlTagAllowingContent
        {
            public string TagName => "a";
            string IHtmlTag.TagStart => "<a";
            string IHtmlTag.EndTag => "</a>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct ABBR : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "abbr";
            string IHtmlTag.TagStart => "<abbr";
            string IHtmlTag.EndTag => "</abbr>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct ADDRESS : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "address";
            string IHtmlTag.TagStart => "<address";
            string IHtmlTag.EndTag => "</address>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct AREA : IHtmlTag, IHasAttr_alt, IHasAttr_coords, IHasAttr_shape, IHasAttr_href, IHasAttr_target, IHasAttr_download, IHasAttr_ping, IHasAttr_rel, IHasAttr_referrerpolicy
        {
            public string TagName => "area";
            string IHtmlTag.TagStart => "<area";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct ARTICLE : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "article";
            string IHtmlTag.TagStart => "<article";
            string IHtmlTag.EndTag => "</article>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct ASIDE : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "aside";
            string IHtmlTag.TagStart => "<aside";
            string IHtmlTag.EndTag => "</aside>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct AUDIO : IHtmlTag, IHasAttr_src, IHasAttr_crossorigin, IHasAttr_preload, IHasAttr_autoplay, IHasAttr_loop, IHasAttr_muted, IHasAttr_controls, IHtmlTagAllowingContent
        {
            public string TagName => "audio";
            string IHtmlTag.TagStart => "<audio";
            string IHtmlTag.EndTag => "</audio>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct B : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "b";
            string IHtmlTag.TagStart => "<b";
            string IHtmlTag.EndTag => "</b>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct BASE : IHtmlTag, IHasAttr_href, IHasAttr_target
        {
            public string TagName => "base";
            string IHtmlTag.TagStart => "<base";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct BDI : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "bdi";
            string IHtmlTag.TagStart => "<bdi";
            string IHtmlTag.EndTag => "</bdi>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct BDO : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "bdo";
            string IHtmlTag.TagStart => "<bdo";
            string IHtmlTag.EndTag => "</bdo>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct BLOCKQUOTE : IHtmlTag, IHasAttr_cite, IHtmlTagAllowingContent
        {
            public string TagName => "blockquote";
            string IHtmlTag.TagStart => "<blockquote";
            string IHtmlTag.EndTag => "</blockquote>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct BODY : IHtmlTag, IHasAttr_onafterprint, IHasAttr_onbeforeprint, IHasAttr_onbeforeunload, IHasAttr_onhashchange, IHasAttr_onlanguagechange, IHasAttr_onmessage, IHasAttr_onoffline, IHasAttr_ononline, IHasAttr_onpagehide, IHasAttr_onpageshow, IHasAttr_onpopstate, IHasAttr_onrejectionhandled, IHasAttr_onstorage, IHasAttr_onunhandledrejection, IHasAttr_onunload, IHtmlTagAllowingContent
        {
            public string TagName => "body";
            string IHtmlTag.TagStart => "<body";
            string IHtmlTag.EndTag => "</body>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct BR : IHtmlTag
        {
            public string TagName => "br";
            string IHtmlTag.TagStart => "<br";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct BUTTON : IHtmlTag, IHasAttr_autofocus, IHasAttr_disabled, IHasAttr_form, IHasAttr_formaction, IHasAttr_formenctype, IHasAttr_formmethod, IHasAttr_formnovalidate, IHasAttr_formtarget, IHasAttr_name, IHasAttr_type, IHasAttr_value, IHtmlTagAllowingContent
        {
            public string TagName => "button";
            string IHtmlTag.TagStart => "<button";
            string IHtmlTag.EndTag => "</button>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct CANVAS : IHtmlTag, IHasAttr_width, IHasAttr_height, IHtmlTagAllowingContent
        {
            public string TagName => "canvas";
            string IHtmlTag.TagStart => "<canvas";
            string IHtmlTag.EndTag => "</canvas>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct CAPTION : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "caption";
            string IHtmlTag.TagStart => "<caption";
            string IHtmlTag.EndTag => "</caption>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct CITE : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "cite";
            string IHtmlTag.TagStart => "<cite";
            string IHtmlTag.EndTag => "</cite>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct CODE : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "code";
            string IHtmlTag.TagStart => "<code";
            string IHtmlTag.EndTag => "</code>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct COL : IHtmlTag, IHasAttr_span
        {
            public string TagName => "col";
            string IHtmlTag.TagStart => "<col";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct COLGROUP : IHtmlTag, IHasAttr_span, IHtmlTagAllowingContent
        {
            public string TagName => "colgroup";
            string IHtmlTag.TagStart => "<colgroup";
            string IHtmlTag.EndTag => "</colgroup>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct DATA : IHtmlTag, IHasAttr_value, IHtmlTagAllowingContent
        {
            public string TagName => "data";
            string IHtmlTag.TagStart => "<data";
            string IHtmlTag.EndTag => "</data>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct DATALIST : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "datalist";
            string IHtmlTag.TagStart => "<datalist";
            string IHtmlTag.EndTag => "</datalist>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct DD : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "dd";
            string IHtmlTag.TagStart => "<dd";
            string IHtmlTag.EndTag => "</dd>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct DEL : IHtmlTag, IHasAttr_cite, IHasAttr_datetime, IHtmlTagAllowingContent
        {
            public string TagName => "del";
            string IHtmlTag.TagStart => "<del";
            string IHtmlTag.EndTag => "</del>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct DETAILS : IHtmlTag, IHasAttr_open, IHtmlTagAllowingContent
        {
            public string TagName => "details";
            string IHtmlTag.TagStart => "<details";
            string IHtmlTag.EndTag => "</details>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct DFN : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "dfn";
            string IHtmlTag.TagStart => "<dfn";
            string IHtmlTag.EndTag => "</dfn>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct DIALOG : IHtmlTag, IHasAttr_open, IHtmlTagAllowingContent
        {
            public string TagName => "dialog";
            string IHtmlTag.TagStart => "<dialog";
            string IHtmlTag.EndTag => "</dialog>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct DIV : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "div";
            string IHtmlTag.TagStart => "<div";
            string IHtmlTag.EndTag => "</div>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct DL : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "dl";
            string IHtmlTag.TagStart => "<dl";
            string IHtmlTag.EndTag => "</dl>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct DT : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "dt";
            string IHtmlTag.TagStart => "<dt";
            string IHtmlTag.EndTag => "</dt>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct EM : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "em";
            string IHtmlTag.TagStart => "<em";
            string IHtmlTag.EndTag => "</em>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct EMBED : IHtmlTag, IHasAttr_src, IHasAttr_type, IHasAttr_width, IHasAttr_height
        {
            public string TagName => "embed";
            string IHtmlTag.TagStart => "<embed";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct FIELDSET : IHtmlTag, IHasAttr_disabled, IHasAttr_form, IHasAttr_name, IHtmlTagAllowingContent
        {
            public string TagName => "fieldset";
            string IHtmlTag.TagStart => "<fieldset";
            string IHtmlTag.EndTag => "</fieldset>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct FIGCAPTION : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "figcaption";
            string IHtmlTag.TagStart => "<figcaption";
            string IHtmlTag.EndTag => "</figcaption>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct FIGURE : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "figure";
            string IHtmlTag.TagStart => "<figure";
            string IHtmlTag.EndTag => "</figure>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct FOOTER : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "footer";
            string IHtmlTag.TagStart => "<footer";
            string IHtmlTag.EndTag => "</footer>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct FORM : IHtmlTag, IHasAttr_accept_charset, IHasAttr_action, IHasAttr_autocomplete, IHasAttr_enctype, IHasAttr_method, IHasAttr_name, IHasAttr_novalidate, IHasAttr_target, IHtmlTagAllowingContent
        {
            public string TagName => "form";
            string IHtmlTag.TagStart => "<form";
            string IHtmlTag.EndTag => "</form>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct H1 : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "h1";
            string IHtmlTag.TagStart => "<h1";
            string IHtmlTag.EndTag => "</h1>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct H2 : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "h2";
            string IHtmlTag.TagStart => "<h2";
            string IHtmlTag.EndTag => "</h2>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct H3 : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "h3";
            string IHtmlTag.TagStart => "<h3";
            string IHtmlTag.EndTag => "</h3>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct H4 : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "h4";
            string IHtmlTag.TagStart => "<h4";
            string IHtmlTag.EndTag => "</h4>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct H5 : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "h5";
            string IHtmlTag.TagStart => "<h5";
            string IHtmlTag.EndTag => "</h5>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct H6 : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "h6";
            string IHtmlTag.TagStart => "<h6";
            string IHtmlTag.EndTag => "</h6>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct HEAD : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "head";
            string IHtmlTag.TagStart => "<head";
            string IHtmlTag.EndTag => "</head>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct HEADER : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "header";
            string IHtmlTag.TagStart => "<header";
            string IHtmlTag.EndTag => "</header>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct HGROUP : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "hgroup";
            string IHtmlTag.TagStart => "<hgroup";
            string IHtmlTag.EndTag => "</hgroup>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct HR : IHtmlTag
        {
            public string TagName => "hr";
            string IHtmlTag.TagStart => "<hr";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct HTML : IHtmlTag, IHasAttr_manifest, IHtmlTagAllowingContent
        {
            public string TagName => "html";
            string IHtmlTag.TagStart => "<html";
            string IHtmlTag.EndTag => "</html>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct I : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "i";
            string IHtmlTag.TagStart => "<i";
            string IHtmlTag.EndTag => "</i>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct IFRAME : IHtmlTag, IHasAttr_src, IHasAttr_srcdoc, IHasAttr_name, IHasAttr_sandbox, IHasAttr_allowfullscreen, IHasAttr_allowpaymentrequest, IHasAttr_allowusermedia, IHasAttr_width, IHasAttr_height, IHasAttr_referrerpolicy
        {
            public string TagName => "iframe";
            string IHtmlTag.TagStart => "<iframe";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct IMG : IHtmlTag, IHasAttr_alt, IHasAttr_src, IHasAttr_srcset, IHasAttr_crossorigin, IHasAttr_usemap, IHasAttr_ismap, IHasAttr_width, IHasAttr_height, IHasAttr_referrerpolicy
        {
            public string TagName => "img";
            string IHtmlTag.TagStart => "<img";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct INPUT : IHtmlTag, IHasAttr_accept, IHasAttr_alt, IHasAttr_autocomplete, IHasAttr_autofocus, IHasAttr_checked, IHasAttr_dirname, IHasAttr_disabled, IHasAttr_form, IHasAttr_formaction, IHasAttr_formenctype, IHasAttr_formmethod, IHasAttr_formnovalidate, IHasAttr_formtarget, IHasAttr_height, IHasAttr_inputmode, IHasAttr_list, IHasAttr_max, IHasAttr_maxlength, IHasAttr_min, IHasAttr_minlength, IHasAttr_multiple, IHasAttr_name, IHasAttr_pattern, IHasAttr_placeholder, IHasAttr_readonly, IHasAttr_required, IHasAttr_size, IHasAttr_src, IHasAttr_step, IHasAttr_type, IHasAttr_value, IHasAttr_width
        {
            public string TagName => "input";
            string IHtmlTag.TagStart => "<input";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct INS : IHtmlTag, IHasAttr_cite, IHasAttr_datetime, IHtmlTagAllowingContent
        {
            public string TagName => "ins";
            string IHtmlTag.TagStart => "<ins";
            string IHtmlTag.EndTag => "</ins>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct KBD : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "kbd";
            string IHtmlTag.TagStart => "<kbd";
            string IHtmlTag.EndTag => "</kbd>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct LABEL : IHtmlTag, IHasAttr_for, IHtmlTagAllowingContent
        {
            public string TagName => "label";
            string IHtmlTag.TagStart => "<label";
            string IHtmlTag.EndTag => "</label>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct LEGEND : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "legend";
            string IHtmlTag.TagStart => "<legend";
            string IHtmlTag.EndTag => "</legend>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct LI : IHtmlTag, IHasAttr_value, IHtmlTagAllowingContent
        {
            public string TagName => "li";
            string IHtmlTag.TagStart => "<li";
            string IHtmlTag.EndTag => "</li>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct LINK : IHtmlTag, IHasAttr_href, IHasAttr_crossorigin, IHasAttr_rel, IHasAttr_as, IHasAttr_media, IHasAttr_hreflang, IHasAttr_type, IHasAttr_sizes, IHasAttr_referrerpolicy, IHasAttr_nonce, IHasAttr_integrity
        {
            public string TagName => "link";
            string IHtmlTag.TagStart => "<link";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct MAIN : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "main";
            string IHtmlTag.TagStart => "<main";
            string IHtmlTag.EndTag => "</main>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct MAP : IHtmlTag, IHasAttr_name, IHtmlTagAllowingContent
        {
            public string TagName => "map";
            string IHtmlTag.TagStart => "<map";
            string IHtmlTag.EndTag => "</map>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct MARK : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "mark";
            string IHtmlTag.TagStart => "<mark";
            string IHtmlTag.EndTag => "</mark>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct MENU : IHtmlTag, IHasAttr_type, IHasAttr_label, IHtmlTagAllowingContent
        {
            public string TagName => "menu";
            string IHtmlTag.TagStart => "<menu";
            string IHtmlTag.EndTag => "</menu>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct MENUITEM : IHtmlTag, IHasAttr_type, IHasAttr_label, IHasAttr_icon, IHasAttr_disabled, IHasAttr_checked, IHasAttr_radiogroup, IHasAttr_default, IHtmlTagAllowingContent
        {
            public string TagName => "menuitem";
            string IHtmlTag.TagStart => "<menuitem";
            string IHtmlTag.EndTag => "</menuitem>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct META : IHtmlTag, IHasAttr_name, IHasAttr_http_equiv, IHasAttr_content, IHasAttr_charset
        {
            public string TagName => "meta";
            string IHtmlTag.TagStart => "<meta";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct METER : IHtmlTag, IHasAttr_value, IHasAttr_min, IHasAttr_max, IHasAttr_low, IHasAttr_high, IHasAttr_optimum, IHtmlTagAllowingContent
        {
            public string TagName => "meter";
            string IHtmlTag.TagStart => "<meter";
            string IHtmlTag.EndTag => "</meter>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct NAV : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "nav";
            string IHtmlTag.TagStart => "<nav";
            string IHtmlTag.EndTag => "</nav>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct NOSCRIPT : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "noscript";
            string IHtmlTag.TagStart => "<noscript";
            string IHtmlTag.EndTag => "</noscript>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct OBJECT : IHtmlTag, IHasAttr_data, IHasAttr_type, IHasAttr_typemustmatch, IHasAttr_name, IHasAttr_usemap, IHasAttr_form, IHasAttr_width, IHasAttr_height, IHtmlTagAllowingContent
        {
            public string TagName => "object";
            string IHtmlTag.TagStart => "<object";
            string IHtmlTag.EndTag => "</object>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct OL : IHtmlTag, IHasAttr_reversed, IHasAttr_start, IHasAttr_type, IHtmlTagAllowingContent
        {
            public string TagName => "ol";
            string IHtmlTag.TagStart => "<ol";
            string IHtmlTag.EndTag => "</ol>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct OPTGROUP : IHtmlTag, IHasAttr_disabled, IHasAttr_label, IHtmlTagAllowingContent
        {
            public string TagName => "optgroup";
            string IHtmlTag.TagStart => "<optgroup";
            string IHtmlTag.EndTag => "</optgroup>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct OPTION : IHtmlTag, IHasAttr_disabled, IHasAttr_label, IHasAttr_selected, IHasAttr_value, IHtmlTagAllowingContent
        {
            public string TagName => "option";
            string IHtmlTag.TagStart => "<option";
            string IHtmlTag.EndTag => "</option>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct OUTPUT : IHtmlTag, IHasAttr_for, IHasAttr_form, IHasAttr_name, IHtmlTagAllowingContent
        {
            public string TagName => "output";
            string IHtmlTag.TagStart => "<output";
            string IHtmlTag.EndTag => "</output>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct P : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "p";
            string IHtmlTag.TagStart => "<p";
            string IHtmlTag.EndTag => "</p>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct PARAM : IHtmlTag, IHasAttr_name, IHasAttr_value
        {
            public string TagName => "param";
            string IHtmlTag.TagStart => "<param";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct PICTURE : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "picture";
            string IHtmlTag.TagStart => "<picture";
            string IHtmlTag.EndTag => "</picture>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct PRE : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "pre";
            string IHtmlTag.TagStart => "<pre";
            string IHtmlTag.EndTag => "</pre>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct PROGRESS : IHtmlTag, IHasAttr_value, IHasAttr_max, IHtmlTagAllowingContent
        {
            public string TagName => "progress";
            string IHtmlTag.TagStart => "<progress";
            string IHtmlTag.EndTag => "</progress>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct Q : IHtmlTag, IHasAttr_cite, IHtmlTagAllowingContent
        {
            public string TagName => "q";
            string IHtmlTag.TagStart => "<q";
            string IHtmlTag.EndTag => "</q>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct RP : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "rp";
            string IHtmlTag.TagStart => "<rp";
            string IHtmlTag.EndTag => "</rp>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct RT : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "rt";
            string IHtmlTag.TagStart => "<rt";
            string IHtmlTag.EndTag => "</rt>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct RUBY : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "ruby";
            string IHtmlTag.TagStart => "<ruby";
            string IHtmlTag.EndTag => "</ruby>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct S : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "s";
            string IHtmlTag.TagStart => "<s";
            string IHtmlTag.EndTag => "</s>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct SAMP : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "samp";
            string IHtmlTag.TagStart => "<samp";
            string IHtmlTag.EndTag => "</samp>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct SCRIPT : IHtmlTag, IHasAttr_src, IHasAttr_type, IHasAttr_charset, IHasAttr_async, IHasAttr_defer, IHasAttr_crossorigin, IHasAttr_nonce, IHasAttr_integrity, IHtmlTagAllowingContent
        {
            public string TagName => "script";
            string IHtmlTag.TagStart => "<script";
            string IHtmlTag.EndTag => "</script>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct SECTION : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "section";
            string IHtmlTag.TagStart => "<section";
            string IHtmlTag.EndTag => "</section>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct SELECT : IHtmlTag, IHasAttr_autocomplete, IHasAttr_autofocus, IHasAttr_disabled, IHasAttr_form, IHasAttr_multiple, IHasAttr_name, IHasAttr_required, IHasAttr_size, IHtmlTagAllowingContent
        {
            public string TagName => "select";
            string IHtmlTag.TagStart => "<select";
            string IHtmlTag.EndTag => "</select>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct SLOT : IHtmlTag, IHasAttr_name, IHtmlTagAllowingContent
        {
            public string TagName => "slot";
            string IHtmlTag.TagStart => "<slot";
            string IHtmlTag.EndTag => "</slot>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct SMALL : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "small";
            string IHtmlTag.TagStart => "<small";
            string IHtmlTag.EndTag => "</small>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct SOURCE : IHtmlTag, IHasAttr_src, IHasAttr_sizes, IHasAttr_media
        {
            public string TagName => "source";
            string IHtmlTag.TagStart => "<source";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct SPAN : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "span";
            string IHtmlTag.TagStart => "<span";
            string IHtmlTag.EndTag => "</span>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct STRONG : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "strong";
            string IHtmlTag.TagStart => "<strong";
            string IHtmlTag.EndTag => "</strong>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct STYLE : IHtmlTag, IHasAttr_media, IHasAttr_nonce, IHasAttr_type, IHtmlTagAllowingContent
        {
            public string TagName => "style";
            string IHtmlTag.TagStart => "<style";
            string IHtmlTag.EndTag => "</style>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct SUB : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "sub";
            string IHtmlTag.TagStart => "<sub";
            string IHtmlTag.EndTag => "</sub>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct SUMMARY : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "summary";
            string IHtmlTag.TagStart => "<summary";
            string IHtmlTag.EndTag => "</summary>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct SUP : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "sup";
            string IHtmlTag.TagStart => "<sup";
            string IHtmlTag.EndTag => "</sup>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct TABLE : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "table";
            string IHtmlTag.TagStart => "<table";
            string IHtmlTag.EndTag => "</table>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct TBODY : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "tbody";
            string IHtmlTag.TagStart => "<tbody";
            string IHtmlTag.EndTag => "</tbody>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct TD : IHtmlTag, IHasAttr_colspan, IHasAttr_rowspan, IHasAttr_headers, IHtmlTagAllowingContent
        {
            public string TagName => "td";
            string IHtmlTag.TagStart => "<td";
            string IHtmlTag.EndTag => "</td>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct TEMPLATE : IHtmlTag
        {
            public string TagName => "template";
            string IHtmlTag.TagStart => "<template";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct TEXTAREA : IHtmlTag, IHasAttr_autofocus, IHasAttr_cols, IHasAttr_dirname, IHasAttr_disabled, IHasAttr_form, IHasAttr_inputmode, IHasAttr_maxlength, IHasAttr_minlength, IHasAttr_name, IHasAttr_placeholder, IHasAttr_readonly, IHasAttr_required, IHasAttr_rows, IHasAttr_wrap, IHtmlTagAllowingContent
        {
            public string TagName => "textarea";
            string IHtmlTag.TagStart => "<textarea";
            string IHtmlTag.EndTag => "</textarea>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct TFOOT : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "tfoot";
            string IHtmlTag.TagStart => "<tfoot";
            string IHtmlTag.EndTag => "</tfoot>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct TH : IHtmlTag, IHasAttr_colspan, IHasAttr_rowspan, IHasAttr_headers, IHasAttr_scope, IHasAttr_abbr, IHtmlTagAllowingContent
        {
            public string TagName => "th";
            string IHtmlTag.TagStart => "<th";
            string IHtmlTag.EndTag => "</th>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct THEAD : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "thead";
            string IHtmlTag.TagStart => "<thead";
            string IHtmlTag.EndTag => "</thead>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct TIME : IHtmlTag, IHasAttr_datetime, IHtmlTagAllowingContent
        {
            public string TagName => "time";
            string IHtmlTag.TagStart => "<time";
            string IHtmlTag.EndTag => "</time>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct TITLE : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "title";
            string IHtmlTag.TagStart => "<title";
            string IHtmlTag.EndTag => "</title>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct TR : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "tr";
            string IHtmlTag.TagStart => "<tr";
            string IHtmlTag.EndTag => "</tr>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct TRACK : IHtmlTag, IHasAttr_default, IHasAttr_kind, IHasAttr_label, IHasAttr_src, IHasAttr_srclang
        {
            public string TagName => "track";
            string IHtmlTag.TagStart => "<track";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
        public struct U : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "u";
            string IHtmlTag.TagStart => "<u";
            string IHtmlTag.EndTag => "</u>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct UL : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "ul";
            string IHtmlTag.TagStart => "<ul";
            string IHtmlTag.EndTag => "</ul>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct VAR : IHtmlTag, IHtmlTagAllowingContent
        {
            public string TagName => "var";
            string IHtmlTag.TagStart => "<var";
            string IHtmlTag.EndTag => "</var>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct VIDEO : IHtmlTag, IHasAttr_src, IHasAttr_crossorigin, IHasAttr_poster, IHasAttr_preload, IHasAttr_autoplay, IHasAttr_playsinline, IHasAttr_loop, IHasAttr_muted, IHasAttr_controls, IHasAttr_width, IHasAttr_height, IHtmlTagAllowingContent
        {
            public string TagName => "video";
            string IHtmlTag.TagStart => "<video";
            string IHtmlTag.EndTag => "</video>";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
            HtmlFragment[] IHtmlTagAllowingContent.Contents { get; set; }
        }
        public struct WBR : IHtmlTag
        {
            public string TagName => "wbr";
            string IHtmlTag.TagStart => "<wbr";
            string IHtmlTag.EndTag => "";
            HtmlAttribute[] IHtmlTag.Attributes { get; set; }
        }
    }

    public static class Tags
    {

        ///<summary>Hyperlink. See: <a href="https://html.spec.whatwg.org/#the-a-element">https://html.spec.whatwg.org/#the-a-element</a><br /></summary>
        public static readonly TagNames.A _a = new TagNames.A();

        ///<summary>Abbreviation. See: <a href="https://html.spec.whatwg.org/#the-abbr-element">https://html.spec.whatwg.org/#the-abbr-element</a><br /></summary>
        public static readonly TagNames.ABBR _abbr = new TagNames.ABBR();

        ///<summary>Contact information for a page or article element. See: <a href="https://html.spec.whatwg.org/#the-address-element">https://html.spec.whatwg.org/#the-address-element</a><br /></summary>
        public static readonly TagNames.ADDRESS _address = new TagNames.ADDRESS();

        ///<summary>Hyperlink or dead area on an image map. See: <a href="https://html.spec.whatwg.org/#the-area-element">https://html.spec.whatwg.org/#the-area-element</a><br /></summary>
        public static readonly TagNames.AREA _area = new TagNames.AREA();

        ///<summary>Self-contained syndicatable or reusable composition. See: <a href="https://html.spec.whatwg.org/#the-article-element">https://html.spec.whatwg.org/#the-article-element</a><br /></summary>
        public static readonly TagNames.ARTICLE _article = new TagNames.ARTICLE();

        ///<summary>Sidebar for tangentially related content. See: <a href="https://html.spec.whatwg.org/#the-aside-element">https://html.spec.whatwg.org/#the-aside-element</a><br /></summary>
        public static readonly TagNames.ASIDE _aside = new TagNames.ASIDE();

        ///<summary>Audio player. See: <a href="https://html.spec.whatwg.org/#the-audio-element">https://html.spec.whatwg.org/#the-audio-element</a><br /></summary>
        public static readonly TagNames.AUDIO _audio = new TagNames.AUDIO();

        ///<summary>Keywords. See: <a href="https://html.spec.whatwg.org/#the-b-element">https://html.spec.whatwg.org/#the-b-element</a><br /></summary>
        public static readonly TagNames.B _b = new TagNames.B();

        ///<summary>Base URL and default target browsing context for hyperlinks and forms. See: <a href="https://html.spec.whatwg.org/#the-base-element">https://html.spec.whatwg.org/#the-base-element</a><br /></summary>
        public static readonly TagNames.BASE _base = new TagNames.BASE();

        ///<summary>Text directionality isolation. See: <a href="https://html.spec.whatwg.org/#the-bdi-element">https://html.spec.whatwg.org/#the-bdi-element</a><br /></summary>
        public static readonly TagNames.BDI _bdi = new TagNames.BDI();

        ///<summary>Text directionality formatting. See: <a href="https://html.spec.whatwg.org/#the-bdo-element">https://html.spec.whatwg.org/#the-bdo-element</a><br /></summary>
        public static readonly TagNames.BDO _bdo = new TagNames.BDO();

        ///<summary>A section quoted from another source. See: <a href="https://html.spec.whatwg.org/#the-blockquote-element">https://html.spec.whatwg.org/#the-blockquote-element</a><br /></summary>
        public static readonly TagNames.BLOCKQUOTE _blockquote = new TagNames.BLOCKQUOTE();

        ///<summary>Document body. See: <a href="https://html.spec.whatwg.org/#the-body-element">https://html.spec.whatwg.org/#the-body-element</a><br /></summary>
        public static readonly TagNames.BODY _body = new TagNames.BODY();

        ///<summary>Line break, e.g. in poem or postal address. See: <a href="https://html.spec.whatwg.org/#the-br-element">https://html.spec.whatwg.org/#the-br-element</a><br /></summary>
        public static readonly TagNames.BR _br = new TagNames.BR();

        ///<summary>Button control. See: <a href="https://html.spec.whatwg.org/#the-button-element">https://html.spec.whatwg.org/#the-button-element</a><br /></summary>
        public static readonly TagNames.BUTTON _button = new TagNames.BUTTON();

        ///<summary>Scriptable bitmap canvas. See: <a href="https://html.spec.whatwg.org/#the-canvas-element">https://html.spec.whatwg.org/#the-canvas-element</a><br /></summary>
        public static readonly TagNames.CANVAS _canvas = new TagNames.CANVAS();

        ///<summary>Table caption. See: <a href="https://html.spec.whatwg.org/#the-caption-element">https://html.spec.whatwg.org/#the-caption-element</a><br /></summary>
        public static readonly TagNames.CAPTION _caption = new TagNames.CAPTION();

        ///<summary>Title of a work. See: <a href="https://html.spec.whatwg.org/#the-cite-element">https://html.spec.whatwg.org/#the-cite-element</a><br /></summary>
        public static readonly TagNames.CITE _cite = new TagNames.CITE();

        ///<summary>Computer code. See: <a href="https://html.spec.whatwg.org/#the-code-element">https://html.spec.whatwg.org/#the-code-element</a><br /></summary>
        public static readonly TagNames.CODE _code = new TagNames.CODE();

        ///<summary>Table column. See: <a href="https://html.spec.whatwg.org/#the-col-element">https://html.spec.whatwg.org/#the-col-element</a><br /></summary>
        public static readonly TagNames.COL _col = new TagNames.COL();

        ///<summary>Group of columns in a table. See: <a href="https://html.spec.whatwg.org/#the-colgroup-element">https://html.spec.whatwg.org/#the-colgroup-element</a><br /></summary>
        public static readonly TagNames.COLGROUP _colgroup = new TagNames.COLGROUP();

        ///<summary>Machine-readable equivalent. See: <a href="https://html.spec.whatwg.org/#the-data-element">https://html.spec.whatwg.org/#the-data-element</a><br /></summary>
        public static readonly TagNames.DATA _data = new TagNames.DATA();

        ///<summary>Container for options for combo box control. See: <a href="https://html.spec.whatwg.org/#the-datalist-element">https://html.spec.whatwg.org/#the-datalist-element</a><br /></summary>
        public static readonly TagNames.DATALIST _datalist = new TagNames.DATALIST();

        ///<summary>Content for corresponding dt element(s). See: <a href="https://html.spec.whatwg.org/#the-dd-element">https://html.spec.whatwg.org/#the-dd-element</a><br /></summary>
        public static readonly TagNames.DD _dd = new TagNames.DD();

        ///<summary>A removal from the document. See: <a href="https://html.spec.whatwg.org/#the-del-element">https://html.spec.whatwg.org/#the-del-element</a><br /></summary>
        public static readonly TagNames.DEL _del = new TagNames.DEL();

        ///<summary>Disclosure control for hiding details. See: <a href="https://html.spec.whatwg.org/#the-details-element">https://html.spec.whatwg.org/#the-details-element</a><br /></summary>
        public static readonly TagNames.DETAILS _details = new TagNames.DETAILS();

        ///<summary>Defining instance. See: <a href="https://html.spec.whatwg.org/#the-dfn-element">https://html.spec.whatwg.org/#the-dfn-element</a><br /></summary>
        public static readonly TagNames.DFN _dfn = new TagNames.DFN();

        ///<summary>Dialog box or window. See: <a href="https://html.spec.whatwg.org/#the-dialog-element">https://html.spec.whatwg.org/#the-dialog-element</a><br /></summary>
        public static readonly TagNames.DIALOG _dialog = new TagNames.DIALOG();

        ///<summary>Generic flow container, or container for name-value groups in dl elements. See: <a href="https://html.spec.whatwg.org/#the-div-element">https://html.spec.whatwg.org/#the-div-element</a><br /></summary>
        public static readonly TagNames.DIV _div = new TagNames.DIV();

        ///<summary>Association list consisting of zero or more name-value groups. See: <a href="https://html.spec.whatwg.org/#the-dl-element">https://html.spec.whatwg.org/#the-dl-element</a><br /></summary>
        public static readonly TagNames.DL _dl = new TagNames.DL();

        ///<summary>Legend for corresponding dd element(s). See: <a href="https://html.spec.whatwg.org/#the-dt-element">https://html.spec.whatwg.org/#the-dt-element</a><br /></summary>
        public static readonly TagNames.DT _dt = new TagNames.DT();

        ///<summary>Stress emphasis. See: <a href="https://html.spec.whatwg.org/#the-em-element">https://html.spec.whatwg.org/#the-em-element</a><br /></summary>
        public static readonly TagNames.EM _em = new TagNames.EM();

        ///<summary>Plugin. See: <a href="https://html.spec.whatwg.org/#the-embed-element">https://html.spec.whatwg.org/#the-embed-element</a><br /></summary>
        public static readonly TagNames.EMBED _embed = new TagNames.EMBED();

        ///<summary>Group of form controls. See: <a href="https://html.spec.whatwg.org/#the-fieldset-element">https://html.spec.whatwg.org/#the-fieldset-element</a><br /></summary>
        public static readonly TagNames.FIELDSET _fieldset = new TagNames.FIELDSET();

        ///<summary>Caption for figure. See: <a href="https://html.spec.whatwg.org/#the-figcaption-element">https://html.spec.whatwg.org/#the-figcaption-element</a><br /></summary>
        public static readonly TagNames.FIGCAPTION _figcaption = new TagNames.FIGCAPTION();

        ///<summary>Figure with optional caption. See: <a href="https://html.spec.whatwg.org/#the-figure-element">https://html.spec.whatwg.org/#the-figure-element</a><br /></summary>
        public static readonly TagNames.FIGURE _figure = new TagNames.FIGURE();

        ///<summary>Footer for a page or section. See: <a href="https://html.spec.whatwg.org/#the-footer-element">https://html.spec.whatwg.org/#the-footer-element</a><br /></summary>
        public static readonly TagNames.FOOTER _footer = new TagNames.FOOTER();

        ///<summary>User-submittable form. See: <a href="https://html.spec.whatwg.org/#the-form-element">https://html.spec.whatwg.org/#the-form-element</a><br /></summary>
        public static readonly TagNames.FORM _form = new TagNames.FORM();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly TagNames.H1 _h1 = new TagNames.H1();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly TagNames.H2 _h2 = new TagNames.H2();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly TagNames.H3 _h3 = new TagNames.H3();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly TagNames.H4 _h4 = new TagNames.H4();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly TagNames.H5 _h5 = new TagNames.H5();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly TagNames.H6 _h6 = new TagNames.H6();

        ///<summary>Container for document metadata. See: <a href="https://html.spec.whatwg.org/#the-head-element">https://html.spec.whatwg.org/#the-head-element</a><br /></summary>
        public static readonly TagNames.HEAD _head = new TagNames.HEAD();

        ///<summary>Introductory or navigational aids for a page or section. See: <a href="https://html.spec.whatwg.org/#the-header-element">https://html.spec.whatwg.org/#the-header-element</a><br /></summary>
        public static readonly TagNames.HEADER _header = new TagNames.HEADER();

        ///<summary>heading group. See: <a href="https://html.spec.whatwg.org/#the-hgroup-element">https://html.spec.whatwg.org/#the-hgroup-element</a><br /></summary>
        public static readonly TagNames.HGROUP _hgroup = new TagNames.HGROUP();

        ///<summary>Thematic break. See: <a href="https://html.spec.whatwg.org/#the-hr-element">https://html.spec.whatwg.org/#the-hr-element</a><br /></summary>
        public static readonly TagNames.HR _hr = new TagNames.HR();

        ///<summary>Root element. See: <a href="https://html.spec.whatwg.org/#the-html-element">https://html.spec.whatwg.org/#the-html-element</a><br /></summary>
        public static readonly TagNames.HTML _html = new TagNames.HTML();

        ///<summary>Alternate voice. See: <a href="https://html.spec.whatwg.org/#the-i-element">https://html.spec.whatwg.org/#the-i-element</a><br /></summary>
        public static readonly TagNames.I _i = new TagNames.I();

        ///<summary>Nested browsing context. See: <a href="https://html.spec.whatwg.org/#the-iframe-element">https://html.spec.whatwg.org/#the-iframe-element</a><br /></summary>
        public static readonly TagNames.IFRAME _iframe = new TagNames.IFRAME();

        ///<summary>Image. See: <a href="https://html.spec.whatwg.org/#the-img-element">https://html.spec.whatwg.org/#the-img-element</a><br /></summary>
        public static readonly TagNames.IMG _img = new TagNames.IMG();

        ///<summary>Form control. See: <a href="https://html.spec.whatwg.org/#the-input-element">https://html.spec.whatwg.org/#the-input-element</a><br /></summary>
        public static readonly TagNames.INPUT _input = new TagNames.INPUT();

        ///<summary>An addition to the document. See: <a href="https://html.spec.whatwg.org/#the-ins-element">https://html.spec.whatwg.org/#the-ins-element</a><br /></summary>
        public static readonly TagNames.INS _ins = new TagNames.INS();

        ///<summary>User input. See: <a href="https://html.spec.whatwg.org/#the-kbd-element">https://html.spec.whatwg.org/#the-kbd-element</a><br /></summary>
        public static readonly TagNames.KBD _kbd = new TagNames.KBD();

        ///<summary>Caption for a form control. See: <a href="https://html.spec.whatwg.org/#the-label-element">https://html.spec.whatwg.org/#the-label-element</a><br /></summary>
        public static readonly TagNames.LABEL _label = new TagNames.LABEL();

        ///<summary>Caption for fieldset. See: <a href="https://html.spec.whatwg.org/#the-legend-element">https://html.spec.whatwg.org/#the-legend-element</a><br /></summary>
        public static readonly TagNames.LEGEND _legend = new TagNames.LEGEND();

        ///<summary>List item. See: <a href="https://html.spec.whatwg.org/#the-li-element">https://html.spec.whatwg.org/#the-li-element</a><br /></summary>
        public static readonly TagNames.LI _li = new TagNames.LI();

        ///<summary>Link metadata. See: <a href="https://html.spec.whatwg.org/#the-link-element">https://html.spec.whatwg.org/#the-link-element</a><br /></summary>
        public static readonly TagNames.LINK _link = new TagNames.LINK();

        ///<summary>Container for the dominant contents of another element. See: <a href="https://html.spec.whatwg.org/#the-main-element">https://html.spec.whatwg.org/#the-main-element</a><br /></summary>
        public static readonly TagNames.MAIN _main = new TagNames.MAIN();

        ///<summary>Image map. See: <a href="https://html.spec.whatwg.org/#the-map-element">https://html.spec.whatwg.org/#the-map-element</a><br /></summary>
        public static readonly TagNames.MAP _map = new TagNames.MAP();

        ///<summary>Highlight. See: <a href="https://html.spec.whatwg.org/#the-mark-element">https://html.spec.whatwg.org/#the-mark-element</a><br /></summary>
        public static readonly TagNames.MARK _mark = new TagNames.MARK();

        ///<summary>Menu of commands. See: <a href="https://html.spec.whatwg.org/#the-menu-element">https://html.spec.whatwg.org/#the-menu-element</a><br /></summary>
        public static readonly TagNames.MENU _menu = new TagNames.MENU();

        ///<summary>Menu command. See: <a href="https://html.spec.whatwg.org/#the-menuitem-element">https://html.spec.whatwg.org/#the-menuitem-element</a><br /></summary>
        public static readonly TagNames.MENUITEM _menuitem = new TagNames.MENUITEM();

        ///<summary>Text metadata. See: <a href="https://html.spec.whatwg.org/#the-meta-element">https://html.spec.whatwg.org/#the-meta-element</a><br /></summary>
        public static readonly TagNames.META _meta = new TagNames.META();

        ///<summary>Gauge. See: <a href="https://html.spec.whatwg.org/#the-meter-element">https://html.spec.whatwg.org/#the-meter-element</a><br /></summary>
        public static readonly TagNames.METER _meter = new TagNames.METER();

        ///<summary>Section with navigational links. See: <a href="https://html.spec.whatwg.org/#the-nav-element">https://html.spec.whatwg.org/#the-nav-element</a><br /></summary>
        public static readonly TagNames.NAV _nav = new TagNames.NAV();

        ///<summary>Fallback content for script. See: <a href="https://html.spec.whatwg.org/#the-noscript-element">https://html.spec.whatwg.org/#the-noscript-element</a><br /></summary>
        public static readonly TagNames.NOSCRIPT _noscript = new TagNames.NOSCRIPT();

        ///<summary>Image, nested browsing context, or plugin. See: <a href="https://html.spec.whatwg.org/#the-object-element">https://html.spec.whatwg.org/#the-object-element</a><br /></summary>
        public static readonly TagNames.OBJECT _object = new TagNames.OBJECT();

        ///<summary>Ordered list. See: <a href="https://html.spec.whatwg.org/#the-ol-element">https://html.spec.whatwg.org/#the-ol-element</a><br /></summary>
        public static readonly TagNames.OL _ol = new TagNames.OL();

        ///<summary>Group of options in a list box. See: <a href="https://html.spec.whatwg.org/#the-optgroup-element">https://html.spec.whatwg.org/#the-optgroup-element</a><br /></summary>
        public static readonly TagNames.OPTGROUP _optgroup = new TagNames.OPTGROUP();

        ///<summary>Option in a list box or combo box control. See: <a href="https://html.spec.whatwg.org/#the-option-element">https://html.spec.whatwg.org/#the-option-element</a><br /></summary>
        public static readonly TagNames.OPTION _option = new TagNames.OPTION();

        ///<summary>Calculated output value. See: <a href="https://html.spec.whatwg.org/#the-output-element">https://html.spec.whatwg.org/#the-output-element</a><br /></summary>
        public static readonly TagNames.OUTPUT _output = new TagNames.OUTPUT();

        ///<summary>Paragraph. See: <a href="https://html.spec.whatwg.org/#the-p-element">https://html.spec.whatwg.org/#the-p-element</a><br /></summary>
        public static readonly TagNames.P _p = new TagNames.P();

        ///<summary>Parameter for object. See: <a href="https://html.spec.whatwg.org/#the-param-element">https://html.spec.whatwg.org/#the-param-element</a><br /></summary>
        public static readonly TagNames.PARAM _param = new TagNames.PARAM();

        ///<summary>Image. See: <a href="https://html.spec.whatwg.org/#the-picture-element">https://html.spec.whatwg.org/#the-picture-element</a><br /></summary>
        public static readonly TagNames.PICTURE _picture = new TagNames.PICTURE();

        ///<summary>Block of preformatted text. See: <a href="https://html.spec.whatwg.org/#the-pre-element">https://html.spec.whatwg.org/#the-pre-element</a><br /></summary>
        public static readonly TagNames.PRE _pre = new TagNames.PRE();

        ///<summary>Progress bar. See: <a href="https://html.spec.whatwg.org/#the-progress-element">https://html.spec.whatwg.org/#the-progress-element</a><br /></summary>
        public static readonly TagNames.PROGRESS _progress = new TagNames.PROGRESS();

        ///<summary>Quotation. See: <a href="https://html.spec.whatwg.org/#the-q-element">https://html.spec.whatwg.org/#the-q-element</a><br /></summary>
        public static readonly TagNames.Q _q = new TagNames.Q();

        ///<summary>Parenthesis for ruby annotation text. See: <a href="https://html.spec.whatwg.org/#the-rp-element">https://html.spec.whatwg.org/#the-rp-element</a><br /></summary>
        public static readonly TagNames.RP _rp = new TagNames.RP();

        ///<summary>Ruby annotation text. See: <a href="https://html.spec.whatwg.org/#the-rt-element">https://html.spec.whatwg.org/#the-rt-element</a><br /></summary>
        public static readonly TagNames.RT _rt = new TagNames.RT();

        ///<summary>Ruby annotation(s). See: <a href="https://html.spec.whatwg.org/#the-ruby-element">https://html.spec.whatwg.org/#the-ruby-element</a><br /></summary>
        public static readonly TagNames.RUBY _ruby = new TagNames.RUBY();

        ///<summary>Inaccurate text. See: <a href="https://html.spec.whatwg.org/#the-s-element">https://html.spec.whatwg.org/#the-s-element</a><br /></summary>
        public static readonly TagNames.S _s = new TagNames.S();

        ///<summary>Computer output. See: <a href="https://html.spec.whatwg.org/#the-samp-element">https://html.spec.whatwg.org/#the-samp-element</a><br /></summary>
        public static readonly TagNames.SAMP _samp = new TagNames.SAMP();

        ///<summary>Embedded script. See: <a href="https://html.spec.whatwg.org/#the-script-element">https://html.spec.whatwg.org/#the-script-element</a><br /></summary>
        public static readonly TagNames.SCRIPT _script = new TagNames.SCRIPT();

        ///<summary>Generic document or application section. See: <a href="https://html.spec.whatwg.org/#the-section-element">https://html.spec.whatwg.org/#the-section-element</a><br /></summary>
        public static readonly TagNames.SECTION _section = new TagNames.SECTION();

        ///<summary>List box control. See: <a href="https://html.spec.whatwg.org/#the-select-element">https://html.spec.whatwg.org/#the-select-element</a><br /></summary>
        public static readonly TagNames.SELECT _select = new TagNames.SELECT();

        ///<summary>Shadow tree slot. See: <a href="https://html.spec.whatwg.org/#the-slot-element">https://html.spec.whatwg.org/#the-slot-element</a><br /></summary>
        public static readonly TagNames.SLOT _slot = new TagNames.SLOT();

        ///<summary>Side comment. See: <a href="https://html.spec.whatwg.org/#the-small-element">https://html.spec.whatwg.org/#the-small-element</a><br /></summary>
        public static readonly TagNames.SMALL _small = new TagNames.SMALL();

        ///<summary>Image source for img or media source for video or audio. See: <a href="https://html.spec.whatwg.org/#the-source-element">https://html.spec.whatwg.org/#the-source-element</a><br /></summary>
        public static readonly TagNames.SOURCE _source = new TagNames.SOURCE();

        ///<summary>Generic phrasing container. See: <a href="https://html.spec.whatwg.org/#the-span-element">https://html.spec.whatwg.org/#the-span-element</a><br /></summary>
        public static readonly TagNames.SPAN _span = new TagNames.SPAN();

        ///<summary>Importance. See: <a href="https://html.spec.whatwg.org/#the-strong-element">https://html.spec.whatwg.org/#the-strong-element</a><br /></summary>
        public static readonly TagNames.STRONG _strong = new TagNames.STRONG();

        ///<summary>Embedded styling information. See: <a href="https://html.spec.whatwg.org/#the-style-element">https://html.spec.whatwg.org/#the-style-element</a><br /></summary>
        public static readonly TagNames.STYLE _style = new TagNames.STYLE();

        ///<summary>Subscript. See: <a href="https://html.spec.whatwg.org/#the-sub-and-sup-elements">https://html.spec.whatwg.org/#the-sub-and-sup-elements</a><br /></summary>
        public static readonly TagNames.SUB _sub = new TagNames.SUB();

        ///<summary>Caption for details. See: <a href="https://html.spec.whatwg.org/#the-summary-element">https://html.spec.whatwg.org/#the-summary-element</a><br /></summary>
        public static readonly TagNames.SUMMARY _summary = new TagNames.SUMMARY();

        ///<summary>Superscript. See: <a href="https://html.spec.whatwg.org/#the-sub-and-sup-elements">https://html.spec.whatwg.org/#the-sub-and-sup-elements</a><br /></summary>
        public static readonly TagNames.SUP _sup = new TagNames.SUP();

        ///<summary>Table. See: <a href="https://html.spec.whatwg.org/#the-table-element">https://html.spec.whatwg.org/#the-table-element</a><br /></summary>
        public static readonly TagNames.TABLE _table = new TagNames.TABLE();

        ///<summary>Group of rows in a table. See: <a href="https://html.spec.whatwg.org/#the-tbody-element">https://html.spec.whatwg.org/#the-tbody-element</a><br /></summary>
        public static readonly TagNames.TBODY _tbody = new TagNames.TBODY();

        ///<summary>Table cell. See: <a href="https://html.spec.whatwg.org/#the-td-element">https://html.spec.whatwg.org/#the-td-element</a><br /></summary>
        public static readonly TagNames.TD _td = new TagNames.TD();

        ///<summary>Template. See: <a href="https://html.spec.whatwg.org/#the-template-element">https://html.spec.whatwg.org/#the-template-element</a><br /></summary>
        public static readonly TagNames.TEMPLATE _template = new TagNames.TEMPLATE();

        ///<summary>Multiline text controls. See: <a href="https://html.spec.whatwg.org/#the-textarea-element">https://html.spec.whatwg.org/#the-textarea-element</a><br /></summary>
        public static readonly TagNames.TEXTAREA _textarea = new TagNames.TEXTAREA();

        ///<summary>Group of footer rows in a table. See: <a href="https://html.spec.whatwg.org/#the-tfoot-element">https://html.spec.whatwg.org/#the-tfoot-element</a><br /></summary>
        public static readonly TagNames.TFOOT _tfoot = new TagNames.TFOOT();

        ///<summary>Table header cell. See: <a href="https://html.spec.whatwg.org/#the-th-element">https://html.spec.whatwg.org/#the-th-element</a><br /></summary>
        public static readonly TagNames.TH _th = new TagNames.TH();

        ///<summary>Group of heading rows in a table. See: <a href="https://html.spec.whatwg.org/#the-thead-element">https://html.spec.whatwg.org/#the-thead-element</a><br /></summary>
        public static readonly TagNames.THEAD _thead = new TagNames.THEAD();

        ///<summary>Machine-readable equivalent of date- or time-related data. See: <a href="https://html.spec.whatwg.org/#the-time-element">https://html.spec.whatwg.org/#the-time-element</a><br /></summary>
        public static readonly TagNames.TIME _time = new TagNames.TIME();

        ///<summary>Document title. See: <a href="https://html.spec.whatwg.org/#the-title-element">https://html.spec.whatwg.org/#the-title-element</a><br /></summary>
        public static readonly TagNames.TITLE _title = new TagNames.TITLE();

        ///<summary>Table row. See: <a href="https://html.spec.whatwg.org/#the-tr-element">https://html.spec.whatwg.org/#the-tr-element</a><br /></summary>
        public static readonly TagNames.TR _tr = new TagNames.TR();

        ///<summary>Timed text track. See: <a href="https://html.spec.whatwg.org/#the-track-element">https://html.spec.whatwg.org/#the-track-element</a><br /></summary>
        public static readonly TagNames.TRACK _track = new TagNames.TRACK();

        ///<summary>Keywords. See: <a href="https://html.spec.whatwg.org/#the-u-element">https://html.spec.whatwg.org/#the-u-element</a><br /></summary>
        public static readonly TagNames.U _u = new TagNames.U();

        ///<summary>List. See: <a href="https://html.spec.whatwg.org/#the-ul-element">https://html.spec.whatwg.org/#the-ul-element</a><br /></summary>
        public static readonly TagNames.UL _ul = new TagNames.UL();

        ///<summary>Variable. See: <a href="https://html.spec.whatwg.org/#the-var-element">https://html.spec.whatwg.org/#the-var-element</a><br /></summary>
        public static readonly TagNames.VAR _var = new TagNames.VAR();

        ///<summary>Video player. See: <a href="https://html.spec.whatwg.org/#the-video-element">https://html.spec.whatwg.org/#the-video-element</a><br /></summary>
        public static readonly TagNames.VIDEO _video = new TagNames.VIDEO();

        ///<summary>Line breaking opportunity. See: <a href="https://html.spec.whatwg.org/#the-wbr-element">https://html.spec.whatwg.org/#the-wbr-element</a><br /></summary>
        public static readonly TagNames.WBR _wbr = new TagNames.WBR();
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
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("accesskey", attrValue);
        public static THtmlTag _contenteditable<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("contenteditable", attrValue);
        public static THtmlTag _contextmenu<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("contextmenu", attrValue);
        public static THtmlTag _dir<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("dir", attrValue);
        public static THtmlTag _draggable<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("draggable", attrValue);
        public static THtmlTag _hidden<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("hidden", attrValue);
        public static THtmlTag _is<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("is", attrValue);
        public static THtmlTag _itemid<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("itemid", attrValue);
        public static THtmlTag _itemprop<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("itemprop", attrValue);
        public static THtmlTag _itemref<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("itemref", attrValue);
        public static THtmlTag _itemscope<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("itemscope", attrValue);
        public static THtmlTag _itemtype<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("itemtype", attrValue);
        public static THtmlTag _lang<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("lang", attrValue);
        public static THtmlTag _spellcheck<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("spellcheck", attrValue);
        public static THtmlTag _style<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("style", attrValue);
        public static THtmlTag _tabindex<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("tabindex", attrValue);
        public static THtmlTag _title<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("title", attrValue);
        public static THtmlTag _translate<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute("translate", attrValue);
        public static THtmlTag _href<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_href, IHtmlTag
            => htmlTagExpr.Attribute("href", attrValue);
        public static THtmlTag _target<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_target, IHtmlTag
            => htmlTagExpr.Attribute("target", attrValue);
        public static THtmlTag _download<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_download, IHtmlTag
            => htmlTagExpr.Attribute("download", attrValue);
        public static THtmlTag _ping<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_ping, IHtmlTag
            => htmlTagExpr.Attribute("ping", attrValue);
        public static THtmlTag _rel<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_rel, IHtmlTag
            => htmlTagExpr.Attribute("rel", attrValue);
        public static THtmlTag _hreflang<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_hreflang, IHtmlTag
            => htmlTagExpr.Attribute("hreflang", attrValue);
        public static THtmlTag _type<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_type, IHtmlTag
            => htmlTagExpr.Attribute("type", attrValue);
        public static THtmlTag _referrerpolicy<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_referrerpolicy, IHtmlTag
            => htmlTagExpr.Attribute("referrerpolicy", attrValue);
        public static THtmlTag _alt<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_alt, IHtmlTag
            => htmlTagExpr.Attribute("alt", attrValue);
        public static THtmlTag _coords<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_coords, IHtmlTag
            => htmlTagExpr.Attribute("coords", attrValue);
        public static THtmlTag _shape<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_shape, IHtmlTag
            => htmlTagExpr.Attribute("shape", attrValue);
        public static THtmlTag _src<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_src, IHtmlTag
            => htmlTagExpr.Attribute("src", attrValue);
        public static THtmlTag _crossorigin<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_crossorigin, IHtmlTag
            => htmlTagExpr.Attribute("crossorigin", attrValue);
        public static THtmlTag _preload<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_preload, IHtmlTag
            => htmlTagExpr.Attribute("preload", attrValue);
        public static THtmlTag _autoplay<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_autoplay, IHtmlTag
            => htmlTagExpr.Attribute("autoplay", attrValue);
        public static THtmlTag _loop<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_loop, IHtmlTag
            => htmlTagExpr.Attribute("loop", attrValue);
        public static THtmlTag _muted<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_muted, IHtmlTag
            => htmlTagExpr.Attribute("muted", attrValue);
        public static THtmlTag _controls<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_controls, IHtmlTag
            => htmlTagExpr.Attribute("controls", attrValue);
        public static THtmlTag _cite<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_cite, IHtmlTag
            => htmlTagExpr.Attribute("cite", attrValue);
        public static THtmlTag _onafterprint<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onafterprint, IHtmlTag
            => htmlTagExpr.Attribute("onafterprint", attrValue);
        public static THtmlTag _onbeforeprint<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onbeforeprint, IHtmlTag
            => htmlTagExpr.Attribute("onbeforeprint", attrValue);
        public static THtmlTag _onbeforeunload<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onbeforeunload, IHtmlTag
            => htmlTagExpr.Attribute("onbeforeunload", attrValue);
        public static THtmlTag _onhashchange<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onhashchange, IHtmlTag
            => htmlTagExpr.Attribute("onhashchange", attrValue);
        public static THtmlTag _onlanguagechange<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onlanguagechange, IHtmlTag
            => htmlTagExpr.Attribute("onlanguagechange", attrValue);
        public static THtmlTag _onmessage<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onmessage, IHtmlTag
            => htmlTagExpr.Attribute("onmessage", attrValue);
        public static THtmlTag _onoffline<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onoffline, IHtmlTag
            => htmlTagExpr.Attribute("onoffline", attrValue);
        public static THtmlTag _ononline<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_ononline, IHtmlTag
            => htmlTagExpr.Attribute("ononline", attrValue);
        public static THtmlTag _onpagehide<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onpagehide, IHtmlTag
            => htmlTagExpr.Attribute("onpagehide", attrValue);
        public static THtmlTag _onpageshow<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onpageshow, IHtmlTag
            => htmlTagExpr.Attribute("onpageshow", attrValue);
        public static THtmlTag _onpopstate<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onpopstate, IHtmlTag
            => htmlTagExpr.Attribute("onpopstate", attrValue);
        public static THtmlTag _onrejectionhandled<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onrejectionhandled, IHtmlTag
            => htmlTagExpr.Attribute("onrejectionhandled", attrValue);
        public static THtmlTag _onstorage<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onstorage, IHtmlTag
            => htmlTagExpr.Attribute("onstorage", attrValue);
        public static THtmlTag _onunhandledrejection<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onunhandledrejection, IHtmlTag
            => htmlTagExpr.Attribute("onunhandledrejection", attrValue);
        public static THtmlTag _onunload<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_onunload, IHtmlTag
            => htmlTagExpr.Attribute("onunload", attrValue);
        public static THtmlTag _autofocus<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_autofocus, IHtmlTag
            => htmlTagExpr.Attribute("autofocus", attrValue);
        public static THtmlTag _disabled<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_disabled, IHtmlTag
            => htmlTagExpr.Attribute("disabled", attrValue);
        public static THtmlTag _form<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_form, IHtmlTag
            => htmlTagExpr.Attribute("form", attrValue);
        public static THtmlTag _formaction<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_formaction, IHtmlTag
            => htmlTagExpr.Attribute("formaction", attrValue);
        public static THtmlTag _formenctype<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_formenctype, IHtmlTag
            => htmlTagExpr.Attribute("formenctype", attrValue);
        public static THtmlTag _formmethod<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_formmethod, IHtmlTag
            => htmlTagExpr.Attribute("formmethod", attrValue);
        public static THtmlTag _formnovalidate<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_formnovalidate, IHtmlTag
            => htmlTagExpr.Attribute("formnovalidate", attrValue);
        public static THtmlTag _formtarget<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_formtarget, IHtmlTag
            => htmlTagExpr.Attribute("formtarget", attrValue);
        public static THtmlTag _name<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_name, IHtmlTag
            => htmlTagExpr.Attribute("name", attrValue);
        public static THtmlTag _value<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_value, IHtmlTag
            => htmlTagExpr.Attribute("value", attrValue);
        public static THtmlTag _width<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_width, IHtmlTag
            => htmlTagExpr.Attribute("width", attrValue);
        public static THtmlTag _height<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_height, IHtmlTag
            => htmlTagExpr.Attribute("height", attrValue);
        public static THtmlTag _span<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_span, IHtmlTag
            => htmlTagExpr.Attribute("span", attrValue);
        public static THtmlTag _datetime<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_datetime, IHtmlTag
            => htmlTagExpr.Attribute("datetime", attrValue);
        public static THtmlTag _open<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_open, IHtmlTag
            => htmlTagExpr.Attribute("open", attrValue);
        public static THtmlTag _accept_charset<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_accept_charset, IHtmlTag
            => htmlTagExpr.Attribute("accept-charset", attrValue);
        public static THtmlTag _action<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_action, IHtmlTag
            => htmlTagExpr.Attribute("action", attrValue);
        public static THtmlTag _autocomplete<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_autocomplete, IHtmlTag
            => htmlTagExpr.Attribute("autocomplete", attrValue);
        public static THtmlTag _enctype<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_enctype, IHtmlTag
            => htmlTagExpr.Attribute("enctype", attrValue);
        public static THtmlTag _method<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_method, IHtmlTag
            => htmlTagExpr.Attribute("method", attrValue);
        public static THtmlTag _novalidate<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_novalidate, IHtmlTag
            => htmlTagExpr.Attribute("novalidate", attrValue);
        public static THtmlTag _manifest<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_manifest, IHtmlTag
            => htmlTagExpr.Attribute("manifest", attrValue);
        public static THtmlTag _srcdoc<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_srcdoc, IHtmlTag
            => htmlTagExpr.Attribute("srcdoc", attrValue);
        public static THtmlTag _sandbox<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_sandbox, IHtmlTag
            => htmlTagExpr.Attribute("sandbox", attrValue);
        public static THtmlTag _allowfullscreen<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_allowfullscreen, IHtmlTag
            => htmlTagExpr.Attribute("allowfullscreen", attrValue);
        public static THtmlTag _allowpaymentrequest<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_allowpaymentrequest, IHtmlTag
            => htmlTagExpr.Attribute("allowpaymentrequest", attrValue);
        public static THtmlTag _allowusermedia<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_allowusermedia, IHtmlTag
            => htmlTagExpr.Attribute("allowusermedia", attrValue);
        public static THtmlTag _srcset<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_srcset, IHtmlTag
            => htmlTagExpr.Attribute("srcset", attrValue);
        public static THtmlTag _usemap<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_usemap, IHtmlTag
            => htmlTagExpr.Attribute("usemap", attrValue);
        public static THtmlTag _ismap<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_ismap, IHtmlTag
            => htmlTagExpr.Attribute("ismap", attrValue);
        public static THtmlTag _accept<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_accept, IHtmlTag
            => htmlTagExpr.Attribute("accept", attrValue);
        public static THtmlTag _checked<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_checked, IHtmlTag
            => htmlTagExpr.Attribute("checked", attrValue);
        public static THtmlTag _dirname<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_dirname, IHtmlTag
            => htmlTagExpr.Attribute("dirname", attrValue);
        public static THtmlTag _inputmode<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_inputmode, IHtmlTag
            => htmlTagExpr.Attribute("inputmode", attrValue);
        public static THtmlTag _list<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_list, IHtmlTag
            => htmlTagExpr.Attribute("list", attrValue);

        public static THtmlTag _max<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_max, IHtmlTag
            => htmlTagExpr.Attribute("max", attrValue);
        public static THtmlTag _maxlength<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_maxlength, IHtmlTag
            => htmlTagExpr.Attribute("maxlength", attrValue);
        public static THtmlTag _min<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_min, IHtmlTag
            => htmlTagExpr.Attribute("min", attrValue);
        public static THtmlTag _minlength<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_minlength, IHtmlTag
            => htmlTagExpr.Attribute("minlength", attrValue);
        public static THtmlTag _multiple<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_multiple, IHtmlTag
            => htmlTagExpr.Attribute("multiple", attrValue);
        public static THtmlTag _pattern<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_pattern, IHtmlTag
            => htmlTagExpr.Attribute("pattern", attrValue);
        public static THtmlTag _placeholder<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_placeholder, IHtmlTag
            => htmlTagExpr.Attribute("placeholder", attrValue);
        public static THtmlTag _readonly<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_readonly, IHtmlTag
            => htmlTagExpr.Attribute("readonly", attrValue);
        public static THtmlTag _required<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_required, IHtmlTag
            => htmlTagExpr.Attribute("required", attrValue);
        public static THtmlTag _size<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_size, IHtmlTag
            => htmlTagExpr.Attribute("size", attrValue);
        public static THtmlTag _step<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_step, IHtmlTag
            => htmlTagExpr.Attribute("step", attrValue);
        public static THtmlTag _for<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_for, IHtmlTag
            => htmlTagExpr.Attribute("for", attrValue);
        public static THtmlTag _as<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_as, IHtmlTag
            => htmlTagExpr.Attribute("as", attrValue);
        public static THtmlTag _media<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_media, IHtmlTag
            => htmlTagExpr.Attribute("media", attrValue);
        public static THtmlTag _sizes<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_sizes, IHtmlTag
            => htmlTagExpr.Attribute("sizes", attrValue);
        public static THtmlTag _nonce<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_nonce, IHtmlTag
            => htmlTagExpr.Attribute("nonce", attrValue);
        public static THtmlTag _integrity<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_integrity, IHtmlTag
            => htmlTagExpr.Attribute("integrity", attrValue);
        public static THtmlTag _label<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_label, IHtmlTag
            => htmlTagExpr.Attribute("label", attrValue);
        public static THtmlTag _icon<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_icon, IHtmlTag
            => htmlTagExpr.Attribute("icon", attrValue);
        public static THtmlTag _radiogroup<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_radiogroup, IHtmlTag
            => htmlTagExpr.Attribute("radiogroup", attrValue);
        public static THtmlTag _default<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_default, IHtmlTag
            => htmlTagExpr.Attribute("default", attrValue);
        public static THtmlTag _http_equiv<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_http_equiv, IHtmlTag
            => htmlTagExpr.Attribute("http-equiv", attrValue);
        public static THtmlTag _content<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_content, IHtmlTag
            => htmlTagExpr.Attribute("content", attrValue);
        public static THtmlTag _charset<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_charset, IHtmlTag
            => htmlTagExpr.Attribute("charset", attrValue);
        public static THtmlTag _low<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_low, IHtmlTag
            => htmlTagExpr.Attribute("low", attrValue);
        public static THtmlTag _high<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_high, IHtmlTag
            => htmlTagExpr.Attribute("high", attrValue);
        public static THtmlTag _optimum<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_optimum, IHtmlTag
            => htmlTagExpr.Attribute("optimum", attrValue);
        public static THtmlTag _data<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_data, IHtmlTag
            => htmlTagExpr.Attribute("data", attrValue);
        public static THtmlTag _typemustmatch<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_typemustmatch, IHtmlTag
            => htmlTagExpr.Attribute("typemustmatch", attrValue);
        public static THtmlTag _reversed<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_reversed, IHtmlTag
            => htmlTagExpr.Attribute("reversed", attrValue);
        public static THtmlTag _start<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_start, IHtmlTag
            => htmlTagExpr.Attribute("start", attrValue);
        public static THtmlTag _selected<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_selected, IHtmlTag
            => htmlTagExpr.Attribute("selected", attrValue);
        public static THtmlTag _async<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_async, IHtmlTag
            => htmlTagExpr.Attribute("async", attrValue);
        public static THtmlTag _defer<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_defer, IHtmlTag
            => htmlTagExpr.Attribute("defer", attrValue);
        public static THtmlTag _colspan<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_colspan, IHtmlTag
            => htmlTagExpr.Attribute("colspan", attrValue);
        public static THtmlTag _rowspan<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_rowspan, IHtmlTag
            => htmlTagExpr.Attribute("rowspan", attrValue);
        public static THtmlTag _headers<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_headers, IHtmlTag
            => htmlTagExpr.Attribute("headers", attrValue);
        public static THtmlTag _cols<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_cols, IHtmlTag
            => htmlTagExpr.Attribute("cols", attrValue);
        public static THtmlTag _rows<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_rows, IHtmlTag
            => htmlTagExpr.Attribute("rows", attrValue);
        public static THtmlTag _wrap<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_wrap, IHtmlTag
            => htmlTagExpr.Attribute("wrap", attrValue);
        public static THtmlTag _scope<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_scope, IHtmlTag
            => htmlTagExpr.Attribute("scope", attrValue);
        public static THtmlTag _abbr<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_abbr, IHtmlTag
            => htmlTagExpr.Attribute("abbr", attrValue);
        public static THtmlTag _kind<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_kind, IHtmlTag
            => htmlTagExpr.Attribute("kind", attrValue);
        public static THtmlTag _srclang<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_srclang, IHtmlTag
            => htmlTagExpr.Attribute("srclang", attrValue);
        public static THtmlTag _poster<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_poster, IHtmlTag
            => htmlTagExpr.Attribute("poster", attrValue);
        public static THtmlTag _playsinline<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_playsinline, IHtmlTag
            => htmlTagExpr.Attribute("playsinline", attrValue);
    }

}

