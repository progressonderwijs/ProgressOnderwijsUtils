using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

//this is not recommended?? downsides are unclear though some suggest it might be slow.

namespace ProgressOnderwijsUtils.Drawing
{
    // Uses tips from http://www.glennjones.net/Post/799/Highqualitydynamicallyresizedimageswithnet.htm
    /// <summary>
    /// Provides a number of wrappers around idiotically designed core framework functionality.  Nothing fancy, just sets flags to high-quality, finds a jpeg encoder etc.
    /// Be aware that Images (and Bitmaps) are IDisposable and it's strongly suggested you dispose them when done.  Because exceptions can cause unexpected code flow, you should do so by
    /// using(Image myImage = ImageTools.Resize(...)) { [...use new image...] }
    /// The caller is responsible for disposing images!  (BAH memory allocation... bah...)
    /// Finally, MSDN suggests that the entirety of imaging may be slow in ASP.NET... well, there's no alternative, so here's to hoping that's no longer true.
    /// </summary>
    public static class ImageTools
    {
        /// <summary>
        /// Sets a graphics object to use high quality primitives (mostly for various scaling/blending operations)
        /// </summary>
        public static void setHQ([NotNull] Graphics g)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        }

        [NotNull]
        public static Bitmap Resize([NotNull] Image oldImage, int newWidth, int newHeight)
        {
            Bitmap bitmap = null;
            try {
                bitmap = new Bitmap(newWidth, newHeight);
                using (var g = Graphics.FromImage(bitmap)) {
                    setHQ(g);
                    g.DrawImage(oldImage, new Rectangle(0, 0, newWidth, newHeight));
                }
            } catch {
                bitmap?.Dispose();
                throw;
            }
            return bitmap;
        }

        /// <summary>
        /// Resizes an image while maintaining aspect ratio.  It does this by
        /// chosing the largest possible centered rectangle in the old image
        /// that has the same aspect ratio as the output dimensions.  This rectangle
        /// is then resized to the new dimensions, unless it's smaller, then only
        /// clipping occurs.
        /// </summary>
        /// <param name="oldImage">The image to resize</param>
        /// <param name="newWidth">The target width</param>
        /// <param name="newHeight">The target height</param>
        /// <returns>A new Bitmap (don't forget to Dispose it!)</returns>
        [NotNull]
        public static Bitmap DownscaleAndClip([NotNull] Image oldImage, int newWidth, int newHeight)
        {
            var oldWidth = oldImage.Width;
            var oldHeight = oldImage.Height;
            Bitmap bitmap = null;
            var clipRectangle = ClipRectangle(oldWidth, oldHeight, newWidth, newHeight);
            if (clipRectangle.Width * clipRectangle.Height < newWidth * newHeight) {
                newWidth = clipRectangle.Width;
                newHeight = clipRectangle.Height;
            }

            try {
                bitmap = new Bitmap(newWidth, newHeight);
                using (var g = Graphics.FromImage(bitmap)) {
                    setHQ(g);
                    g.DrawImage(
                        oldImage,
                        new Rectangle(0, 0, newWidth, newHeight),
                        clipRectangle,
                        GraphicsUnit.Pixel);
                } //done with drawing on "g"
            } catch {
                bitmap?.Dispose();
                throw;
            }
            return bitmap;
        }

        /// <summary>Converts any image format to a known aspect ratio, saving as Jpeg; returns the bytes of the new
        /// jpeg file or NULL if the source image is corrupt of too large (i.e. has a dimension greater than MAX_IMAGE_DIMENSION)
        /// It does this by chosing the largest possible centered rectangle in the old image
        /// that has the same aspect ratio as the output dimensions.  This rectangle
        /// is then resized to the new dimensions, unless it's smaller, then only
        /// clipping occurs.
        /// 
        /// SECURITY: this method is safe if and only if microsoft's Image.FromStream is safe.
        /// SECURITY: the image is _always_ converted, never stored raw, so corrupted image attacks against other users cannot occur.
        /// </summary>
        /// <param name="origImageData">The bytes of the image to resize</param>
        /// <param name="targetWidth">The target width</param>
        /// <param name="targetHeight">The target height</param>
        /// <returns>The bytes of the resulting JPEG, or NULL if the original is corrupt or too large.</returns>
        [CanBeNull]
        public static byte[] Downscale_Clip_ConvertToJpeg([NotNull] byte[] origImageData, int targetWidth, int targetHeight)
        {
            using (var uploadedImage = ToImage(origImageData)) {
                var oldHeight = uploadedImage.Height;
                var oldWidth = uploadedImage.Width;

                if (oldHeight > MAX_IMAGE_DIMENSION || oldWidth > MAX_IMAGE_DIMENSION || oldHeight < 1 || oldWidth < 1) {
                    return null;
                }
                using (var thumbnail = DownscaleAndClip(uploadedImage, targetWidth, targetHeight))
                using (var ms = new MemoryStream()) {
                    SaveImageAsJpeg(thumbnail, ms); //always resave as a security precaution.
                    return ms.ToArray();
                }
            }
        }

        const int MAX_IMAGE_DIMENSION = 10000; //10 000 by 10 000 is quite insane ;-)
        [NotNull]
        public static Image ToImage([NotNull] byte[] arr) => Image.FromStream(new MemoryStream(arr), true, true);

        /// <summary>
        /// Given a width and a height, and a aspect ratio, computes the clipping rectangle fitting within that width and height but of the right aspect ratio.
        /// The aspect ratio is specified by giving a sample width and height.  These parameters can scaled arbirarily as long as the ratio remains the same (and no integer overflow occurs)
        /// For example ClipRectangle(w,h,4,3) returns the same result as ClipRectangle(w,h,8,6).
        /// </summary>
        /// <param name="oldWidth">The width of the image</param>
        /// <param name="oldHeight">The height of the image</param>
        /// <param name="aspectWidth">The aspect width</param>
        /// <param name="aspectHeight">The aspect height</param>
        /// <returns>A maximally sized, centered clipping rectangle no larger than the specified dimensions.</returns>
        public static Rectangle ClipRectangle(int oldWidth, int oldHeight, int aspectWidth, int aspectHeight)
        {
            //is the input image too wide or too tall?
            var tooWide = oldWidth * aspectHeight > aspectWidth * oldHeight;

            var clipWidth = tooWide ? (oldHeight * aspectWidth + aspectHeight / 2) / aspectHeight : oldWidth;
            var clipHeight = tooWide ? oldHeight : (oldWidth * aspectHeight + aspectWidth / 2) / aspectWidth;
            var clipX = (oldWidth - clipWidth) / 2;
            var clipY = (oldHeight - clipHeight) / 2;
            return new Rectangle(clipX, clipY, clipWidth, clipHeight);
        }

        public static void SaveImageAsJpeg([NotNull] Image image, [NotNull] Stream outputStream, int? quality = null)
        {
            var jpgInfo = ImageCodecInfo.GetImageEncoders().First(codecInfo => codecInfo.MimeType == "image/jpeg");
            using (var encParams = new EncoderParameters(1)) {
                encParams.Param[0] = new EncoderParameter(Encoder.Quality, quality ?? 90L); //quality should be in the range [0..100]
                image.Save(outputStream, jpgInfo, encParams);
            }
        }

        public static void SaveResizedImage([NotNull] byte[] pasfoto, int targetWidth, int targetHeight, [NotNull] Stream outputStream)
        {
            using (var fromDatabase = ToImage(pasfoto))
                if (fromDatabase.Height == targetHeight) {
                    outputStream.Write(pasfoto, 0, pasfoto.Length);
                } else {
                    using (Image smallImage = Resize(fromDatabase, targetWidth, targetHeight))
                        SaveImageAsJpeg(smallImage, outputStream);
                }
        }
    }
}
