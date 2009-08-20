using Gtk;
using Gdk;
using System;
using System.Collections.Generic;

namespace Weland {
    // resizable, scrollable map view
    public class MapWindow : Gtk.Window {
	public Level Level {
	    get { return drawingArea.Level; }
	    set { 
		drawingArea.Level = value;
		editor.Level = value;
	    }
	}

	Wadfile wadfile = new Wadfile();
	Editor editor = new Editor();

	MenuItem levelMenu;
	MenuItem fileItem;
	MenuBar menuBar;

	Toolbar toolbar;

	string Filename;

	void BuildToolbar() {
	    toolbar = new Toolbar();
	    toolbar.Orientation = Orientation.Horizontal;
	    toolbar.ToolbarStyle = ToolbarStyle.Icons;

	    ToolButton newButton = new ToolButton(Stock.New);
	    newButton.Clicked += new EventHandler(NewLevel);
	    newButton.TooltipText = "New Level";
	    toolbar.Insert(newButton, -1);

	    ToolButton openButton = new ToolButton(Stock.Open);
	    openButton.Clicked += new EventHandler(OpenFile);
	    openButton.TooltipText = "Open";
	    toolbar.Insert(openButton, -1);

	    ToolButton saveButton = new ToolButton(Stock.Save);
	    saveButton.Clicked += new EventHandler(Save);
	    saveButton.TooltipText = "Save";
	    toolbar.Insert(saveButton, -1);

	    toolbar.Insert(new SeparatorToolItem(), -1);

	    RadioToolButton zoomButton = new RadioToolButton(new GLib.SList(IntPtr.Zero));
	    zoomButton.IconWidget = new Gtk.Image(null, "zoom.png");
	    zoomButton.Clicked += new EventHandler(delegate(object obj, EventArgs args) { ChooseTool(Tool.Zoom); });
	    toolbar.Insert(zoomButton, -1);

	    RadioToolButton moveButton = new RadioToolButton(zoomButton);
	    moveButton.IconWidget = new Gtk.Image(null, "move.png");
	    moveButton.Clicked += new EventHandler(delegate(object obj, EventArgs args) { ChooseTool(Tool.Move); });
	    toolbar.Insert(moveButton, -1);

	    RadioToolButton lineButton = new RadioToolButton(zoomButton);
	    lineButton.IconWidget = new Gtk.Image(null, "line.png");
	    lineButton.Clicked += new EventHandler(delegate(object obj, EventArgs args) { ChooseTool(Tool.Line); });
	    toolbar.Insert(lineButton, -1);

	    toolbar.Insert(new SeparatorToolItem(), -1);
	    
	    ToggleToolButton showGridButton = new ToggleToolButton();
	    showGridButton.IconWidget = new Gtk.Image(null, "grid.png");
	    showGridButton.Active = true;
	    showGridButton.Toggled += new EventHandler(delegate(object obj, EventArgs args) { drawingArea.ShowGrid = ((ToggleToolButton) obj).Active; Redraw(); });
	    showGridButton.TooltipText = "Show Grid";

	    toolbar.Insert(showGridButton, -1);

	    ToggleToolButton showMonstersButton = new ToggleToolButton();
	    showMonstersButton.IconWidget = new Gtk.Image(null, "monster.png");
	    showMonstersButton.Active = true;
	    showMonstersButton.Toggled += new EventHandler(delegate(object obj, EventArgs args) { drawingArea.ShowMonsters = ((ToggleToolButton) obj).Active; Redraw(); });
	    showMonstersButton.TooltipText = "Show Monsters";
	    toolbar.Insert(showMonstersButton, -1);

	    ToggleToolButton showObjectsButton = new ToggleToolButton();
	    showObjectsButton.IconWidget = new Gtk.Image(null, "pistol-ammo.png");
	    showObjectsButton.Active = true;
	    showObjectsButton.Toggled += new EventHandler(delegate(object obj, EventArgs args) { drawingArea.ShowObjects = ((ToggleToolButton) obj).Active; Redraw(); });
	    showObjectsButton.TooltipText = "Show Objects";
	    toolbar.Insert(showObjectsButton, -1);

	    ToggleToolButton showSceneryButton = new ToggleToolButton();
	    showSceneryButton.IconWidget = new Gtk.Image(null, "flower.png");
	    showSceneryButton.Active = true;
	    showSceneryButton.Toggled += new EventHandler(delegate(object obj, EventArgs args) { drawingArea.ShowScenery = ((ToggleToolButton) obj).Active; Redraw(); });
	    showSceneryButton.TooltipText = "Show Scenery";
	    toolbar.Insert(showSceneryButton, -1);

	    ToggleToolButton showPlayersButton = new ToggleToolButton();
	    showPlayersButton.IconWidget = new Gtk.Image(null, "player.png");
	    showPlayersButton.Active = true;
	    showPlayersButton.Toggled += new EventHandler(delegate(object obj, EventArgs args) { drawingArea.ShowPlayers = ((ToggleToolButton) obj).Active; Redraw(); });
	    showPlayersButton.TooltipText = "Show Players";
	    toolbar.Insert(showPlayersButton, -1);

	    ToggleToolButton showGoalsButton = new ToggleToolButton();
	    showGoalsButton.IconWidget = new Gtk.Image(null, "flag.png");
	    showGoalsButton.Active = true;
	    showGoalsButton.Toggled += new EventHandler(delegate(object obj, EventArgs args) { drawingArea.ShowGoals = ((ToggleToolButton) obj).Active; Redraw(); });
	    showGoalsButton.TooltipText = "Show Goals";
	    toolbar.Insert(showGoalsButton, -1);

	    ToggleToolButton showSoundsButton = new ToggleToolButton();
	    showSoundsButton.IconWidget = new Gtk.Image(null, "sound.png");
	    showSoundsButton.Active = true;
	    showSoundsButton.Toggled += new EventHandler(delegate(object obj, EventArgs args) { drawingArea.ShowSounds = ((ToggleToolButton) obj).Active; Redraw(); });
	    showSoundsButton.TooltipText = "Show Sounds";
	    toolbar.Insert(showSoundsButton, -1);

	    toolbar.Insert(new SeparatorToolItem(), -1);

	    RadioToolButton grid2048Button = new RadioToolButton((new GLib.SList(IntPtr.Zero)));
	    grid2048Button.Label = "2";
	    grid2048Button.Toggled += new EventHandler(delegate(object obj, EventArgs args) { drawingArea.GridResolution = 2048; Redraw(); });
	    grid2048Button.TooltipText = "2 World";

	    toolbar.Insert(grid2048Button, -1);

	    RadioToolButton grid1024Button = new RadioToolButton(grid2048Button);
	    grid1024Button.Label = "1";
	    grid1024Button.Toggled += new EventHandler(delegate(object obj, EventArgs args) { drawingArea.GridResolution = 1024; Redraw(); });
	    grid1024Button.TooltipText = "World";
	    
	    toolbar.Insert(grid1024Button, -1);

	    RadioToolButton grid512Button = new RadioToolButton(grid2048Button);
	    grid512Button.Label = "1/2";
	    grid512Button.Toggled += new EventHandler(delegate(object obj, EventArgs args) { drawingArea.GridResolution = 512; Redraw(); });
	    grid512Button.TooltipText = "1/2 World";
	    
	    toolbar.Insert(grid512Button, -1);

	    RadioToolButton grid256Button = new RadioToolButton(grid2048Button);
	    grid256Button.Label = "1/4";
	    grid256Button.Toggled += new EventHandler(delegate(object obj, EventArgs args) { drawingArea.GridResolution = 256; Redraw(); });
	    grid256Button.TooltipText = "1/4 World";
	    
	    toolbar.Insert(grid256Button, -1);

	    RadioToolButton grid128Button = new RadioToolButton(grid2048Button);
	    grid128Button.Label = "1/8";
	    grid128Button.Toggled += new EventHandler(delegate(object obj, EventArgs args) { drawingArea.GridResolution = 128; Redraw(); });
	    grid128Button.TooltipText = "1/8 World";
	    
	    toolbar.Insert(grid128Button, -1);

	    grid1024Button.Active = true;
	}

	void BuildMenubar() {
	    AccelGroup agr = new AccelGroup();
	    AddAccelGroup(agr);

	    menuBar = new MenuBar();
	    Menu fileMenu = new Menu();

	    ImageMenuItem newItem = new ImageMenuItem(Stock.New, agr);
	    newItem.Activated += new EventHandler(NewLevel);
	    fileMenu.Append(newItem);

	    ImageMenuItem openItem = new ImageMenuItem(Stock.Open, agr);
	    openItem.Activated += new EventHandler(OpenFile);
	    fileMenu.Append(openItem);

	    ImageMenuItem saveItem = new ImageMenuItem(Stock.Save, agr);
	    saveItem.Activated += new EventHandler(Save);
	    fileMenu.Append(saveItem);

	    ImageMenuItem saveAsItem = new ImageMenuItem(Stock.SaveAs, agr);
	    saveAsItem.Activated += new EventHandler(SaveAs);
	    fileMenu.Append(saveAsItem);

	    fileMenu.Append(new SeparatorMenuItem());
	    
	    ImageMenuItem exitItem = new ImageMenuItem(Stock.Quit, agr);
	    exitItem.Activated += new EventHandler(
						   delegate(object obj, EventArgs a) {
						       Application.Quit();
						   });
	    fileMenu.Append(exitItem);
	    fileItem = new MenuItem("File");
	    fileItem.Submenu = fileMenu;
	    menuBar.Append(fileItem);

	    Menu editMenu = new Menu();
	    ImageMenuItem undoItem = new ImageMenuItem(Stock.Undo, agr);
	    undoItem.AddAccelerator("activate", agr, new AccelKey(Gdk.Key.z, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
	    undoItem.Activated += new EventHandler(delegate(object obj, EventArgs a) { editor.Undo(); Redraw(); });
	    editMenu.Append(undoItem);

	    MenuItem editItem = new MenuItem("Edit");
	    editItem.Submenu = editMenu;
	    
	    menuBar.Append(editItem);

	    levelMenu = new MenuItem("Level");
	    menuBar.Append(levelMenu);
	}

	public MapWindow(string title) : base(title) {
	    AllowShrink = true;
	    ScrollEvent += new ScrollEventHandler(Scroll);
			
	    Table table = new Table(2, 2, false);

	    drawingArea.ConfigureEvent += new ConfigureEventHandler(ConfigureDrawingArea);
	    drawingArea.ButtonPressEvent += new ButtonPressEventHandler(ButtonPress);
	    drawingArea.ButtonReleaseEvent += new ButtonReleaseEventHandler(ButtonRelease);
	    drawingArea.MotionNotifyEvent += new MotionNotifyEventHandler(Motion);
	    drawingArea.Events = EventMask.ExposureMask | EventMask.ButtonPressMask | EventMask.ButtonReleaseMask | EventMask.ButtonMotionMask;
	    drawingArea.SetSizeRequest(600, 400);

	    hadjust = new Adjustment(0, 0, 0, 0, 0, 0);
	    vadjust = new Adjustment(0, 0, 0, 0, 0, 0);
			
	    hscroll = new HScrollbar(hadjust);
	    vscroll = new VScrollbar(vadjust);

	    hscroll.ValueChanged += new EventHandler(HValueChangedEvent);
	    vscroll.ValueChanged += new EventHandler(VValueChangedEvent);
	    table.Attach(drawingArea, 0, 1, 0, 1, AttachOptions.Shrink | AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Shrink | AttachOptions.Expand | AttachOptions.Fill, 0, 0);
	    table.Attach(hscroll, 0, 1, 1, 2, AttachOptions.Shrink | AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
	    table.Attach(vscroll, 1, 2, 0, 1, 0, AttachOptions.Shrink | AttachOptions.Expand | AttachOptions.Fill, 0, 0);
	    
	    VBox box = new VBox();

	    BuildMenubar();
	    box.PackStart(menuBar, false, false, 0);

	    BuildToolbar();
	    box.PackStart(toolbar, false, false, 0);

	    box.Add(table);
		
	    Add(box);

	    editor.Tool = Tool.Zoom;
	}

	MapDrawingArea drawingArea = new MapDrawingArea();
	HScrollbar hscroll;
	VScrollbar vscroll;

	Adjustment hadjust;
	Adjustment vadjust;

	public void Center(short X, short Y) {
	    drawingArea.Center(X, Y);
	    vscroll.Value = drawingArea.Transform.YOffset;
	    hscroll.Value = drawingArea.Transform.XOffset;
	}
	
	void AdjustScrollRange() {
	    int width, height;
	    width = drawingArea.Allocation.Width;
	    height = drawingArea.Allocation.Height;
	    double scale = drawingArea.Transform.Scale;
	    double VUpper = short.MaxValue - height / scale;
	    if (VUpper < short.MinValue) 
		VUpper = short.MinValue;
			
	    vadjust.Lower = short.MinValue;
	    vadjust.Upper = VUpper;
	    if (drawingArea.Transform.YOffset > VUpper) {
		vscroll.Value = VUpper;
		drawingArea.Transform.YOffset = (short) VUpper;
	    }

	    vadjust.StepIncrement = 32.0 / scale;
	    vadjust.PageIncrement = 256.0 / scale;

	    hadjust.Lower = short.MinValue;
	    double HUpper = short.MaxValue - width / scale;
	    if (HUpper < short.MinValue) 
		HUpper = short.MinValue;
	    hadjust.Upper = HUpper;
	    if (drawingArea.Transform.XOffset > HUpper) {
		hscroll.Value = HUpper;
		drawingArea.Transform.XOffset = (short) HUpper;
	    }

	    hadjust.StepIncrement = 32.0 / scale;
	    hadjust.PageIncrement = 256.0 / scale;

	    Redraw();
	}

	void HValueChangedEvent(object obj, EventArgs e) {
	    drawingArea.Transform.XOffset = (short) hscroll.Value;
	    Redraw();
	}

	void VValueChangedEvent(object obj, EventArgs e) {
	    drawingArea.Transform.YOffset = (short) vscroll.Value;
	    Redraw();
	}

	void ConfigureDrawingArea(object obj, ConfigureEventArgs args) {
	    AdjustScrollRange();
	    args.RetVal = true;
	}

	void Redraw() {
	    drawingArea.QueueDrawArea(0, 0, drawingArea.Allocation.Width, drawingArea.Allocation.Height);
	    editor.ClearDirty();
	}

	void RedrawDirty() {
	    if (editor.Dirty) {
		int X1 = (int) drawingArea.Transform.ToScreenX(editor.RedrawLeft) - 1;
		int Y1 = (int) drawingArea.Transform.ToScreenY(editor.RedrawTop) - 1;
		int X2 = (int) drawingArea.Transform.ToScreenX(editor.RedrawRight) + 1;
		int Y2 = (int) drawingArea.Transform.ToScreenY(editor.RedrawBottom) + 1;
		drawingArea.QueueDrawArea(X1, Y1, X2 -X1, Y2 - Y1);
		editor.ClearDirty();
	    }
	}

	void Scroll(object obj, ScrollEventArgs args) {
		if (args.Event.Direction == ScrollDirection.Down) {
			vscroll.Value += 32.0 / drawingArea.Transform.Scale;
		} else if (args.Event.Direction == ScrollDirection.Up) {
			vscroll.Value -= 32.0 / drawingArea.Transform.Scale;
		} else if (args.Event.Direction == ScrollDirection.Right) {
			hscroll.Value += 32.0 / drawingArea.Transform.Scale;
		} else if (args.Event.Direction == ScrollDirection.Left) {
			hscroll.Value -= 32.0 / drawingArea.Transform.Scale;
		}
		args.RetVal = true;
	}

	double xDown;
	double yDown;
	short xOffsetDown;
	short yOffsetDown;
	
	void ButtonPress(object obj, ButtonPressEventArgs args) {
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

	void ButtonRelease(object obj, ButtonReleaseEventArgs args) {
	    editor.ButtonRelease(drawingArea.Transform.ToMapX(args.Event.X), drawingArea.Transform.ToMapY(args.Event.Y));
	    Redraw();
	    args.RetVal = true;
	}

	void Motion(object obj, MotionNotifyEventArgs args) {
	    if (editor.Tool == Tool.Move) {
		hscroll.Value = xOffsetDown + (xDown - args.Event.X) / drawingArea.Transform.Scale;
		vscroll.Value = yOffsetDown + (yDown - args.Event.Y) / drawingArea.Transform.Scale;
		args.RetVal = true;
	    } else {
		editor.Motion(drawingArea.Transform.ToMapX(args.Event.X), drawingArea.Transform.ToMapY(args.Event.Y));
		RedrawDirty();
	    }
	}

	public bool CheckSave() {
	    if (editor.Changed) {
		MessageDialog dialog = new MessageDialog(this, DialogFlags.DestroyWithParent, MessageType.Warning, ButtonsType.YesNo, "Do you wish to save changes?");
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
		Title = wadfile.Directory[n].LevelName;
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
			item.Activated += new EventHandler(delegate(object obj, EventArgs args) { SelectLevel(levelNumber); });
			menu.Append(item);
		    }
		}
		menu.ShowAll();
		levelMenu.Submenu = menu;
		editor.Changed = false;
		SelectLevel(0);
	    }
	    catch (Wadfile.BadMapException e) {
		MessageDialog dialog = new MessageDialog(this, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, e.Message);
		dialog.Run();
		dialog.Destroy();
	    }
	}

	void OpenFile(object obj, EventArgs args) {
	    if (CheckSave()) {
		FileChooserDialog d = new FileChooserDialog("Choose the file to open", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
		if (d.Run() == (int) ResponseType.Accept) {
		    OpenFile(d.Filename);
		}
		d.Destroy();
	    }
	}

	public void NewLevel() {
	    wadfile = new Wadfile();
	    Level = new Level();
	    levelMenu.Submenu = null;
	    drawingArea.Transform = new Transform();
	    editor.Snap = (short) (8 / drawingArea.Transform.Scale);
	    Center(0, 0);
	    AdjustScrollRange();
	    Title = "Untitled Level";
	    Filename = "";
	}

	void NewLevel(object obj, EventArgs args) {
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
	    FileChooserDialog d = new FileChooserDialog(message, this, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Save", ResponseType.Accept);
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

	void SaveAs(object obj, EventArgs args) {
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

	void Save(object obj, EventArgs args) {
	    Save();
	}

	void ChooseTool(Tool tool) {
	    editor.Tool = tool;
	    if (tool == Tool.Zoom) {
		drawingArea.GdkWindow.Cursor = new Cursor(CursorType.Target);
	    } else if (tool == Tool.Move) {
		drawingArea.GdkWindow.Cursor = new Cursor(CursorType.Fleur);
	    } else if (tool == Tool.Line) {
		drawingArea.GdkWindow.Cursor = new Cursor(CursorType.Cross);
	    } else {
		drawingArea.GdkWindow.Cursor = null;
	    }
	}
    }
}