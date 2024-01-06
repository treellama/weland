using Cairo;
using System.Runtime.InteropServices;

namespace Weland
{
    public class ImageUtilities
    {
        public static Gdk.Pixbuf ImageToPixbuf(Image image)
        {
            return new Gdk.Pixbuf(image.Data, false, 8, image.Width, image.Height, image.Width * 3);
        }

        public static ImageSurface ImageToSurface(Image image)
        {
            var surface = new ImageSurface(Format.RGB24, image.Width, image.Height);
            var bytes = image.ToBGRA();
            surface.Flush();
            Marshal.Copy(bytes, 0, surface.DataPtr, bytes.Length);
            surface.MarkDirty();

            return surface;
        }

        public static Image ResizeImage(Image image, int width, int height)
        {
            var result = new Image(width, height);
            using (var src = ImageToSurface(image))
            {
                using (var dst = new ImageSurface(Format.RGB24, width, height))
                {
                    using (var ctx = new Context(dst))
                    {
                        ctx.Scale((double) width / image.Width, (double) height / image.Height);
                        ctx.SetSource(src, 0, 0);
                        ctx.Operator = Operator.Source;
                        ctx.Paint();
                    }

                    var bytes = new byte[width * height * 4];
                    dst.Flush();
                    Marshal.Copy(dst.DataPtr, bytes, 0, bytes.Length);

                    result.FromBGRA(bytes);
                }
            }

            return result;
        }
    }
}
