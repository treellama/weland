using Cairo;
using System;
using System.Collections.Generic;

namespace Weland {
    public class CairoDrawer : Drawer {
	Context context;
	public CairoDrawer(Gdk.Window window) : base(window) {
	    context = Gdk.CairoHelper.Create(window);
	}

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
	    context.MoveTo(new PointD(p.X + 0.5, p.Y + 1.5));
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

	public override void FillStrokePolygon(Color fill, Color stroke, List<Point> points) {
	    context.Save();

	    OutlinePolygon(points);
	    context.Color = new Cairo.Color(fill.R, fill.G, fill.B);
	    context.FillPreserve();
	    
	    context.Color = new Cairo.Color(stroke.R, stroke.G, stroke.B);
	    context.LineWidth = 1.0;
	    context.Stroke();

	    context.Restore();
	}

	public override void Dispose() {
	    ((IDisposable) context.Target).Dispose();
	    ((IDisposable) context).Dispose();
	}
    }
}