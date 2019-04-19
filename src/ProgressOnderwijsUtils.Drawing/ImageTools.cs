using System;
using System.IO;
using JetBrains.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.Primitives;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ProgressOnderwijsUtils.Drawing
{
    public static class ImageTools
    {
        [NotNull]
        public static Image<TPixel> Resize<TPixel>([NotNull] Image<TPixel> oldImage, int newWidth, int newHeight)
            where TPixel : struct, IPixel<TPixel>
        {
            if (newWidth <= 0 || newHeight <= 0) {
                throw new ArgumentException($"Can only resize to positive dimensions, not {newWidth}x{newHeight}");
            }
            return oldImage.Clone(context => context.Resize(newWidth, newHeight, KnownResamplers.Lanczos2, true));
        }

        /// <summary>
        /// Resizes an image while maintaining aspect ratio.  It does this by chosing the largest possible centered rectangle in the old image that has
        /// the same aspect ratio as the output dimensions.  This rectangle is then resized to the new dimensions, unless it's smaller, then only
        /// clipping occurs.
        /// </summary>
        /// <param name="oldImage">The image to resize</param>
        /// <param name="newWidth">The target width</param>
        /// <param name="newHeight">The target height</param>
        /// <returns>A new Bitmap (don't forget to Dispose it!)</returns>
        [NotNull]
        public static Image<TPixel> DownscaleAndClip<TPixel>([NotNull] Image<TPixel> oldImage, int newWidth, int newHeight)
            where TPixel : struct, IPixel<TPixel>
        {
            var oldWidth = oldImage.Width;
            var oldHeight = oldImage.Height;
            var clipRectangle = ClipRectangle(oldWidth, oldHeight, newWidth, newHeight);
            return oldImage.Clone(context => {
                context.Crop(clipRectangle);
                if (newWidth * newHeight < clipRectangle.Width * clipRectangle.Height) {
                    context.Resize(newWidth, newHeight, KnownResamplers.Lanczos2, true);
                }
            });
        }

        /// <summary>Converts any image format to a known aspect ratio, saving as Jpeg; returns the bytes of the new jpeg file or NULL if the
        /// source image is corrupt of too large (i.e. has a dimension greater than MAX_IMAGE_DIMENSION) It does this by chosing the largest
        /// possible centered rectangle in the old image that has the same aspect ratio as the output dimensions.  This rectangle is then resized
        /// to the new dimensions, unless it's smaller, then only clipping occurs.
        /// 
        /// SECURITY: this method is safe only if https://github.com/SixLabors/ImageSharp is, and specifically the Image.Identify and Image.Load methods
        /// SECURITY: the image is _always_ converted, never stored raw, so corrupted image attacks against users (other than our system) cannot occur.
        /// </summary>
        /// <param name="origImageData">The bytes of the image to resize</param>
        /// <param name="targetWidth">The target width</param>
        /// <param name="targetHeight">The target height</param>
        /// <returns>The bytes of the resulting JPEG, or NULL if the original is corrupt or too large.</returns>
        [CanBeNull]
        public static byte[] Downscale_Clip_ConvertToJpeg([NotNull] byte[] origImageData, int targetWidth, int targetHeight)
        {
            using (var uploadedImage = ToImage(origImageData))
            using (var thumbnail = DownscaleAndClip(uploadedImage, targetWidth, targetHeight))
            using (var ms = new MemoryStream()) {
                SaveImageAsJpeg(thumbnail, ms); //always resave as a security precaution.
                return ms.ToArray();
            }
        }

        const int MAX_IMAGE_DIMENSION = 10_000; //10 000 by 10 000 is quite insane ;-)

        [CanBeNull]
        public static Image<Rgba32> ToImage([NotNull] byte[] arr)
        {
            using (var mem = new MemoryStream(arr)) {
                var metadata = Image.Identify(mem);
                var oldHeight = metadata.Height;
                var oldWidth = metadata.Width;

                if (oldHeight > MAX_IMAGE_DIMENSION || oldWidth > MAX_IMAGE_DIMENSION || oldHeight < 1 || oldWidth < 1) {
                    return null;
                }
            }
            return Image.Load(arr);
        }

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

        public static void SaveImageAsJpeg<TPixel>([NotNull] Image<TPixel> image, [NotNull] Stream outputStream, int? quality = null)
            where TPixel : struct, IPixel<TPixel>
            => image.Save(outputStream, new JpegEncoder { Quality = quality ?? 90 });

        public static void SaveResizedImage([NotNull] byte[] pasfoto, int targetWidth, int targetHeight, [NotNull] Stream outputStream)
        {
            using (var fromDatabase = ToImage(pasfoto)) {
                if (fromDatabase.Height == targetHeight) {
                    outputStream.Write(pasfoto, 0, pasfoto.Length);
                } else {
                    using (var smallImage = Resize(fromDatabase, targetWidth, targetHeight)) {
                        SaveImageAsJpeg(smallImage, outputStream);
                    }
                }
            }
        }
    }
}
