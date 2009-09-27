namespace Weland {
    public class Annotation : ISerializableBE {
	public static readonly uint Tag = Wadfile.Chunk("NOTE");
	public const int Size = 72;
	const int textLength = 64;

	public short Type;
	public short X;
	public short Y;
	public short PolygonIndex;
	public string Text;

	public void Load(BinaryReaderBE reader) {
	    Type = reader.ReadInt16();
	    X = reader.ReadInt16();
	    Y = reader.ReadInt16();
	    PolygonIndex = reader.ReadInt16();
	    Text = reader.ReadMacString(textLength);
	}

	public void Save(BinaryWriterBE writer) {
	    writer.Write(Type);
	    writer.Write(X);
	    writer.Write(Y);
	    writer.Write(PolygonIndex);
	    writer.WriteMacString(Text, textLength);
	}
    }
}