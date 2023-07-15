using System;
using System.Collections.Generic;

namespace Weland {
    public enum Tool {
	Select,
	Zoom,
	Move,
	Line,
	Fill,
	Object,
	Annotation,
	FloorHeight,
	CeilingHeight,
	PolygonType,
	FloorLight,
	CeilingLight,
	MediaLight,
	Media,
	AmbientSound,
	RandomSound,
	FloorTexture,
	CeilingTexture,
        VisualMode
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
	
	/*** begin custom grid code ***/
	public double Rotation = 0;
	public Point Center = new Point(0,0);
	public double Scale = 1;
	public bool UseCustomGrid = true;
	
	public int CurrentGrid = 1;

	public double MaxScale = 8.0;
	public double MinScale = 1.0 / 8.0;
	/*** end custom grid code ***/

    }

    public class Selection {
	public short Point = -1;
	public short Object = -1;
	public short Line = -1;
	public short Polygon = -1;
	public short Annotation = -1;

	public void Clear() {
	    Point = -1;
	    Object = -1;
	    Line = -1;
	    Polygon = -1;
	    Annotation = -1;
	}

	public void CopyFrom(Selection other) {
	    Point = other.Point;
	    Object = other.Object;
	    Line = other.Line;
	    Polygon = other.Polygon;
	    Annotation = other.Annotation;
	}
    }

    public class Editor {
	public bool Dirty = false;
	public bool RedrawAll = false;
	public short RedrawTop, RedrawBottom, RedrawLeft, RedrawRight;

	public bool Changed = false;
	public Grid Grid;
	public Selection Selection;
	Wadfile.DirectoryEntry undoState;
	Selection undoSelection;

	public double Scale;
	public Level Level;
	public Tool Tool {
	    get {
		return tool;
	    }
	    set {
		tool = value;
		if (Level.TemporaryLineStartIndex != -1) {
		    if (Level.FindEndpointLines(Level.TemporaryLineStartIndex).Count == 0) {
                        Level.DeletePoint(Level.TemporaryLineStartIndex);
		    }
		    Level.TemporaryLineStartIndex = -1;
		}
		if (value == Tool.Line) {
		    short selected_point = Selection.Point;
		    Selection.Clear();
		    Selection.Point = selected_point;
		} else if (value == Tool.Object) {
		    short selected_object = Selection.Object;
		    Selection.Clear();
		    Selection.Object = selected_object;
		} else if (value == Tool.Annotation) {
		    short selected_annotation = Selection.Annotation;
		    Selection.Clear();
		    Selection.Annotation = selected_annotation;
		} else if (value != Tool.Select && value != Tool.Move) {
		    Selection.Clear();
		}
	    }
	}
	Tool tool;
	public short PaintIndex;
	public ShapeDescriptor PaintDescriptor = new ShapeDescriptor();
	public short PaintTransferMode;
	MapObject lastObject = null;

	bool undoSet = false;
	short lastX;
	short lastY;
	short downX;
	short downY;

	public int DefaultSnapDistance;
	public int ObjectSnapDistance;
	public int InertiaDistance;
	public bool SplitPolygonLines;
	public bool SnapObjects;

	public void LoadSettings() {
	    DefaultSnapDistance = Weland.Settings.GetSetting("Distance/Select/Default", 4);
	    ObjectSnapDistance = Weland.Settings.GetSetting("Distance/Select/Object", 8);
	    InertiaDistance = Weland.Settings.GetSetting("Distance/Inertia/Default", 8);
	    SplitPolygonLines = Weland.Settings.GetSetting("Editor/SplitPolygonLines", false);
	    SnapObjects = Weland.Settings.GetSetting("Editor/SnapObjects", false);
	}

	public void SaveSettings() {
	    Weland.Settings.PutSetting("Distance/Select/Default", DefaultSnapDistance);
	    Weland.Settings.PutSetting("Distance/Select/Object", ObjectSnapDistance);
	    Weland.Settings.PutSetting("Distance/Inertia/Default", InertiaDistance);
	    Weland.Settings.PutSetting("Editor/SplitPolygonLines", SplitPolygonLines);
	    Weland.Settings.PutSetting("Editor/SnapObjects", SnapObjects);
	}

	public Editor() {
	    LoadSettings();
	}
	
	int DefaultSnap() {
	    return (int) Math.Round(DefaultSnapDistance / Scale);
	}

	int ObjectSnap() {
	    return (int) Math.Round(ObjectSnapDistance / Scale);
	}

	int Inertia() {
	    return (int) Math.Round(InertiaDistance / Scale);
	}
	
	/*** begin custom grid code ***/
	
	Point GridAdjust(Point p) {
		double s,c;
		int i,j;
		
		if (Grid.Visible && Grid.Snap) {
		if(Grid.UseCustomGrid) {
		c=Math.Cos(Grid.Rotation*2*Math.PI/360);
		s=Math.Sin(Grid.Rotation*2*Math.PI/360);
		
		i=(int)(Grid.Resolution*Grid.Scale*Math.Round((c*(p.X-Grid.Center.X)-s*(p.Y-Grid.Center.Y))/(Grid.Resolution*Grid.Scale)));
		j=(int)(Grid.Resolution*Grid.Scale*Math.Round((s*(p.X-Grid.Center.X)+c*(p.Y-Grid.Center.Y))/(Grid.Resolution*Grid.Scale)));
		p.X = (short)((double)Grid.Center.X + c*i + s*j);
		p.Y = (short)((double)Grid.Center.Y - s*i + c*j);
		} else {
		p.X = (short) (Math.Round((double) p.X / Grid.Resolution) * Grid.Resolution);
		p.Y = (short) (Math.Round((double) p.Y / Grid.Resolution) * Grid.Resolution);
		}	
	    }
		return p;
	}
	
	/*** old code ***/
	/*
	short GridAdjust(short value) {
	    if (Grid.Visible && Grid.Snap) {
		return (short) (Math.Round((double) value / Grid.Resolution) * Grid.Resolution);
	    } else {
		return value;
	    }
	}
	*/
	
	/*** end custom grid code ***/
	

	short ClosestAnnotation(Point p) {
	    short index = Level.GetClosestAnnotation(p);
	    if (index != -1 && Level.Distance(p, new Point(Level.Annotations[index].X, Level.Annotations[index].Y)) < DefaultSnap()) {
		return index;
	    } else {
		return -1;
	    }
	}

	short ClosestPoint(Point p) {
	    short index = Level.GetClosestPoint(p);
	    if (index != -1 && Level.Distance(p, Level.Endpoints[index]) < DefaultSnap()) {
		return index;
	    } else {
		return -1;
	    }
	}

	short ClosestObject(Point p) {
	    short index = Level.GetClosestObject(p);
	    if (index != -1 && Level.Distance(p, new Point(Level.Objects[index].X, Level.Objects[index].Y)) < ObjectSnap()) {
		return index;
	    } else {
		return -1;
	    }
	}

	short ClosestLine(Point p) {
	    short index = Level.GetClosestLine(p);
	    if (index != -1 && Level.Distance(p, Level.Lines[index]) < DefaultSnap()) {
		return index;
	    } else {
		return -1;
	    }
	}

	Point Constrain(Point p, bool constrainAngle, bool constrainLength) {
	    int StartX = Level.Endpoints[Level.TemporaryLineStartIndex].X;
	    int StartY = Level.Endpoints[Level.TemporaryLineStartIndex].Y;
	    int X = p.X - StartX;
	    int Y = p.Y - StartY;

	    double r = Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
	    double theta = Math.Atan2(Y, X);
	    
	    /*** begin custom grid code ***/
	    if (constrainLength) {
			if(Grid.UseCustomGrid) r = Math.Round(r / (Grid.Resolution * Grid.Scale)) * Grid.Resolution * Grid.Scale;
			else r = Math.Round(r / Grid.Resolution) * Grid.Resolution;
	    }

	    if (constrainAngle) {
		double smallest_angle = Math.PI / 8;
			if(Grid.UseCustomGrid) theta = -Grid.Rotation*Math.PI/180 + Math.Round((theta + Grid.Rotation*Math.PI/180) / smallest_angle) * smallest_angle;
			else theta = Math.Round(theta / smallest_angle) * smallest_angle;
	    }
	    /*** end custom grid code ***/

	    return new Point((short) (Math.Round(r * Math.Cos(theta)) + StartX), (short) (Math.Round(r * Math.Sin(theta)) + StartY));
	}

	bool CanSplitLine(short index) {
	    Line line = Level.Lines[index];
	    if (line.ClockwisePolygonOwner == -1 && line.CounterclockwisePolygonOwner == -1) {
		return true;
	    }

	    if (line.ClockwisePolygonOwner != -1 && Level.Polygons[line.ClockwisePolygonOwner].VertexCount == Polygon.MaxVertexCount) {
		return false;
	    }

	    if (line.CounterclockwisePolygonOwner != -1 && Level.Polygons[line.CounterclockwisePolygonOwner].VertexCount == Polygon.MaxVertexCount) {
		return false;
	    }

	    return SplitPolygonLines;
	}

	public void StartLine(short X, short Y) {
	    SetUndo();
	    Selection.Clear();
	    Point p = new Point(X, Y);
	    short index = ClosestPoint(p);
	    if (index == -1) {
		p = GridAdjust(new Point(X,Y));	//custom grid code
		//p = new Point(GridAdjust(X), GridAdjust(Y));	old code
		index = ClosestPoint(p);
	    }
	    
	    if (index != -1) {
		Level.TemporaryLineStartIndex = index;
		Level.TemporaryLineEnd = p;
	    } else {
		index = Level.GetClosestLine(p);
		if (index != -1 && Level.Distance(p, Level.Lines[index]) < DefaultSnap() && CanSplitLine(index)) {
		    //split
		    Point onLine = Level.ClosestPointOnLine(p, Level.Lines[index]);
		    Level.TemporaryLineStartIndex = Level.SplitLine(index, onLine.X, onLine.Y);
		    Level.TemporaryLineEnd = p;
		} else {
		    Level.TemporaryLineStartIndex = Level.NewPoint(p.X, p.Y);
		    Level.TemporaryLineEnd = p;
		}
	    }

	    Changed = true;
	}

	public void UpdateLine(short X, short Y, bool constrainAngle, bool constrainLength) {
	    if (Level.TemporaryLineStartIndex == -1) {
		return;
	    }

	    AddDirty(Level.Endpoints[Level.TemporaryLineStartIndex]);
	    AddDirty(Level.TemporaryLineEnd);
	    Point p = new Point(X, Y);
	    short index = -1;
	    if (constrainAngle || constrainLength) {
		p = Constrain(p, constrainAngle, constrainLength);
	    } else {
		index = ClosestPoint(p);
		if (index == -1) {
		    p = GridAdjust(new Point(X,Y));	//custom grid code
		    //p = new Point(GridAdjust(X), GridAdjust(Y));	old code
		    index = ClosestPoint(p);
		}
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

	public void ConnectLine(short X, short Y, bool constrainAngle, bool constrainLength) {
	    if (Level.TemporaryLineStartIndex == -1) {
		return;
	    }

	    Point p = new Point(X, Y);
	    Point ap = GridAdjust(new Point(X,Y));	//custom grid code
	    //Point ap = new Point(GridAdjust(X), GridAdjust(Y));	old code
	    Point z = ap;
	    if (constrainAngle || constrainLength) {
		p = Constrain(p, constrainAngle, constrainLength);
		z = p;
	    }

	    // don't draw really short lines
	    if (Level.Distance(Level.Endpoints[Level.TemporaryLineStartIndex], z) < DefaultSnap()) {
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

	    short index = -1;
	    if (!constrainLength && !constrainAngle) {
		index = ClosestPoint(p);
		if (index == -1) {
		    p = ap;
		    index = ClosestPoint(p);
		}
	    } else {
		// should really connect if it's right on top of the other point!
		index = Level.GetClosestPoint(p);
		if (index != -1 && Level.Distance(p, Level.Endpoints[index]) > 1 / Scale) {
		    index = -1;
		}
	    }

	    if (index != -1) {
		Level.NewLine(Level.TemporaryLineStartIndex, index);
		Selection.Point = index;
	    } else {
		index = Level.GetClosestLine(p);
		if (index != -1 && Level.Distance(p, Level.Lines[index]) < DefaultSnap() && CanSplitLine(index)) {
		    Point onLine = Level.ClosestPointOnLine(p, Level.Lines[index]);
		    Line line = Level.Lines[index];
		    if (line.EndpointIndexes[0] != Level.TemporaryLineStartIndex && line.EndpointIndexes[1] != Level.TemporaryLineStartIndex) {
			Level.NewLine(Level.TemporaryLineStartIndex, Level.SplitLine(index, onLine.X, onLine.Y));
		    } else {
			Level.SplitLine(index, onLine.X, onLine.Y);
		    }
		} else {
		    Level.NewLine(Level.TemporaryLineStartIndex, Level.NewPoint(p.X, p.Y));
		}
		Selection.Point = (short) (Level.Endpoints.Count - 1);
	    }
	    Level.TemporaryLineStartIndex = -1;	   

	    Changed = true;
	}

	void Fill(short X, short Y) {
	    if (!undoSet) {
		Wadfile.DirectoryEntry prevUndo = null;
		if (undoState != null) {
		    prevUndo = undoState.Clone();
		}
		SetUndo();
		if (!Level.FillPolygon(X, Y)) {
		    undoState = prevUndo;
		} else {
		    undoSet = true;
		    Changed = true;
		}
	    } else {
		if (Level.FillPolygon(X, Y)) {
		    Polygon p = FindPolygon(X, Y);
		    if (p != null) DirtyPolygon(p);
		}
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
		    lastObject = Level.Objects[index];
		} else {
		    index = ClosestAnnotation(p);
		    if (index != -1) {
			Selection.Annotation = index;
		    } else {
			index = ClosestLine(p);
			if (index != -1) {
			    Selection.Line = index;
			} else {
			    index = Level.GetEnclosingPolygon(p);
			    if (index != -1) {
				Selection.Polygon = index;
			    }
			}
		    }
		} 
	    }
	    undoSet = false;
	}

	void AltSelect(short X, short Y) {
	    if (Selection.Polygon != -1) {
		Polygon polygon = Level.Polygons[Selection.Polygon];
		if (polygon.Type == PolygonType.Teleporter) {
		    short index = Level.GetEnclosingPolygon(new Point(X, Y));
		    if (index != -1) {
			SetUndo();
			polygon.Permutation = index;
		    }
		}
	    }
	}

	public bool EditAnnotation = false; // HACK

	void PlaceAnnotation(short X, short Y) {
	    Point p = new Point(X, Y);
	    short index = ClosestAnnotation(p);
	    if (index != -1) {
		Selection.Clear();
		Selection.Annotation = index;
		EditAnnotation = false;
	    } else {
		short polygon_index = Level.GetEnclosingPolygon(p);
		if (polygon_index != -1) {
		    SetUndo();
		    Annotation note = new Annotation();
		    note.X = p.X;
		    note.Y = p.Y;
		    note.PolygonIndex = polygon_index;
		    note.Text = "Unknown";
		    Level.Annotations.Add(note);
		    Selection.Annotation = (short) (Level.Annotations.Count - 1);
		    Changed = true;
		    EditAnnotation = true;
		}
	    }
	}

        void PlaceVisualMode(short X, short Y) {
            Level.VisualModePoint = new Point(X, Y);
            Level.VisualModePolygonIndex = Level.GetEnclosingPolygon(Level.VisualModePoint);
        }

	void PlaceObject(short X, short Y) {
	    Point p = new Point(X, Y);
	    short index = ClosestObject(p);
	    if (index != -1) {
		Selection.Clear();
		Selection.Object = index;
		lastObject = Level.Objects[index];
	    } else {
		short polygon_index = Level.GetEnclosingPolygon(p);
		if (polygon_index != -1) {
		    SetUndo();
		    short object_index = Level.NewObject(p, polygon_index);
		    MapObject o = Level.Objects[object_index];
		    if (lastObject != null) {
			o.CopyFrom(lastObject);
		    } else {
			o.Type = ObjectType.Player;
		    }

		    Selection.Object = object_index;
		    lastObject = o;

		    if (o.Type == ObjectType.Monster) {
			Level.MonsterPlacement[o.Index].InitialCount++;
		    } else if (o.Type == ObjectType.Item) {
			Level.ItemPlacement[o.Index].InitialCount++;
		    }
		    Changed = true;
		}
	    }
	}

	void TranslatePoint(short index, int X, int Y) {
	    Point p = Level.Endpoints[index];
	    short x = (short) (p.X + X);
	    short y = (short) (p.Y + Y);
	    MovePoint(index, x, y);
	}

	void MovePoint(short index, short X, short Y) {
		Point p = Level.Endpoints[index];
		AddDirty(p);
		p.X = X;
		p.Y = Y;
		Level.Endpoints[index] = p;
		AddDirty(p);
		
		// dirty every endpoint line(!)
		HashSet<Line> lines = Level.EndpointLines[index];
		foreach (Line line in lines) {
		    AddDirty(Level.Endpoints[line.EndpointIndexes[0]]);
		    AddDirty(Level.Endpoints[line.EndpointIndexes[1]]);
		}

		// update attached polygon concavity
		List<short> polygons = Level.FindEndpointPolygons(index);
		
		// dirty every attached polygon(!!?)
		foreach (short i in polygons) {
		    Polygon polygon = Level.Polygons[i];
		    Level.UpdatePolygonConcavity(polygon);
		    DirtyPolygon(polygon);
		}

	}

	void TranslateObject(MapObject obj, int X, int Y) {
	    obj.X = (short) (obj.X + X);
	    obj.Y = (short) (obj.Y + Y);
	}

	void TranslateAnnotation(Annotation a, int X, int Y) {
	    a.X = (short) (a.X + X);
	    a.Y = (short) (a.Y + Y);
	}

	public void NudgeSelected(short X, short Y) {
	    bool changed = true;
	    SetUndo();
	    if (Selection.Point != -1) {
		Point p = Level.Endpoints[Selection.Point];
		short newX = (short) (p.X + X);
		short newY = (short) (p.Y + Y);
		/*** custom grid code ***/
		if (Grid.Visible && Grid.Snap) {
			if(Grid.UseCustomGrid) {
			    if (X > 0) {
				newX += (short)(Grid.Resolution*Grid.Scale);
			    } else if (X < 0) {
				newX -= (short)(Grid.Resolution*Grid.Scale);
			    }
			    if (Y > 0) {
				newY += (short)(Grid.Resolution*Grid.Scale);
			    } else if (Y < 0) {
				newY -= (short)(Grid.Resolution*Grid.Scale);
			    }
			} else {
				//original code
				if (X > 0) {
				newX += Grid.Resolution;
			    } else if (X < 0) {
				newX -= Grid.Resolution;
			    }
			    if (Y > 0) {
				newY += Grid.Resolution;
			    } else if (Y < 0) {
				newY -= Grid.Resolution;
				}
			}
		}
		Point adjp = new Point(newX,newY);
		adjp = GridAdjust(adjp);
		MovePoint(Selection.Point, adjp.X, adjp.Y);
		//MovePoint(Selection.Point, GridAdjust(newX), GridAdjust(newY));	old code
		/*** end custom grid code ***/
	    } else if (Selection.Object != -1) {
		MapObject obj = Level.Objects[Selection.Object];
		Point p = new Point((short) (obj.X + X), (short) (obj.Y + Y));
		short polygon_index = Level.GetEnclosingPolygon(p);
		if (polygon_index != -1) {
		    obj.X = (short) (obj.X + X);
		    obj.Y = (short) (obj.Y + Y);
		    obj.PolygonIndex = polygon_index;
		}
	    } else if (Selection.Line != -1) {
		Line line = Level.Lines[Selection.Line];
		TranslatePoint(line.EndpointIndexes[0], X, Y);
		TranslatePoint(line.EndpointIndexes[1], X, Y);
	    } else if (Selection.Polygon != -1) {
		Polygon polygon = Level.Polygons[Selection.Polygon];
		Dictionary<short, bool> endpoints = new Dictionary<short, bool>();
		for (int i = 0; i < polygon.VertexCount; ++i) {
		    Line line = Level.Lines[polygon.LineIndexes[i]];
		    endpoints[line.EndpointIndexes[0]] = true;
		    endpoints[line.EndpointIndexes[1]] = true;
		}

		foreach (short i in endpoints.Keys) {
		    TranslatePoint(i, X, Y);
		}

		foreach (MapObject mapObject in Level.Objects) {
		    if (mapObject.PolygonIndex == Selection.Polygon) {
			TranslateObject(mapObject, X, Y);
		    }
		}

		foreach (Annotation annotation in Level.Annotations) {
		    if (annotation.PolygonIndex == Selection.Polygon) {
			TranslateAnnotation(annotation, X, Y);
		    }
		}
	    } else if (Selection.Annotation != -1) {
		Annotation note = Level.Annotations[Selection.Annotation];
		TranslateAnnotation(note, X, Y);
	    } else {
		changed = false;
	    }

	    if (changed) {
		Changed = true;
	    }
	}

	void MoveSelected(short X, short Y) {
	    bool changed = true;
	    if (!undoSet) {
		if (Math.Abs(X - downX) < Inertia() && Math.Abs(Y - downY) < Inertia()) {
		    return;
		} else {
		    lastX = downX;
		    lastY = downY;
		}
	    }

	    if (Selection.Point != -1) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}
		
		/*** begin custom grid code ***/
		Point adjp = new Point(X,Y);
		adjp=GridAdjust(adjp);
		MovePoint(Selection.Point, adjp.X, adjp.Y);
		/*** end custom grid code ***/
		//MovePoint(Selection.Point, GridAdjust(X), GridAdjust(Y));	old code
	    } else if (Selection.Object != -1) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}

		MapObject obj = Level.Objects[Selection.Object];
		AddDirty(new Point(obj.X, obj.Y));
		Point p = new Point((short) (X), (short) (Y));
		if (SnapObjects) { p = GridAdjust(p); }
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
	    } else if (Selection.Polygon != -1) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}

		Polygon polygon = Level.Polygons[Selection.Polygon];
		Dictionary<short, bool> endpoints = new Dictionary<short, bool>();
		for (int i = 0; i < polygon.VertexCount; ++i) {
		    Line line = Level.Lines[polygon.LineIndexes[i]];
		    endpoints[line.EndpointIndexes[0]] = true;
		    endpoints[line.EndpointIndexes[1]] = true;
		}

		foreach (short i in endpoints.Keys) {
		    TranslatePoint(i, X - lastX, Y - lastY);
		}

		foreach (MapObject mapObject in Level.Objects) {
		    if (mapObject.PolygonIndex == Selection.Polygon) {
			TranslateObject(mapObject, X - lastX, Y - lastY);
		    }
		}

		foreach (Annotation annotation in Level.Annotations) {
		    if (annotation.PolygonIndex == Selection.Polygon) {
			TranslateAnnotation(annotation, X - lastX, Y - lastY);
		    }
		}
	    } else if (Selection.Annotation != -1) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}

		Annotation annotation = Level.Annotations[Selection.Annotation];
		Point p = new Point((short) (X), (short) (Y));
		short polygon_index = Level.GetEnclosingPolygon(p);
		if (polygon_index != -1) {
		    annotation.X = p.X;
		    annotation.Y = p.Y;
		    annotation.PolygonIndex = polygon_index;
		}
		RedrawAll = true;
		Dirty = true;
	    } else {
		changed = false;
	    }

	    if (changed) {
		Changed = true;
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
		Changed = true;
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
		p.CeilingHeight = PaintIndex;
		DirtyPolygon(p);
		Changed = true;
	    }
	}

	void GetPolygonType(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		PaintIndex = (short) p.Type;
	    }
	}

	void SetPolygonType(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}
	    
		bool scan = (p.Type == PolygonType.Platform || (PolygonType) PaintIndex == PolygonType.Platform);
		p.Type = (PolygonType) PaintIndex;
		if (scan) 
		    ScanPlatforms();
		DirtyPolygon(p);
		Changed = true;
	    }
	}

	void GetFloorLight(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		PaintIndex = p.FloorLight;
	    }
	}

	void SetMediaLight(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}
		p.MediaLight = PaintIndex;
		DirtyPolygon(p);
		Changed = true;
	    }
	}

	void GetMediaLight(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		PaintIndex = p.MediaLight;
	    }
	}

	void SetFloorLight(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}
		p.FloorLight = PaintIndex;
		DirtyPolygon(p);
		Changed = true;
	    }
	}

	void GetCeilingLight(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		PaintIndex = p.CeilingLight;
	    }
	}

	void SetCeilingLight(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}
		p.CeilingLight = PaintIndex;
		DirtyPolygon(p);
		Changed = true;
	    }
	}

	void GetMedia(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		PaintIndex = p.MediaIndex;
	    }
	}

	void SetMedia(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}
		p.MediaIndex = PaintIndex;
		DirtyPolygon(p);
		Changed = true;
	    }
	}

	void GetAmbientSound(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		PaintIndex = p.AmbientSound;
	    }
	}

	void SetAmbientSound(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}
		p.AmbientSound = PaintIndex;
		DirtyPolygon(p);
		Changed = true;
	    }
	}

	void GetRandomSound(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		PaintIndex = p.RandomSound;
	    }
	}

	void SetRandomSound(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}
		p.RandomSound = PaintIndex;
		DirtyPolygon(p);
		Changed = true;
	    }
	}

	void GetFloorTexture(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		PaintDescriptor = p.FloorTexture;
		PaintTransferMode = p.FloorTransferMode;
	    }
	}
	
	void SetFloorTexture(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}
		p.FloorTexture = PaintDescriptor;
		p.FloorTransferMode = PaintTransferMode;
		DirtyPolygon(p);
		Changed = true;
	    }
	}

	void GetCeilingTexture(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		PaintDescriptor = p.CeilingTexture;
		PaintTransferMode = p.CeilingTransferMode;
	    }
	}
	
	void SetCeilingTexture(short X, short Y) {
	    Polygon p = FindPolygon(X, Y);
	    if (p != null) {
		if (!undoSet) {
		    SetUndo();
		    undoSet = true;
		}
		p.CeilingTexture = PaintDescriptor;
		p.CeilingTransferMode = PaintTransferMode;
		DirtyPolygon(p);
		Changed = true;
	    }
	}

	public void ButtonPress(short X, short Y, EditorModifiers mods) {
	    if (Tool == Tool.FloorHeight) {
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
	    } else if (Tool == Tool.PolygonType) {
		if (Alt(mods) || RightClick(mods)) {
		    GetPolygonType(X, Y);
		} else {
		    undoSet = false;
		    SetPolygonType(X, Y);
		}
	    } else if (Tool == Tool.FloorLight) {
		if (Alt(mods) || RightClick(mods)) {
		    GetFloorLight(X, Y);
		} else {
		    undoSet = false;
		    SetFloorLight(X, Y);
		}
	    } else if (Tool == Tool.CeilingLight) {
		if (Alt(mods) || RightClick(mods)) {
		    GetCeilingLight(X, Y);
		} else {
		    undoSet = false;
		    SetCeilingLight(X, Y);
		}
	    } else if (Tool == Tool.MediaLight) {
		if (Alt(mods) || RightClick(mods)) {
		    GetMediaLight(X, Y);
		} else {
		    undoSet = false;
		    SetMediaLight(X, Y);
		}
	    } else if (Tool == Tool.Media) {
		if (Alt(mods) || RightClick(mods)) {
		    GetMedia(X, Y);
		} else {
		    undoSet = false;
		    SetMedia(X, Y);
		}
	    } else if (Tool == Tool.AmbientSound) {
		if (Alt(mods) || RightClick(mods)) {
		    GetAmbientSound(X, Y);
		} else {
		    undoSet = false;
		    SetAmbientSound(X, Y);
		}
	    } else if (Tool == Tool.RandomSound) {
		if (Alt(mods) || RightClick(mods)) {
		    GetRandomSound(X, Y);
		} else {
		    undoSet = false;
		    SetRandomSound(X, Y);
		}
	    } else if (Tool == Tool.FloorTexture) {
		if (Alt(mods) || RightClick(mods)) {
		    GetFloorTexture(X, Y);
		} else {
		    undoSet = false;
		    SetFloorTexture(X, Y);
		}
	    } else if (Tool == Tool.CeilingTexture) {
		if (Alt(mods) || RightClick(mods)) {
		    GetCeilingTexture(X, Y);
		} else {
		    undoSet = false;
		    SetCeilingTexture(X, Y);
		}
	    } else if (Tool == Tool.Select) {
		if (Alt(mods) || RightClick(mods)) {
		    AltSelect(X, Y);
		} else {
		    Select(X, Y);
		}
	    } else if (!RightClick(mods)) {
		Point p = new Point(X, Y);

		if (Tool == Tool.Line) {
		    StartLine(X, Y);
		} else if (Tool == Tool.Fill) {
		    undoSet = false;
		    Fill(X, Y);
		} else if (Tool == Tool.Object) {
		    if (SnapObjects) { p = GridAdjust(p); }
		    PlaceObject(p.X, p.Y);
		} else if (Tool == Tool.Annotation) {
		    PlaceAnnotation(X, Y);
		} else if (Tool == Tool.VisualMode) {
                    PlaceVisualMode(X, Y);
                }
	    }

	    downX = lastX = X;
	    downY = lastY = Y;
	}

	public void Motion(short X, short Y, EditorModifiers mods) {
	    if (Tool == Tool.Line) {
		bool constrainAngle = (mods & EditorModifiers.Shift) != 0;
		bool constrainLength = (mods & EditorModifiers.Alt) != 0;
		UpdateLine(X, Y, constrainAngle, constrainLength);
	    } else if (Tool == Tool.Fill) {
		Fill(X, Y);
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
	    } else if (Tool == Tool.PolygonType) {
		if (Alt(mods) || RightClick(mods)) {
		    GetPolygonType(X, Y);
		} else {
		    SetPolygonType(X, Y);
		}
	    } else if (Tool == Tool.FloorLight) {
		if (Alt(mods) || RightClick(mods)) {
		    GetFloorLight(X, Y);
		} else {
		    SetFloorLight(X, Y);
		}
	    } else if (Tool == Tool.CeilingLight) {
		if (Alt(mods) || RightClick(mods)) {
		    GetCeilingLight(X, Y);
		} else {
		    SetCeilingLight(X, Y);
		}
	    } else if (Tool == Tool.MediaLight) {
		if (Alt(mods) || RightClick(mods)) {
		    GetMediaLight(X, Y);
		} else {
		    SetMediaLight(X, Y);
		}
	    } else if (Tool == Tool.Media) {
		if (Alt(mods) || RightClick(mods)) {
		    GetMedia(X, Y);
		} else {
		    SetMedia(X, Y);
		}
	    } else if (Tool == Tool.AmbientSound) {
		if (Alt(mods) || RightClick(mods)) {
		    GetAmbientSound(X, Y);
		} else {
		    SetAmbientSound(X, Y);
		}
	    } else if (Tool == Tool.RandomSound) {
		if (Alt(mods) || RightClick(mods)) {
		    GetRandomSound(X, Y);
		} else {
		    SetRandomSound(X, Y);
		}
	    } else if (Tool == Tool.FloorTexture) {
		if (Alt(mods) || RightClick(mods)) {
		    GetFloorTexture(X, Y);
		} else {
		    SetFloorTexture(X, Y);
		}
	    } else if (Tool == Tool.CeilingTexture) {
		if (Alt(mods) || RightClick(mods)) {
		    GetCeilingTexture(X, Y);
		} else {
		    SetCeilingTexture(X, Y);
		}

	    }

	    lastX = X;
	    lastY = Y;
	}

	public void ButtonRelease(short X, short Y, EditorModifiers mods) {
	    if (Tool == Tool.Line) {
		bool constrainAngle = (mods & EditorModifiers.Shift) != 0;
		bool constrainLength = (mods & EditorModifiers.Alt) != 0;
		ConnectLine(X, Y, constrainAngle, constrainLength);
	    }
	}

	public void DeleteSelected() {
	    bool changed = true;
	    if (Selection.Point != -1) {
		// find the closest point to delete next

		// this is pretty ridiculous: endpoint indices can
		// change after the delete, and so can line indices;
		// so remember a line reference and index into that
		// line's EndpointIndexes list
		List<short> nextPointCandidates = new List<short>();
		foreach (short i in Level.FindEndpointLines(Selection.Point)) {
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
		    foreach (short i in Level.FindEndpointLines(point_index)) {
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
	    } else if (Selection.Polygon != -1) {
		SetUndo();
		Level.DeletePolygon(Selection.Polygon);
		Selection.Polygon = -1;
	    } else if (Selection.Annotation != -1) {
		SetUndo();
		Level.Annotations.RemoveAt(Selection.Annotation);
		Selection.Annotation = -1;
	    } else {
		changed = false;
	    }

	    if (changed) Changed = true;
	}

	public void SetUndo() {
	    undoState = Level.Save().Clone();
	    undoSelection = new Selection();
	    undoSelection.CopyFrom(Selection);
	}

	public void ClearUndo() {
	    undoState = null;
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
	    RedrawAll = false;
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

	// ensures 1:1 platform:polygon mapping
	public void ScanPlatforms() {
	    Dictionary<short, Platform> map = new Dictionary<short, Platform>();
	    for (int i = Level.Platforms.Count - 1; i >= 0; --i) {
		Platform platform = Level.Platforms[i];
		if (platform.PolygonIndex == -1) {
                    Level.DeletePlatform((short) i);
		} else {
		    Polygon polygon = Level.Polygons[platform.PolygonIndex];
		    if (polygon.Type != PolygonType.Platform) {
                        Level.DeletePlatform((short) i);
		    } else {
			map[platform.PolygonIndex] = platform;
		    }
		}
	    }

	    for (int i = 0; i < Level.Polygons.Count; ++i) {
		Polygon polygon = Level.Polygons[i];
		if (polygon.Type == PolygonType.Platform && !map.ContainsKey((short) i)) {
		    Platform platform = new Platform();
		    platform.SetTypeWithDefaults(PlatformType.SphtDoor);
		    platform.PolygonIndex = (short) i;
		    Level.Platforms.Add(platform);
		    polygon.Permutation = (short) (Level.Platforms.Count - 1);
		}
	    }
	}

	public void Recenter() {
	    SetUndo();
	    // find the extents
	    int minX = short.MaxValue;
	    int maxX = short.MinValue;
	    int minY = short.MaxValue;
	    int maxY = short.MinValue;

	    foreach (Point p in Level.Endpoints) {
		if (p.X < minX) minX = p.X;
		if (p.X > maxX) maxX = p.X;
		if (p.Y < minY) minY = p.Y;
		if (p.Y > maxY) maxY = p.Y;
	    }

	    foreach (Annotation a in Level.Annotations) {
		if (a.X < minX) minX = a.X;
		if (a.X > maxX) maxX = a.X;
		if (a.Y < minY) minY = a.Y;
		if (a.Y > maxY) maxY = a.Y;
	    }

	    // we assume nobody has map objects outside of polygon bounds!
	    
	    int marginX = (ushort.MaxValue - (maxX - minX)) / 2;
	    int marginY = (ushort.MaxValue - (maxY - minY)) / 2;

	    int newMinX = short.MinValue + marginX;
	    int newMinY = short.MinValue + marginY;

	    int offsetX = ((newMinX - minX) / World.One) * World.One;
	    int offsetY = ((newMinY - minY) / World.One) * World.One;

	    for (int i = 0; i < Level.Endpoints.Count; ++i) {
		TranslatePoint((short) i, offsetX, offsetY);
	    }

	    foreach (MapObject o in Level.Objects) {
		TranslateObject(o, offsetX, offsetY);
	    }
	    
	    foreach (Annotation a in Level.Annotations) {
		TranslateAnnotation(a, offsetX, offsetY);
	    }
	    
	    Changed = true;
	}

	public bool SetCustomGridOriginToSelected() {
	    if (Selection.Point != -1) {
		Point p = Level.Endpoints[Selection.Point];
		Grid.Center.X = p.X;
		Grid.Center.Y = p.Y;
		return true;
	    } else if (Selection.Line != -1) {
		Line l = Level.Lines[Selection.Line];
		Point p1 = Level.Endpoints[l.EndpointIndexes[0]];
		Point p2 = Level.Endpoints[l.EndpointIndexes[1]];
		
		if (Grid.Center.X == p1.X && Grid.Center.Y == p1.Y) {
		    Grid.Center.X = p2.X;
		    Grid.Center.Y = p2.Y;
		} else {
		    Grid.Center.X = p1.X;
		    Grid.Center.Y = p1.Y;
		}
		return true;
	    }
	    
	    return false;
	}

	public bool SetCustomGridRotationToSelected() {
	    if (Selection.Line != -1) {
		Line l = Level.Lines[Selection.Line];
		Point p1 = Level.Endpoints[l.EndpointIndexes[0]];
		Point p2 = Level.Endpoints[l.EndpointIndexes[1]];
		Grid.Rotation = -(180 / Math.PI) * Math.Atan2(p2.Y-p1.Y, p2.X-p1.X);
		while(Grid.Rotation < 0)
		    Grid.Rotation += 90;
		
		while(Grid.Rotation > 90)
		    Grid.Rotation -= 90;

		return true;
	    }

	    return false;
	}

	public bool SetCustomGridScaleToSelected() {
	    if (Selection.Line != -1) {
		Line l = Level.Lines[Selection.Line];
		Point p1 = Level.Endpoints[l.EndpointIndexes[0]];
		Point p2 = Level.Endpoints[l.EndpointIndexes[1]];
		double d = Math.Sqrt((double) (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y) ) / World.One;

		while (d > Grid.MaxScale) {
		    d /= 2.0;
		}
		while (d < Grid.MinScale) {
		    d *= 2.0;
		}

		Grid.Scale = d;
		return true;
	    }

	    return false;
	}

        public void SetObjectType(MapObject o, ObjectType type) {
            if (o.Type != type) {
                if (o.Type == ObjectType.Monster) {
                    if (Level.MonsterPlacement[o.Index].InitialCount > 0) {
                        --Level.MonsterPlacement[o.Index].InitialCount;
                    }
                } else if (o.Type == ObjectType.Item) {
                    if (Level.ItemPlacement[o.Index].InitialCount > 0) {
                        --Level.ItemPlacement[o.Index].InitialCount;
                    }
                }

                if (type == ObjectType.Monster) {
                    ++Level.MonsterPlacement[o.Index].InitialCount;
                } else if (type == ObjectType.Item) {
                    ++Level.ItemPlacement[o.Index].InitialCount;
                }
            }

            o.Type = type;
        }
    }
}

