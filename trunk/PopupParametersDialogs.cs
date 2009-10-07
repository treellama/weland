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
}