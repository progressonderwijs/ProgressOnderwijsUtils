namespace ProgressOnderwijsUtils.Html
{
    public static class HtmlToTextContent
    {
        public static string TextContent(this HtmlFragment fragment)
        {
            var fastStringBuilder = FastShortStringBuilder.Create();
            AppendTextContent(ref fastStringBuilder, fragment);
            return fastStringBuilder.Value;
        }

        static void AppendTextContent(ref FastShortStringBuilder fastStringBuilder, HtmlFragment fragment)
        {
            if (fragment.Content is string str) {
                fastStringBuilder.AppendText(str);
            } else {
                foreach (var child in fragment.Children) {
                    AppendTextContent(ref fastStringBuilder, child);
                }
            }
        }
    }
}
