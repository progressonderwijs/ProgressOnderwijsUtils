﻿namespace ProgressOnderwijsUtils.Html
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
        public static TExpression with_accesskey<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("accesskey", attrValue);
        public static TExpression with_class<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("class", attrValue);
        public static TExpression with_contenteditable<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("contenteditable", attrValue);
        public static TExpression with_contextmenu<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("contextmenu", attrValue);
        public static TExpression with_dir<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("dir", attrValue);
        public static TExpression with_draggable<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("draggable", attrValue);
        public static TExpression with_dropzone<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("dropzone", attrValue);
        public static TExpression with_hidden<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("hidden", attrValue);
        public static TExpression with_id<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("id", attrValue);
        public static TExpression with_itemid<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("itemid", attrValue);
        public static TExpression with_itemprop<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("itemprop", attrValue);
        public static TExpression with_itemref<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("itemref", attrValue);
        public static TExpression with_itemscope<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("itemscope", attrValue);
        public static TExpression with_itemtype<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("itemtype", attrValue);
        public static TExpression with_lang<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("lang", attrValue);
        public static TExpression with_spellcheck<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("spellcheck", attrValue);
        public static TExpression with_style<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("style", attrValue);
        public static TExpression with_tabindex<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("tabindex", attrValue);
        public static TExpression with_title<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("title", attrValue);
        public static TExpression with_translate<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("translate", attrValue);
        public static HtmlStartTag<TTagType> with_href<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_href, IHtmlTagName
            => htmlTagExpr.withAttribute("href", attrValue);
        public static HtmlStartTag<TTagType> with_target<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_target, IHtmlTagName
            => htmlTagExpr.withAttribute("target", attrValue);
        public static HtmlStartTag<TTagType> with_download<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_download, IHtmlTagName
            => htmlTagExpr.withAttribute("download", attrValue);
        public static HtmlStartTag<TTagType> with_ping<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_ping, IHtmlTagName
            => htmlTagExpr.withAttribute("ping", attrValue);
        public static HtmlStartTag<TTagType> with_rel<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_rel, IHtmlTagName
            => htmlTagExpr.withAttribute("rel", attrValue);
        public static HtmlStartTag<TTagType> with_hreflang<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_hreflang, IHtmlTagName
            => htmlTagExpr.withAttribute("hreflang", attrValue);
        public static HtmlStartTag<TTagType> with_type<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_type, IHtmlTagName
            => htmlTagExpr.withAttribute("type", attrValue);
        public static HtmlStartTag<TTagType> with_alt<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_alt, IHtmlTagName
            => htmlTagExpr.withAttribute("alt", attrValue);
        public static HtmlStartTag<TTagType> with_coords<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_coords, IHtmlTagName
            => htmlTagExpr.withAttribute("coords", attrValue);
        public static HtmlStartTag<TTagType> with_shape<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_shape, IHtmlTagName
            => htmlTagExpr.withAttribute("shape", attrValue);
        public static HtmlStartTag<TTagType> with_src<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_src, IHtmlTagName
            => htmlTagExpr.withAttribute("src", attrValue);
        public static HtmlStartTag<TTagType> with_crossorigin<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_crossorigin, IHtmlTagName
            => htmlTagExpr.withAttribute("crossorigin", attrValue);
        public static HtmlStartTag<TTagType> with_preload<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_preload, IHtmlTagName
            => htmlTagExpr.withAttribute("preload", attrValue);
        public static HtmlStartTag<TTagType> with_autoplay<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_autoplay, IHtmlTagName
            => htmlTagExpr.withAttribute("autoplay", attrValue);
        public static HtmlStartTag<TTagType> with_mediagroup<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_mediagroup, IHtmlTagName
            => htmlTagExpr.withAttribute("mediagroup", attrValue);
        public static HtmlStartTag<TTagType> with_loop<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_loop, IHtmlTagName
            => htmlTagExpr.withAttribute("loop", attrValue);
        public static HtmlStartTag<TTagType> with_muted<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_muted, IHtmlTagName
            => htmlTagExpr.withAttribute("muted", attrValue);
        public static HtmlStartTag<TTagType> with_controls<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_controls, IHtmlTagName
            => htmlTagExpr.withAttribute("controls", attrValue);
        public static HtmlStartTag<TTagType> with_cite<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_cite, IHtmlTagName
            => htmlTagExpr.withAttribute("cite", attrValue);
        public static HtmlStartTag<TTagType> with_onafterprint<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_onafterprint, IHtmlTagName
            => htmlTagExpr.withAttribute("onafterprint", attrValue);
        public static HtmlStartTag<TTagType> with_onbeforeprint<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_onbeforeprint, IHtmlTagName
            => htmlTagExpr.withAttribute("onbeforeprint", attrValue);
        public static HtmlStartTag<TTagType> with_onbeforeunload<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_onbeforeunload, IHtmlTagName
            => htmlTagExpr.withAttribute("onbeforeunload", attrValue);
        public static HtmlStartTag<TTagType> with_onhashchange<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_onhashchange, IHtmlTagName
            => htmlTagExpr.withAttribute("onhashchange", attrValue);
        public static HtmlStartTag<TTagType> with_onlanguagechange<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_onlanguagechange, IHtmlTagName
            => htmlTagExpr.withAttribute("onlanguagechange", attrValue);
        public static HtmlStartTag<TTagType> with_onmessage<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_onmessage, IHtmlTagName
            => htmlTagExpr.withAttribute("onmessage", attrValue);
        public static HtmlStartTag<TTagType> with_onoffline<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_onoffline, IHtmlTagName
            => htmlTagExpr.withAttribute("onoffline", attrValue);
        public static HtmlStartTag<TTagType> with_ononline<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_ononline, IHtmlTagName
            => htmlTagExpr.withAttribute("ononline", attrValue);
        public static HtmlStartTag<TTagType> with_onpagehide<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_onpagehide, IHtmlTagName
            => htmlTagExpr.withAttribute("onpagehide", attrValue);
        public static HtmlStartTag<TTagType> with_onpageshow<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_onpageshow, IHtmlTagName
            => htmlTagExpr.withAttribute("onpageshow", attrValue);
        public static HtmlStartTag<TTagType> with_onpopstate<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_onpopstate, IHtmlTagName
            => htmlTagExpr.withAttribute("onpopstate", attrValue);
        public static HtmlStartTag<TTagType> with_onrejectionhandled<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_onrejectionhandled, IHtmlTagName
            => htmlTagExpr.withAttribute("onrejectionhandled", attrValue);
        public static HtmlStartTag<TTagType> with_onstorage<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_onstorage, IHtmlTagName
            => htmlTagExpr.withAttribute("onstorage", attrValue);
        public static HtmlStartTag<TTagType> with_onunhandledrejection<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_onunhandledrejection, IHtmlTagName
            => htmlTagExpr.withAttribute("onunhandledrejection", attrValue);
        public static HtmlStartTag<TTagType> with_onunload<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_onunload, IHtmlTagName
            => htmlTagExpr.withAttribute("onunload", attrValue);
        public static HtmlStartTag<TTagType> with_autofocus<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_autofocus, IHtmlTagName
            => htmlTagExpr.withAttribute("autofocus", attrValue);
        public static HtmlStartTag<TTagType> with_disabled<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_disabled, IHtmlTagName
            => htmlTagExpr.withAttribute("disabled", attrValue);
        public static HtmlStartTag<TTagType> with_form<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_form, IHtmlTagName
            => htmlTagExpr.withAttribute("form", attrValue);
        public static HtmlStartTag<TTagType> with_formaction<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_formaction, IHtmlTagName
            => htmlTagExpr.withAttribute("formaction", attrValue);
        public static HtmlStartTag<TTagType> with_formenctype<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_formenctype, IHtmlTagName
            => htmlTagExpr.withAttribute("formenctype", attrValue);
        public static HtmlStartTag<TTagType> with_formmethod<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_formmethod, IHtmlTagName
            => htmlTagExpr.withAttribute("formmethod", attrValue);
        public static HtmlStartTag<TTagType> with_formnovalidate<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_formnovalidate, IHtmlTagName
            => htmlTagExpr.withAttribute("formnovalidate", attrValue);
        public static HtmlStartTag<TTagType> with_formtarget<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_formtarget, IHtmlTagName
            => htmlTagExpr.withAttribute("formtarget", attrValue);
        public static HtmlStartTag<TTagType> with_menu<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_menu, IHtmlTagName
            => htmlTagExpr.withAttribute("menu", attrValue);
        public static HtmlStartTag<TTagType> with_name<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_name, IHtmlTagName
            => htmlTagExpr.withAttribute("name", attrValue);
        public static HtmlStartTag<TTagType> with_value<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_value, IHtmlTagName
            => htmlTagExpr.withAttribute("value", attrValue);
        public static HtmlStartTag<TTagType> with_width<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_width, IHtmlTagName
            => htmlTagExpr.withAttribute("width", attrValue);
        public static HtmlStartTag<TTagType> with_height<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_height, IHtmlTagName
            => htmlTagExpr.withAttribute("height", attrValue);
        public static HtmlStartTag<TTagType> with_span<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_span, IHtmlTagName
            => htmlTagExpr.withAttribute("span", attrValue);
        public static HtmlStartTag<TTagType> with_datetime<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_datetime, IHtmlTagName
            => htmlTagExpr.withAttribute("datetime", attrValue);
        public static HtmlStartTag<TTagType> with_open<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_open, IHtmlTagName
            => htmlTagExpr.withAttribute("open", attrValue);
        public static HtmlStartTag<TTagType> with_accept_charset<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_accept_charset, IHtmlTagName
            => htmlTagExpr.withAttribute("accept-charset", attrValue);
        public static HtmlStartTag<TTagType> with_action<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_action, IHtmlTagName
            => htmlTagExpr.withAttribute("action", attrValue);
        public static HtmlStartTag<TTagType> with_autocomplete<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_autocomplete, IHtmlTagName
            => htmlTagExpr.withAttribute("autocomplete", attrValue);
        public static HtmlStartTag<TTagType> with_enctype<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_enctype, IHtmlTagName
            => htmlTagExpr.withAttribute("enctype", attrValue);
        public static HtmlStartTag<TTagType> with_method<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_method, IHtmlTagName
            => htmlTagExpr.withAttribute("method", attrValue);
        public static HtmlStartTag<TTagType> with_novalidate<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_novalidate, IHtmlTagName
            => htmlTagExpr.withAttribute("novalidate", attrValue);
        public static HtmlStartTag<TTagType> with_manifest<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_manifest, IHtmlTagName
            => htmlTagExpr.withAttribute("manifest", attrValue);
        public static HtmlStartTag<TTagType> with_srcdoc<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_srcdoc, IHtmlTagName
            => htmlTagExpr.withAttribute("srcdoc", attrValue);
        public static HtmlStartTag<TTagType> with_sandbox<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_sandbox, IHtmlTagName
            => htmlTagExpr.withAttribute("sandbox", attrValue);
        public static HtmlStartTag<TTagType> with_allowfullscreen<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_allowfullscreen, IHtmlTagName
            => htmlTagExpr.withAttribute("allowfullscreen", attrValue);
        public static HtmlStartTag<TTagType> with_srcset<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_srcset, IHtmlTagName
            => htmlTagExpr.withAttribute("srcset", attrValue);
        public static HtmlStartTag<TTagType> with_usemap<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_usemap, IHtmlTagName
            => htmlTagExpr.withAttribute("usemap", attrValue);
        public static HtmlStartTag<TTagType> with_ismap<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_ismap, IHtmlTagName
            => htmlTagExpr.withAttribute("ismap", attrValue);
        public static HtmlStartTag<TTagType> with_accept<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_accept, IHtmlTagName
            => htmlTagExpr.withAttribute("accept", attrValue);
        public static HtmlStartTag<TTagType> with_checked<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_checked, IHtmlTagName
            => htmlTagExpr.withAttribute("checked", attrValue);
        public static HtmlStartTag<TTagType> with_dirname<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_dirname, IHtmlTagName
            => htmlTagExpr.withAttribute("dirname", attrValue);
        public static HtmlStartTag<TTagType> with_inputmode<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_inputmode, IHtmlTagName
            => htmlTagExpr.withAttribute("inputmode", attrValue);
        public static HtmlStartTag<TTagType> with_list<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_list, IHtmlTagName
            => htmlTagExpr.withAttribute("list", attrValue);
        public static HtmlStartTag<TTagType> with_max<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_max, IHtmlTagName
            => htmlTagExpr.withAttribute("max", attrValue);
        public static HtmlStartTag<TTagType> with_maxlength<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_maxlength, IHtmlTagName
            => htmlTagExpr.withAttribute("maxlength", attrValue);
        public static HtmlStartTag<TTagType> with_min<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_min, IHtmlTagName
            => htmlTagExpr.withAttribute("min", attrValue);
        public static HtmlStartTag<TTagType> with_minlength<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_minlength, IHtmlTagName
            => htmlTagExpr.withAttribute("minlength", attrValue);
        public static HtmlStartTag<TTagType> with_multiple<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_multiple, IHtmlTagName
            => htmlTagExpr.withAttribute("multiple", attrValue);
        public static HtmlStartTag<TTagType> with_pattern<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_pattern, IHtmlTagName
            => htmlTagExpr.withAttribute("pattern", attrValue);
        public static HtmlStartTag<TTagType> with_placeholder<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_placeholder, IHtmlTagName
            => htmlTagExpr.withAttribute("placeholder", attrValue);
        public static HtmlStartTag<TTagType> with_readonly<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_readonly, IHtmlTagName
            => htmlTagExpr.withAttribute("readonly", attrValue);
        public static HtmlStartTag<TTagType> with_required<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_required, IHtmlTagName
            => htmlTagExpr.withAttribute("required", attrValue);
        public static HtmlStartTag<TTagType> with_size<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_size, IHtmlTagName
            => htmlTagExpr.withAttribute("size", attrValue);
        public static HtmlStartTag<TTagType> with_step<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_step, IHtmlTagName
            => htmlTagExpr.withAttribute("step", attrValue);
        public static HtmlStartTag<TTagType> with_challenge<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_challenge, IHtmlTagName
            => htmlTagExpr.withAttribute("challenge", attrValue);
        public static HtmlStartTag<TTagType> with_keytype<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_keytype, IHtmlTagName
            => htmlTagExpr.withAttribute("keytype", attrValue);
        public static HtmlStartTag<TTagType> with_for<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_for, IHtmlTagName
            => htmlTagExpr.withAttribute("for", attrValue);
        public static HtmlStartTag<TTagType> with_media<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_media, IHtmlTagName
            => htmlTagExpr.withAttribute("media", attrValue);
        public static HtmlStartTag<TTagType> with_sizes<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_sizes, IHtmlTagName
            => htmlTagExpr.withAttribute("sizes", attrValue);
        public static HtmlStartTag<TTagType> with_label<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_label, IHtmlTagName
            => htmlTagExpr.withAttribute("label", attrValue);
        public static HtmlStartTag<TTagType> with_icon<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_icon, IHtmlTagName
            => htmlTagExpr.withAttribute("icon", attrValue);
        public static HtmlStartTag<TTagType> with_radiogroup<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_radiogroup, IHtmlTagName
            => htmlTagExpr.withAttribute("radiogroup", attrValue);
        public static HtmlStartTag<TTagType> with_default<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_default, IHtmlTagName
            => htmlTagExpr.withAttribute("default", attrValue);
        public static HtmlStartTag<TTagType> with_http_equiv<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_http_equiv, IHtmlTagName
            => htmlTagExpr.withAttribute("http-equiv", attrValue);
        public static HtmlStartTag<TTagType> with_content<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_content, IHtmlTagName
            => htmlTagExpr.withAttribute("content", attrValue);
        public static HtmlStartTag<TTagType> with_charset<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_charset, IHtmlTagName
            => htmlTagExpr.withAttribute("charset", attrValue);
        public static HtmlStartTag<TTagType> with_low<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_low, IHtmlTagName
            => htmlTagExpr.withAttribute("low", attrValue);
        public static HtmlStartTag<TTagType> with_high<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_high, IHtmlTagName
            => htmlTagExpr.withAttribute("high", attrValue);
        public static HtmlStartTag<TTagType> with_optimum<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_optimum, IHtmlTagName
            => htmlTagExpr.withAttribute("optimum", attrValue);
        public static HtmlStartTag<TTagType> with_data<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_data, IHtmlTagName
            => htmlTagExpr.withAttribute("data", attrValue);
        public static HtmlStartTag<TTagType> with_typemustmatch<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_typemustmatch, IHtmlTagName
            => htmlTagExpr.withAttribute("typemustmatch", attrValue);
        public static HtmlStartTag<TTagType> with_reversed<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_reversed, IHtmlTagName
            => htmlTagExpr.withAttribute("reversed", attrValue);
        public static HtmlStartTag<TTagType> with_start<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_start, IHtmlTagName
            => htmlTagExpr.withAttribute("start", attrValue);
        public static HtmlStartTag<TTagType> with_selected<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_selected, IHtmlTagName
            => htmlTagExpr.withAttribute("selected", attrValue);
        public static HtmlStartTag<TTagType> with_async<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_async, IHtmlTagName
            => htmlTagExpr.withAttribute("async", attrValue);
        public static HtmlStartTag<TTagType> with_defer<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_defer, IHtmlTagName
            => htmlTagExpr.withAttribute("defer", attrValue);
        public static HtmlStartTag<TTagType> with_nonce<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_nonce, IHtmlTagName
            => htmlTagExpr.withAttribute("nonce", attrValue);
        public static HtmlStartTag<TTagType> with_scoped<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_scoped, IHtmlTagName
            => htmlTagExpr.withAttribute("scoped", attrValue);
        public static HtmlStartTag<TTagType> with_colspan<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_colspan, IHtmlTagName
            => htmlTagExpr.withAttribute("colspan", attrValue);
        public static HtmlStartTag<TTagType> with_rowspan<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_rowspan, IHtmlTagName
            => htmlTagExpr.withAttribute("rowspan", attrValue);
        public static HtmlStartTag<TTagType> with_headers<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_headers, IHtmlTagName
            => htmlTagExpr.withAttribute("headers", attrValue);
        public static HtmlStartTag<TTagType> with_cols<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_cols, IHtmlTagName
            => htmlTagExpr.withAttribute("cols", attrValue);
        public static HtmlStartTag<TTagType> with_rows<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_rows, IHtmlTagName
            => htmlTagExpr.withAttribute("rows", attrValue);
        public static HtmlStartTag<TTagType> with_wrap<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_wrap, IHtmlTagName
            => htmlTagExpr.withAttribute("wrap", attrValue);
        public static HtmlStartTag<TTagType> with_scope<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_scope, IHtmlTagName
            => htmlTagExpr.withAttribute("scope", attrValue);
        public static HtmlStartTag<TTagType> with_abbr<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_abbr, IHtmlTagName
            => htmlTagExpr.withAttribute("abbr", attrValue);
        public static HtmlStartTag<TTagType> with_kind<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_kind, IHtmlTagName
            => htmlTagExpr.withAttribute("kind", attrValue);
        public static HtmlStartTag<TTagType> with_srclang<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_srclang, IHtmlTagName
            => htmlTagExpr.withAttribute("srclang", attrValue);
        public static HtmlStartTag<TTagType> with_poster<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_poster, IHtmlTagName
            => htmlTagExpr.withAttribute("poster", attrValue);
    }

    public static class TagNames
    {
        public struct @a : IHtmlTagName, IHasAttr_href, IHasAttr_target, IHasAttr_download, IHasAttr_ping, IHasAttr_rel, IHasAttr_hreflang, IHasAttr_type { public string TagName => "a"; }
        public struct @abbr : IHtmlTagName { public string TagName => "abbr"; }
        public struct @address : IHtmlTagName { public string TagName => "address"; }
        public struct @area : IHtmlTagName, IHasAttr_alt, IHasAttr_coords, IHasAttr_shape, IHasAttr_href, IHasAttr_target, IHasAttr_download, IHasAttr_ping, IHasAttr_rel { public string TagName => "area"; }
        public struct @article : IHtmlTagName { public string TagName => "article"; }
        public struct @aside : IHtmlTagName { public string TagName => "aside"; }
        public struct @audio : IHtmlTagName, IHasAttr_src, IHasAttr_crossorigin, IHasAttr_preload, IHasAttr_autoplay, IHasAttr_mediagroup, IHasAttr_loop, IHasAttr_muted, IHasAttr_controls { public string TagName => "audio"; }
        public struct @b : IHtmlTagName { public string TagName => "b"; }
        public struct @base : IHtmlTagName, IHasAttr_href, IHasAttr_target { public string TagName => "base"; }
        public struct @bdi : IHtmlTagName { public string TagName => "bdi"; }
        public struct @bdo : IHtmlTagName { public string TagName => "bdo"; }
        public struct @blockquote : IHtmlTagName, IHasAttr_cite { public string TagName => "blockquote"; }
        public struct @body : IHtmlTagName, IHasAttr_onafterprint, IHasAttr_onbeforeprint, IHasAttr_onbeforeunload, IHasAttr_onhashchange, IHasAttr_onlanguagechange, IHasAttr_onmessage, IHasAttr_onoffline, IHasAttr_ononline, IHasAttr_onpagehide, IHasAttr_onpageshow, IHasAttr_onpopstate, IHasAttr_onrejectionhandled, IHasAttr_onstorage, IHasAttr_onunhandledrejection, IHasAttr_onunload { public string TagName => "body"; }
        public struct @br : IHtmlTagName { public string TagName => "br"; }
        public struct @button : IHtmlTagName, IHasAttr_autofocus, IHasAttr_disabled, IHasAttr_form, IHasAttr_formaction, IHasAttr_formenctype, IHasAttr_formmethod, IHasAttr_formnovalidate, IHasAttr_formtarget, IHasAttr_menu, IHasAttr_name, IHasAttr_type, IHasAttr_value { public string TagName => "button"; }
        public struct @canvas : IHtmlTagName, IHasAttr_width, IHasAttr_height { public string TagName => "canvas"; }
        public struct @caption : IHtmlTagName { public string TagName => "caption"; }
        public struct @cite : IHtmlTagName { public string TagName => "cite"; }
        public struct @code : IHtmlTagName { public string TagName => "code"; }
        public struct @col : IHtmlTagName, IHasAttr_span { public string TagName => "col"; }
        public struct @colgroup : IHtmlTagName, IHasAttr_span { public string TagName => "colgroup"; }
        public struct @data : IHtmlTagName, IHasAttr_value { public string TagName => "data"; }
        public struct @datalist : IHtmlTagName { public string TagName => "datalist"; }
        public struct @dd : IHtmlTagName { public string TagName => "dd"; }
        public struct @del : IHtmlTagName, IHasAttr_cite, IHasAttr_datetime { public string TagName => "del"; }
        public struct @details : IHtmlTagName, IHasAttr_open { public string TagName => "details"; }
        public struct @dfn : IHtmlTagName { public string TagName => "dfn"; }
        public struct @dialog : IHtmlTagName, IHasAttr_open { public string TagName => "dialog"; }
        public struct @div : IHtmlTagName { public string TagName => "div"; }
        public struct @dl : IHtmlTagName { public string TagName => "dl"; }
        public struct @dt : IHtmlTagName { public string TagName => "dt"; }
        public struct @em : IHtmlTagName { public string TagName => "em"; }
        public struct @embed : IHtmlTagName, IHasAttr_src, IHasAttr_type, IHasAttr_width, IHasAttr_height { public string TagName => "embed"; }
        public struct @fieldset : IHtmlTagName, IHasAttr_disabled, IHasAttr_form, IHasAttr_name { public string TagName => "fieldset"; }
        public struct @figcaption : IHtmlTagName { public string TagName => "figcaption"; }
        public struct @figure : IHtmlTagName { public string TagName => "figure"; }
        public struct @footer : IHtmlTagName { public string TagName => "footer"; }
        public struct @form : IHtmlTagName, IHasAttr_accept_charset, IHasAttr_action, IHasAttr_autocomplete, IHasAttr_enctype, IHasAttr_method, IHasAttr_name, IHasAttr_novalidate, IHasAttr_target { public string TagName => "form"; }
        public struct @h1 : IHtmlTagName { public string TagName => "h1"; }
        public struct @h2 : IHtmlTagName { public string TagName => "h2"; }
        public struct @h3 : IHtmlTagName { public string TagName => "h3"; }
        public struct @h4 : IHtmlTagName { public string TagName => "h4"; }
        public struct @h5 : IHtmlTagName { public string TagName => "h5"; }
        public struct @h6 : IHtmlTagName { public string TagName => "h6"; }
        public struct @head : IHtmlTagName { public string TagName => "head"; }
        public struct @header : IHtmlTagName { public string TagName => "header"; }
        public struct @hgroup : IHtmlTagName { public string TagName => "hgroup"; }
        public struct @hr : IHtmlTagName { public string TagName => "hr"; }
        public struct @html : IHtmlTagName, IHasAttr_manifest { public string TagName => "html"; }
        public struct @i : IHtmlTagName { public string TagName => "i"; }
        public struct @iframe : IHtmlTagName, IHasAttr_src, IHasAttr_srcdoc, IHasAttr_name, IHasAttr_sandbox, IHasAttr_allowfullscreen, IHasAttr_width, IHasAttr_height { public string TagName => "iframe"; }
        public struct @img : IHtmlTagName, IHasAttr_alt, IHasAttr_src, IHasAttr_srcset, IHasAttr_crossorigin, IHasAttr_usemap, IHasAttr_ismap, IHasAttr_width, IHasAttr_height { public string TagName => "img"; }
        public struct @input : IHtmlTagName, IHasAttr_accept, IHasAttr_alt, IHasAttr_autocomplete, IHasAttr_autofocus, IHasAttr_checked, IHasAttr_dirname, IHasAttr_disabled, IHasAttr_form, IHasAttr_formaction, IHasAttr_formenctype, IHasAttr_formmethod, IHasAttr_formnovalidate, IHasAttr_formtarget, IHasAttr_height, IHasAttr_inputmode, IHasAttr_list, IHasAttr_max, IHasAttr_maxlength, IHasAttr_min, IHasAttr_minlength, IHasAttr_multiple, IHasAttr_name, IHasAttr_pattern, IHasAttr_placeholder, IHasAttr_readonly, IHasAttr_required, IHasAttr_size, IHasAttr_src, IHasAttr_step, IHasAttr_type, IHasAttr_value, IHasAttr_width { public string TagName => "input"; }
        public struct @ins : IHtmlTagName, IHasAttr_cite, IHasAttr_datetime { public string TagName => "ins"; }
        public struct @kbd : IHtmlTagName { public string TagName => "kbd"; }
        public struct @keygen : IHtmlTagName, IHasAttr_autofocus, IHasAttr_challenge, IHasAttr_disabled, IHasAttr_form, IHasAttr_keytype, IHasAttr_name { public string TagName => "keygen"; }
        public struct @label : IHtmlTagName, IHasAttr_form, IHasAttr_for { public string TagName => "label"; }
        public struct @legend : IHtmlTagName { public string TagName => "legend"; }
        public struct @li : IHtmlTagName, IHasAttr_value { public string TagName => "li"; }
        public struct @link : IHtmlTagName, IHasAttr_href, IHasAttr_crossorigin, IHasAttr_rel, IHasAttr_media, IHasAttr_hreflang, IHasAttr_type, IHasAttr_sizes { public string TagName => "link"; }
        public struct @main : IHtmlTagName { public string TagName => "main"; }
        public struct @map : IHtmlTagName, IHasAttr_name { public string TagName => "map"; }
        public struct @mark : IHtmlTagName { public string TagName => "mark"; }
        public struct @math : IHtmlTagName { public string TagName => "math"; }
        public struct @menu : IHtmlTagName, IHasAttr_type, IHasAttr_label { public string TagName => "menu"; }
        public struct @menuitem : IHtmlTagName, IHasAttr_type, IHasAttr_label, IHasAttr_icon, IHasAttr_disabled, IHasAttr_checked, IHasAttr_radiogroup, IHasAttr_default { public string TagName => "menuitem"; }
        public struct @meta : IHtmlTagName, IHasAttr_name, IHasAttr_http_equiv, IHasAttr_content, IHasAttr_charset { public string TagName => "meta"; }
        public struct @meter : IHtmlTagName, IHasAttr_value, IHasAttr_min, IHasAttr_max, IHasAttr_low, IHasAttr_high, IHasAttr_optimum { public string TagName => "meter"; }
        public struct @nav : IHtmlTagName { public string TagName => "nav"; }
        public struct @noscript : IHtmlTagName { public string TagName => "noscript"; }
        public struct @object : IHtmlTagName, IHasAttr_data, IHasAttr_type, IHasAttr_typemustmatch, IHasAttr_name, IHasAttr_usemap, IHasAttr_form, IHasAttr_width, IHasAttr_height { public string TagName => "object"; }
        public struct @ol : IHtmlTagName, IHasAttr_reversed, IHasAttr_start, IHasAttr_type { public string TagName => "ol"; }
        public struct @optgroup : IHtmlTagName, IHasAttr_disabled, IHasAttr_label { public string TagName => "optgroup"; }
        public struct @option : IHtmlTagName, IHasAttr_disabled, IHasAttr_label, IHasAttr_selected, IHasAttr_value { public string TagName => "option"; }
        public struct @output : IHtmlTagName, IHasAttr_for, IHasAttr_form, IHasAttr_name { public string TagName => "output"; }
        public struct @p : IHtmlTagName { public string TagName => "p"; }
        public struct @param : IHtmlTagName, IHasAttr_name, IHasAttr_value { public string TagName => "param"; }
        public struct @picture : IHtmlTagName { public string TagName => "picture"; }
        public struct @pre : IHtmlTagName { public string TagName => "pre"; }
        public struct @progress : IHtmlTagName, IHasAttr_value, IHasAttr_max { public string TagName => "progress"; }
        public struct @q : IHtmlTagName, IHasAttr_cite { public string TagName => "q"; }
        public struct @rp : IHtmlTagName { public string TagName => "rp"; }
        public struct @rt : IHtmlTagName { public string TagName => "rt"; }
        public struct @ruby : IHtmlTagName { public string TagName => "ruby"; }
        public struct @s : IHtmlTagName { public string TagName => "s"; }
        public struct @samp : IHtmlTagName { public string TagName => "samp"; }
        public struct @script : IHtmlTagName, IHasAttr_src, IHasAttr_type, IHasAttr_charset, IHasAttr_async, IHasAttr_defer, IHasAttr_crossorigin, IHasAttr_nonce { public string TagName => "script"; }
        public struct @section : IHtmlTagName { public string TagName => "section"; }
        public struct @select : IHtmlTagName, IHasAttr_autocomplete, IHasAttr_autofocus, IHasAttr_disabled, IHasAttr_form, IHasAttr_multiple, IHasAttr_name, IHasAttr_required, IHasAttr_size { public string TagName => "select"; }
        public struct @small : IHtmlTagName { public string TagName => "small"; }
        public struct @source : IHtmlTagName, IHasAttr_src, IHasAttr_type, IHasAttr_srcset, IHasAttr_sizes, IHasAttr_media { public string TagName => "source"; }
        public struct @span : IHtmlTagName { public string TagName => "span"; }
        public struct @strong : IHtmlTagName { public string TagName => "strong"; }
        public struct @style : IHtmlTagName, IHasAttr_media, IHasAttr_nonce, IHasAttr_type, IHasAttr_scoped { public string TagName => "style"; }
        public struct @sub : IHtmlTagName { public string TagName => "sub"; }
        public struct @summary : IHtmlTagName { public string TagName => "summary"; }
        public struct @sup : IHtmlTagName { public string TagName => "sup"; }
        public struct @svg : IHtmlTagName { public string TagName => "svg"; }
        public struct @table : IHtmlTagName { public string TagName => "table"; }
        public struct @tbody : IHtmlTagName { public string TagName => "tbody"; }
        public struct @td : IHtmlTagName, IHasAttr_colspan, IHasAttr_rowspan, IHasAttr_headers { public string TagName => "td"; }
        public struct @template : IHtmlTagName { public string TagName => "template"; }
        public struct @textarea : IHtmlTagName, IHasAttr_autofocus, IHasAttr_cols, IHasAttr_dirname, IHasAttr_disabled, IHasAttr_form, IHasAttr_inputmode, IHasAttr_maxlength, IHasAttr_minlength, IHasAttr_name, IHasAttr_placeholder, IHasAttr_readonly, IHasAttr_required, IHasAttr_rows, IHasAttr_wrap { public string TagName => "textarea"; }
        public struct @tfoot : IHtmlTagName { public string TagName => "tfoot"; }
        public struct @th : IHtmlTagName, IHasAttr_colspan, IHasAttr_rowspan, IHasAttr_headers, IHasAttr_scope, IHasAttr_abbr { public string TagName => "th"; }
        public struct @thead : IHtmlTagName { public string TagName => "thead"; }
        public struct @time : IHtmlTagName, IHasAttr_datetime { public string TagName => "time"; }
        public struct @title : IHtmlTagName { public string TagName => "title"; }
        public struct @tr : IHtmlTagName { public string TagName => "tr"; }
        public struct @track : IHtmlTagName, IHasAttr_default, IHasAttr_kind, IHasAttr_label, IHasAttr_src, IHasAttr_srclang { public string TagName => "track"; }
        public struct @u : IHtmlTagName { public string TagName => "u"; }
        public struct @ul : IHtmlTagName { public string TagName => "ul"; }
        public struct @var : IHtmlTagName { public string TagName => "var"; }
        public struct @video : IHtmlTagName, IHasAttr_src, IHasAttr_crossorigin, IHasAttr_poster, IHasAttr_preload, IHasAttr_autoplay, IHasAttr_mediagroup, IHasAttr_loop, IHasAttr_muted, IHasAttr_controls, IHasAttr_width, IHasAttr_height { public string TagName => "video"; }
        public struct @wbr : IHtmlTagName { public string TagName => "wbr"; }
    }

    public static class Tags
    {

        ///<summary>Hyperlink. See: <a href="https://html.spec.whatwg.org/#the-a-element">https://html.spec.whatwg.org/#the-a-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@a> _a = new HtmlStartTag<TagNames.@a>();

        ///<summary>Abbreviation. See: <a href="https://html.spec.whatwg.org/#the-abbr-element">https://html.spec.whatwg.org/#the-abbr-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@abbr> _abbr = new HtmlStartTag<TagNames.@abbr>();

        ///<summary>Contact information for a page or article element. See: <a href="https://html.spec.whatwg.org/#the-address-element">https://html.spec.whatwg.org/#the-address-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@address> _address = new HtmlStartTag<TagNames.@address>();

        ///<summary>Hyperlink or dead area on an image map. See: <a href="https://html.spec.whatwg.org/#the-area-element">https://html.spec.whatwg.org/#the-area-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@area> _area = new HtmlStartTag<TagNames.@area>();

        ///<summary>Self-contained syndicatable or reusable composition. See: <a href="https://html.spec.whatwg.org/#the-article-element">https://html.spec.whatwg.org/#the-article-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@article> _article = new HtmlStartTag<TagNames.@article>();

        ///<summary>Sidebar for tangentially related content. See: <a href="https://html.spec.whatwg.org/#the-aside-element">https://html.spec.whatwg.org/#the-aside-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@aside> _aside = new HtmlStartTag<TagNames.@aside>();

        ///<summary>Audio player. See: <a href="https://html.spec.whatwg.org/#the-audio-element">https://html.spec.whatwg.org/#the-audio-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@audio> _audio = new HtmlStartTag<TagNames.@audio>();

        ///<summary>Keywords. See: <a href="https://html.spec.whatwg.org/#the-b-element">https://html.spec.whatwg.org/#the-b-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@b> _b = new HtmlStartTag<TagNames.@b>();

        ///<summary>Base URL and default target browsing context for hyperlinks and forms. See: <a href="https://html.spec.whatwg.org/#the-base-element">https://html.spec.whatwg.org/#the-base-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@base> _base = new HtmlStartTag<TagNames.@base>();

        ///<summary>Text directionality isolation. See: <a href="https://html.spec.whatwg.org/#the-bdi-element">https://html.spec.whatwg.org/#the-bdi-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@bdi> _bdi = new HtmlStartTag<TagNames.@bdi>();

        ///<summary>Text directionality formatting. See: <a href="https://html.spec.whatwg.org/#the-bdo-element">https://html.spec.whatwg.org/#the-bdo-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@bdo> _bdo = new HtmlStartTag<TagNames.@bdo>();

        ///<summary>A section quoted from another source. See: <a href="https://html.spec.whatwg.org/#the-blockquote-element">https://html.spec.whatwg.org/#the-blockquote-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@blockquote> _blockquote = new HtmlStartTag<TagNames.@blockquote>();

        ///<summary>Document body. See: <a href="https://html.spec.whatwg.org/#the-body-element">https://html.spec.whatwg.org/#the-body-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@body> _body = new HtmlStartTag<TagNames.@body>();

        ///<summary>Line break, e.g. in poem or postal address. See: <a href="https://html.spec.whatwg.org/#the-br-element">https://html.spec.whatwg.org/#the-br-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@br> _br = new HtmlStartTag<TagNames.@br>();

        ///<summary>Button control. See: <a href="https://html.spec.whatwg.org/#the-button-element">https://html.spec.whatwg.org/#the-button-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@button> _button = new HtmlStartTag<TagNames.@button>();

        ///<summary>Scriptable bitmap canvas. See: <a href="https://html.spec.whatwg.org/#the-canvas-element">https://html.spec.whatwg.org/#the-canvas-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@canvas> _canvas = new HtmlStartTag<TagNames.@canvas>();

        ///<summary>Table caption. See: <a href="https://html.spec.whatwg.org/#the-caption-element">https://html.spec.whatwg.org/#the-caption-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@caption> _caption = new HtmlStartTag<TagNames.@caption>();

        ///<summary>Title of a work. See: <a href="https://html.spec.whatwg.org/#the-cite-element">https://html.spec.whatwg.org/#the-cite-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@cite> _cite = new HtmlStartTag<TagNames.@cite>();

        ///<summary>Computer code. See: <a href="https://html.spec.whatwg.org/#the-code-element">https://html.spec.whatwg.org/#the-code-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@code> _code = new HtmlStartTag<TagNames.@code>();

        ///<summary>Table column. See: <a href="https://html.spec.whatwg.org/#the-col-element">https://html.spec.whatwg.org/#the-col-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@col> _col = new HtmlStartTag<TagNames.@col>();

        ///<summary>Group of columns in a table. See: <a href="https://html.spec.whatwg.org/#the-colgroup-element">https://html.spec.whatwg.org/#the-colgroup-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@colgroup> _colgroup = new HtmlStartTag<TagNames.@colgroup>();

        ///<summary>Machine-readable equivalent. See: <a href="https://html.spec.whatwg.org/#the-data-element">https://html.spec.whatwg.org/#the-data-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@data> _data = new HtmlStartTag<TagNames.@data>();

        ///<summary>Container for options for combo box control. See: <a href="https://html.spec.whatwg.org/#the-datalist-element">https://html.spec.whatwg.org/#the-datalist-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@datalist> _datalist = new HtmlStartTag<TagNames.@datalist>();

        ///<summary>Content for corresponding dt element(s). See: <a href="https://html.spec.whatwg.org/#the-dd-element">https://html.spec.whatwg.org/#the-dd-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@dd> _dd = new HtmlStartTag<TagNames.@dd>();

        ///<summary>A removal from the document. See: <a href="https://html.spec.whatwg.org/#the-del-element">https://html.spec.whatwg.org/#the-del-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@del> _del = new HtmlStartTag<TagNames.@del>();

        ///<summary>Disclosure control for hiding details. See: <a href="https://html.spec.whatwg.org/#the-details-element">https://html.spec.whatwg.org/#the-details-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@details> _details = new HtmlStartTag<TagNames.@details>();

        ///<summary>Defining instance. See: <a href="https://html.spec.whatwg.org/#the-dfn-element">https://html.spec.whatwg.org/#the-dfn-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@dfn> _dfn = new HtmlStartTag<TagNames.@dfn>();

        ///<summary>Dialog box or window. See: <a href="https://html.spec.whatwg.org/#the-dialog-element">https://html.spec.whatwg.org/#the-dialog-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@dialog> _dialog = new HtmlStartTag<TagNames.@dialog>();

        ///<summary>Generic flow container. See: <a href="https://html.spec.whatwg.org/#the-div-element">https://html.spec.whatwg.org/#the-div-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@div> _div = new HtmlStartTag<TagNames.@div>();

        ///<summary>Association list consisting of zero or more name-value groups. See: <a href="https://html.spec.whatwg.org/#the-dl-element">https://html.spec.whatwg.org/#the-dl-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@dl> _dl = new HtmlStartTag<TagNames.@dl>();

        ///<summary>Legend for corresponding dd element(s). See: <a href="https://html.spec.whatwg.org/#the-dt-element">https://html.spec.whatwg.org/#the-dt-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@dt> _dt = new HtmlStartTag<TagNames.@dt>();

        ///<summary>Stress emphasis. See: <a href="https://html.spec.whatwg.org/#the-em-element">https://html.spec.whatwg.org/#the-em-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@em> _em = new HtmlStartTag<TagNames.@em>();

        ///<summary>Plugin. See: <a href="https://html.spec.whatwg.org/#the-embed-element">https://html.spec.whatwg.org/#the-embed-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@embed> _embed = new HtmlStartTag<TagNames.@embed>();

        ///<summary>Group of form controls. See: <a href="https://html.spec.whatwg.org/#the-fieldset-element">https://html.spec.whatwg.org/#the-fieldset-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@fieldset> _fieldset = new HtmlStartTag<TagNames.@fieldset>();

        ///<summary>Caption for figure. See: <a href="https://html.spec.whatwg.org/#the-figcaption-element">https://html.spec.whatwg.org/#the-figcaption-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@figcaption> _figcaption = new HtmlStartTag<TagNames.@figcaption>();

        ///<summary>Figure with optional caption. See: <a href="https://html.spec.whatwg.org/#the-figure-element">https://html.spec.whatwg.org/#the-figure-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@figure> _figure = new HtmlStartTag<TagNames.@figure>();

        ///<summary>Footer for a page or section. See: <a href="https://html.spec.whatwg.org/#the-footer-element">https://html.spec.whatwg.org/#the-footer-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@footer> _footer = new HtmlStartTag<TagNames.@footer>();

        ///<summary>User-submittable form. See: <a href="https://html.spec.whatwg.org/#the-form-element">https://html.spec.whatwg.org/#the-form-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@form> _form = new HtmlStartTag<TagNames.@form>();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@h1> _h1 = new HtmlStartTag<TagNames.@h1>();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@h2> _h2 = new HtmlStartTag<TagNames.@h2>();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@h3> _h3 = new HtmlStartTag<TagNames.@h3>();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@h4> _h4 = new HtmlStartTag<TagNames.@h4>();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@h5> _h5 = new HtmlStartTag<TagNames.@h5>();

        ///<summary>Section heading. See: <a href="https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements">https://html.spec.whatwg.org/#the-h1,-h2,-h3,-h4,-h5,-and-h6-elements</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@h6> _h6 = new HtmlStartTag<TagNames.@h6>();

        ///<summary>Container for document metadata. See: <a href="https://html.spec.whatwg.org/#the-head-element">https://html.spec.whatwg.org/#the-head-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@head> _head = new HtmlStartTag<TagNames.@head>();

        ///<summary>Introductory or navigational aids for a page or section. See: <a href="https://html.spec.whatwg.org/#the-header-element">https://html.spec.whatwg.org/#the-header-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@header> _header = new HtmlStartTag<TagNames.@header>();

        ///<summary>heading group. See: <a href="https://html.spec.whatwg.org/#the-hgroup-element">https://html.spec.whatwg.org/#the-hgroup-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@hgroup> _hgroup = new HtmlStartTag<TagNames.@hgroup>();

        ///<summary>Thematic break. See: <a href="https://html.spec.whatwg.org/#the-hr-element">https://html.spec.whatwg.org/#the-hr-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@hr> _hr = new HtmlStartTag<TagNames.@hr>();

        ///<summary>Root element. See: <a href="https://html.spec.whatwg.org/#the-html-element">https://html.spec.whatwg.org/#the-html-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@html> _html = new HtmlStartTag<TagNames.@html>();

        ///<summary>Alternate voice. See: <a href="https://html.spec.whatwg.org/#the-i-element">https://html.spec.whatwg.org/#the-i-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@i> _i = new HtmlStartTag<TagNames.@i>();

        ///<summary>Nested browsing context. See: <a href="https://html.spec.whatwg.org/#the-iframe-element">https://html.spec.whatwg.org/#the-iframe-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@iframe> _iframe = new HtmlStartTag<TagNames.@iframe>();

        ///<summary>Image. See: <a href="https://html.spec.whatwg.org/#the-img-element">https://html.spec.whatwg.org/#the-img-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@img> _img = new HtmlStartTag<TagNames.@img>();

        ///<summary>Form control. See: <a href="https://html.spec.whatwg.org/#the-input-element">https://html.spec.whatwg.org/#the-input-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@input> _input = new HtmlStartTag<TagNames.@input>();

        ///<summary>An addition to the document. See: <a href="https://html.spec.whatwg.org/#the-ins-element">https://html.spec.whatwg.org/#the-ins-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@ins> _ins = new HtmlStartTag<TagNames.@ins>();

        ///<summary>User input. See: <a href="https://html.spec.whatwg.org/#the-kbd-element">https://html.spec.whatwg.org/#the-kbd-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@kbd> _kbd = new HtmlStartTag<TagNames.@kbd>();

        ///<summary>Cryptographic key-pair generator form control. See: <a href="https://html.spec.whatwg.org/#the-keygen-element">https://html.spec.whatwg.org/#the-keygen-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@keygen> _keygen = new HtmlStartTag<TagNames.@keygen>();

        ///<summary>Caption for a form control. See: <a href="https://html.spec.whatwg.org/#the-label-element">https://html.spec.whatwg.org/#the-label-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@label> _label = new HtmlStartTag<TagNames.@label>();

        ///<summary>Caption for fieldset. See: <a href="https://html.spec.whatwg.org/#the-legend-element">https://html.spec.whatwg.org/#the-legend-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@legend> _legend = new HtmlStartTag<TagNames.@legend>();

        ///<summary>List item. See: <a href="https://html.spec.whatwg.org/#the-li-element">https://html.spec.whatwg.org/#the-li-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@li> _li = new HtmlStartTag<TagNames.@li>();

        ///<summary>Link metadata. See: <a href="https://html.spec.whatwg.org/#the-link-element">https://html.spec.whatwg.org/#the-link-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@link> _link = new HtmlStartTag<TagNames.@link>();

        ///<summary>Container for the dominant contents of another element. See: <a href="https://html.spec.whatwg.org/#the-main-element">https://html.spec.whatwg.org/#the-main-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@main> _main = new HtmlStartTag<TagNames.@main>();

        ///<summary>Image map. See: <a href="https://html.spec.whatwg.org/#the-map-element">https://html.spec.whatwg.org/#the-map-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@map> _map = new HtmlStartTag<TagNames.@map>();

        ///<summary>Highlight. See: <a href="https://html.spec.whatwg.org/#the-mark-element">https://html.spec.whatwg.org/#the-mark-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@mark> _mark = new HtmlStartTag<TagNames.@mark>();

        ///<summary>MathML root. See: <a href="https://html.spec.whatwg.org/#math">https://html.spec.whatwg.org/#math</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@math> _math = new HtmlStartTag<TagNames.@math>();

        ///<summary>Menu of commands. See: <a href="https://html.spec.whatwg.org/#the-menu-element">https://html.spec.whatwg.org/#the-menu-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@menu> _menu = new HtmlStartTag<TagNames.@menu>();

        ///<summary>Menu command. See: <a href="https://html.spec.whatwg.org/#the-menuitem-element">https://html.spec.whatwg.org/#the-menuitem-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@menuitem> _menuitem = new HtmlStartTag<TagNames.@menuitem>();

        ///<summary>Text metadata. See: <a href="https://html.spec.whatwg.org/#the-meta-element">https://html.spec.whatwg.org/#the-meta-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@meta> _meta = new HtmlStartTag<TagNames.@meta>();

        ///<summary>Gauge. See: <a href="https://html.spec.whatwg.org/#the-meter-element">https://html.spec.whatwg.org/#the-meter-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@meter> _meter = new HtmlStartTag<TagNames.@meter>();

        ///<summary>Section with navigational links. See: <a href="https://html.spec.whatwg.org/#the-nav-element">https://html.spec.whatwg.org/#the-nav-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@nav> _nav = new HtmlStartTag<TagNames.@nav>();

        ///<summary>Fallback content for script. See: <a href="https://html.spec.whatwg.org/#the-noscript-element">https://html.spec.whatwg.org/#the-noscript-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@noscript> _noscript = new HtmlStartTag<TagNames.@noscript>();

        ///<summary>Image, nested browsing context, or plugin. See: <a href="https://html.spec.whatwg.org/#the-object-element">https://html.spec.whatwg.org/#the-object-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@object> _object = new HtmlStartTag<TagNames.@object>();

        ///<summary>Ordered list. See: <a href="https://html.spec.whatwg.org/#the-ol-element">https://html.spec.whatwg.org/#the-ol-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@ol> _ol = new HtmlStartTag<TagNames.@ol>();

        ///<summary>Group of options in a list box. See: <a href="https://html.spec.whatwg.org/#the-optgroup-element">https://html.spec.whatwg.org/#the-optgroup-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@optgroup> _optgroup = new HtmlStartTag<TagNames.@optgroup>();

        ///<summary>Option in a list box or combo box control. See: <a href="https://html.spec.whatwg.org/#the-option-element">https://html.spec.whatwg.org/#the-option-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@option> _option = new HtmlStartTag<TagNames.@option>();

        ///<summary>Calculated output value. See: <a href="https://html.spec.whatwg.org/#the-output-element">https://html.spec.whatwg.org/#the-output-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@output> _output = new HtmlStartTag<TagNames.@output>();

        ///<summary>Paragraph. See: <a href="https://html.spec.whatwg.org/#the-p-element">https://html.spec.whatwg.org/#the-p-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@p> _p = new HtmlStartTag<TagNames.@p>();

        ///<summary>Parameter for object. See: <a href="https://html.spec.whatwg.org/#the-param-element">https://html.spec.whatwg.org/#the-param-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@param> _param = new HtmlStartTag<TagNames.@param>();

        ///<summary>Image. See: <a href="https://html.spec.whatwg.org/#the-picture-element">https://html.spec.whatwg.org/#the-picture-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@picture> _picture = new HtmlStartTag<TagNames.@picture>();

        ///<summary>Block of preformatted text. See: <a href="https://html.spec.whatwg.org/#the-pre-element">https://html.spec.whatwg.org/#the-pre-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@pre> _pre = new HtmlStartTag<TagNames.@pre>();

        ///<summary>Progress bar. See: <a href="https://html.spec.whatwg.org/#the-progress-element">https://html.spec.whatwg.org/#the-progress-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@progress> _progress = new HtmlStartTag<TagNames.@progress>();

        ///<summary>Quotation. See: <a href="https://html.spec.whatwg.org/#the-q-element">https://html.spec.whatwg.org/#the-q-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@q> _q = new HtmlStartTag<TagNames.@q>();

        ///<summary>Parenthesis for ruby annotation text. See: <a href="https://html.spec.whatwg.org/#the-rp-element">https://html.spec.whatwg.org/#the-rp-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@rp> _rp = new HtmlStartTag<TagNames.@rp>();

        ///<summary>Ruby annotation text. See: <a href="https://html.spec.whatwg.org/#the-rt-element">https://html.spec.whatwg.org/#the-rt-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@rt> _rt = new HtmlStartTag<TagNames.@rt>();

        ///<summary>Ruby annotation(s). See: <a href="https://html.spec.whatwg.org/#the-ruby-element">https://html.spec.whatwg.org/#the-ruby-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@ruby> _ruby = new HtmlStartTag<TagNames.@ruby>();

        ///<summary>Inaccurate text. See: <a href="https://html.spec.whatwg.org/#the-s-element">https://html.spec.whatwg.org/#the-s-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@s> _s = new HtmlStartTag<TagNames.@s>();

        ///<summary>Computer output. See: <a href="https://html.spec.whatwg.org/#the-samp-element">https://html.spec.whatwg.org/#the-samp-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@samp> _samp = new HtmlStartTag<TagNames.@samp>();

        ///<summary>Embedded script. See: <a href="https://html.spec.whatwg.org/#the-script-element">https://html.spec.whatwg.org/#the-script-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@script> _script = new HtmlStartTag<TagNames.@script>();

        ///<summary>Generic document or application section. See: <a href="https://html.spec.whatwg.org/#the-section-element">https://html.spec.whatwg.org/#the-section-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@section> _section = new HtmlStartTag<TagNames.@section>();

        ///<summary>List box control. See: <a href="https://html.spec.whatwg.org/#the-select-element">https://html.spec.whatwg.org/#the-select-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@select> _select = new HtmlStartTag<TagNames.@select>();

        ///<summary>Side comment. See: <a href="https://html.spec.whatwg.org/#the-small-element">https://html.spec.whatwg.org/#the-small-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@small> _small = new HtmlStartTag<TagNames.@small>();

        ///<summary>Media source for video or audio. See: <a href="https://html.spec.whatwg.org/#the-source-element">https://html.spec.whatwg.org/#the-source-element</a><br />Image source for an img. See: <a href="https://html.spec.whatwg.org/#the-source-element-when-used-with-the-picture-element">https://html.spec.whatwg.org/#the-source-element-when-used-with-the-picture-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@source> _source = new HtmlStartTag<TagNames.@source>();

        ///<summary>Generic phrasing container. See: <a href="https://html.spec.whatwg.org/#the-span-element">https://html.spec.whatwg.org/#the-span-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@span> _span = new HtmlStartTag<TagNames.@span>();

        ///<summary>Importance. See: <a href="https://html.spec.whatwg.org/#the-strong-element">https://html.spec.whatwg.org/#the-strong-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@strong> _strong = new HtmlStartTag<TagNames.@strong>();

        ///<summary>Embedded styling information. See: <a href="https://html.spec.whatwg.org/#the-style-element">https://html.spec.whatwg.org/#the-style-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@style> _style = new HtmlStartTag<TagNames.@style>();

        ///<summary>Subscript. See: <a href="https://html.spec.whatwg.org/#the-sub-and-sup-elements">https://html.spec.whatwg.org/#the-sub-and-sup-elements</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@sub> _sub = new HtmlStartTag<TagNames.@sub>();

        ///<summary>Caption for details. See: <a href="https://html.spec.whatwg.org/#the-summary-element">https://html.spec.whatwg.org/#the-summary-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@summary> _summary = new HtmlStartTag<TagNames.@summary>();

        ///<summary>Superscript. See: <a href="https://html.spec.whatwg.org/#the-sub-and-sup-elements">https://html.spec.whatwg.org/#the-sub-and-sup-elements</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@sup> _sup = new HtmlStartTag<TagNames.@sup>();

        ///<summary>SVG root. See: <a href="https://html.spec.whatwg.org/#svg">https://html.spec.whatwg.org/#svg</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@svg> _svg = new HtmlStartTag<TagNames.@svg>();

        ///<summary>Table. See: <a href="https://html.spec.whatwg.org/#the-table-element">https://html.spec.whatwg.org/#the-table-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@table> _table = new HtmlStartTag<TagNames.@table>();

        ///<summary>Group of rows in a table. See: <a href="https://html.spec.whatwg.org/#the-tbody-element">https://html.spec.whatwg.org/#the-tbody-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@tbody> _tbody = new HtmlStartTag<TagNames.@tbody>();

        ///<summary>Table cell. See: <a href="https://html.spec.whatwg.org/#the-td-element">https://html.spec.whatwg.org/#the-td-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@td> _td = new HtmlStartTag<TagNames.@td>();

        ///<summary>Template. See: <a href="https://html.spec.whatwg.org/#the-template-element">https://html.spec.whatwg.org/#the-template-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@template> _template = new HtmlStartTag<TagNames.@template>();

        ///<summary>Multiline text field. See: <a href="https://html.spec.whatwg.org/#the-textarea-element">https://html.spec.whatwg.org/#the-textarea-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@textarea> _textarea = new HtmlStartTag<TagNames.@textarea>();

        ///<summary>Group of footer rows in a table. See: <a href="https://html.spec.whatwg.org/#the-tfoot-element">https://html.spec.whatwg.org/#the-tfoot-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@tfoot> _tfoot = new HtmlStartTag<TagNames.@tfoot>();

        ///<summary>Table header cell. See: <a href="https://html.spec.whatwg.org/#the-th-element">https://html.spec.whatwg.org/#the-th-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@th> _th = new HtmlStartTag<TagNames.@th>();

        ///<summary>Group of heading rows in a table. See: <a href="https://html.spec.whatwg.org/#the-thead-element">https://html.spec.whatwg.org/#the-thead-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@thead> _thead = new HtmlStartTag<TagNames.@thead>();

        ///<summary>Machine-readable equivalent of date- or time-related data. See: <a href="https://html.spec.whatwg.org/#the-time-element">https://html.spec.whatwg.org/#the-time-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@time> _time = new HtmlStartTag<TagNames.@time>();

        ///<summary>Document title. See: <a href="https://html.spec.whatwg.org/#the-title-element">https://html.spec.whatwg.org/#the-title-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@title> _title = new HtmlStartTag<TagNames.@title>();

        ///<summary>Table row. See: <a href="https://html.spec.whatwg.org/#the-tr-element">https://html.spec.whatwg.org/#the-tr-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@tr> _tr = new HtmlStartTag<TagNames.@tr>();

        ///<summary>Timed text track. See: <a href="https://html.spec.whatwg.org/#the-track-element">https://html.spec.whatwg.org/#the-track-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@track> _track = new HtmlStartTag<TagNames.@track>();

        ///<summary>Keywords. See: <a href="https://html.spec.whatwg.org/#the-u-element">https://html.spec.whatwg.org/#the-u-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@u> _u = new HtmlStartTag<TagNames.@u>();

        ///<summary>List. See: <a href="https://html.spec.whatwg.org/#the-ul-element">https://html.spec.whatwg.org/#the-ul-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@ul> _ul = new HtmlStartTag<TagNames.@ul>();

        ///<summary>Variable. See: <a href="https://html.spec.whatwg.org/#the-var-element">https://html.spec.whatwg.org/#the-var-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@var> _var = new HtmlStartTag<TagNames.@var>();

        ///<summary>Video player. See: <a href="https://html.spec.whatwg.org/#the-video-element">https://html.spec.whatwg.org/#the-video-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@video> _video = new HtmlStartTag<TagNames.@video>();

        ///<summary>Line breaking opportunity. See: <a href="https://html.spec.whatwg.org/#the-wbr-element">https://html.spec.whatwg.org/#the-wbr-element</a><br /></summary>
        public static readonly HtmlStartTag<TagNames.@wbr> _wbr = new HtmlStartTag<TagNames.@wbr>();
    }
}