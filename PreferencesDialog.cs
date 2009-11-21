using Glade;
using Gtk;
using System;

namespace Weland {
    public class PreferencesDialog {
	public PreferencesDialog(Window parent) {
	    Glade.XML gxml = new Glade.XML(null, "preferences.glade", "dialog1", null);
	    gxml.Autoconnect(this);
	    dialog1.TransientFor = parent;
	}
    
	public void Run() {
	    antialias.Active = Weland.Settings.GetSetting("Drawer/SmoothLines", true);
	    dialog1.ShowAll();
	    dialog1.Show();
	    dialog1.Run();
	    Weland.Settings.PutSetting("Drawer/SmoothLines", antialias.Active);
	    dialog1.Destroy();
	}

	[Widget] Dialog dialog1;

	[Widget] ToggleButton antialias;
    }
}