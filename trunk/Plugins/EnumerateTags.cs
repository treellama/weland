// EnumerateTags.cs is a sample no-UI plugin for Weland

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Weland;

[assembly:AssemblyVersionAttribute ("1.0.0.0")]
[assembly:AssemblyTitleAttribute ("Enumerate Tags")]
[assembly:AssemblyDescriptionAttribute ("Display a list of tags used by the level")]
[assembly:AssemblyCopyrightAttribute ("\u2117 2011 Gregory Smith (GPL)")]

public class Plugin {
    public static bool Compatible() {
	//	Version v = Assembly.GetEntryAssembly().GetName().Version;
	//	return (v.Major >= 2 || (v.Major == 1 && v.Minor >= 3));
	return true;
    }

    public static string Name() {
	return "Enumerate Tags";
    }

    public static void Run(Editor editor)
    {
	Level level = editor.Level;
	SortedDictionary<int, List<string>> tags = new SortedDictionary<int, List<string>>();
	
	for (int i = 0; i < level.Lights.Count; ++i) {
	    Light light = level.Lights[i];
	    int tag = light.TagIndex;
	    if (tag > 0) {
		if (!tags.ContainsKey(tag)) {
		    tags[tag] = new List<string>();
		}
		tags[tag].Add(String.Format("light {0}", i));
	    }
	}
	
	for (int i = 0; i < level.Sides.Count; ++i) {
	    Side side = level.Sides[i];
	    if (side.IsControlPanel && side.IsTagSwitch() && side.ControlPanelPermutation > 0) {
		int tag = side.ControlPanelPermutation;
		if (!tags.ContainsKey(tag)) {
		    tags[tag] = new List<string>();
		}
		tags[tag].Add(String.Format("control panel on line {0}", side.LineIndex));
	    }
	}

	for (int i = 0; i < level.Platforms.Count; ++i) {
	    Platform platform = level.Platforms[i];
	    int tag = platform.Tag;
	    if (tag > 0) {
		if (!tags.ContainsKey(tag)) {
		    tags[tag] = new List<string>();
		}
		tags[tag].Add(String.Format("platform at polygon {0}", platform.PolygonIndex));
	    }
	}
	
	Console.WriteLine(level.Name);
	foreach (var kvp in tags) {
	    Console.WriteLine("Tag {0}", kvp.Key);
	    foreach(String s in kvp.Value) {
		Console.WriteLine("\t{0}", s);
	    }
	}
    }
}