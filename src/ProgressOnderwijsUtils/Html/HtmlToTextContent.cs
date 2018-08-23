using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    public static class HtmlToTextContent
    {
        [NotNull]
        public static string TextContent(this HtmlFragment fragment)
        {
            var fastStringBuilder = FastShortStringBuilder.Create();
            AppendTextContent(ref fastStringBuilder, fragment.Content);
            return fastStringBuilder.FinishBuilding();
        }

        static void AppendTextContent(ref FastShortStringBuilder fastStringBuilder, object fragmentContent)
        {
            if (fragmentContent is string str) {
                fastStringBuilder.AppendText(str);
            } else if (fragmentContent is HtmlFragment[] childFragments) {
                foreach (var child in childFragments) {
                    AppendTextContent(ref fastStringBuilder, child.Content);
                }
            } else if (fragmentContent is IHtmlTagAllowingContent elemWithContent && elemWithContent.Contents.Content is object nonNullFragmentContent) {
                AppendTextContent(ref fastStringBuilder, nonNullFragmentContent);
            }
        }
    }
}
