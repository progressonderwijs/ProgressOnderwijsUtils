using Yahoo.Yui.Compressor;

namespace ProgressOnderwijsUtils.WebSupport
{
    public static class MinifyYui
    {
        //static readonly CssCompressor cssCompressor = new CssCompressor();
        public static string MinifyCss(string originalCss) => new CssCompressor().Compress(originalCss);
        //static readonly JavaScriptCompressor jsCompressor = new JavaScriptCompressor();
        [CodeDieAlleenWordtGebruiktInTests]
        public static string MinifyJs(string originalJS) => new JavaScriptCompressor().Compress(originalJS);
    }
}
