using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;//this is not recommended?? downsides are unclear though some suggest it might be slow.
using System.IO;
namespace ProgressOnderwijsUtils
{
	// Uses tips from http://www.glennjones.net/Post/799/Highqualitydynamicallyresizedimageswithnet.htm
	/// <summary>
	/// Provides a number of wrappers around idiotically designed core framework functionality.  Nothing fancy, just sets flags to high-quality, finds a jpeg encoder etc.
	/// </summary>
	public static class ImageTools
	{
		/// <summary>
		/// Sets a graphics object to use high quality primitives (mostly for various scaling/blending operations)
		/// </summary>
		/// <param name="g"></param>
		public static void setHQ(Graphics g) {
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
			g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
		}

		public static Bitmap Resize(Image oldImage, int newWidth, int newHeight)
		{
			Bitmap bitmap=null;
			try
			{
				bitmap = new Bitmap(newWidth, newHeight);
				using (Graphics g = Graphics.FromImage(bitmap))
				{
					setHQ(g);
					g.DrawImage(oldImage, new Rectangle(0, 0, newWidth, newHeight));
				}
			}
			catch
			{
				if (bitmap!=null) bitmap.Dispose();
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
		public static Bitmap DownscaleAndClip(Image oldImage, int newWidth, int newHeight)
		{
			int oldWidth = oldImage.Width;
			int oldHeight = oldImage.Height;
			Bitmap bitmap = null;
			Rectangle clipRectangle = ImageTools.ClipRectangle(oldWidth, oldHeight, newWidth, newHeight);
			if (clipRectangle.Width * clipRectangle.Height < newWidth * newHeight)
			{
				newWidth=clipRectangle.Width;
				newHeight=clipRectangle.Height;
			}

			try
			{
				bitmap = new Bitmap(newWidth,newHeight);
				using (Graphics g = Graphics.FromImage(bitmap))
				{
					setHQ(g);
					g.DrawImage(oldImage,
						new Rectangle(0,0,newWidth,newHeight),
						clipRectangle, GraphicsUnit.Pixel);
				}//done with drawing on "g"
			}
			catch
			{
				if (bitmap != null) bitmap.Dispose();
				throw;
			}
			return bitmap;
		}



		public static Image ToImage(byte[] arr) { return ToImage(new MemoryStream(arr)); }

		public static Image ToImage(Stream str) { return Image.FromStream(str, true, true); }


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
			bool tooWide = oldWidth * aspectHeight > aspectWidth * oldHeight;

			int clipWidth = tooWide ? (oldHeight * aspectWidth+aspectHeight/2) / aspectHeight : oldWidth;
			int clipHeight = tooWide ? oldHeight : (oldWidth * aspectHeight + aspectWidth/2) / aspectWidth;
			int clipX = (oldWidth - clipWidth) / 2;
			int clipY = (oldHeight - clipHeight) / 2;
			return new Rectangle(clipX, clipY, clipWidth, clipHeight);
		}

		public static void SaveImageAsJpeg(Image image, Stream stream, int quality)
		{
			ImageCodecInfo[] Info = ImageCodecInfo.GetImageEncoders();
			ImageCodecInfo jpgInfo = Array.Find(Info, delegate(ImageCodecInfo i) { return i.MimeType == "image/jpeg"; });
			using (EncoderParameters encParams = new EncoderParameters(1))
			{
				encParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
				image.Save(stream, jpgInfo, encParams);
			}
		}

		public static void SaveImageAsJpeg(Image image, Stream stream)
		{
			SaveImageAsJpeg(image, stream, 90);
		}
	}
}
