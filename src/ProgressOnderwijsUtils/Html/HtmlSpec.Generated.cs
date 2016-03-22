namespace ProgressOnderwijsUtils.Html
{
    using AttributeNameInterfaces;
    namespace AttributeNameInterfaces
    {
        public interface IHasAttr_href { }
        public interface IHasAttr_target { }
        public interface IHasAttr_download { }
        public interface IHasAttr_ping { }
        public interface IHasAttr_rel { }
        public interface IHasAttr_hreflang { }
        public interface IHasAttr_type { }
        public interface IHasAttr_alt { }
        public interface IHasAttr_coords { }
        public interface IHasAttr_shape { }
        public interface IHasAttr_src { }
        public interface IHasAttr_crossorigin { }
        public interface IHasAttr_preload { }
        public interface IHasAttr_autoplay { }
        public interface IHasAttr_mediagroup { }
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
        public interface IHasAttr_menu { }
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
        public interface IHasAttr_challenge { }
        public interface IHasAttr_keytype { }
        public interface IHasAttr_for { }
        public interface IHasAttr_media { }
        public interface IHasAttr_sizes { }
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
        public interface IHasAttr_nonce { }
        public interface IHasAttr_scoped { }
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
    }

    public static class AttributeConstructionMethods
    {
        public static TExpression _accesskey<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("accesskey", attrValue);
        public static TExpression _class<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("class", attrValue);
        public static TExpression _contenteditable<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("contenteditable", attrValue);
        public static TExpression _contextmenu<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("contextmenu", attrValue);
        public static TExpression _dir<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("dir", attrValue);
        public static TExpression _draggable<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("draggable", attrValue);
        public static TExpression _dropzone<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("dropzone", attrValue);
        public static TExpression _hidden<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("hidden", attrValue);
        public static TExpression _id<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("id", attrValue);
        public static TExpression _itemid<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("itemid", attrValue);
        public static TExpression _itemprop<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("itemprop", attrValue);
        public static TExpression _itemref<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("itemref", attrValue);
        public static TExpression _itemscope<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("itemscope", attrValue);
        public static TExpression _itemtype<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("itemtype", attrValue);
        public static TExpression _lang<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("lang", attrValue);
        public static TExpression _spellcheck<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("spellcheck", attrValue);
        public static TExpression _style<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("style", attrValue);
        public static TExpression _tabindex<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("tabindex", attrValue);
        public static TExpression _title<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("title", attrValue);
        public static TExpression _translate<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("translate", attrValue);
        public static HtmlTag<TName> _href<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_href, IHtmlTagName
            => htmlTagExpr.Attribute("href", attrValue);
        public static HtmlTag<TName> _target<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_target, IHtmlTagName
            => htmlTagExpr.Attribute("target", attrValue);
        public static HtmlTag<TName> _download<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_download, IHtmlTagName
            => htmlTagExpr.Attribute("download", attrValue);
        public static HtmlTag<TName> _ping<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_ping, IHtmlTagName
            => htmlTagExpr.Attribute("ping", attrValue);
        public static HtmlTag<TName> _rel<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_rel, IHtmlTagName
            => htmlTagExpr.Attribute("rel", attrValue);
        public static HtmlTag<TName> _hreflang<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_hreflang, IHtmlTagName
            => htmlTagExpr.Attribute("hreflang", attrValue);
        public static HtmlTag<TName> _type<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_type, IHtmlTagName
            => htmlTagExpr.Attribute("type", attrValue);
        public static HtmlTag<TName> _alt<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_alt, IHtmlTagName
            => htmlTagExpr.Attribute("alt", attrValue);
        public static HtmlTag<TName> _coords<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_coords, IHtmlTagName
            => htmlTagExpr.Attribute("coords", attrValue);
        public static HtmlTag<TName> _shape<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_shape, IHtmlTagName
            => htmlTagExpr.Attribute("shape", attrValue);
        public static HtmlTag<TName> _src<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_src, IHtmlTagName
            => htmlTagExpr.Attribute("src", attrValue);
        public static HtmlTag<TName> _crossorigin<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_crossorigin, IHtmlTagName
            => htmlTagExpr.Attribute("crossorigin", attrValue);
        public static HtmlTag<TName> _preload<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_preload, IHtmlTagName
            => htmlTagExpr.Attribute("preload", attrValue);
        public static HtmlTag<TName> _autoplay<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_autoplay, IHtmlTagName
            => htmlTagExpr.Attribute("autoplay", attrValue);
        public static HtmlTag<TName> _mediagroup<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_mediagroup, IHtmlTagName
            => htmlTagExpr.Attribute("mediagroup", attrValue);
        public static HtmlTag<TName> _loop<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_loop, IHtmlTagName
            => htmlTagExpr.Attribute("loop", attrValue);
        public static HtmlTag<TName> _muted<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_muted, IHtmlTagName
            => htmlTagExpr.Attribute("muted", attrValue);
        public static HtmlTag<TName> _controls<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_controls, IHtmlTagName
            => htmlTagExpr.Attribute("controls", attrValue);
        public static HtmlTag<TName> _cite<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_cite, IHtmlTagName
            => htmlTagExpr.Attribute("cite", attrValue);
        public static HtmlTag<TName> _onafterprint<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_onafterprint, IHtmlTagName
            => htmlTagExpr.Attribute("onafterprint", attrValue);
        public static HtmlTag<TName> _onbeforeprint<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_onbeforeprint, IHtmlTagName
            => htmlTagExpr.Attribute("onbeforeprint", attrValue);
        public static HtmlTag<TName> _onbeforeunload<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_onbeforeunload, IHtmlTagName
            => htmlTagExpr.Attribute("onbeforeunload", attrValue);
        public static HtmlTag<TName> _onhashchange<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_onhashchange, IHtmlTagName
            => htmlTagExpr.Attribute("onhashchange", attrValue);
        public static HtmlTag<TName> _onlanguagechange<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_onlanguagechange, IHtmlTagName
            => htmlTagExpr.Attribute("onlanguagechange", attrValue);
        public static HtmlTag<TName> _onmessage<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_onmessage, IHtmlTagName
            => htmlTagExpr.Attribute("onmessage", attrValue);
        public static HtmlTag<TName> _onoffline<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_onoffline, IHtmlTagName
            => htmlTagExpr.Attribute("onoffline", attrValue);
        public static HtmlTag<TName> _ononline<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_ononline, IHtmlTagName
            => htmlTagExpr.Attribute("ononline", attrValue);
        public static HtmlTag<TName> _onpagehide<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_onpagehide, IHtmlTagName
            => htmlTagExpr.Attribute("onpagehide", attrValue);
        public static HtmlTag<TName> _onpageshow<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_onpageshow, IHtmlTagName
            => htmlTagExpr.Attribute("onpageshow", attrValue);
        public static HtmlTag<TName> _onpopstate<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_onpopstate, IHtmlTagName
            => htmlTagExpr.Attribute("onpopstate", attrValue);
        public static HtmlTag<TName> _onrejectionhandled<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_onrejectionhandled, IHtmlTagName
            => htmlTagExpr.Attribute("onrejectionhandled", attrValue);
        public static HtmlTag<TName> _onstorage<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_onstorage, IHtmlTagName
            => htmlTagExpr.Attribute("onstorage", attrValue);
        public static HtmlTag<TName> _onunhandledrejection<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_onunhandledrejection, IHtmlTagName
            => htmlTagExpr.Attribute("onunhandledrejection", attrValue);
        public static HtmlTag<TName> _onunload<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_onunload, IHtmlTagName
            => htmlTagExpr.Attribute("onunload", attrValue);
        public static HtmlTag<TName> _autofocus<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_autofocus, IHtmlTagName
            => htmlTagExpr.Attribute("autofocus", attrValue);
        public static HtmlTag<TName> _disabled<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_disabled, IHtmlTagName
            => htmlTagExpr.Attribute("disabled", attrValue);
        public static HtmlTag<TName> _form<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_form, IHtmlTagName
            => htmlTagExpr.Attribute("form", attrValue);
        public static HtmlTag<TName> _formaction<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_formaction, IHtmlTagName
            => htmlTagExpr.Attribute("formaction", attrValue);
        public static HtmlTag<TName> _formenctype<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_formenctype, IHtmlTagName
            => htmlTagExpr.Attribute("formenctype", attrValue);
        public static HtmlTag<TName> _formmethod<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_formmethod, IHtmlTagName
            => htmlTagExpr.Attribute("formmethod", attrValue);
        public static HtmlTag<TName> _formnovalidate<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_formnovalidate, IHtmlTagName
            => htmlTagExpr.Attribute("formnovalidate", attrValue);
        public static HtmlTag<TName> _formtarget<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_formtarget, IHtmlTagName
            => htmlTagExpr.Attribute("formtarget", attrValue);
        public static HtmlTag<TName> _menu<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_menu, IHtmlTagName
            => htmlTagExpr.Attribute("menu", attrValue);
        public static HtmlTag<TName> _name<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_name, IHtmlTagName
            => htmlTagExpr.Attribute("name", attrValue);
        public static HtmlTag<TName> _value<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_value, IHtmlTagName
            => htmlTagExpr.Attribute("value", attrValue);
        public static HtmlTag<TName> _width<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_width, IHtmlTagName
            => htmlTagExpr.Attribute("width", attrValue);
        public static HtmlTag<TName> _height<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_height, IHtmlTagName
            => htmlTagExpr.Attribute("height", attrValue);
        public static HtmlTag<TName> _span<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_span, IHtmlTagName
            => htmlTagExpr.Attribute("span", attrValue);
        public static HtmlTag<TName> _datetime<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_datetime, IHtmlTagName
            => htmlTagExpr.Attribute("datetime", attrValue);
        public static HtmlTag<TName> _open<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_open, IHtmlTagName
            => htmlTagExpr.Attribute("open", attrValue);
        public static HtmlTag<TName> _accept_charset<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_accept_charset, IHtmlTagName
            => htmlTagExpr.Attribute("accept-charset", attrValue);
        public static HtmlTag<TName> _action<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_action, IHtmlTagName
            => htmlTagExpr.Attribute("action", attrValue);
        public static HtmlTag<TName> _autocomplete<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_autocomplete, IHtmlTagName
            => htmlTagExpr.Attribute("autocomplete", attrValue);
        public static HtmlTag<TName> _enctype<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_enctype, IHtmlTagName
            => htmlTagExpr.Attribute("enctype", attrValue);
        public static HtmlTag<TName> _method<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_method, IHtmlTagName
            => htmlTagExpr.Attribute("method", attrValue);
        public static HtmlTag<TName> _novalidate<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_novalidate, IHtmlTagName
            => htmlTagExpr.Attribute("novalidate", attrValue);
        public static HtmlTag<TName> _manifest<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_manifest, IHtmlTagName
            => htmlTagExpr.Attribute("manifest", attrValue);
        public static HtmlTag<TName> _srcdoc<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_srcdoc, IHtmlTagName
            => htmlTagExpr.Attribute("srcdoc", attrValue);
        public static HtmlTag<TName> _sandbox<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_sandbox, IHtmlTagName
            => htmlTagExpr.Attribute("sandbox", attrValue);
        public static HtmlTag<TName> _allowfullscreen<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_allowfullscreen, IHtmlTagName
            => htmlTagExpr.Attribute("allowfullscreen", attrValue);
        public static HtmlTag<TName> _srcset<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_srcset, IHtmlTagName
            => htmlTagExpr.Attribute("srcset", attrValue);
        public static HtmlTag<TName> _usemap<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_usemap, IHtmlTagName
            => htmlTagExpr.Attribute("usemap", attrValue);
        public static HtmlTag<TName> _ismap<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_ismap, IHtmlTagName
            => htmlTagExpr.Attribute("ismap", attrValue);
        public static HtmlTag<TName> _accept<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_accept, IHtmlTagName
            => htmlTagExpr.Attribute("accept", attrValue);
        public static HtmlTag<TName> _checked<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_checked, IHtmlTagName
            => htmlTagExpr.Attribute("checked", attrValue);
        public static HtmlTag<TName> _dirname<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_dirname, IHtmlTagName
            => htmlTagExpr.Attribute("dirname", attrValue);
        public static HtmlTag<TName> _inputmode<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_inputmode, IHtmlTagName
            => htmlTagExpr.Attribute("inputmode", attrValue);
        public static HtmlTag<TName> _list<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_list, IHtmlTagName
            => htmlTagExpr.Attribute("list", attrValue);
        public static HtmlTag<TName> _max<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_max, IHtmlTagName
            => htmlTagExpr.Attribute("max", attrValue);
        public static HtmlTag<TName> _maxlength<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_maxlength, IHtmlTagName
            => htmlTagExpr.Attribute("maxlength", attrValue);
        public static HtmlTag<TName> _min<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_min, IHtmlTagName
            => htmlTagExpr.Attribute("min", attrValue);
        public static HtmlTag<TName> _minlength<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_minlength, IHtmlTagName
            => htmlTagExpr.Attribute("minlength", attrValue);
        public static HtmlTag<TName> _multiple<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_multiple, IHtmlTagName
            => htmlTagExpr.Attribute("multiple", attrValue);
        public static HtmlTag<TName> _pattern<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_pattern, IHtmlTagName
            => htmlTagExpr.Attribute("pattern", attrValue);
        public static HtmlTag<TName> _placeholder<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_placeholder, IHtmlTagName
            => htmlTagExpr.Attribute("placeholder", attrValue);
        public static HtmlTag<TName> _readonly<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_readonly, IHtmlTagName
            => htmlTagExpr.Attribute("readonly", attrValue);
        public static HtmlTag<TName> _required<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_required, IHtmlTagName
            => htmlTagExpr.Attribute("required", attrValue);
        public static HtmlTag<TName> _size<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_size, IHtmlTagName
            => htmlTagExpr.Attribute("size", attrValue);
        public static HtmlTag<TName> _step<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_step, IHtmlTagName
            => htmlTagExpr.Attribute("step", attrValue);
        public static HtmlTag<TName> _challenge<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_challenge, IHtmlTagName
            => htmlTagExpr.Attribute("challenge", attrValue);
        public static HtmlTag<TName> _keytype<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_keytype, IHtmlTagName
            => htmlTagExpr.Attribute("keytype", attrValue);
        public static HtmlTag<TName> _for<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_for, IHtmlTagName
            => htmlTagExpr.Attribute("for", attrValue);
        public static HtmlTag<TName> _media<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_media, IHtmlTagName
            => htmlTagExpr.Attribute("media", attrValue);
        public static HtmlTag<TName> _sizes<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_sizes, IHtmlTagName
            => htmlTagExpr.Attribute("sizes", attrValue);
        public static HtmlTag<TName> _label<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_label, IHtmlTagName
            => htmlTagExpr.Attribute("label", attrValue);
        public static HtmlTag<TName> _icon<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_icon, IHtmlTagName
            => htmlTagExpr.Attribute("icon", attrValue);
        public static HtmlTag<TName> _radiogroup<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_radiogroup, IHtmlTagName
            => htmlTagExpr.Attribute("radiogroup", attrValue);
        public static HtmlTag<TName> _default<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_default, IHtmlTagName
            => htmlTagExpr.Attribute("default", attrValue);
        public static HtmlTag<TName> _http_equiv<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_http_equiv, IHtmlTagName
            => htmlTagExpr.Attribute("http-equiv", attrValue);
        public static HtmlTag<TName> _content<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_content, IHtmlTagName
            => htmlTagExpr.Attribute("content", attrValue);
        public static HtmlTag<TName> _charset<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_charset, IHtmlTagName
            => htmlTagExpr.Attribute("charset", attrValue);
        public static HtmlTag<TName> _low<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_low, IHtmlTagName
            => htmlTagExpr.Attribute("low", attrValue);
        public static HtmlTag<TName> _high<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_high, IHtmlTagName
            => htmlTagExpr.Attribute("high", attrValue);
        public static HtmlTag<TName> _optimum<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_optimum, IHtmlTagName
            => htmlTagExpr.Attribute("optimum", attrValue);
        public static HtmlTag<TName> _data<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_data, IHtmlTagName
            => htmlTagExpr.Attribute("data", attrValue);
        public static HtmlTag<TName> _typemustmatch<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_typemustmatch, IHtmlTagName
            => htmlTagExpr.Attribute("typemustmatch", attrValue);
        public static HtmlTag<TName> _reversed<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_reversed, IHtmlTagName
            => htmlTagExpr.Attribute("reversed", attrValue);
        public static HtmlTag<TName> _start<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_start, IHtmlTagName
            => htmlTagExpr.Attribute("start", attrValue);
        public static HtmlTag<TName> _selected<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_selected, IHtmlTagName
            => htmlTagExpr.Attribute("selected", attrValue);
        public static HtmlTag<TName> _async<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_async, IHtmlTagName
            => htmlTagExpr.Attribute("async", attrValue);
        public static HtmlTag<TName> _defer<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_defer, IHtmlTagName
            => htmlTagExpr.Attribute("defer", attrValue);
        public static HtmlTag<TName> _nonce<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_nonce, IHtmlTagName
            => htmlTagExpr.Attribute("nonce", attrValue);
        public static HtmlTag<TName> _scoped<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_scoped, IHtmlTagName
            => htmlTagExpr.Attribute("scoped", attrValue);
        public static HtmlTag<TName> _colspan<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_colspan, IHtmlTagName
            => htmlTagExpr.Attribute("colspan", attrValue);
        public static HtmlTag<TName> _rowspan<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_rowspan, IHtmlTagName
            => htmlTagExpr.Attribute("rowspan", attrValue);
        public static HtmlTag<TName> _headers<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_headers, IHtmlTagName
            => htmlTagExpr.Attribute("headers", attrValue);
        public static HtmlTag<TName> _cols<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_cols, IHtmlTagName
            => htmlTagExpr.Attribute("cols", attrValue);
        public static HtmlTag<TName> _rows<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_rows, IHtmlTagName
            => htmlTagExpr.Attribute("rows", attrValue);
        public static HtmlTag<TName> _wrap<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_wrap, IHtmlTagName
            => htmlTagExpr.Attribute("wrap", attrValue);
        public static HtmlTag<TName> _scope<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_scope, IHtmlTagName
            => htmlTagExpr.Attribute("scope", attrValue);
        public static HtmlTag<TName> _abbr<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_abbr, IHtmlTagName
            => htmlTagExpr.Attribute("abbr", attrValue);
        public static HtmlTag<TName> _kind<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_kind, IHtmlTagName
            => htmlTagExpr.Attribute("kind", attrValue);
        public static HtmlTag<TName> _srclang<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_srclang, IHtmlTagName
            => htmlTagExpr.Attribute("srclang", attrValue);
        public static HtmlTag<TName> _poster<TName>(this HtmlTag<TName> htmlTagExpr, string attrValue)
            where TName : struct, IHasAttr_poster, IHtmlTagName
            => htmlTagExpr.Attribute("poster", attrValue);
    }

    public static class TagNames
    {
        public struct A : IHtmlTagName, IHasAttr_href, IHasAttr_target, IHasAttr_download, IHasAttr_ping, IHasAttr_rel, IHasAttr_hreflang, IHasAttr_type { public string TagName => "a"; }
        public struct ABBR : IHtmlTagName { public string TagName => "abbr"; }
        public struct ADDRESS : IHtmlTagName { public string TagName => "address"; }
        public struct AREA : IHtmlTagName, IHasAttr_alt, IHasAttr_coords, IHasAttr_shape, IHasAttr_href, IHasAttr_target, IHasAttr_download, IHasAttr_ping, IHasAttr_rel { public string TagName => "area"; }
        public struct ARTICLE : IHtmlTagName { public string TagName => "article"; }
        public struct ASIDE : IHtmlTagName { public string TagName => "aside"; }
        public struct AUDIO : IHtmlTagName, IHasAttr_src, IHasAttr_crossorigin, IHasAttr_preload, IHasAttr_autoplay, IHasAttr_mediagroup, IHasAttr_loop, IHasAttr_muted, IHasAttr_controls { public string TagName => "audio"; }
        public struct B : IHtmlTagName { public string TagName => "b"; }
        public struct BASE : IHtmlTagName, IHasAttr_href, IHasAttr_target { public string TagName => "base"; }
        public struct BDI : IHtmlTagName { public string TagName => "bdi"; }
        public struct BDO : IHtmlTagName { public string TagName => "bdo"; }
        public struct BLOCKQUOTE : IHtmlTagName, IHasAttr_cite { public string TagName => "blockquote"; }
        public struct BODY : IHtmlTagName, IHasAttr_onafterprint, IHasAttr_onbeforeprint, IHasAttr_onbeforeunload, IHasAttr_onhashchange, IHasAttr_onlanguagechange, IHasAttr_onmessage, IHasAttr_onoffline, IHasAttr_ononline, IHasAttr_onpagehide, IHasAttr_onpageshow, IHasAttr_onpopstate, IHasAttr_onrejectionhandled, IHasAttr_onstorage, IHasAttr_onunhandledrejection, IHasAttr_onunload { public string TagName => "body"; }
        public struct BR : IHtmlTagName { public string TagName => "br"; }
        public struct BUTTON : IHtmlTagName, IHasAttr_autofocus, IHasAttr_disabled, IHasAttr_form, IHasAttr_formaction, IHasAttr_formenctype, IHasAttr_formmethod, IHasAttr_formnovalidate, IHasAttr_formtarget, IHasAttr_menu, IHasAttr_name, IHasAttr_type, IHasAttr_value { public string TagName => "button"; }
        public struct CANVAS : IHtmlTagName, IHasAttr_width, IHasAttr_height { public string TagName => "canvas"; }
        public struct CAPTION : IHtmlTagName { public string TagName => "caption"; }
        public struct CITE : IHtmlTagName { public string TagName => "cite"; }
        public struct CODE : IHtmlTagName { public string TagName => "code"; }
        public struct COL : IHtmlTagName, IHasAttr_span { public string TagName => "col"; }
        public struct COLGROUP : IHtmlTagName, IHasAttr_span { public string TagName => "colgroup"; }
        public struct DATA : IHtmlTagName, IHasAttr_value { public string TagName => "data"; }
        public struct DATALIST : IHtmlTagName { public string TagName => "datalist"; }
        public struct DD : IHtmlTagName { public string TagName => "dd"; }
        public struct DEL : IHtmlTagName, IHasAttr_cite, IHasAttr_datetime { public string TagName => "del"; }
        public struct DETAILS : IHtmlTagName, IHasAttr_open { public string TagName => "details"; }
        public struct DFN : IHtmlTagName { public string TagName => "dfn"; }
        public struct DIALOG : IHtmlTagName, IHasAttr_open { public string TagName => "dialog"; }
        public struct DIV : IHtmlTagName { public string TagName => "div"; }
        public struct DL : IHtmlTagName { public string TagName => "dl"; }
        public struct DT : IHtmlTagName { public string TagName => "dt"; }
        public struct EM : IHtmlTagName { public string TagName => "em"; }
        public struct EMBED : IHtmlTagName, IHasAttr_src, IHasAttr_type, IHasAttr_width, IHasAttr_height { public string TagName => "embed"; }
        public struct FIELDSET : IHtmlTagName, IHasAttr_disabled, IHasAttr_form, IHasAttr_name { public string TagName => "fieldset"; }
        public struct FIGCAPTION : IHtmlTagName { public string TagName => "figcaption"; }
        public struct FIGURE : IHtmlTagName { public string TagName => "figure"; }
        public struct FOOTER : IHtmlTagName { public string TagName => "footer"; }
        public struct FORM : IHtmlTagName, IHasAttr_accept_charset, IHasAttr_action, IHasAttr_autocomplete, IHasAttr_enctype, IHasAttr_method, IHasAttr_name, IHasAttr_novalidate, IHasAttr_target { public string TagName => "form"; }
        public struct H1 : IHtmlTagName { public string TagName => "h1"; }
        public struct H2 : IHtmlTagName { public string TagName => "h2"; }
        public struct H3 : IHtmlTagName { public string TagName => "h3"; }
        public struct H4 : IHtmlTagName { public string TagName => "h4"; }
        public struct H5 : IHtmlTagName { public string TagName => "h5"; }
        public struct H6 : IHtmlTagName { public string TagName => "h6"; }
        public struct HEAD : IHtmlTagName { public string TagName => "head"; }
        public struct HEADER : IHtmlTagName { public string TagName => "header"; }
        public struct HGROUP : IHtmlTagName { public string TagName => "hgroup"; }
        public struct HR : IHtmlTagName { public string TagName => "hr"; }
        public struct HTML : IHtmlTagName, IHasAttr_manifest { public string TagName => "html"; }
        public struct I : IHtmlTagName { public string TagName => "i"; }
        public struct IFRAME : IHtmlTagName, IHasAttr_src, IHasAttr_srcdoc, IHasAttr_name, IHasAttr_sandbox, IHasAttr_allowfullscreen, IHasAttr_width, IHasAttr_height { public string TagName => "iframe"; }
        public struct IMG : IHtmlTagName, IHasAttr_alt, IHasAttr_src, IHasAttr_srcset, IHasAttr_crossorigin, IHasAttr_usemap, IHasAttr_ismap, IHasAttr_width, IHasAttr_height { public string TagName => "img"; }
        public struct INPUT : IHtmlTagName, IHasAttr_accept, IHasAttr_alt, IHasAttr_autocomplete, IHasAttr_autofocus, IHasAttr_checked, IHasAttr_dirname, IHasAttr_disabled, IHasAttr_form, IHasAttr_formaction, IHasAttr_formenctype, IHasAttr_formmethod, IHasAttr_formnovalidate, IHasAttr_formtarget, IHasAttr_height, IHasAttr_inputmode, IHasAttr_list, IHasAttr_max, IHasAttr_maxlength, IHasAttr_min, IHasAttr_minlength, IHasAttr_multiple, IHasAttr_name, IHasAttr_pattern, IHasAttr_placeholder, IHasAttr_readonly, IHasAttr_required, IHasAttr_size, IHasAttr_src, IHasAttr_step, IHasAttr_type, IHasAttr_value, IHasAttr_width { public string TagName => "input"; }
        public struct INS : IHtmlTagName, IHasAttr_cite, IHasAttr_datetime { public string TagName => "ins"; }
        public struct KBD : IHtmlTagName { public string TagName => "kbd"; }
        public struct KEYGEN : IHtmlTagName, IHasAttr_autofocus, IHasAttr_challenge, IHasAttr_disabled, IHasAttr_form, IHasAttr_keytype, IHasAttr_name { public string TagName => "keygen"; }
        public struct LABEL : IHtmlTagName, IHasAttr_form, IHasAttr_for { public string TagName => "label"; }
        public struct LEGEND : IHtmlTagName { public string TagName => "legend"; }
        public struct LI : IHtmlTagName, IHasAttr_value { public string TagName => "li"; }
        public struct LINK : IHtmlTagName, IHasAttr_href, IHasAttr_crossorigin, IHasAttr_rel, IHasAttr_media, IHasAttr_hreflang, IHasAttr_type, IHasAttr_sizes { public string TagName => "link"; }
        public struct MAIN : IHtmlTagName { public string TagName => "main"; }
        public struct MAP : IHtmlTagName, IHasAttr_name { public string TagName => "map"; }
        public struct MARK : IHtmlTagName { public string TagName => "mark"; }
        public struct MATH : IHtmlTagName { public string TagName => "math"; }
        public struct MENU : IHtmlTagName, IHasAttr_type, IHasAttr_label { public string TagName => "menu"; }
        public struct MENUITEM : IHtmlTagName, IHasAttr_type, IHasAttr_label, IHasAttr_icon, IHasAttr_disabled, IHasAttr_checked, IHasAttr_radiogroup, IHasAttr_default { public string TagName => "menuitem"; }
        public struct META : IHtmlTagName, IHasAttr_name, IHasAttr_http_equiv, IHasAttr_content, IHasAttr_charset { public string TagName => "meta"; }
        public struct METER : IHtmlTagName, IHasAttr_value, IHasAttr_min, IHasAttr_max, IHasAttr_low, IHasAttr_high, IHasAttr_optimum { public string TagName => "meter"; }
        public struct NAV : IHtmlTagName { public string TagName => "nav"; }
        public struct NOSCRIPT : IHtmlTagName { public string TagName => "noscript"; }
        public struct OBJECT : IHtmlTagName, IHasAttr_data, IHasAttr_type, IHasAttr_typemustmatch, IHasAttr_name, IHasAttr_usemap, IHasAttr_form, IHasAttr_width, IHasAttr_height { public string TagName => "object"; }
        public struct OL : IHtmlTagName, IHasAttr_reversed, IHasAttr_start, IHasAttr_type { public string TagName => "ol"; }
        public struct OPTGROUP : IHtmlTagName, IHasAttr_disabled, IHasAttr_label { public string TagName => "optgroup"; }
        public struct OPTION : IHtmlTagName, IHasAttr_disabled, IHasAttr_label, IHasAttr_selected, IHasAttr_value { public string TagName => "option"; }
        public struct OUTPUT : IHtmlTagName, IHasAttr_for, IHasAttr_form, IHasAttr_name { public string TagName => "output"; }
        public struct P : IHtmlTagName { public string TagName => "p"; }
        public struct PARAM : IHtmlTagName, IHasAttr_name, IHasAttr_value { public string TagName => "param"; }
        public struct PICTURE : IHtmlTagName { public string TagName => "picture"; }
        public struct PRE : IHtmlTagName { public string TagName => "pre"; }
        public struct PROGRESS : IHtmlTagName, IHasAttr_value, IHasAttr_max { public string TagName => "progress"; }
        public struct Q : IHtmlTagName, IHasAttr_cite { public string TagName => "q"; }
        public struct RP : IHtmlTagName { public string TagName => "rp"; }
        public struct RT : IHtmlTagName { public string TagName => "rt"; }
        public struct RUBY : IHtmlTagName { public string TagName => "ruby"; }
        public struct S : IHtmlTagName { public string TagName => "s"; }
        public struct SAMP : IHtmlTagName { public string TagName => "samp"; }
        public struct SCRIPT : IHtmlTagName, IHasAttr_src, IHasAttr_type, IHasAttr_charset, IHasAttr_async, IHasAttr_defer, IHasAttr_crossorigin, IHasAttr_nonce { public string TagName => "script"; }
        public struct SECTION : IHtmlTagName { public string TagName => "section"; }
        public struct SELECT : IHtmlTagName, IHasAttr_autocomplete, IHasAttr_autofocus, IHasAttr_disabled, IHasAttr_form, IHasAttr_multiple, IHasAttr_name, IHasAttr_required, IHasAttr_size { public string TagName => "select"; }
        public struct SMALL : IHtmlTagName { public string TagName => "small"; }
        public struct SOURCE : IHtmlTagName, IHasAttr_src, IHasAttr_type, IHasAttr_srcset, IHasAttr_sizes, IHasAttr_media { public string TagName => "source"; }
        public struct SPAN : IHtmlTagName { public string TagName => "span"; }
        public struct STRONG : IHtmlTagName { public string TagName => "strong"; }
        public struct STYLE : IHtmlTagName, IHasAttr_media, IHasAttr_nonce, IHasAttr_type, IHasAttr_scoped { public string TagName => "style"; }
        public struct SUB : IHtmlTagName { public string TagName => "sub"; }
        public struct SUMMARY : IHtmlTagName { public string TagName => "summary"; }
        public struct SUP : IHtmlTagName { public string TagName => "sup"; }
        public struct SVG : IHtmlTagName { public string TagName => "svg"; }
        public struct TABLE : IHtmlTagName { public string TagName => "table"; }
        public struct TBODY : IHtmlTagName { public string TagName => "tbody"; }
        public struct TD : IHtmlTagName, IHasAttr_colspan, IHasAttr_rowspan, IHasAttr_headers { public string TagName => "td"; }
        public struct TEMPLATE : IHtmlTagName { public string TagName => "template"; }
        public struct TEXTAREA : IHtmlTagName, IHasAttr_autofocus, IHasAttr_cols, IHasAttr_dirname, IHasAttr_disabled, IHasAttr_form, IHasAttr_inputmode, IHasAttr_maxlength, IHasAttr_minlength, IHasAttr_name, IHasAttr_placeholder, IHasAttr_readonly, IHasAttr_required, IHasAttr_rows, IHasAttr_wrap { public string TagName => "textarea"; }
        public struct TFOOT : IHtmlTagName { public string TagName => "tfoot"; }
        public struct TH : IHtmlTagName, IHasAttr_colspan, IHasAttr_rowspan, IHasAttr_headers, IHasAttr_scope, IHasAttr_abbr { public string TagName => "th"; }
        public struct THEAD : IHtmlTagName { public string TagName => "thead"; }
        public struct TIME : IHtmlTagName, IHasAttr_datetime { public string TagName => "time"; }
        public struct TITLE : IHtmlTagName { public string TagName => "title"; }
        public struct TR : IHtmlTagName { public string TagName => "tr"; }
        public struct TRACK : IHtmlTagName, IHasAttr_default, IHasAttr_kind, IHasAttr_label, IHasAttr_src, IHasAttr_srclang { public string TagName => "track"; }
        public struct U : IHtmlTagName { public string TagName => "u"; }
        public struct UL : IHtmlTagName { public string TagName => "ul"; }
        public struct VAR : IHtmlTagName { public string TagName => "var"; }
        public struct VIDEO : IHtmlTagName, IHasAttr_src, IHasAttr_crossorigin, IHasAttr_poster, IHasAttr_preload, IHasAttr_autoplay, IHasAttr_mediagroup, IHasAttr_loop, IHasAttr_muted, IHasAttr_controls, IHasAttr_width, IHasAttr_height { public string TagName => "video"; }
        public struct WBR : IHtmlTagName { public string TagName => "wbr"; }
    }

    public static class Tags
    {

        ///<summary>Hyperlink. See: <a href="https://html.spec.whatwg.org/#the-a-element">https://html.spec.whatwg.org/#the-a-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.A> _a = new HtmlTag<TagNames.A>();

        ///<summary>Abbreviation. See: <a href="https://html.spec.whatwg.org/#the-abbr-element">https://html.spec.whatwg.org/#the-abbr-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.ABBR> _abbr = new HtmlTag<TagNames.ABBR>();

        ///<summary>Contact information for a page or article element. See: <a href="https://html.spec.whatwg.org/#the-address-element">https://html.spec.whatwg.org/#the-address-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.ADDRESS> _address = new HtmlTag<TagNames.ADDRESS>();

        ///<summary>Hyperlink or dead area on an image map. See: <a href="https://html.spec.whatwg.org/#the-area-element">https://html.spec.whatwg.org/#the-area-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.AREA> _area = new HtmlTag<TagNames.AREA>();

        ///<summary>Self-contained syndicatable or reusable composition. See: <a href="https://html.spec.whatwg.org/#the-article-element">https://html.spec.whatwg.org/#the-article-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.ARTICLE> _article = new HtmlTag<TagNames.ARTICLE>();

        ///<summary>Sidebar for tangentially related content. See: <a href="https://html.spec.whatwg.org/#the-aside-element">https://html.spec.whatwg.org/#the-aside-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.ASIDE> _aside = new HtmlTag<TagNames.ASIDE>();

        ///<summary>Audio player. See: <a href="https://html.spec.whatwg.org/#the-audio-element">https://html.spec.whatwg.org/#the-audio-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.AUDIO> _audio = new HtmlTag<TagNames.AUDIO>();

        ///<summary>Keywords. See: <a href="https://html.spec.whatwg.org/#the-b-element">https://html.spec.whatwg.org/#the-b-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.B> _b = new HtmlTag<TagNames.B>();

        ///<summary>Base URL and default target browsing context for hyperlinks and forms. See: <a href="https://html.spec.whatwg.org/#the-base-element">https://html.spec.whatwg.org/#the-base-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.BASE> _base = new HtmlTag<TagNames.BASE>();

        ///<summary>Text directionality isolation. See: <a href="https://html.spec.whatwg.org/#the-bdi-element">https://html.spec.whatwg.org/#the-bdi-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.BDI> _bdi = new HtmlTag<TagNames.BDI>();

        ///<summary>Text directionality formatting. See: <a href="https://html.spec.whatwg.org/#the-bdo-element">https://html.spec.whatwg.org/#the-bdo-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.BDO> _bdo = new HtmlTag<TagNames.BDO>();

        ///<summary>A section quoted from another source. See: <a href="https://html.spec.whatwg.org/#the-blockquote-element">https://html.spec.whatwg.org/#the-blockquote-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.BLOCKQUOTE> _blockquote = new HtmlTag<TagNames.BLOCKQUOTE>();

        ///<summary>Document body. See: <a href="https://html.spec.whatwg.org/#the-body-element">https://html.spec.whatwg.org/#the-body-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.BODY> _body = new HtmlTag<TagNames.BODY>();

        ///<summary>Line break, e.g. in poem or postal address. See: <a href="https://html.spec.whatwg.org/#the-br-element">https://html.spec.whatwg.org/#the-br-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.BR> _br = new HtmlTag<TagNames.BR>();

        ///<summary>Button control. See: <a href="https://html.spec.whatwg.org/#the-button-element">https://html.spec.whatwg.org/#the-button-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.BUTTON> _button = new HtmlTag<TagNames.BUTTON>();

        ///<summary>Scriptable bitmap canvas. See: <a href="https://html.spec.whatwg.org/#the-canvas-element">https://html.spec.whatwg.org/#the-canvas-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.CANVAS> _canvas = new HtmlTag<TagNames.CANVAS>();

        ///<summary>Table caption. See: <a href="https://html.spec.whatwg.org/#the-caption-element">https://html.spec.whatwg.org/#the-caption-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.CAPTION> _caption = new HtmlTag<TagNames.CAPTION>();

        ///<summary>Title of a work. See: <a href="https://html.spec.whatwg.org/#the-cite-element">https://html.spec.whatwg.org/#the-cite-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.CITE> _cite = new HtmlTag<TagNames.CITE>();

        ///<summary>Computer code. See: <a href="https://html.spec.whatwg.org/#the-code-element">https://html.spec.whatwg.org/#the-code-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.CODE> _code = new HtmlTag<TagNames.CODE>();

        ///<summary>Table column. See: <a href="https://html.spec.whatwg.org/#the-col-element">https://html.spec.whatwg.org/#the-col-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.COL> _col = new HtmlTag<TagNames.COL>();

        ///<summary>Group of columns in a table. See: <a href="https://html.spec.whatwg.org/#the-colgroup-element">https://html.spec.whatwg.org/#the-colgroup-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.COLGROUP> _colgroup = new HtmlTag<TagNames.COLGROUP>();

        ///<summary>Machine-readable equivalent. See: <a href="https://html.spec.whatwg.org/#the-data-element">https://html.spec.whatwg.org/#the-data-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.DATA> _data = new HtmlTag<TagNames.DATA>();

        ///<summary>Container for options for combo box control. See: <a href="https://html.spec.whatwg.org/#the-datalist-element">https://html.spec.whatwg.org/#the-datalist-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.DATALIST> _datalist = new HtmlTag<TagNames.DATALIST>();

        ///<summary>Content for corresponding dt element(s). See: <a href="https://html.spec.whatwg.org/#the-dd-element">https://html.spec.whatwg.org/#the-dd-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.DD> _dd = new HtmlTag<TagNames.DD>();

        ///<summary>A removal from the document. See: <a href="https://html.spec.whatwg.org/#the-del-element">https://html.spec.whatwg.org/#the-del-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.DEL> _del = new HtmlTag<TagNames.DEL>();

        ///<summary>Disclosure control for hiding details. See: <a href="https://html.spec.whatwg.org/#the-details-element">https://html.spec.whatwg.org/#the-details-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.DETAILS> _details = new HtmlTag<TagNames.DETAILS>();

        ///<summary>Defining instance. See: <a href="https://html.spec.whatwg.org/#the-dfn-element">https://html.spec.whatwg.org/#the-dfn-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.DFN> _dfn = new HtmlTag<TagNames.DFN>();

        ///<summary>Dialog box or window. See: <a href="https://html.spec.whatwg.org/#the-dialog-element">https://html.spec.whatwg.org/#the-dialog-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.DIALOG> _dialog = new HtmlTag<TagNames.DIALOG>();

        ///<summary>Generic flow container. See: <a href="https://html.spec.whatwg.org/#the-div-element">https://html.spec.whatwg.org/#the-div-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.DIV> _div = new HtmlTag<TagNames.DIV>();

        ///<summary>Association list consisting of zero or more name-value groups. See: <a href="https://html.spec.whatwg.org/#the-dl-element">https://html.spec.whatwg.org/#the-dl-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.DL> _dl = new HtmlTag<TagNames.DL>();

        ///<summary>Legend for corresponding dd element(s). See: <a href="https://html.spec.whatwg.org/#the-dt-element">https://html.spec.whatwg.org/#the-dt-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.DT> _dt = new HtmlTag<TagNames.DT>();

        ///<summary>Stress emphasis. See: <a href="https://html.spec.whatwg.org/#the-em-element">https://html.spec.whatwg.org/#the-em-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.EM> _em = new HtmlTag<TagNames.EM>();

        ///<summary>Plugin. See: <a href="https://html.spec.whatwg.org/#the-embed-element">https://html.spec.whatwg.org/#the-embed-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.EMBED> _embed = new HtmlTag<TagNames.EMBED>();

        ///<summary>Group of form controls. See: <a href="https://html.spec.whatwg.org/#the-fieldset-element">https://html.spec.whatwg.org/#the-fieldset-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.FIELDSET> _fieldset = new HtmlTag<TagNames.FIELDSET>();

        ///<summary>Caption for figure. See: <a href="https://html.spec.whatwg.org/#the-figcaption-element">https://html.spec.whatwg.org/#the-figcaption-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.FIGCAPTION> _figcaption = new HtmlTag<TagNames.FIGCAPTION>();

        ///<summary>Figure with optional caption. See: <a href="https://html.spec.whatwg.org/#the-figure-element">https://html.spec.whatwg.org/#the-figure-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.FIGURE> _figure = new HtmlTag<TagNames.FIGURE>();

        ///<summary>Footer for a page or section. See: <a href="https://html.spec.whatwg.org/#the-footer-element">https://html.spec.whatwg.org/#the-footer-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.FOOTER> _footer = new HtmlTag<TagNames.FOOTER>();

        ///<summary>User-submittable form. See: <a href="https://html.spec.whatwg.org/#the-form-element">https://html.spec.whatwg.org/#the-form-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.FORM> _form = new HtmlTag<TagNames.FORM>();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlTag<TagNames.H1> _h1 = new HtmlTag<TagNames.H1>();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlTag<TagNames.H2> _h2 = new HtmlTag<TagNames.H2>();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlTag<TagNames.H3> _h3 = new HtmlTag<TagNames.H3>();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlTag<TagNames.H4> _h4 = new HtmlTag<TagNames.H4>();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlTag<TagNames.H5> _h5 = new HtmlTag<TagNames.H5>();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlTag<TagNames.H6> _h6 = new HtmlTag<TagNames.H6>();

        ///<summary>Container for document metadata. See: <a href="https://html.spec.whatwg.org/#the-head-element">https://html.spec.whatwg.org/#the-head-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.HEAD> _head = new HtmlTag<TagNames.HEAD>();

        ///<summary>Introductory or navigational aids for a page or section. See: <a href="https://html.spec.whatwg.org/#the-header-element">https://html.spec.whatwg.org/#the-header-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.HEADER> _header = new HtmlTag<TagNames.HEADER>();

        ///<summary>heading group. See: <a href="https://html.spec.whatwg.org/#the-hgroup-element">https://html.spec.whatwg.org/#the-hgroup-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.HGROUP> _hgroup = new HtmlTag<TagNames.HGROUP>();

        ///<summary>Thematic break. See: <a href="https://html.spec.whatwg.org/#the-hr-element">https://html.spec.whatwg.org/#the-hr-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.HR> _hr = new HtmlTag<TagNames.HR>();

        ///<summary>Root element. See: <a href="https://html.spec.whatwg.org/#the-html-element">https://html.spec.whatwg.org/#the-html-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.HTML> _html = new HtmlTag<TagNames.HTML>();

        ///<summary>Alternate voice. See: <a href="https://html.spec.whatwg.org/#the-i-element">https://html.spec.whatwg.org/#the-i-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.I> _i = new HtmlTag<TagNames.I>();

        ///<summary>Nested browsing context. See: <a href="https://html.spec.whatwg.org/#the-iframe-element">https://html.spec.whatwg.org/#the-iframe-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.IFRAME> _iframe = new HtmlTag<TagNames.IFRAME>();

        ///<summary>Image. See: <a href="https://html.spec.whatwg.org/#the-img-element">https://html.spec.whatwg.org/#the-img-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.IMG> _img = new HtmlTag<TagNames.IMG>();

        ///<summary>Form control. See: <a href="https://html.spec.whatwg.org/#the-input-element">https://html.spec.whatwg.org/#the-input-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.INPUT> _input = new HtmlTag<TagNames.INPUT>();

        ///<summary>An addition to the document. See: <a href="https://html.spec.whatwg.org/#the-ins-element">https://html.spec.whatwg.org/#the-ins-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.INS> _ins = new HtmlTag<TagNames.INS>();

        ///<summary>User input. See: <a href="https://html.spec.whatwg.org/#the-kbd-element">https://html.spec.whatwg.org/#the-kbd-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.KBD> _kbd = new HtmlTag<TagNames.KBD>();

        ///<summary>Cryptographic key-pair generator form control. See: <a href="https://html.spec.whatwg.org/#the-keygen-element">https://html.spec.whatwg.org/#the-keygen-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.KEYGEN> _keygen = new HtmlTag<TagNames.KEYGEN>();

        ///<summary>Caption for a form control. See: <a href="https://html.spec.whatwg.org/#the-label-element">https://html.spec.whatwg.org/#the-label-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.LABEL> _label = new HtmlTag<TagNames.LABEL>();

        ///<summary>Caption for fieldset. See: <a href="https://html.spec.whatwg.org/#the-legend-element">https://html.spec.whatwg.org/#the-legend-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.LEGEND> _legend = new HtmlTag<TagNames.LEGEND>();

        ///<summary>List item. See: <a href="https://html.spec.whatwg.org/#the-li-element">https://html.spec.whatwg.org/#the-li-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.LI> _li = new HtmlTag<TagNames.LI>();

        ///<summary>Link metadata. See: <a href="https://html.spec.whatwg.org/#the-link-element">https://html.spec.whatwg.org/#the-link-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.LINK> _link = new HtmlTag<TagNames.LINK>();

        ///<summary>Container for the dominant contents of another element. See: <a href="https://html.spec.whatwg.org/#the-main-element">https://html.spec.whatwg.org/#the-main-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.MAIN> _main = new HtmlTag<TagNames.MAIN>();

        ///<summary>Image map. See: <a href="https://html.spec.whatwg.org/#the-map-element">https://html.spec.whatwg.org/#the-map-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.MAP> _map = new HtmlTag<TagNames.MAP>();

        ///<summary>Highlight. See: <a href="https://html.spec.whatwg.org/#the-mark-element">https://html.spec.whatwg.org/#the-mark-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.MARK> _mark = new HtmlTag<TagNames.MARK>();

        ///<summary>MathML root. See: <a href="https://html.spec.whatwg.org/#math">https://html.spec.whatwg.org/#math</a><br /></summary>
        public static readonly HtmlTag<TagNames.MATH> _math = new HtmlTag<TagNames.MATH>();

        ///<summary>Menu of commands. See: <a href="https://html.spec.whatwg.org/#the-menu-element">https://html.spec.whatwg.org/#the-menu-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.MENU> _menu = new HtmlTag<TagNames.MENU>();

        ///<summary>Menu command. See: <a href="https://html.spec.whatwg.org/#the-menuitem-element">https://html.spec.whatwg.org/#the-menuitem-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.MENUITEM> _menuitem = new HtmlTag<TagNames.MENUITEM>();

        ///<summary>Text metadata. See: <a href="https://html.spec.whatwg.org/#the-meta-element">https://html.spec.whatwg.org/#the-meta-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.META> _meta = new HtmlTag<TagNames.META>();

        ///<summary>Gauge. See: <a href="https://html.spec.whatwg.org/#the-meter-element">https://html.spec.whatwg.org/#the-meter-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.METER> _meter = new HtmlTag<TagNames.METER>();

        ///<summary>Section with navigational links. See: <a href="https://html.spec.whatwg.org/#the-nav-element">https://html.spec.whatwg.org/#the-nav-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.NAV> _nav = new HtmlTag<TagNames.NAV>();

        ///<summary>Fallback content for script. See: <a href="https://html.spec.whatwg.org/#the-noscript-element">https://html.spec.whatwg.org/#the-noscript-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.NOSCRIPT> _noscript = new HtmlTag<TagNames.NOSCRIPT>();

        ///<summary>Image, nested browsing context, or plugin. See: <a href="https://html.spec.whatwg.org/#the-object-element">https://html.spec.whatwg.org/#the-object-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.OBJECT> _object = new HtmlTag<TagNames.OBJECT>();

        ///<summary>Ordered list. See: <a href="https://html.spec.whatwg.org/#the-ol-element">https://html.spec.whatwg.org/#the-ol-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.OL> _ol = new HtmlTag<TagNames.OL>();

        ///<summary>Group of options in a list box. See: <a href="https://html.spec.whatwg.org/#the-optgroup-element">https://html.spec.whatwg.org/#the-optgroup-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.OPTGROUP> _optgroup = new HtmlTag<TagNames.OPTGROUP>();

        ///<summary>Option in a list box or combo box control. See: <a href="https://html.spec.whatwg.org/#the-option-element">https://html.spec.whatwg.org/#the-option-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.OPTION> _option = new HtmlTag<TagNames.OPTION>();

        ///<summary>Calculated output value. See: <a href="https://html.spec.whatwg.org/#the-output-element">https://html.spec.whatwg.org/#the-output-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.OUTPUT> _output = new HtmlTag<TagNames.OUTPUT>();

        ///<summary>Paragraph. See: <a href="https://html.spec.whatwg.org/#the-p-element">https://html.spec.whatwg.org/#the-p-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.P> _p = new HtmlTag<TagNames.P>();

        ///<summary>Parameter for object. See: <a href="https://html.spec.whatwg.org/#the-param-element">https://html.spec.whatwg.org/#the-param-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.PARAM> _param = new HtmlTag<TagNames.PARAM>();

        ///<summary>Image. See: <a href="https://html.spec.whatwg.org/#the-picture-element">https://html.spec.whatwg.org/#the-picture-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.PICTURE> _picture = new HtmlTag<TagNames.PICTURE>();

        ///<summary>Block of preformatted text. See: <a href="https://html.spec.whatwg.org/#the-pre-element">https://html.spec.whatwg.org/#the-pre-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.PRE> _pre = new HtmlTag<TagNames.PRE>();

        ///<summary>Progress bar. See: <a href="https://html.spec.whatwg.org/#the-progress-element">https://html.spec.whatwg.org/#the-progress-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.PROGRESS> _progress = new HtmlTag<TagNames.PROGRESS>();

        ///<summary>Quotation. See: <a href="https://html.spec.whatwg.org/#the-q-element">https://html.spec.whatwg.org/#the-q-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.Q> _q = new HtmlTag<TagNames.Q>();

        ///<summary>Parenthesis for ruby annotation text. See: <a href="https://html.spec.whatwg.org/#the-rp-element">https://html.spec.whatwg.org/#the-rp-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.RP> _rp = new HtmlTag<TagNames.RP>();

        ///<summary>Ruby annotation text. See: <a href="https://html.spec.whatwg.org/#the-rt-element">https://html.spec.whatwg.org/#the-rt-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.RT> _rt = new HtmlTag<TagNames.RT>();

        ///<summary>Ruby annotation(s). See: <a href="https://html.spec.whatwg.org/#the-ruby-element">https://html.spec.whatwg.org/#the-ruby-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.RUBY> _ruby = new HtmlTag<TagNames.RUBY>();

        ///<summary>Inaccurate text. See: <a href="https://html.spec.whatwg.org/#the-s-element">https://html.spec.whatwg.org/#the-s-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.S> _s = new HtmlTag<TagNames.S>();

        ///<summary>Computer output. See: <a href="https://html.spec.whatwg.org/#the-samp-element">https://html.spec.whatwg.org/#the-samp-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.SAMP> _samp = new HtmlTag<TagNames.SAMP>();

        ///<summary>Embedded script. See: <a href="https://html.spec.whatwg.org/#the-script-element">https://html.spec.whatwg.org/#the-script-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.SCRIPT> _script = new HtmlTag<TagNames.SCRIPT>();

        ///<summary>Generic document or application section. See: <a href="https://html.spec.whatwg.org/#the-section-element">https://html.spec.whatwg.org/#the-section-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.SECTION> _section = new HtmlTag<TagNames.SECTION>();

        ///<summary>List box control. See: <a href="https://html.spec.whatwg.org/#the-select-element">https://html.spec.whatwg.org/#the-select-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.SELECT> _select = new HtmlTag<TagNames.SELECT>();

        ///<summary>Side comment. See: <a href="https://html.spec.whatwg.org/#the-small-element">https://html.spec.whatwg.org/#the-small-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.SMALL> _small = new HtmlTag<TagNames.SMALL>();

        ///<summary>Media source for video or audio. See: <a href="https://html.spec.whatwg.org/#the-source-element">https://html.spec.whatwg.org/#the-source-element</a><br />Image source for an img. See: <a href="https://html.spec.whatwg.org/#the-source-element-when-used-with-the-picture-element">https://html.spec.whatwg.org/#the-source-element-when-used-with-the-picture-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.SOURCE> _source = new HtmlTag<TagNames.SOURCE>();

        ///<summary>Generic phrasing container. See: <a href="https://html.spec.whatwg.org/#the-span-element">https://html.spec.whatwg.org/#the-span-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.SPAN> _span = new HtmlTag<TagNames.SPAN>();

        ///<summary>Importance. See: <a href="https://html.spec.whatwg.org/#the-strong-element">https://html.spec.whatwg.org/#the-strong-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.STRONG> _strong = new HtmlTag<TagNames.STRONG>();

        ///<summary>Embedded styling information. See: <a href="https://html.spec.whatwg.org/#the-style-element">https://html.spec.whatwg.org/#the-style-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.STYLE> _style = new HtmlTag<TagNames.STYLE>();

        ///<summary>Subscript. See: <a href="https://html.spec.whatwg.org/#the-sub-and-sup-elements">https://html.spec.whatwg.org/#the-sub-and-sup-elements</a><br /></summary>
        public static readonly HtmlTag<TagNames.SUB> _sub = new HtmlTag<TagNames.SUB>();

        ///<summary>Caption for details. See: <a href="https://html.spec.whatwg.org/#the-summary-element">https://html.spec.whatwg.org/#the-summary-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.SUMMARY> _summary = new HtmlTag<TagNames.SUMMARY>();

        ///<summary>Superscript. See: <a href="https://html.spec.whatwg.org/#the-sub-and-sup-elements">https://html.spec.whatwg.org/#the-sub-and-sup-elements</a><br /></summary>
        public static readonly HtmlTag<TagNames.SUP> _sup = new HtmlTag<TagNames.SUP>();

        ///<summary>SVG root. See: <a href="https://html.spec.whatwg.org/#svg">https://html.spec.whatwg.org/#svg</a><br /></summary>
        public static readonly HtmlTag<TagNames.SVG> _svg = new HtmlTag<TagNames.SVG>();

        ///<summary>Table. See: <a href="https://html.spec.whatwg.org/#the-table-element">https://html.spec.whatwg.org/#the-table-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.TABLE> _table = new HtmlTag<TagNames.TABLE>();

        ///<summary>Group of rows in a table. See: <a href="https://html.spec.whatwg.org/#the-tbody-element">https://html.spec.whatwg.org/#the-tbody-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.TBODY> _tbody = new HtmlTag<TagNames.TBODY>();

        ///<summary>Table cell. See: <a href="https://html.spec.whatwg.org/#the-td-element">https://html.spec.whatwg.org/#the-td-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.TD> _td = new HtmlTag<TagNames.TD>();

        ///<summary>Template. See: <a href="https://html.spec.whatwg.org/#the-template-element">https://html.spec.whatwg.org/#the-template-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.TEMPLATE> _template = new HtmlTag<TagNames.TEMPLATE>();

        ///<summary>Multiline text field. See: <a href="https://html.spec.whatwg.org/#the-textarea-element">https://html.spec.whatwg.org/#the-textarea-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.TEXTAREA> _textarea = new HtmlTag<TagNames.TEXTAREA>();

        ///<summary>Group of footer rows in a table. See: <a href="https://html.spec.whatwg.org/#the-tfoot-element">https://html.spec.whatwg.org/#the-tfoot-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.TFOOT> _tfoot = new HtmlTag<TagNames.TFOOT>();

        ///<summary>Table header cell. See: <a href="https://html.spec.whatwg.org/#the-th-element">https://html.spec.whatwg.org/#the-th-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.TH> _th = new HtmlTag<TagNames.TH>();

        ///<summary>Group of heading rows in a table. See: <a href="https://html.spec.whatwg.org/#the-thead-element">https://html.spec.whatwg.org/#the-thead-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.THEAD> _thead = new HtmlTag<TagNames.THEAD>();

        ///<summary>Machine-readable equivalent of date- or time-related data. See: <a href="https://html.spec.whatwg.org/#the-time-element">https://html.spec.whatwg.org/#the-time-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.TIME> _time = new HtmlTag<TagNames.TIME>();

        ///<summary>Document title. See: <a href="https://html.spec.whatwg.org/#the-title-element">https://html.spec.whatwg.org/#the-title-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.TITLE> _title = new HtmlTag<TagNames.TITLE>();

        ///<summary>Table row. See: <a href="https://html.spec.whatwg.org/#the-tr-element">https://html.spec.whatwg.org/#the-tr-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.TR> _tr = new HtmlTag<TagNames.TR>();

        ///<summary>Timed text track. See: <a href="https://html.spec.whatwg.org/#the-track-element">https://html.spec.whatwg.org/#the-track-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.TRACK> _track = new HtmlTag<TagNames.TRACK>();

        ///<summary>Keywords. See: <a href="https://html.spec.whatwg.org/#the-u-element">https://html.spec.whatwg.org/#the-u-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.U> _u = new HtmlTag<TagNames.U>();

        ///<summary>List. See: <a href="https://html.spec.whatwg.org/#the-ul-element">https://html.spec.whatwg.org/#the-ul-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.UL> _ul = new HtmlTag<TagNames.UL>();

        ///<summary>Variable. See: <a href="https://html.spec.whatwg.org/#the-var-element">https://html.spec.whatwg.org/#the-var-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.VAR> _var = new HtmlTag<TagNames.VAR>();

        ///<summary>Video player. See: <a href="https://html.spec.whatwg.org/#the-video-element">https://html.spec.whatwg.org/#the-video-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.VIDEO> _video = new HtmlTag<TagNames.VIDEO>();

        ///<summary>Line breaking opportunity. See: <a href="https://html.spec.whatwg.org/#the-wbr-element">https://html.spec.whatwg.org/#the-wbr-element</a><br /></summary>
        public static readonly HtmlTag<TagNames.WBR> _wbr = new HtmlTag<TagNames.WBR>();
    }
}


