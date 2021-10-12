using Gtk;
using System;
using System.IO;
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
	public static Plugins plugins = new Plugins();

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
            if (PlatformDetection.IsWindows)
            {
                CheckWindowsGtk();
            }
            
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

        // gtk-sharp doesn't always load correctly from the GAC; this code
        // copied from MonoDevelop looks for a registry key the gtk-sharp
        // installer sets, and uses that to load the DLLs from the right place
        static bool CheckWindowsGtk()
        {
            string location = null;
            Version version = null;
            Version minVersion = new Version(2, 12, 22);
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Xamarin\GtkSharp\InstallFolder"))
            {
                if (key != null)
                    location = key.GetValue(null) as string;
            }
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Xamarin\GtkSharp\Version"))
            {
                if (key != null)
                    Version.TryParse(key.GetValue(null) as string, out version);
            }
            //TODO: check build version of GTK# dlls in GAC
            if (version == null || version < minVersion || location == null || !File.Exists(Path.Combine(location, "bin", "libgtk-win32-2.0-0.dll")))
            {
                Console.WriteLine("Did not find required GTK# installation");
                //  string url = "http://monodevelop.com/Download";
                //  string caption = "Fatal Error";
                //  string message =
                //      "{0} did not find the required version of GTK#. Please click OK to open the download page, where " +
                //      "you can download and install the latest version.";
                //  if (DisplayWindowsOkCancelMessage (
                //      string.Format (message, BrandingService.ApplicationName, url), caption)
                //  ) {
                //      Process.Start (url);
                //  }
                return false;
            }
            Console.WriteLine("Found GTK# version " + version);
            var path = Path.Combine(location, @"bin");
            Console.WriteLine("SetDllDirectory(\"{0}\") ", path);
            try
            {
                if (SetDllDirectory(path))
                {
                    return true;
                }
            }
            catch (EntryPointNotFoundException)
            {
            }
            // this shouldn't happen unless something is weird in Windows
            Console.WriteLine("Unable to set GTK+ dll directory");
            return true;
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool SetDllDirectory(string lpPathName);
    }
}
