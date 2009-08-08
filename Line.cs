using System.IO;

namespace Weland {
    public class Line {
	public const uint Tag = 0x4c494e53; // LINS
	public const int Size = 32;
	    
	public short[] EndpointIndexes = new short[2];
	public ushort Flags;
	    
	public short ClockwisePolygonSideIndex = -1;
	public short CounterclockwisePolygonSideIndex = -1;
	public short ClockwisePolygonOwner = -1;
	public short CounterclockwisePolygonOwner = -1;

	public void Load(BinaryReaderBE reader) {
	    EndpointIndexes[0] = reader.ReadInt16();
	    EndpointIndexes[1] = reader.ReadInt16();
	    Flags = reader.ReadUInt16();
	    reader.ReadInt16(); // length
	    reader.ReadInt16(); // highest_adjacent_floor
	    reader.ReadInt16(); // lowest_adjacent_floor
	    ClockwisePolygonSideIndex = reader.ReadInt16();
	    CounterclockwisePolygonSideIndex = reader.ReadInt16();
	    ClockwisePolygonOwner = reader.ReadInt16();
	    CounterclockwisePolygonOwner = reader.ReadInt16();
	    reader.BaseStream.Seek(12, SeekOrigin.Current);
	}
    }
}