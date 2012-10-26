using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Test.Properties;
using ProgressOnderwijsUtils;

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

		[Test]
		public void CanResizeImage()
		{
			var resImage = Resources.rainbow;
			using (var down_W = ImageTools.Resize(resImage, 100, resImage.Height))
			using (var down_H = ImageTools.Resize(resImage, resImage.Width, 100))
			using (var down_W_H = ImageTools.Resize(down_W, 50, 50))
			using (var down_H_W = ImageTools.Resize(down_H, 50, 50))
			using (var down_WH = ImageTools.Resize(resImage, 50, 50))
			{
				PAssert.That(() =>
					!(from y in Enumerable.Range(0,50)
					  from x in Enumerable.Range(0, 50)
					  select new { x, y }).Where(p => down_W_H.GetPixel(p.x, p.y).Distance(down_WH.GetPixel(p.x, p.y)) > 0.02).Any());
				PAssert.That(() =>
					!(from y in Enumerable.Range(0, 50)
					  from x in Enumerable.Range(0, 50)
					  select new { x, y }).Where(p => down_H_W.GetPixel(p.x, p.y).Distance(down_WH.GetPixel(p.x, p.y)) > 0.02).Any());
				PAssert.That(() =>
					!(from y in Enumerable.Range(0, 50)
					  from x in Enumerable.Range(0, 50)
					  select new { x, y }).Where(p => down_H_W.GetPixel(p.x, p.y).Distance(down_W_H.GetPixel(p.x, p.y)) > 0.02).Any());

				Assert.Throws<ArgumentException>(() => ImageTools.Resize(resImage, 100, 0));

			}
		}

		[Test]
		public void CanDownscaleImage()
		{
			var resImage = Resources.rainbow;
			using (var down_W = ImageTools.DownscaleAndClip(resImage, 100, 50))
			using (var down_H = ImageTools.DownscaleAndClip(resImage, 2000, 1000))
			using (var down_W_H = ImageTools.DownscaleAndClip(down_W, 50, 50))
			using (var down_H_W = ImageTools.DownscaleAndClip(down_H, 50, 50))
			{
				PAssert.That(() =>
					!(from y in Enumerable.Range(0, 50)
					  from x in Enumerable.Range(0, 50)
					  select new { x, y }).Where(p => down_H_W.GetPixel(p.x, p.y).Distance(down_W_H.GetPixel(p.x, p.y)) > 0.05).Any());
			}
			Assert.Throws<ArgumentException>(() => ImageTools.Resize(resImage, 100, 0));
		}

	}
}
