using Gdk;
using System;
using System.Collections.Generic;

namespace Weland {
    public class GdkDrawer : Drawer {
	Gdk.Window window;
	public GdkDrawer(Gdk.Window w) : base(w) { 
	    window = w;
	}

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
	    window.DrawLine(g, (int) Math.Round(p1.X), (int) Math.Round(p1.Y), (int) Math.Round(p2.X), (int) Math.Round(p2.Y));
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

	public override void FillStrokePolygon(Color fill, Color stroke, List<Point> points) {
	    Gdk.Point[] pointArray = new Gdk.Point[points.Count];
	    for (int i = 0; i < points.Count; ++i) {
		pointArray[i].X = (int) points[i].X;
		pointArray[i].Y = (int) points[i].Y;
	    }

	    Gdk.GC g = new Gdk.GC(window);
	    g.RgbFgColor = GdkColor(fill);
	    window.DrawPolygon(g, true, pointArray);
	    g.RgbFgColor = GdkColor(stroke);
	    window.DrawPolygon(g, false, pointArray);
	}

	public override void DrawGridIntersect(Color c, Point p) { }
	public override void Dispose() {
	    window = null;
	}
    }
}