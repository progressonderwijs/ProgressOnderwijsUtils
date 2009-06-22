using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils.WebSupport
{
	public static class JavascriptMinifyYUI
	{
		public static string Minify(string originalJS)
		{
			return Yahoo.Yui.Compressor.JavaScriptCompressor.Compress(originalJS);
		}
	}
}
