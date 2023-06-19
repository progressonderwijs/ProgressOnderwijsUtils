namespace ProgressOnderwijsUtils.Html;

public static class HtmlToTextContent
{
    public static string TextContent(this HtmlFragment fragment)
    {
        var fastStringBuilder = MutableShortStringBuilder.Create();
        AppendTextContent(ref fastStringBuilder, fragment.Implementation);
        return fastStringBuilder.FinishBuilding();
    }

    static void AppendTextContent(ref MutableShortStringBuilder mutableStringBuilder, object? fragmentContent)
    {
        if (fragmentContent is string str) {
            mutableStringBuilder.AppendText(str);
        } else if (fragmentContent is HtmlFragment[] childFragments) {
            foreach (var child in childFragments) {
                AppendTextContent(ref mutableStringBuilder, child.Implementation);
            }
        } else if (fragmentContent is IHtmlElementAllowingContent elemWithContent && elemWithContent.GetContent().Implementation is { } nonNullFragmentContent) {
            AppendTextContent(ref mutableStringBuilder, nonNullFragmentContent);
        }
    }
}
