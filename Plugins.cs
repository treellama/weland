using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gtk;

namespace Weland {
    public class Plugins {
	class PluginInfo {
	    public string Name;
	    public MethodInfo GtkRun;
	    public MethodInfo Run;
	};
	List<PluginInfo> plugins = new List<PluginInfo>();

	public Plugins() {
	    List<string> files = new List<string>();

	    string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	    try {
		foreach (string file in Directory.GetFiles(Path.Combine(exePath, "Plugins"), "*.dll")) {
		    files.Add(file);
		}
	    } catch { }
		
	    if (PlatformDetection.IsMac) {
		try {
		    foreach (string file in Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(exePath)), "Plugins"), "*.dll")) {
			files.Add(file);
		    }
		} catch { }
	    }

	    string applicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Weland");
	    try {
		foreach (string file in Directory.GetFiles(Path.Combine(applicationData, "Plugins"), "*.dll")) {
		    files.Add(file);
		}
	    } catch { }

	    foreach (string file in files)
	    {
		try {

		    // needs to have at least name and run or gtkrun
		    Assembly a = Assembly.LoadFrom(file);
		    
		    foreach (Type t in a.GetTypes())
		    {
			PluginInfo plugin = new PluginInfo();
			MethodInfo compatibleMethod = t.GetMethod("Compatible");
			if (compatibleMethod == null || !compatibleMethod.IsStatic || !((bool) compatibleMethod.Invoke(0, new object[0])))
			{
			    continue;
			}

			MethodInfo nameMethod = t.GetMethod("Name");
			if (nameMethod != null && nameMethod.IsStatic)
			{
			    plugin.Name = (string) nameMethod.Invoke(0, new object[0]);
			    plugin.Run = t.GetMethod("Run");
			    plugin.GtkRun = t.GetMethod("GtkRun");

			    if (plugin.Run != null && !plugin.Run.IsStatic)
			    {
				plugin.Run = null;
			    }

			    if (plugin.GtkRun != null && !plugin.GtkRun.IsStatic)
			    {
				plugin.GtkRun = null;
			    }

			    if (plugin.Run != null || plugin.GtkRun != null)
			    {
				plugins.Add(plugin);
				break;
			    }
			}
		    }
		} catch {
		    continue;
		}
	    }
	}

	public int Length {
	    get {
		return plugins.Count; 
	    }
	}

	public string GetName(int plugin) {
	    return plugins[plugin].Name;
	}

	public void GtkRun(Editor editor, int plugin) {
	    try 
	    {
		object[] args = { editor };
		if (plugins[plugin].GtkRun != null) {
		    plugins[plugin].GtkRun.Invoke(0, args);
		} else if (plugins[plugin].Run != null) {
		    StringWriter log = new StringWriter();
		    Console.SetOut(log);

		    plugins[plugin].Run.Invoke(0, args);
		    
		    StreamWriter stdout = new StreamWriter(Console.OpenStandardOutput());
		    stdout.AutoFlush = true;
		    Console.SetOut(stdout);
		    
		    if (log.GetStringBuilder().Length > 0) {
			new LogWindow(plugins[plugin].Name, log.ToString());
		    }
		}
	    } catch (Exception e) {
		using var d = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, e.Message);
		d.Title = "Plugin Exception";
		d.SecondaryText = e.StackTrace;
		d.Run();
	    }
	}
    }
}