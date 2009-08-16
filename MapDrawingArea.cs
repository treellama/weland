using System;
using Cairo;
using System.Collections.Generic;

namespace Weland {
    public class Transform {
	public Transform() { }
	public double Scale = 1.0 / 32.0;
	public short XOffset = 0;
	public short YOffset = 0;

	public double ToScreenX(short x) { 
	    return (x - XOffset) * Scale;
	}

	public double ToScreenY(short y) {
	    return (y - YOffset) * Scale;
	}

	public short ToMapX(double X) {
	    return (short) ((double) (X / Scale) + XOffset);
	}

	public short ToMapY(double Y) {
	    return (short) ((double) (Y / Scale) + YOffset);
	}

	public PointD ToScreenPointD(Point p) {
	    return new PointD(ToScreenX(p.X) + 0.5, ToScreenY(p.Y) + 0.5);
	}

	public Drawer.Point ToScreenPoint(Point p) {
	    return new Drawer.Point(ToScreenX(p.X) + 0.5, ToScreenY(p.Y) + 0.5);
	}
    }

    public class MapDrawingArea : Gtk.DrawingArea {

	public Transform Transform = new Transform();
	public Level Level;
	public short GridResolution = 1024;
	public bool ShowGrid = true;

	Drawer drawer;

	public MapDrawingArea() { 
	    itemImages[ItemType.Magnum] = new Gdk.Pixbuf("resources/pistol.png");
	    itemImages[ItemType.MagnumMagazine] = new Gdk.Pixbuf("resources/pistol-ammo.png");
	    itemImages[ItemType.PlasmaPistol] = new Gdk.Pixbuf("resources/fusion.png");
	    itemImages[ItemType.PlasmaMagazine] = new Gdk.Pixbuf("resources/fusion-ammo.png");
	    itemImages[ItemType.AssaultRifle] = new Gdk.Pixbuf("resources/ar.png");
	    itemImages[ItemType.AssaultRifleMagazine] = new Gdk.Pixbuf("resources/ar-ammo.png");
	    itemImages[ItemType.AssaultGrenadeMagazine] = new Gdk.Pixbuf("resources/ar-grenades.png");
	    itemImages[ItemType.MissileLauncher] = new Gdk.Pixbuf("resources/rl.png");
	    itemImages[ItemType.MissileLauncherMagazine] = new Gdk.Pixbuf("resources/rl-ammo.png");
	    itemImages[ItemType.InvisibilityPowerup] = new Gdk.Pixbuf("resources/powerup.png");
	    itemImages[ItemType.InvincibilityPowerup] = new Gdk.Pixbuf("resources/invinc.png");
	    itemImages[ItemType.InfravisionPowerup] = new Gdk.Pixbuf("resources/powerup.png");
	    itemImages[ItemType.AlienShotgun] = new Gdk.Pixbuf("resources/alien-gun.png");
	    itemImages[ItemType.Flamethrower] = new Gdk.Pixbuf("resources/tozt.png");
	    itemImages[ItemType.FlamethrowerCanister] = new Gdk.Pixbuf("resources/tozt-ammo.png");
	    itemImages[ItemType.ExtravisionPowerup] = new Gdk.Pixbuf("resources/powerup.png");
	    itemImages[ItemType.OxygenPowerup] = new Gdk.Pixbuf("resources/oxygen.png");
	    itemImages[ItemType.EnergyPowerup] = new Gdk.Pixbuf("resources/1x.png");
	    itemImages[ItemType.DoubleEnergyPowerup] = new Gdk.Pixbuf("resources/2x.png");
	    itemImages[ItemType.TripleEnergyPowerup] = new Gdk.Pixbuf("resources/3x.png");
	    itemImages[ItemType.Shotgun] = new Gdk.Pixbuf("resources/shotgun.png");
	    itemImages[ItemType.ShotgunMagazine] = new Gdk.Pixbuf("resources/shotgun-ammo.png");
	    itemImages[ItemType.SphtDoorKey] = new Gdk.Pixbuf("resources/uplink-chip.png");
	    itemImages[ItemType.RedBall] = new Gdk.Pixbuf("resources/skull.png");
	    itemImages[ItemType.Smg] = new Gdk.Pixbuf("resources/smg.png");
	    itemImages[ItemType.SmgAmmo] = new Gdk.Pixbuf("resources/smg-ammo.png");
	}

	Drawer.Color backgroundColor = new Drawer.Color(0.25, 0.25, 0.25);
	Drawer.Color pointColor = new Drawer.Color(1, 0, 0);
	Drawer.Color solidLineColor = new Drawer.Color(0, 0, 0);
	Drawer.Color transparentLineColor = new Drawer.Color(0, 0.75, 0.75);
	Drawer.Color selectedLineColor = new Drawer.Color(1, 1, 0);
	Drawer.Color polygonColor = new Drawer.Color(0.75, 0.75, 0.75);
	Drawer.Color gridLineColor = new Drawer.Color(0.5, 0.5, 0.5);
	Drawer.Color gridPointColor = new Drawer.Color(0, 0.75, 0.75);
	Drawer.Color objectColor = new Drawer.Color(1, 1, 0);
	Drawer.Color playerColor = new Drawer.Color(1, 1, 0);
	Drawer.Color monsterColor = new Drawer.Color(1, 0, 0);

	Gdk.Pixbuf sceneryImage = new Gdk.Pixbuf("resources/flower.png");
	Gdk.Pixbuf soundImage = new Gdk.Pixbuf("resources/sound.png");
	Gdk.Pixbuf goalImage = new Gdk.Pixbuf("resources/flag.png");

	Dictionary<ItemType, Gdk.Pixbuf> itemImages = new Dictionary<ItemType, Gdk.Pixbuf>();

	protected override bool OnExposeEvent(Gdk.EventExpose args) {
	    drawer = new CairoDrawer(GdkWindow);

	    drawer.Clear(backgroundColor);
	    
	    if (ShowGrid) {
		DrawGrid();
	    }
	    
	    if (Level != null) {
		
		foreach (Polygon polygon in Level.Polygons) {
		    DrawPolygon(polygon);
		}
		
		foreach (Line line in Level.Lines) {
		    DrawLine(line);
		}

		if (Level.TemporaryLineStartIndex != -1) {
		    // draw the temporarily drawn line
		    drawer.DrawLine(selectedLineColor, Transform.ToScreenPoint(Level.Endpoints[Level.TemporaryLineStartIndex]), Transform.ToScreenPoint(Level.TemporaryLineEnd));
		}

		foreach (Point point in Level.Endpoints) {
		    DrawPoint(point);
		}
		
		foreach (MapObject obj in Level.Objects) {
		    DrawObject(obj);
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

	void DrawGrid() {
	    Point p1 = new Point();
	    Point p2 = new Point();

	    for (int i = 0; i < short.MaxValue; i += GridResolution) {
		p1.X = short.MinValue;
		p1.Y = (short) i;
		p2.X = short.MaxValue;
		p2.Y = (short) i;

		drawer.DrawLine(gridLineColor, Transform.ToScreenPoint(p1), Transform.ToScreenPoint(p2));

		p1.Y = (short) -i;
		p2.Y = (short) -i;

		drawer.DrawLine(gridLineColor, Transform.ToScreenPoint(p1), Transform.ToScreenPoint(p2));

		p1.X = (short) i;
		p1.Y = short.MinValue;
		p2.X = (short) i;
		p2.Y = short.MaxValue;

		drawer.DrawLine(gridLineColor, Transform.ToScreenPoint(p1), Transform.ToScreenPoint(p2));

		p1.X = (short) -i;
		p2.X = (short) -i;
			
		drawer.DrawLine(gridLineColor, Transform.ToScreenPoint(p1), Transform.ToScreenPoint(p2));
	    }

	    for (int i = 0; i < short.MaxValue; i += 1024) {
		for (int j = 0; j < short.MaxValue; j += 1024) {
		    p1.X = (short) i;
		    p1.Y = (short) j;
		    drawer.DrawGridIntersect(gridPointColor, Transform.ToScreenPoint(p1));

		    p1.X = (short) -i;
		    drawer.DrawGridIntersect(gridPointColor, Transform.ToScreenPoint(p1));

		    p1.Y = (short) -j;
		    drawer.DrawGridIntersect(gridPointColor, Transform.ToScreenPoint(p1));

		    p1.X = (short) i;
		    drawer.DrawGridIntersect(gridPointColor, Transform.ToScreenPoint(p1));
		}
	    }
	}		
	    

	void DrawLine(Line line) {
	    Point p1 = Level.Endpoints[line.EndpointIndexes[0]];
	    Point p2 = Level.Endpoints[line.EndpointIndexes[1]];

	    Drawer.Color color;
	    if (line.ClockwisePolygonOwner != -1 && line.CounterclockwisePolygonOwner != -1) {
		color = transparentLineColor;
	    } else {
		color = solidLineColor;
	    }

	    drawer.DrawLine(color, Transform.ToScreenPoint(p1), Transform.ToScreenPoint(p2));
	}

	void DrawPolygon(Polygon polygon) {
	    List<Drawer.Point> points = new List<Drawer.Point>();
	    for (int i = 0; i < polygon.VertexCount; ++i) {
		points.Add(Transform.ToScreenPoint(Level.Endpoints[polygon.EndpointIndexes[i]]));
	    }
	    drawer.FillPolygon(polygonColor, points);
	}
	
	void DrawTriangle(Drawer.Color c, double X, double Y, double angle) {
	    double rads = angle * Math.PI / 180;
	    List<Drawer.Point> points = new List<Drawer.Point>();
	    points.Add(new Drawer.Point(X + 8 * Math.Cos(rads), Y + 8 * Math.Sin(rads)));
	    points.Add(new Drawer.Point(X + 10 * Math.Cos(rads + 2 * Math.PI * 0.4), Y + 10 * Math.Sin(rads + 2 * Math.PI * 0.4)));
	    points.Add(new Drawer.Point(X + 10 * Math.Cos(rads - 2 * Math.PI * 0.4), Y + 10 * Math.Sin(rads - 2 * Math.PI * 0.4)));
	    
	    drawer.FillStrokePolygon(c, new Drawer.Color(0, 0, 0), points);
	}

	void DrawImage(Gdk.Pixbuf image, double X, double Y) {
	    GdkWindow.DrawPixbuf(new Gdk.GC(GdkWindow), image, 0, 0, (int) X - image.Width / 2, (int) Y - image.Height / 2, -1, -1, Gdk.RgbDither.Normal, 0, 0);
	}

	void DrawObject(MapObject obj) {
	    if (obj.Type == ObjectType.Player) {
		DrawTriangle(playerColor, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y), obj.Facing * 360 / 512);
	    } else if (obj.Type == ObjectType.Monster) {
		DrawTriangle(monsterColor, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y), obj.Facing * 360 / 512);
	    } else if (obj.Type == ObjectType.Scenery) {
		DrawImage(sceneryImage, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y));
	    } else if (obj.Type == ObjectType.Sound) {
		DrawImage(soundImage, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y));
	    } else if (obj.Type == ObjectType.Goal) {
		DrawImage(goalImage, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y));
	    } else if (obj.Type == ObjectType.Item) {
		if (itemImages.ContainsKey((ItemType) obj.Index) && itemImages[(ItemType) obj.Index] != null) {
		    DrawImage(itemImages[(ItemType) obj.Index], Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y));
		} else {
		    drawer.DrawPoint(objectColor, new Drawer.Point(Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y)));
		}
	    }
	}
    }
}