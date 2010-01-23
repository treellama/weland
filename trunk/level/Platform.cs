using System;
using System.IO;

namespace Weland {
    public enum PlatformType : short {
	SphtDoor,
	SphtSplitDoor,
	LockedSphtDoor,
	SphtPlatform,
	NoisySphtPlatform,
	HeavySphtDoor,
	PfhorDoor,
	HeavySphtPlatform,
	PfhorPlatform
    }

    public class Platform : ISerializableBE {
	public const int Size = 32;
	public static readonly uint StaticTag = Wadfile.Chunk("plat");

	public const int DynamicSize = 140;
	public static readonly uint DynamicTag = Wadfile.Chunk("PLAT");

	public PlatformType Type;
	public short Speed;
	public short Delay;
	public short MaximumHeight;
	public short MinimumHeight;
	PlatformFlags flags;
	public short PolygonIndex;
	public short Tag;

	public bool InitiallyActive {
	    get { return GetFlag(PlatformFlags.InitiallyActive); }
	    set { SetFlag(PlatformFlags.InitiallyActive, value); }
	}

	public bool InitiallyExtended {
	    get { return GetFlag(PlatformFlags.InitiallyExtended); }
	    set { SetFlag(PlatformFlags.InitiallyExtended, value); }
	}

	public bool DeactivatesAtEachLevel {
	    get { return GetFlag(PlatformFlags.DeactivatesAtEachLevel); }
	    set { SetFlag(PlatformFlags.DeactivatesAtEachLevel, value); }
	}

	public bool DeactivatesAtInitialLevel {
	    get { return GetFlag(PlatformFlags.DeactivatesAtInitialLevel); }
	    set { SetFlag(PlatformFlags.DeactivatesAtInitialLevel, value); }
	}

	public bool ActivatesAdjacentPlatformsWhenDeactivating {
	    get { return GetFlag(PlatformFlags.ActivatesAdjacentPlatformsWhenDeactivating); }
	    set { SetFlag(PlatformFlags.ActivatesAdjacentPlatformsWhenDeactivating, value); }
	}

	public bool ExtendsFloorToCeiling {
	    get { return GetFlag(PlatformFlags.ExtendsFloorToCeiling); }
	    set { SetFlag(PlatformFlags.ExtendsFloorToCeiling, value); }
	}

	public bool ComesFromFloor {
	    get { return GetFlag(PlatformFlags.ComesFromFloor); }
	    set { SetFlag(PlatformFlags.ComesFromFloor, value); }
	}

	public bool ComesFromCeiling {
	    get { return GetFlag(PlatformFlags.ComesFromCeiling); }
	    set { SetFlag(PlatformFlags.ComesFromCeiling, value); }
	}

	public bool CausesDamage {
	    get { return GetFlag(PlatformFlags.CausesDamage); }
	    set { SetFlag(PlatformFlags.CausesDamage, value); }
	}

	public bool DoesNotActivateParent {
	    get { return GetFlag(PlatformFlags.DoesNotActivateParent); }
	    set { SetFlag(PlatformFlags.DoesNotActivateParent, value); }
	}

	public bool ActivatesOnlyOnce {
	    get { return GetFlag(PlatformFlags.ActivatesOnlyOnce); }
	    set { SetFlag(PlatformFlags.ActivatesOnlyOnce, value); }
	}

	public bool ActivatesLight {
	    get { return GetFlag(PlatformFlags.ActivatesLight); }
	    set { SetFlag(PlatformFlags.ActivatesLight, value); }
	}

	public bool DeactivatesLight {
	    get { return GetFlag(PlatformFlags.DeactivatesLight); }
	    set { SetFlag(PlatformFlags.DeactivatesLight, value); }
	}

	public bool IsPlayerControllable {
	    get { return GetFlag(PlatformFlags.IsPlayerControllable); }
	    set { SetFlag(PlatformFlags.IsPlayerControllable, value); }
	}

	public bool IsMonsterControllable {
	    get { return GetFlag(PlatformFlags.IsMonsterControllable); }
	    set { SetFlag(PlatformFlags.IsMonsterControllable, value); }
	}

	public bool ReversesDirectionWhenObstructed {
	    get { return GetFlag(PlatformFlags.ReversesDirectionWhenObstructed); }
	    set { SetFlag(PlatformFlags.ReversesDirectionWhenObstructed, value); }
	}

	public bool CannotBeExternallyDeactivated {
	    get { return GetFlag(PlatformFlags.CannotBeExternallyDeactivated); }
	    set { SetFlag(PlatformFlags.CannotBeExternallyDeactivated, value); }
	}

	public bool UsesNativePolygonHeights {
	    get { return GetFlag(PlatformFlags.UsesNativePolygonHeights); }
	    set { SetFlag(PlatformFlags.UsesNativePolygonHeights, value); }
	}

	public bool DelaysBeforeActivation {
	    get { return GetFlag(PlatformFlags.DelaysBeforeActivation); }
	    set { SetFlag(PlatformFlags.DelaysBeforeActivation, value); }
	}

	public bool ActivatesAdjacentPlatformsWhenActivating {
	    get { return GetFlag(PlatformFlags.ActivatesAdjacentPlatformsWhenActivating); }
	    set { SetFlag(PlatformFlags.ActivatesAdjacentPlatformsWhenActivating, value); }
	}

	public bool DeactivatesAdjacentPlatformsWhenActivating {
	    get { return GetFlag(PlatformFlags.DeactivatesAdjacentPlatformsWhenActivating); }
	    set { SetFlag(PlatformFlags.DeactivatesAdjacentPlatformsWhenActivating, value); }
	}

	public bool DeactivatesAdjacentPlatformsWhenDeactivating {
	    get { return GetFlag(PlatformFlags.DeactivatesAdjacentPlatformsWhenDeactivating); }
	    set { SetFlag(PlatformFlags.DeactivatesAdjacentPlatformsWhenDeactivating, value); }
	}

	public bool ContractsSlower {
	    get { return GetFlag(PlatformFlags.ContractsSlower); }
	    set { SetFlag(PlatformFlags.ContractsSlower, value); }
	}

	public bool ActivatesAdjacantPlatformsAtEachLevel {
	    get { return GetFlag(PlatformFlags.ActivatesAdjacantPlatformsAtEachLevel); }
	    set { SetFlag(PlatformFlags.ActivatesAdjacantPlatformsAtEachLevel, value); }
	}

	public bool IsLocked {
	    get { return GetFlag(PlatformFlags.IsLocked); }
	    set { SetFlag(PlatformFlags.IsLocked, value); }
	}

	public bool IsSecret {
	    get { return GetFlag(PlatformFlags.IsSecret); }
	    set { SetFlag(PlatformFlags.IsSecret, value); }
	}

	public bool IsDoor {
	    get { return GetFlag(PlatformFlags.IsDoor); }
	    set { SetFlag(PlatformFlags.IsDoor, value); }
	}

	// does not copy polygon index
	public void CopyFrom(Platform other) {
	    Type = other.Type;
	    MaximumHeight = other.MaximumHeight;
	    MinimumHeight = other.MinimumHeight;
	    Speed = other.Speed;
	    Delay = other.Delay;
	    flags = other.flags;
	    Tag = other.Tag;
	}

	public void SetTypeWithDefaults(PlatformType type) {
	    const short baseSpeed = World.One / 60;
	    const short baseDelay = 30;
	    Type = type;
	    MaximumHeight = -1;
	    MinimumHeight = -1;

	    const PlatformFlags doorFlags = 
		PlatformFlags.DeactivatesAtInitialLevel |
		PlatformFlags.ExtendsFloorToCeiling |
		PlatformFlags.IsPlayerControllable | 
		PlatformFlags.IsMonsterControllable |
		PlatformFlags.ReversesDirectionWhenObstructed |
		PlatformFlags.InitiallyExtended | 
		PlatformFlags.ComesFromCeiling |
		PlatformFlags.IsDoor;

	    const PlatformFlags platformFlags =
		PlatformFlags.InitiallyActive |
		PlatformFlags.InitiallyExtended |
		PlatformFlags.ComesFromFloor |
		PlatformFlags.ReversesDirectionWhenObstructed;

	    switch (type) {
	    case PlatformType.SphtDoor:
		Speed = 2 * baseSpeed;
		Delay = 4 * baseDelay;
		flags = doorFlags;
		break;
	    case PlatformType.SphtSplitDoor:
	    case PlatformType.LockedSphtDoor:
		Speed = baseSpeed;
		Delay = 4 * baseDelay;
		flags = doorFlags | PlatformFlags.ComesFromFloor;
		break;
	    case PlatformType.SphtPlatform:
	    case PlatformType.NoisySphtPlatform:
		Speed = baseSpeed;
		Delay = 2 * baseDelay;
		flags = PlatformFlags.InitiallyActive |
		    PlatformFlags.InitiallyExtended | 
		    PlatformFlags.ComesFromFloor |
		    PlatformFlags.ReversesDirectionWhenObstructed;
		break;
	    case PlatformType.HeavySphtDoor:
		Speed = baseSpeed;
		Delay = 4 * baseDelay;
		flags = doorFlags;
		break;
	    case PlatformType.PfhorDoor:
		Speed = 2 * baseSpeed;
		Delay = 4 * baseDelay;
		flags = doorFlags;
		break;
	    case PlatformType.HeavySphtPlatform:
		Speed = baseSpeed;
		Delay = 4 * baseDelay;
		flags = platformFlags;
		break;
	    case PlatformType.PfhorPlatform:
		Speed = baseSpeed;
		Delay = 2 * baseDelay;
		flags = platformFlags;
		break;
	    }
	}

	public void Load(BinaryReaderBE reader) {
	    Type = (PlatformType) reader.ReadInt16();
	    Speed = reader.ReadInt16();
	    Delay = reader.ReadInt16();
	    MaximumHeight = reader.ReadInt16();
	    MinimumHeight = reader.ReadInt16();
	    flags = (PlatformFlags) reader.ReadUInt32();
	    PolygonIndex = reader.ReadInt16();
	    Tag = reader.ReadInt16();
	    reader.BaseStream.Seek(7 * 2, SeekOrigin.Current); // unused
	}

	public void LoadDynamic(BinaryReaderBE reader) {
	    Type = (PlatformType) reader.ReadInt16();
	    flags = (PlatformFlags) reader.ReadUInt32();
	    Speed = reader.ReadInt16();
	    Delay = reader.ReadInt16();
	    
	    short minimumFloorHeight = reader.ReadInt16();
	    short maximumFloorHeight = reader.ReadInt16();
	    short minimumCeilingHeight = reader.ReadInt16();
	    short maximumCeilingHeight = reader.ReadInt16();

	    PolygonIndex = reader.ReadInt16();

	    reader.BaseStream.Seek(8 + 64 + 2, SeekOrigin.Current); // stuff we don't care about
	    Tag = reader.ReadInt16();
	    reader.BaseStream.Seek(22 * 2, SeekOrigin.Current);
	    
	    if (ComesFromCeiling && ComesFromFloor) {
		MaximumHeight = maximumCeilingHeight;
		MinimumHeight = minimumFloorHeight;
	    } else if (ComesFromCeiling) {
		MaximumHeight = maximumCeilingHeight;
		MinimumHeight = minimumCeilingHeight;
	    } else if (ComesFromFloor) {
		MaximumHeight = maximumFloorHeight;
		MinimumHeight = minimumFloorHeight;
	    }
	}

	public void Save(BinaryWriterBE writer) {
	    writer.Write((short) Type);
	    writer.Write(Speed);
	    writer.Write(Delay);
	    writer.Write(MaximumHeight);
	    writer.Write(MinimumHeight);
	    writer.Write((uint) flags);
	    writer.Write(PolygonIndex);
	    writer.Write(Tag);
	    writer.Write(new byte[7 * 2]);
	}

	void SetFlag(PlatformFlags flag, bool value) {
	    if (value) 
		flags |= flag;
	    else
		flags &= ~flag;
	}

	bool GetFlag(PlatformFlags flag) {
	    return (flags & flag) == flag;
	}
    }

    [Flags] internal enum PlatformFlags {
	None,
	InitiallyActive = 1 << 0,
	InitiallyExtended = 1 << 1,
	DeactivatesAtEachLevel = 1 << 2,
	DeactivatesAtInitialLevel = 1 << 3,
	ActivatesAdjacentPlatformsWhenDeactivating = 1 << 4,
	ExtendsFloorToCeiling = 1 << 5,
	ComesFromFloor = 1 << 6,
	ComesFromCeiling = 1 << 7,
	CausesDamage = 1 << 8,
	DoesNotActivateParent = 1 << 9,
	ActivatesOnlyOnce = 1 << 10,
	ActivatesLight = 1 << 11,
	DeactivatesLight = 1 << 12,
	IsPlayerControllable = 1 << 13,
	IsMonsterControllable = 1 << 14,
	ReversesDirectionWhenObstructed = 1 << 15,
	CannotBeExternallyDeactivated = 1 << 16,
	UsesNativePolygonHeights = 1 << 17,
	DelaysBeforeActivation = 1 << 18,
	ActivatesAdjacentPlatformsWhenActivating = 1 << 19,
	DeactivatesAdjacentPlatformsWhenActivating = 1 << 20,
	DeactivatesAdjacentPlatformsWhenDeactivating = 1 << 21,
	ContractsSlower = 1 << 22,
	ActivatesAdjacantPlatformsAtEachLevel = 1 << 23,
	IsLocked = 1 << 24,
	IsSecret = 1 << 25,
	IsDoor = 1 << 26
    }
}