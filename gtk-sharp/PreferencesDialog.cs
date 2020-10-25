using Glade;
using Gtk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Weland {
    public class PreferencesDialog {
	public PreferencesDialog(Window parent, MapDrawingArea drawingArea, Editor theEditor) {
	    Glade.XML gxml = new Glade.XML(null, "preferences.glade", "dialog1", null);
	    gxml.Autoconnect(this);
	    dialog1.TransientFor = parent;
            if (PlatformDetection.IsMac) {
                alephOneButton.Action = FileChooserAction.SelectFolder;
            }
	    area = drawingArea;
	    editor = theEditor;
	}

	Gdk.Color ToGDK(Drawer.Color color) {
	    return new Gdk.Color((byte) (color.R * 0xff), (byte) (color.G * 0xff), (byte) (color.B * 0xff));
	}

	Drawer.Color FromGDK(Gdk.Color color) {
	    return new Drawer.Color((double) color.Red / ushort.MaxValue, (double) color.Green / ushort.MaxValue, (double) color.Blue / ushort.MaxValue);
	}

	void LoadColors(MapDrawingArea a) {
	    backgroundColor.Color = ToGDK(a.backgroundColor);
	    gridColor.Color = ToGDK(a.gridLineColor);
	    gridPointColor.Color = ToGDK(a.gridPointColor);
	    polygonColor.Color = ToGDK(a.polygonColor);
	    selectedPolygonColor.Color = ToGDK(a.selectedPolygonColor);
	    invalidPolygonColor.Color = ToGDK(a.invalidPolygonColor);
	    destinationPolygonColor.Color = ToGDK(a.destinationPolygonColor);
	    pointColor.Color = ToGDK(a.pointColor);
	    lineColor.Color = ToGDK(a.solidLineColor);
	    transparentLineColor.Color = ToGDK(a.transparentLineColor);
	    impassableLineColor.Color = ToGDK(a.impassableLineColor);
	    selectionColor.Color = ToGDK(a.selectedLineColor);
	    playerColor.Color = ToGDK(a.playerColor);
	    monsterColor.Color = ToGDK(a.monsterColor);
	    civilianColor.Color = ToGDK(a.civilianColor);
	    annotationColor.Color = ToGDK(a.annotationColor);	    
	}
    
	public void Run() {
	    antialias.Active = Weland.Settings.GetSetting("Drawer/SmoothLines", true);
	    showHiddenVertices.Active = Weland.Settings.GetSetting("MapWindow/ShowHiddenVertices", true);
	    rememberDeletedSides.Active = Weland.Settings.GetSetting("Editor/RememberDeletedSides", false);

	    LoadColors(area);
	    selectionDistance.Value = editor.DefaultSnapDistance;
	    objectDistance.Value = editor.ObjectSnapDistance;
	    dragInertia.Value = editor.InertiaDistance;
	    splitPolygonLines.Active = editor.SplitPolygonLines;

	    shapesFileButton.SetFilename(Weland.Settings.GetSetting("ShapesFile/Path", ""));
            alephOneButton.SetFilename(Weland.Settings.GetSetting("VisualMode/AlephOne", ""));
            scenarioButton.SetFilename(Weland.Settings.GetSetting("VisualMode/Scenario", ""));

	    dialog1.ShowAll();
	    dialog1.Show();
	    if (dialog1.Run() == (int) ResponseType.Ok) {
		Weland.Settings.PutSetting("Drawer/SmoothLines", antialias.Active);
		Weland.Settings.PutSetting("MapWindow/ShowHiddenVertices", showHiddenVertices.Active);
		Weland.Settings.PutSetting("Editor/RememberDeletedSides", rememberDeletedSides.Active);
		if (Weland.Settings.GetSetting("ShapesFile/Path", "") != shapesFileButton.Filename) {
		    Weland.Settings.PutSetting("ShapesFile/Path", shapesFileButton.Filename);
		    ShapesFile shapes = new ShapesFile();
		    shapes.Load(shapesFileButton.Filename);
		    Weland.Shapes = shapes;
		}
                Weland.Settings.PutSetting("VisualMode/AlephOne", alephOneButton.Filename);
                Weland.Settings.PutSetting("VisualMode/Scenario", scenarioButton.Filename);

		Level.FilterPoints = !showHiddenVertices.Active;
		Level.RememberDeletedSides = rememberDeletedSides.Active;

		area.backgroundColor = FromGDK(backgroundColor.Color);
		area.gridLineColor = FromGDK(gridColor.Color);
		area.gridPointColor = FromGDK(gridPointColor.Color);
		area.polygonColor = FromGDK(polygonColor.Color);
		area.selectedPolygonColor = FromGDK(selectedPolygonColor.Color);
		area.invalidPolygonColor = FromGDK(invalidPolygonColor.Color);
		area.destinationPolygonColor = FromGDK(destinationPolygonColor.Color);
		area.pointColor = FromGDK(pointColor.Color);
		area.solidLineColor = FromGDK(lineColor.Color);
		area.transparentLineColor = FromGDK(transparentLineColor.Color);
		area.impassableLineColor = FromGDK(impassableLineColor.Color);
		area.selectedLineColor = FromGDK(selectionColor.Color);
		area.playerColor = FromGDK(playerColor.Color);
		area.monsterColor = FromGDK(monsterColor.Color);
		area.civilianColor = FromGDK(civilianColor.Color);
		area.annotationColor = FromGDK(annotationColor.Color);
		area.SaveColors();

		editor.DefaultSnapDistance = (int) selectionDistance.Value;
		editor.ObjectSnapDistance = (int) objectDistance.Value;
		editor.InertiaDistance = (int) dragInertia.Value;
		editor.SplitPolygonLines = splitPolygonLines.Active;
		editor.SaveSettings();
	    }
	    dialog1.Destroy();
	}

	protected void OnResetColors(object o, EventArgs args) {
	    MapDrawingArea defaults = new MapDrawingArea();
	    defaults.DefaultColors();
	    LoadColors(defaults);
	}

        internal string EscapeArgument(string s) {
            if (PlatformDetection.IsMac) {
                return "-C" + s;
            } else {
                return s;
            }
        }

        protected void OnEditPreferences(object o, EventArgs args) {
            Process p = new Process();
            List<string> arguments = new List<string>();

            if (PlatformDetection.IsMac) {
                p.StartInfo.FileName = "open";
                arguments.Add("-a");
                arguments.Add("\"" + alephOneButton.Filename + "\"");
                arguments.Add("-W");
                arguments.Add("--args");
            } else {
                p.StartInfo.FileName = alephOneButton.Filename;
            }

            arguments.Add("-s");
            arguments.Add("-e");
            arguments.Add("\"" + EscapeArgument(scenarioButton.Filename) + "\"");

            p.StartInfo.Arguments = String.Join(" ", arguments);
            p.EnableRaisingEvents = true;

             MessageDialog d = new MessageDialog(dialog1, DialogFlags.DestroyWithParent | DialogFlags.Modal, MessageType.Other, ButtonsType.None, "Edit Visual Mode Preferences...");

            p.Exited += delegate(object sender, EventArgs e) {
                Gtk.Application.Invoke(delegate {
                        d.Destroy();
                    });
            };

            p.Start();
            d.Run();
        }

	MapDrawingArea area;
	Editor editor;

	[Widget] Dialog dialog1;

	[Widget] ToggleButton antialias;
	[Widget] ToggleButton showHiddenVertices;
	[Widget] ToggleButton splitPolygonLines;
	[Widget] ToggleButton rememberDeletedSides;

	[Widget] ColorButton backgroundColor;
	[Widget] ColorButton gridColor;
	[Widget] ColorButton gridPointColor;
	[Widget] ColorButton polygonColor;
	[Widget] ColorButton selectedPolygonColor;
	[Widget] ColorButton invalidPolygonColor;
	[Widget] ColorButton destinationPolygonColor;
	[Widget] ColorButton pointColor;
	[Widget] ColorButton lineColor;
	[Widget] ColorButton transparentLineColor;
	[Widget] ColorButton impassableLineColor;
	[Widget] ColorButton selectionColor;
	[Widget] ColorButton playerColor;
	[Widget] ColorButton monsterColor;
	[Widget] ColorButton civilianColor;
	[Widget] ColorButton annotationColor;

	[Widget] HScale selectionDistance;
	[Widget] HScale objectDistance;
	[Widget] HScale dragInertia;

	[Widget] FileChooserButton shapesFileButton;
        [Widget] FileChooserButton alephOneButton;
        [Widget] FileChooserButton scenarioButton;
    }
}
