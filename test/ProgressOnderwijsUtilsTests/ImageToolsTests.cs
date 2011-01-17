using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Test.Properties;
using ProgressOnderwijsUtils;
using System.IO;

namespace ProgressOnderwijsUtilsTests
{
	static class Helper
	{
		public static double Distance(this Color a, Color b)
		{
			return (Math.Abs(a.G - b.G) * 0.5 + Math.Abs(a.R - b.R) * 0.35 + Math.Abs(a.B - b.B) * 0.15) / 255.0;
		}
	}
	[TestFixture]
	public class ImageToolsTests
	{
		[Test]
		public void CanResaveImage()
		{
			var resImage = Resources.rainbow;
			using (var ms = new MemoryStream())
			{
				ImageTools.SaveImageAsJpeg(resImage, ms, 100);
				using (var loadedImage = (Bitmap)ImageTools.ToImage(ms.ToArray()))
				{
					PAssert.That(() => loadedImage.Width == resImage.Width && loadedImage.Height == resImage.Height);
					PAssert.That(() =>
						!(from y in Enumerable.Range(0, loadedImage.Height)
						  from x in Enumerable.Range(0, loadedImage.Width)
						  select new { x, y }).Where(p => loadedImage.GetPixel(p.x, p.y).Distance(resImage.GetPixel(p.x, p.y)) > 0.05).Any());
				}
			}
		}
	}
}
