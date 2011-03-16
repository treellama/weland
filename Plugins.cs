using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Weland {
    public class Plugins {
	class PluginInfo {
	    public string Name;
	    public MethodInfo GtkRun;
	    public MethodInfo Run;
	};
	List<PluginInfo> plugins = new List<PluginInfo>();

	public Plugins() {
	    string[] files;
	    try {
		string pluginsFolder = Path.Combine(System.Environment.CurrentDirectory, "Plugins");
		files = Directory.GetFiles(pluginsFolder, "*.dll");
	    } catch {
		return;
	    }
		
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
			    plugin.Name = (string) nameMethod.Invoke(0, new Object[0]);
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

	public void GtkRun(Level level, int plugin) {
	    object[] args = { level };
	    
	    if (plugins[plugin].GtkRun != null) {
		plugins[plugin].GtkRun.Invoke(0, args);
	    } else if (plugins[plugin].Run != null) {
		plugins[plugin].Run.Invoke(0, args);
	    }
	}
    }
}