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
	public short Flags;

	public void Load(BinaryReaderBE reader) {
	    Type = (ObjectType) reader.ReadInt16();
	    Index = reader.ReadInt16();
	    Facing = reader.ReadInt16();
	    PolygonIndex = reader.ReadInt16();
	    X = reader.ReadInt16();
	    Y = reader.ReadInt16();
	    Z = reader.ReadInt16();
	    Flags = reader.ReadInt16();
	}

	public void Save(BinaryWriterBE writer) {
	    writer.Write((short) Type);
	    writer.Write(Index);
	    writer.Write(Facing);
	    writer.Write(PolygonIndex);
	    writer.Write(X);
	    writer.Write(Y);
	    writer.Write(Z);
	    writer.Write(Flags);
	}
    }
}
 