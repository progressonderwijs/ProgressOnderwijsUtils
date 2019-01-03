using System;
using System.Drawing;
using System.IO;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using Xunit;

namespace ProgressOnderwijsUtils.Drawing.Tests
{
    static class Helper
    {
        public static double Distance(this Color a, Color b)
            => (Math.Abs(a.G - b.G) * 0.5 + Math.Abs(a.R - b.R) * 0.35 + Math.Abs(a.B - b.B) * 0.15) / 255.0;
    }

    public sealed class ImageToolsTests
    {
        [NotNull]
        static Bitmap GetRainbow()
        {
            return (Bitmap)Image.FromStream(
                typeof(ImageToolsTests).Assembly.GetManifestResourceStream(typeof(ImageToolsTests), "rainbow.jpg")
                ?? throw new Exception("rainbow.jpg is not embedded")
            );
        }

        static void AssertImagesSimilar([NotNull] Bitmap img1, Bitmap img2)
        {
            PAssert.That(() => img1.Size == img2.Size);

            var pixelsDiffs =
                from y in Enumerable.Range(0, img1.Height)
                from x in Enumerable.Range(0, img1.Width)
                let err = img1.GetPixel(x, y).Distance(img2.GetPixel(x, y))
                select new { x, y, err };

            var bitness = 8 * IntPtr.Size;
            var accuracy = bitness == 32 ? 0.1 : 0.025; //for some odd reason, the 32-bit clr has much lower image scaling accuracy...

            var badPixels = pixelsDiffs.Where(diff => diff.err > accuracy);
            PAssert.That(() => !badPixels.Any());
        }

        [Fact]
        public void CanDownscaleImage()
        {
            using (var resImage = GetRainbow())
            using (var down_W = ImageTools.DownscaleAndClip(resImage, 100, 50))
            using (var down_H = ImageTools.DownscaleAndClip(resImage, 2000, 1000))
            using (var down_W_H = ImageTools.DownscaleAndClip(down_W, 50, 50))
            using (var down_H_W = ImageTools.DownscaleAndClip(down_H, 50, 50)) {
                AssertImagesSimilar(down_H_W, down_W_H);

                //als het test-plaatje geen contrast heeft, dan is deze hele test zinloos: verifieer dat het test plaatje contrast heeft:
                PAssert.That(() => down_W_H.GetPixel(25, 25) != down_W_H.GetPixel(20, 20));
            }
        }

        [Fact]
        public void CannotResizeToZero()
        {
            using (var resImage = GetRainbow())
                Assert.Throws<ArgumentException>(() => ImageTools.Resize(resImage, 100, 0));
        }

        [Fact]
        public void CanResaveImage()
        {
            using (var resImage = GetRainbow())
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

        [Fact]
        public void CanResizeImage()
        {
            using (var resImage = GetRainbow())
            using (var down_W = ImageTools.Resize(resImage, 100, resImage.Height))
            using (var down_H = ImageTools.Resize(resImage, resImage.Width, 100))
            using (var down_W_H = ImageTools.Resize(down_W, 50, 50))
            using (var down_H_W = ImageTools.Resize(down_H, 50, 50))
            using (var down_WH = ImageTools.Resize(resImage, 50, 50)) {
                AssertImagesSimilar(down_W_H, down_WH);
                AssertImagesSimilar(down_H_W, down_WH);
                AssertImagesSimilar(down_H_W, down_W_H);

                //als het test-plaatje geen contrast heeft, dan is deze hele test zinloos: verifieer dat het test plaatje contrast heeft:
                PAssert.That(() => down_W_H.GetPixel(45, 25) != down_W_H.GetPixel(10, 30));
            }
        }
    }
}
