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

	Wadfile wadfile = new Wadfile();
	Editor editor = new Editor();

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

	[Widget] ToggleToolButton showGridButton;
	[Widget] ToggleToolButton showMonstersButton;
	[Widget] ToggleToolButton showObjectsButton;
	[Widget] ToggleToolButton showSceneryButton;
	[Widget] ToggleToolButton showPlayersButton;
	[Widget] ToggleToolButton showGoalsButton;
	[Widget] ToggleToolButton showSoundsButton;

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
	    window1.AllowShrink = true;
	    window1.Resize(640, 480);
	    window1.Show();

	    SetIconResource(selectButton, "arrow.png");
	    SetIconResource(zoomButton, "zoom.png");
	    SetIconResource(moveButton, "move.png");
	    SetIconResource(lineButton, "line.png");
	    SetIconResource(fillButton, "fill.png");

	    SetIconResource(showGridButton, "grid.png");
	    SetIconResource(showMonstersButton, "monster.png");
	    SetIconResource(showObjectsButton, "pistol-ammo.png");
	    SetIconResource(showSceneryButton, "flower.png");
	    SetIconResource(showPlayersButton, "player.png");
	    SetIconResource(showGoalsButton, "flag.png");
	    SetIconResource(showSoundsButton, "sound.png");

	    //	    oneWUButton.Active = true;
	    
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
	
	internal void OnButtonPressed(object obj, ButtonPressEventArgs args) {
	    EventButton ev = args.Event;
	    short X = drawingArea.Transform.ToMapX(ev.X);
	    short Y = drawingArea.Transform.ToMapY(ev.Y);

	    if (editor.Tool == Tool.Zoom) {
		if (ev.Button == 1) {
		    drawingArea.Transform.Scale *= Math.Sqrt(2.0);
		} else if (ev.Button == 3) {
		    drawingArea.Transform.Scale /= Math.Sqrt(2.0);
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
		editor.ButtonPress(X, Y);
		Redraw();
	    }
	    args.RetVal = true;
	}

	internal void OnButtonReleased(object obj, ButtonReleaseEventArgs args) {
	    editor.ButtonRelease(drawingArea.Transform.ToMapX(args.Event.X), drawingArea.Transform.ToMapY(args.Event.Y));
	    Redraw();
	    args.RetVal = true;
	}

	internal void OnMotion(object obj, MotionNotifyEventArgs args) {
	    if (editor.Tool == Tool.Move) {
		hscrollbar1.Value = xOffsetDown + (xDown - args.Event.X) / drawingArea.Transform.Scale;
		vscrollbar1.Value = yOffsetDown + (yDown - args.Event.Y) / drawingArea.Transform.Scale;
		args.RetVal = true;
	    } else {
		editor.Motion(drawingArea.Transform.ToMapX(args.Event.X), drawingArea.Transform.ToMapY(args.Event.Y));
		RedrawDirty();
	    }
	}

	Tool oldTool = Tool.Move;

	[GLib.ConnectBefore() ] internal void OnKeyPressed(object obj, KeyPressEventArgs args) {
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
	    case Gdk.Key.h:
	    case Gdk.Key.H:
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
	    case Gdk.Key.Delete:
	    case Gdk.Key.BackSpace:
		editor.DeleteSelected();
		Redraw();
		break;
	    case Gdk.Key.space:
		if (editor.Tool != Tool.Move) {
		    oldTool = editor.Tool;
		    ChooseTool(Tool.Move);
		}
		break;
	    default:
		caught = false;
		break;
	    }
	    args.RetVal = caught;
	}

	[GLib.ConnectBefore()] internal void OnKeyReleased(object obj, KeyReleaseEventArgs args) {
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
	    }
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
	    Redraw();
	}

	internal void OnUndo(object o, EventArgs e) {
	    editor.Undo();
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
		drawingArea.GridResolution = 2048;
	    } else if (button == oneWUButton) {
		drawingArea.GridResolution = 1024;
	    } else if (button == halfWUButton) {
		drawingArea.GridResolution = 512;
	    } else if (button == quarterWUButton) {
		drawingArea.GridResolution = 256;
	    } else if (button == eighthWUButton) {
		drawingArea.GridResolution = 128;
	    }
	    Redraw();
	}

	internal void OnToggleVisible(object o, EventArgs e) {
	    ToggleToolButton button = (ToggleToolButton) o;
	    if (button == showGridButton) {
		drawingArea.ShowGrid = button.Active;
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

	protected void OnLevelParameters(object o, EventArgs e) {
	    LevelParametersDialog d = new LevelParametersDialog(window1, Level);
	    d.Run();
	    window1.Title = Level.MapInfo.Name;
	}
    }
}