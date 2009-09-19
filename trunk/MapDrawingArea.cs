using System;
using System.Collections.Generic;

namespace Weland {
    [Flags] public enum CohenSutherland {
	Inside = 0x0,
	Top = 0x1,
	Bottom = 0x2,
	Left = 0x4,
	Right = 0x8
    }

    public class Transform {
	public Transform() { }
	public double Scale = 1.0 / 16.0;
	public short XOffset = 0;
	public short YOffset = 0;

	public double ToScreenX(short x) { 
	    return Math.Floor((x - XOffset) * Scale);
	}

	public double ToScreenY(short y) {
	    return Math.Floor((y - YOffset) * Scale);
	}

	public short ToMapX(double X) {
	    return (short) Math.Min(short.MaxValue, Math.Max(short.MinValue, ((double) (X / Scale) + XOffset)));
	}

	public short ToMapY(double Y) {
	    return (short) Math.Min(short.MaxValue, Math.Max(short.MinValue, ((double) (Y / Scale) + YOffset)));
	}

	public Drawer.Point ToScreenPoint(Point p) {
	    return new Drawer.Point(ToScreenX(p.X), ToScreenY(p.Y));
	}
    }

    public enum DrawMode {
	Draw,
	FloorHeight,
	CeilingHeight,
	PolygonType
    }

    public class MapDrawingArea : Gtk.DrawingArea {

	public Transform Transform = new Transform();
	public Level Level;
	public Grid Grid = new Grid();
	public Selection Selection = new Selection();
	public bool ShowMonsters = true;
	public bool ShowObjects = true;
	public bool ShowScenery = true;
	public bool ShowPlayers = true;
	public bool ShowGoals = true;
	public bool ShowSounds = true;
	public bool Antialias = true;
	public DrawMode Mode = DrawMode.Draw;

	public Dictionary<short, Drawer.Color> PaintColors = new Dictionary<short, Drawer.Color>();

	Drawer drawer;

	public MapDrawingArea() { 
	    itemImages[ItemType.Magnum] = new Gdk.Pixbuf(null, "pistol.png");
	    itemImages[ItemType.MagnumMagazine] = new Gdk.Pixbuf(null, "pistol-ammo.png");
	    itemImages[ItemType.PlasmaPistol] = new Gdk.Pixbuf(null, "fusion.png");
	    itemImages[ItemType.PlasmaMagazine] = new Gdk.Pixbuf(null, "fusion-ammo.png");
	    itemImages[ItemType.AssaultRifle] = new Gdk.Pixbuf(null, "ar.png");
	    itemImages[ItemType.AssaultRifleMagazine] = new Gdk.Pixbuf(null, "ar-ammo.png");
	    itemImages[ItemType.AssaultGrenadeMagazine] = new Gdk.Pixbuf(null, "ar-grenades.png");
	    itemImages[ItemType.MissileLauncher] = new Gdk.Pixbuf(null, "rl.png");
	    itemImages[ItemType.MissileLauncherMagazine] = new Gdk.Pixbuf(null, "rl-ammo.png");
	    itemImages[ItemType.InvisibilityPowerup] = new Gdk.Pixbuf(null, "powerup.png");
	    itemImages[ItemType.InvincibilityPowerup] = new Gdk.Pixbuf(null, "invinc.png");
	    itemImages[ItemType.InfravisionPowerup] = new Gdk.Pixbuf(null, "powerup.png");
	    itemImages[ItemType.AlienShotgun] = new Gdk.Pixbuf(null, "alien-gun.png");
	    itemImages[ItemType.Flamethrower] = new Gdk.Pixbuf(null, "tozt.png");
	    itemImages[ItemType.FlamethrowerCanister] = new Gdk.Pixbuf(null, "tozt-ammo.png");
	    itemImages[ItemType.ExtravisionPowerup] = new Gdk.Pixbuf(null, "powerup.png");
	    itemImages[ItemType.OxygenPowerup] = new Gdk.Pixbuf(null, "oxygen.png");
	    itemImages[ItemType.EnergyPowerup] = new Gdk.Pixbuf(null, "1x.png");
	    itemImages[ItemType.DoubleEnergyPowerup] = new Gdk.Pixbuf(null, "2x.png");
	    itemImages[ItemType.TripleEnergyPowerup] = new Gdk.Pixbuf(null, "3x.png");
	    itemImages[ItemType.Shotgun] = new Gdk.Pixbuf(null, "shotgun.png");
	    itemImages[ItemType.ShotgunMagazine] = new Gdk.Pixbuf(null, "shotgun-ammo.png");
	    itemImages[ItemType.SphtDoorKey] = new Gdk.Pixbuf(null, "uplink-chip.png");
	    itemImages[ItemType.RedBall] = new Gdk.Pixbuf(null, "skull.png");
	    itemImages[ItemType.Smg] = new Gdk.Pixbuf(null, "smg.png");
	    itemImages[ItemType.SmgAmmo] = new Gdk.Pixbuf(null, "smg-ammo.png");
	}

	Drawer.Color backgroundColor = new Drawer.Color(0.33, 0.33, 0.33);
	Drawer.Color pointColor = new Drawer.Color(1, 0, 0);
	Drawer.Color impassableLineColor = new Drawer.Color(0.2, 0.98, 0.48);
	Drawer.Color solidLineColor = new Drawer.Color(0, 0, 0);
	Drawer.Color transparentLineColor = new Drawer.Color(0.2, 0.8, 0.8);
	Drawer.Color selectedLineColor = new Drawer.Color(1, 1, 0);
	Drawer.Color selectedPolygonColor = new Drawer.Color((double) 0xff/0xff, 
							  (double) 0xcc/0xff,
							  (double) 0x66/0xff);
	Drawer.Color polygonColor = new Drawer.Color(0.87, 0.87, 0.87);
	Drawer.Color invalidPolygonColor = new Drawer.Color((double) 0xfb/0xff,
							    (double) 0x48/0xff,
							    (double) 0x09/0xff);
	Drawer.Color gridLineColor = new Drawer.Color(0.6, 0.6, 0.6);
	Drawer.Color gridPointColor = new Drawer.Color(0, 0.8, 0.8);
	Drawer.Color objectColor = new Drawer.Color(1, 1, 0);
	Drawer.Color playerColor = new Drawer.Color(1, 1, 0);
	Drawer.Color monsterColor = new Drawer.Color(1, 0, 0);
	Drawer.Color civilianColor = new Drawer.Color(0, 0, 1);

	Gdk.Pixbuf sceneryImage = new Gdk.Pixbuf(null, "flower.png");
	Gdk.Pixbuf soundImage = new Gdk.Pixbuf(null, "sound.png");
	Gdk.Pixbuf goalImage = new Gdk.Pixbuf(null, "flag.png");

	Dictionary<ItemType, Gdk.Pixbuf> itemImages = new Dictionary<ItemType, Gdk.Pixbuf>();

	protected override bool OnExposeEvent(Gdk.EventExpose args) {
#if SYSTEM_DRAWING
	    drawer = new SystemDrawer(GdkWindow, Antialias);
#else
	    if (!Antialias && !MapWindow.IsMac()) {
		drawer = new GdkDrawer(GdkWindow);
	    } else {
		drawer = new CairoDrawer(GdkWindow, Antialias);
	    }	
#endif
	    drawer.Clear(backgroundColor);
	    
	    if (Grid.Visible) {
		DrawGrid();
	    }
	    
	    if (Level != null) {

		// clipping area to Map
		int Left, Right, Top, Bottom;
		Left = Transform.ToMapX(args.Area.X);
		Right = Transform.ToMapX(args.Area.Width + args.Area.X);
		Top = Transform.ToMapY(args.Area.Y);
		Bottom = Transform.ToMapY(args.Area.Height + args.Area.Y);

		CohenSutherland[] Points = new CohenSutherland[Level.Endpoints.Count];

		for (short i = 0; i < Level.Endpoints.Count; ++i) {
		    Point p = Level.Endpoints[i];
		    Points[i] = CohenSutherland.Inside;
		    if (p.X < Left) {
			Points[i] |= CohenSutherland.Left;
		    } else if (p.X > Right) {
			Points[i] |= CohenSutherland.Right;
		    }

		    if (p.Y < Top) {
			Points[i] |= CohenSutherland.Top;
		    } else if (p.Y > Bottom) {
			Points[i] |= CohenSutherland.Bottom;
		    }
		}

		foreach (Polygon polygon in Level.Polygons) {
		    CohenSutherland code = ~CohenSutherland.Inside;
		    for (short i = 0; i < polygon.VertexCount; ++i) {
			code &= Points[polygon.EndpointIndexes[i]];
		    }
		    if (code == CohenSutherland.Inside) 
			DrawPolygon(polygon, false);
		}

		if (Selection.Polygon != -1) {
		    Polygon polygon = Level.Polygons[Selection.Polygon];
		    CohenSutherland code = ~CohenSutherland.Inside;
		    for (short i = 0; i < polygon.VertexCount; ++i) {
			code &= Points[polygon.EndpointIndexes[i]];
		    }
		    if (code == CohenSutherland.Inside)
			DrawPolygon(polygon, true);
		}
		
		foreach (Line line in Level.Lines) {
		    if ((Points[line.EndpointIndexes[0]] & Points[line.EndpointIndexes[1]]) == CohenSutherland.Inside) {
			DrawLine(line);
		    }
		}

		for (short i = 0; i < Level.Endpoints.Count; ++i) {
		    if (Points[i] == CohenSutherland.Inside) {
			DrawPoint(Level.Endpoints[i]);
		    }
		}
		
		if (Mode == DrawMode.Draw) {
		    int ObjectSize = (int) (16 / 2 / Transform.Scale);
		    MapObject selectedObj = null;
		    if (Selection.Object != -1) {
			selectedObj = Level.Objects[Selection.Object];
		    }
		    foreach (MapObject obj in Level.Objects) {
			if (obj != selectedObj && obj.X > Left - ObjectSize && obj.X < Right + ObjectSize && obj.Y > Top - ObjectSize && obj.Y < Bottom + ObjectSize) {
			    DrawObject(obj, false);
			}
		    }
		    if (selectedObj != null && selectedObj.X > Left - ObjectSize && selectedObj.X < Right + ObjectSize && selectedObj.Y > Top - ObjectSize && selectedObj.Y < Bottom + ObjectSize) {
			DrawObject(selectedObj, true);
		    }
		}

		if (Level.TemporaryLineStartIndex != -1) {
		    // draw the temporarily drawn line
		    drawer.DrawLine(selectedLineColor, Transform.ToScreenPoint(Level.Endpoints[Level.TemporaryLineStartIndex]), Transform.ToScreenPoint(Level.TemporaryLineEnd));
		}

		if (Selection.Point != -1) {
		    // draw the selected point
		    DrawFatPoint(selectedLineColor, Level.Endpoints[Selection.Point]);
		}
	    }

	    drawer.Dispose();
	    return true;
	}

	public void Center(short X, short Y) {
	    Transform.XOffset = (short) (X - Allocation.Width / 2 / Transform.Scale);
	    Transform.YOffset = (short) (Y - Allocation.Height / 2 / Transform.Scale);
	}

	void DrawPoint(Point point) {
	    drawer.DrawPoint(pointColor, Transform.ToScreenPoint(point));
	}

	void DrawFatPoint(Drawer.Color color, Point point) {
	    const int r = 2;
	    List<Drawer.Point> points = new List<Drawer.Point>();
	    double X = Transform.ToScreenX(point.X);
	    double Y = Transform.ToScreenY(point.Y);
	    points.Add(new Drawer.Point(X - r, Y - r));
	    points.Add(new Drawer.Point(X + r, Y - r));
	    points.Add(new Drawer.Point(X + r, Y + r));
	    points.Add(new Drawer.Point(X - r, Y + r));
	    drawer.FillStrokePolygon(color, new Drawer.Color(0, 0, 0), points);
	}

	void DrawGrid() {
	    Point p1 = new Point();
	    Point p2 = new Point();

	    short Left = Transform.ToMapX(0);
	    short Right = Transform.ToMapX(Allocation.Width);
	    short Top = Transform.ToMapY(0);
	    short Bottom = Transform.ToMapY(Allocation.Height);
	    
	    // draw horizontal map lines
	    for (int j = (Top / Grid.Resolution) * Grid.Resolution; j < Bottom; j += Grid.Resolution) {
		p1.X = Left;
		p1.Y = (short) j;
		p2.X = Right;
		p2.Y = (short) j;
		
		drawer.DrawLine(gridLineColor, Transform.ToScreenPoint(p1), Transform.ToScreenPoint(p2));
	    }

	    // draw vertical map lines
	    for (int i = (Left  / Grid.Resolution) * Grid.Resolution; i < Right; i += Grid.Resolution) {
		p1.X = (short) i;
		p1.Y = Top;
		p2.X = (short) i;
		p2.Y = Bottom;
		
		drawer.DrawLine(gridLineColor, Transform.ToScreenPoint(p1), Transform.ToScreenPoint(p2));
	    }

	    // draw grid intersects
	    int wu = Math.Max(1024, (int) Grid.Resolution);
	    for (int i = (Left / wu) * wu; i < Right; i += wu) {
		for (int j = (Top / wu) * wu; j < Bottom; j += wu) {
		    p1.X = (short) i;
		    p1.Y = (short) j;
		    drawer.DrawGridIntersect(gridPointColor, Transform.ToScreenPoint(p1));
		}
	    }
	}
	    

	void DrawLine(Line line) {
	    Point p1 = Level.Endpoints[line.EndpointIndexes[0]];
	    Point p2 = Level.Endpoints[line.EndpointIndexes[1]];

	    Drawer.Color color = solidLineColor;
	    if (Selection.Line != -1 && line == Level.Lines[Selection.Line]) {
		color = selectedLineColor;
	    } else if (line.Transparent) {
		color = transparentLineColor;
	    } else if (line.Solid && line.ClockwisePolygonOwner != -1 && line.CounterclockwisePolygonOwner != -1) {
		Polygon poly1 = Level.Polygons[line.ClockwisePolygonOwner];
		Polygon poly2 = Level.Polygons[line.CounterclockwisePolygonOwner];
		if (poly1.FloorHeight != poly2.FloorHeight) {
		    color = impassableLineColor;
		}
	    }

	    drawer.DrawLine(color, Transform.ToScreenPoint(p1), Transform.ToScreenPoint(p2));
	}

	void DrawPolygon(Polygon polygon, bool highlight) {
	    List<Drawer.Point> points = new List<Drawer.Point>();
	    for (int i = 0; i < polygon.VertexCount; ++i) {
		points.Add(Transform.ToScreenPoint(Level.Endpoints[polygon.EndpointIndexes[i]]));
	    }
	    if (Mode == DrawMode.FloorHeight) {
		drawer.FillPolygon(PaintColors[polygon.FloorHeight], points);
	    } else if (Mode == DrawMode.CeilingHeight) {
		drawer.FillPolygon(PaintColors[polygon.CeilingHeight], points);
	    } else if (Mode == DrawMode.PolygonType) {
		drawer.FillPolygon(PaintColors[(short) polygon.Type], points);
	    } else {
		if (highlight) {
		    drawer.FillPolygon(selectedPolygonColor, points);
		} else if (polygon.Concave) {
		    drawer.FillPolygon(invalidPolygonColor, points);
		} else {
		    drawer.FillPolygon(polygonColor, points);
		}
	    }
	}
	
	void DrawTriangle(Drawer.Color c, double X, double Y, double angle, bool highlight) {
	    double rads = angle * Math.PI / 180;
	    List<Drawer.Point> points = new List<Drawer.Point>();
	    points.Add(new Drawer.Point(X + 8 * Math.Cos(rads), Y + 8 * Math.Sin(rads)));
	    points.Add(new Drawer.Point(X + 10 * Math.Cos(rads + 2 * Math.PI * 0.4), Y + 10 * Math.Sin(rads + 2 * Math.PI * 0.4)));
	    points.Add(new Drawer.Point(X + 10 * Math.Cos(rads - 2 * Math.PI * 0.4), Y + 10 * Math.Sin(rads - 2 * Math.PI * 0.4)));
	    
	    if (highlight) {
		drawer.FillStrokePolygon(new Drawer.Color(0, 0, 0), c, points);
	    } else {
		drawer.FillStrokePolygon(c, new Drawer.Color(0, 0, 0), points);
	    }

	}

	void DrawImage(Gdk.Pixbuf image, double X, double Y, bool highlight) {
	    int x = (int) X - image.Width / 2;
	    int y = (int) Y - image.Height / 2;
	    if (highlight) {
		Gdk.GC gc = new Gdk.GC(GdkWindow);
		if (!MapWindow.IsMac()) { // UGH
		    Gdk.Pixmap mask = new Gdk.Pixmap(null, image.Width, image.Height, 1);
		    image.RenderThresholdAlpha(mask, 0, 0, 0, 0, -1, -1, 1);
		    
		    gc.ClipMask = mask;
		}
		
		for (int i = -2; i <= 2; ++i) {
		    for (int j = -2; j <= 2; ++j) {
			gc.SetClipOrigin(x + i, y + j);
			gc.RgbFgColor = new Gdk.Color(255, 255, 0);
			GdkWindow.DrawRectangle(gc, true, x + i, y + j, image.Width, image.Height);
		    }
		}
	    }

	    GdkWindow.DrawPixbuf(new Gdk.GC(GdkWindow), image, 0, 0, x, y, -1, -1, Gdk.RgbDither.Normal, 0, 0);
	}

	void DrawObject(MapObject obj, bool highlight) {
	    if (obj.Type == ObjectType.Player) {
		if (ShowPlayers) {
		    DrawTriangle(playerColor, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y), obj.Facing, highlight);
		}
	    } else if (obj.Type == ObjectType.Monster) {
		if (ShowMonsters) {
		    Drawer.Color color;
		    if ((obj.Index >= 12 && obj.Index <= 15) || (obj.Index >= 43 && obj.Index <= 46)) {
			color = civilianColor;
		    } else {
			color = monsterColor;
		    }
		    
		    DrawTriangle(color, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y), obj.Facing, highlight);
		}
	    } else if (obj.Type == ObjectType.Scenery) {
		if (ShowScenery) {
		    DrawImage(sceneryImage, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y), highlight);
		}
	    } else if (obj.Type == ObjectType.Sound) {
		if (ShowSounds) {
		    DrawImage(soundImage, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y), highlight);
		}
	    } else if (obj.Type == ObjectType.Goal) {
		if (ShowGoals) {
		    DrawImage(goalImage, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y), highlight);
		}
	    } else if (obj.Type == ObjectType.Item) {
		if (ShowObjects) {
		    if (itemImages.ContainsKey((ItemType) obj.Index) && itemImages[(ItemType) obj.Index] != null) {
			DrawImage(itemImages[(ItemType) obj.Index], Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y), highlight);
		    } else {
			drawer.DrawPoint(objectColor, new Drawer.Point(Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y)));
		    }
		}
	    }
	}
    }
}