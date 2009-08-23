using System;
using System.Collections.Generic;

namespace Weland {
    public enum Tool {
	Zoom,
	Move,
	Line,
	Fill
    }

    public class Editor {
	public bool Dirty = false;
	public short RedrawTop, RedrawBottom, RedrawLeft, RedrawRight;

	public bool Changed = false;

	Wadfile.DirectoryEntry undoState;

	public short Snap;
	public Level Level;
	public Tool Tool = Tool.Zoom;

	public Point LineStart;
	public Point LineEnd;

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

	public void ButtonPress(short X, short Y) {
	    if (Tool == Tool.Line) {
		StartLine(X, Y);
	    } else if (Tool == Tool.Fill) {
		Wadfile.DirectoryEntry prevUndo = undoState.Clone();
		SetUndo();
		if (!Level.FillPolygon(X, Y)) {
		    undoState = prevUndo;
		}
	    }
	}

	public void Motion(short X, short Y) {
	    if (Tool == Tool.Line) {
		UpdateLine(X, Y);
	    }
	}

	public void ButtonRelease(short X, short Y) {
	    if (Tool == Tool.Line) {
		ConnectLine(X, Y);
	    }
	}

	public void SetUndo() {
	    undoState = Level.Save().Clone();
	}

	public void Undo() {
	    Wadfile.DirectoryEntry redo = Level.Save().Clone();
	    Level.Load(undoState);
	    undoState = redo;
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