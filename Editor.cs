using System;
using System.Collections.Generic;

namespace Weland {
    public enum Tool {
	Select,
	Zoom,
	Move,
	Line,
	Fill,
    }

    public class Editor {
	public bool Dirty = false;
	public short RedrawTop, RedrawBottom, RedrawLeft, RedrawRight;

	public bool Changed = false;

	Wadfile.DirectoryEntry undoState;

	public short Snap;
	public Level Level;
	public Tool Tool {
	    get {
		return tool;
	    }
	    set {
		tool = value;
		if (value != Tool.Select) {
		    Level.SelectedPoint = -1;
		}
	    }
	}
	Tool tool;
		

	public Point LineStart;
	public Point LineEnd;

	bool undoSet = false;
	short lastX;
	short lastY;

	public void StartLine(short X, short Y) {
	    SetUndo();
	    Point p = new Point(X, Y);
	    short index = Level.GetClosestPoint(p);
	    if (index != -1 && Level.Distance(p, Level.Endpoints[index]) < Snap) {
		Level.TemporaryLineStartIndex = index;
		Level.TemporaryLineEnd = new Point(Level.Endpoints[index].X, Level.Endpoints[index].Y);
	    } else {
		index = Level.GetClosestLine(p);
		if (index != -1 && Level.Distance(p, Level.Lines[index]) < Snap && Level.Lines[index].ClockwisePolygonOwner == -1 && Level.Lines[index].CounterclockwisePolygonOwner == -1) {
		    //split
		    Level.TemporaryLineStartIndex = Level.SplitLine(index, X, Y);
		    Level.TemporaryLineEnd = new Point(X, Y);
		} else {
		    Level.TemporaryLineStartIndex = Level.NewPoint(X, Y);
		    Level.TemporaryLineEnd = new Point(X, Y);
		}
	    }

	    Changed = true;
	}

	public void UpdateLine(short X, short Y) {
	    Point p = new Point(X, Y);
	    AddDirty(Level.Endpoints[Level.TemporaryLineStartIndex]);
	    AddDirty(Level.TemporaryLineEnd);
	    short index = Level.GetClosestPoint(p);
	    if (index != -1 && Level.Distance(p, Level.Endpoints[index]) < Snap) {
		Level.TemporaryLineEnd.X = Level.Endpoints[index].X;
		Level.TemporaryLineEnd.Y = Level.Endpoints[index].Y;
	    } else {
		Level.TemporaryLineEnd.X = X;
		Level.TemporaryLineEnd.Y = Y;
	    }
	    AddDirty(Level.TemporaryLineEnd);

	    Changed = true;
	}

	public void ConnectLine(short X, short Y) {
	    Point p = new Point(X, Y);

	    // don't draw really short lines
	    if (Level.Distance(Level.Endpoints[Level.TemporaryLineStartIndex], p) < Snap) {
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

	    short index = Level.GetClosestPoint(p);
	    if (index != -1 && Level.Distance(p, Level.Endpoints[index]) < Snap) {
		Level.NewLine(Level.TemporaryLineStartIndex, index);
	    } else {
		index = Level.GetClosestLine(p);
		if (index != -1 && Level.Distance(p, Level.Lines[index]) < Snap && Level.Lines[index].ClockwisePolygonOwner == -1 && Level.Lines[index].CounterclockwisePolygonOwner == -1) {
		    Level.NewLine(Level.TemporaryLineStartIndex, Level.SplitLine(index, X, Y));
		} else {
		    Level.NewLine(Level.TemporaryLineStartIndex, Level.NewPoint(X, Y));
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
	    Point p = new Point(X, Y);
	    short index = Level.GetClosestPoint(p);
	    if (index != -1 && Level.Distance(p, Level.Endpoints[index]) < Snap) {
		Level.SelectedPoint = index;
	    } else {
		Level.SelectedPoint = -1;
	    }
	    undoSet = false;
	}

	void MoveSelected(short X, short Y) {
	    if (Level.SelectedPoint != -1) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}
		Point p = Level.Endpoints[Level.SelectedPoint];
		AddDirty(p);
		p.X = (short) (p.X + X - lastX);
		p.Y = (short) (p.Y + Y - lastY);
		Level.Endpoints[Level.SelectedPoint] = p;
		AddDirty(p);
		
		// dirty every endpoint line(!)
		List<short> lines = Level.EndpointLines(Level.SelectedPoint);
		foreach (short i in lines) {
		    AddDirty(Level.Endpoints[Level.Lines[i].EndpointIndexes[0]]);
		    AddDirty(Level.Endpoints[Level.Lines[i].EndpointIndexes[1]]);
		}

		// update attached polygon concavity
		List<short> polygons = Level.EndpointPolygons(Level.SelectedPoint);
		
		// dirty every attached polygon(!!?)
		foreach (short i in polygons) {
		    Polygon polygon = Level.Polygons[i];
		    Level.UpdatePolygonConcavity(polygon);
		    for (int j = 0; j < polygon.VertexCount; ++j) {
			AddDirty(Level.Endpoints[Level.Lines[polygon.LineIndexes[j]].EndpointIndexes[0]]);
			AddDirty(Level.Endpoints[Level.Lines[polygon.LineIndexes[j]].EndpointIndexes[1]]);
		    }
		}
	    }
	}

	public void ButtonPress(short X, short Y) {
	    if (Tool == Tool.Line) {
		StartLine(X, Y);
	    } else if (Tool == Tool.Fill) {
		Fill(X, Y);
	    } else if (Tool == Tool.Select) {
		Select(X, Y);
	    }
	    lastX = X;
	    lastY = Y;
	}

	public void Motion(short X, short Y) {
	    if (Tool == Tool.Line) {
		UpdateLine(X, Y);
	    } else if (Tool == Tool.Select) {
		MoveSelected(X, Y);
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
	    if (Level.SelectedPoint != -1) {
		SetUndo();
		Level.DeletePoint(Level.SelectedPoint);
		Level.SelectedPoint = -1;
	    }
	}

	public void SetUndo() {
	    undoState = Level.Save().Clone();
	}

	public void Undo() {
	    if (undoState != null) {
		Wadfile.DirectoryEntry redo = Level.Save().Clone();
		Level.Load(undoState);
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
    }
}