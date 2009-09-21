using Glade;
using Gtk;
using System;

namespace Weland {
    public enum InspectorPage {
	Object,
	Polygon
    }

    public enum PolygonPage {
	Normal,
	Base,
	Platform
    }

    public partial class MapWindow {
	[Widget] Notebook inspector;
	[Widget] ComboBox objectGroup;
	[Widget] Notebook objectNotebook;

	[Widget] ComboBox monsterType;
	[Widget] ComboBox monsterActivatedBy;
	[Widget] HScale monsterAngle;
	[Widget] Entry monsterHeight;
	[Widget] CheckButton monsterFromCeiling;
	[Widget] CheckButton monsterTeleportsIn;
	[Widget] CheckButton monsterTeleportsOut;
	[Widget] CheckButton monsterIsBlind;
	[Widget] CheckButton monsterIsDeaf;

	[Widget] HScale playerAngle;
	[Widget] CheckButton playerFromCeiling;
	[Widget] Entry playerHeight;

	[Widget] ComboBox sceneryType;
	[Widget] HScale sceneryAngle;
	[Widget] Entry sceneryHeight;
	[Widget] CheckButton sceneryFromCeiling;

	[Widget] ComboBox itemType;
	[Widget] Entry itemHeight;
	[Widget] CheckButton itemFromCeiling;
	[Widget] CheckButton itemTeleportsIn;
	[Widget] CheckButton itemNetworkOnly;

	[Widget] ComboBox goalType;

	[Widget] ComboBox soundType;
	[Widget] HScale soundVolume;
	[Widget] ComboBox soundLight;
	ListStore soundLightStore = new ListStore(typeof(int));
	[Widget] Entry soundHeight;
	[Widget] CheckButton soundFromCeiling;
	[Widget] CheckButton soundIsOnPlatform;
	[Widget] CheckButton soundFloats;
	[Widget] CheckButton soundUseLight;

	[Widget] ComboBox polygonType;
	[Widget] Notebook polygonNotebook;
	[Widget] ComboBox baseTeam;

	bool applyChanges = true;

	void SetupInspector() {
	    soundLight.Model = soundLightStore;
	    CellRendererText text = new CellRendererText();
	    soundLight.PackStart(text, false);
	    soundLight.AddAttribute(text, "text", 0);
	}

	void UpdateInspector() {
	    applyChanges = false;
	    if (selection.Object != -1) {
		MapObject mapObject = Level.Objects[selection.Object];
		inspector.CurrentPage = (int) InspectorPage.Object;
		inspector.Show();
		objectGroup.Active = (int) mapObject.Type;
		objectNotebook.CurrentPage = (int) mapObject.Type;
		if (mapObject.Type == ObjectType.Monster) {
		    monsterType.Active = mapObject.Index - 1;
		    monsterActivatedBy.Active = (int) mapObject.ActivationBias;
		    monsterAngle.Value = mapObject.Facing;
		    monsterHeight.Text = String.Format("{0:0.000}", World.ToDouble(mapObject.Z));
		    monsterFromCeiling.Active = mapObject.FromCeiling;
		    monsterTeleportsIn.Active = mapObject.Invisible;
		    monsterTeleportsOut.Active = mapObject.Floats;
		    monsterIsBlind.Active = mapObject.Blind;
		    monsterIsDeaf.Active = mapObject.Deaf;
		} else if (mapObject.Type == ObjectType.Player) {
		    playerAngle.Value = mapObject.Facing;
		    playerHeight.Text = String.Format("{0:0.000}", World.ToDouble(mapObject.Z));
		    playerFromCeiling.Active = mapObject.FromCeiling;
		} else if (mapObject.Type == ObjectType.Scenery) {
		    sceneryType.Active = mapObject.Index;
		    sceneryAngle.Value = mapObject.Facing;
		    sceneryHeight.Text = String.Format("{0:0.000}", World.ToDouble(mapObject.Z));
		    sceneryFromCeiling.Active = mapObject.FromCeiling;
		} else if (mapObject.Type == ObjectType.Item) {
		    itemType.Active = mapObject.Index - 1;
		    itemHeight.Text = String.Format("{0:0.000}", World.ToDouble(mapObject.Z));
		    itemFromCeiling.Active = mapObject.FromCeiling;
		    itemTeleportsIn.Active = mapObject.Invisible;
		    itemNetworkOnly.Active = mapObject.NetworkOnly;
		} else if (mapObject.Type == ObjectType.Goal) {
		    goalType.Active = mapObject.Index;
		} else if (mapObject.Type == ObjectType.Sound) {
		    soundType.Active = mapObject.Index;
		    soundHeight.Text = String.Format("{0:0.000}", World.ToDouble(mapObject.Z));
		    soundFromCeiling.Active = mapObject.FromCeiling;
		    soundIsOnPlatform.Active = mapObject.OnPlatform;
		    soundFloats.Active = mapObject.Floats;
		    soundUseLight.Active = mapObject.UseLightForVolume;
		    if (!mapObject.UseLightForVolume) {
			soundVolume.Sensitive = true;
			soundLight.Sensitive = false;
			soundLight.Active = 0;
			soundVolume.Value = mapObject.Volume;
		    } else {
			soundLight.Sensitive = true;
			soundLightStore.Clear();
			for (int i = 0; i < Level.Lights.Count; ++i) {
			    soundLightStore.AppendValues(i);
			}
			soundLight.Active = mapObject.Light;
			soundVolume.Sensitive = false;
			soundVolume.Value = 1;
		    }
		}
	    } else if (selection.Polygon != -1) {
		inspector.CurrentPage = (int) InspectorPage.Polygon;
		Polygon polygon = Level.Polygons[selection.Polygon];
		polygonType.Active = (int) polygon.Type;
		switch (polygon.Type) {
		case PolygonType.Base:
		    polygonNotebook.CurrentPage = (int) PolygonPage.Base;
		    baseTeam.Active = polygon.Permutation;
		    break;
		case PolygonType.Platform:
		    polygonNotebook.CurrentPage = (int) PolygonPage.Platform;
		    break;
		default:
		    polygonNotebook.CurrentPage = (int) PolygonPage.Normal;
		    break;
		}
		inspector.Show();
	    } else {
		inspector.Hide();
	    }
	    applyChanges = true;
	}

	void RedrawSelectedObject() {
	    const int slop = 12;
	    MapObject mapObject = Level.Objects[selection.Object];
	    drawingArea.QueueDrawArea((int) drawingArea.Transform.ToScreenX(mapObject.X) - slop, (int) drawingArea.Transform.ToScreenY(mapObject.Y) - slop, slop * 2, slop * 2);	    
	}

	protected void OnObjectChanged(object obj, EventArgs args) {
	    if (!applyChanges) return;
	    MapObject mapObject = Level.Objects[selection.Object];

	    // decrement item and monster initial counts
	    if (mapObject.Type == ObjectType.Monster && mapObject.Index >= 0 && mapObject.Index < Level.MonsterPlacement.Count && Level.MonsterPlacement[mapObject.Index].InitialCount > 0) {
		Level.MonsterPlacement[mapObject.Index].InitialCount--;
	    } else if (mapObject.Type == ObjectType.Item && mapObject.Index >= 0 && mapObject.Index < Level.ItemPlacement.Count && Level.ItemPlacement[mapObject.Index].InitialCount > 0) {
		Level.ItemPlacement[mapObject.Index].InitialCount--;
	    }

	    if (mapObject.Type == ObjectType.Monster) {
		mapObject.Index = (short) (monsterType.Active + 1);
		mapObject.ActivationBias = (ActivationBias) monsterActivatedBy.Active;
		mapObject.Facing = monsterAngle.Value;
		try {
		    mapObject.Z = World.FromDouble(double.Parse(monsterHeight.Text));
		} catch (Exception) { }
		mapObject.FromCeiling = monsterFromCeiling.Active;
		mapObject.Invisible = monsterTeleportsIn.Active;
		mapObject.Floats = monsterTeleportsOut.Active;
		mapObject.Blind = monsterIsBlind.Active;
		mapObject.Deaf = monsterIsDeaf.Active;
	    } else if (mapObject.Type == ObjectType.Player) {
		mapObject.Facing = playerAngle.Value;
		try {
		    mapObject.Z = World.FromDouble(double.Parse(playerHeight.Text));
		} catch (Exception) { }

		mapObject.FromCeiling = playerFromCeiling.Active;
	    } else if (mapObject.Type == ObjectType.Scenery) {
		mapObject.Index = (short) sceneryType.Active;
		mapObject.Facing = sceneryAngle.Value;
		try {
		    mapObject.Z = World.FromDouble(double.Parse(sceneryHeight.Text));
		} catch (Exception) { }
		mapObject.FromCeiling = sceneryFromCeiling.Active;
	    } else if (mapObject.Type == ObjectType.Item) {
		mapObject.Index = (short) (itemType.Active + 1);
		try {
		    mapObject.Z = World.FromDouble(double.Parse(itemHeight.Text));
		} catch (Exception) { }
		mapObject.FromCeiling = itemFromCeiling.Active;
		mapObject.Invisible = itemTeleportsIn.Active;
		mapObject.NetworkOnly = itemNetworkOnly.Active;
	    } else if (mapObject.Type == ObjectType.Goal) {
		mapObject.Index = (short) goalType.Active;
	    } else if (mapObject.Type == ObjectType.Sound) {
		mapObject.Index = (short) soundType.Active;
		try {
		    mapObject.Z = World.FromDouble(double.Parse(soundHeight.Text));
		} catch (Exception) {}
		mapObject.FromCeiling = soundFromCeiling.Active;
		mapObject.OnPlatform = soundIsOnPlatform.Active;
		mapObject.Floats = soundFloats.Active;
		if (soundUseLight.Active) {
		    mapObject.Light = soundLight.Active;
		} else {
		    mapObject.Volume = (int) soundVolume.Value;
		}
	    }

	    // increment item and monster placement counts
	    if (mapObject.Type == ObjectType.Monster) {
		Level.MonsterPlacement[mapObject.Index].InitialCount++;
	    } else if (mapObject.Type == ObjectType.Item) {
		Level.ItemPlacement[mapObject.Index].InitialCount++;
	    }

	    RedrawSelectedObject();
	}

	protected void OnPlatformParameters(object obj, EventArgs args) {
	    for (int i = 0; i < Level.Platforms.Count; ++i) {
		Platform platform = Level.Platforms[i];
		if (platform.PolygonIndex == selection.Polygon) {
		    PlatformParametersDialog d = new PlatformParametersDialog(window1, Level, (short) i);
		    d.Run();
		    break;
		}
	    }
	}

	protected void OnPolygonChanged(object obj, EventArgs args) {
	    Polygon polygon = Level.Polygons[selection.Polygon];
	    if (polygon.Type == PolygonType.Base) {
		polygon.Permutation = (short) baseTeam.Active;
	    }
	}

	protected void OnObjectTypeChanged(object obj, EventArgs args) {
	    if (!applyChanges) return;

	    MapObject mapObject = Level.Objects[selection.Object];
	    mapObject.Type = (ObjectType) objectGroup.Active;
	    RedrawSelectedObject();
	    UpdateInspector();
	}
	
	protected void OnPolygonTypeChanged(object obj, EventArgs args) {
	    if (!applyChanges) return;

	    Polygon polygon = Level.Polygons[selection.Polygon];
	    bool scan = (polygon.Type == PolygonType.Platform || (PolygonType) polygonType.Active == PolygonType.Platform);
	    polygon.Type = (PolygonType) polygonType.Active;
	    if (scan) {
		editor.ScanPlatforms();
	    }
	    UpdateInspector();
	}

	protected void OnUseLightToggled(object obj, EventArgs args) {
	    if (!soundUseLight.Active) {
		soundVolume.Sensitive = true;
		soundLight.Sensitive = false;
	    } else {
		soundLight.Sensitive = true;
		soundLightStore.Clear();
		for (int i = 0; i < Level.Lights.Count; ++i) {
		    soundLightStore.AppendValues(i);
		}
		soundVolume.Sensitive = false;
	    }
	    soundLight.Active = 0;
	    soundVolume.Value = 1;
	    OnObjectChanged(obj, args);
	}
    }
}