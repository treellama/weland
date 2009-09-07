using Gtk;
using Gdk;
using Glade;
using System;
using System.Collections.Generic;

namespace Weland {
    // resizable, scrollable map view
    public class MapWindow {
	public Level Level {
	    get { return drawingArea.Level; }
	    set { 
		drawingArea.Level = value;
		editor.Level = value;
	    }
	}

	static public bool IsMac() {
	    return ((int) System.Environment.OSVersion.Platform == 6 || 
		    // ugh, Mono
		    (System.Environment.OSVersion.Platform == PlatformID.Unix && System.Environment.OSVersion.Version.Major == 9));	    
	}

	Wadfile wadfile = new Wadfile();
	Editor editor = new Editor();
	Grid grid = new Grid();

	SortedList<short, bool> paintIndexes = new SortedList<short, bool>();
	List<Gdk.Color> paintColors = new List<Gdk.Color>();

	[Widget] Gtk.Window window1;
	[Widget] VScrollbar vscrollbar1;
	[Widget] HScrollbar hscrollbar1;
	[Widget] MenuItem levelItem;
	[Widget] Table table1;

	[Widget] RadioToolButton selectButton;
	[Widget] RadioToolButton zoomButton;
	[Widget] RadioToolButton moveButton;
	[Widget] RadioToolButton lineButton;
	[Widget] RadioToolButton fillButton;
	[Widget] RadioToolButton floorHeightButton;
	[Widget] RadioToolButton ceilingHeightButton;

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

	string Filename;

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

	public MapWindow(string title) {
	    Glade.XML gxml = new Glade.XML(null, "mapwindow.glade", "window1", null);
	    gxml.Autoconnect(this);
	    SetupDrawingArea();

	    SetIconResource(selectButton, "arrow.png");
	    SetIconResource(zoomButton, "zoom.png");
	    SetIconResource(moveButton, "move.png");
	    SetIconResource(lineButton, "line.png");
	    SetIconResource(fillButton, "fill.png");
	    SetIconResource(floorHeightButton, "floor-height.png");
	    SetIconResource(ceilingHeightButton, "ceiling-height.png");

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

	    editor.Grid = grid;
	    drawingArea.Grid = grid;

	    window1.AllowShrink = true;
	    window1.Resize(640, 480);
	    window1.Show();

	    window1.Focus = null;
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

	void Redraw() {
	    drawingArea.QueueDrawArea(0, 0, drawingArea.Allocation.Width, drawingArea.Allocation.Height);
	    editor.ClearDirty();
	}

	void RedrawDirty() {
	    const int dirtySlop = 4;
	    if (editor.Dirty) {
		int X1 = (int) drawingArea.Transform.ToScreenX(editor.RedrawLeft) - dirtySlop;
		int Y1 = (int) drawingArea.Transform.ToScreenY(editor.RedrawTop) - dirtySlop;
		int X2 = (int) drawingArea.Transform.ToScreenX(editor.RedrawRight) + dirtySlop;
		int Y2 = (int) drawingArea.Transform.ToScreenY(editor.RedrawBottom) + dirtySlop;
		drawingArea.QueueDrawArea(X1, Y1, X2 -X1, Y2 - Y1);
		editor.ClearDirty();
	    }
	}

	internal void OnScroll(object obj, ScrollEventArgs args) {
		if (args.Event.Direction == ScrollDirection.Down) {
			vscrollbar1.Value += 32.0 / drawingArea.Transform.Scale;
		} else if (args.Event.Direction == ScrollDirection.Up) {
			vscrollbar1.Value -= 32.0 / drawingArea.Transform.Scale;
		} else if (args.Event.Direction == ScrollDirection.Right) {
			hscrollbar1.Value += 32.0 / drawingArea.Transform.Scale;
		} else if (args.Event.Direction == ScrollDirection.Left) {
			hscrollbar1.Value -= 32.0 / drawingArea.Transform.Scale;
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
	    
	    if ((type & ModifierType.Mod1Mask) != 0) {
		modifiers |= EditorModifiers.Alt;
	    }

	    return modifiers;
	}
	
	internal void OnButtonPressed(object obj, ButtonPressEventArgs args) {
	    EventButton ev = args.Event;
	    short X = drawingArea.Transform.ToMapX(ev.X);
	    short Y = drawingArea.Transform.ToMapY(ev.Y);

	    if (editor.Tool == Tool.Zoom) {
		if (ev.Button == 3 || (ev.State & ModifierType.Mod1Mask) != 0) {
		    drawingArea.Transform.Scale /= Math.Sqrt(2.0);
		} else {
		    drawingArea.Transform.Scale *= Math.Sqrt(2.0);
		}

		Center(X, Y);
		AdjustScrollRange();
		editor.Snap = (short) (8 / drawingArea.Transform.Scale);
	    } else if (editor.Tool == Tool.Move) {
		xDown = ev.X;
		yDown = ev.Y;
		xOffsetDown = drawingArea.Transform.XOffset;
		yOffsetDown = drawingArea.Transform.YOffset;
	    } else {
		EditorModifiers modifiers = Modifiers(ev.State);
		if (ev.Button == 3) {
		    modifiers |= EditorModifiers.RightClick;
		}
		editor.ButtonPress(X, Y, modifiers);
		Redraw();
	    }

	    args.RetVal = true;
	}

	internal void OnButtonReleased(object obj, ButtonReleaseEventArgs args) {
	    editor.ButtonRelease(drawingArea.Transform.ToMapX(args.Event.X), drawingArea.Transform.ToMapY(args.Event.Y));
	    Redraw();

	    if (editor.Tool == Tool.FloorHeight || editor.Tool == Tool.CeilingHeight) {
		// update the paint mode button
		int i = paintIndexes.IndexOfKey(editor.PaintIndex);
		if (i != -1) {
		    ColorRadioButton crb = (ColorRadioButton) paletteButtonbox.Children[i];
		    if (!crb.Active) {
			crb.Active = true;
		    }
		}
	    }

	    args.RetVal = true;
	}

	internal void OnMotion(object obj, MotionNotifyEventArgs args) {
	    if (editor.Tool == Tool.Move) {
		hscrollbar1.Value = xOffsetDown + (xDown - args.Event.X) / drawingArea.Transform.Scale;
		vscrollbar1.Value = yOffsetDown + (yDown - args.Event.Y) / drawingArea.Transform.Scale;
		args.RetVal = true;
	    } else {
		editor.Motion(drawingArea.Transform.ToMapX(args.Event.X), drawingArea.Transform.ToMapY(args.Event.Y), Modifiers(args.Event.State));
		RedrawDirty();
	    }
	}

	Tool oldTool = Tool.Move;

	[GLib.ConnectBefore()] internal void OnSpecialKeyPressed(object obj, KeyPressEventArgs args) {
	    if (args.Event.Key == Gdk.Key.space) {
		if (editor.Tool != Tool.Move) {
		    oldTool = editor.Tool;
		    editor.Tool = Tool.Move;
		    drawingArea.GdkWindow.Cursor = new Cursor(CursorType.Fleur);		    
		}
		args.RetVal = true;    
	    }
	}

	internal void OnKeyPressed(object obj, KeyPressEventArgs args) {
	    bool caught = true;
	    switch (args.Event.Key) {
	    case Gdk.Key.F3:
		drawingArea.Antialias = !drawingArea.Antialias;
		Redraw();
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
	    case Gdk.Key.d:
	    case Gdk.Key.D:
		moveButton.Active = true;
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
		Redraw();
		break;
	    case Gdk.Key.o:
		palette.Visible = !palette.Visible;
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
	    }
	}

	public bool CheckSave() {
	    if (editor.Changed) {
		MessageDialog dialog = new MessageDialog(window1, DialogFlags.DestroyWithParent, MessageType.Warning, ButtonsType.YesNo, "Do you wish to save changes?");
		if (dialog.Run() == (int) ResponseType.Yes) {
		    dialog.Destroy();
		    return Save();
		} else {
		    dialog.Destroy();
		    return true;
		}
	    } else {
		return true;
	    }
	}

	public void SelectLevel(int n) {
	    if (CheckSave()) {
		Level = new Level();
		Level.Load(wadfile.Directory[n]);
		drawingArea.Transform = new Transform();
		editor.Snap = (short) (8 / drawingArea.Transform.Scale);
		Center(0, 0);
		AdjustScrollRange();
		window1.Title = wadfile.Directory[n].LevelName;
		ChooseTool(editor.Tool);
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
		Menu menu = new Menu();
		foreach (var kvp in wadfile.Directory) {
		    if (kvp.Value.Chunks.ContainsKey(MapInfo.Tag)) {
			MenuItem item = new MenuItem(kvp.Value.LevelName);
			int levelNumber = kvp.Key;
			item.Activated += delegate(object obj, EventArgs args) { SelectLevel(levelNumber); };
			menu.Append(item);
		    }
		}
		menu.ShowAll();
		levelItem.Submenu = menu;
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
		if (d.Run() == (int) ResponseType.Accept) {
		    OpenFile(d.Filename);
		}
		d.Destroy();
	    }
	}

	public void NewLevel() {
	    wadfile = new Wadfile();
	    Level = new Level();
	    levelItem.Submenu = null;
	    drawingArea.Transform = new Transform();
	    editor.Snap = (short) (8 / drawingArea.Transform.Scale);
	    Center(0, 0);
	    AdjustScrollRange();
	    window1.Title = "Untitled Level";
	    Filename = "";
	    ChooseTool(editor.Tool);
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
	    if (d.Run() == (int) ResponseType.Accept) {
		wadfile = new Wadfile();
		Level.AssurePlayerStart();
		Redraw();
		wadfile.Directory[0] = Level.Save();
		wadfile.Save(d.Filename);
		Filename = d.Filename;

		editor.Changed = false;
		saved = true;
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
		Level.AssurePlayerStart();
		Redraw();
		wadfile.Directory[0] = Level.Save();
		wadfile.Save(Filename);
		editor.Changed = false;
		return true;
	    }
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
	    } else if (button == floorHeightButton) {
		ChooseTool(Tool.FloorHeight);
	    } else if (button == ceilingHeightButton) {
		ChooseTool(Tool.CeilingHeight);
	    }
	}

	void OnChangePaintIndex(object obj, EventArgs args) {
	    ColorRadioButton crb = (ColorRadioButton) obj;
	    if (crb.Active) {
		editor.PaintIndex = paintIndexes.Keys[crb.Index];
	    }
	}

	void BuildHeightPalette(SortedList<short, bool> heights) {
	    while (paletteButtonbox.Children.Length > 0) {
		paletteButtonbox.Remove(paletteButtonbox.Children[0]);
	    }
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
		
		drawingArea.PaintColors[paintIndexes.Keys[i]] = new Drawer.Color((double) c.Red / ushort.MaxValue, (double) c.Green / ushort.MaxValue, (double) c.Blue / ushort.MaxValue);
	    }

	    paletteButtonbox.ShowAll();
	    
	    paletteAddButton.Sensitive = true;
	    paletteEditButton.Sensitive = (paintIndexes.Keys.Count > 0);
	}

	void ChooseTool(Tool tool) {
	    editor.Tool = tool;
	    if (tool == Tool.Zoom) {
		drawingArea.GdkWindow.Cursor = new Cursor(CursorType.Target);
	    } else if (tool == Tool.Move) {
		drawingArea.GdkWindow.Cursor = new Cursor(CursorType.Fleur);
	    } else if (tool == Tool.Line) {
		drawingArea.GdkWindow.Cursor = new Cursor(CursorType.Cross);
	    } else if (tool == Tool.Fill) {
		drawingArea.GdkWindow.Cursor = new Cursor(CursorType.Spraycan);
	    } else {
		drawingArea.GdkWindow.Cursor = null;
	    }

	    if (tool == Tool.FloorHeight) {
		BuildHeightPalette(editor.GetFloorHeights());
		palette.Show();
		drawingArea.Mode = DrawMode.FloorHeight;
	    } else if (tool == Tool.CeilingHeight) {
		BuildHeightPalette(editor.GetCeilingHeights());
		palette.Show();
		drawingArea.Mode = DrawMode.CeilingHeight;
	    } else {
		drawingArea.Mode = DrawMode.Draw;
		palette.Hide();
	    }
	    Redraw();
	}

	internal void OnUndo(object o, EventArgs e) {
	    editor.Undo();
	    ChooseTool(editor.Tool);
	    Redraw();
	}

	internal void OnQuit(object o, EventArgs e) {
	    Application.Quit();
	}

	internal void OnDelete(object o, DeleteEventArgs e) {
	    Application.Quit();
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
	    Redraw();
	}

	internal void OnToggleVisible(object o, EventArgs e) {
	    ToggleToolButton button = (ToggleToolButton) o;
	    if (button == showGridButton) {
		grid.Visible = button.Active;
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
	}

	protected void OnLevelParameters(object o, EventArgs e) {
	    LevelParametersDialog d = new LevelParametersDialog(window1, Level);
	    d.Run();
	    window1.Title = Level.Name;
	}

	protected void OnPave(object o, EventArgs e) {
	    Level.Pave();
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
	    }
	}

	void PaletteEdit() {
	    if (editor.Tool == Tool.FloorHeight) {
		short original_height = editor.PaintIndex;
		DoubleDialog dialog = new DoubleDialog("Edit Floor Height", window1);
		dialog.Value = World.ToDouble(original_height);
		if (dialog.Run() == (int) ResponseType.Ok && dialog.Valid) {
		    short height = World.FromDouble(dialog.Value);
		    editor.ChangeFloorHeights(original_height, height);
		    BuildHeightPalette(editor.GetFloorHeights());

		    ((ColorRadioButton) paletteButtonbox.Children[paintIndexes.IndexOfKey(height)]).Active = true;
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
		    BuildHeightPalette(editor.GetCeilingHeights());

		    ((ColorRadioButton) paletteButtonbox.Children[paintIndexes.IndexOfKey(height)]).Active = true;
		}
		dialog.Destroy();
		Redraw();
	    }

	}

	protected void OnPaletteEdit(object o, EventArgs e) { 
	    PaletteEdit();
	}
    }
}