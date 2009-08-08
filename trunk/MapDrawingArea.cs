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
    }

    public class MapDrawingArea : Gtk.DrawingArea {

	public Transform Transform = new Transform();
	public Level Level;
	public short GridResolution = 1024;

	public MapDrawingArea() { 
	    itemImages[MapObject.Item.Magnum] = new ImageSurface("resources/pistol.png");
	    itemImages[MapObject.Item.MagnumMagazine] = new ImageSurface("resources/pistol-ammo.png");
	    itemImages[MapObject.Item.PlasmaPistol] = new ImageSurface("resources/fusion.png");
	    itemImages[MapObject.Item.PlasmaMagazine] = new ImageSurface("resources/fusion-ammo.png");
	    itemImages[MapObject.Item.AssaultRifle] = new ImageSurface("resources/ar.png");
	    itemImages[MapObject.Item.AssaultRifleMagazine] = new ImageSurface("resources/ar-ammo.png");
	    itemImages[MapObject.Item.AssaultGrenadeMagazine] = new ImageSurface("resources/ar-grenades.png");
	    itemImages[MapObject.Item.MissileLauncher] = new ImageSurface("resources/rl.png");
	    itemImages[MapObject.Item.MissileLauncherMagazine] = new ImageSurface("resources/rl-ammo.png");
	    itemImages[MapObject.Item.InvisibilityPowerup] = new ImageSurface("resources/powerup.png");
	    itemImages[MapObject.Item.InvincibilityPowerup] = new ImageSurface("resources/invinc.png");
	    itemImages[MapObject.Item.InfravisionPowerup] = new ImageSurface("resources/powerup.png");
	    itemImages[MapObject.Item.AlienShotgun] = new ImageSurface("resources/alien-gun.png");
	    itemImages[MapObject.Item.Flamethrower] = new ImageSurface("resources/tozt.png");
	    itemImages[MapObject.Item.FlamethrowerCanister] = new ImageSurface("resources/tozt-ammo.png");
	    itemImages[MapObject.Item.ExtravisionPowerup] = new ImageSurface("resources/powerup.png");
	    itemImages[MapObject.Item.OxygenPowerup] = new ImageSurface("resources/oxygen.png");
	    itemImages[MapObject.Item.EnergyPowerup] = new ImageSurface("resources/1x.png");
	    itemImages[MapObject.Item.DoubleEnergyPowerup] = new ImageSurface("resources/2x.png");
	    itemImages[MapObject.Item.TripleEnergyPowerup] = new ImageSurface("resources/3x.png");
	    itemImages[MapObject.Item.Shotgun] = new ImageSurface("resources/shotgun.png");
	    itemImages[MapObject.Item.ShotgunMagazine] = new ImageSurface("resources/shotgun-ammo.png");
	    itemImages[MapObject.Item.SphtDoorKey] = new ImageSurface("resources/uplink-chip.png");
	    itemImages[MapObject.Item.Ball] = new ImageSurface("resources/skull.png");
	    itemImages[MapObject.Item.Smg] = new ImageSurface("resources/smg.png");
	    itemImages[MapObject.Item.SmgAmmo] = new ImageSurface("resources/smg-ammo.png");
	}

	Color backgroundColor = new Color(0.25, 0.25, 0.25);
	Color pointColor = new Color(1, 0, 0);
	Color solidLineColor = new Color(0, 0, 0);
	Color transparentLineColor = new Color(0, 0.75, 0.75);
	Color selectedLineColor = new Color(1, 1, 0);
	Color polygonColor = new Color(0.75, 0.75, 0.75);
	Color gridLineColor = new Color(0.5, 0.5, 0.5);
	Color gridPointColor = new Color(0, 0.75, 0.75);
	Color objectColor = new Color(1, 1, 0);
	Color playerColor = new Color(1, 1, 0);
	Color monsterColor = new Color(1, 0, 0);

	ImageSurface sceneryImage = new ImageSurface("resources/flower.png");
	ImageSurface soundImage = new ImageSurface("resources/sound.png");
	ImageSurface goalImage = new ImageSurface("resources/flag.png");

	Dictionary<MapObject.Item, ImageSurface> itemImages = new Dictionary<MapObject.Item, ImageSurface>();

	protected override bool OnExposeEvent(Gdk.EventExpose args) {
	    Context context = Gdk.CairoHelper.Create(GdkWindow);
	    
	    context.Color = backgroundColor;
	    context.Paint();
	    
	    DrawGrid(context);
	    
	    if (Level != null) {
		
		foreach (Polygon polygon in Level.Polygons) {
		    DrawPolygon(context, polygon);
		}
		
		foreach (Line line in Level.Lines) {
		    DrawLine(context, line);
		}

		if (Level.TemporaryLineStartIndex != -1) {
		    // draw the temporarily drawn line
		    context.MoveTo(Transform.ToScreenPointD(Level.Endpoints[Level.TemporaryLineStartIndex]));
		    context.LineTo(Transform.ToScreenPointD(Level.TemporaryLineEnd));
		    context.Color = selectedLineColor;
		    context.LineWidth = 1.0;
		    context.Stroke();
		}

		foreach (Point point in Level.Endpoints) {
		    DrawPoint(context, point);
		}
		
		foreach (MapObject obj in Level.Objects) {
		    DrawObject(context, obj);
		}
	    }
	    
	    ((IDisposable) context.Target).Dispose();
	    ((IDisposable) context).Dispose();
	    
	    return true;
	}

	public void Center(short X, short Y) {
	    Transform.XOffset = (short) (X - Allocation.Width / 2 / Transform.Scale);
	    Transform.YOffset = (short) (Y - Allocation.Height / 2 / Transform.Scale);
	}

	void DrawPoint(Context context, Point point) {
	    context.MoveTo(Transform.ToScreenPointD(point));
	    context.ClosePath();
	    context.LineCap = LineCap.Round;
	    context.Color = pointColor;
	    context.LineWidth = 2.5;
	    context.Stroke();
	}

	void DrawGrid(Context context) {
	    Point p1 = new Point();
	    Point p2 = new Point();

	    for (int i = 0; i < short.MaxValue; i += GridResolution) {
		p1.X = short.MinValue;
		p1.Y = (short) i;
		p2.X = short.MaxValue;
		p2.Y = (short) i;

		context.MoveTo(Transform.ToScreenPointD(p1));
		context.LineTo(Transform.ToScreenPointD(p2));

		p1.Y = (short) -i;
		p2.Y = (short) -i;
			
		context.MoveTo(Transform.ToScreenPointD(p1));
		context.LineTo(Transform.ToScreenPointD(p2));

		p1.X = (short) i;
		p1.Y = short.MinValue;
		p2.X = (short) i;
		p2.Y = short.MaxValue;

		context.MoveTo(Transform.ToScreenPointD(p1));
		context.LineTo(Transform.ToScreenPointD(p2));

		p1.X = (short) -i;
		p2.X = (short) -i;
			
		context.MoveTo(Transform.ToScreenPointD(p1));
		context.LineTo(Transform.ToScreenPointD(p2));
	    }

	    context.Color = gridLineColor;
	    context.LineWidth = 1.0;
	    context.Stroke();

	    for (int i = 0; i < short.MaxValue; i += 1024) {
		for (int j = 0; j < short.MaxValue; j += 1024) {
		    p1.X = (short) i;
		    p1.Y = (short) j;
		    
		    context.MoveTo(Transform.ToScreenPointD(p1));
		    context.ClosePath();

		    p1.X = (short) -i;

		    context.MoveTo(Transform.ToScreenPointD(p1));
		    context.ClosePath();

		    p1.Y = (short) -j;
		    
		    context.MoveTo(Transform.ToScreenPointD(p1));
		    context.ClosePath();

		    p1.X = (short) i;
		    
		    context.MoveTo(Transform.ToScreenPointD(p1));
		    context.ClosePath();
		}
	    }

	    context.LineCap = LineCap.Round;
	    context.Color = gridPointColor;
	    context.LineWidth = 2.0;
	    context.Stroke();
	}		
	    

	void DrawLine(Context context, Line line) {
	    Point p1 = Level.Endpoints[line.EndpointIndexes[0]];
	    Point p2 = Level.Endpoints[line.EndpointIndexes[1]];
	    
	    context.MoveTo(Transform.ToScreenPointD(p1));
	    context.LineTo(Transform.ToScreenPointD(p2));
	    if (line.ClockwisePolygonOwner != -1 && line.CounterclockwisePolygonOwner != -1) {
		context.Color = transparentLineColor;
	    } else {
		context.Color = solidLineColor;
	    }

	    context.LineWidth = 1.0;
	    context.Stroke();
	}

	void DrawPolygon(Context context, Polygon polygon) {
	    Point p = Level.Endpoints[polygon.EndpointIndexes[0]];
	    context.MoveTo(Transform.ToScreenPointD(p));
	    for (int i = 1; i < polygon.VertexCount; ++i) {
		context.LineTo(Transform.ToScreenPointD(Level.Endpoints[polygon.EndpointIndexes[i]]));
	    }

	    context.Color = polygonColor;
	    context.ClosePath();
	    context.Fill();
	}

	void DrawTriangle(Context context, double X, double Y, double angle) {
	    double rads = angle * Math.PI / 180;
	    PointD p1 = new PointD(X + 8 * Math.Cos(rads), Y + 8 * Math.Sin(rads));
	    PointD p2 = new PointD(X + 10 * Math.Cos(rads + 2 * Math.PI * 0.4), Y + 10 * Math.Sin(rads + 2 * Math.PI * 0.4));
	    PointD p3 = new PointD(X + 10 * Math.Cos(rads - 2 * Math.PI * 0.4), Y + 10 * Math.Sin(rads - 2 * Math.PI * 0.4));

	    context.MoveTo(p1);
	    context.LineTo(p2);
	    context.LineTo(p3);
	    context.ClosePath();
	    context.FillPreserve();
	    context.Color = new Color(0, 0, 0);
	    context.LineWidth = 1.0;
	    context.Stroke();
	}

	void DrawImage(Context context, ImageSurface surface, double X, double Y) {
	    context.SetSourceSurface(surface, (int) X - surface.Width / 2, (int) Y - surface.Height / 2);
	    context.Paint();
	}

	void DrawObject(Context context, MapObject obj) {
	    if (obj.Type == MapObject.Types.Player) {
		context.Color = playerColor;
		DrawTriangle(context, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y), obj.Facing * 360 / 512);
	    } else if (obj.Type == MapObject.Types.Monster) {
		context.Color = monsterColor;
		DrawTriangle(context, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y), obj.Facing * 360 / 512);
	    } else if (obj.Type == MapObject.Types.Scenery) {
		DrawImage(context, sceneryImage, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y));
	    } else if (obj.Type == MapObject.Types.Sound) {
		DrawImage(context, soundImage, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y));
	    } else if (obj.Type == MapObject.Types.Goal) {
		DrawImage(context, goalImage, Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y));
	    } else if (obj.Type == MapObject.Types.Item) {
		if (itemImages.ContainsKey((MapObject.Item) obj.Index)) {
		    DrawImage(context, itemImages[(MapObject.Item) obj.Index], Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y));
		} else {
		    PointD p = new PointD(Transform.ToScreenX(obj.X), Transform.ToScreenY(obj.Y));
		    context.MoveTo(p);
		    context.ClosePath();
		    context.LineCap = LineCap.Round;
		    context.Color = objectColor;
		    context.LineWidth = 2.5;
		    context.Stroke();
		}
	    }
	}
    }
}