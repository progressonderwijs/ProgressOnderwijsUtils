#nullable enable
using static ProgressOnderwijsUtils.Html.AttributeNameInterfaces;

namespace ProgressOnderwijsUtils.Html;

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
    [Obsolete]
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
    public static THtmlTag _inert<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
        where THtmlTag : struct, IHtmlElement<THtmlTag>
        => htmlTagExpr.Attribute("inert", attrValue);
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
    public static THtmlTag _popover<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
        where THtmlTag : struct, IHtmlElement<THtmlTag>
        => htmlTagExpr.Attribute("popover", attrValue);
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
    public static THtmlTag _popovertarget<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
        where THtmlTag : struct, IHasAttr_popovertarget, IHtmlElement<THtmlTag>
        => htmlTagExpr.Attribute("popovertarget", attrValue);
    public static THtmlTag _popovertargetaction<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
        where THtmlTag : struct, IHasAttr_popovertargetaction, IHtmlElement<THtmlTag>
        => htmlTagExpr.Attribute("popovertargetaction", attrValue);
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
    public static THtmlTag _loading<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
        where THtmlTag : struct, IHasAttr_loading, IHtmlElement<THtmlTag>
        => htmlTagExpr.Attribute("loading", attrValue);
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
    public static THtmlTag _fetchpriority<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
        where THtmlTag : struct, IHasAttr_fetchpriority, IHtmlElement<THtmlTag>
        => htmlTagExpr.Attribute("fetchpriority", attrValue);
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
    public static THtmlTag _blocking<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
        where THtmlTag : struct, IHasAttr_blocking, IHtmlElement<THtmlTag>
        => htmlTagExpr.Attribute("blocking", attrValue);
    public static THtmlTag _color<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
        where THtmlTag : struct, IHasAttr_color, IHtmlElement<THtmlTag>
        => htmlTagExpr.Attribute("color", attrValue);
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
    public static THtmlTag _nomodule<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
        where THtmlTag : struct, IHasAttr_nomodule, IHtmlElement<THtmlTag>
        => htmlTagExpr.Attribute("nomodule", attrValue);
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