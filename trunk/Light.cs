using System;
using System.IO;

namespace Weland {
    public enum LightType : short {
	Normal,
	Strobe,
	Media
    }

    [Flags] public enum LightFlags : ushort {
	None = 0x00,
	InitiallyActive = 0x01,
	SlavedIntensities = 0x02,
	Stateless = 0x04
    }

    public enum LightingFunction {
	Constant,
	Linear,
	Smooth,
	Flicker
    }

    public class Light : ISerializableBE {
	public static readonly uint Tag = Wadfile.Chunk("LITE");
	public struct Function {
	    public LightingFunction LightingFunction;
	    public short Period;
	    public short DeltaPeriod;
	    public double Intensity;
	    public double DeltaIntensity;

	    public Function(LightingFunction f, short period, short deltaPeriod, double intensity, double deltaIntensity) {
		LightingFunction = f;
		Period = period;
		DeltaPeriod = deltaPeriod;
		Intensity = intensity;
		DeltaIntensity = deltaIntensity;
	    }

	    public void Load(BinaryReaderBE reader) {
		LightingFunction = (LightingFunction) reader.ReadInt16();
		Period = reader.ReadInt16();
		DeltaPeriod = reader.ReadInt16();
		Intensity = reader.ReadFixed();
		DeltaIntensity = reader.ReadFixed();
	    }

	    public void Save(BinaryWriterBE writer) {
		writer.Write((short) LightingFunction);
		writer.Write(Period);
		writer.Write(DeltaPeriod);
		writer.WriteFixed(Intensity);
		writer.WriteFixed(DeltaIntensity);
	    }
	}
	
	public Light() { 
	    SetTypeWithDefaults(LightType.Normal);
	}

	public Light(double intensity) : this() {
	    PrimaryActive.Intensity = intensity;
	    SecondaryActive.Intensity = intensity;
	    BecomingActive.Intensity = intensity;
	}

	public bool InitiallyActive {
	    get {
		return (Flags & LightFlags.InitiallyActive) != 0;
	    }
	    
	    set {
		if (value) {
		    Flags |= LightFlags.InitiallyActive;
		} else {
		    Flags &= ~LightFlags.InitiallyActive;
		}
	    }
	}

	public bool Stateless {
	    get {
		return (Flags & LightFlags.Stateless) != 0;
	    }
	    
	    set {
		if (value) {
		    Flags |= LightFlags.Stateless;
		} else {
		    Flags &= ~LightFlags.Stateless;
		}
	    }
	}

	public void SetTypeWithDefaults(LightType type) {
	    Type = type;
	    Flags = LightFlags.SlavedIntensities | LightFlags.InitiallyActive;
	    Phase = 0;
	    if (type == LightType.Normal) {
		PrimaryActive = SecondaryActive = BecomingActive = PrimaryInactive = SecondaryInactive = BecomingInactive = new Function(LightingFunction.Constant, 30, 0, 1, 0);
	    } else if (type == LightType.Strobe) {
		PrimaryActive = new Function(LightingFunction.Constant, 15, 0, 1, 0);
		SecondaryActive = new Function(LightingFunction.Constant, 15, 0, 0.5, 0);
		BecomingActive = new Function(LightingFunction.Smooth, 30, 0, 0.5, 0);
		PrimaryInactive = SecondaryInactive = BecomingInactive = new Function(LightingFunction.Constant, 30, 0, 0, 0);
	    } else if (type == LightType.Media) {
		PrimaryActive = new Function(LightingFunction.Smooth, 300, 0, 1, 0);
		SecondaryActive = new Function(LightingFunction.Smooth, 300, 0, 0, 0);
		BecomingActive = new Function(LightingFunction.Smooth, 30, 0, 1, 0);
		PrimaryInactive = SecondaryInactive = new Function(LightingFunction.Constant, 30, 0, 0, 0);
		BecomingInactive = new Function(LightingFunction.Smooth, 30, 0, 0, 0);
	    }
	}

	public LightType Type;
	public LightFlags Flags = LightFlags.SlavedIntensities;
	public short Phase;
	public Function PrimaryActive;
	public Function SecondaryActive;
	public Function BecomingActive;
	public Function PrimaryInactive;
	public Function SecondaryInactive;
	public Function BecomingInactive;
	public short TagIndex;

	public void Load(BinaryReaderBE reader) {
	    Type = (LightType) reader.ReadInt16();
	    Flags = (LightFlags) reader.ReadUInt16();
	    Phase = reader.ReadInt16();
	    PrimaryActive.Load(reader);
	    SecondaryActive.Load(reader);
	    BecomingActive.Load(reader);
	    PrimaryInactive.Load(reader);
	    SecondaryInactive.Load(reader);
	    BecomingInactive.Load(reader);
	    TagIndex = reader.ReadInt16();
	    reader.BaseStream.Seek(4 * 2, SeekOrigin.Current); // unused
	}

	public void Save(BinaryWriterBE writer) { 
	    writer.Write((short) Type);
	    writer.Write((ushort) Flags);
	    writer.Write(Phase);
	    PrimaryActive.Save(writer);
	    SecondaryActive.Save(writer);
	    BecomingActive.Save(writer);
	    PrimaryInactive.Save(writer);
	    SecondaryInactive.Save(writer);
	    BecomingInactive.Save(writer);
	    writer.Write(TagIndex);
	    writer.Write(new byte[4*2]);
	}
    }
}