using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    public static class HtmlToTextContent
    {
        [NotNull]
        public static string TextContent(this HtmlFragment fragment)
        {
            var fastStringBuilder = FastShortStringBuilder.Create();
            AppendTextContent(ref fastStringBuilder, fragment);
            return fastStringBuilder.FinishBuilding();
        }

        static void AppendTextContent(ref FastShortStringBuilder fastStringBuilder, HtmlFragment fragment)
        {
            if (fragment.Content is string str) {
                fastStringBuilder.AppendText(str);
            } else if (fragment.Content is HtmlFragment[] childFragments) {
                foreach (var child in childFragments) {
                    AppendTextContent(ref fastStringBuilder, child);
                }
            } else if (fragment.Content is IHtmlTagAllowingContent elemWithContent && elemWithContent.Contents != null) {
                foreach (var child in elemWithContent.Contents) {
                    AppendTextContent(ref fastStringBuilder, child);
                }
            }
        }
    }
}
