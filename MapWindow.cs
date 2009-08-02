using Gtk;
using System;

namespace Weland {
    // resizable, scrollable map view
    public class MapWindow : Gtk.Window {
	public Level Level {
	    get { return drawingArea.Level; }
	    set { drawingArea.Level = value; }
	}

	Wadfile wadfile = new Wadfile();

	MenuItem levelMenu;
	MenuItem fileItem;

	public MapWindow(string title) : base(title) {
	    AllowShrink = true;
	    ScrollEvent += new ScrollEventHandler(Scroll);
			
	    Table table = new Table(2, 2, false);

	    drawingArea.ConfigureEvent += new ConfigureEventHandler(ConfigureDrawingArea);
	    drawingArea.ButtonPressEvent += new ButtonPressEventHandler(ButtonPress);
	    drawingArea.Events = Gdk.EventMask.ExposureMask | Gdk.EventMask.ButtonPressMask;
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
	    
	    AccelGroup agr = new AccelGroup();
	    AddAccelGroup(agr);

	    VBox box = new VBox();

	    MenuBar mb = new MenuBar();
	    Menu fileMenu = new Menu();
	    ImageMenuItem openItem = new ImageMenuItem(Stock.Open, agr);
	    openItem.Activated += new EventHandler(OpenFile);

	    fileMenu.Append(openItem);

	    ImageMenuItem exitItem = new ImageMenuItem(Stock.Quit, agr);
	    exitItem.Activated += new EventHandler(
		    delegate(object obj, EventArgs a) {
			    Application.Quit();
		    });
	    fileMenu.Append(exitItem);
	    fileItem = new MenuItem("File");
	    fileItem.Submenu = fileMenu;
	    mb.Append(fileItem);
	    levelMenu = new MenuItem("Level");
	    mb.Append(levelMenu);
	    box.PackStart(mb, false, false, 0);

	    Toolbar toolbar = new Toolbar();
	    toolbar.Orientation = Orientation.Vertical;
	    toolbar.ToolbarStyle = ToolbarStyle.Icons;

	    ToggleToolButton zoomButton = new ToggleToolButton();
	    zoomButton.IconWidget = new Image("resources/zoom.png");
	    zoomButton.Active = true;
	    toolbar.Insert(zoomButton, -1);
	    

	    HBox hb = new HBox();
	    hb.PackStart(toolbar, false, false, 0);
	    hb.Add(table);
	    box.Add(hb);
		
	    Add(box);

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
		if (args.Event.Direction == Gdk.ScrollDirection.Down) {
			vscroll.Value += 32.0 / drawingArea.Transform.Scale;
		} else if (args.Event.Direction == Gdk.ScrollDirection.Up) {
			vscroll.Value -= 32.0 / drawingArea.Transform.Scale;
		} else if (args.Event.Direction == Gdk.ScrollDirection.Right) {
			hscroll.Value += 32.0 / drawingArea.Transform.Scale;
		} else if (args.Event.Direction == Gdk.ScrollDirection.Left) {
			hscroll.Value -= 32.0 / drawingArea.Transform.Scale;
		}
		args.RetVal = true;
	}

	void ButtonPress(object obj, ButtonPressEventArgs args) {
	    Gdk.EventButton ev = args.Event;
	    short X = drawingArea.Transform.ToMapX(ev.X);
	    short Y = drawingArea.Transform.ToMapY(ev.Y);
	    if (ev.Button == 1) {
		drawingArea.Transform.Scale *= Math.Sqrt(2.0);
	    } else if (ev.Button == 3) {
		drawingArea.Transform.Scale /= Math.Sqrt(2.0);
	    }
	    Center(X, Y);
	    AdjustScrollRange();
	    args.RetVal = true;
	}

	public void SelectLevel(int n) {
	    Level = new Level();
	    Level.Load(wadfile.Directory[n]);
	    drawingArea.Transform = new Transform();
	    Center(0, 0);
	    AdjustScrollRange();
	    Title = wadfile.Directory[n].LevelName;
	}

	public void OpenFile(string filename) {
	    try {
		wadfile = new Wadfile();
		wadfile.Load(filename);
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

		Level = null;
		drawingArea.Transform = new Transform();
		Center(0, 0);
		AdjustScrollRange();
		Title = "Weland";
		levelMenu.Submenu = null;
	    }
	}

	void OpenFile(object obj, EventArgs args) {
	    FileChooserDialog d = new FileChooserDialog("Choose the file to open", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
	    if (d.Run() == (int) ResponseType.Accept) {
		OpenFile(d.Filename);
	    }
	    d.Destroy();
	}
    }
}