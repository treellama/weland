using System;
using System.IO;

namespace Weland{
    [Flags] 
    public enum EnvironmentFlags : short {
	Normal = 0x0000,
	Vacuum = 0x0001,
	Magnetic = 0x0002,
	Rebellion = 0x0004,
	LowGravity = 0x0008,
	Network = 0x2000,
	SinglePlayer = 0x4000
    }

    [Flags]
    public enum MissionFlags : short {
	None = 0x0000,
	Extermination = 0x0001,
	Exploration = 0x0002,
	Retrieval = 0x0004,
	Repair = 0x0008,
	Rescue = 0x0010
    }

    [Flags]
    public enum EntryPointFlags : int {
	SinglePlayer = 0x01,
	MultiplayerCooperative = 0x02,
	MultiplayerCarnage = 0x04,
	KillTheManWithTheBall = 0x08,
	KingOfTheHill = 0x10,
	Defense = 0x20,
	Rugby = 0x40,
	CaptureTheFlag = 0x80
    }
	
    public class MapInfo : ISerializableBE {
	// Marathon 2 calls this static_data
	public static readonly uint Tag = Wadfile.Chunk("Minf");
	public const int Size = 88;
	
	public short Environment;
	public short PhysicsModel;
	public short Landscape;
	public MissionFlags MissionFlags;
	public EnvironmentFlags EnvironmentFlags;

	public string Name = "Untitled Level";
	public const int LevelNameLength = 66;
	public EntryPointFlags EntryPointFlags = EntryPointFlags.SinglePlayer;

	public void Load(BinaryReaderBE reader) {
	    Environment = reader.ReadInt16();
	    PhysicsModel = reader.ReadInt16();
	    Landscape = reader.ReadInt16();
	    MissionFlags = (MissionFlags) reader.ReadInt16();
	    EnvironmentFlags = (EnvironmentFlags) reader.ReadInt16();

	    reader.BaseStream.Seek(8, SeekOrigin.Current); // unused
	    Name = reader.ReadMacString(LevelNameLength);
	    EntryPointFlags = (EntryPointFlags) reader.ReadInt32();
	}

	public void Save(BinaryWriterBE writer) {
	    writer.Write(Environment);
	    writer.Write(PhysicsModel);
	    writer.Write(Landscape);
	    writer.Write((short) MissionFlags);
	    writer.Write((short) EnvironmentFlags);
	    writer.Write(new byte[8]);
	    writer.WriteMacString(Name, LevelNameLength);
	    writer.Write((int) EntryPointFlags);
	}
    }
}

