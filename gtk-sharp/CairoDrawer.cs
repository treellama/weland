using Cairo;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Weland
{
    public class CairoDrawer : Drawer
    {

        public Context Context { get; }

        public CairoDrawer(Context context, bool antialias)
        {
            if (!antialias)
            {
                Context.Antialias = Antialias.None;
            }
            Context = context;
        }

        class TextureSurface : IDisposable
        {
            public TextureSurface(ShapeDescriptor d)
            {
                System.Drawing.Bitmap bitmap = Weland.Shapes.GetShape(d);
                byte[] bytes = new byte[bitmap.Width * bitmap.Height * 4];
                if (d.Collection >= 27)
                {
                    for (int x = 0; x < bitmap.Width; ++x)
                    {
                        for (int y = 0; y < bitmap.Height; ++y)
                        {
                            System.Drawing.Color c = bitmap.GetPixel(x, y);
                            int offset = (y * bitmap.Width + x) * 4;
                            bytes[offset] = c.B;
                            bytes[offset + 1] = c.G;
                            bytes[offset + 2] = c.R;
                            bytes[offset + 3] = c.A;
                        }
                    }
                }
                else
                {
                    for (int x = 0; x < bitmap.Width; ++x)
                    {
                        for (int y = 0; y < bitmap.Height; ++y)
                        {
                            System.Drawing.Color c = bitmap.GetPixel(x, y);
                            int offset = (x * bitmap.Width + y) * 4;
                            bytes[offset] = c.B;
                            bytes[offset + 1] = c.G;
                            bytes[offset + 2] = c.R;
                            bytes[offset + 3] = c.A;
                        }
                    }
                }

                handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                surface = new ImageSurface(bytes, Format.ARGB32, bitmap.Width, bitmap.Height, d.Collection >= 27 ? bitmap.Width * 4 : bitmap.Height * 4);
            }

            public void Dispose()
            {
                surface.Dispose();

                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }

            public ImageSurface Surface
            {
                get { return surface; }
            }

            private ImageSurface surface;
            private GCHandle handle;
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
