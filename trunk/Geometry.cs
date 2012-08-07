using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Weland {
    public delegate bool PolygonFilter(Polygon p);
    public delegate bool ObjectFilter(MapObject o);

    public partial class Level {
	public static PolygonFilter Filter = p => true;
	public static ObjectFilter ObjectFilter = o => true;
	public static bool FilterPoints = false;
	public static bool RememberDeletedSides = false;

	Dictionary<short, Side> ClockwiseOrphanedSides = new Dictionary<short, Side>();
	Dictionary<short, Side> CounterClockwiseOrphanedSides = new Dictionary<short, Side>();
	HashSet<short> LinesReflectingOrphanedSides = new HashSet<short>();

	public bool FilterLine(PolygonFilter f, Line line) {
	    if (line.ClockwisePolygonOwner == -1) {
		if (line.CounterclockwisePolygonOwner == -1) {
		    return true;
		} else {
		    return f(Polygons[line.CounterclockwisePolygonOwner]);
		}
	    } else {
		if (line.CounterclockwisePolygonOwner == -1) {
		    return f(Polygons[line.ClockwisePolygonOwner]);
		} else {
		    return f(Polygons[line.ClockwisePolygonOwner]) || f(Polygons[line.CounterclockwisePolygonOwner]);
		}
	    }
	}

	public bool FilterPoint(PolygonFilter f, short index) {
	    if (!FilterPoints) return true;
	    HashSet<Polygon> polygonSet = EndpointPolygons[index];
	    foreach (Polygon polygon in polygonSet) {
		if (f(polygon)) {
		    return true;
		}
	    }

	    HashSet<Line> lineSet = EndpointLines[index];
	    foreach (Line line in lineSet) {
		if (FilterLine(f, line)) {
		    return true;
		}
	    }
	    return false;
	}

	public int Distance(Point p0, Point p1) {
	    return (int) Math.Round(Math.Sqrt(Math.Pow(p0.X - p1.X, 2) + Math.Pow(p0.Y - p1.Y, 2)));
	}

	double U(Point p, Line line) {
	    Point e0 = Endpoints[line.EndpointIndexes[0]];
	    Point e1 = Endpoints[line.EndpointIndexes[1]];

	    return  ((((double) p.X - e0.X) * ((double) e1.X - e0.X)) + (((double) p.Y - e0.Y) * ((double) e1.Y - e0.Y))) / (Math.Pow(e1.X - e0.X, 2) + Math.Pow(e1.Y - e0.Y, 2));
	}

	public Point ClosestPointOnLine(Point p, Line line) {
	    Point e0 = Endpoints[line.EndpointIndexes[0]];
	    Point e1 = Endpoints[line.EndpointIndexes[1]];

	    double u = U(p, line);
	    Point point;
	    point.X = (short) Math.Round(e0.X + u * (e1.X - e0.X));
	    point.Y = (short) Math.Round(e0.Y + u * (e1.Y - e0.Y));
	    return point;
	}

	public int Distance(Point p, Line line) {
	    Point e0 = Endpoints[line.EndpointIndexes[0]];
	    Point e1 = Endpoints[line.EndpointIndexes[1]];

	    double u = U(p, line);

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

	public short GetClosestAnnotation(Point p) {
	    int min = int.MaxValue;
	    short closest_annotation = -1;
	    for (short i = 0; i < Annotations.Count; ++i) {
		Annotation a = Annotations[i];
		int distance = Distance(p, new Point(a.X, a.Y));
		if (distance < min) {
		    closest_annotation = i;
		    min = distance;
		}
	    }
	    return closest_annotation;
	}

	public short GetClosestPoint(Point p) {
	    int min = int.MaxValue;
	    short closest_point = -1;
	    for (short i = 0; i < Endpoints.Count; ++i) {	
		if (FilterPoint(Filter, i)) {
		    int distance = Distance(p, Endpoints[i]);
		    if (distance < min) {
			closest_point = i;
			min = distance;
		    }
		}
	    }
	    return closest_point;
	}

	public short GetClosestObject(Point p) {
	    int min = int.MaxValue;
	    short closest_object = -1;
	    for (short i = 0; i < Objects.Count; ++i) {
		if (Filter(Polygons[Objects[i].PolygonIndex]) && ObjectFilter(Objects[i])) {
		    int distance = Distance(p, new Point(Objects[i].X, Objects[i].Y));
		    if (distance < min) {
			closest_object = i;
			min = distance;
		    }
		}
	    }

	    return closest_object;
	}

	public short GetClosestLine(Point p) {
	    int min = int.MaxValue;
	    short closest_line = -1;
	    for (short i = 0; i < Lines.Count; ++i) {
		if (FilterLine(Filter, Lines[i])) {
		    int distance = Distance(p, Lines[i]);
		    if (distance < min) {
			closest_line = i;
			min = distance;
		    }
		}
	    }
	    return closest_line;
	}

	public short GetEnclosingPolygon(Point p) {
	    for (int i = 0; i < Polygons.Count; ++i) {
		if (Filter(Polygons[i])) {
		    List<short> points = GetPointRingFromLineRing(new List<short>(Polygons[i].LineIndexes).GetRange(0, Polygons[i].VertexCount));
		    if (PointLoopEnclosesPoint(p, points)) {
			return (short) i;
		    }
		}		
	    }
	    return -1;
	}

	public short NewPoint(short X, short Y) {
	    Point p = new Point();
	    p.X = X;
	    p.Y = Y;
	    Endpoints.Add(p);
	    EndpointPolygons.Add(new HashSet<Polygon>());
	    EndpointLines.Add(new HashSet<Line>());
	    return (short) (Endpoints.Count - 1);
	}

	public short NewLine(short p1, short p2) {
	    Line line = new Line();
	    line.EndpointIndexes[0] = p1;
	    line.EndpointIndexes[1] = p2;
	    EndpointLines[p1].Add(line);
	    EndpointLines[p2].Add(line);
	    Lines.Add(line);
	    return (short) (Lines.Count - 1);
	}

	public short NewObject(Point p, short polygon_index) {
	    MapObject o = new MapObject();
	    o.X = p.X;
	    o.Y = p.Y;
	    o.PolygonIndex = polygon_index;
	    Objects.Add(o);
	    return (short) (Objects.Count - 1);
	}

	// split line, return a new point index at X, Y
	public short SplitLine(short index, short X, short Y) {
	    Line line = Lines[index];
	    short e0 = line.EndpointIndexes[0];
	    short e1 = line.EndpointIndexes[1];
	    short p = NewPoint(X, Y);

	    short l1 = NewLine(e0, p);
	    short l2 = NewLine(p, e1);

	    if (line.ClockwisePolygonOwner != -1) {
		Polygon polygon = Polygons[line.ClockwisePolygonOwner];

		for (int i = 0; i < Polygon.MaxVertexCount; ++i) {
		    if (polygon.LineIndexes[i] == index) {
			for (int j = Polygon.MaxVertexCount - 2; j >= i; --j) {
			    polygon.LineIndexes[j + 1] = polygon.LineIndexes[j];
			    polygon.AdjacentPolygonIndexes[j + 1] = polygon.AdjacentPolygonIndexes[j];
			    polygon.EndpointIndexes[j + 1] = polygon.EndpointIndexes[j];
			}


			polygon.LineIndexes[i] = l1;
			polygon.LineIndexes[i + 1] = l2;

			if (polygon.EndpointIndexes[i] == e0) {
			    polygon.EndpointIndexes[i + 1] = p;
			} else {
			    polygon.EndpointIndexes[i] = p;
			}
			EndpointPolygons[p].Add(polygon);

			polygon.VertexCount++;
			UpdatePolygonConcavity(polygon);

			if (line.ClockwisePolygonSideIndex != -1) {
			    // clone it
			    Side side = Sides[line.ClockwisePolygonSideIndex];
			    MemoryStream stream = new MemoryStream();
			    BinaryWriterBE writer = new BinaryWriterBE(stream);
			    side.Save(writer);
			    
			    stream.Seek(0, SeekOrigin.Begin);
			    Side s1 = new Side();
			    BinaryReaderBE reader = new BinaryReaderBE(stream);
			    s1.Load(reader);
			    s1.LineIndex = l1;
			    Lines[l1].ClockwisePolygonSideIndex = (short) Sides.Count;
			    Sides.Add(s1);
			    
			    stream.Seek(0, SeekOrigin.Begin);
			    Side s2 = new Side();
			    s2.Load(reader);
			    s2.LineIndex = l2;
			    short l1_length = (short) Distance(Endpoints[e0], Endpoints[p]);
			    if (!s2.Primary.Texture.IsEmpty()) {
				s2.Primary.X += l1_length;
			    }

			    if (!s2.Secondary.Texture.IsEmpty()) {
				s2.Secondary.X += l1_length;
			    }

			    if (!s2.Transparent.Texture.IsEmpty()) {
				s2.Transparent.X += l1_length;
			    }

			    Lines[l2].ClockwisePolygonSideIndex = (short) Sides.Count;
			    Sides.Add(s2);
			}
			break;
		    }
		}

		Lines[l1].ClockwisePolygonOwner = line.ClockwisePolygonOwner;
		Lines[l2].ClockwisePolygonOwner = line.ClockwisePolygonOwner;
	    }

	    if (line.CounterclockwisePolygonOwner != -1) {
		Polygon polygon = Polygons[line.CounterclockwisePolygonOwner];
		for (int i = 0; i < Polygon.MaxVertexCount; ++i) {
		    if (polygon.LineIndexes[i] == index) {
			for (int j = Polygon.MaxVertexCount - 2; j >= i; --j) {
			    polygon.LineIndexes[j + 1] = polygon.LineIndexes[j];
			    polygon.AdjacentPolygonIndexes[j + 1] = polygon.AdjacentPolygonIndexes[j];
			    polygon.EndpointIndexes[j + 1] = polygon.EndpointIndexes[j];
			}


			polygon.LineIndexes[i] = l2;
			polygon.LineIndexes[i + 1] = l1;
			if (polygon.EndpointIndexes[i] == e0) {
			    polygon.EndpointIndexes[i] = p;
			} else {
			    polygon.EndpointIndexes[i + 1] = p;
			}

			polygon.VertexCount++;
			UpdatePolygonConcavity(polygon);

			if (line.CounterclockwisePolygonSideIndex != -1) {
			    // clone it
			    Side side = Sides[line.CounterclockwisePolygonSideIndex];
			    MemoryStream stream = new MemoryStream();
			    BinaryWriterBE writer = new BinaryWriterBE(stream);
			    side.Save(writer);
			    
			    stream.Seek(0, SeekOrigin.Begin);
			    Side s1 = new Side();
			    BinaryReaderBE reader = new BinaryReaderBE(stream);
			    s1.Load(reader);
			    s1.LineIndex = l1;
			    Lines[l1].CounterclockwisePolygonSideIndex = (short) Sides.Count;
			    Sides.Add(s1);
			    
			    stream.Seek(0, SeekOrigin.Begin);
			    Side s2 = new Side();
			    s2.Load(reader);
			    s2.LineIndex = l2;
			    short l1_length = (short) Distance(Endpoints[e0], Endpoints[p]);
			    if (!s2.Primary.Texture.IsEmpty()) {
				s2.Primary.X += l1_length;
			    }

			    if (!s2.Secondary.Texture.IsEmpty()) {
				s2.Secondary.X += l1_length;
			    }

			    if (!s2.Transparent.Texture.IsEmpty()) {
				s2.Transparent.X += l1_length;
			    }

			    Lines[l2].CounterclockwisePolygonSideIndex = (short) Sides.Count;
			    Sides.Add(s2);
			}
			break;
		    }
		}

		Lines[l1].CounterclockwisePolygonOwner = line.CounterclockwisePolygonOwner;
		Lines[l2].CounterclockwisePolygonOwner = line.CounterclockwisePolygonOwner;

		if (Lines[l1].ClockwisePolygonOwner != -1) {
		    Lines[l1].Transparent = true;
		}

		if (Lines[l2].ClockwisePolygonOwner != -1) {
		    Lines[l2].Transparent = true;
		}
	    }

	    if (line.ClockwisePolygonSideIndex != -1) {
		DeleteSide(line.ClockwisePolygonSideIndex);
	    }

	    if (line.CounterclockwisePolygonSideIndex != -1) {
		DeleteSide(line.CounterclockwisePolygonSideIndex);
	    }

	    DeleteLineIndex(index);

	    return p;
	}

	// returns all line indices due east of X, Y sorted by x intersect
	SortedDictionary<double, short> GetFillCandidateLines(short X, short Y) {
	    SortedDictionary<double, short> candidates = new SortedDictionary<double, short>();
	    for (int i = 0; i < Lines.Count; ++i) {
		if (FilterLine(Filter, Lines[i])) {
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
	    }
	    return candidates;
	}
	
	// get all lines attached to endpoint
	public List<short> FindEndpointLines(short index) {
	    List<short> lines = new List<short>();
	    for (int i = 0; i < Lines.Count; ++i) {
		Line line = Lines[i];
		if (FilterLine(Filter, line) && (line.EndpointIndexes[0] == index ||
					 line.EndpointIndexes[1] == index)) {
		    lines.Add((short) i);
		}
	    }
	    return lines;
	}

	// get all polygons that have this endpoint
	public List<short> FindEndpointPolygons(short index) {
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
	    if (depth >= 8) {
		return false;
	    } else if (current == target) {
		int other_index = Lines[list[list.Count - 1]].EndpointIndexes[0];
		if (other_index == current)
		    other_index = Lines[list[list.Count - 1]].EndpointIndexes[1];
		double cross = Cross(Diff(Endpoints[prev], Endpoints[current]), Diff(Endpoints[other_index], Endpoints[current]));
		if (cross >= 0.0) {
		    return true;
		} else {
                    return false;
		}	
	    }

	    List<short> neighbors = FindEndpointLines(current);
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
	    foreach (short i in points) {
		EndpointPolygons[i].Add(polygon);
	    }

	    short index = (short) Polygons.Count;
	    Polygons.Add(polygon);
	    Polygon adjacent = null;
	    for (int i = 0; i < loop.Count; ++i) {
		Line line = Lines[loop[i]];
		if (line.EndpointIndexes[1] == polygon.EndpointIndexes[i]) {
		    line.ClockwisePolygonOwner = index;
		    if (line.CounterclockwisePolygonOwner != -1 && adjacent == null) {
			    adjacent = Polygons[line.CounterclockwisePolygonOwner];
		    }
		    
		    if (RememberDeletedSides && ClockwiseOrphanedSides.ContainsKey(loop[i])) {
			Side side = ClockwiseOrphanedSides[loop[i]];
			line.ClockwisePolygonSideIndex = (short) Sides.Count;
			side.LineIndex = loop[i];
			side.PolygonIndex = index;
			Sides.Add(side);
			FixSideType(side);
			ClockwiseOrphanedSides.Remove(loop[i]);
		    }
		} else {
		    line.CounterclockwisePolygonOwner = index;
		    if (line.ClockwisePolygonOwner != -1 && adjacent == null) {
			adjacent = Polygons[line.ClockwisePolygonOwner];
		    }

		    if (RememberDeletedSides && CounterClockwiseOrphanedSides.ContainsKey(loop[i])) {
			Side side = CounterClockwiseOrphanedSides[loop[i]];
			line.CounterclockwisePolygonSideIndex = (short) Sides.Count;
			side.LineIndex = loop[i];
			side.PolygonIndex = index;
			Sides.Add(side);
			FixSideType(side);
			CounterClockwiseOrphanedSides.Remove(loop[i]);
		    }
		}

		if (line.ClockwisePolygonOwner != -1 &&
		    line.CounterclockwisePolygonOwner != -1) {
		    line.Flags = LineFlags.Transparent;
		    
		    // empty the side opposite
		    if (line.ClockwisePolygonOwner == index) {
			if (line.CounterclockwisePolygonSideIndex != -1) {
			    if (RememberDeletedSides && LinesReflectingOrphanedSides.Contains(loop[i])) {
				LinesReflectingOrphanedSides.Remove(loop[i]);
			    } else {
				Sides[line.CounterclockwisePolygonSideIndex].Clear();
			    }
			}
		    } else {
			if (line.ClockwisePolygonSideIndex != -1) {
			    if (RememberDeletedSides && LinesReflectingOrphanedSides.Contains(loop[i])) {
				LinesReflectingOrphanedSides.Remove(loop[i]);
			    } else {
				Sides[line.ClockwisePolygonSideIndex].Clear();
			    }
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
	    UpdatePolygonConcavity(polygon);
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
	    Polygon polygonRef = Polygons[index];
	    
	    short platformIndex = -1;
	    if (Polygons[index].Type == PolygonType.Platform) {
		platformIndex = Polygons[index].Permutation;
	    }
	    Polygons.RemoveAt(index);

	    for (short line_index = 0; line_index < Lines.Count; ++line_index) {
		Line line = Lines[line_index];
		if (line.ClockwisePolygonOwner > index) {
		    --line.ClockwisePolygonOwner;
		} else if (line.ClockwisePolygonOwner == index) {
		    line.ClockwisePolygonOwner = -1;
		    if (RememberDeletedSides) {
			if (LinesReflectingOrphanedSides.Contains(line_index)) {
			    LinesReflectingOrphanedSides.Remove(line_index);
			} else {
			    LinesReflectingOrphanedSides.Add(line_index);
			}
		    }
		    line.Flags = LineFlags.Solid;
		}
		
		if (line.CounterclockwisePolygonOwner > index) {
		    --line.CounterclockwisePolygonOwner;
		} else if (line.CounterclockwisePolygonOwner == index) {
		    line.CounterclockwisePolygonOwner = -1;
		    if (RememberDeletedSides) {
			if (LinesReflectingOrphanedSides.Contains(line_index)) {
			    LinesReflectingOrphanedSides.Remove(line_index);
			} else {
			    LinesReflectingOrphanedSides.Add(line_index);
			}
		    }
		    line.Flags = LineFlags.Solid;
		}

		EndpointPolygons[line.EndpointIndexes[0]].Remove(polygonRef);
		EndpointPolygons[line.EndpointIndexes[1]].Remove(polygonRef);
	    }

	    for (int i = Sides.Count - 1; i >= 0; --i) {
		if (Sides[i].PolygonIndex > index) {
		    --Sides[i].PolygonIndex;
		} else if (Sides[i].PolygonIndex == index) {
		    DeleteSide((short) i);
		}
	    }

	    foreach (Side side in Sides) {
		if (side.IsControlPanel && side.IsPlatformSwitch()) {
		    if (side.ControlPanelPermutation > index) {
			--side.ControlPanelPermutation;
		    }
		}
	    }
	    
	    for (int i = Objects.Count - 1; i >= 0; --i) {
		if (Objects[i].PolygonIndex > index) {
		    --Objects[i].PolygonIndex;
		} else if (Objects[i].PolygonIndex == index) {
		    DeleteObject((short) i);
		}
	    }

	    if (platformIndex != -1) {
		Platforms.RemoveAt(platformIndex);
		foreach (Polygon polygon in Polygons) {
		    if (polygon.Type == PolygonType.Platform) {
			if (polygon.Permutation > platformIndex) {
			    --polygon.Permutation;
			}
		    }
		}
	    }

	    for (int i = Platforms.Count - 1; i >= 0; --i) {
		if (Platforms[i].PolygonIndex > index) {
		    --Platforms[i].PolygonIndex;
		}
	    }

	    for (int i = Annotations.Count - 1; i >= 0; --i) {
		if (Annotations[i].PolygonIndex > index) {
		    --Annotations[i].PolygonIndex;
		} else if (Annotations[i].PolygonIndex == index) {
		    Annotations.RemoveAt(i);
		}
	    }

	    foreach (Polygon polygon in Polygons) {
		if (polygon.Type == PolygonType.Teleporter) {
		    if (polygon.Permutation > index) {
			--polygon.Permutation;
		    } else if (polygon.Permutation == index) {
			polygon.Permutation = 0;
		    }
		} else if (polygon.Type == PolygonType.PlatformOnTrigger ||
			   polygon.Type == PolygonType.PlatformOffTrigger) {
		    if (polygon.Permutation > index) {
			--polygon.Permutation;
		    } 
		}
	    }
	}


	public void DeleteLineIndex(short index) {
	    Line line = Lines[index];
	    EndpointLines[line.EndpointIndexes[0]].Remove(line);
	    EndpointLines[line.EndpointIndexes[1]].Remove(line);
	    if (RememberDeletedSides) {
		ClockwiseOrphanedSides.Remove(index);
		CounterClockwiseOrphanedSides.Remove(index);
		LinesReflectingOrphanedSides.Remove(index);
	    }
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
	    EndpointPolygons.RemoveAt(index);
	    EndpointLines.RemoveAt(index);
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

		if (adjacent.Type == PolygonType.Platform) {
		    // calculate side based on maxima
		    Platform adjacentPlatform = Platforms[adjacent.Permutation];
		    if (adjacentPlatform.ComesFromCeiling && adjacentPlatform.ComesFromFloor) {
			if (adjacentPlatform.MinimumHeight == -1) {
			    floor_height = AutocalPlatformMinimum(adjacent.Permutation);
			} else {
			    floor_height = adjacentPlatform.MinimumHeight;
			}

			if (adjacentPlatform.MaximumHeight == -1) {
			    ceiling_height = AutocalPlatformMaximum(adjacent.Permutation);
			} else {
			    ceiling_height = adjacentPlatform.MaximumHeight;
			}
		    } else if (!adjacentPlatform.UsesNativePolygonHeights) {
			if (adjacentPlatform.ComesFromFloor) {
			    if (adjacentPlatform.MinimumHeight == -1) {
				floor_height = AutocalPlatformMinimum(adjacent.Permutation);
			    } else {
				floor_height = adjacentPlatform.MinimumHeight;
			    }
			} else if (adjacentPlatform.ComesFromCeiling) {
			    if (adjacentPlatform.MaximumHeight == -1) {
				ceiling_height = AutocalPlatformMaximum(adjacent.Permutation);
			    } else {
				ceiling_height = adjacentPlatform.MaximumHeight;
			    }
			}
		    }
		}

		if (opposite.Type == PolygonType.Platform) {
		    // calculate side based on minima
		    Platform oppositePlatform = Platforms[opposite.Permutation];
		    if (oppositePlatform.ComesFromCeiling && oppositePlatform.ComesFromFloor) {
			int floor = oppositePlatform.MinimumHeight;
			int ceiling = oppositePlatform.MaximumHeight;
			if (oppositePlatform.MinimumHeight == -1) {
			    floor = AutocalPlatformMinimum(opposite.Permutation);
			}
			if (oppositePlatform.MaximumHeight == -1) {
			    ceiling = AutocalPlatformMaximum(opposite.Permutation);
			}
			opposite_floor_height = opposite_ceiling_height = (short) (floor + (ceiling - floor) / 2);
		    } else if (!oppositePlatform.UsesNativePolygonHeights) {
			if (oppositePlatform.ComesFromFloor) {
			    if (oppositePlatform.MaximumHeight == -1) {
				opposite_floor_height = AutocalPlatformMaximum(opposite.Permutation);
			    } else {
				opposite_floor_height = oppositePlatform.MaximumHeight;
			    }
			} else if (oppositePlatform.ComesFromCeiling) {
			    if (oppositePlatform.MinimumHeight == -1) {
				opposite_ceiling_height = AutocalPlatformMinimum(opposite.Permutation);
			    } else {
				opposite_ceiling_height = oppositePlatform.MinimumHeight;
			    }
			}
		    }
		}

		bool platformSafety = (adjacent.Type == PolygonType.Platform || opposite.Type == PolygonType.Platform) && side.Type != SideType.Full;

		// we should handle platforms more intelligently than this
		if (opposite_ceiling_height < ceiling_height && opposite_floor_height > floor_height) {
		    side.Type = SideType.Split;
		} else if (opposite_floor_height > floor_height) {
		    if (!platformSafety) {
			side.Type = SideType.Low;
		    }
		} else if (opposite_ceiling_height < ceiling_height) {
		    if (!platformSafety) {
			side.Type = SideType.High;
		    }
		}
	    }
	}

	public void DeleteSide(short side_index) {
	    Side side = Sides[side_index];
	    Sides.RemoveAt(side_index);
	    for (short line_index = 0; line_index < Lines.Count; ++line_index) {
		Line line = Lines[line_index];
		if (line.CounterclockwisePolygonSideIndex > side_index) {
		    --line.CounterclockwisePolygonSideIndex;
		} else if (line.CounterclockwisePolygonSideIndex == side_index) {
		    if (RememberDeletedSides) {
			CounterClockwiseOrphanedSides[line_index] = side;
		    }
		    line.CounterclockwisePolygonSideIndex = -1;
		}
		if (line.ClockwisePolygonSideIndex > side_index) {
		    --line.ClockwisePolygonSideIndex;
		} else if (line.ClockwisePolygonSideIndex == side_index) {
		    if (RememberDeletedSides) {
			ClockwiseOrphanedSides[line_index] = side;
		    }
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
		if (RememberDeletedSides) {
		    ClockwiseOrphanedSides.Remove(line_index);
		}
		line.ClockwisePolygonSideIndex = side_index;
	    } else if (line.CounterclockwisePolygonOwner == polygon_index) {
		if (RememberDeletedSides) {
		    CounterClockwiseOrphanedSides.Remove(line_index);
		}
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
		if (side.Type == SideType.Full) {
		    // only pave if there's no polygon opposite
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
		    if (opposite == null || opposite.FloorHeight > adjacent.CeilingHeight || opposite.CeilingHeight < adjacent.FloorHeight) {
			side.Primary.Texture.Collection = collection;
			side.Primary.Texture.Bitmap = wall;
			side.PrimaryTransferMode = 0;
			side.PrimaryLightsourceIndex = 0;
		    }
		} else {
		    side.Primary.Texture.Collection = collection;
		    side.Primary.Texture.Bitmap = wall;
		    side.PrimaryTransferMode = 0;
		    side.PrimaryLightsourceIndex = 0;
		}
	    }
	    if (side.Type == SideType.Split && side.Secondary.Texture.IsEmpty()) {
		side.Secondary.Texture.Collection = collection;
		side.Secondary.Texture.Bitmap = wall;
		side.SecondaryTransferMode = 0;
		side.SecondaryLightsourceIndex = 0;
	    }

	    if (side.IsControlPanel && side.IsPlatformSwitch()) {
		if (side.ControlPanelPermutation < 0 || side.ControlPanelPermutation > Polygons.Count || Polygons[side.ControlPanelPermutation].Type != PolygonType.Platform) {
		    side.IsControlPanel = false;
		}
	    }
	}

	public void NukeObjects() {
	    while (Objects.Count > 0) {
		DeleteObject(0);
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

	    for (int i = Sides.Count - 1; i >= 0; --i) {
		Side side = Sides[i];
		if (side.PolygonIndex == -1 || side.LineIndex == -1) {
		    DeleteSide((short) i);
		}
	    }

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
		if (cw_side == null && cw_polygon != null && (ccw_polygon == null || ccw_polygon.Type == PolygonType.Platform || cw_polygon.FloorHeight < ccw_polygon.FloorHeight || cw_polygon.CeilingHeight > ccw_polygon.CeilingHeight)) {
		    cw_side = Sides[NewSide(line.ClockwisePolygonOwner, (short) i)];
		}

		if (ccw_side == null && ccw_polygon != null && (cw_polygon == null || cw_polygon.Type == PolygonType.Platform || ccw_polygon.FloorHeight < cw_polygon.FloorHeight || ccw_polygon.CeilingHeight > cw_polygon.CeilingHeight)) {
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

	public List<Polygon> AdjacentPolygons(Polygon polygon) {
	    List<Polygon> result = new List<Polygon>();
	    for (int i = 0; i < polygon.VertexCount; ++i) {
		Line line = Lines[polygon.LineIndexes[i]];
		if (line.ClockwisePolygonOwner == -1 || line.CounterclockwisePolygonOwner == -1) 
		    continue;

		if (Polygons[line.CounterclockwisePolygonOwner] == polygon) {
		    if (line.ClockwisePolygonOwner != -1) {
			result.Add(Polygons[line.ClockwisePolygonOwner]);
		    }
		} else {
		    if (line.CounterclockwisePolygonOwner != -1) {
			result.Add(Polygons[line.CounterclockwisePolygonOwner]);
		    }
		}
	    }
	    return result;
	}

	public short LowestAdjacentFloor(Polygon polygon) {
	    short min = short.MaxValue;
	    foreach (Polygon adjacent in AdjacentPolygons(polygon)) {
		if (adjacent.FloorHeight < min) {
		    min = adjacent.FloorHeight;
		}
	    }
	    return min;
	}

	public short HighestAdjacentFloor(Polygon polygon) {
	    short max = short.MinValue;
	    foreach (Polygon adjacent in AdjacentPolygons(polygon)) {
		if (adjacent.FloorHeight > max) {
		    max = adjacent.FloorHeight;
		}
	    }
	    return max;
	}

	public short LowestAdjacentCeiling(Polygon polygon) {
	    short min = short.MaxValue;
	    foreach (Polygon adjacent in AdjacentPolygons(polygon)) {
		if (adjacent.CeilingHeight < min) {
		    min = adjacent.CeilingHeight;
		}
	    }
	    return min;
	}

	public short HighestAdjacentCeiling(Polygon polygon) {
	    short max = short.MinValue;
	    foreach (Polygon adjacent in AdjacentPolygons(polygon)) {
		if (adjacent.CeilingHeight > max) {
		    max = adjacent.CeilingHeight;
		}
	    }
	    return max;
	}

	public short AutocalPlatformMinimum(short platform_index) {
	    Platform platform = Platforms[platform_index];
	    Polygon polygon = Polygons[platform.PolygonIndex];
	    if (platform.ComesFromCeiling && platform.ComesFromFloor) {
		return LowestAdjacentFloor(polygon);
	    } else if (platform.ComesFromCeiling) {
		return LowestAdjacentCeiling(polygon);
	    } else {
		return LowestAdjacentFloor(polygon);
	    }
	}
	
	public short AutocalPlatformMaximum(short platform_index) {
	    Platform platform = Platforms[platform_index];
	    Polygon polygon = Polygons[platform.PolygonIndex];
	    if (platform.ComesFromCeiling && platform.ComesFromFloor) {
		return HighestAdjacentCeiling(polygon);
	    } else if (platform.ComesFromCeiling) {
		return HighestAdjacentCeiling(polygon);
	    } else {
		return HighestAdjacentFloor(polygon);
	    }
	}

	public short FindZeroLengthLine() {
	    for (int i = 0; i < Lines.Count; ++i) {
		Line line = Lines[i];
		Point p0 = Endpoints[line.EndpointIndexes[0]];
		Point p1 = Endpoints[line.EndpointIndexes[1]];
		if (Distance(p0, p1) == 0) {
		    return (short) i;
		}
	    }
	    return -1;
	}
    }
}
