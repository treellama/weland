using System;
using System.Collections.Generic;

namespace Weland {
    public enum Tool {
	Select,
	Zoom,
	Move,
	Line,
	Fill,
	FloorHeight,
	CeilingHeight
    }

    [Flags] public enum EditorModifiers {
	None,
	Shift = 0x01,
	Control = 0x02,
	Alt = 0x04,
	RightClick = 0x08
    }

    public class Grid {
	public bool Visible = true;
	public bool Snap = true;
	public short Resolution = 1024;
    }

    public class Selection {
	public short Point = -1;
	public short Object = -1;
	public short Line = -1;

	public void Clear() {
	    Point = -1;
	    Object = -1;
	    Line = -1;
	}

	public void CopyFrom(Selection other) {
	    Point = other.Point;
	    Object = other.Object;
	    Line = other.Line;
	}
    }

    public class Editor {
	public bool Dirty = false;
	public short RedrawTop, RedrawBottom, RedrawLeft, RedrawRight;

	public bool Changed = false;
	public Grid Grid;
	public Selection Selection;
	Wadfile.DirectoryEntry undoState;
	Selection undoSelection;

	public short Snap;
	public Level Level;
	public Tool Tool {
	    get {
		return tool;
	    }
	    set {
		tool = value;
		if (value != Tool.Select && value != Tool.Move) {
		    Selection.Clear();
		}
	    }
	}
	Tool tool;
	public short PaintIndex;
		

	bool undoSet = false;
	short lastX;
	short lastY;

	short GridAdjust(short value) {
	    if (Grid.Visible && Grid.Snap) {
		return (short) (Math.Round((double) value / Grid.Resolution) * Grid.Resolution);
	    } else {
		return value;
	    }
	}

	short ClosestPoint(Point p) {
	    short index = Level.GetClosestPoint(p);
	    if (index != -1 && Level.Distance(p, Level.Endpoints[index]) < Snap) {
		return index;
	    } else {
		return -1;
	    }
	}

	short ClosestObject(Point p) {
	    short index = Level.GetClosestObject(p);
	    if (index != -1 && Level.Distance(p, new Point(Level.Objects[index].X, Level.Objects[index].Y)) < Snap) {
		return index;
	    } else {
		return -1;
	    }
	}

	short ClosestLine(Point p) {
	    short index = Level.GetClosestLine(p);
	    if (index != -1 && Level.Distance(p, Level.Lines[index]) < Snap) {
		return index;
	    } else {
		return -1;
	    }
	}

	public void StartLine(short X, short Y) {
	    SetUndo();
	    Point p = new Point(X, Y);
	    short index = ClosestPoint(p);
	    if (index == -1) {
		p = new Point(GridAdjust(X), GridAdjust(Y));
		index = ClosestPoint(p);
	    }
	    
	    if (index != -1) {
		Level.TemporaryLineStartIndex = index;
		Level.TemporaryLineEnd = p;
	    } else {
		index = Level.GetClosestLine(p);
		if (index != -1 && Level.Distance(p, Level.Lines[index]) < Snap && Level.Lines[index].ClockwisePolygonOwner == -1 && Level.Lines[index].CounterclockwisePolygonOwner == -1) {
		    //split
		    Level.TemporaryLineStartIndex = Level.SplitLine(index, p.X, p.Y);
		    Level.TemporaryLineEnd = p;
		} else {
		    Level.TemporaryLineStartIndex = Level.NewPoint(p.X, p.Y);
		    Level.TemporaryLineEnd = p;
		}
	    }

	    Changed = true;
	}

	public void UpdateLine(short X, short Y) {
	    AddDirty(Level.Endpoints[Level.TemporaryLineStartIndex]);
	    AddDirty(Level.TemporaryLineEnd);
	    Point p = new Point(X, Y);
	    short index = ClosestPoint(p);
	    if (index == -1) {
		p = new Point(GridAdjust(X), GridAdjust(Y));
		index = ClosestPoint(p);
	    }

	    if (index != -1) {
		Level.TemporaryLineEnd.X = Level.Endpoints[index].X;
		Level.TemporaryLineEnd.Y = Level.Endpoints[index].Y;
	    } else {
		Level.TemporaryLineEnd.X = p.X;
		Level.TemporaryLineEnd.Y = p.Y;
	    }
	    AddDirty(Level.TemporaryLineEnd);

	    Changed = true;
	}

	public void ConnectLine(short X, short Y) {
	    Point p = new Point(X, Y);
	    Point ap = new Point(GridAdjust(X), GridAdjust(Y));

	    // don't draw really short lines
	    if (Level.Distance(Level.Endpoints[Level.TemporaryLineStartIndex], ap) < Snap) {
		// if the start point is the latest created, and unconnected, remove it
		bool connected = false;
		if (Level.TemporaryLineStartIndex == Level.Endpoints.Count - 1) {
		    foreach (Line line in Level.Lines) {
			if (line.EndpointIndexes[0] == Level.TemporaryLineStartIndex || line.EndpointIndexes[1] == Level.TemporaryLineStartIndex) {
			    connected = true;
			    break;
			}
		    }

		    if (!connected) {
			Level.Endpoints.RemoveAt(Level.TemporaryLineStartIndex);
		    }
		}
		Level.TemporaryLineStartIndex = -1;
		return;
	    }

	    short index = ClosestPoint(p);
	    if (index == -1) {
		p = ap;
		index = ClosestPoint(p);
	    }

	    if (index != -1) {
		Level.NewLine(Level.TemporaryLineStartIndex, index);
	    } else {
		index = Level.GetClosestLine(p);
		if (index != -1 && Level.Distance(p, Level.Lines[index]) < Snap && Level.Lines[index].ClockwisePolygonOwner == -1 && Level.Lines[index].CounterclockwisePolygonOwner == -1) {
		    Level.NewLine(Level.TemporaryLineStartIndex, Level.SplitLine(index, p.X, p.Y));
		} else {
		    Level.NewLine(Level.TemporaryLineStartIndex, Level.NewPoint(p.X, p.Y));
		}
	    }
	    Level.TemporaryLineStartIndex = -1;	   

	    Changed = true;
	}

	void Fill(short X, short Y) {
	    Wadfile.DirectoryEntry prevUndo = null;
	    if (undoState != null) {
		prevUndo = undoState.Clone();
	    }
	    SetUndo();
	    if (!Level.FillPolygon(X, Y)) {
		undoState = prevUndo;
	    }
	}

	void Select(short X, short Y) {
	    Selection.Clear();
	    Point p = new Point(X, Y);
	    short index = ClosestPoint(p);
	    if (index != -1) {
		Selection.Point = index;
	    } else {
		index = ClosestObject(p);
		if (index != -1) {
		    Selection.Object = index;
		} else {
		    index = ClosestLine(p);
		    if (index != -1) {
			Selection.Line = index;
		    }
		}
	    }
	    undoSet = false;
	}

	void TranslatePoint(short index, int X, int Y) {
		Point p = Level.Endpoints[index];
		AddDirty(p);
		p.X = (short) (p.X + X);
		p.Y = (short) (p.Y + Y);
		Level.Endpoints[index] = p;
		AddDirty(p);
		
		// dirty every endpoint line(!)
		List<short> lines = Level.EndpointLines(index);
		foreach (short i in lines) {
		    AddDirty(Level.Endpoints[Level.Lines[i].EndpointIndexes[0]]);
		    AddDirty(Level.Endpoints[Level.Lines[i].EndpointIndexes[1]]);
		}

		// update attached polygon concavity
		List<short> polygons = Level.EndpointPolygons(index);
		
		// dirty every attached polygon(!!?)
		foreach (short i in polygons) {
		    Polygon polygon = Level.Polygons[i];
		    Level.UpdatePolygonConcavity(polygon);
		    DirtyPolygon(polygon);
		}

	}

	void MoveSelected(short X, short Y) {
	    if (Selection.Point != -1) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}

		TranslatePoint(Selection.Point, X - lastX, Y - lastY);
	    } else if (Selection.Object != -1) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}

		MapObject obj = Level.Objects[Selection.Object];
		AddDirty(new Point(obj.X, obj.Y));
		Point p = new Point((short) (X), (short) (Y));
		short polygon_index = Level.GetEnclosingPolygon(p);
		if (polygon_index != -1) {
		    obj.X = p.X;
		    obj.Y = p.Y;
		    AddDirty(new Point(obj.X, obj.Y));
		    obj.PolygonIndex = polygon_index;
		}
	    } else if (Selection.Line != -1) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}

		Line line = Level.Lines[Selection.Line];
		TranslatePoint(line.EndpointIndexes[0], X - lastX, Y - lastY);
		TranslatePoint(line.EndpointIndexes[1], X - lastX, Y - lastY);
	    }
	}

	void DirtyPolygon(Polygon p) {
	    for (int i = 0; i < p.VertexCount; ++i) {
		AddDirty(Level.Endpoints[Level.Lines[p.LineIndexes[i]].EndpointIndexes[0]]);
		AddDirty(Level.Endpoints[Level.Lines[p.LineIndexes[i]].EndpointIndexes[1]]);
	    }
	}

	bool Alt(EditorModifiers mods) {
	    return ((mods & EditorModifiers.Alt) != 0);
	}

	bool RightClick(EditorModifiers mods) {
	    return ((mods & EditorModifiers.RightClick) != 0);
	}

	Polygon FindPolygon(short X,  short Y) {
	    short index = Level.GetEnclosingPolygon(new Point(X, Y));
	    if (index != -1) {
		return Level.Polygons[index];
	    } else {
		return null;
	    }
	}

	void GetFloorHeight(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		PaintIndex = p.FloorHeight;
	    }
	}
	
	void SetFloorHeight(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}
		p.FloorHeight = PaintIndex;
		DirtyPolygon(p);
	    }
	}

	void GetCeilingHeight(short X, short Y) {
	    Polygon p = FindPolygon(X,  Y);
	    if (p != null) {
		PaintIndex = p.CeilingHeight;
	    }
	}

	void SetCeilingHeight(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}
	    }
	    p.CeilingHeight = PaintIndex;
	    DirtyPolygon(p);
	}

	public void ButtonPress(short X, short Y, EditorModifiers mods) {
	    if (Tool == Tool.Line) {
		StartLine(X, Y);
	    } else if (Tool == Tool.Fill) {
		Fill(X, Y);
	    } else if (Tool == Tool.Select) {
		Select(X, Y);
	    } else if (Tool == Tool.FloorHeight) {
		if (Alt(mods) || RightClick(mods)) {
		    GetFloorHeight(X, Y);
		} else {
		    undoSet = false;
		    SetFloorHeight(X, Y);
		}
	    } else if (Tool == Tool.CeilingHeight) {
		if (Alt(mods) || RightClick(mods)) {
		    GetCeilingHeight(X, Y);
		} else {
		    undoSet = false;
		    SetCeilingHeight(X, Y);
		}
	    }
	    lastX = X;
	    lastY = Y;
	}

	public void Motion(short X, short Y, EditorModifiers mods) {
	    if (Tool == Tool.Line) {
		UpdateLine(X, Y);
	    } else if (Tool == Tool.Select) {
		MoveSelected(X, Y);
	    } else if (Tool == Tool.FloorHeight) {
		if (Alt(mods) || RightClick(mods)) {
		    GetFloorHeight(X, Y);
		} else {
		    SetFloorHeight(X, Y);
		}
	    } else if (Tool == Tool.CeilingHeight) {
		if (Alt(mods) || RightClick(mods)) {
		    GetCeilingHeight(X, Y);
		} else {
		    SetCeilingHeight(X, Y);
		}
	    }
	    lastX = X;
	    lastY = Y;
	}

	public void ButtonRelease(short X, short Y) {
	    if (Tool == Tool.Line) {
		ConnectLine(X, Y);
	    }
	}

	public void DeleteSelected() {
	    if (Selection.Point != -1) {
		// find the closest point to delete next

		// this is pretty ridiculous: endpoint indices can
		// change after the delete, and so can line indices;
		// so remember a line reference and index into that
		// line's EndpointIndexes list
		List<short> nextPointCandidates = new List<short>();
		foreach (short i in Level.EndpointLines(Selection.Point)) {
		    if (Level.Lines[i].EndpointIndexes[0] == Selection.Point) {
			nextPointCandidates.Add(Level.Lines[i].EndpointIndexes[1]);
		    } else {
			nextPointCandidates.Add(Level.Lines[i].EndpointIndexes[0]);
		    }
		}

		nextPointCandidates.Sort(delegate(short p0, short p1) {
			return Level.Distance(Level.Endpoints[p0], Level.Endpoints[Selection.Point]) - Level.Distance(Level.Endpoints[p1], Level.Endpoints[Selection.Point]);
		    });
		
		Line nextLine = null;
		int nextLineEndpointIndex = 0;
		foreach (short point_index in nextPointCandidates) {
		    foreach (short i in Level.EndpointLines(point_index)) {
			if (Level.Lines[i].EndpointIndexes[0] == point_index &&
			    Level.Lines[i].EndpointIndexes[1] != Selection.Point) {
			    nextLine = Level.Lines[i];
			    nextLineEndpointIndex = 0;
			    break;
			} else if (Level.Lines[i].EndpointIndexes[1] == point_index &&
				   Level.Lines[i].EndpointIndexes[0] != Selection.Point) {
			    nextLine = Level.Lines[i];
			    nextLineEndpointIndex = 1;
			}
		    }
		    if (nextLine != null) {
			break;
		    }
		}
		SetUndo();
		Level.DeletePoint(Selection.Point);
		if (nextLine != null) {
		    Selection.Point = nextLine.EndpointIndexes[nextLineEndpointIndex];
		} else {
		    Selection.Point = -1;
		}
	    } else if (Selection.Object != -1) {
		SetUndo();
		Level.DeleteObject(Selection.Object);
		Selection.Object = -1;
	    } else if (Selection.Line != -1) {
		SetUndo();
		Level.DeleteLine(Selection.Line);
		Selection.Line = -1;
	    }
	}

	public void SetUndo() {
	    undoState = Level.Save().Clone();
	    undoSelection = new Selection();
	    undoSelection.CopyFrom(Selection);
	}

	public void Undo() {
	    if (undoState != null) {
		Wadfile.DirectoryEntry redo = Level.Save().Clone();
		Level.Load(undoState);
		Selection temp = new Selection();
		temp.CopyFrom(Selection);
		Selection.CopyFrom(undoSelection);
		undoSelection.CopyFrom(temp);
		undoState = redo;
	    }
	}

	// add to the dirty rectangle
	void AddDirty(Point p) {
	    if (!Dirty) {
		Dirty = true;
		RedrawTop = RedrawBottom = p.Y;
		RedrawLeft = RedrawRight = p.X;
	    } else {
		if (p.X < RedrawLeft) {
		    RedrawLeft = p.X;
		} 
		if (p.X > RedrawRight) {
		    RedrawRight = p.X;
		}
		if (p.Y > RedrawBottom) {
		    RedrawBottom = p.Y;
		} 
		if (p.Y < RedrawTop) {
		    RedrawTop = p.Y;
		}
	    }
	}

	public void ClearDirty() {
	    Dirty = false;
	}

	public SortedList<short, bool> GetFloorHeights() {
	    SortedList<short, bool> list = new SortedList<short, bool>();
	    foreach (Polygon p in Level.Polygons) {
		list[p.FloorHeight] = true;
	    }
	    
	    return list;
	}
	
	public SortedList<short, bool> GetCeilingHeights() {
	    SortedList<short, bool> list = new SortedList<short, bool>();
	    foreach (Polygon p in Level.Polygons) {
		list[p.CeilingHeight] = true;
	    }
	    return list;
	}

	public void ChangeFloorHeights(short old_height, short new_height) {
	    if (old_height != new_height) {
		SetUndo();
		foreach (Polygon p in Level.Polygons) {
		    if (p.FloorHeight == old_height) {
			p.FloorHeight = new_height;
		    }
		}
	    }
	}

	public void ChangeCeilingHeights(short old_height, short new_height) {
	    if (old_height != new_height) {
		SetUndo();
		foreach (Polygon p in Level.Polygons) {
		    if (p.CeilingHeight == old_height) {
			p.CeilingHeight = new_height;
		    }
		}
	    }
	}
    }
}