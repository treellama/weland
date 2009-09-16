using Glade;
using Gtk;
using System;

namespace Weland {
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

	bool applyObjectChanges = true;

	void UpdateInspector() {
	    applyObjectChanges = false;
	    if (selection.Object != -1) {
		MapObject mapObject = Level.Objects[selection.Object];
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
		    itemType.Active = mapObject.Index;
		    itemHeight.Text = String.Format("{0:0.000}", World.ToDouble(mapObject.Z));
		    itemFromCeiling.Active = mapObject.FromCeiling;
		    itemTeleportsIn.Active = mapObject.Invisible;
		    itemNetworkOnly.Active = mapObject.NetworkOnly;
		}
	    } else {
		inspector.Hide();
	    }
	    applyObjectChanges = true;
	}

	void RedrawSelectedObject() {
	    const int slop = 12;
	    MapObject mapObject = Level.Objects[selection.Object];
	    drawingArea.QueueDrawArea((int) drawingArea.Transform.ToScreenX(mapObject.X) - slop, (int) drawingArea.Transform.ToScreenY(mapObject.Y) - slop, slop * 2, slop * 2);	    
	}

	protected void OnObjectChanged(object obj, EventArgs args) {
	    if (!applyObjectChanges) return;
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
		mapObject.Index = (short) itemType.Active;
		try {
		    mapObject.Z = World.FromDouble(double.Parse(itemHeight.Text));
		} catch (Exception) { }
		mapObject.FromCeiling = itemFromCeiling.Active;
		mapObject.Invisible = itemTeleportsIn.Active;
		mapObject.NetworkOnly = itemNetworkOnly.Active;
	    }

	    // increment item and monster placement counts
	    if (mapObject.Type == ObjectType.Monster) {
		Level.MonsterPlacement[mapObject.Index].InitialCount++;
	    } else if (mapObject.Type == ObjectType.Item) {
		Level.ItemPlacement[mapObject.Index].InitialCount++;
	    }

	    RedrawSelectedObject();
	}

	protected void OnObjectTypeChanged(object obj, EventArgs args) {
	    if (!applyObjectChanges) return;

	    MapObject mapObject = Level.Objects[selection.Object];
	    mapObject.Type = (ObjectType) objectGroup.Active;
	    RedrawSelectedObject();
	    UpdateInspector();
	}
    }
}