namespace Weland {
    public struct Point : ISerializableBE {
	public const uint Tag = 0x504e5453; // PNTS
	public const int Size = 4;

	public short X;
	public short Y;

	public Point(short x, short y) {
	    X = x;
	    Y = y;
	}
	    
	public void Load(BinaryReaderBE reader) {
	    X = reader.ReadInt16();
	    Y = reader.ReadInt16();
	}
    }
	 
    public class Endpoint : ISerializableBE {
	public const uint Tag = 0x45504e54; // EPNT
	public const int Size = 16;

	public short Flags;
	public short HighestAdjacentFloorHeight;
	public short LowestAdjacentFloorHeight;
	public Point Vertex;
	public Point Transformed;
	public short SupportingPolygonIndex;

	public void Load(BinaryReaderBE reader) {
	    Flags = reader.ReadInt16(); 
	    HighestAdjacentFloorHeight = reader.ReadInt16();
	    LowestAdjacentFloorHeight  = reader.ReadInt16();
	    Vertex.Load(reader);
	    Transformed.Load(reader);
	    SupportingPolygonIndex = reader.ReadInt16();
	}
    }
}

