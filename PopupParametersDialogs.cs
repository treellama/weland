using Gtk;
using Glade;
using System;

namespace Weland {
    public class LineParametersDialog {
	public LineParametersDialog(Window parent, Level theLevel, Line theLine) {
	    level = theLevel;
	    line = theLine;
	    Glade.XML gxml = new Glade.XML(null, "lineparameters.glade", "dialog1", null);
	    gxml.Autoconnect(this);
	    dialog1.TransientFor = parent;
	}

	public void Run() {
	    solid.Active = line.Solid;
	    transparent.Active = line.Transparent;
	    dialog1.Run();
	    line.Solid = solid.Active;
	    line.Transparent = transparent.Active;
	    dialog1.Destroy();
	}

	protected void OnRemoveTextures(object obj, EventArgs args) {
	    if (line.ClockwisePolygonSideIndex != -1) {
		level.DeleteSide(line.ClockwisePolygonSideIndex);
	    }
	    if (line.CounterclockwisePolygonSideIndex != -1) {
		level.DeleteSide(line.CounterclockwisePolygonSideIndex);
	    }
	}

	Level level;
	Line line;
	
	[Widget] Dialog dialog1;

	[Widget] CheckButton solid;
	[Widget] CheckButton transparent;
    }

    public class PointParametersDialog {
	public PointParametersDialog(Window parent, Level theLevel, short theIndex) {
	    level = theLevel;
	    index = theIndex;
	    Glade.XML gxml = new Glade.XML(null, "pointparameters.glade", "dialog1", null);
	    gxml.Autoconnect(this);
	    dialog1.TransientFor = parent;
	}

	public void Run() {
	    Point p = level.Endpoints[index];
	    pointX.Text = String.Format("{0}", p.X);
	    pointY.Text = String.Format("{0}", p.Y);
	    dialog1.Run();
	    
	    Point n = p;
	    short i;
	    if (short.TryParse(pointX.Text, out i)) {
		n.X = i;
	    }
	    if (short.TryParse(pointY.Text, out i)) {
		n.Y = i;
	    }
	    level.Endpoints[index] = n;
	    dialog1.Destroy();
	}

	Level level;
	short index;

	[Widget] Dialog dialog1;

	[Widget] Entry pointX;
	[Widget] Entry pointY;
    }
}