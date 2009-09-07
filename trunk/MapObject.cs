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
	Ball,
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

    [Flags] public enum MapObjectFlags : short {
	None,
	Invisible = 0x01, // teleports in
	OnPlatform = 0x01, // for sounds only
	FromCeiling = 0x02,
	Blind = 0x04,
	Deaf = 0x08,
	Floats = 0x10,
	NetworkOnly = 0x20
    }

    public class MapObject : ISerializableBE {
	public static readonly uint Tag = Wadfile.Chunk("OBJS");
	public const int Size = 16;

	public ObjectType Type;
	public short Index;
	public short Facing;
	public short PolygonIndex = 0;
	public short X;
	public short Y;
	public short Z;
	MapObjectFlags flags;

	void SetFlag(MapObjectFlags flag, bool value) {
	    if (value) {
		flags |= flag;
	    } else {
		flags &= flag;
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
	    Facing = reader.ReadInt16();
	    PolygonIndex = reader.ReadInt16();
	    X = reader.ReadInt16();
	    Y = reader.ReadInt16();
	    Z = reader.ReadInt16();
	    flags = (MapObjectFlags) reader.ReadInt16();
	}

	public void Save(BinaryWriterBE writer) {
	    writer.Write((short) Type);
	    writer.Write(Index);
	    writer.Write(Facing);
	    writer.Write(PolygonIndex);
	    writer.Write(X);
	    writer.Write(Y);
	    writer.Write(Z);
	    writer.Write((short) flags);
	}
    }
}
 