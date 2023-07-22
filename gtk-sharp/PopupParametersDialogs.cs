using Gtk;
using System;

using Widget = Gtk.Builder.ObjectAttribute;

namespace Weland
{
    public class LineParametersDialog
    {
        public LineParametersDialog(Window parent, Level theLevel, Line theLine)
        {
            level = theLevel;
            line = theLine;
            var builder = new Builder("lineparameters.glade");
            builder.Autoconnect(this);
            dialog1.TransientFor = parent;
        }

        public void Run()
        {
            solid.Active = line.Solid;
            solid.Sensitive = !(line.ClockwisePolygonOwner == -1 || line.CounterclockwisePolygonOwner == -1);
            transparent.Active = line.Transparent;
            decorative.Active = line.Decorative;
            dialog1.Run();
            line.Solid = solid.Active;
            line.Transparent = transparent.Active;
            line.Decorative = decorative.Active;
            dialog1.Destroy();
        }

        protected void OnRemoveTextures(object obj, EventArgs args)
        {
            if (line.ClockwisePolygonSideIndex != -1)
            {
                level.DeleteSide(line.ClockwisePolygonSideIndex);
            }
            if (line.CounterclockwisePolygonSideIndex != -1)
            {
                level.DeleteSide(line.CounterclockwisePolygonSideIndex);
            }
        }

        Level level;
        Line line;

        [Widget] Dialog dialog1;

        [Widget] CheckButton solid;
        [Widget] CheckButton transparent;
        [Widget] CheckButton decorative;
    }

    public class PointParametersDialog
    {
        public PointParametersDialog(Window parent, Level theLevel, short theIndex)
        {
            level = theLevel;
            index = theIndex;
            var builder = new Builder("pointparameters.glade");
            builder.Autoconnect(this);
            dialog1.TransientFor = parent;
        }

        public void Run()
        {
            Point p = level.Endpoints[index];
            pointX.Text = String.Format("{0}", p.X);
            pointY.Text = String.Format("{0}", p.Y);
            dialog1.Run();

            Point n = p;
            short i;
            if (short.TryParse(pointX.Text, out i))
            {
                n.X = i;
            }
            if (short.TryParse(pointY.Text, out i))
            {
                n.Y = i;
            }
            level.Endpoints[index] = n;
            dialog1.Destroy();
        }

        Level level;
        short index;

        [Widget] Dialog dialog1;

        [Widget] Entry pointX;
        [Widget] Entry pointY;
    }

    public class GotoDialog : IDisposable
    {
        public GotoDialog(Window parent)
        {
            var builder = new Builder("goto.glade");
            builder.Autoconnect(this);
            dialog1.TransientFor = parent;
        }

        public int Run()
        {
            Type.Active = 2;
            dialog1.Focus = Number;
            return dialog1.Run();
        }

        public void Dispose()
        {
            dialog1.Dispose();
        }

        protected void OnEntryActivated(object obj, EventArgs args)
        {
            dialog1.Respond(ResponseType.Ok);
        }

        [Widget] Dialog dialog1;
        [Widget] public ComboBox Type;
        [Widget] public Entry Number;
    }
}
