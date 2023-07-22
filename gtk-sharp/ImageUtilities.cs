using SkiaSharp;
using BmpSharp;

namespace Weland
{
    public class ImageUtilities
    {
        public static Gdk.Pixbuf ImageToPixbuf(SKBitmap bitmap)
        {
            var bitmapToEncode = new BmpSharp.Bitmap(bitmap.Width, bitmap.Height, bitmap.GetPixelSpan().ToArray(), BitsPerPixelEnum.RGBA32);
            return new Gdk.Pixbuf(bitmapToEncode.GetBmpStream(true));
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