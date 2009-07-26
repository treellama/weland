namespace Weland {
    public class MapObject {
	public const uint Tag = 0x4f424a53; // OBJS
	public const int Size = 16;

	public enum Types {
	    Monster,
	    Scenery,
	    Item,
	    Player,
	    Goal,
	    Sound
	};

	public Types Type;
	public short Index;
	public short Facing;
	public short PolygonIndex;
	public short X;
	public short Y;
	public short Z;
	public short Flags;

	public void Load(BinaryReaderBE reader) {
	    Type = (Types) reader.ReadInt16();
	    Index = reader.ReadInt16();
	    Facing = reader.ReadInt16();
	    PolygonIndex = reader.ReadInt16();
	    X = reader.ReadInt16();
	    Y = reader.ReadInt16();
	    Z = reader.ReadInt16();
	    Flags = reader.ReadInt16();
	}
    }
}
 