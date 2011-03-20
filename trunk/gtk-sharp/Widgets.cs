using Gtk;
using Gdk;
using Glade;
using System;
using System.IO;

namespace Weland {
    public class HSV {
	static byte Component(double v) {
	    int b = (int) (v * 255);
	    if (b < 0) b = 0;
	    if (b > 255) b = 255;
	    return (byte) b;
	}

	// h [0, 360) s [0,1] v [0,1]
	public static Gdk.Color ToRGB(double h, double s, double v) {
	    double r = 0;
	    double g = 0;
	    double b = 0;
	    if (s == 0) {
		r = g = b = v;
	    } else {
		h = h / 60;
		int i = (int) Math.Floor(h);
		double f = h - i;
		double p = v * (1 - s);
		double q = v * (1 - f * s);
		double t = v * (1 - (1 - f) * s);
		switch (i) {
		case 0:
		    r = v;
		    g = t;
		    b = p;
		    break;
		case 1:
		    r = q;
		    g = v;
		    b = p;
		    break;
		case 2:
		    r = p;
		    g = v;
		    b = t;
		    break;
		case 3:
		    r = p;
		    g = q;
		    b = v;
		    break;
		case 4:
		    r = t;
		    g = p;
		    b = v;
		    break;
		case 5:
		    r = v;
		    g = p;
		    b = q;
		    break;
		}
	    }

	    return new Gdk.Color(Component(r), Component(g), Component(b));
	}
    }

    // draws a flat color
    public class ColorRadioButton : RadioButton {
	const int margin = 4;
	static readonly Gdk.Color labelColor = new Gdk.Color(0xff, 0xff, 0xff);
	static readonly Gdk.Color boxColor = new Gdk.Color(0x0, 0x0, 0x0);
	Gdk.Color color;

	public int Index;

	public event EventHandler DoubleClicked;

	public ColorRadioButton(ColorRadioButton other, string label, Gdk.Color c) : base(other, label) {
	    DrawIndicator = false;
	    color = c;
	    Child.ModifyFg(StateType.Normal, labelColor);
	    Child.ModifyFg(StateType.Active, labelColor);
	    Child.ModifyFg(StateType.Prelight, labelColor);
	}

	protected override bool OnExposeEvent(Gdk.EventExpose args) {
	    Gdk.Window win = GdkWindow;
	    Gdk.GC g = new Gdk.GC(win);
	    g.RgbFgColor = color;
	    win.DrawRectangle(g, true, Allocation.X + margin, Allocation.Y + margin, Allocation.Width - margin * 2, Allocation.Height - margin * 2);

	    if (Active || State == StateType.Active) {
		g.RgbFgColor = boxColor;
		win.DrawRectangle(g, false, Allocation.X + 1, Allocation.Y + 1, Allocation.Width - 3, Allocation.Height - 3);
	    }

	    PropagateExpose(Child, args);

	    return false;
	}

	protected override void OnSizeAllocated(Gdk.Rectangle rect) {
	    base.OnSizeAllocated(rect);
	    rect.X += margin;
	    rect.Y += margin;
	    rect.Width -= margin * 2;
	    rect.Height -= margin * 2;
	    Child.SizeAllocate(rect);
	}

	protected override bool OnButtonPressEvent(Gdk.EventButton e) {
	    if (e.Type == EventType.TwoButtonPress) {
		if (DoubleClicked != null) {
		    this.DoubleClicked(this, new EventArgs());
		}
		return true;
	    } else {
		return base.OnButtonPressEvent(e);
	    }
	}
    }

    public class EntryDialog : Dialog { 
	Entry entry = new Entry();

	public EntryDialog(string title, Gtk.Window w) : base(title, w, DialogFlags.Modal | DialogFlags.DestroyWithParent) {
	    Resizable = false;
	    entry.Visible = true;
	    entry.Activated += OnEntryActivated;
	    VBox.Add(entry);
	    
	    AddActionWidget(new Button(Stock.Cancel), ResponseType.Cancel);
	    
	    Button ok = new Button(Stock.Ok);
	    ok.CanDefault = true;
	    AddActionWidget(ok, ResponseType.Ok);
	    ok.GrabDefault();

	    ShowAll();
	}

	public string Text {
	    get { return entry.Text; }
	    set { 
		entry.Text = value;
		entry.SelectRegion(0, -1);
	    }
	}

	void OnEntryActivated(object obj, EventArgs args) {
	    Respond(ResponseType.Ok);
	}
    }

    public class DoubleDialog : Dialog {
	Entry entry = new Entry();
	
	public DoubleDialog(string title, Gtk.Window w) : base(title, w, DialogFlags.Modal | DialogFlags.DestroyWithParent) {
	    Resizable = false;
	    entry.Visible = true;
	    entry.Activated += OnEntryActivated;
	    VBox.Add(entry);
	    
	    AddActionWidget(new Button(Stock.Cancel), ResponseType.Cancel);

	    Button ok = new Button(Stock.Ok);
	    ok.CanDefault = true;
	    AddActionWidget(ok, ResponseType.Ok);
	    ok.GrabDefault();

	    ShowAll();
	}

	public bool Valid {
	    get {
		double d;
		return double.TryParse(entry.Text, out d);
	    }
	}

	public double Value {
	    get {
		return double.Parse(entry.Text);
	    }
	    
	    set {
		entry.Text = String.Format("{0:0.000}", value);
		entry.SelectRegion(0, -1);
	    }
	}

	void OnEntryActivated(object obj, EventArgs args) {
	    Respond(ResponseType.Ok);
	}
    }
    
    public class IntDialog : Dialog {
	Entry entry = new Entry();
	
	public IntDialog(string title, Gtk.Window w) : base(title, w, DialogFlags.Modal | DialogFlags.DestroyWithParent) {
	    Resizable = false;
	    entry.Visible = true;
	    entry.Activated += OnEntryActivated;
	    VBox.Add(entry);
	    
	    AddActionWidget(new Button(Stock.Cancel), ResponseType.Cancel);

	    Button ok = new Button(Stock.Ok);
	    ok.CanDefault = true;
	    AddActionWidget(ok, ResponseType.Ok);
	    ok.GrabDefault();

	    ShowAll();
	}

	public bool Valid {
	    get {
		int d;
		return int.TryParse(entry.Text, out d);
	    }
	}

	public int Value {
	    get {
		return int.Parse(entry.Text);
	    }
	    
	    set {
		entry.Text = String.Format("{0}", value);
		entry.SelectRegion(0, -1);
	    }
	}

	void OnEntryActivated(object obj, EventArgs args) {
	    Respond(ResponseType.Ok);
	}
    }

    public class PointDialog {
	public PointDialog(Gtk.Window w) {
	    Glade.XML gxml = new Glade.XML(null, "pointparameters.glade", "dialog1", null);
	    gxml.Autoconnect(this);
	    dialog1.TransientFor = w;
	}

	public void Run() {
	    dialog1.Run();

	    short i;
	    if (short.TryParse(pointX.Text, out i)) {
		p.X = i;
	    }

	    if (short.TryParse(pointY.Text, out i)) {
		p.Y = i;
	    }
	    dialog1.Destroy();
	}

	Point p;
	
	[Widget] Dialog dialog1;
	[Widget] Entry pointX;
	[Widget] Entry pointY;

	public Point Value {
	    get {
		return p;
	    }

	    set {
		p = value;
		pointX.Text = String.Format("{0}", p.X);
		pointY.Text = String.Format("{0}", p.Y);
	    }
	}
    }

    public class LogWindow : Gtk.Window {
	public LogWindow(string title, string contents) : base(title) {
	    this.contents = contents;
	    
	    SetDefaultSize(640, 480);

	    VBox vbox = new VBox(false, 2);
	    vbox.BorderWidth = 5;

	    ScrolledWindow scrolledWindow = new ScrolledWindow();

	    TextView textView = new TextView();
	    textView.Buffer.Text = contents;
	    textView.Editable = false;
	    
	    scrolledWindow.Add(textView);
	    scrolledWindow.ShowAll();
	    
	    HButtonBox buttonBox = new HButtonBox();
	    buttonBox.Layout = ButtonBoxStyle.End;
	    buttonBox.Spacing = 5;
	    
	    Button saveAs = new Button(Stock.SaveAs);
	    saveAs.Clicked += delegate(object obj, EventArgs args) {
		this.SaveAs();
	    };

	    buttonBox.Add(saveAs);

	    Button close = new Button(Stock.Close);
	    close.CanDefault = true;
	    close.Clicked += delegate(object obj, EventArgs args) {
		this.Close();
	    };

	    buttonBox.Add(close);

	    vbox.PackStart(scrolledWindow, true, true, 5);
	    vbox.PackStart(buttonBox, false, false, 5);

	    Add(vbox);

	    ShowAll();

	    close.GrabDefault();
	}

	public void Close() {
	    this.Destroy(); 
	}

	public void SaveAs() {
	    FileChooserDialog d = new FileChooserDialog("Save log as", this, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Save", ResponseType.Accept);
	    d.SetCurrentFolder(Weland.Settings.GetSetting("LastSave/Folder", Environment.GetFolderPath(Environment.SpecialFolder.Personal)));
	    d.CurrentName = "Log.txt";
	    d.DoOverwriteConfirmation = true;
	    try {
		if (d.Run() == (int) ResponseType.Accept) {
		    using (TextWriter w = new StreamWriter(d.Filename)) {
			w.Write(contents);
		    }
		    Weland.Settings.PutSetting("LastSave/Folder", System.IO.Path.GetDirectoryName(d.Filename));
		}
	    } catch (Exception e) {
		MessageDialog m = new MessageDialog(this, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, "An error occured while exporting.");
		m.Title = "Save error";
		m.SecondaryText = e.Message;
		m.Run();
		m.Destroy();	
	    }
	    d.Destroy();
	}

	string contents;
    }
}
