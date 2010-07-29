using System;
using System.IO;

namespace Weland {
    public enum MediaType : short {
	Water,
	Lava,
	Goo,
	Sewage,
	Jjaro
    }

    public enum MediaFlags : ushort {
	SoundObstructedByFloor = 0x1
    }

    public class Media : ISerializableBE {
	public const int size = 32;
	public static readonly uint Tag = Wadfile.Chunk("medi");
	
	public MediaType Type;
	public MediaFlags Flags;
	public short LightIndex;
	short direction;
	public double Direction {
	    get { return Angle.ToDouble(direction); }
	    set { direction = Angle.FromDouble(value); }
	}

	public short CurrentMagnitude;
	public short Low;
	public short High;
	public double MinimumLightIntensity;
	
	public bool SoundObstructedByFloor {
	    get { return (Flags & MediaFlags.SoundObstructedByFloor) != 0; }
	    set {
		if (value) {
		    Flags |= MediaFlags.SoundObstructedByFloor;
		} else {
		    Flags &= ~MediaFlags.SoundObstructedByFloor;
		}
	    }
	}

	public void Load(BinaryReaderBE reader) {
	    Type = (MediaType) reader.ReadInt16();
	    Flags = (MediaFlags) reader.ReadUInt16();
	    LightIndex = reader.ReadInt16();
	    direction = reader.ReadInt16();
	    CurrentMagnitude = reader.ReadInt16();
	    Low = reader.ReadInt16();
	    High = reader.ReadInt16();
	    reader.ReadInt16(); // X
	    reader.ReadInt16(); // Y
	    reader.ReadInt16(); // Height
	    MinimumLightIntensity = reader.ReadFixed();
	    reader.ReadInt16(); // Texture
	    reader.ReadInt16(); // TransferMode
	    reader.BaseStream.Seek(2 * 2, SeekOrigin.Current);
	}

	public void Save(BinaryWriterBE writer) {
	    writer.Write((short) Type);
	    writer.Write((ushort) Flags);
	    writer.Write(LightIndex);
	    writer.Write(direction);
	    writer.Write(CurrentMagnitude);
	    writer.Write(Low);
	    writer.Write(High);
	    writer.Write(new byte[6]); // X, Y, Height
	    writer.WriteFixed(MinimumLightIntensity);
	    writer.Write(new byte[4]); // texture, transfer mode
	    writer.Write(new byte[4]); // unused
	}
    }
}