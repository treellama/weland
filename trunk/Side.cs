using System;
using System.IO;

namespace Weland {
    public enum SideType : short {
	Full,
	High,
	Low,
	Composite, // never
	Split
    }
    
    [Flags] public enum SideFlags : ushort {
	None = 0x0,
	ControlPanelStatus = 0x01,
	IsControlPanel = 0x02,
	IsRepairSwitch = 0x04,
	IsDestructiveSwitch = 0x08,
	IsLightedSwitch = 0x10,
	SwitchCanBeDestroyed = 0x20,
	SwitchCanOnlyBeHitByProjectiles = 0x40,
	Dirty = 0x4000
    }

    public enum ControlPanelType : short {
	Oxygen,
	Shield,
	DoubleShield,
	TripleShield,
	LightSwitch,
	PlatformSwitch,
	TagSwitch,
	PatternBuffer,
	Terminal
    }

    public class Side : ISerializableBE {
	public const int Size = 64;
	public static readonly uint Tag = Wadfile.Chunk("SIDS");

	public struct TextureDefinition {
	    public short X;
	    public short Y;
	    public ushort Texture;

	    public void Load(BinaryReaderBE reader) {
		X = reader.ReadInt16();
		Y = reader.ReadInt16();
		Texture = reader.ReadUInt16();
	    }

	    public void Save(BinaryWriterBE writer) { 
		writer.Write(X);
		writer.Write(Y);
		writer.Write(Texture);
	    }
	}

	public SideType Type;
	public SideFlags Flags;
	public TextureDefinition Primary;
	public TextureDefinition Secondary;
	public TextureDefinition Transparent;

	public ControlPanelType ControlPanelType;
	public short ControlPanelPermutation;
	public short PrimaryTransferMode;
	public short SecondaryTransferMode;
	public short TransparentTransferMode;
	public short PolygonIndex;
	public short LineIndex;
	public short PrimaryLightsourceIndex;
	public short SecondaryLightsourceIndex;
	public short TransparentLightsourceIndex;
	public int AmbientDelta;
	

	public void Load(BinaryReaderBE reader) {
	    Type = (SideType) reader.ReadInt16();
	    Flags = (SideFlags) reader.ReadUInt16();
	    Primary.Load(reader);
	    Secondary.Load(reader);
	    Transparent.Load(reader);
	    reader.BaseStream.Seek(16, SeekOrigin.Current); // exclusion zone
	    ControlPanelType = (ControlPanelType) reader.ReadInt16();
	    ControlPanelPermutation = reader.ReadInt16();
	    PrimaryTransferMode = reader.ReadInt16();
	    SecondaryTransferMode = reader.ReadInt16();
	    TransparentTransferMode = reader.ReadInt16();
	    PolygonIndex = reader.ReadInt16();
	    LineIndex = reader.ReadInt16();
	    PrimaryLightsourceIndex = reader.ReadInt16();
	    SecondaryLightsourceIndex = reader.ReadInt16();
	    TransparentLightsourceIndex = reader.ReadInt16();
	    AmbientDelta = reader.ReadInt32();
	    reader.ReadInt16(); // unused
	}
	
	public void Save(BinaryWriterBE writer) { 
	    writer.Write((short) Type);
	    writer.Write((ushort) Flags);
	    Primary.Save(writer);
	    Secondary.Save(writer);
	    Transparent.Save(writer);
	    writer.Write(new byte[16]);
	    writer.Write((short) ControlPanelType);
	    writer.Write((short) ControlPanelPermutation);
	    writer.Write(PrimaryTransferMode);
	    writer.Write(SecondaryTransferMode);
	    writer.Write(TransparentTransferMode);
	    writer.Write(PolygonIndex);
	    writer.Write(LineIndex);
	    writer.Write(PrimaryLightsourceIndex);
	    writer.Write(SecondaryLightsourceIndex);
	    writer.Write(TransparentLightsourceIndex);
	    writer.Write(AmbientDelta);
	    writer.Write(new byte[2]);
	}
    }
}