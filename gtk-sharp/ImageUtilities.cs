using SkiaSharp;
using System;

namespace Weland
{
    public class ImageUtilities
    {
        public static Gdk.Pixbuf ImageToPixbuf(SKBitmap bitmap)
        {
            using SKPixmap pixmap = bitmap.PeekPixels();
            using SKImage skiaImage = SKImage.FromPixels(pixmap);

            var info = new SKImageInfo(skiaImage.Width, skiaImage.Height);
            var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, has_alpha: true, 8, info.Width, info.Height);

            using (var newPixMap = new SKPixmap(info, pixbuf.Pixels, pixbuf.Rowstride))
            {
                skiaImage.ReadPixels(newPixMap, 0, 0);
            }

            if (info.ColorType == SKColorType.Bgra8888)
            {
                SKSwizzle.SwapRedBlue(pixbuf.Pixels, info.Width * info.Height);
            }

            GC.KeepAlive(bitmap);
            return pixbuf;
        }

        public static SKBitmap ResizeImage(SKBitmap image, int width, int height)
        {
            return image.Resize(new SKImageInfo(width, height), SKFilterQuality.None);
        }

        public static SKBitmap RotateBitmap(SKBitmap image, float degrees)
        {
            var rotatedImage = new SKBitmap(image.Height, image.Width);
            using (var canvas = new SKCanvas(rotatedImage))
            {
                canvas.Clear(SKColors.White);
                canvas.Translate(rotatedImage.Width, 0);
                canvas.RotateDegrees(degrees);
                canvas.DrawBitmap(image, 0, 0);
            }

            return rotatedImage;
        }
    }
}