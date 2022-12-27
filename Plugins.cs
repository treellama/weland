using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Weland
{
    public class Plugins
    {
        class PluginInfo
        {
            public string Name = "";
            //	    public MethodInfo GtkRun;
            public MethodInfo? Run;
        };
        List<PluginInfo> plugins = new List<PluginInfo>();

        public Plugins()
        {
            List<string> files = new List<string>();

            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (exePath is not null)
            {
                try
                {
                    foreach (string file in Directory.GetFiles(Path.Combine(exePath, "Plugins"), "*.dll"))
                    {
                        files.Add(file);
                    }
                }
                catch { }
            }

            /*	    if (PlatformDetection.IsMac) {
                    try {
                        foreach (string file in Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(exePath)), "Plugins"), "*.dll")) {
                        files.Add(file);
                        }
                    } catch { }
                }
                */

            string applicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Weland");
            try
            {
                foreach (string file in Directory.GetFiles(Path.Combine(applicationData, "Plugins"), "*.dll"))
                {
                    files.Add(file);
                }
            }
            catch { }

            foreach (string file in files)
            {
                try
                {

                    // needs to have at least name and run or gtkrun
                    Assembly a = Assembly.LoadFrom(file);

                    foreach (Type t in a.GetTypes())
                    {
                        PluginInfo plugin = new PluginInfo();
                        var compatibleMethod = t.GetMethod("Compatible");
                        if (compatibleMethod == null || !compatibleMethod.IsStatic)
                        {
                            continue;
                        }

                        var compatible = compatibleMethod.Invoke(0, new object[0]);
                        if (compatible == null || !(bool)compatible)
                        {
                            continue;
                        }

                        var nameMethod = t.GetMethod("Name");
                        if (nameMethod is not null && nameMethod.IsStatic)
                        {
                            var name = nameMethod.Invoke(0, new object[0]);
                            if (name != null)
                            {
                                plugin.Name = (string)name;
                            }

                            plugin.Run = t.GetMethod("Run");

                            if (plugin.Run != null && !plugin.Run.IsStatic)
                            {
                                plugin.Run = null;
                            }

                            if (plugin.Run != null)
                            {
                                plugins.Add(plugin);
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        public int Length
        {
            get
            {
                return plugins.Count;
            }
        }

        public string GetName(int plugin)
        {
            return plugins[plugin].Name;
        }
    }
}
