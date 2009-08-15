using Gtk;
using System;
using Cairo;

namespace Weland {
    public class Weland {
	public static int Main (string[] args) {
	    Application.Init();
	    
	    MapWindow window = new MapWindow("Weland");
	    window.DeleteEvent += new DeleteEventHandler(DeleteWindow);
	    
	    if (args.Length == 1)
		window.OpenFile(args[0]);
	    else
		window.NewLevel();
	    
	    window.ShowAll();
	    window.Focus = null;
	    Application.Run();
	    return 0;
	}

	static void DeleteWindow(object obj, DeleteEventArgs args) {
	    Application.Quit();
	    args.RetVal = true;
	}
    }
}
