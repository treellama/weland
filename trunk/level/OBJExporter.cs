using System;
using System.IO;
using System.Collections.Generic;

namespace Weland {
    public class OBJExporter {
	const double Scale = 32.0;

	public OBJExporter(Level level) {
	    this.level = level;
	}

	public void Export(string path) {
	    faces.Clear();
	    vertices.Clear();
	    endpointVertices.Clear();
	    for (int i = 0; i < level.Endpoints.Count; ++i) {
		endpointVertices.Add(new Dictionary<short, int>());
	    }

	    foreach (Polygon p in level.Polygons) {
		if (p.CeilingHeight > p.FloorHeight) {
		    if (p.FloorTransferMode != 9) {
			faces.Add(FloorFace(p));
		    }
		    if (p.CeilingTransferMode != 9) {
			faces.Add(CeilingFace(p));
		    }
		    for (int i = 0; i < p.VertexCount; ++i) {
			InsertLineFaces(level.Lines[p.LineIndexes[i]], p);
		    }
		}
	    }

	    using (TextWriter w = new StreamWriter(path)) {
		
		foreach (Vertex v in vertices) {
		    v.Write(w);
		}
		
		foreach (int[] f in faces) {
		    WriteFace(w, f);
		}
	    }
	}

	int GetVertexIndex(int endpointIndex, short height) {
	    if (!endpointVertices[endpointIndex].ContainsKey(height)) {
		Point p = level.Endpoints[endpointIndex];
		Vertex v = new Vertex();
		v.X = p.X;
		v.Y = p.Y;
		v.Z = height;
		endpointVertices[endpointIndex][height] = vertices.Count;
		vertices.Add(v);
	    }
	    return endpointVertices[endpointIndex][height];
	}

	int[] FloorFace(Polygon p) {
	    int[] result = new int[p.VertexCount];
	    for (int i = 0; i < p.VertexCount; ++i) {
		result[i] = GetVertexIndex(p.EndpointIndexes[i], p.FloorHeight);
	    }
	    return result;
	}

	int[] CeilingFace(Polygon p) {
	    int[] result = new int[p.VertexCount];
	    for (int i = 0; i < p.VertexCount; ++i) {
		result[i] = GetVertexIndex(p.EndpointIndexes[i], p.CeilingHeight);
	    }

	    return result;
	}

	int[] BuildFace(int left, int right, short ceiling, short floor) {
	    int[] result = new int[4];
	    result[0] = GetVertexIndex(left, floor);
	    result[1] = GetVertexIndex(right, floor);
	    result[2] = GetVertexIndex(right, ceiling);
	    result[3] = GetVertexIndex(left, ceiling);

	    return result;
	}

	static void WriteFace(TextWriter w, int[] face) {
	    w.Write("f");
	    for (int i = 0; i < face.Length; ++i) {
		w.Write(" " + (face[i] + 1));
	    }
	    w.WriteLine();
	}
	
	void InsertLineFaces(Line line, Polygon p) {
	    int left;
	    int right;
	    Polygon opposite = null;
	    Side side = null;
	    if (line.ClockwisePolygonOwner != -1 && level.Polygons[line.ClockwisePolygonOwner] == p) {
		left = line.EndpointIndexes[0];
		right = line.EndpointIndexes[1];
		if (line.CounterclockwisePolygonOwner != -1) {
		    opposite = level.Polygons[line.CounterclockwisePolygonOwner];
		}
		if (line.ClockwisePolygonSideIndex != -1) {
		    side = level.Sides[line.ClockwisePolygonSideIndex];
		}
	    } else {
		left = line.EndpointIndexes[1];
		right = line.EndpointIndexes[0];
		if (line.ClockwisePolygonOwner != -1) {
		    opposite = level.Polygons[line.ClockwisePolygonOwner];
		}
		if (line.CounterclockwisePolygonSideIndex != -1) {
		    side = level.Sides[line.CounterclockwisePolygonSideIndex];
		}
	    }

	    bool landscapeTop = false;
	    bool landscapeBottom = false;
	    if (side != null) {
		if (side.Type == SideType.Low) {
		    if (side.PrimaryTransferMode == 9) {
			landscapeBottom = true;
		    }
		} else {
		    if (side.PrimaryTransferMode == 9) {
			landscapeTop = true;
		    }
		    if (side.SecondaryTransferMode == 9) {
			landscapeBottom = true;
		    }
		}
	    }

	    if (opposite == null || (opposite.FloorHeight > p.CeilingHeight || opposite.CeilingHeight < p.FloorHeight)) {
		if (!landscapeTop) {
		    faces.Add(BuildFace(left, right, p.FloorHeight, p.CeilingHeight));
		}
	    } else {
		if (opposite.FloorHeight > p.FloorHeight) {
		    if (!landscapeBottom) {
			faces.Add(BuildFace(left, right, p.FloorHeight, opposite.FloorHeight));
		    }
		}
		if (opposite.CeilingHeight < p.CeilingHeight) {
		    if (!landscapeTop) {
			faces.Add(BuildFace(left, right, opposite.CeilingHeight, p.CeilingHeight));
		    }
		}
	    }
	}

	struct Vertex {
	    public short X;
	    public short Y;
	    public short Z;
	    
	    public void Write(TextWriter w) {
		w.WriteLine("v {0} {1} {2}", World.ToDouble(X) * Scale, World.ToDouble(Z) * Scale, World.ToDouble(Y) * Scale);
	    }
	}

	Level level;
	List<Vertex> vertices = new List<Vertex>();
	List<Dictionary<short, int>> endpointVertices = new List<Dictionary<short, int>>();
	List<int[]> faces = new List<int[]>();
    }
}