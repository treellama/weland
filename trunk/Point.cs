namespace Weland {
    public struct Point : ISerializableBE {
	public static readonly uint Tag = Wadfile.Chunk("PNTS");
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

	public void Save(BinaryWriterBE writer) {
	    writer.Write(X);
	    writer.Write(Y);
	}
    }
	 
    public class Endpoint : ISerializableBE {
	public static readonly uint Tag = Wadfile.Chunk("EPNT");
	public const int Size = 16;

	public short Flags;
	public short HighestAdjacentFloor;
	public short LowestAdjacentFloor;
	public Point Vertex;
	public Point Transformed;
	public short SupportingPolygonIndex;

	public void Load(BinaryReaderBE reader) {
	    Flags = reader.ReadInt16(); 
	    HighestAdjacentFloor = reader.ReadInt16();
	    LowestAdjacentFloor = reader.ReadInt16();
	    Vertex.Load(reader);
	    Transformed.Load(reader);
	    SupportingPolygonIndex = reader.ReadInt16();
	}

	// weland should never write these
	public void Save(BinaryWriterBE writer) {
	    writer.Write(Flags);
	    writer.Write(HighestAdjacentFloor);
	    writer.Write(LowestAdjacentFloor);
	    Vertex.Save(writer);
	    Transformed.Save(writer);
	    writer.Write(SupportingPolygonIndex);
	}
    }
}

