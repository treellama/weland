using Gtk;
using Glade;
using System;

namespace Weland {
    public class LevelParametersDialog {
	public LevelParametersDialog(Window parent, Level theLevel) {
	    level = theLevel;
	    Glade.XML gxml = new Glade.XML(null, "levelparameters.glade", "dialog1", null);
	    gxml.Autoconnect(this);
	    dialog1.TransientFor = parent;
	}

	public void Run() {
	    levelName.Text = level.Name;
	    environment.Active = level.Environment;
	    landscape.Active = level.Landscape;

	    vacuum.Active = level.Vacuum;
	    magnetic.Active = level.Magnetic;
	    rebellion.Active = level.Rebellion;
	    lowGravity.Active = level.LowGravity;
	    
	    extermination.Active = level.Extermination;
	    exploration.Active = level.Exploration;
	    retrieval.Active = level.Retrieval;
	    repair.Active = level.Repair;
	    rescue.Active = level.Rescue;

	    solo.Active = level.SinglePlayer;
	    coop.Active = level.MultiplayerCooperative;
	    emfh.Active = level.MultiplayerCarnage;
	    ktmwtb.Active = level.KillTheManWithTheBall;
	    koth.Active = level.KingOfTheHill;
	    rugby.Active = level.Rugby;
	    ctf.Active = level.CaptureTheFlag;

	    int response = dialog1.Run();

	    if (response == (int) ResponseType.Ok) {
		level.Name = levelName.Text;
		level.Environment = (short) environment.Active;
		level.Landscape = (short) landscape.Active;

		level.Vacuum = vacuum.Active;
		level.Magnetic = magnetic.Active;
		level.Rebellion = rebellion.Active;
		level.LowGravity = lowGravity.Active;

		level.Extermination = extermination.Active;
		level.Exploration = exploration.Active;
		level.Retrieval = retrieval.Active;
		level.Repair = repair.Active;
		level.Rescue = rescue.Active;

		level.SinglePlayer = solo.Active;
		level.MultiplayerCooperative = coop.Active;
		level.MultiplayerCarnage = emfh.Active;
		level.KillTheManWithTheBall = ktmwtb.Active;
		level.KingOfTheHill = koth.Active;
		level.Rugby = rugby.Active;
		level.CaptureTheFlag = ctf.Active;
	    }

	    dialog1.Destroy();
	}

	Level level;

	[Widget] Dialog dialog1;

	[Widget] Entry levelName;
	[Widget] ComboBox environment;
	[Widget] ComboBox landscape;

	[Widget] CheckButton vacuum;
	[Widget] CheckButton magnetic;
	[Widget] CheckButton rebellion;
	[Widget] CheckButton lowGravity;

	[Widget] CheckButton extermination;
	[Widget] CheckButton exploration;
	[Widget] CheckButton retrieval;
	[Widget] CheckButton repair;
	[Widget] CheckButton rescue;

	[Widget] CheckButton solo;
	[Widget] CheckButton coop;
	[Widget] CheckButton emfh;
	[Widget] CheckButton ktmwtb;
	[Widget] CheckButton koth;
	[Widget] CheckButton rugby;
	[Widget] CheckButton ctf;
    }
}