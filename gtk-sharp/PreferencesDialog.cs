using Gtk;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using Widget = Gtk.Builder.ObjectAttribute;

namespace Weland
{
    public class PreferencesDialog
    {
        public PreferencesDialog(Window parent, MapDrawingArea drawingArea, Editor theEditor)
        {
            var builder = new Builder("preferences.glade");
            builder.Autoconnect(this);
            dialog1.TransientFor = parent;
            if (PlatformDetection.IsMac)
            {
                alephOneButton.Action = FileChooserAction.SelectFolder;
            }
            area = drawingArea;
            editor = theEditor;
        }

        Gdk.RGBA ToGDK(Drawer.Color color)
        {
            return new Gdk.RGBA()
            {
                Red = color.R,
                Green = color.G,
                Blue = color.B,
                Alpha = 1.0
            };
        }

        Drawer.Color FromGDK(Gdk.RGBA color)
        {
            return new Drawer.Color(color.Red, color.Green, color.Blue);
        }

        void LoadColors(MapDrawingArea a)
        {
            backgroundColor.Rgba = ToGDK(a.backgroundColor);
            gridColor.Rgba = ToGDK(a.gridLineColor);
            gridPointColor.Rgba = ToGDK(a.gridPointColor);
            polygonColor.Rgba = ToGDK(a.polygonColor);
            selectedPolygonColor.Rgba = ToGDK(a.selectedPolygonColor);
            invalidPolygonColor.Rgba = ToGDK(a.invalidPolygonColor);
            destinationPolygonColor.Rgba = ToGDK(a.destinationPolygonColor);
            pointColor.Rgba = ToGDK(a.pointColor);
            lineColor.Rgba = ToGDK(a.solidLineColor);
            transparentLineColor.Rgba = ToGDK(a.transparentLineColor);
            impassableLineColor.Rgba = ToGDK(a.impassableLineColor);
            selectionColor.Rgba = ToGDK(a.selectedLineColor);
            playerColor.Rgba = ToGDK(a.playerColor);
            monsterColor.Rgba = ToGDK(a.monsterColor);
            civilianColor.Rgba = ToGDK(a.civilianColor);
            annotationColor.Rgba = ToGDK(a.annotationColor);
        }

        public void Run()
        {
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
            if (dialog1.Run() == (int)ResponseType.Ok)
            {
                Weland.Settings.PutSetting("Drawer/SmoothLines", antialias.Active);
                Weland.Settings.PutSetting("MapWindow/ShowHiddenVertices", showHiddenVertices.Active);
                Weland.Settings.PutSetting("Editor/RememberDeletedSides", rememberDeletedSides.Active);
                if (Weland.Settings.GetSetting("ShapesFile/Path", "") != shapesFileButton.Filename)
                {
                    Weland.Settings.PutSetting("ShapesFile/Path", shapesFileButton.Filename);
                    ShapesFile shapes = new ShapesFile();
                    shapes.Load(shapesFileButton.Filename);
                    Weland.Shapes = shapes;
                }
                Weland.Settings.PutSetting("VisualMode/AlephOne", alephOneButton.Filename);
                Weland.Settings.PutSetting("VisualMode/Scenario", scenarioButton.Filename);

                Level.FilterPoints = !showHiddenVertices.Active;
                Level.RememberDeletedSides = rememberDeletedSides.Active;

                area.backgroundColor = FromGDK(backgroundColor.Rgba);
                area.gridLineColor = FromGDK(gridColor.Rgba);
                area.gridPointColor = FromGDK(gridPointColor.Rgba);
                area.polygonColor = FromGDK(polygonColor.Rgba);
                area.selectedPolygonColor = FromGDK(selectedPolygonColor.Rgba);
                area.invalidPolygonColor = FromGDK(invalidPolygonColor.Rgba);
                area.destinationPolygonColor = FromGDK(destinationPolygonColor.Rgba);
                area.pointColor = FromGDK(pointColor.Rgba);
                area.solidLineColor = FromGDK(lineColor.Rgba);
                area.transparentLineColor = FromGDK(transparentLineColor.Rgba);
                area.impassableLineColor = FromGDK(impassableLineColor.Rgba);
                area.selectedLineColor = FromGDK(selectionColor.Rgba);
                area.playerColor = FromGDK(playerColor.Rgba);
                area.monsterColor = FromGDK(monsterColor.Rgba);
                area.civilianColor = FromGDK(civilianColor.Rgba);
                area.annotationColor = FromGDK(annotationColor.Rgba);
                area.SaveColors();

                editor.DefaultSnapDistance = (int)selectionDistance.Value;
                editor.ObjectSnapDistance = (int)objectDistance.Value;
                editor.InertiaDistance = (int)dragInertia.Value;
                editor.SplitPolygonLines = splitPolygonLines.Active;
                editor.SaveSettings();
            }
            dialog1.Destroy();
        }

        protected void OnResetColors(object o, EventArgs args)
        {
            MapDrawingArea defaults = new MapDrawingArea();
            defaults.DefaultColors();
            LoadColors(defaults);
        }

        internal string EscapeArgument(string s)
        {
            if (PlatformDetection.IsMac)
            {
                return "-C" + s;
            }
            else
            {
                return s;
            }
        }

        protected void OnEditPreferences(object o, EventArgs args)
        {
            Process p = new Process();
            List<string> arguments = new List<string>();

            if (PlatformDetection.IsMac)
            {
                p.StartInfo.FileName = "open";
                arguments.Add("-a");
                arguments.Add("\"" + alephOneButton.Filename + "\"");
                arguments.Add("-W");
                arguments.Add("--args");
            }
            else
            {
                p.StartInfo.FileName = alephOneButton.Filename;
            }

            arguments.Add("-e");
            arguments.Add("\"" + EscapeArgument(scenarioButton.Filename) + "\"");

            p.StartInfo.Arguments = String.Join(" ", arguments);
            p.EnableRaisingEvents = true;

            MessageDialog d = new MessageDialog(dialog1, DialogFlags.DestroyWithParent | DialogFlags.Modal, MessageType.Other, ButtonsType.None, "Edit Visual Mode Preferences...");

            p.Exited += delegate (object sender, EventArgs e)
            {
                Gtk.Application.Invoke(delegate
                {
                    d.Destroy();
                });
            };

            p.Start();
            d.Run();
        }

        MapDrawingArea area;
        Editor editor;

    #pragma warning disable 0649

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

    #pragma warning restore 0649
    }
}
