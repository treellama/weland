using Gtk;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Weland {
    // from Mono.TextEditor.Platform
    public static class PlatformDetection {
	static PlatformDetection() {
	    IsWindows = System.IO.Path.DirectorySeparatorChar == '\\';
	    IsMac = !IsWindows && IsRunningOnMac();
	    IsX11 = !IsMac && System.Environment.OSVersion.Platform == PlatformID.Unix;
	}

	public static bool IsMac { get; private set; }
	public static bool IsX11 { get; private set; }
	public static bool IsWindows { get; private set; }
	
	//From Managed.Windows.Forms/XplatUI
	static bool IsRunningOnMac ()
	{
	    IntPtr buf = IntPtr.Zero;
	    try {
		buf = Marshal.AllocHGlobal (8192);
		// This is a hacktastic way of getting sysname from uname ()
		if (uname (buf) == 0) {
		    string os = Marshal.PtrToStringAnsi (buf);
		    if (os == "Darwin")
			return true;
		}
	    } catch {
	    } finally {
		if (buf != IntPtr.Zero)
		    Marshal.FreeHGlobal (buf);
	    }
	    return false;
	}
	
	[DllImport ("libc")]
        static extern int uname (IntPtr buf);
    }

    public class Weland {
	public static Settings Settings = new Settings();

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
