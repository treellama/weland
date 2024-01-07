using Gtk;
using System;
using System.Text;

namespace Weland
{
    public static class PlatformDetection
    {
        public static bool IsMac => OperatingSystem.IsMacOS();
        public static bool IsX11 => Environment.OSVersion.Platform == PlatformID.Unix && !IsMac;
        public static bool IsWindows => OperatingSystem.IsWindows();
    }

    public delegate void ShapesFileChangedEventHandler();

    public class Weland
    {
        public static Settings Settings = new Settings();

        static ShapesFile shapes;
        public static Plugins plugins = new Plugins();

        public static event ShapesFileChangedEventHandler ShapesChanged;

        public static ShapesFile Shapes
        {
            get { return shapes; }
            set
            {
                bool changed = (shapes != value);
                shapes = value;
                if (changed && ShapesChanged != null)
                {
                    ShapesChanged();
                }
            }
        }

        static void OnUnhandledException(Exception outer)
        {
            Exception e;
            if (outer.InnerException != null)
            {
                e = outer.InnerException;
            }
            else
            {
                e = outer;
            }

            using var d = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, e.Message);
            d.Title = "Unhandled Exception";
            d.SecondaryText = e.StackTrace;
            d.Run();
        }

        public static void OnUnhandledException(object o, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            OnUnhandledException(e);
        }

        public static void OnUnhandledException(GLib.UnhandledExceptionArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            OnUnhandledException(e);
            args.ExitApplication = true;
        }

        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);
            GLib.ExceptionManager.UnhandledException += new GLib.UnhandledExceptionHandler(OnUnhandledException);

            Application.Init();

            Gtk.Settings.Default.ApplicationPreferDarkTheme = true;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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
