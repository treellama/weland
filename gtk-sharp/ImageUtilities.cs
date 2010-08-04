using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Weland {
    public class ImageUtilities {

	public static Gdk.Pixbuf ImageToPixbuf(Image image) {
	    MemoryStream stream = new MemoryStream();
	    image.Save(stream, ImageFormat.Bmp);
	    stream.Position = 0;
	    return new Gdk.Pixbuf(stream);
	}

	public static System.Drawing.Bitmap ResizeImage(Image image, int width, int height) {
	    System.Drawing.Bitmap scaled = new System.Drawing.Bitmap(width, height);
	    using (Graphics graphics = Graphics.FromImage(scaled)) {
		graphics.DrawImage(image, new Rectangle(0, 0, scaled.Width, scaled.Height));
	    }

	    return scaled;
	}
    }
}