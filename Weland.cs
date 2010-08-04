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

    public delegate void ShapesFileChangedEventHandler();

    public class Weland {
	public static Settings Settings = new Settings();

	static ShapesFile shapes;

	public static event ShapesFileChangedEventHandler ShapesChanged;

	public static ShapesFile Shapes {
	    get { return shapes; }
	    set {
		bool changed = (shapes != value);
		shapes = value;
		if (changed && ShapesChanged != null) {
		    ShapesChanged();
		}
	    }
	}

	static void OnUnhandledException(Exception outer) {
	    Exception e;
	    if (outer.InnerException != null) {
		e = outer.InnerException;
	    } else {
		e = outer;
	    }

	    MessageDialog d = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, e.Message);
	    d.Title = "Unhandled Exception";
	    d.SecondaryText = e.StackTrace;
	    d.Run();
	    d.Destroy();		
	}

	public static void OnUnhandledException(object o, UnhandledExceptionEventArgs args) {
	    Exception e = (Exception) args.ExceptionObject;
	    OnUnhandledException(e);
	}

	public static void OnUnhandledException(GLib.UnhandledExceptionArgs args) {
	    Exception e = (Exception) args.ExceptionObject;
	    OnUnhandledException(e);
	    args.ExitApplication = true;
	}

	public static int Main (string[] args) {
	    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);
	    GLib.ExceptionManager.UnhandledException += new GLib.UnhandledExceptionHandler(OnUnhandledException);
	    Application.Init();
	    
	    ShapesFile shapes = new ShapesFile();
	    shapes.Load(Settings.GetSetting("ShapesFile/Path", ""));
	    Shapes = shapes;
	    MapWindow window = new MapWindow("Weland");

	    if (args.Length == 1 && !args[0].StartsWith("-psn_"))
		window.OpenFile(args[0]);
	    else
		window.NewLevel();
	    
	    Application.Run();
	    return 0;
	}
    }
}
