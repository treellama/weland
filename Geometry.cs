using System;

namespace Weland {
    public partial class Level {
	public short TemporaryLineStartIndex = -1;
	public Point TemporaryLineEnd;

	public int Distance(Point p0, Point p1) {
	    return (int) Math.Round(Math.Sqrt(Math.Pow(p0.X - p1.X, 2) + Math.Pow(p0.Y - p1.Y, 2)));
	}

	public int Distance(Point p, Line line) {
	    Point e0 = Endpoints[line.EndpointIndexes[0]];
	    Point e1 = Endpoints[line.EndpointIndexes[1]];

	    double u = (((p.X - e0.X) * (e1.X - e0.X)) + ((p.Y - e0.Y) * (e1.Y - e0.Y))) / (Math.Pow(e1.X - e0.X, 2) + Math.Pow(e1.Y - e0.Y, 2));

	    if (u > 1.0) {
		return Distance(p, e1);
	    } else if (u < 0.0) {
		return Distance(p, e0);
	    } else {
		Point i = new Point();
		i.X = (short) Math.Round(e0.X + u * (e1.X - e0.X));
		i.Y = (short) Math.Round(e0.Y + u * (e1.Y - e0.Y));
		return Distance(p, i);
	    }
	}

	public short GetClosestPoint(Point p) {
	    int min = int.MaxValue;
	    short closest_point = -1;
	    for (short i = 0; i < Endpoints.Count; ++i) {		
		int distance = Distance(p, Endpoints[i]);
		if (distance < min) {
		    closest_point = i;
		    min = distance;
		}
	    }
	    return closest_point;
	}

	public short GetClosestLine(Point p) {
	    int min = int.MaxValue;
	    short closest_line = -1;
	    for (short i = 0; i < Lines.Count; ++i) {
		int distance = Distance(p, Lines[i]);
		if (distance < min) {
		    closest_line = i;
		    min = distance;
		}
	    }
	    return closest_line;
	}

	public short NewPoint(short X, short Y) {
	    Point p = new Point();
	    p.X = X;
	    p.Y = Y;
	    Endpoints.Add(p);
	    return (short) (Endpoints.Count - 1);
	}

	public short NewLine(short p1, short p2) {
	    Line line = new Line();
	    line.EndpointIndexes[0] = p1;
	    line.EndpointIndexes[1] = p2;
	    Lines.Add(line);
	    return (short) (Lines.Count - 1);
	}

	// delete a line (for SplitLine; points aren't cleaned up)
	void DeleteLine(short index) {
	    Lines.RemoveAt(index);
	    foreach (Polygon p in Polygons) {
		p.DeleteLine(index);
	    }
	    
	}

	// split line, return a new point index at X, Y
	public short SplitLine(short index, short X, short Y) {
	    Line line = Lines[index];
	    short e0 = line.EndpointIndexes[0];
	    short e1 = line.EndpointIndexes[1];
	    short p = NewPoint(X, Y);
	    DeleteLine(index);

	    NewLine(e0, p);
	    NewLine(p, e1);

	    return p;
	}
    }
}