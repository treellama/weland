using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Weland {
    public partial class Level {
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

	public double Dot(Point p0, Point p1) {
	    return p0.X * p1.X + p0.Y * p1.Y;
	}

	public double Cross(Point p0, Point p1) {
	    return p0.Y * p1.X - p0.X * p1.Y;
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

	public short GetClosestObject(Point p) {
	    int min = int.MaxValue;
	    short closest_object = -1;
	    for (short i = 0; i < Objects.Count; ++i) {
		int distance = Distance(p, new Point(Objects[i].X, Objects[i].Y));
		if (distance < min) {
		    closest_object = i;
		    min = distance;
		}
	    }

	    return closest_object;
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

	public short GetEnclosingPolygon(Point p) {
	    for (int i = 0; i < Polygons.Count; ++i) {
		List<short> points = GetPointRingFromLineRing(new List<short>(Polygons[i].LineIndexes).GetRange(0, Polygons[i].VertexCount));
		if (PointLoopEnclosesPoint(p, points)) {
		    return (short) i;
		}		
	    }
	    return -1;
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

	// split line, return a new point index at X, Y
	public short SplitLine(short index, short X, short Y) {
	    Line line = Lines[index];
	    short e0 = line.EndpointIndexes[0];
	    short e1 = line.EndpointIndexes[1];
	    short p = NewPoint(X, Y);
	    
	    DeleteLineIndex(index);

	    NewLine(e0, p);
	    NewLine(p, e1);

	    return p;
	}

	// returns all line indices due east of X, Y sorted by x intersect
	SortedDictionary<double, short> GetFillCandidateLines(short X, short Y) {
	    SortedDictionary<double, short> candidates = new SortedDictionary<double, short>();
	    for (int i = 0; i < Lines.Count; ++i) {
		Point p0 = Endpoints[Lines[i].EndpointIndexes[0]];
		Point p1 = Endpoints[Lines[i].EndpointIndexes[1]];

		if (p1.Y == p0.Y) {
		    continue;
		}

		double u = (double) (Y - p0.Y) / (p1.Y - p0.Y);

		double x_intersect = p0.X + (u * (p1.X - p0.X));
		if (u >= 0.0 && u <= 1.0 && x_intersect > X) {
		    // note that if two lines miraculously share the
		    // same X intercept, one will be lost--oh well,
		    // click somewhere else
		    candidates[x_intersect] = (short) i;
		}
	    }
	    return candidates;
	}

	// get all lines attached to endpoint
	public List<short> EndpointLines(short index) {
	    List<short> lines = new List<short>();
	    for (int i = 0; i < Lines.Count; ++i) {
		Line line = Lines[i];
		if (line.EndpointIndexes[0] == index ||
		    line.EndpointIndexes[1] == index) {
		    lines.Add((short) i);
		}
	    }
	    return lines;
	}

	// get all polygons that have this endpoint
	public List<short> EndpointPolygons(short index) {
	    List<short> polygons = new List<short>();
	    for (int i = 0; i < Polygons.Count; ++i) {
		for (int j = 0; j < Polygons[i].VertexCount; ++j) {
		    Line line = Lines[Polygons[i].LineIndexes[j]];
		    if (line.EndpointIndexes[0] == index ||
			line.EndpointIndexes[1] == index) {
			polygons.Add((short) i);
			break;
		    }
		}
	    }
	    return polygons;
	}

	Point Diff(Point p0, Point p1) {
	    return new Point((short) (p0.X - p1.X), (short) (p0.Y - p1.Y));
	}

	double Norm(Point p) {
	    return Math.Sqrt(Math.Pow(p.X, 2) + Math.Pow(p.Y, 2));
	}

	bool BuildLoop(short target, short prev, short current, int depth, short starter, List<short> list) {
	    if (current == target) {
		int other_index = Lines[list[list.Count - 1]].EndpointIndexes[0];
		if (other_index == current)
		    other_index = Lines[list[list.Count - 1]].EndpointIndexes[1];
		double cross = Cross(Diff(Endpoints[prev], Endpoints[current]), Diff(Endpoints[other_index], Endpoints[current]));
		if (cross >= 0.0) {
		    return true;
		} else {
                    return false;
		}	
	    } else if (depth > 8) {
		return false;
	    }

	    List<short> neighbors = EndpointLines(current);
	    neighbors.Remove(starter);
	    if (neighbors.Count == 0) {
		return false;
	    }

	    // C# needs a multimap :(
	    SortedDictionary<double, List<short>> dotProducts = new SortedDictionary<double, List<short>>();
	    foreach (short i in neighbors) {
		if (Lines[i].EndpointIndexes[0] == prev ||
		    Lines[i].EndpointIndexes[1] == prev) {
		    continue;
		}

		Point p0 = Endpoints[Lines[i].EndpointIndexes[0]];
		Point p1 = Endpoints[Lines[i].EndpointIndexes[1]];
		double dp;

		if (Lines[i].EndpointIndexes[0] == current) {
		    dp = Dot(Diff(Endpoints[prev], Endpoints[current]), Diff(p1, Endpoints[current])) / Norm(Diff(p1, Endpoints[current]));
		} else {
		    dp = Dot(Diff(Endpoints[prev], Endpoints[current]), Diff(p0, Endpoints[current])) / Norm(Diff(p0, Endpoints[current]));
		}
		if (!dotProducts.ContainsKey(dp)) {
		    dotProducts[dp] = new List<short>();
		}
		dotProducts[dp].Add(i);
	    }

	    short firstNeighbor = -1;
	    foreach (var kvp in dotProducts) {
		foreach (short i in kvp.Value) {
		    double cross;
		    Point p0 = Endpoints[Lines[i].EndpointIndexes[0]];
		    Point p1 = Endpoints[Lines[i].EndpointIndexes[1]];
		    
		    if (Lines[i].EndpointIndexes[0] == current) {
			cross = Cross(Diff(Endpoints[prev], Endpoints[current]), Diff(p1, Endpoints[current]));
		    } else {
			cross = Cross(Diff(Endpoints[prev], Endpoints[current]), Diff(p0, Endpoints[current]));
		    }
		    
		    if (cross >= 0.0) {
			firstNeighbor = i;
		    }
		}
	    }

	    if (firstNeighbor == -1) {
		return false;
	    }

	    short tightest;
	    if (Lines[firstNeighbor].EndpointIndexes[0] == current) {
		tightest = Lines[firstNeighbor].EndpointIndexes[1];
	    } else {
		tightest = Lines[firstNeighbor].EndpointIndexes[0];
	    }

	    list.Add(firstNeighbor);
	    return (BuildLoop(target, current, tightest, depth + 1, starter, list));

	}
	List<short> GetPointRingFromLineRing(List<short> lines) {
	    List<short> points = new List<short>();
	    // find the first point
	    if (Lines[lines[0]].EndpointIndexes[0] == Lines[lines[1]].EndpointIndexes[0] ||
		Lines[lines[0]].EndpointIndexes[0] == Lines[lines[1]].EndpointIndexes[1]) {
		points.Add(Lines[lines[0]].EndpointIndexes[0]);
	    } else {
		points.Add(Lines[lines[0]].EndpointIndexes[1]);
	    }
	    for (int i = 1; i < lines.Count; ++i) {
		if (points[points.Count - 1] == Lines[lines[i]].EndpointIndexes[0]) {
		    points.Add(Lines[lines[i]].EndpointIndexes[1]);
		} else {
		    points.Add(Lines[lines[i]].EndpointIndexes[0]);
		}
	    }
	    return points;
	}

	double Angle(int X0, int Y0, int X1, int Y1) {
	    // computes the angle from the line from the origin to p0,
	    // and the line from the origin to p1, and returns a value
	    // in the range [-pi,pi]
	    double dtheta = (Math.Atan2(Y0, X0) - Math.Atan2(Y1, X1)) % (Math.PI * 2);
	    if (dtheta > Math.PI) 
		return dtheta - Math.PI * 2;
	    else if (dtheta < -Math.PI) 
		return dtheta + Math.PI * 2;
	    else 
		return dtheta;
	}

	bool PointLoopEnclosesPoint(Point p, List<short> points) {
	    double angle_sum = 0;
	    for (int i = 0; i < points.Count; ++i) {
		Point p0 = Endpoints[points[i]];
		Point p1 = Endpoints[points[(i + 1) % points.Count]];
		double angle = Angle(p0.X - p.X, p0.Y - p.Y, p1.X - p.X, p1.Y - p.Y);
		angle_sum += angle;
	    }
	    return !(Math.Abs(angle_sum) < Math.PI);
	}

	List<short> SelectLineLoop(short X, short Y) {
	    SortedDictionary<double, short> candidates = GetFillCandidateLines(X, Y);
	    foreach (var kvp in candidates) {
		Line line = Lines[kvp.Value];
		short p0 = line.EndpointIndexes[0];
		short p1 = line.EndpointIndexes[1];
		if (Endpoints[p0].Y > Endpoints[p1].Y) {
		    p0 = line.EndpointIndexes[1];
		    p1 = line.EndpointIndexes[0];
		}

		List<short> lines = new List<short>();
		lines.Add(kvp.Value);
		if (BuildLoop(p0, p0, p1, 0, kvp.Value, lines)) {
		    // make sure the points enclose X, Y!
		    List<short> points = GetPointRingFromLineRing(lines);
		    if (PointLoopEnclosesPoint(new Point(X, Y), points)) {
			return lines;
		    }
		}
	    }
	    return new List<short>();
	}
	
	public bool FillPolygon(short X, short Y) {
	    if (GetEnclosingPolygon(new Point(X, Y)) != -1) {
		return false;
	    }

	    List<short> loop = SelectLineLoop(X, Y);
	    if (loop.Count == 0) {
		return false;
	    }

	    foreach(short i in loop) {
		if (Lines[i].ClockwisePolygonOwner != -1 &&
		    Lines[i].CounterclockwisePolygonOwner != -1) {
		    return false;
		}
	    }

	    Polygon polygon = new Polygon();
	    polygon.VertexCount = (ushort) loop.Count;
	    loop.CopyTo(polygon.LineIndexes);

	    List<short> points = GetPointRingFromLineRing(loop);
	    points.CopyTo(polygon.EndpointIndexes);

	    short index = (short) Polygons.Count;
	    Polygons.Add(polygon);
	    Polygon adjacent = null;
	    for (int i = 0; i < loop.Count; ++i) {
		Line line = Lines[loop[i]];
		if (line.EndpointIndexes[1] == polygon.EndpointIndexes[i]) {
		    line.ClockwisePolygonOwner = index;
		    if (adjacent == null && line.CounterclockwisePolygonOwner != -1) {
			adjacent = Polygons[line.CounterclockwisePolygonOwner];
		    }
		} else {
		    line.CounterclockwisePolygonOwner = index;
		    if (adjacent == null && line.ClockwisePolygonOwner != -1) {
			adjacent = Polygons[line.ClockwisePolygonOwner];
		    }
		}
		if (line.ClockwisePolygonOwner != -1 &&
		    line.CounterclockwisePolygonOwner != -1) {
		    line.Flags = LineFlags.Transparent;
		    
		    // empty the side opposite
		    if (line.ClockwisePolygonOwner == index) {
			if (line.CounterclockwisePolygonSideIndex != -1) {
			    Sides[line.CounterclockwisePolygonSideIndex].Clear();
			}
		    } else {
			if (line.ClockwisePolygonSideIndex != -1) {
			    Sides[line.ClockwisePolygonSideIndex].Clear();
			}
		    }
		}
	    }
	    if (adjacent != null) {
		// copy some settings from it
		polygon.FloorHeight = adjacent.FloorHeight;
		polygon.CeilingHeight = adjacent.CeilingHeight;
		polygon.FloorTexture = adjacent.FloorTexture;
		polygon.CeilingTexture = adjacent.CeilingTexture;
		polygon.FloorTransferMode = adjacent.FloorTransferMode;
		polygon.CeilingTransferMode = adjacent.CeilingTransferMode;
		polygon.FloorLight = adjacent.FloorLight;
		polygon.CeilingLight = adjacent.CeilingLight;
	    }
	    return true;
	}
    
	public void UpdatePolygonConcavity(Polygon polygon) {
	    List<short> lines = new List<short>();
	    for (int i = 0; i < polygon.VertexCount; ++i) {
		lines.Add(polygon.LineIndexes[i]);
	    }
	    double positive = 0;
	    List<short> points = GetPointRingFromLineRing(lines);
	    for (int i = 0; i < points.Count; ++i) {
		Point p0 = Endpoints[points[i]];
		Point p1 = Endpoints[points[(i + 1) % points.Count]];
		Point p2 = Endpoints[points[(i + 2) % points.Count]];
		double cross = Cross(Diff(p1, p0), Diff(p2, p1));
		if (positive == 0) {
		    positive = cross;
		} else if (cross != 0) {
		    if ((positive > 0) != (cross > 0)) {
			polygon.Concave = true;
			return;
		    }
		}
	    }
	    polygon.Concave = false;
	}

	public Point PolygonCenter(Polygon polygon) {
	    double X = 0;
	    double Y = 0;
	    for (int i = 0; i < polygon.VertexCount; ++i) {
		X += Endpoints[polygon.EndpointIndexes[i]].X;
		Y += Endpoints[polygon.EndpointIndexes[i]].Y;
	    }

	    return new Point((short) (X / polygon.VertexCount), (short) (Y / polygon.VertexCount));
	}

	public void DeleteObject(short index) {
	    MapObject obj = Objects[index];
	    if (obj.Type == ObjectType.Monster && MonsterPlacement[obj.Index].InitialCount > 0) {
		MonsterPlacement[obj.Index].InitialCount--;
	    } else if (obj.Type == ObjectType.Item && ItemPlacement[obj.Index].InitialCount > 0) {
		ItemPlacement[obj.Index].InitialCount--;
	    }
	    Objects.RemoveAt(index);
	}

	public void DeletePolygon(short index) {
	    Polygons.RemoveAt(index);
	    foreach (Line line in Lines) {
		if (line.ClockwisePolygonOwner > index) {
		    --line.ClockwisePolygonOwner;
		} else if (line.ClockwisePolygonOwner == index) {
		    line.ClockwisePolygonOwner = -1;
		    line.Flags = LineFlags.Solid;
		}
		
		if (line.CounterclockwisePolygonOwner > index) {
		    --line.CounterclockwisePolygonOwner;
		} else if (line.CounterclockwisePolygonOwner == index) {
		    line.CounterclockwisePolygonOwner = -1;
		    line.Flags = LineFlags.Solid;
		}
	    }

	    for (int i = Sides.Count - 1; i >= 0; --i) {
		if (Sides[i].PolygonIndex > index) {
		    --Sides[i].PolygonIndex;
		} else if (Sides[i].PolygonIndex == index) {
		    Sides.RemoveAt(i);
		}
	    }

	    for (int i = Objects.Count - 1; i >= 0; --i) {
		if (Objects[i].PolygonIndex > index) {
		    --Objects[i].PolygonIndex;
		} else if (Objects[i].PolygonIndex == index) {
		    DeleteObject((short) i);
		}
	    }

	    for (int i = Platforms.Count - 1; i >= 0; --i) {
		if (Platforms[i].PolygonIndex > index) {
		    --Platforms[i].PolygonIndex;
		} else if (Platforms[i].PolygonIndex == index) {
		    Platforms.RemoveAt(i);
		}
	    }
	}


	public void DeleteLineIndex(short index) {
	    Lines.RemoveAt(index);
	    foreach (Polygon poly in Polygons) {
		poly.DeleteLine(index);
	    }
	    foreach (Side side in Sides) {
		side.DeleteLine(index);
	    }
	}

	public void DeletePointIndex(short index) {
	    Endpoints.RemoveAt(index);
	    foreach (Line line in Lines) {
		if (line.EndpointIndexes[0] > index) {
		    --line.EndpointIndexes[0];
		}

		if (line.EndpointIndexes[1] > index) {
		    --line.EndpointIndexes[1];
		}
	    }

	    foreach (Polygon poly in Polygons) {
		for (int i = 0; i < poly.VertexCount; ++i) {
		    if (poly.EndpointIndexes[i] > index) {
			--poly.EndpointIndexes[i];
		    }
		}
	    }
	}

	public void DeleteLine(short index) {
	    Line line = Lines[index];
	    if (line.ClockwisePolygonOwner != -1) {
		DeletePolygon(line.ClockwisePolygonOwner);
	    }
	    if (line.CounterclockwisePolygonOwner != -1) {
		DeletePolygon(line.CounterclockwisePolygonOwner);
	    }

	    DeleteLineIndex(index);
	    
	    bool e0_has_other_lines = false;
	    bool e1_has_other_lines = false;
	    foreach (Line other_line in Lines) {
		if (other_line.EndpointIndexes[0] == line.EndpointIndexes[0] ||
		    other_line.EndpointIndexes[1] == line.EndpointIndexes[0]) {
		    e0_has_other_lines = true;
		    if (e1_has_other_lines) break;
		}
		if (other_line.EndpointIndexes[0] == line.EndpointIndexes[1] ||
		    other_line.EndpointIndexes[1] == line.EndpointIndexes[1]) {
		    e1_has_other_lines = true;
		    if (e0_has_other_lines) break;
		}
	    }

	    if (!e0_has_other_lines) {
		DeletePointIndex(line.EndpointIndexes[0]);
		if (line.EndpointIndexes[1] > line.EndpointIndexes[0]) {
		    --line.EndpointIndexes[1];
		}
	    }

	    if (!e1_has_other_lines) {
		DeletePointIndex(line.EndpointIndexes[1]);
	    }
	}

	public void DeletePoint(short index) {
	    List<Line> lines = new List<Line>();
	    foreach (Line line in Lines) {
		if (line.EndpointIndexes[0] == index ||
		    line.EndpointIndexes[1] == index) {
		    lines.Add(line);
		}
	    }

	    // this is pretty sleazy, I hate relying on ref comparison
	    if (lines.Count > 0) {
		foreach (Line line in lines) {
		    DeleteLine((short) Lines.IndexOf(line));
		}
	    } else {
		DeletePointIndex(index);
	    }
	}

	void FixSideType(Side side) {
	    Polygon adjacent = Polygons[side.PolygonIndex];
	    Line line = Lines[side.LineIndex];
	    Polygon opposite = null;
	    if (line.CounterclockwisePolygonOwner == side.PolygonIndex &&
		line.ClockwisePolygonOwner != -1) {
		opposite = Polygons[line.ClockwisePolygonOwner];
	    } else if (line.ClockwisePolygonOwner == side.PolygonIndex &&
		       line.CounterclockwisePolygonOwner != -1) {
		opposite = Polygons[line.CounterclockwisePolygonOwner];
	    }
	    if (opposite == null) {
		side.Type = SideType.Full;
	    } else {
		short ceiling_height = adjacent.CeilingHeight;
		short floor_height = adjacent.FloorHeight;
		short opposite_ceiling_height = opposite.CeilingHeight;
		short opposite_floor_height = opposite.FloorHeight;

		// we should handle platforms more intelligently than this
		if ((opposite_ceiling_height < ceiling_height && opposite_floor_height > floor_height) || adjacent.Type == 5 || opposite.Type == 5) {
		    side.Type = SideType.Split;
		} else if (opposite_floor_height > floor_height) {
		    side.Type = SideType.Low;
		} else if (opposite_ceiling_height < ceiling_height) {
		    side.Type = SideType.High;
		} else {
		    side.Type = SideType.Full;
		}
	    }
	}

	public void DeleteSide(short side_index) {
	    Sides.RemoveAt(side_index);
	    foreach (Line line in Lines) {
		if (line.CounterclockwisePolygonSideIndex > side_index) {
		    --line.CounterclockwisePolygonSideIndex;
		} else if (line.CounterclockwisePolygonSideIndex == side_index) {
		    line.CounterclockwisePolygonSideIndex = -1;
		}
		if (line.ClockwisePolygonSideIndex > side_index) {
		    --line.ClockwisePolygonSideIndex;
		} else if (line.ClockwisePolygonSideIndex == side_index) {
		    line.ClockwisePolygonSideIndex = -1;
		}
	    }
	}

	public short NewSide(short polygon_index, short line_index) {
	    short side_index = (short) Sides.Count;
	    
	    Side side = new Side();
	    Line line = Lines[line_index];

	    side.LineIndex = line_index;
	    side.PolygonIndex = polygon_index;
	    Sides.Add(side);
	    FixSideType(side);

	    if (line.ClockwisePolygonOwner == polygon_index) {
		line.ClockwisePolygonSideIndex = side_index;
	    } else if (line.CounterclockwisePolygonOwner == polygon_index) {
		line.CounterclockwisePolygonSideIndex = side_index;
	    } else {
		Debug.Assert(false);
	    }

	    return side_index;
	}

	void PaveSide(Side side, byte collection) {
	    const int wall = 5;
	    FixSideType(side);
	    if (side.Primary.Texture.IsEmpty()) {
		side.Primary.Texture.Collection = collection;
		side.Primary.Texture.Bitmap = wall;
		side.PrimaryTransferMode = 0;
	    }
	    if (side.Type == SideType.Split && side.Secondary.Texture.IsEmpty()) {
		side.Secondary.Texture.Collection = collection;
		side.Secondary.Texture.Bitmap = wall;
		side.SecondaryTransferMode = 0;
	    }

	}

	public void NukeTextures() {
	    while (Sides.Count > 0) {
		DeleteSide(0);
	    }
	    foreach (Polygon polygon in Polygons) {
		polygon.FloorTexture = ShapeDescriptor.Empty;
		polygon.CeilingTexture = ShapeDescriptor.Empty;
	    }
	}
	
	public void Pave() {
	    byte collection = (byte) (Environment + 17);
	    const int floor = 6;
	    const int ceiling = 7;

	    for (int i = 0; i < Lines.Count; ++i) {
		Line line = Lines[i];
		Polygon cw_polygon = null;
		Polygon ccw_polygon = null;
		if (line.ClockwisePolygonOwner != -1) {
		    cw_polygon = Polygons[line.ClockwisePolygonOwner];
		}
		if (line.CounterclockwisePolygonOwner != -1) {
		    ccw_polygon = Polygons[line.CounterclockwisePolygonOwner];
		}

		Side cw_side = null;
		if (line.ClockwisePolygonSideIndex != -1) {
		    cw_side = Sides[line.ClockwisePolygonSideIndex];
		}
		
		Side ccw_side = null;
		if (line.CounterclockwisePolygonSideIndex != -1) {
		    ccw_side = Sides[line.CounterclockwisePolygonSideIndex];
		}

		// we should be a little more smarter about generating
		// sides against platforms
		if (cw_side == null && cw_polygon != null && (ccw_polygon == null || ccw_polygon.Type == 5 || cw_polygon.FloorHeight < ccw_polygon.FloorHeight || cw_polygon.CeilingHeight > ccw_polygon.CeilingHeight)) {
		    cw_side = Sides[NewSide(line.ClockwisePolygonOwner, (short) i)];
		}

		if (ccw_side == null && ccw_polygon != null && (cw_polygon == null || cw_polygon.Type == 5 || ccw_polygon.FloorHeight < cw_polygon.FloorHeight || ccw_polygon.CeilingHeight > cw_polygon.CeilingHeight)) {
		    ccw_side = Sides[NewSide(line.CounterclockwisePolygonOwner, (short) i)];
		}

		if (cw_side != null) {
		    PaveSide(cw_side, collection);
		}

		if (ccw_side != null) {
		    PaveSide(ccw_side, collection);
		}
	    }
	    
	    foreach (Polygon polygon in Polygons) {
		if (polygon.FloorTexture.IsEmpty()) {
		    polygon.FloorTexture.Collection = collection;
		    polygon.FloorTexture.Bitmap = floor;
		    polygon.FloorTransferMode = 0;
		}

		if (polygon.CeilingTexture.IsEmpty()) {
		    polygon.CeilingTexture.Collection = collection;
		    polygon.CeilingTexture.Bitmap = ceiling;
		    polygon.CeilingTransferMode = 0;
		}
	    }
	}
    }
}
