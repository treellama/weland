using System.IO;
using System.Text;

public class BinaryReaderBE : BinaryReader
{
	public BinaryReaderBE(Stream stream, Encoding encoding) : base(stream, encoding) { }
	public BinaryReaderBE(Stream stream) : base(stream) { }

	public override short ReadInt16()
	{
		byte byte1 = ReadByte();
		byte byte2 = ReadByte();
		return (short) (byte1 << 8 | byte2);
	}

	public override ushort ReadUInt16()
	{
		byte byte1 = ReadByte();
		byte byte2 = ReadByte();
		return (ushort) (byte1 << 8 | byte2);
	}

	public override int ReadInt32()
	{
		byte byte1 = ReadByte();
		byte byte2 = ReadByte();
		byte byte3 = ReadByte();
		byte byte4 = ReadByte();

		return (int) ((byte1 << 24) | (byte2 << 16) | (byte3 << 8) | (byte4));
	}
	public override uint ReadUInt32()
	{
		byte byte1 = ReadByte();
		byte byte2 = ReadByte();
		byte byte3 = ReadByte();
		byte byte4 = ReadByte();
		return (uint) ((byte1 << 24) | (byte2 << 16) | (byte3 << 8) | (byte4));
	}

	public double ReadFixed() {
	    int i = ReadInt32();
	    return (double) i / ushort.MaxValue;
	}

	public string ReadMacString(int length)
	{
		byte[] bytes = ReadBytes(length);
		Encoding macRoman = Encoding.GetEncoding(10000);
		return macRoman.GetString(bytes).Split('\0')[0];
	}
}