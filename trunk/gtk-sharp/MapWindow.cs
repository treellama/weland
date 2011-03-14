using Gtk;
using Gdk;
using Glade;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace Weland {
    // resizable, scrollable map view
    public partial class MapWindow {
	public Level Level {
	    get { return drawingArea.Level; }
	    set { 
		drawingArea.Level = value;
		editor.Level = value;
	    }
	}

	Wadfile wadfile = new Wadfile();
	Editor editor = new Editor();
	Grid grid = new Grid();
	Selection selection = new Selection();

	SortedList<short, bool> paintIndexes = new SortedList<short, bool>();
	List<Gdk.Color> paintColors = new List<Gdk.Color>();

	[Widget] Gtk.Window window1;
	[Widget] MenuBar menubar1;
	[Widget] VScrollbar vscrollbar1;
	[Widget] HScrollbar hscrollbar1;
	[Widget] MenuItem levelItem;
	[Widget] Table table1;

	[Widget] MenuItem quitItem;
	[Widget] MenuItem quitSeparator;
	[Widget] MenuItem aboutItem;
	[Widget] MenuItem preferencesItem;

	[Widget] HScale viewFloorHeight;
	[Widget] HScale viewCeilingHeight;

	[Widget] RadioButton layer1;
	[Widget] RadioButton layer2;
	[Widget] RadioButton layer3;
	[Widget] RadioButton layer4;
	[Widget] RadioButton layer5;
	[Widget] RadioButton layer6;

	[Widget] MenuItem drawModeItem;
	[Widget] MenuItem floorHeightItem;
	[Widget] MenuItem ceilingHeightItem;
	[Widget] MenuItem floorTextureItem;
	[Widget] MenuItem ceilingTextureItem;
	[Widget] MenuItem polygonTypeItem;
	[Widget] MenuItem floorLightItem;
	[Widget] MenuItem ceilingLightItem;
	[Widget] MenuItem mediaLightItem;
	[Widget] MenuItem mediaItem;
	[Widget] MenuItem ambientSoundItem;
	[Widget] MenuItem randomSoundItem;

	[Widget] RadioToolButton selectButton;
	[Widget] RadioToolButton zoomButton;
	[Widget] RadioToolButton moveButton;
	[Widget] RadioToolButton lineButton;
	[Widget] RadioToolButton fillButton;
	[Widget] RadioToolButton objectButton;
	[Widget] RadioToolButton annotationButton;
	[Widget] RadioToolButton floorHeightButton;
	[Widget] RadioToolButton ceilingHeightButton;
	[Widget] RadioToolButton polygonTypeButton;
	[Widget] RadioToolButton floorLightButton;
	[Widget] RadioToolButton ceilingLightButton;
	[Widget] RadioToolButton mediaLightButton;
	[Widget] RadioToolButton mediaButton;
	[Widget] RadioToolButton ambientSoundButton;
	[Widget] RadioToolButton randomSoundButton; 
	[Widget] RadioToolButton floorTextureButton;
	[Widget] RadioToolButton ceilingTextureButton;

	[Widget] ToggleToolButton showGridButton;
	[Widget] ToggleToolButton snapToGridButton;
	[Widget] ToggleToolButton showMonstersButton;
	[Widget] ToggleToolButton showObjectsButton;
	[Widget] ToggleToolButton showSceneryButton;
	[Widget] ToggleToolButton showPlayersButton;
	[Widget] ToggleToolButton showGoalsButton;
	[Widget] ToggleToolButton showSoundsButton;

	[Widget] VBox palette;
	[Widget] VButtonBox paletteButtonbox;
	[Widget] Button paletteAddButton;
	[Widget] Button paletteEditButton;

	[Widget] VBox texturePalette;

	[Widget] Statusbar statusbar;

	string Filename;
	int layer = 1;

	void SetupDrawingArea() {
	    drawingArea.ConfigureEvent += OnConfigure;
	    drawingArea.MotionNotifyEvent += OnMotion;
	    drawingArea.ButtonPressEvent += OnButtonPressed;
	    drawingArea.ButtonReleaseEvent += OnButtonReleased;
	    drawingArea.Events = 
		EventMask.ExposureMask | 
		EventMask.ButtonPressMask | 
		EventMask.ButtonReleaseMask | 
		EventMask.ButtonMotionMask;

	    table1.Attach(drawingArea, 0, 1, 0, 1, AttachOptions.Shrink | AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Shrink | AttachOptions.Expand | AttachOptions.Fill, 0, 0);

	    drawingArea.Show();
	}

	void SetIconResource(ToolButton button, string resource) {
	    button.IconWidget = new Gtk.Image(null, resource);
	    button.IconWidget.Show();
	}

	bool HeightFilter(Polygon p) {
	    return (World.ToDouble(p.FloorHeight) >= viewFloorHeight.Value && World.ToDouble(p.FloorHeight) <= viewCeilingHeight.Value);
	}

	bool ObjectFilter(MapObject o) {
	    switch (o.Type) {
	    case ObjectType.Monster:
		return drawingArea.ShowMonsters;
	    case ObjectType.Scenery:
		return drawingArea.ShowScenery;
	    case ObjectType.Item:
		return drawingArea.ShowObjects;
	    case ObjectType.Player:
		return drawingArea.ShowPlayers;
	    case ObjectType.Goal:
		return drawingArea.ShowGoals;
	    case ObjectType.Sound:
		return drawingArea.ShowSounds;
	    }

	    return true;
	}

	void ResetViewHeight() {
	    viewFloorHeight.Value = -32;
	    viewCeilingHeight.Value = 32;
	    SaveLayer();
	}

	protected void OnViewHeightReset(object o, EventArgs args) {
	    ResetViewHeight();
	}
	
	protected void OnViewHeight(object o, EventArgs args) {
	    SaveLayer();
	    Redraw();
	}

	void DoMacIntegration() {
	    try {
		IgeMacIntegration.IgeMacMenu.MenuBar = menubar1;
		IgeMacIntegration.IgeMacMenu.QuitMenuItem = quitItem;

		IgeMacIntegration.IgeMacMenuGroup appMenuGroup = IgeMacIntegration.IgeMacMenu.AddAppMenuGroup();
		appMenuGroup.AddMenuItem(aboutItem, "About Weland");
		appMenuGroup.AddMenuItem(new SeparatorMenuItem(), "-");
		appMenuGroup.AddMenuItem(preferencesItem, "Preferences…");
		quitSeparator.Hide();

		menubar1.Hide();
	    } catch (DllNotFoundException) { }	    
	}

	public MapWindow(string title) {
	    Glade.XML gxml = new Glade.XML(null, "mapwindow.glade", "window1", null);
	    gxml.Autoconnect(this);
	    SetupDrawingArea();

	    SetIconResource(selectButton, "arrow.png");
	    SetIconResource(zoomButton, "zoom.png");
	    SetIconResource(moveButton, "move.png");
	    SetIconResource(lineButton, "line.png");
	    SetIconResource(fillButton, "fill.png");
	    SetIconResource(objectButton, "object.png");
	    SetIconResource(annotationButton, "annotation.png");
	    SetIconResource(floorHeightButton, "floor-height.png");
	    SetIconResource(ceilingHeightButton, "ceiling-height.png");
	    SetIconResource(polygonTypeButton, "polygon-type.png");
	    SetIconResource(floorLightButton, "floor-light.png");
	    SetIconResource(ceilingLightButton, "ceiling-light.png");
	    SetIconResource(mediaLightButton, "media-light.png");
	    SetIconResource(mediaButton, "liquids.png");
	    SetIconResource(ambientSoundButton, "ambient-sound.png");
	    SetIconResource(randomSoundButton, "random-sound.png");
	    SetIconResource(floorTextureButton, "floor-texture.png");
	    SetIconResource(ceilingTextureButton, "ceiling-texture.png");

	    SetIconResource(showGridButton, "grid.png");
	    SetIconResource(snapToGridButton, "snap.png");
	    SetIconResource(showMonstersButton, "monster.png");
	    SetIconResource(showObjectsButton, "pistol-ammo.png");
	    SetIconResource(showSceneryButton, "flower.png");
	    SetIconResource(showPlayersButton, "player.png");
	    SetIconResource(showGoalsButton, "flag.png");
	    SetIconResource(showSoundsButton, "sound.png");

	    double[] angles = { 0, 240, 120, 22, 300, 180, 60 };
	    for (int s = 0; s < 3; ++s) {
		for (int i = 0; i < angles.Length; ++i) {
		    for (int j = 0; j < 6; ++j) {
			paintColors.Add(HSV.ToRGB(angles[i], (double) (3 - s) / 3, ((double) (6 - j) * 0.9 / 6 + 0.1)));
		    }
		}
	    }

	    layer = 1;
	    LoadLayer();

	    editor.Grid = grid;
	    editor.Selection = selection;
	    editor.Scale = drawingArea.Transform.Scale;
	    Level.Filter = HeightFilter;
	    Level.ObjectFilter = ObjectFilter;
	    drawingArea.Grid = grid;
	    drawingArea.Selection = selection;
	    drawingArea.Filter = HeightFilter;

	    if (PlatformDetection.IsMac) {
		DoMacIntegration();
	    }

	    SetupInspector();

	    showGridButton.Active = Weland.Settings.GetSetting("Grid/Visible", true);
	    snapToGridButton.Active = Weland.Settings.GetSetting("Grid/Constrain", true);
	    grid.Resolution = (short) Weland.Settings.GetSetting("Grid/Resolution", World.One);
	    if (grid.Resolution == World.One * 2) {
		twoWUButton.Active = true;
	    } else if (grid.Resolution == World.One) {
		oneWUButton.Active = true;
	    } else if (grid.Resolution == World.One / 2) {
		halfWUButton.Active = true;
	    } else if (grid.Resolution == World.One / 4) {
		quarterWUButton.Active = true;
	    } else if (grid.Resolution == World.One / 8) {
		eighthWUButton.Active = true;
	    }

	    Level.FilterPoints = !Weland.Settings.GetSetting("MapWindow/ShowHiddenVertices", true);

	    // glade doesn't hook this up?
	    textureIcons.SelectionChanged += OnTextureSelected;

	    Weland.ShapesChanged += OnShapesChanged;

	    window1.AllowShrink = true;
	    int width = Weland.Settings.GetSetting("MapWindow/Width", 800);
	    int height = Weland.Settings.GetSetting("MapWindow/Height", 600);
	    window1.Resize(width, height);

	    int top = Weland.Settings.GetSetting("MapWindow/Top", 0);
	    int left = Weland.Settings.GetSetting("MapWindow/Left", 0);
	    window1.Move(left, top);

	    window1.Show();

	    window1.Focus = null;

	}

	void UpdateStatusBar() {
	    const int id = 1;
	    statusbar.Pop(id);

	    if (selection.Point != -1) {
		statusbar.Push(id, String.Format("Point Index: {0}\tLocation: ({1:0.000}, {2:0.000})", selection.Point, World.ToDouble(Level.Endpoints[selection.Point].X), World.ToDouble(Level.Endpoints[selection.Point].Y)));
	    } else if (selection.Line != -1) {
		Line line = Level.Lines[selection.Line];
		statusbar.Push(id, String.Format("Line Index: {0}\tLine Length: {1:0.000} WU", selection.Line, World.ToDouble(Level.Distance(Level.Endpoints[line.EndpointIndexes[0]], Level.Endpoints[line.EndpointIndexes[1]])))); 
	    } else if (selection.Polygon != -1) {
		Polygon polygon = Level.Polygons[selection.Polygon];
		statusbar.Push(id, String.Format("Polygon index {0}\tFloor Height: {1:0.000}, Ceiling Height: {2:0.000}", selection.Polygon, World.ToDouble(polygon.FloorHeight), World.ToDouble(polygon.CeilingHeight)));
	    } else if (selection.Object != -1) {
		MapObject obj = Level.Objects[selection.Object];
		statusbar.Push(id, String.Format("Object index: {0}\tLocation: ({1:0.000}, {2:0.000})", selection.Object, World.ToDouble(obj.X), World.ToDouble(obj.Y)));
	    } else if (selection.Annotation != -1) {
		Annotation note = Level.Annotations[selection.Annotation];
		statusbar.Push(id, String.Format("Annotation index: {0}\tPolygon: {1}\tLocation: ({2:0.000}, {3:0.000})", selection.Annotation, note.PolygonIndex, World.ToDouble(note.X), World.ToDouble(note.Y)));
	    } else if (Level.TemporaryLineStartIndex != -1) {
		Point p0 = Level.Endpoints[Level.TemporaryLineStartIndex];
		Point p1 = Level.TemporaryLineEnd;
		int X = p1.X - p0.X;
		int Y = p1.Y - p0.Y;
		double r = Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
		double theta = Math.Atan2(-Y, X);
		if (theta < 0) { 
		    theta += 2 * Math.PI;
		}
		statusbar.Push(id, String.Format("Line Length: {0:0.000} WU\tAngle: {1:0.0}Â°", World.ToDouble((int) Math.Round(r)), theta * 180 / Math.PI));
	    } else {
		statusbar.Push(id, String.Format("Level: {0}\t{1} Polygons, {2} Lights, {3} Objects", Level.Name, Level.Polygons.Count, Level.Lights.Count, Level.Objects.Count));
	    }
	}

	MapDrawingArea drawingArea = new MapDrawingArea();

	public void Center(short X, short Y) {
	    drawingArea.Center(X, Y);
	    vscrollbar1.Value = drawingArea.Transform.YOffset;
	    hscrollbar1.Value = drawingArea.Transform.XOffset;
	}
	
	void AdjustScrollRange() {
	    int width, height;
	    width = drawingArea.Allocation.Width;
	    height = drawingArea.Allocation.Height;
	    double scale = drawingArea.Transform.Scale;
	    double VUpper = short.MaxValue - height / scale;
	    if (VUpper < short.MinValue) 
		VUpper = short.MinValue;
			
	    vscrollbar1.Adjustment.Lower = short.MinValue;
	    vscrollbar1.Adjustment.Upper = VUpper;
	    if (drawingArea.Transform.YOffset > VUpper) {
		vscrollbar1.Value = VUpper;
		drawingArea.Transform.YOffset = (short) VUpper;
	    }

	    vscrollbar1.Adjustment.StepIncrement = 32.0 / scale;
	    vscrollbar1.Adjustment.PageIncrement = 256.0 / scale;

	    hscrollbar1.Adjustment.Lower = short.MinValue;
	    double HUpper = short.MaxValue - width / scale;
	    if (HUpper < short.MinValue) 
		HUpper = short.MinValue;
	    hscrollbar1.Adjustment.Upper = HUpper;
	    if (drawingArea.Transform.XOffset > HUpper) {
		hscrollbar1.Value = HUpper;
		drawingArea.Transform.XOffset = (short) HUpper;
	    }

	    hscrollbar1.Adjustment.StepIncrement = 32.0 / scale;
	    hscrollbar1.Adjustment.PageIncrement = 256.0 / scale;

	    Redraw();
	}

	internal void OnHValueChanged(object obj, EventArgs e) {
	    drawingArea.Transform.XOffset = (short) hscrollbar1.Value;
	    Redraw();
	}

	internal void OnVValueChanged(object obj, EventArgs e) {
	    drawingArea.Transform.YOffset = (short) vscrollbar1.Value;
	    Redraw();
	}

	internal void OnConfigure(object obj, ConfigureEventArgs args) {
	    AdjustScrollRange();
	    args.RetVal = true;
	}

	[GLib.ConnectBefore()] protected void OnWindowConfigure(object obj, ConfigureEventArgs args) {
	    int X, Y;
	    window1.GetPosition(out X, out Y);
	    Weland.Settings.PutSetting("MapWindow/Width", args.Event.Width);
	    Weland.Settings.PutSetting("MapWindow/Height", args.Event.Height);
	    Weland.Settings.PutSetting("MapWindow/Top", Y);
	    Weland.Settings.PutSetting("MapWindow/Left", X);
	}

	void Redraw() {
	    drawingArea.QueueDrawArea(0, 0, drawingArea.Allocation.Width, drawingArea.Allocation.Height);
	    editor.ClearDirty();
	}

	void RedrawDirty() {
	    const int dirtySlop = 12;
	    if (editor.Dirty) {
		if (editor.RedrawAll) {
		    Redraw();
		} else {
		    int X1 = (int) drawingArea.Transform.ToScreenX(editor.RedrawLeft) - dirtySlop;
		    int Y1 = (int) drawingArea.Transform.ToScreenY(editor.RedrawTop) - dirtySlop;
		    int X2 = (int) drawingArea.Transform.ToScreenX(editor.RedrawRight) + dirtySlop;
		    int Y2 = (int) drawingArea.Transform.ToScreenY(editor.RedrawBottom) + dirtySlop;
		    drawingArea.QueueDrawArea(X1, Y1, X2 -X1, Y2 - Y1);
		    editor.ClearDirty();
		}
	    }
	}
	
	enum ScrollWheelBehavior {
	    Zoom,
	    Pan
	}

	internal void OnScroll(object obj, ScrollEventArgs args) {
	    ScrollWheelBehavior behavior = (ScrollWheelBehavior) Weland.Settings.GetSetting("ScrollWheelBehavior", (int) ScrollWheelBehavior.Zoom);
	    if (behavior == ScrollWheelBehavior.Zoom) {
		short X = drawingArea.Transform.ToMapX(args.Event.X);
		short Y = drawingArea.Transform.ToMapY(args.Event.Y);

		if (args.Event.Direction == ScrollDirection.Down) {
		    ZoomOut(X, Y);
		} else if (args.Event.Direction == ScrollDirection.Up) {
		    ZoomIn(X, Y);
		}
	    } else if (behavior == ScrollWheelBehavior.Pan) {
		if (args.Event.Direction == ScrollDirection.Down) {
		    vscrollbar1.Value += 32.0 / drawingArea.Transform.Scale;
		} else if (args.Event.Direction == ScrollDirection.Up) {
		    vscrollbar1.Value -= 32.0 / drawingArea.Transform.Scale;
		} else if (args.Event.Direction == ScrollDirection.Right) {
		    hscrollbar1.Value += 32.0 / drawingArea.Transform.Scale;
		} else if (args.Event.Direction == ScrollDirection.Left) {
		    hscrollbar1.Value -= 32.0 / drawingArea.Transform.Scale;
		}
	    }
	    args.RetVal = true;
	}

	double xDown;
	double yDown;
	short xOffsetDown;
	short yOffsetDown;

	static EditorModifiers Modifiers(Gdk.ModifierType type) {
	    EditorModifiers modifiers = EditorModifiers.None;
	    if ((type & ModifierType.ShiftMask) != 0) {
		modifiers |= EditorModifiers.Shift;
	    }

	    if ((type & ModifierType.ControlMask) != 0) {
		modifiers |= EditorModifiers.Control;
	    }
	    
	    if (PlatformDetection.IsMac) {
		if (optionKey) {
		    modifiers |= EditorModifiers.Alt;
		}
	    } else if ((type & ModifierType.Mod1Mask) != 0) {
		modifiers |= EditorModifiers.Alt;
	    }

	    return modifiers;
	}

	void ZoomAt(short Xt, short Yt, double factor) {
	    double X = drawingArea.Transform.ToScreenX(Xt);
	    double Y = drawingArea.Transform.ToScreenY(Yt);
	    double originalScale = drawingArea.Transform.Scale;
	    double scale = originalScale * factor;
	    drawingArea.Transform.Scale = scale;
	    editor.Scale = scale;
	    int xNew = (int) Math.Round(X / originalScale - X / scale + drawingArea.Transform.XOffset);
	    int yNew = (int) Math.Round(Y / originalScale - Y / scale + drawingArea.Transform.YOffset);
	    vscrollbar1.Value = yNew;
	    hscrollbar1.Value = xNew;
	    AdjustScrollRange();
	}

	void ZoomIn(short X, short Y) {
	    ZoomAt(X, Y, Math.Pow(2.0, 1.0 / 4));
	}

	void ZoomOut(short X, short Y) {
	    ZoomAt(X, Y, 1 / Math.Pow(2.0, 1.0 / 4));
	}

	public void OnZoomIn(object obj, EventArgs args) {
	    short X = drawingArea.Transform.ToMapX(drawingArea.Allocation.Width / 2);
	    short Y = drawingArea.Transform.ToMapY(drawingArea.Allocation.Height / 2);
	    ZoomIn(X, Y);
	}

	public void OnZoomOut(object obj, EventArgs args) {
	    short X = drawingArea.Transform.ToMapX(drawingArea.Allocation.Width / 2);
	    short Y = drawingArea.Transform.ToMapY(drawingArea.Allocation.Height/ 2);
	    ZoomOut(X, Y);
	}

	void EditAnnotation() {
	    Annotation annotation = Level.Annotations[selection.Annotation];
	    EntryDialog d = new EntryDialog("Annotation Text", window1);
	    d.Text = annotation.Text;
	    if (d.Run() == (int) ResponseType.Ok) {
		annotation.Text = d.Text;
		editor.Changed = true;
		Redraw();
	    }
	    d.Destroy();	    
	    editor.EditAnnotation = false;
	}
	
	internal void OnButtonPressed(object obj, ButtonPressEventArgs args) {
	    EventButton ev = args.Event;
	    short X = drawingArea.Transform.ToMapX(ev.X);
	    short Y = drawingArea.Transform.ToMapY(ev.Y);

	    if (ev.Type == EventType.ButtonPress) {
		if (ev.Button == 2 || editor.Tool == Tool.Move) {
		    xDown = ev.X;
		    yDown = ev.Y;
		    xOffsetDown = drawingArea.Transform.XOffset;
		    yOffsetDown = drawingArea.Transform.YOffset;
		} else if (editor.Tool == Tool.Zoom) {
		    if (ev.Button == 3 || (ev.State & ModifierType.Mod1Mask) != 0) {
			ZoomOut(X, Y);
		    } else {
			ZoomIn(X, Y);
		    }
		} else {
		    EditorModifiers modifiers = Modifiers(ev.State);
		    if (ev.Button == 3) {
			modifiers |= EditorModifiers.RightClick;
		    }
		    if (ev.Button == 1 || ev.Button == 3) {
			editor.ButtonPress(X, Y, modifiers);
			Redraw();
		    }
		}
	    } else if (ev.Type == EventType.TwoButtonPress) {
		if (selection.Line != -1) {
		    LineParametersDialog d = new LineParametersDialog(window1, Level, Level.Lines[selection.Line]);
		    d.Run();
		    editor.Changed = true;
		    Redraw();
		} else if (selection.Point != -1) {
		    PointParametersDialog d = new PointParametersDialog(window1, Level, selection.Point);
		    d.Run();
		    editor.Changed = true;
		    Redraw();
		} else if (selection.Annotation != -1) {
		    EditAnnotation();
		}
	    }

	    args.RetVal = true;
	}

	internal void OnButtonReleased(object obj, ButtonReleaseEventArgs args) {
	    if (args.Event.Button == 1) {
		EditorModifiers modifiers = Modifiers(args.Event.State);
		editor.ButtonRelease(drawingArea.Transform.ToMapX(args.Event.X), drawingArea.Transform.ToMapY(args.Event.Y), modifiers);
		Redraw();
	    }

	    if (editor.Tool == Tool.FloorHeight || editor.Tool == Tool.CeilingHeight || editor.Tool == Tool.PolygonType || editor.Tool == Tool.FloorLight || editor.Tool == Tool.CeilingLight || editor.Tool == Tool.MediaLight || editor.Tool == Tool.Media || editor.Tool == Tool.AmbientSound || editor.Tool == Tool.RandomSound) {
		// update the paint mode button
		int i = paintIndexes.IndexOfKey(editor.PaintIndex);
		if (i != -1) {
		    ColorRadioButton crb = (ColorRadioButton) paletteButtonbox.Children[i];
		    if (!crb.Active) {
			crb.Active = true;
		    }
		}
	    } else if (editor.Tool == Tool.Annotation && editor.EditAnnotation) {
		EditAnnotation();
	    } else if (editor.Tool == Tool.FloorTexture || editor.Tool == Tool.CeilingTexture) {
		UpdateTexturePalette(false);
	    }

	    UpdateInspector();
	    UpdateStatusBar();

	    args.RetVal = true;
	}

	internal void OnMotion(object obj, MotionNotifyEventArgs args) {
	    if (editor.Tool == Tool.Move || ((args.Event.State & ModifierType.Button2Mask) != 0)) {
		hscrollbar1.Value = xOffsetDown + (xDown - args.Event.X) / drawingArea.Transform.Scale;
		vscrollbar1.Value = yOffsetDown + (yDown - args.Event.Y) / drawingArea.Transform.Scale;
		args.RetVal = true;
	    } else if ((args.Event.State & ModifierType.Button1Mask) != 0) {
		editor.Motion(drawingArea.Transform.ToMapX(args.Event.X), drawingArea.Transform.ToMapY(args.Event.Y), Modifiers(args.Event.State));
		RedrawDirty();
	    }

	    if (editor.Tool == Tool.Line) {
		UpdateStatusBar();
	    }
	}

	Tool oldTool = Tool.Move;

	static bool optionKey = false;

	bool HandleMacAccelerator(Gdk.Key key) {
	    switch (key) {
	    case Gdk.Key.z:
		OnUndo(null, null);
		break;
	    case Gdk.Key.q:
		OnQuit(null, null);
		break;
	    case Gdk.Key.o:
		OnOpen(null, null);
		break;
	    case Gdk.Key.n:
		OnNew(null, null);
		break;
	    case Gdk.Key.s:
		OnSave(null, null);
		break;
	    case Gdk.Key.d:
		drawModeItem.Activate();
		break;
	    case Gdk.Key.equal:
		OnZoomIn(null, null);
		break;
	    case Gdk.Key.minus:
		OnZoomOut(null, null);
		break;
	    case Gdk.Key.g:
		OnGoto(null, null);
		break;
	    case Gdk.Key.m:
		OnLevelParameters(null, null);
		break;
	    case Gdk.Key.i:
		OnItemParameters(null, null);
		break;
	    default:
		return false;
	    }
	    return true;
	}

	[GLib.ConnectBefore()] internal void OnSpecialKeyPressed(object obj, KeyPressEventArgs args) {
	    args.RetVal = true;
	    if (PlatformDetection.IsMac) {
		if (args.Event.Key == Gdk.Key.Alt_L || args.Event.Key == Gdk.Key.Alt_R) {
		    optionKey = true;
		    return;
		} else if ((args.Event.State & ModifierType.MetaMask) != 0 || (args.Event.State & ModifierType.Mod1Mask) != 0) {
		    if (HandleMacAccelerator(args.Event.Key)) {
			return;
		    }
		}
	    }

	    switch (args.Event.Key) {
	    case Gdk.Key.space:
		if (editor.Tool != Tool.Move) {
		    oldTool = editor.Tool;
		    editor.Tool = Tool.Move;
		    drawingArea.GdkWindow.Cursor = new Cursor(CursorType.Fleur);		    
		}
		break;
	    case Gdk.Key.Up:
		if (palette.Visible) {
		    PalettePrev();
		} else {
		    editor.NudgeSelected(0, (short) ((double) -1 / drawingArea.Transform.Scale));
		    UpdateStatusBar();
		}
		Redraw();
		break;
	    case Gdk.Key.Down:
		if (palette.Visible) {
		    PaletteNext();
		} else {
		    editor.NudgeSelected(0, (short) ((double) 1 / drawingArea.Transform.Scale));
		    UpdateStatusBar();
		}
		Redraw();
		break;
	    case Gdk.Key.Left:
		editor.NudgeSelected((short) ((double) -1 / drawingArea.Transform.Scale), 0);
		UpdateStatusBar();
		Redraw();
		break;
	    case Gdk.Key.Right:
		editor.NudgeSelected((short) ((double) 1 / drawingArea.Transform.Scale), 0);
		UpdateStatusBar();
		Redraw();
		break;
	    default:
		args.RetVal = false;
		break;
	    }
	}

	internal void OnKeyPressed(object obj, KeyPressEventArgs args) {
	    bool caught = true;
	    switch (args.Event.Key) {
	    case Gdk.Key.F1:
		layer1.Active = true;
		break;
	    case Gdk.Key.F2:
		layer2.Active = true;
		break;
	    case Gdk.Key.F3:
		layer3.Active = true;
		break;
	    case Gdk.Key.F4:
		layer4.Active = true;
		break;
	    case Gdk.Key.F5:
		layer5.Active = true;
		break;
	    case Gdk.Key.F6:
		layer6.Active = true;
		break;
	    case Gdk.Key.a:
	    case Gdk.Key.A:
		selectButton.Active = true;
		break;
	    case Gdk.Key.z:
	    case Gdk.Key.Z:
		zoomButton.Active = true;
		break;
	    case Gdk.Key.l:
	    case Gdk.Key.L:
		lineButton.Active = true;
		break;
	    case Gdk.Key.f:
	    case Gdk.Key.F:
		fillButton.Active = true;
		break;
	    case Gdk.Key.o:
	    case Gdk.Key.O:
		objectButton.Active = true;
		break;
	    case Gdk.Key.d:
	    case Gdk.Key.D:
		moveButton.Active = true;
		break;
	    case Gdk.Key.t:
		annotationButton.Active = true;
		break;
	    case Gdk.Key.Key_5:
		twoWUButton.Active = true;
		break;
	    case Gdk.Key.Key_4:
		oneWUButton.Active = true;
		break;
	    case Gdk.Key.Key_3:
		halfWUButton.Active = true;
		break;
	    case Gdk.Key.Key_2:
		quarterWUButton.Active = true;
		break;
	    case Gdk.Key.Key_1:
		eighthWUButton.Active = true;
		break;
	    case Gdk.Key.numbersign:
		showGridButton.Active = !showGridButton.Active;
		break;
	    case Gdk.Key.percent:
		snapToGridButton.Active = !snapToGridButton.Active;
		break;
	    case Gdk.Key.Delete:
	    case Gdk.Key.BackSpace:
		editor.DeleteSelected();
		UpdateInspector();
		UpdateStatusBar();
		Redraw();
		break;
	    default:
		caught = false;
		break;
	    }
	    args.RetVal = caught;
	}

	[GLib.ConnectBefore()] internal void OnSpecialKeyReleased(object obj, KeyReleaseEventArgs args) {
	    if (args.Event.Key == Gdk.Key.space) {
		ChooseTool(oldTool);
		oldTool = Tool.Move;
		args.RetVal = true;
	    } else if (PlatformDetection.IsMac && (args.Event.Key == Gdk.Key.Alt_L || args.Event.Key == Gdk.Key.Alt_R)) {
		optionKey = false;
		args.RetVal = true;
		return;
	    }
	}

	public bool CheckSave() {
	    if (editor.Changed) {
		MessageDialog dialog = new MessageDialog(window1, DialogFlags.DestroyWithParent, MessageType.Warning, ButtonsType.None, "Do you wish to save changes?");
		dialog.AddButton(Stock.Discard, ResponseType.No);
		dialog.AddButton(Stock.Cancel, ResponseType.Cancel);
		dialog.Default = dialog.AddButton(Stock.Save, ResponseType.Yes);
		int response = dialog.Run();
		dialog.Destroy();
		if (response == (int) ResponseType.Yes) {
		    return Save();
		} else if (response == (int) ResponseType.No) {
		    return true;
		} else {
		    return false;
		}
	    } else {
		return true;
	    }
	}

	void BuildLevelMenu() {
	    Menu menu = new Menu();
	    foreach (var kvp in wadfile.Directory) {
		if (kvp.Value.Chunks.ContainsKey(MapInfo.Tag)) {
		    MenuItem item = new MenuItem(kvp.Value.LevelName);
		    int levelNumber = kvp.Key;
		    item.Activated += delegate(object obj, EventArgs args) { SelectLevel(levelNumber); };
		    menu.Append(item);
		}
	    }
	    if (menu.Children.Length == 0) {
		MenuItem item = new MenuItem(Level.Name);
		item.Sensitive = false;
		menu.Append(item);
	    }
	    menu.ShowAll();
	    levelItem.Submenu = menu;
	}

	public void SelectLevel(int n) {
	    if (CheckSave()) {
		Level = new Level();
		Level.Load(wadfile.Directory[n]);
		selection.Clear();
		editor.ClearUndo();
		Center(0, 0);
		AdjustScrollRange();
		layer1.Active = true;
		ResetViewHeight();
		window1.Title = wadfile.Directory[n].LevelName;
		ChooseTool(editor.Tool);
		editor.Changed = false;
	    }
	}

	public void OpenFile(string filename) {
	    try {
		Wadfile w = new Wadfile();
		w.Load(filename);
		wadfile = w;
		if (wadfile.Directory.Count > 1) {
		    Filename =  "";
		} else {
		    Filename = filename;
		}
		BuildLevelMenu();
		editor.Changed = false;
		SelectLevel(0);
	    }
	    catch (Wadfile.BadMapException e) {
		MessageDialog dialog = new MessageDialog(window1, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, e.Message);
		dialog.Run();
		dialog.Destroy();
	    }
	}

	internal void OnOpen(object obj, EventArgs args) {
	    if (CheckSave()) {
		FileChooserDialog d = new FileChooserDialog("Choose the file to open", window1, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
		d.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
		d.SetCurrentFolder(Weland.Settings.GetSetting("LastOpen/Folder", d.CurrentFolder));
		FileFilter filter = new FileFilter();
		filter.Name = "Marathon and Aleph One Maps";
		filter.AddPattern("*.sceA");
		filter.AddPattern("*.sce2");
		d.AddFilter(filter);
		filter = new FileFilter();
		filter.Name = "All Files";
		filter.AddPattern("*");
		d.AddFilter(filter);
		if (d.Run() == (int) ResponseType.Accept) {
		    Weland.Settings.PutSetting("LastOpen/Folder", Path.GetDirectoryName(d.Filename));
		    OpenFile(d.Filename);
		}
		d.Destroy();
	    }
	}

	public void NewLevel() {
	    wadfile = new Wadfile();
	    Level = new Level();
	    levelItem.Submenu = null;
	    Center(0, 0);
	    AdjustScrollRange();
	    layer1.Active = true;
	    ResetViewHeight();
	    selection.Clear();
	    editor.ClearUndo();
	    BuildLevelMenu();
	    window1.Title = "Untitled Level";
	    Filename = "";
	    ChooseTool(editor.Tool);
	    editor.Changed = false;
	}

	internal void OnNew(object obj, EventArgs args) {
	    if (CheckSave()) {
		NewLevel();
	    }
	}
	
	bool SaveAs() {
	    bool saved = false;
	    string message;
	    if (wadfile.Directory.Count > 1) {
		message = "Export level";
	    } else {
		message = "Save level as";
	    }
	    FileChooserDialog d = new FileChooserDialog(message, window1, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Save", ResponseType.Accept);
	    d.SetCurrentFolder(Weland.Settings.GetSetting("LastSave/Folder", Environment.GetFolderPath(Environment.SpecialFolder.Personal)));
	    d.CurrentName = Level.Name + ".sceA";
	    d.DoOverwriteConfirmation = true;
	    try {
		if (d.Run() == (int) ResponseType.Accept) {
		    wadfile = new Wadfile();
		    Level.AssurePlayerStart();
		    Redraw();
		    wadfile.Directory[0] = Level.Save();
		    wadfile.Save(d.Filename);
		    Filename = d.Filename;
		    
		    Weland.Settings.PutSetting("LastSave/Folder", Path.GetDirectoryName(d.Filename));
		    
		    editor.Changed = false;
		    saved = true;
		}
	    } catch (Exception e) {
		MessageDialog m = new MessageDialog(window1, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, "An error occurred while saving.");
		m.Title = "Save Error";
		m.SecondaryText = e.Message;
		m.Run();
		m.Destroy();
	    }
	    d.Destroy();

	    return saved;
	}

	internal void OnSaveAs(object obj, EventArgs args) {
	    SaveAs();
	}

	bool Save() {
	    if (Filename == "") {
		return SaveAs();
	    } else {
		bool success = false;
		try {
		    Level.AssurePlayerStart();
		    Redraw();
		    wadfile.Directory[0] = Level.Save();
		    wadfile.Save(Filename);
		    editor.Changed = false;
		    success = true;
		} catch (Exception e) {
		    MessageDialog m = new MessageDialog(window1, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, "An error occurred while saving.");
		    m.Title = "Save Error";
		    m.SecondaryText = e.Message;
		    m.Run();
		    m.Destroy();		    
		}
		return success;
	    }
	}
	
	internal void OnExportOBJ(object obj, EventArgs args) {
	    FileChooserDialog d = new FileChooserDialog("Export OBJ as", window1, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Save", ResponseType.Accept);
	    d.SetCurrentFolder(Weland.Settings.GetSetting("LastSave/Folder", Environment.GetFolderPath(Environment.SpecialFolder.Personal)));
	    d.CurrentName = Level.Name + ".obj";
	    d.DoOverwriteConfirmation = true;
	    try {
		if (d.Run() == (int) ResponseType.Accept) {
		    OBJExporter exporter = new OBJExporter(Level);
		    exporter.Export(d.Filename);
		    
		    Weland.Settings.PutSetting("LastSave/FoldeR", Path.GetDirectoryName(d.Filename));
		}
	    } catch (Exception e) {
		MessageDialog m = new MessageDialog(window1, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, "An error occured while exporting.");
		m.Title = "Export error";
		m.SecondaryText = e.Message;
		m.Run();
		m.Destroy();
	    }
	    d.Destroy();
	}

	internal void OnSave(object obj, EventArgs args) {
	    Save();
	}

	internal void OnChooseTool(object obj, EventArgs args) {
	    ToolButton button = (ToolButton) obj;
	    if (button == selectButton) {
		ChooseTool(Tool.Select);
	    } else if (button == zoomButton) {
		ChooseTool(Tool.Zoom);
	    } else if (button == moveButton) {
		ChooseTool(Tool.Move);
	    } else if (button == lineButton) {
		ChooseTool(Tool.Line);
	    } else if (button == fillButton) {
		ChooseTool(Tool.Fill);
	    } else if (button == objectButton) {
		ChooseTool(Tool.Object);
	    } else if (button == annotationButton) {
		ChooseTool(Tool.Annotation);
	    } else if (button == floorHeightButton) {
		ChooseTool(Tool.FloorHeight);
	    } else if (button == ceilingHeightButton) {
		ChooseTool(Tool.CeilingHeight);
	    } else if (button == polygonTypeButton) {
		ChooseTool(Tool.PolygonType);
	    } else if (button == floorLightButton) {
		ChooseTool(Tool.FloorLight);
	    } else if (button == ceilingLightButton) {
		ChooseTool(Tool.CeilingLight);
	    } else if (button == mediaLightButton) {
		ChooseTool(Tool.MediaLight);
	    } else if (button == mediaButton) {
		ChooseTool(Tool.Media);
	    } else if (button == ambientSoundButton) {
		ChooseTool(Tool.AmbientSound);
	    } else if (button == randomSoundButton) {
		ChooseTool(Tool.RandomSound);
	    } else if (button == floorTextureButton) {
		ChooseTool(Tool.FloorTexture);
	    } else if (button == ceilingTextureButton) {
		ChooseTool(Tool.CeilingTexture);
	    }
	}

	protected void OnViewMenu(object obj, EventArgs args) {
	    MenuItem item = (MenuItem) obj;
	    if (item == drawModeItem) {
		selectButton.Active = true;
	    } else if (item == floorHeightItem) {
		floorHeightButton.Active = true;
	    } else if (item == ceilingHeightItem) {
		ceilingHeightButton.Active = true;
	    } else if (item == floorTextureItem) {
		floorTextureButton.Active = true;
	    } else if (item == ceilingTextureItem) {
		ceilingTextureButton.Active = true;
	    } else if (item == polygonTypeItem) {
		polygonTypeButton.Active = true;
	    } else if (item == floorLightItem) {
		floorLightButton.Active = true;
	    } else if (item == ceilingLightItem) {
		ceilingLightButton.Active = true;
	    } else if (item == mediaLightItem) {
		mediaLightButton.Active = true;
	    } else if (item == mediaItem) {
		mediaButton.Active = true;
	    } else if (item == ambientSoundItem) {
		ambientSoundButton.Active = true;
	    } else if (item == randomSoundItem) {
		randomSoundButton.Active = true;
	    }
	}

	void OnChangePaintIndex(object obj, EventArgs args) {
	    ColorRadioButton crb = (ColorRadioButton) obj;
	    if (crb.Active) {
		editor.PaintIndex = paintIndexes.Keys[crb.Index];
	    }
	}

	void ClearPalette() {
	    while (paletteButtonbox.Children.Length > 0) {
		paletteButtonbox.Remove(paletteButtonbox.Children[0]);
	    }
	}

	Drawer.Color GdkToDrawer(Gdk.Color c) {
	    return new Drawer.Color((double) c.Red / ushort.MaxValue, (double) c.Green / ushort.MaxValue, (double) c.Blue / ushort.MaxValue);
	}

	void BuildHeightPalette(SortedList<short, bool> heights) {
	    ClearPalette();
	    drawingArea.PaintColors = new Dictionary<short, Drawer.Color>();
	    
	    paintIndexes = heights;
	    if (paintIndexes.Keys.Count > 0) {
		editor.PaintIndex = paintIndexes.Keys[0];
	    }
	    ColorRadioButton b = null;
	    for (int i = 0; i < paintIndexes.Keys.Count; ++i) {
		Gdk.Color c = paintColors[i % paintColors.Count];
		b = new ColorRadioButton(b, String.Format("{0:0.000}", World.ToDouble(paintIndexes.Keys[i])), c);
		b.Index = i;
		b.Toggled += OnChangePaintIndex;
		b.DoubleClicked += OnPaletteEdit;
		paletteButtonbox.Add(b);
		
		drawingArea.PaintColors[paintIndexes.Keys[i]] = GdkToDrawer(c);
	    }

	    paletteButtonbox.ShowAll();
	    
	    paletteAddButton.Sensitive = true;
	    paletteEditButton.Sensitive = (paintIndexes.Keys.Count > 0);
	}

	void BuildMediaPalette() {
	    ClearPalette();
	    drawingArea.PaintColors = new Dictionary<short, Drawer.Color>();
	    paintIndexes.Clear();
	    for (int i = -1; i < Level.Medias.Count; ++i) {
		paintIndexes[(short) i] = true;
	    }
	    editor.PaintIndex = -1;

	    // set up "none"
	    ColorRadioButton b = new ColorRadioButton(null, "None", new Gdk.Color(127, 127, 127));
	    b.Index = 0;
	    b.Toggled += OnChangePaintIndex;
	    drawingArea.PaintColors[-1] = new Drawer.Color(0.5, 0.5, 0.5);
	    paletteButtonbox.Add(b);

	    for (int i = 0; i < paintIndexes.Keys.Count - 1; ++i) {
		Gdk.Color c = paintColors[i % paintColors.Count];
		b = new ColorRadioButton(b, String.Format("{0}", i), c);
		b.Index = i + 1;
		b.Toggled += OnChangePaintIndex;
		b.DoubleClicked += OnPaletteEdit;
		paletteButtonbox.Add(b);

		drawingArea.PaintColors[(short) i] = GdkToDrawer(c);
	    }

	    paletteButtonbox.ShowAll();
	    paletteAddButton.Sensitive = true;
	    paletteEditButton.Sensitive = true;
	}

	void BuildAmbientSoundPalette() {
	    ClearPalette();
	    drawingArea.PaintColors = new Dictionary<short, Drawer.Color>();
	    paintIndexes.Clear();
	    for (int i = -1; i < Level.AmbientSounds.Count; ++i) {
		paintIndexes[(short) i] = true;
	    }
	    editor.PaintIndex = -1;

	    ColorRadioButton b = new ColorRadioButton(null, "None", new Gdk.Color(127, 127, 127));
	    b.Index = 0;
	    b.Toggled += OnChangePaintIndex;
	    drawingArea.PaintColors[-1] = new Drawer.Color(0.5, 0.5, 0.5);
	    paletteButtonbox.Add(b);
	    
	    for (int i = 0; i < paintIndexes.Keys.Count - 1; ++i) {
		Gdk.Color c = paintColors[i % paintColors.Count];
		b = new ColorRadioButton(b, String.Format("{0}", i), c);
		b.Index = i + 1;
		b.Toggled += OnChangePaintIndex;
		b.DoubleClicked += OnPaletteEdit;
		paletteButtonbox.Add(b);

		drawingArea.PaintColors[(short) i] = GdkToDrawer(c);
	    }

	    paletteButtonbox.ShowAll();
	    paletteAddButton.Sensitive = true;
	    paletteEditButton.Sensitive = true;
	}

	void BuildRandomSoundPalette() {
	    ClearPalette();
	    drawingArea.PaintColors = new Dictionary<short, Drawer.Color>();
	    paintIndexes.Clear();
	    for (int i = -1; i < Level.RandomSounds.Count; ++i) {
		paintIndexes[(short) i] = true;
	    }
	    editor.PaintIndex = -1;

	    ColorRadioButton b = new ColorRadioButton(null, "None", new Gdk.Color(127, 127, 127));
	    b.Index = 0;
	    b.Toggled += OnChangePaintIndex;
	    drawingArea.PaintColors[-1] = new Drawer.Color(0.5, 0.5, 0.5);
	    paletteButtonbox.Add(b);
	    
	    for (int i = 0; i < paintIndexes.Keys.Count - 1; ++i) {
		Gdk.Color c = paintColors[i % paintColors.Count];
		b = new ColorRadioButton(b, String.Format("{0}", i), c);
		b.Index = i + 1;
		b.Toggled += OnChangePaintIndex;
		b.DoubleClicked += OnPaletteEdit;
		paletteButtonbox.Add(b);

		drawingArea.PaintColors[(short) i] = GdkToDrawer(c);
	    }

	    paletteButtonbox.ShowAll();
	    paletteAddButton.Sensitive = true;
	    paletteEditButton.Sensitive = true;
	}
	
	void ChooseTool(Tool tool) {
	    editor.Tool = tool;
	    if (tool == Tool.Zoom) {
		drawingArea.GdkWindow.Cursor = new Cursor(CursorType.Target);
	    } else if (tool == Tool.Move) {
		drawingArea.GdkWindow.Cursor = new Cursor(CursorType.Fleur);
	    } else if (tool == Tool.Line || tool == Tool.Object || tool == Tool.Annotation) {
		drawingArea.GdkWindow.Cursor = new Cursor(CursorType.Cross);
	    } else if (tool == Tool.Fill) {
		drawingArea.GdkWindow.Cursor = new Cursor(CursorType.Spraycan);
	    } else {
		drawingArea.GdkWindow.Cursor = null;
	    }

	    if (tool == Tool.FloorHeight) {
		BuildHeightPalette(editor.GetFloorHeights());
		palette.Show();
		texturePalette.Hide();
		drawingArea.Mode = DrawMode.FloorHeight;
	    } else if (tool == Tool.CeilingHeight) {
		BuildHeightPalette(editor.GetCeilingHeights());
		palette.Show();
		texturePalette.Hide();
		drawingArea.Mode = DrawMode.CeilingHeight;
	    } else if (tool == Tool.PolygonType) {
		BuildPolygonTypePalette();
		palette.Show();
		texturePalette.Hide();
		drawingArea.Mode = DrawMode.PolygonType;
	    } else if (tool == Tool.FloorLight) {
		BuildLightsPalette();
		palette.Show();
		texturePalette.Hide();
		drawingArea.Mode = DrawMode.FloorLight;
	    } else if (tool == Tool.CeilingLight) {
		BuildLightsPalette();
		palette.Show();
		texturePalette.Hide();
		drawingArea.Mode = DrawMode.CeilingLight;
	    } else if (tool == Tool.MediaLight) {
		BuildLightsPalette();
		palette.Show();
		texturePalette.Hide();
		drawingArea.Mode = DrawMode.MediaLight;
	    } else if (tool == Tool.Media) {
		BuildMediaPalette();
		palette.Show();
		texturePalette.Hide();
		drawingArea.Mode = DrawMode.Media;
	    } else if (tool == Tool.AmbientSound) {
		BuildAmbientSoundPalette();
		palette.Show();
		texturePalette.Hide();
		drawingArea.Mode = DrawMode.AmbientSound;
	    } else if (tool == Tool.RandomSound) {
		BuildRandomSoundPalette();
		palette.Show();
		texturePalette.Hide();
		drawingArea.Mode = DrawMode.RandomSound;
	    } else if (tool == Tool.FloorTexture) {
		palette.Hide();
		BuildTexturePalette();
		texturePalette.Show();
		drawingArea.Mode = DrawMode.FloorTexture;
	    } else if (tool == Tool.CeilingTexture) {
		palette.Hide();
		BuildTexturePalette();
		texturePalette.Show();
		drawingArea.Mode = DrawMode.CeilingTexture;
	    } else {
		drawingArea.Mode = DrawMode.Draw;
		palette.Hide();
		texturePalette.Hide();
	    }
	    UpdateInspector();
	    UpdateStatusBar();
	    Redraw();
	}

	void BuildPolygonTypePalette() {
	    ClearPalette();
	    drawingArea.PaintColors = new Dictionary<short, Drawer.Color>();
	    string[] names = {
		"Normal",
		"Item Impassable",
		"Monster & Item Impassable",
		"Hill",
		"Base",
		"Platform",
		"Light On Trigger",
		"Platform On Trigger",
		"Light Off Trigger",
		"Platform Off Trigger",
		"Teleporter",
		"Zone Border",
		"Goal",
		"Visible Monster Trigger",
		"Invisible Monster Trigger",
		"Dual Monster Trigger",
		"Item Trigger",
		"Must Be Explored",
		"Automatic Exit",
		"Minor Ouch",
		"Major Ouch",
		"Glue",
		"Glue Trigger",
		"Superglue"
	    };

	    paintIndexes.Clear();
	    for (int i = 0; i < names.Length; ++i) {
		paintIndexes[(short) i] = true;
	    }
	    editor.PaintIndex = 0;

	    ColorRadioButton b = null;
	    for (int i = 0; i < paintIndexes.Keys.Count; ++i) {
		Gdk.Color c = paintColors[i % paintColors.Count];
		b = new ColorRadioButton(b, names[i], c);
		b.Index = i;
		b.Toggled += OnChangePaintIndex;
		paletteButtonbox.Add(b);

		drawingArea.PaintColors[(short) i] = GdkToDrawer(c);
	    }

	    paletteButtonbox.ShowAll();
	    paletteAddButton.Sensitive = false;
	    paletteEditButton.Sensitive = false;
	}
	
	void BuildLightsPalette() {
	    ClearPalette();
	    drawingArea.PaintColors = new Dictionary<short, Drawer.Color>();

	    paintIndexes.Clear();
	    for (int i = 0; i < Level.Lights.Count; ++i) {
		paintIndexes[(short) i] = true;
	    }
	    if (Level.Lights.Count > 0) {
		editor.PaintIndex = 0;
	    }

	    ColorRadioButton b = null;
	    for (int i = 0; i < paintIndexes.Keys.Count; ++i) {
		Gdk.Color c = paintColors[i % paintColors.Count];
		b = new ColorRadioButton(b, String.Format("{0}", i), c);
		b.Index = i;
		b.Toggled += OnChangePaintIndex;
		b.DoubleClicked += OnPaletteEdit;
		paletteButtonbox.Add(b);
		
		drawingArea.PaintColors[(short) i] = GdkToDrawer(c);
	    }
	    
	    paletteButtonbox.ShowAll();
	    paletteAddButton.Sensitive = true;
	    paletteEditButton.Sensitive = true;
	}

	internal void OnUndo(object o, EventArgs e) {
	    editor.Undo();
	    ChooseTool(editor.Tool);
	    Redraw();
	}

	internal void OnQuit(object o, EventArgs e) {
	    if (CheckSave()) {
		Application.Quit();
	    }
	}

	internal void OnDelete(object o, DeleteEventArgs e) {
	    if (CheckSave()) {
		e.RetVal = false;
		Application.Quit();
	    } else {
		e.RetVal = true;
	    }
	}

	[Widget] ToggleToolButton twoWUButton;
	[Widget] ToggleToolButton oneWUButton;
	[Widget] ToggleToolButton halfWUButton;
	[Widget] ToggleToolButton quarterWUButton;
	[Widget] ToggleToolButton eighthWUButton;
	
	internal void OnGridSize(object o, EventArgs e) {
	    RadioToolButton button = (RadioToolButton) o;
	    if (button == twoWUButton) {
		grid.Resolution = World.One * 2;
	    } else if (button == oneWUButton) {
		grid.Resolution = World.One;
	    } else if (button == halfWUButton) {
		grid.Resolution = World.One / 2;
	    } else if (button == quarterWUButton) {
		grid.Resolution = World.One / 4;
	    } else if (button == eighthWUButton) {
		grid.Resolution = World.One / 8;
	    }
	    Weland.Settings.PutSetting("Grid/Resolution", grid.Resolution);
	    Redraw();
	}

	internal void OnToggleVisible(object o, EventArgs e) {
	    ToggleToolButton button = (ToggleToolButton) o;
	    if (button == showGridButton) {
		grid.Visible = button.Active;
		Weland.Settings.PutSetting("Grid/Visible", button.Active);
	    } else if (button == showMonstersButton) {
		drawingArea.ShowMonsters = button.Active;
	    } else if (button == showObjectsButton) {
		drawingArea.ShowObjects = button.Active;
	    } else if (button == showSceneryButton) {
		drawingArea.ShowScenery = button.Active;
	    } else if (button == showPlayersButton) {
		drawingArea.ShowPlayers = button.Active;
	    } else if (button == showGoalsButton) {
		drawingArea.ShowGoals = button.Active;
	    } else if (button == showSoundsButton) {
		drawingArea.ShowSounds = button.Active;
	    }
	    Redraw();
	}

	internal void OnToggleSnapToGrid(object o, EventArgs e) {
	    grid.Snap = ((ToggleToolButton) o).Active;
	    Weland.Settings.PutSetting("Grid/Constrain", grid.Snap);
	}
	
	
	/*** begin custom grid code ***/
	
	[Widget] HBox customGridHBox;
	[Widget] Scale gridRotationScale;
	[Widget] Scale gridScaleScale;
	[Widget] Button gridRotationButton;
	[Widget] Button gridScaleButton;
	[Widget] Button gridCenterXButton;
	[Widget] Button gridCenterYButton;
	[Widget] RadioButton grid1Button;
	[Widget] RadioButton grid2Button;
	[Widget] RadioButton grid3Button;
	[Widget] RadioButton grid4Button;
	[Widget] RadioButton grid5Button;
	[Widget] RadioButton grid6Button;
		
	internal void OnShowCustomGridToggle(object o, EventArgs e) {
	    ToggleToolButton button = (ToggleToolButton) o;
	    
	    if(button.Active) customGridHBox.Visible = true;
	    else customGridHBox.Visible = false;
	    grid.UseCustomGrid = button.Active;
	}
	
	internal void OnGridRotationChange(object o, EventArgs e) {
		grid.Rotation = ((Range) o).Value;
		gridRotationButton.Label = String.Format("{0:0.0}",grid.Rotation);
		Redraw();
	}
	
	internal void OnGridRotationClick(object o, EventArgs e) {
		double v;
		DoubleDialog dialog = new DoubleDialog("Set Grid Rotation", window1);
		if (dialog.Run() == (int) ResponseType.Ok && dialog.Valid) {
		    v = dialog.Value;
		    if(v < gridRotationScale.Adjustment.Lower) v = gridRotationScale.Adjustment.Lower;
		    else if(v > gridRotationScale.Adjustment.Upper) v = gridRotationScale.Adjustment.Upper;
		    grid.Rotation = v;
		    ((Button) o).Label = String.Format("{0:0.0}",grid.Rotation);
		    gridRotationScale.Value = grid.Rotation;
		}
		dialog.Destroy();
		Redraw();
	}
	
	internal void OnGridScaleChange(object o, EventArgs e) {
		grid.Scale = Math.Pow(10, ((Range) o).Value);
		gridScaleButton.Label = String.Format("{0:0.00}",grid.Scale);
		Redraw();
	}
	
	internal void OnGridScaleClick(object o, EventArgs e) {
		double v;
		DoubleDialog dialog = new DoubleDialog("Set Grid Scale", window1);
		if (dialog.Run() == (int) ResponseType.Ok && dialog.Valid) {
		    v = dialog.Value;
		    if(v < Math.Pow(10, gridScaleScale.Adjustment.Lower)) v = Math.Pow(10, gridScaleScale.Adjustment.Lower);
		    else if(v > Math.Pow(10, gridScaleScale.Adjustment.Upper)) v = Math.Pow(10, gridScaleScale.Adjustment.Upper);
		    grid.Scale = v;
		    ((Button) o).Label = String.Format("{0:0.00}",grid.Scale);
		    gridScaleScale.Value = Math.Log10(grid.Scale);
		}
		dialog.Destroy();
		Redraw();
	}
	
	internal void OnGridCenterXClick(object o, EventArgs e) {
		int v;
		IntDialog dialog = new IntDialog("Set Grid X Center", window1);
		dialog.Value = (int)grid.Center.X;
		if (dialog.Run() == (int) ResponseType.Ok && dialog.Valid) {
		    v = dialog.Value;
		    if(v < short.MinValue) v = short.MinValue;
		    else if (v > short.MaxValue) v = short.MaxValue;
		    grid.Center.X = (short)v;
		    ((Button) o).Label = String.Format("{0}",grid.Center.X);
		}
		dialog.Destroy();
		Redraw();
	}
	
	internal void OnGridCenterYClick(object o, EventArgs e) {
		int v;
		IntDialog dialog = new IntDialog("Set Grid Y Center", window1);
		dialog.Value = (int)grid.Center.Y;
		if (dialog.Run() == (int) ResponseType.Ok && dialog.Valid) {
		    v = dialog.Value;
		    if(v < short.MinValue) v = short.MinValue;
		    else if (v > short.MaxValue) v = short.MaxValue;
		    grid.Center.Y = (short)v;
		    ((Button) o).Label = String.Format("{0}",grid.Center.Y);
		}
		dialog.Destroy();
		Redraw();
	}
	
	
	internal void OnGridCenterAtPoint(object o, EventArgs e) {
		if(selection.Point!=-1) {
			Point p = Level.Endpoints[selection.Point];
			grid.Center.X = p.X;
			grid.Center.Y = p.Y;
			gridCenterXButton.Label = String.Format("{0}",grid.Center.X);
			gridCenterYButton.Label = String.Format("{0}",grid.Center.Y);
			Redraw();
		}
	}
	
	internal void OnGridRotateToLine(object o, EventArgs e) {
		if(selection.Line!=-1) {
			Line l = Level.Lines[selection.Line];
			Point p1 = Level.Endpoints[l.EndpointIndexes[0]], p2 = Level.Endpoints[l.EndpointIndexes[1]];
			grid.Rotation=-(180/Math.PI)*Math.Atan2(p2.Y-p1.Y,p2.X-p1.X);
			while(grid.Rotation<0) grid.Rotation+=90;
			while(grid.Rotation>90) grid.Rotation-=90;
			gridRotationScale.Value = grid.Rotation;
			gridRotationButton.Label = String.Format("{0:0.0}",grid.Rotation);
			Redraw();
		}
	}

	internal void OnGridScaleToLine(object o, EventArgs e) {
		if(selection.Line!=-1) {
			Line l = Level.Lines[selection.Line];
			Point p1 = Level.Endpoints[l.EndpointIndexes[0]], p2 = Level.Endpoints[l.EndpointIndexes[1]];
			double d=Math.Sqrt( (p2.X-p1.X)*(p2.X-p1.X) + (p2.Y-p1.Y)*(p2.Y-p1.Y) )/1024;
			if(d >= Math.Pow(10, gridScaleScale.Adjustment.Lower) && d <= Math.Pow(10, gridScaleScale.Adjustment.Upper)) {
				grid.Scale = d;
				gridScaleScale.Value = Math.Log10(d);
				gridScaleButton.Label = String.Format("{0:0.00}",grid.Scale);
				Redraw();
			}
		}
	}
	
	
	protected void OnGridChange(object o, EventArgs e) {
		int i=0;
		if( ((RadioButton) o).Active) {
			if(o == grid1Button) i=0;
			else if(o == grid2Button) i=1;
			else if(o == grid3Button) i=2;
			else if(o == grid4Button) i=3;
			else if(o == grid5Button) i=4;
			else if(o == grid6Button) i=5;
			
			grid.Rotations[grid.CurrentGrid]=grid.Rotation;
			grid.Centers[grid.CurrentGrid]=grid.Center;
			grid.Scales[grid.CurrentGrid]=grid.Scale;
			
			grid.Rotation=grid.Rotations[i];
			gridRotationScale.Value = grid.Rotation;
			gridRotationButton.Label = String.Format("{0:0.0}",grid.Rotation);
			grid.Center=grid.Centers[i];
			gridCenterXButton.Label = String.Format("{0}",grid.Center.X);
			gridCenterYButton.Label = String.Format("{0}",grid.Center.Y);
			grid.Scale=grid.Scales[i];
			gridScaleScale.Value = Math.Log10(grid.Scale);
			gridScaleButton.Label = String.Format("{0:0.00}",grid.Scale);
			
			grid.CurrentGrid=i;
			
			Redraw();
		}
	}
	
	/*** end custom grid code ***/
	

	protected void OnLevelParameters(object o, EventArgs e) {
	    LevelParametersDialog d = new LevelParametersDialog(window1, Level);
	    if (d.Run() == (int) ResponseType.Ok) {
		editor.Changed = true;
		window1.Title = Level.Name;
		BuildLevelMenu();
	    }
	}

	protected void OnItemParameters(object o, EventArgs e) {
	    ItemParametersDialog d = new ItemParametersDialog(window1, Level);
	    if (d.Run() == (int) ResponseType.Ok) {
		editor.Changed = true;
	    }
	}

	protected void OnMonsterParameters(object o, EventArgs e) {
	    MonsterParametersDialog d = new MonsterParametersDialog(window1, Level);
	    if (d.Run() == (int) ResponseType.Ok) {
		editor.Changed = true;
	    }
	}

	protected void OnPave(object o, EventArgs e) {
	    Level.Pave();
	    editor.Changed = true;
	}

	protected void OnNukeTextures(object o, EventArgs e) {
	    editor.SetUndo();
	    Level.NukeTextures();
	    Level.Pave();
	    editor.Changed = true;
	}

	protected void OnNukeObjects(object o, EventArgs e) {
	    editor.SetUndo();
	    Level.NukeObjects();
	    editor.Changed = true;
	    Redraw();
	}

	void PaletteNext() {
	    for (int i = 0; i < paletteButtonbox.Children.Length - 1; ++i) {
		if (((ColorRadioButton) paletteButtonbox.Children[i]).Active) {
		    ((ColorRadioButton) paletteButtonbox.Children[i + 1]).Active = true;
		    return;
		}
	    }
	}

	void PalettePrev() {
	    for (int i = 1; i < paletteButtonbox.Children.Length; ++i) {
		if (((ColorRadioButton) paletteButtonbox.Children[i]).Active) {
		    ((ColorRadioButton) paletteButtonbox.Children[i - 1]).Active = true;
		}
	    }
	}

	protected void OnPaletteAdd(object o, EventArgs e) {
	    if (editor.Tool == Tool.FloorHeight) {
		DoubleDialog dialog = new DoubleDialog("Add Floor Height", window1);
		if (dialog.Run() == (int) ResponseType.Ok && dialog.Valid) {
		    short height = World.FromDouble(dialog.Value);
		    SortedList<short, bool> heights = editor.GetFloorHeights();
		    heights[height] = true;
		    BuildHeightPalette(heights);
		    ColorRadioButton b = (ColorRadioButton) paletteButtonbox.Children[paintIndexes.IndexOfKey(height)];
		    b.Active = true;
		}
		dialog.Destroy();
	    } else if (editor.Tool == Tool.CeilingHeight) {
		DoubleDialog dialog = new DoubleDialog("Add Ceiling Height", window1);
		if (dialog.Run() == (int) ResponseType.Ok && dialog.Valid) {
		    short height = World.FromDouble(dialog.Value);
		    SortedList<short, bool> heights = editor.GetCeilingHeights();
		    heights[height] = true;
		    BuildHeightPalette(heights);
		    ((ColorRadioButton) paletteButtonbox.Children[paintIndexes.IndexOfKey(height)]).Active = true;
		}
		dialog.Destroy();
	    } else if (editor.Tool == Tool.FloorLight || editor.Tool == Tool.CeilingLight || editor.Tool == Tool.MediaLight) {
		Level.Lights.Add(new Light());
		LightParametersDialog dialog = new LightParametersDialog(window1, Level, (short) (Level.Lights.Count - 1));
		if (dialog.Run() == (int) ResponseType.Ok) {
		    BuildLightsPalette();
		    ((ColorRadioButton) paletteButtonbox.Children[Level.Lights.Count - 1]).Active = true;
		    editor.PaintIndex = (short) (Level.Lights.Count - 1);
		} else {
		    Level.Lights.RemoveAt(Level.Lights.Count - 1);
		}
	    } else if (editor.Tool == Tool.Media) {
		Level.Medias.Add(new Media());
		MediaParametersDialog dialog = new MediaParametersDialog(window1, Level, (short) (Level.Medias.Count - 1));
		if (dialog.Run() == (int) ResponseType.Ok) {
		    BuildMediaPalette();
		    ((ColorRadioButton) paletteButtonbox.Children[Level.Medias.Count]).Active = true;
		    editor.PaintIndex = (short) (Level.Medias.Count - 1);
		} else {
		    Level.Medias.RemoveAt(Level.Medias.Count - 1);
		}
	    } else if (editor.Tool == Tool.AmbientSound) {
		AmbientSound sound = new AmbientSound();
		AmbientSoundParametersDialog dialog = new AmbientSoundParametersDialog(window1, sound);
		if (dialog.Run() == (int) ResponseType.Ok) {
		    Level.AmbientSounds.Add(sound);
		    BuildAmbientSoundPalette();
		    ((ColorRadioButton) paletteButtonbox.Children[Level.AmbientSounds.Count]).Active = true;
		    editor.PaintIndex = (short) (Level.AmbientSounds.Count - 1);
		} 
	    } else if (editor.Tool == Tool.RandomSound) {
		RandomSound sound = new RandomSound();
		RandomSoundParametersDialog dialog = new RandomSoundParametersDialog(window1, sound);
		if (dialog.Run() == (int) ResponseType.Ok) {
		    Level.RandomSounds.Add(sound);
		    BuildRandomSoundPalette();
		    ((ColorRadioButton) paletteButtonbox.Children[Level.RandomSounds.Count]).Active = true;
		    editor.PaintIndex = (short) (Level.RandomSounds.Count - 1);
		}
	    }
	    Redraw();
	}

	void PaletteEdit() {
	    if (editor.Tool == Tool.FloorHeight) {
		short original_height = editor.PaintIndex;
		DoubleDialog dialog = new DoubleDialog("Edit Floor Height", window1);
		dialog.Value = World.ToDouble(original_height);
		if (dialog.Run() == (int) ResponseType.Ok && dialog.Valid) {
		    short height = World.FromDouble(dialog.Value);
		    editor.ChangeFloorHeights(original_height, height);
		    SortedList<short, bool> heights = editor.GetFloorHeights();
		    heights[height] = true;
		    BuildHeightPalette(heights);
		    ((ColorRadioButton) paletteButtonbox.Children[paintIndexes.IndexOfKey(height)]).Active = true;
		    editor.Changed = true;
		}
		dialog.Destroy();
		Redraw();
	    } else if (editor.Tool == Tool.CeilingHeight) {
		short original_height = editor.PaintIndex;
		DoubleDialog dialog = new DoubleDialog("Edit Ceiling Height", window1);
		dialog.Value = World.ToDouble(original_height);
		if (dialog.Run() == (int) ResponseType.Ok && dialog.Valid) {
		    short height = World.FromDouble(dialog.Value);
		    editor.ChangeCeilingHeights(original_height, height);
		    SortedList<short, bool> heights = editor.GetCeilingHeights();
		    heights[height] = true;
		    BuildHeightPalette(heights);
		    ((ColorRadioButton) paletteButtonbox.Children[paintIndexes.IndexOfKey(height)]).Active = true;
		    editor.Changed = true;
		}
		dialog.Destroy();
		Redraw();
	    } else if (editor.Tool == Tool.FloorLight || editor.Tool == Tool.CeilingLight || editor.Tool == Tool.MediaLight) {
		short index = editor.PaintIndex;
		LightParametersDialog dialog = new LightParametersDialog(window1, Level, index);
		if (dialog.Run() == (int) ResponseType.Ok) {
		    editor.Changed = true;
		}
	    } else if (editor.Tool == Tool.Media) {
		short index = editor.PaintIndex;
		if (index != -1) {
		    MediaParametersDialog dialog = new MediaParametersDialog(window1, Level, index);
		    if (dialog.Run() == (int) ResponseType.Ok) {
			editor.Changed = true;
		    }
		}
	    } else if (editor.Tool == Tool.AmbientSound) {
		short index = editor.PaintIndex;
		if (index != -1) {
		    AmbientSoundParametersDialog dialog = new AmbientSoundParametersDialog(window1, Level.AmbientSounds[index]);
		    if (dialog.Run() == (int) ResponseType.Ok) {
			editor.Changed = true;
		    }
		}
	    } else if (editor.Tool == Tool.RandomSound) {
		short index = editor.PaintIndex;
		if (index != -1) {
		    RandomSoundParametersDialog dialog = new RandomSoundParametersDialog(window1, Level.RandomSounds[index]);
		    if (dialog.Run() == (int) ResponseType.Ok) {
			editor.Changed = true;
		    }
		}
	    }
	    Redraw();
	}

	protected void OnPaletteEdit(object o, EventArgs e) { 
	    PaletteEdit();
	}

	protected void OnAbout(object o, EventArgs e) {
	    AboutDialog dialog = new AboutDialog();
	    dialog.ProgramName = "Weland";
	    dialog.Artists = new string[] { "Robert Kreps (application icon)",
					    "tango-art-libre, GIMP, openclipart.org (tool icons)" };
	    dialog.Authors = new string[] { "Gregory Smith <wolfy@treellama.org>",
					    "mijienke <mijenke@gmail.com> (custom grid)",
					    "effigy <this.effigy@gmail.com> (tooltips)",
					    "with thanks to Eric Peterson for Smithy"};
	    dialog.License = "Weland is available under the GNU General Public License, Version 2. See the file COPYING for details";
	    dialog.Website = "http://sourceforge.net/projects/weland";
	    dialog.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
	    dialog.Run();
	    dialog.Destroy();
	}

	protected void OnRecenterLevel(object o, EventArgs e) {
	    editor.Recenter();
	    Redraw();
	}

	protected void OnGoto(object o, EventArgs e) {
	    GotoDialog dialog = new GotoDialog(window1);
	    if (dialog.Run() == (int) ResponseType.Ok) {
		short index = 0;
		if (short.TryParse(dialog.Number.Text, out index)) {
		    if (dialog.Type.Active == 0) {
			if (index >= 0 && index < Level.Endpoints.Count) {
			    selection.Clear();
			    selectButton.Active = true;
			    selection.Point = index;
			    Point p = Level.Endpoints[selection.Point];
			    Center(p.X, p.Y);
			    UpdateStatusBar();
			    Redraw();
			}
		    } else if (dialog.Type.Active == 1) {
			if (index >= 0 && index < Level.Lines.Count) {
			    selection.Clear();
			    selectButton.Active = true;
			    selection.Line = index;
			    Line line = Level.Lines[selection.Line];
			    Point p1 = Level.Endpoints[line.EndpointIndexes[0]];
			    Point p2 = Level.Endpoints[line.EndpointIndexes[1]];
			    Center((short) ((p1.X + p2.X) / 2), (short) ((p1.Y + p2.Y) / 2));
			    UpdateStatusBar();
			    Redraw();
			}
		    } else if (dialog.Type.Active == 2) {
			if (index >= 0 && index < Level.Polygons.Count) {
			    selection.Clear();
			    selectButton.Active = true;
			    selection.Polygon = index;
			    Polygon polygon = Level.Polygons[index];
			    Point center = Level.PolygonCenter(polygon);
			    Center(center.X, center.Y);
			    UpdateStatusBar();
			    Redraw();
			}
		    }
		}
	    }
	    dialog.Destroy();
	}

	protected void OnFindZeroLengthLines(object o, EventArgs e) {
	    short i = Level.FindZeroLengthLine();
	    if (i == -1) {
		MessageDialog d = new MessageDialog(window1, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, "No zero length lines found.");
		d.Run();
		d.Destroy();
	    } else {
		MessageDialog d = new MessageDialog(window1, DialogFlags.DestroyWithParent, MessageType.Warning, ButtonsType.Ok, String.Format("Line {0} has zero length.", i));
		d.Run();
		d.Destroy();
		selection.Clear();
		selectButton.Active = true;
		selection.Line = i;
		Point p = Level.Endpoints[Level.Lines[i].EndpointIndexes[0]];
		Center(p.X, p.Y);
		UpdateStatusBar();
		Redraw();
	    }
	}

	void LoadLayer() {
	    double floorHeight = Weland.Settings.GetSetting(String.Format("Layers/Layer{0}/FloorHeight", layer), -32.0);
	    double ceilingHeight = Weland.Settings.GetSetting(String.Format("Layers/Layer{0}/CeilingHeight", layer), 32.0);
	    
	    viewFloorHeight.Value = floorHeight;
	    viewCeilingHeight.Value = ceilingHeight;
	}

	void SaveLayer() {
	    Weland.Settings.PutSetting(String.Format("Layers/Layer{0}/FloorHeight", layer), viewFloorHeight.Value);
	    Weland.Settings.PutSetting(String.Format("Layers/Layer{0}/CeilingHeight", layer), viewCeilingHeight.Value);
	}

	protected void OnLayerChanged(object o, EventArgs e) {
	    if (o == layer1) {
		layer = 1;
	    } else if (o == layer2) {
		layer = 2;
	    } else if (o == layer3) {
		layer = 3;
	    } else if (o == layer4) {
		layer = 4;
	    } else if (o == layer5) {
		layer = 5;
	    } else if (o == layer6) {
		layer = 6;
	    }
	    LoadLayer();
	    Redraw();
	}
	
	protected void OnPreferences(object o, EventArgs e) {
	    PreferencesDialog d = new PreferencesDialog(window1, drawingArea, editor);
	    d.Run();
	    drawingArea.Antialias = Weland.Settings.GetSetting("Drawer/SmoothLines", true);
	    Redraw();
	}
    }
}
