using System;
using Cairo;

namespace Weland {
    public class Transform {
	public Transform() { }
	public double Scale = 1.0 / 32.0;
	public short XOffset = 0;
	public short YOffset = 0;

	public double ToScreenX(short x) { 
	    return (x - XOffset) * Scale;
	}

	public double ToScreenY(short y) {
	    return (y - YOffset) * Scale;
	}

	public short ToMapX(double X) {
	    return (short) ((double) (X / Scale) + XOffset);
	}

	public short ToMapY(double Y) {
	    return (short) ((double) (Y / Scale) + YOffset);
	}

	public PointD ToScreenPointD(Point p) {
	    return new PointD(ToScreenX(p.X) + 0.5, ToScreenY(p.Y) + 0.5);
	}
    }

    public class MapDrawingArea : Gtk.DrawingArea {

	public Transform Transform = new Transform();
	public Level Level;
	public short GridResolution = 1024;

	public MapDrawingArea() { }

	Color backgroundColor = new Color(0.25, 0.25, 0.25);
	Color pointColor = new Color(1, 0, 0);
	Color solidLineColor = new Color(0, 0, 0);
	Color transparentLineColor = new Color(0, 0.75, 0.75);
	Color polygonColor = new Color(0.75, 0.75, 0.75);
	Color gridLineColor = new Color(0.5, 0.5, 0.5);
	Color gridPointColor = new Color(0, 0.75, 0.75);

	protected override bool OnExposeEvent(Gdk.EventExpose args) {
	    Context context = Gdk.CairoHelper.Create(GdkWindow);

	    context.Color = backgroundColor;
	    context.Paint();

	    DrawGrid(context);

	    if (Level != null) {
		    
		    foreach (Polygon polygon in Level.Polygons) {
			    DrawPolygon(context, polygon);
		    }
		    
		    foreach (Line line in Level.Lines) {
			    DrawLine(context, line);
		    }
		    
		    foreach (Point point in Level.Endpoints) {
			    DrawPoint(context, point);
		    }
	    }
		    
	    ((IDisposable) context.Target).Dispose();
	    ((IDisposable) context).Dispose();

	    return true;
	}

	public void Center(short X, short Y) {
	    Transform.XOffset = (short) (X - Allocation.Width / 2 / Transform.Scale);
	    Transform.YOffset = (short) (Y - Allocation.Height / 2 / Transform.Scale);
	}

	void DrawPoint(Context context, Point point) {
	    context.MoveTo(Transform.ToScreenPointD(point));
	    context.ClosePath();
	    context.LineCap = LineCap.Round;
	    context.Color = pointColor;
	    context.LineWidth = 2.5;
	    context.Stroke();
	}

	void DrawGrid(Context context) {
	    Point p1 = new Point();
	    Point p2 = new Point();

	    for (int i = 0; i < short.MaxValue; i += GridResolution) {
		p1.X = short.MinValue;
		p1.Y = (short) i;
		p2.X = short.MaxValue;
		p2.Y = (short) i;

		context.MoveTo(Transform.ToScreenPointD(p1));
		context.LineTo(Transform.ToScreenPointD(p2));

		p1.Y = (short) -i;
		p2.Y = (short) -i;
			
		context.MoveTo(Transform.ToScreenPointD(p1));
		context.LineTo(Transform.ToScreenPointD(p2));

		p1.X = (short) i;
		p1.Y = short.MinValue;
		p2.X = (short) i;
		p2.Y = short.MaxValue;

		context.MoveTo(Transform.ToScreenPointD(p1));
		context.LineTo(Transform.ToScreenPointD(p2));

		p1.X = (short) -i;
		p2.X = (short) -i;
			
		context.MoveTo(Transform.ToScreenPointD(p1));
		context.LineTo(Transform.ToScreenPointD(p2));
	    }

	    context.Color = gridLineColor;
	    context.LineWidth = 1.0;
	    context.Stroke();

	    for (int i = 0; i < short.MaxValue; i += 1024) {
		for (int j = 0; j < short.MaxValue; j += 1024) {
		    p1.X = (short) i;
		    p1.Y = (short) j;
		    
		    context.MoveTo(Transform.ToScreenPointD(p1));
		    context.ClosePath();

		    p1.X = (short) -i;

		    context.MoveTo(Transform.ToScreenPointD(p1));
		    context.ClosePath();

		    p1.Y = (short) -j;
		    
		    context.MoveTo(Transform.ToScreenPointD(p1));
		    context.ClosePath();

		    p1.X = (short) i;
		    
		    context.MoveTo(Transform.ToScreenPointD(p1));
		    context.ClosePath();
		}
	    }

	    context.LineCap = LineCap.Round;
	    context.Color = gridPointColor;
	    context.LineWidth = 2.0;
	    context.Stroke();
	}		
	    

	void DrawLine(Context context, Line line) {
	    Point p1 = Level.Endpoints[line.EndpointIndexes[0]];
	    Point p2 = Level.Endpoints[line.EndpointIndexes[1]];
	    
	    context.MoveTo(Transform.ToScreenPointD(p1));
	    context.LineTo(Transform.ToScreenPointD(p2));
	    if (line.ClockwisePolygonOwner != -1 && line.CounterclockwisePolygonOwner != -1) {
		context.Color = transparentLineColor;
	    } else {
		context.Color = solidLineColor;
	    }

	    context.LineWidth = 1.0;
	    context.Stroke();
	}

	void DrawPolygon(Context context, Polygon polygon) {
	    Point p = Level.Endpoints[polygon.EndpointIndexes[0]];
	    context.MoveTo(Transform.ToScreenPointD(p));
	    for (int i = 1; i < polygon.VertexCount; ++i) {
		context.LineTo(Transform.ToScreenPointD(Level.Endpoints[polygon.EndpointIndexes[i]]));
	    }

	    context.Color = polygonColor;
	    context.ClosePath();
	    context.Fill();
	}
    }
}