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