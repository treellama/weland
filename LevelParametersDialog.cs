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
	    levelName.Text = level.MapInfo.Name;
	    environment.Active = level.MapInfo.Environment;
	    landscape.Active = level.MapInfo.Landscape;
	    vacuum.Active = (level.MapInfo.EnvironmentFlags & EnvironmentFlags.Vacuum) == EnvironmentFlags.Vacuum;
	    magnetic.Active = (level.MapInfo.EnvironmentFlags & EnvironmentFlags.Magnetic) == EnvironmentFlags.Magnetic;
	    rebellion.Active = (level.MapInfo.EnvironmentFlags & EnvironmentFlags.Rebellion) == EnvironmentFlags.Rebellion;
	    lowGravity.Active = (level.MapInfo.EnvironmentFlags & EnvironmentFlags.LowGravity) == EnvironmentFlags.LowGravity;
	    
	    extermination.Active = (level.MapInfo.MissionFlags & MissionFlags.Extermination) == MissionFlags.Extermination;
	    exploration.Active = (level.MapInfo.MissionFlags & MissionFlags.Exploration) == MissionFlags.Exploration;
	    retrieval.Active = (level.MapInfo.MissionFlags & MissionFlags.Retrieval) == MissionFlags.Retrieval;
	    repair.Active = (level.MapInfo.MissionFlags & MissionFlags.Repair) == MissionFlags.Repair;
	    rescue.Active = (level.MapInfo.MissionFlags & MissionFlags.Rescue) == MissionFlags.Rescue;

	    solo.Active = (level.MapInfo.EntryPointFlags & EntryPointFlags.SinglePlayer) == EntryPointFlags.SinglePlayer;
	    coop.Active = (level.MapInfo.EntryPointFlags & EntryPointFlags.MultiplayerCooperative) == EntryPointFlags.MultiplayerCooperative;
	    emfh.Active = (level.MapInfo.EntryPointFlags & EntryPointFlags.MultiplayerCarnage) == EntryPointFlags.MultiplayerCarnage;
	    ktmwtb.Active = (level.MapInfo.EntryPointFlags & EntryPointFlags.KillTheManWithTheBall) == EntryPointFlags.KillTheManWithTheBall;
	    koth.Active = (level.MapInfo.EntryPointFlags & EntryPointFlags.KingOfTheHill) == EntryPointFlags.KingOfTheHill;
	    rugby.Active = (level.MapInfo.EntryPointFlags & EntryPointFlags.Rugby) == EntryPointFlags.Rugby;
	    ctf.Active = (level.MapInfo.EntryPointFlags & EntryPointFlags.CaptureTheFlag) == EntryPointFlags.CaptureTheFlag;

	    int response = dialog1.Run();

	    if (response == (int) ResponseType.Ok) {
		level.MapInfo.Name = levelName.Text;
		level.MapInfo.Environment = (short) environment.Active;
		level.MapInfo.Landscape = (short) landscape.Active;

		{
		    EnvironmentFlags flags = 0;
		    if (vacuum.Active)
			flags |= EnvironmentFlags.Vacuum;
		    if (magnetic.Active) 
			flags |= EnvironmentFlags.Magnetic;
		    if (rebellion.Active) 
			flags |= EnvironmentFlags.Rebellion;
		    if (lowGravity.Active) 
			flags |= EnvironmentFlags.LowGravity;

		    level.MapInfo.EnvironmentFlags = flags;
		}

		{ 
		    MissionFlags flags = 0;
		    if (extermination.Active) 
			flags |= MissionFlags.Extermination;
		    if (exploration.Active) 
			flags |= MissionFlags.Exploration;
		    if (retrieval.Active) 
			flags |= MissionFlags.Retrieval;
		    if (repair.Active) 
			flags |= MissionFlags.Repair;
		    if (rescue.Active) 
			flags |= MissionFlags.Rescue;

		    level.MapInfo.MissionFlags = flags;
		}

		{ 
		    EntryPointFlags flags = 0;
		    if (solo.Active) 
			flags |= EntryPointFlags.SinglePlayer;
		    if (coop.Active) 
			flags |= EntryPointFlags.MultiplayerCooperative;
		    if (emfh.Active) 
			flags |= EntryPointFlags.MultiplayerCarnage;
		    if (ktmwtb.Active) 
			flags |= EntryPointFlags.KillTheManWithTheBall;
		    if (koth.Active) 
			flags |= EntryPointFlags.KingOfTheHill;
		    if (rugby.Active) 
			flags |= EntryPointFlags.Rugby;
		    if (ctf.Active) 
			flags |= EntryPointFlags.CaptureTheFlag;

		    level.MapInfo.EntryPointFlags = flags;
		}
		
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