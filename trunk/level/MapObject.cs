using System;

namespace Weland {
    public enum ObjectType {
	Monster,
	Scenery,
	Item,
	Player,
	Goal,
	Sound
    }

    public enum ItemType {
	Knife,
	Magnum,
	MagnumMagazine,
	PlasmaPistol,
	PlasmaMagazine,
	AssaultRifle,
	AssaultRifleMagazine,
	AssaultGrenadeMagazine,
	MissileLauncher,
	MissileLauncherMagazine,
	InvisibilityPowerup,
	InvincibilityPowerup,
	InfravisionPowerup,
	AlienShotgun,
	AlienShotgunMagazine,
	Flamethrower,
	FlamethrowerCanister,
	ExtravisionPowerup,
	OxygenPowerup,
	EnergyPowerup,
	DoubleEnergyPowerup,
	TripleEnergyPowerup,
	Shotgun,
	ShotgunMagazine,
	SphtDoorKey,
	UplinkChip,
	LightBlueBall,
	RedBall,
	VioletBall,
	YellowBall,
	BrownBall,
	OrangeBall,
	BlueBall, // heh heh
	GreenBall,
	Smg,
	SmgAmmo
    }

    public enum MonsterType {
	Marine,
	TickEnergy,
	TickOxygen,
	TickKamakazi,
	CompilerMinor,
	CompilerMajor,
	CompilerMinorInvisible,
	CompilerMajorInvisible,
	FighterMinor,
	FighterMajor,
	FighterMinorProjectile,
	FighterMajorProjectile,
	CivilianCrew,
	CivilianScience,
	CivilianSecurity,
	CivilianAssimilated,
	HummerMinor,
	HummerMajor,
	HummerBigMinor,
	HummerBigMajor,
	HummerPossessed,
	CyborgMinor,
	CyborgMajor,
	CyborgFlameMinor,
	CyborgFlameMajor,
	EnforcerMinor,
	EnforcerMajor,
	HunterMinor,
	HunterMajor,
	TrooperMinor,
	TrooperMajor,
	MotherOfAllCyborgs,
	MotherOfAllHunters,
	SewageYeti,
	WaterYeti,
	LavaYeti,
	DefenderMinor,
	DefenderMajor,
	JuggernautMinor,
	JuggernautMajor,
	TinyFighter,
	TinyBob,
	TinyYeti,
	CivilianFusionCrew,
	CivilianFusionScience,
	CivilianFusionSecurity,
	CivilianFusionAssimilated
    }

    [Flags] public enum MapObjectFlags : ushort {
	None,
	Invisible = 0x01, // teleports in
	OnPlatform = 0x01, // for sounds only
	FromCeiling = 0x02,
	Blind = 0x04,
	Deaf = 0x08,
	Floats = 0x10, // or teleports out
	NetworkOnly = 0x20
    }

    public enum ActivationBias {
	Player,
	NearestHostile,
	Goal,
	Randomly
    }

    public class MapObject : ISerializableBE {
	public static readonly uint Tag = Wadfile.Chunk("OBJS");
	public const int Size = 16;

	private ObjectType type;
  public ObjectType Type 
  {
    get { return type; }
    set
    {
      if(value != type)
      {
      if(value == ObjectType.Scenery)
      {
          //Fix Bug #3504813
          SetFlag(MapObjectFlags.Invisible, false);
          SetFlag(MapObjectFlags.OnPlatform, false);
          SetFlag(MapObjectFlags.Blind, false);
          SetFlag(MapObjectFlags.Deaf, false);
          SetFlag(MapObjectFlags.Floats, false);
          SetFlag(MapObjectFlags.NetworkOnly, false);
      }
      else if(value == ObjectType.Item)
      {
          //Fix Bug #3504812
          SetFlag(MapObjectFlags.OnPlatform, false);
          SetFlag(MapObjectFlags.Blind, false);
          SetFlag(MapObjectFlags.Deaf, false);
          SetFlag(MapObjectFlags.Floats, false);
      }
      this.type = value;
      }
      
    }
  }
	public short Index;
	short facing;
	
	public short PolygonIndex = -1;
	public short X;
	public short Y;
	public short Z;
	MapObjectFlags flags;

	const int ActivationBiasShift = 12;

	public ActivationBias ActivationBias {
	    get {
		return (ActivationBias) ((ushort) flags >> ActivationBiasShift);
	    }

	    set {
		flags = (MapObjectFlags) (((ushort) flags & (1 << ActivationBiasShift) - 1) | ((ushort) value << ActivationBiasShift));
	    }
	}

	public void CopyFrom(MapObject other) {
	    Type = other.Type;
	    Index = other.Index;
	    facing = other.facing;
	    Z = other.Z;
	    flags = other.flags;
	}

	public double Facing {
	    get { return Angle.ToDouble(facing); }
	    set { facing = Angle.FromDouble(value); }
	}

	public int Volume {
	    get { return (int) Math.Round((double) facing * 100 / 255); }
	    set { facing = (short) Math.Round((double) value * 255 / 100); }
	}

	public bool UseLightForVolume {
	    get { return facing < 0; } 
	}

	public int Light {
	    get { return -facing; }
	    set { facing = (short) -value; }
	}

	void SetFlag(MapObjectFlags flag, bool value) {
	    if (value) {
		flags |= flag;
	    } else {
		flags &= ~flag;
	    }
	}

	bool GetFlag(MapObjectFlags flag) {
	    return (flags & flag) != 0;
	}

	public bool Invisible {
	    get {
		return GetFlag(MapObjectFlags.Invisible);
	    }

	    set {
		SetFlag(MapObjectFlags.Invisible, value);
	    }
	}

	public bool OnPlatform {
	    get {
		return GetFlag(MapObjectFlags.OnPlatform);
	    }

	    set {
		SetFlag(MapObjectFlags.OnPlatform, value);
	    }
	}

	public bool FromCeiling {
	    get {
		return GetFlag(MapObjectFlags.FromCeiling);
	    }

	    set {
		SetFlag(MapObjectFlags.FromCeiling, value);
	    }
	}

	public bool Blind {
	    get {
		return GetFlag(MapObjectFlags.Blind);
	    }

	    set {
		SetFlag(MapObjectFlags.Blind, value);
	    }
	}

	public bool Deaf {
	    get {
		return GetFlag(MapObjectFlags.Deaf);
	    }

	    set {
		SetFlag(MapObjectFlags.Deaf, value);
	    }
	}

	public bool Floats {
	    get {
		return GetFlag(MapObjectFlags.Floats);
	    }

	    set {
		SetFlag(MapObjectFlags.Floats, value);
	    }
	}

	public bool NetworkOnly {
	    get {
		return GetFlag(MapObjectFlags.NetworkOnly);
	    }

	    set {
		SetFlag(MapObjectFlags.NetworkOnly, value);
	    }
	}

	public void Load(BinaryReaderBE reader) {
	    Type = (ObjectType) reader.ReadInt16();
	    Index = reader.ReadInt16();
	    facing = reader.ReadInt16();
	    PolygonIndex = reader.ReadInt16();
	    X = reader.ReadInt16();
	    Y = reader.ReadInt16();
	    Z = reader.ReadInt16();
	    flags = (MapObjectFlags) reader.ReadUInt16();
	}

	public void Save(BinaryWriterBE writer) {
	    writer.Write((short) Type);
	    writer.Write(Index);
	    writer.Write(facing);
	    writer.Write(PolygonIndex);
	    writer.Write(X);
	    writer.Write(Y);
	    writer.Write(Z);
	    writer.Write((ushort) flags);
	}
    }

    [Flags] public enum PlacementFlags : ushort {
	None,
	RandomLocation
    }

    public class Placement : ISerializableBE {
	public const int Size = 12;
	public static readonly uint Tag = Wadfile.Chunk("plac");
	public const int Count = 64;

	PlacementFlags flags;
	public short InitialCount;
	public short MinimumCount;
	public short MaximumCount;
	public short RandomCount;
	ushort randomChance;

	public bool RandomLocation {
	    get {
		return (flags & PlacementFlags.RandomLocation) != 0;
	    }
	    set {
		if (value) {
		    flags |= PlacementFlags.RandomLocation;
		} else {
		    flags &= ~PlacementFlags.RandomLocation;
		}
	    }
	}

	public int RandomPercent {
	    get {
		return (int) Math.Round((double) randomChance * 100 / 65535);
	    }
	    set {
		randomChance = (ushort) Math.Round((double) value * 65535 / 100);
	    }
	}

	public void Load(BinaryReaderBE reader) {
	    flags = (PlacementFlags) reader.ReadUInt16();
	    InitialCount = reader.ReadInt16();
	    MinimumCount = reader.ReadInt16();
	    MaximumCount = reader.ReadInt16();
	    RandomCount = reader.ReadInt16();
	    randomChance = reader.ReadUInt16();
	}

	public void Save(BinaryWriterBE writer) {
	    writer.Write((ushort) flags);
	    writer.Write(InitialCount);
	    writer.Write(MinimumCount);
	    writer.Write(MaximumCount);
	    writer.Write(RandomCount);
	    writer.Write(randomChance);
	}
    }
}
 