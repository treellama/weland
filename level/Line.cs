using System;
using System.IO;

namespace Weland {
    [Flags] public enum LineFlags : ushort {
	HasTransparentSide = 0x200,
	VariableElevation = 0x400,
	Elevation = 0x800,
	Landscape = 0x1000,
	Transparent = 0x2000,
	Solid = 0x4000
    }

    public class Line : ISerializableBE {
	public static readonly uint Tag = Wadfile.Chunk("LINS");
	public const int Size = 32;
	    
	public short[] EndpointIndexes = new short[2];
	public LineFlags Flags = LineFlags.Solid;

	public short Length;
	public short HighestAdjacentFloor;
	public short LowestAdjacentCeiling;
	    
	public short ClockwisePolygonSideIndex = -1;
	public short CounterclockwisePolygonSideIndex = -1;
	public short ClockwisePolygonOwner = -1;
	public short CounterclockwisePolygonOwner = -1;

	public bool Transparent {
	    get { return (Flags & LineFlags.Transparent) != 0; }
	    set { if (value) Flags |= LineFlags.Transparent; else Flags &= ~LineFlags.Transparent; }
	}

	public bool Solid {
	    get { return (Flags & LineFlags.Solid) != 0; }
	    set { if (value) Flags |= LineFlags.Solid; else Flags &= ~LineFlags.Solid; }
	}

	public bool VariableElevation {
	    get { return (Flags & LineFlags.VariableElevation) != 0; }
	    set { if (value) Flags |= LineFlags.VariableElevation; else Flags &= ~LineFlags.VariableElevation; }
	}

	public void Load(BinaryReaderBE reader) {
	    EndpointIndexes[0] = reader.ReadInt16();
	    EndpointIndexes[1] = reader.ReadInt16();
	    Flags = (LineFlags) reader.ReadUInt16();
	    Length = reader.ReadInt16();
	    HighestAdjacentFloor = reader.ReadInt16();
	    LowestAdjacentCeiling = reader.ReadInt16();
	    ClockwisePolygonSideIndex = reader.ReadInt16();
	    CounterclockwisePolygonSideIndex = reader.ReadInt16();
	    ClockwisePolygonOwner = reader.ReadInt16();
	    CounterclockwisePolygonOwner = reader.ReadInt16();
	    reader.BaseStream.Seek(12, SeekOrigin.Current);
	}

	public void Save(BinaryWriterBE writer) {
	    writer.Write(EndpointIndexes[0]);
	    writer.Write(EndpointIndexes[1]);
	    writer.Write((ushort) Flags);
	    writer.Write(Length);
	    writer.Write(HighestAdjacentFloor);
	    writer.Write(LowestAdjacentCeiling);
	    writer.Write(ClockwisePolygonSideIndex);
	    writer.Write(CounterclockwisePolygonSideIndex);
	    writer.Write(ClockwisePolygonOwner);
	    writer.Write(CounterclockwisePolygonOwner);
	    writer.Write(new byte[12]);
	}
    }
}