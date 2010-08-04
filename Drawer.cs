using System;
using System.Collections.Generic;

namespace Weland {
    public abstract class Drawer : IDisposable {
	public struct Point {
	    public double X;
	    public double Y;

	    public Point(double x, double y) {
		X = x;
		Y = y;
	    }
	}

	public struct Color {
	    public double R;
	    public double G;
	    public double B;
	    
	    public Color(double r, double g, double b) {
		R = r;
		G = g;
		B = b;
	    }
	}

	public abstract void Clear(Color c);
	public abstract void DrawPoint(Color c, Point p);
	public abstract void DrawLine(Color c, Point p1, Point p2);
	public abstract void FillPolygon(Color c, List<Point> points);
	public abstract void TexturePolygon(ShapeDescriptor d, List<Point> points);
	public abstract void FillStrokePolygon(Color fill, Color stroke, List<Point> points, bool dashed);
	public abstract void DrawGridIntersect(Color c, Point p);
	public abstract void Dispose();

	public virtual ShapesFile Shapes {
	    get { return shapes; }
	    set { shapes = value; }
	}
	protected ShapesFile shapes;
    }
}