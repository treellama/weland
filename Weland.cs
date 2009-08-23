using Gtk;
using System;

namespace Weland {
    public class Weland {
	public static int Main (string[] args) {
	    Application.Init();
	    
	    MapWindow window = new MapWindow("Weland");
	    
	    if (args.Length == 1)
		window.OpenFile(args[0]);
	    else
		window.NewLevel();
	    
	    Application.Run();
	    return 0;
	}
    }
}
