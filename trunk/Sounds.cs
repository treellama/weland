using System;
using System.IO;

namespace Weland {
    public class AmbientSound : ISerializableBE {
	public static readonly uint Tag = Wadfile.Chunk("ambi");
	public const int Size = 16;
	public ushort Flags;
	public short SoundIndex;
	public short Volume;
	
	public void Load(BinaryReaderBE reader) {
	    Flags = reader.ReadUInt16();
	    SoundIndex = reader.ReadInt16();
	    Volume = reader.ReadInt16();
	    reader.BaseStream.Seek(5 * 2, SeekOrigin.Current);
	}

	public void Save(BinaryWriterBE writer) {
	    writer.Write(Flags);
	    writer.Write(SoundIndex);
	    writer.Write(Volume);
	    writer.Write(new byte[5 * 2]);
	}
    }

    public enum RandomSoundFlags : ushort {
	NonDirectional = 0x01
    }

    public class RandomSound : ISerializableBE {
	public static readonly uint Tag = Wadfile.Chunk("bonk");
	public const int Size = 32;

	public RandomSoundFlags Flags;
	public bool NonDirectional {
	    get { return (Flags & RandomSoundFlags.NonDirectional) != 0; }
	    set {
		if (value) {
		    Flags |= RandomSoundFlags.NonDirectional;
		} else {
		    Flags &= ~RandomSoundFlags.NonDirectional;
		}
	    }
	}

	public short SoundIndex;
	public short Volume;
	public short DeltaVolume;
	public short Period;
	public short DeltaPeriod;
	short direction;
	public double Direction {
	    get { return Angle.ToDouble(direction); }
	    set { direction = Angle.FromDouble(value); }
	}
	short deltaDirection;
	public double DeltaDirection {
	    get { return Angle.ToDouble(deltaDirection); }
	    set { deltaDirection = Angle.FromDouble(value); }
	}
	public double Pitch;
	public double DeltaPitch;
	short phase = -1;

	public void Load(BinaryReaderBE reader) {
	    Flags = (RandomSoundFlags) reader.ReadUInt16();
	    SoundIndex = reader.ReadInt16();
	    Volume = reader.ReadInt16();
	    DeltaVolume = reader.ReadInt16();
	    Period = reader.ReadInt16();
	    DeltaPeriod = reader.ReadInt16();
	    direction = reader.ReadInt16();
	    deltaDirection = reader.ReadInt16();
	    Pitch = reader.ReadFixed();
	    DeltaPitch = reader.ReadFixed();
	    phase = reader.ReadInt16();
	    reader.BaseStream.Seek(2 * 3, SeekOrigin.Current);
	}

	public void Save(BinaryWriterBE writer) {
	    writer.Write((ushort) Flags);
	    writer.Write(SoundIndex);
	    writer.Write(Volume);
	    writer.Write(DeltaVolume);
	    writer.Write(Period);
	    writer.Write(DeltaPeriod);
	    writer.Write(direction);
	    writer.Write(deltaDirection);
	    writer.WriteFixed(Pitch);
	    writer.WriteFixed(DeltaPitch);
	    writer.Write(phase);
	    writer.Write(new byte[2 * 3]);
	}
    }
}