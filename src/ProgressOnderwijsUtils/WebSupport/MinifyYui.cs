using System;
using System.Collections.Generic;
using System.Linq;
using Yahoo.Yui.Compressor;

namespace ProgressOnderwijsUtils.WebSupport
{
	public static class MinifyYui
	{
		//static readonly CssCompressor cssCompressor = new CssCompressor();
		public static string MinifyCss(string originalCss) { return new CssCompressor().Compress(originalCss); }
		//static readonly JavaScriptCompressor jsCompressor = new JavaScriptCompressor();
		public static string MinifyJs(string originalJS) { return new JavaScriptCompressor().Compress(originalJS); }
	}
}
