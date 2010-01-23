using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace Weland {
    public class SystemDrawer : Drawer {
	Graphics graphics;
	public SystemDrawer(Gdk.Window window, bool antialias) {
	    graphics = Gtk.DotNet.Graphics.FromDrawable(window);
	    if (antialias) {
		graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
	    }
	}

	System.Drawing.Color SystemColor(Color c) {
	    return System.Drawing.Color.FromArgb((byte) (c.R * 255), (byte) (c.G * 255), (byte) (c.B * 255));
	}

	public override void Clear(Color c) { 
	    graphics.Clear(SystemColor(c));
	}

	public override void DrawPoint(Color c, Point p) { 
	    Pen pen = new Pen(SystemColor(c));
	    graphics.DrawRectangle(pen, (float) (p.X - 0.5), (float) (p.Y - 0.5), 1, 1);
	}

	public override void DrawLine(Color c, Point p1, Point p2) { 
	    Pen pen = new Pen(SystemColor(c));
	    graphics.DrawLine(pen, (float) p1.X, (float) p1.Y, (float) p2.X, (float) p2.Y);
	}
	public override void FillPolygon(Color c, List<Point> points) { 
	    System.Drawing.PointF[] pointArray = new System.Drawing.PointF[points.Count];
	    for (int i = 0; i < points.Count; ++i) {
		pointArray[i].X = (float) points[i].X;
		pointArray[i].Y = (float) points[i].Y;
	    }	    
	    graphics.FillPolygon(new SolidBrush(SystemColor(c)), pointArray);
	}
	public override void FillStrokePolygon(Color fill, Color stroke, List<Point> points, bool dashed) { 
	    System.Drawing.PointF[] pointArray = new System.Drawing.PointF[points.Count];
	    for (int i = 0; i < points.Count; ++i) {
		pointArray[i].X = (float) points[i].X;
		pointArray[i].Y = (float) points[i].Y;
	    }	    
	    graphics.FillPolygon(new SolidBrush(SystemColor(fill)), pointArray);
	    Pen pen = new Pen(SystemColor(stroke));
	    if (dashed) {
		pen.DashPattern = new float[] { 2, 2 };
	    }
	    graphics.DrawPolygon(pen, pointArray);
	}

	public override void DrawGridIntersect(Color c, Point p) { 
	    Pen pen = new Pen(SystemColor(c));
	    graphics.DrawLine(pen, (float) (p.X - 1), (float) p.Y, (float) (p.X + 1), (float) p.Y);
	    graphics.DrawLine(pen, (float) (p.X), (float) (p.Y - 1), (float) p.X, (float) (p.Y + 1));
	}

	public override void Dispose() { 
	    graphics.Dispose();
	}
    }
}