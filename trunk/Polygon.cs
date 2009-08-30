using System.IO;

namespace Weland {
    public class Polygon : ISerializableBE {
	public static readonly uint Tag = Wadfile.Chunk("POLY");
	public const int Size = 128;
	public const int MaxVertexCount = 8;

	public short Type;
	public ushort Flags;
	public short Permutation = -1;
	public ushort VertexCount;

	public short[] EndpointIndexes = new short[MaxVertexCount];
	public short[] LineIndexes = new short[MaxVertexCount];
	
	public short FloorTexture = -1;
	public short CeilingTexture = -1;
	public short FloorHeight = 0;
	public short CeilingHeight = 1024;
	public short FloorLight;
	public short CeilingLight;

	public short FirstObjectIndex = -1;

	public short FloorTransferMode;
	public short CeilingTransferMode;

	public short[] AdjacentPolygonIndexes = new short[MaxVertexCount];
	
	public short[] SideIndexes = new short[MaxVertexCount];
	public Point FloorOrigin;
	public Point CeilingOrigin;
	
	public short MediaIndex;
	public short MediaLight;
	
	public short AmbientSound = -1;
	public short RandomSound = -1;

	// not stored
	public bool Concave;

	public Polygon() {
	    for (int i = 0; i < MaxVertexCount; ++i) {
		EndpointIndexes[i] = -1;
		LineIndexes[i] = -1;
		AdjacentPolygonIndexes[i] = -1;
		SideIndexes[i] = -1;
	    }
	}
	
	public void Load(BinaryReaderBE reader) {
	    Type = reader.ReadInt16();
	    Flags = reader.ReadUInt16();
	    Permutation = reader.ReadInt16();
	    VertexCount = reader.ReadUInt16();
	    for (int i = 0; i < MaxVertexCount; ++i) {
		EndpointIndexes[i] = reader.ReadInt16();
	    }

	    for (int i = 0; i < MaxVertexCount; ++i) {
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
	    
	    for (int i = 0; i < MaxVertexCount; ++i) {
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

	public void Save(BinaryWriterBE writer) {
	    writer.Write(Type);
	    writer.Write(Flags);
	    writer.Write(Permutation);
	    writer.Write(VertexCount);
	    for (int i = 0; i < MaxVertexCount; ++i) {
		writer.Write(EndpointIndexes[i]);
	    }
	    
	    for (int i = 0; i < MaxVertexCount; ++i) {
		writer.Write(LineIndexes[i]);
	    }

	    writer.Write(FloorTexture);
	    writer.Write(CeilingTexture);
	    writer.Write(FloorHeight);
	    writer.Write(CeilingHeight);
	    writer.Write(FloorLight);
	    writer.Write(CeilingLight);
	    
	    writer.Write((int) 0); // area
	    writer.Write(FirstObjectIndex);
	    
	    writer.Write((short) -1); // first exclusion zone index
	    writer.Write((short) -1); // line exclusion zone count
	    writer.Write((short) -1); // point exclusion zone count
	    writer.Write(FloorTransferMode);
	    writer.Write(CeilingTransferMode);

	    for (int i = 0; i < MaxVertexCount; ++i) {
		writer.Write(AdjacentPolygonIndexes[i]);
	    }

	    writer.Write((short) -1); // first neighbor index
	    writer.Write((short) -1); // neighbor count
	    writer.Write((short) 0); // center.x
	    writer.Write((short) 0); // center.y

	    for (int i = 0; i < MaxVertexCount; ++i) {
		writer.Write(SideIndexes[i]);
	    }

	    FloorOrigin.Save(writer);
	    CeilingOrigin.Save(writer);
	    
	    writer.Write(MediaIndex);
	    writer.Write(MediaLight);
	    
	    writer.Write((short) -1); // sound source indexes
	    
	    writer.Write(AmbientSound);
	    writer.Write(RandomSound);
	    
	    writer.Write(new byte[2]);
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
