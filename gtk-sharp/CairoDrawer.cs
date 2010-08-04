using Cairo;
using System;
using System.Collections.Generic;

namespace Weland {
    public class CairoDrawer : Drawer {
	Context context;
	public CairoDrawer(Gdk.Window window, bool antialias) {
	    context = Gdk.CairoHelper.Create(window);
	    if (!antialias) {
		context.Antialias = Antialias.None;
	    }
	}

	class TextureCache : IDisposable {
	    public TextureCache() {
		Weland.ShapesChanged += new ShapesFileChangedEventHandler(OnShapesChanged);
	    }

	    public void Dispose() {
		Weland.ShapesChanged -= new ShapesFileChangedEventHandler(OnShapesChanged);
	    }

	    public void OnShapesChanged() {
		bcache.Clear();
		cache.Clear();
	    }

	    public ImageSurface GetSurface(ShapeDescriptor d) {
		if (!cache.ContainsKey((ushort) d)) {
		    System.Drawing.Bitmap bitmap = Weland.Shapes.GetShape(d);
		    byte[] bytes = new byte[bitmap.Width * bitmap.Height * 4];
		    for (int x = 0; x < bitmap.Width; ++x) {
			for (int y = 0; y < bitmap.Height; ++y) {
			    System.Drawing.Color c  = bitmap.GetPixel(x, y);
			    int offset = (y * bitmap.Width + x) * 4;
			    bytes[offset] = c.B;
			    bytes[offset + 1] = c.G;
			    bytes[offset + 2] = c.R;
			    bytes[offset + 3] = c.A;
			}
		    }
		    
		    bcache.Add((ushort) d, bytes);
		    cache.Add((ushort) d, new ImageSurface(bytes, Format.ARGB32, bitmap.Width, bitmap.Height, bitmap.Width * 4));
		}
		return cache[(ushort) d];
	    }

	    Dictionary<ushort, ImageSurface> cache = new Dictionary<ushort, ImageSurface>();
	    Dictionary<ushort, byte[]> bcache = new Dictionary<ushort, byte[]>();
	}

	static TextureCache cache = new TextureCache();

	public override void Clear(Color c) {
	    context.Save();

	    context.Color = new Cairo.Color(c.R, c.G, c.B);
	    context.Paint();

	    context.Restore();
	}

	public override void DrawPoint(Color c, Point p) {
	    context.Save();

	    context.MoveTo(new PointD(p.X + 0.5, p.Y + 0.5));
	    context.ClosePath();
	    context.LineCap = LineCap.Round;
	    context.Color = new Cairo.Color(c.R, c.G, c.B);
	    context.LineWidth = 2.5;
	    context.Stroke();

	    context.Restore();
	}

	public override void DrawGridIntersect(Color c, Point p) {
	    context.Save();

	    context.MoveTo(new PointD(p.X - 0.5, p.Y + 0.5));
	    context.LineTo(new PointD(p.X + 1.5, p.Y + 0.5));
	    context.ClosePath();

	    context.MoveTo(new PointD(p.X + 0.5, p.Y - 0.5));
	    context.LineTo(new PointD(p.X + 0.5, p.Y + 1.5));
	    context.ClosePath();

	    context.Color = new Cairo.Color(c.R, c.G, c.B);
	    context.LineWidth = 1.0;
	    context.Stroke();

	    context.Restore();
	}

	public override void DrawLine(Color c, Point p1, Point p2) { 
	    context.Save();

	    context.MoveTo(new PointD(p1.X + 0.5, p1.Y + 0.5));
	    context.LineTo(new PointD(p2.X + 0.5, p2.Y + 0.5));
	    context.ClosePath();
	    context.Color = new Cairo.Color(c.R, c.G, c.B);
	    context.LineWidth = 1.0;
	    context.Stroke();

	    context.Restore();
	}

	void OutlinePolygon(List<Point> points) {
	    context.MoveTo(new PointD(points[0].X + 0.5, points[0].Y + 0.5));
	    for (int i = 1; i < points.Count; ++i) {
		context.LineTo(new PointD(points[i].X + 0.5, points[i].Y + 0.5));
	    }
	    context.ClosePath();
	}

	public override void FillPolygon(Color c, List<Point> points) { 
	    context.Save();

	    OutlinePolygon(points);
	    context.Color = new Cairo.Color(c.R, c.G, c.B);
	    context.Fill();

	    context.Restore();
	}

	public override void FillStrokePolygon(Color fill, Color stroke, List<Point> points, bool dashed) {
	    context.Save();
	    
	    OutlinePolygon(points);
	    context.Color = new Cairo.Color(fill.R, fill.G, fill.B);
	    context.FillPreserve();
	    
	    if (dashed) {
		context.SetDash(new double[] { 2.0, 2.0 }, 0);
	    }
	    context.Color = new Cairo.Color(stroke.R, stroke.G, stroke.B);
	    context.LineWidth = 1.0;
	    context.Stroke();

	    context.Restore();
	}

	public override void TexturePolygon(ShapeDescriptor d, List<Point> points) { 
	    context.Save();

	    OutlinePolygon(points);
	    SurfacePattern pattern = new SurfacePattern(cache.GetSurface(d));
	    pattern.Extend = Cairo.Extend.Repeat;
	    context.Source = pattern;
	    context.Fill();

	    context.Restore();
	}
	
	public override void Dispose() {
	    ((IDisposable) context.Target).Dispose();
	    ((IDisposable) context).Dispose();
	}
    }
}