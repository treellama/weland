using System;
using System.IO;
using System.Text;

public class BinaryWriterBE : BinaryWriter
{
    public BinaryWriterBE(Stream stream, Encoding encoding) : base(stream, encoding) { }
    public BinaryWriterBE(Stream stream) : base(stream) { }

    public override void Write(short value) {
	Write((byte) (value >> 8));
	Write((byte) value);
    }

    public override void Write(ushort value) {
	Write((byte) (value >> 8));
	Write((byte) value);
    }

    public override void Write(int value) {
	Write((byte) (value >> 24));
	Write((byte) (value >> 16));
	Write((byte) (value >> 8));
	Write((byte) (value));
    }

    public override void Write(uint value) {
	Write((byte) (value >> 24));
	Write((byte) (value >> 16));
	Write((byte) (value >> 8));
	Write((byte) (value));
    }
    
    public void WriteFixed(double value) {
	int i = (int) Math.Floor(value * ushort.MaxValue);
	Write(i);
    }

    public void WriteMacString(string s, int length) {
	Encoding macRoman = Encoding.GetEncoding(10000);
	byte[] bytes = macRoman.GetBytes(s);
	if (bytes.Length > length - 1) {
	    Write(bytes, 0, length - 1);
	    Write((byte) 0);
	} else {
	    Write(bytes);
	    Write(new byte[length - bytes.Length]);
	}
    }
}
