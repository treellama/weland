using Gdk;
using System;
using System.Collections.Generic;

namespace Weland {
    public class GdkDrawer : Drawer {
	class TextureCache : IDisposable {
	    public TextureCache() {
		Weland.ShapesChanged += new ShapesFileChangedEventHandler(cache.Clear);
	    }

	    public void Dispose() {
		Weland.ShapesChanged -= new ShapesFileChangedEventHandler(cache.Clear);
	    }

	    public Gdk.Pixmap GetPixmap(ShapeDescriptor d) {
		if (!cache.ContainsKey((ushort) d)) {
		    System.Drawing.Bitmap bitmap = Weland.Shapes.GetShape(d);
		    Gdk.Pixbuf pixbuf = ImageUtilities.ImageToPixbuf(bitmap);
		    Gdk.Pixmap pixmap;
		    Gdk.Pixmap mask;
		    pixbuf.RenderPixmapAndMask(out pixmap, out mask, 0);
		    cache.Add((ushort) d, pixmap);
		}

		return cache[(ushort) d];
	    }

	    Dictionary<ushort, Gdk.Pixmap> cache = new Dictionary<ushort, Gdk.Pixmap>();  
	}

	Gdk.Window window;
	public GdkDrawer(Gdk.Window w) { 
	    window = w;
	}

	static TextureCache cache = new TextureCache();

	Gdk.Color GdkColor(Color c) {
	    return new Gdk.Color((byte) (c.R * 255), (byte) (c.G * 255), (byte) (c.B * 255));
	}

	public override void Clear(Color c) { 
	    Gdk.GC g = new Gdk.GC(window);
	    g.RgbFgColor = GdkColor(c);
	    g.RgbBgColor = GdkColor(c);
	    int Width;
	    int Height;
	    window.GetSize(out Width, out Height);
	    window.DrawRectangle(g, true, 0, 0, Width, Height);
	}

	public override void DrawPoint(Color c, Point p) { 
	    Gdk.GC g = new Gdk.GC(window);
	    g.RgbFgColor = GdkColor(c);
	    g.RgbBgColor = GdkColor(c);
	    window.DrawRectangle(g, true, (int) p.X, (int) p.Y, 2, 2);
	}

	public override void DrawLine(Color c, Point p1, Point p2) { 
	    Gdk.GC g = new Gdk.GC(window);
	    
	    g.RgbFgColor = GdkColor(c);
	    window.DrawLine(g, (int) p1.X, (int) p1.Y, (int) p2.X, (int) p2.Y);
	}

	public override void FillPolygon(Color c, List<Point> points) { 
	    Gdk.Point[] pointArray = new Gdk.Point[points.Count];
	    for (int i = 0; i < points.Count; ++i) {
		pointArray[i].X = (int) points[i].X;
		pointArray[i].Y = (int) points[i].Y;
	    }

	    Gdk.GC g = new Gdk.GC(window);
	    g.RgbFgColor = GdkColor(c);
	    window.DrawPolygon(g, true, pointArray);
	}

	public override void FillStrokePolygon(Color fill, Color stroke, List<Point> points, bool dashed) {
	    Gdk.Point[] pointArray = new Gdk.Point[points.Count];
	    for (int i = 0; i < points.Count; ++i) {
		pointArray[i].X = (int) points[i].X;
		pointArray[i].Y = (int) points[i].Y;
	    }

	    Gdk.GC g = new Gdk.GC(window);
	    g.RgbFgColor = GdkColor(fill);
	    if (dashed) {
		g.SetLineAttributes(1, LineStyle.OnOffDash, CapStyle.NotLast, JoinStyle.Miter);
		g.SetDashes(0, new sbyte[] { 2 }, 1);
	    }
	    window.DrawPolygon(g, true, pointArray);
	    g.RgbFgColor = GdkColor(stroke);
	    window.DrawPolygon(g, false, pointArray);
	}

	public override void DrawGridIntersect(Color c, Point p) { 
	    Gdk.GC g = new Gdk.GC(window);
	    g.RgbFgColor = GdkColor(c);
	    window.DrawLine(g, (int) p.X, (int) (p.Y - 1), (int) p.X, (int) (p.Y + 1));
	    window.DrawLine(g, (int) (p.X - 1), (int) p.Y, (int) (p.X + 1), (int) p.Y);
	}

	public override void TexturePolygon(ShapeDescriptor d, List<Point> points) { 
	    Gdk.Point[] pointArray = new Gdk.Point[points.Count];
	    for (int i = 0; i < points.Count; ++i) {
		pointArray[i].X = (int) points[i].X;
		pointArray[i].Y = (int) points[i].Y;
	    }

	    Gdk.GC g = new Gdk.GC(window);
	    g.Tile = cache.GetPixmap(d);
	    g.Fill = Gdk.Fill.Tiled;
	    window.DrawPolygon(g, true, pointArray);
	}

	public override void Dispose() {
	    window = null;
	}
    }
}