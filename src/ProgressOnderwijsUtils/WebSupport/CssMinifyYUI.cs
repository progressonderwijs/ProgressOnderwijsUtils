using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils.WebSupport
{
	public static class CssMinifyYUI
	{
		public static string Minify(string originalCss) { return Yahoo.Yui.Compressor.CssCompressor.Compress(originalCss); }
	}
}
