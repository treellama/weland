using System.IO;

namespace Weland {
    enum BitmapFlags : ushort {
	ColumnOrder = 0x8000
    }

    class Bitmap {
	public short Width;
	public short Height;
	short bytesPerRow;

	BitmapFlags flags;
	public bool ColumnOrder {
	    get {
		return ((flags & BitmapFlags.ColumnOrder) == BitmapFlags.ColumnOrder);
	    }
	}
	public short BitDepth;

	byte[] data;
	public byte[] Data {
	    get {
		return data;
	    }
	}

	public void Load(BinaryReaderBE reader) {
	    Width = reader.ReadInt16();
	    Height = reader.ReadInt16();
	    bytesPerRow = reader.ReadInt16();
	    flags = (BitmapFlags) reader.ReadUInt16();
	    BitDepth = reader.ReadInt16();
	    
	    int scanlines = ColumnOrder ? Width : Height;
	    reader.BaseStream.Seek(20 + scanlines * 4, SeekOrigin.Current);

	    data = new byte[Width * Height];
	    if (bytesPerRow > -1) {
		// not compressed
		if (ColumnOrder) {
		    // rotate
		    short temp = Width;
		    Width = Height;
		    Height = temp;
		    for (int y = Height - 1; y >= 0; --y) {
			for (int x = 0; x < Width; ++x) {
			    data[x + y * Width] = reader.ReadByte();
			}
		    }
		} else {
		    reader.Read(data, 0, Width * Height);
		}
	    } else {
		for (int x = 0; x < Width; ++x) {
		    short start = reader.ReadInt16();
		    short end = reader.ReadInt16();
		    for (int y = start; y < end; ++y) {
			data[x + y * Width] = reader.ReadByte();
		    }
		}
	    }
	}
    }
}