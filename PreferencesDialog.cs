using Glade;
using Gtk;
using System;

namespace Weland {
    public class PreferencesDialog {
	public PreferencesDialog(Window parent, MapDrawingArea drawingArea, Editor theEditor) {
	    Glade.XML gxml = new Glade.XML(null, "preferences.glade", "dialog1", null);
	    gxml.Autoconnect(this);
	    dialog1.TransientFor = parent;
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

	    LoadColors(area);
	    selectionDistance.Value = editor.DefaultSnapDistance;
	    objectDistance.Value = editor.ObjectSnapDistance;
	    dragInertia.Value = editor.InertiaDistance;

	    dialog1.ShowAll();
	    dialog1.Show();
	    if (dialog1.Run() == (int) ResponseType.Ok) {
		Weland.Settings.PutSetting("Drawer/SmoothLines", antialias.Active);
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
		editor.SaveSettings();
	    }
	    dialog1.Destroy();
	}

	protected void OnResetColors(object o, EventArgs args) {
	    MapDrawingArea defaults = new MapDrawingArea();
	    defaults.DefaultColors();
	    LoadColors(defaults);
	}

	MapDrawingArea area;
	Editor editor;

	[Widget] Dialog dialog1;

	[Widget] ToggleButton antialias;

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
    }
}