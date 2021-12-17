using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace ProgressOnderwijsUtils.Html;

public struct HtmlFragment : IConvertibleToFragment
{
    /// <summary>
    /// Either a string, an IHtmlElement, a non-empty HtmlFragment[], or null (the empty fragment).
    /// </summary>
    public readonly object? Implementation;

    public bool IsTextContent()
        => Implementation is string;

    public bool IsTextContent([NotNullWhen(true)] out string? textContent)
        => (textContent = Implementation as string) != null;

    public bool IsElement()
        => Implementation is IHtmlElement;

    public bool IsElement([NotNullWhen(true)] out IHtmlElement? element)
        => (element = Implementation as IHtmlElement) != null;

    public bool IsElementAllowingContent()
        => Implementation is IHtmlElementAllowingContent;

    public bool IsElementAllowingContent([NotNullWhen(true)] out IHtmlElementAllowingContent? element)
        => (element = Implementation as IHtmlElementAllowingContent) != null;

    public bool IsMultipleNodes()
        => Implementation is HtmlFragment[];

    public bool IsMultipleNodes([NotNullWhen(true)] out HtmlFragment[]? nodes)
        => (nodes = Implementation as HtmlFragment[]) != null;

    /// <summary>
    /// Sets at most one of the out parameters to a non-null value.
    /// </summary>
    public void Deconstruct(out string? textContent, out IHtmlElement? element, out HtmlFragment[]? nodes)
    {
        textContent = Implementation as string;
        element = Implementation as IHtmlElement;
        nodes = Implementation as HtmlFragment[];
    }

    public bool IsEmpty
        => Implementation == null;

    HtmlFragment(object? content)
        => Implementation = content;

    [Pure]
    public static HtmlFragment TextContent(string? textContent)
        => new(textContent != "" ? textContent : null);

    [Pure]
    public static HtmlFragment Element(IHtmlElement? element)
        => new(element);

    [Pure]
    public static HtmlFragment Element(CustomHtmlElement element)
        => new(element.Canonicalize());

    [Pure]
    public static HtmlFragment Element(string tagName, HtmlAttribute[]? attributes, HtmlFragment[]? childNodes)
        => Element(new(tagName, attributes, childNodes));

    [Pure]
    public static HtmlFragment Fragment()
        => Empty;

    [Pure]
    public static HtmlFragment Fragment(HtmlFragment htmlEl)
        => htmlEl;

    [Pure]
    public static HtmlFragment Fragment(HtmlFragment a, HtmlFragment b)
    {
        if (a.IsEmpty) {
            //optimize very common case, when appending to empty.
            return b;
        }
        var kidCounter = new KidCounter();
        kidCounter.CountForKid(a);
        kidCounter.CountForKid(b);
        var collector = new KidCollector(kidCounter.TotalKids);
        collector.AddKid(a);
        collector.AddKid(b);
        Debug.Assert(collector.IsFull());
        return new(collector.retval);
    }

    [Pure]
    public static HtmlFragment Fragment(params HtmlFragment[]? htmlEls)
    {
        if (htmlEls == null || htmlEls.Length == 0) {
            return Empty;
        }
        if (htmlEls.Length == 1) {
            return htmlEls[0];
        }
        var kidCounter = new KidCounter();
        foreach (var child in htmlEls) {
            kidCounter.CountForKid(child);
            if (kidCounter.TotalKids >= 64) {
                return new(htmlEls);
            }
        }
        if (kidCounter.TotalKids == 0) {
            return Empty;
        } else if (!kidCounter.FlattenRelevant) {
            return new(htmlEls);
        }
        var collector = new KidCollector(kidCounter.TotalKids);
        foreach (var child in htmlEls) {
            collector.AddKid(child);
        }
        Debug.Assert(collector.IsFull());

        return collector.retval.Length == 1 ? collector.retval[0] : new(collector.retval);
    }

    struct KidCounter
    {
        public int TotalKids;
        public bool FlattenRelevant;

        public void CountForKid(HtmlFragment child)
        {
            if (child.Implementation is HtmlFragment[] childContents) {
                TotalKids += childContents.Length;
                FlattenRelevant = true;
            } else if (child.IsEmpty) {
                FlattenRelevant = true;
            } else {
                TotalKids++;
            }
        }
    }

    struct KidCollector
    {
        public readonly HtmlFragment[] retval;
        public int writeIdx;

        public KidCollector(int totalKids)
        {
            retval = new HtmlFragment[totalKids];
            writeIdx = 0;
        }

        public void AddKid(HtmlFragment child)
        {
            if (child.Implementation is HtmlFragment[] childContents) {
                foreach (var grandChild in childContents) {
                    retval[writeIdx++] = grandChild;
                }
            } else if (!child.IsEmpty) {
                retval[writeIdx++] = child;
            }
        }

        public bool IsFull()
            => writeIdx == retval.Length;
    }

    [Pure]
    public static HtmlFragment Fragment<T>(IEnumerable<T> htmlEls)
        where T : IConvertibleToFragment
    {
        var retval = new ArrayBuilder<HtmlFragment>();
        foreach (var el in htmlEls) {
            var htmlFragment = el.AsFragment();
            if (htmlFragment.Implementation is HtmlFragment[] kids && kids.Length < 64) {
                foreach (var grandchild in kids) {
                    retval.Add(grandchild);
                }
            } else if (!htmlFragment.IsEmpty) {
                retval.Add(htmlFragment);
            }
        }
        return new(retval.ToArray());
    }

    public override string ToString()
        => $"HtmlFragment: {this.ToStringWithoutDoctype()}";

    public static HtmlFragment Empty
        => new();

    public static implicit operator HtmlFragment(CustomHtmlElement element)
        => Element(element);

    public static implicit operator HtmlFragment(string? textContent)
        => TextContent(textContent);

    public static implicit operator HtmlFragment(HtmlFragment[]? fragments)
        => Fragment(fragments);

    public HtmlFragment Append(HtmlFragment tail)
        => Fragment(this, tail);

    public HtmlFragment Append(params HtmlFragment[]? longTail)
        => Fragment(this, Fragment(longTail));

    public static HtmlFragment operator +(HtmlFragment left, HtmlFragment right)
        => left.Append(right);

    [Pure]
    HtmlFragment IConvertibleToFragment.AsFragment()
        => this;

    public HtmlFragment[] NodesOfFragment()
        => Implementation as HtmlFragment[] ?? (IsEmpty ? EmptyNodes : new[] { this, });

    public HtmlFragment[] ChildNodes()
        => Implementation is IHtmlElementAllowingContent elem
            ? elem.GetContent().NodesOfFragment()
            : Implementation as HtmlFragment[] ?? EmptyNodes;

    public static HtmlFragment[] EmptyNodes
        => Array.Empty<HtmlFragment>();

    /// <summary>
    /// Attempts to parse an html fragment as html5.
    /// 
    /// This is not a security check; the output of this function can still contain dangerous tags!
    /// 
    /// Use Sanitze() to clean up parsed html.
    /// </summary>
    /// <returns>The html fragment.</returns>
    public static HtmlFragment ParseFragment(string? str)
        => ParseFragment(str, new());

    public static HtmlFragment ParseFragment(string? str, HtmlParserOptions options)
    {
        if (string.IsNullOrEmpty(str)) {
            return Empty;
        }
        var body = new HtmlParser().ParseDocument("").DocumentElement.GetElementsByTagName("body").Single();
        return new HtmlParser(options)
            .ParseFragment(str, body)
            .Select(CreateFromAngleSharpNode)
            .AsFragment();
    }

    public static HtmlFragment CreateFromAngleSharpNode(INode? node)
    {
        if (node is IText text) {
            return TextContent(text.NodeValue);
        }

        if (node is IElement element) {
            return Element(
                element.TagName.ToLowerInvariant(),
                element.Attributes.Select(attr => new HtmlAttribute(attr.Name, attr.Value)).ToArray(),
                (node is IHtmlTemplateElement templateElement ? templateElement.Content.ChildNodes : element.ChildNodes).Select(CreateFromAngleSharpNode).ToArray()
            );
        }
        if (node is IDocumentFragment) {
            return node.ChildNodes.Select(CreateFromAngleSharpNode).AsFragment();
        }

        return Empty;
    }
}
