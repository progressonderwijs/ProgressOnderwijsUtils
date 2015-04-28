using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Test.Properties;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
    static class Helper
    {
        public static double Distance(this Color a, Color b) { return (Math.Abs(a.G - b.G) * 0.5 + Math.Abs(a.R - b.R) * 0.35 + Math.Abs(a.B - b.B) * 0.15) / 255.0; }
    }

    [Continuous]
    public class ImageToolsTests
    {
        [Test]
        public void CanResaveImage()
        {
            var resImage = Resources.rainbow;
            using (var ms = new MemoryStream()) {
                ImageTools.SaveImageAsJpeg(resImage, ms, 100);
                using (var loadedImage = (Bitmap)ImageTools.ToImage(ms.ToArray())) {
                    PAssert.That(() => loadedImage.Width == resImage.Width && loadedImage.Height == resImage.Height);
                    PAssert.That(
                        () =>
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
            using (var down_WH = ImageTools.Resize(resImage, 50, 50)) {
                AssertImagesSimilar(down_W_H, down_WH);
                AssertImagesSimilar(down_H_W, down_WH);
                AssertImagesSimilar(down_H_W, down_W_H);
                PAssert.That(() => down_W_H.GetPixel(45, 25) != down_W_H.GetPixel(10, 30));
            }
        }

        [Test]
        public void CannotResizeToZero()
        {
            var resImage = Resources.rainbow;
            Assert.Throws<ArgumentException>(() => ImageTools.Resize(resImage, 100, 0));
        }

        static void AssertImagesSimilar(Bitmap img1, Bitmap img2)
        {
            PAssert.That(() => img1.Size == img2.Size);

            var pixelsDiffs = (
                from y in Enumerable.Range(0, img1.Height)
                from x in Enumerable.Range(0, img1.Width)
                let err = img1.GetPixel(x, y).Distance(img2.GetPixel(x, y))
                select new { x, y, err });

            var bitness = 8 * IntPtr.Size;
            var accuracy = bitness == 32 ? 0.1 : 0.025;

            var badPixels = pixelsDiffs.Where(diff => diff.err > accuracy);
            PAssert.That(() => !badPixels.Any());
        }

        [Test]
        public void CanDownscaleImage()
        {
            var resImage = Resources.rainbow;
            using (var down_W = ImageTools.DownscaleAndClip(resImage, 100, 50))
            using (var down_H = ImageTools.DownscaleAndClip(resImage, 2000, 1000))
            using (var down_W_H = ImageTools.DownscaleAndClip(down_W, 50, 50))
            using (var down_H_W = ImageTools.DownscaleAndClip(down_H, 50, 50)) {
                AssertImagesSimilar(down_H_W, down_W_H);
                PAssert.That(() => down_W_H.GetPixel(25, 25) != down_W_H.GetPixel(20, 20));
            }
        }
    }
}
