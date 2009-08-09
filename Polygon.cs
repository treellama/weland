using System.IO;

namespace Weland {
    public class Polygon {
	public const uint Tag = 0x504f4c59; // POLY
	public const int Size = 128;
	public const int MaxVertexCount = 8;

	public short Type;
	public ushort Flags;
	public short Permutation;
	public ushort VertexCount;
	public short[] EndpointIndexes = new short[MaxVertexCount];
	public short[] LineIndexes = new short[MaxVertexCount];
	
	public short FloorTexture;
	public short CeilingTexture;
	public short FloorHeight;
	public short CeilingHeight;
	public short FloorLight;
	public short CeilingLight;

	public short FirstObjectIndex;

	public short FloorTransferMode;
	public short CeilingTransferMode;

	public short[] AdjacentPolygonIndexes = new short[MaxVertexCount];
	
	public short FirstNeighborIndex;
	public short NeighborCount;

	public short[] SideIndexes = new short[MaxVertexCount];
	public Point FloorOrigin;
	public Point CeilingOrigin;
	
	public short MediaIndex;
	public short MediaLight;
	
	public short AmbientSound;
	public short RandomSound;
	
	public void Load(BinaryReaderBE reader) {
	    Type = reader.ReadInt16();
	    Flags = reader.ReadUInt16();
	    Permutation = reader.ReadInt16();
	    VertexCount = reader.ReadUInt16();
	    for (int i = 0; i < EndpointIndexes.Length; ++i) {
		EndpointIndexes[i] = reader.ReadInt16();
	    }

	    for (int i = 0; i < LineIndexes.Length; ++i) {
		LineIndexes[i] = reader.ReadInt16();
	    }

	    FloorTexture = reader.ReadInt16();
	    CeilingTexture = reader.ReadInt16();
	    FloorHeight = reader.ReadInt16();
	    CeilingHeight = reader.ReadInt16();
	    FloorLight = reader.ReadInt16();
	    CeilingLight = reader.ReadInt16();
	    
	    reader.ReadInt32(); // area
	    FirstObjectIndex = reader.ReadInt16();
	    
	    reader.ReadInt16(); // first_exclusion_zone_index
	    reader.ReadInt16(); // line_exclusion_zone_count
	    reader.ReadInt16(); // point_exclusion_zone_count
	    FloorTransferMode = reader.ReadInt16();
	    CeilingTransferMode = reader.ReadInt16();
	    
	    for (int i = 0; i < AdjacentPolygonIndexes.Length; ++i) {
		AdjacentPolygonIndexes[i] = reader.ReadInt16();
	    }

	    reader.ReadInt16(); // first_neighbor_index
	    reader.ReadInt16(); // neighbor_count
	    
	    reader.ReadInt16(); // center.x
	    reader.ReadInt16(); // center.y

	    for (int i = 0; i < SideIndexes.Length; ++i) {
		SideIndexes[i] = reader.ReadInt16();
	    }

	    FloorOrigin.Load(reader);
	    CeilingOrigin.Load(reader);
	    
	    MediaIndex = reader.ReadInt16();
	    MediaLight = reader.ReadInt16();
	    
	    reader.ReadInt16(); // sound_source_indexes

	    AmbientSound = reader.ReadInt16();
	    RandomSound = reader.ReadInt16();

	    reader.BaseStream.Seek(2, SeekOrigin.Current);
	}

	public void DeleteLine(short index) {
	    for (int i = 0; i < VertexCount; ++i) {
		if (LineIndexes[i] > index) {
		    --LineIndexes[i];
		}
	    }
	}
    }
}
