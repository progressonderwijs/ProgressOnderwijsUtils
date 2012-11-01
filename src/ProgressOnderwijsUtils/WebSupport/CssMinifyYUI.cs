﻿using System;
using System.Collections.Generic;
using System.Linq;
using Yahoo.Yui.Compressor;

namespace ProgressOnderwijsUtils.WebSupport
{
	public static class CssMinifyYUI
	{
		static readonly CssCompressor cssCompressor = new CssCompressor();
		public static string MinifyCss(string originalCss) { return cssCompressor.Compress(originalCss); }
		static readonly JavaScriptCompressor jsCompressor = new JavaScriptCompressor();
		public static string MinifyJs(string originalJS) { return jsCompressor.Compress(originalJS); }
	}
}
