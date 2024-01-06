using Cairo;
using System;
using System.Collections.Generic;

namespace Weland
{
    public class CairoDrawer : Drawer
    {

        public Context Context { get; }

        public CairoDrawer(Context context, bool antialias)
        {
            if (!antialias)
            {
                context.Antialias = Antialias.None;
            }
            Context = context;
        }

        class TextureSurface : IDisposable
        {
            public TextureSurface(ShapeDescriptor d)
            {
                var texture = Weland.Shapes.GetShape(d);
                if (d.Collection < 27)
                {
                    texture = texture.RotateCW();
                }

                surface = ImageUtilities.ImageToSurface(texture);
            }

            public void Dispose()
            {
                surface.Dispose();
            }

            public ImageSurface Surface
            {
                get { return surface; }
            }

            private ImageSurface surface;
        }

        class TextureCache : IDisposable
        {
            public TextureCache()
            {
                Weland.ShapesChanged += new ShapesFileChangedEventHandler(OnShapesChanged);
            }

            public void Dispose()
            {
                Weland.ShapesChanged -= new ShapesFileChangedEventHandler(OnShapesChanged);
                foreach (var kvp in cache)
                {
                    kvp.Value.Dispose();
                }
            }

            public void OnShapesChanged()
            {
                foreach (var kvp in cache)
                {
                    kvp.Value.Dispose();
                }
                cache.Clear();
            }

            public ImageSurface GetSurface(ShapeDescriptor d)
            {
                if (!cache.ContainsKey((ushort)d))
                {
                    cache.Add((ushort)d, new TextureSurface(d));
                }

                return cache[(ushort)d].Surface;
            }

            Dictionary<ushort, TextureSurface> cache = new Dictionary<ushort, TextureSurface>();
        }

        static TextureCache cache = new TextureCache();

        public override void Clear(Color c)
        {
            Context.Save();

            Context.SetSourceRGB(c.R, c.G, c.B);
            Context.Paint();

            Context.Restore();
        }

        public override void DrawPoint(Color c, Point p)
        {
            Context.Save();

            Context.MoveTo(new PointD(p.X + 0.5, p.Y + 0.5));
            Context.ClosePath();
            Context.LineCap = LineCap.Round;
            Context.SetSourceRGB(c.R, c.G, c.B);
            Context.LineWidth = 2.5;
            Context.Stroke();

            Context.Restore();
        }

        public override void DrawGridIntersect(Color c, Point p)
        {
            Context.Save();

            Context.MoveTo(new PointD(p.X - 0.5, p.Y + 0.5));
            Context.LineTo(new PointD(p.X + 1.5, p.Y + 0.5));
            Context.ClosePath();

            Context.MoveTo(new PointD(p.X + 0.5, p.Y - 0.5));
            Context.LineTo(new PointD(p.X + 0.5, p.Y + 1.5));
            Context.ClosePath();

            Context.SetSourceRGB(c.R, c.G, c.B);
            Context.LineWidth = 1.0;
            Context.Stroke();

            Context.Restore();
        }

        public override void DrawLine(Color c, Point p1, Point p2)
        {
            Context.Save();

            Context.MoveTo(new PointD(p1.X + 0.5, p1.Y + 0.5));
            Context.LineTo(new PointD(p2.X + 0.5, p2.Y + 0.5));
            Context.ClosePath();
            Context.SetSourceRGB(c.R, c.G, c.B);
            Context.LineWidth = 1.0;
            Context.Stroke();

            Context.Restore();
        }

        void OutlinePolygon(List<Point> points)
        {
            Context.MoveTo(new PointD(points[0].X + 0.5, points[0].Y + 0.5));
            for (int i = 1; i < points.Count; ++i)
            {
                Context.LineTo(new PointD(points[i].X + 0.5, points[i].Y + 0.5));
            }
            Context.ClosePath();
        }

        public override void FillPolygon(Color c, List<Point> points)
        {
            Context.Save();

            OutlinePolygon(points);
            Context.SetSourceRGB(c.R, c.G, c.B);
            Context.Fill();

            Context.Restore();
        }

        public override void FillStrokePolygon(Color fill, Color stroke, List<Point> points, bool dashed)
        {
            Context.Save();

            OutlinePolygon(points);
            Context.SetSourceRGB(fill.R, fill.G, fill.B);
            Context.FillPreserve();

            if (dashed)
            {
                Context.SetDash(new double[] { 2.0, 2.0 }, 0);
            }
            Context.SetSourceRGB(stroke.R, stroke.G, stroke.B);
            Context.LineWidth = 1.0;
            Context.Stroke();

            Context.Restore();
        }

        public override void TexturePolygon(ShapeDescriptor d, List<Point> points)
        {
            Context.Save();

            OutlinePolygon(points);
            using (SurfacePattern pattern = new SurfacePattern(cache.GetSurface(d)))
            {
                pattern.Extend = Cairo.Extend.Repeat;
                Context.SetSource(pattern);
                Context.Fill();
            }
            Context.Restore();
        }

        public override void Dispose() { }

    }
}
