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

	void BuildToolbar() {
	    toolbar = new Toolbar();
	    toolbar.Orientation = Orientation.Horizontal;
	    toolbar.ToolbarStyle = ToolbarStyle.Icons;

	    ToolButton newButton = new ToolButton(Stock.New);
	    newButton.Clicked += new EventHandler(NewLevel);
	    toolbar.Insert(newButton, -1);

	    ToolButton openButton = new ToolButton(Stock.Open);
	    openButton.Clicked += new EventHandler(OpenFile);
	    toolbar.Insert(openButton, -1);

	    toolbar.Insert(new SeparatorToolItem(), -1);

	    RadioToolButton zoomButton = new RadioToolButton(new GLib.SList(IntPtr.Zero));
	    zoomButton.IconWidget = new Gtk.Image("resources/zoom.png");
	    zoomButton.Clicked += new EventHandler(delegate(object obj, EventArgs args) { ChooseTool(Tool.Zoom); });
	    toolbar.Insert(zoomButton, -1);

	    RadioToolButton moveButton = new RadioToolButton(zoomButton);
	    moveButton.IconWidget = new Gtk.Image("resources/move.png");
	    moveButton.Clicked += new EventHandler(delegate(object obj, EventArgs args) { ChooseTool(Tool.Move); });
	    toolbar.Insert(moveButton, -1);

	    RadioToolButton lineButton = new RadioToolButton(zoomButton);
	    lineButton.IconWidget = new Gtk.Image("resources/line.png");
	    lineButton.Clicked += new EventHandler(delegate(object obj, EventArgs args) { ChooseTool(Tool.Line); });
	    toolbar.Insert(lineButton, -1);
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

	    drawingArea.QueueDrawArea(0, 0, width, height);
	}

	void HValueChangedEvent(object obj, EventArgs e) {
	    drawingArea.Transform.XOffset = (short) hscroll.Value;
			
	    drawingArea.QueueDrawArea(0, 0, drawingArea.Allocation.Width, drawingArea.Allocation.Height);
	}

	void VValueChangedEvent(object obj, EventArgs e) {
	    drawingArea.Transform.YOffset = (short) vscroll.Value;

	    drawingArea.QueueDrawArea(0, 0, drawingArea.Allocation.Width, drawingArea.Allocation.Height);
	}

	void ConfigureDrawingArea(object obj, ConfigureEventArgs args) {
	    AdjustScrollRange();
	    args.RetVal = true;
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
		drawingArea.QueueDrawArea(0, 0, drawingArea.Allocation.Width, drawingArea.Allocation.Height);
	    }
	    args.RetVal = true;
	}

	void ButtonRelease(object obj, ButtonReleaseEventArgs args) {
	    editor.ButtonRelease(drawingArea.Transform.ToMapX(args.Event.X), drawingArea.Transform.ToMapY(args.Event.Y));
	    drawingArea.QueueDrawArea(0, 0, drawingArea.Allocation.Width, drawingArea.Allocation.Height);
	    args.RetVal = true;
	}

	void Motion(object obj, MotionNotifyEventArgs args) {
	    if (editor.Tool == Tool.Move) {
		hscroll.Value = xOffsetDown + (xDown - args.Event.X) / drawingArea.Transform.Scale;
		vscroll.Value = yOffsetDown + (yDown - args.Event.Y) / drawingArea.Transform.Scale;
		args.RetVal = true;
	    } else {
		editor.Motion(drawingArea.Transform.ToMapX(args.Event.X), drawingArea.Transform.ToMapY(args.Event.Y));
		drawingArea.QueueDrawArea(0, 0, drawingArea.Allocation.Width, drawingArea.Allocation.Height);
	    }
	}

	public void SelectLevel(int n) {
	    Level = new Level();
	    Level.Load(wadfile.Directory[n]);
	    drawingArea.Transform = new Transform();
	    editor.Snap = (short) (8 / drawingArea.Transform.Scale);
	    Center(0, 0);
	    AdjustScrollRange();
	    Title = wadfile.Directory[n].LevelName;
	}

	public void OpenFile(string filename) {
	    try {
		Wadfile w = new Wadfile();
		w.Load(filename);
		wadfile = w;
		Menu menu = new Menu();
		foreach (var kvp in wadfile.Directory) {
		    if (kvp.Value.Chunks.ContainsKey(Level.Tag)) {
			MenuItem item = new MenuItem(kvp.Value.LevelName);
			int levelNumber = kvp.Key;
			item.Activated += new EventHandler(delegate(object obj, EventArgs args) { SelectLevel(levelNumber); });
			menu.Append(item);
		    }
		}
		menu.ShowAll();
		levelMenu.Submenu = menu;
		SelectLevel(0);
	    }
	    catch (Wadfile.BadMapException e) {
		MessageDialog dialog = new MessageDialog(this, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, e.Message);
		dialog.Run();
		dialog.Destroy();
	    }
	}

	void OpenFile(object obj, EventArgs args) {
	    FileChooserDialog d = new FileChooserDialog("Choose the file to open", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
	    if (d.Run() == (int) ResponseType.Accept) {
		OpenFile(d.Filename);
	    }
	    d.Destroy();
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
	}

	void NewLevel(object obj, EventArgs args) {
	    NewLevel();
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