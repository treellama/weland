using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

namespace Weland.Models {
    class CollectionHeader {
	public short Status;
	public ushort Flags;
	public int Offset;
	public int Length;
	public int Offset16;
	public int Length16;
	
	public void Load(BinaryReaderBE reader) {
	    Status = reader.ReadInt16();
	    Flags = reader.ReadUInt16();
	    Offset = reader.ReadInt32();
	    Length = reader.ReadInt32();
	    Offset16 = reader.ReadInt32();
	    Length16 = reader.ReadInt32();
	    reader.BaseStream.Seek(12, SeekOrigin.Current);
	}
    }

    public enum CollectionType : short {
	Unused,
	Wall,
	Object,
	Interface,
	Scenery
    }

    public class Collection {
#pragma warning disable 0414
	public short Version;
	public CollectionType Type;
	public ushort Flags;
	
	short colorCount;
	short colorTableCount;
	int colorTableOffset;

	short highLevelShapeCount;
	int highLevelShapeOffsetTableOffset;

	short lowLevelShapeCount;
	int lowLevelShapeOffsetTableOffset;

	public short BitmapCount {
	    get {
		return bitmapCount;
	    }
	}

	public int ColorTableCount {
	    get {
		return colorTables.Count;
	    }
	}

	short bitmapCount;
	int bitmapOffsetTableOffset;

	short pixelsToWorld;
	int size;

#pragma warning restore 0414

	struct ColorValue {
	    public byte Flags;
	    public byte Value;
	    
	    public ushort Red;
	    public ushort Green;
	    public ushort Blue;

	    public void Load(BinaryReaderBE reader) {
		Flags = reader.ReadByte();
		Value = reader.ReadByte();
		
		Red = reader.ReadUInt16();
		Green = reader.ReadUInt16();
		Blue = reader.ReadUInt16();
	    }
	}
	    
    	/// <summary>
        /// This is "Frame" data in Anvil terms.
        /// </summary>
        struct LowLevelShapeDefinition
        {
            public ushort Flags;
            public double MinimumLightIntensity;
            public short BitmapIndex;
            public short OriginX, OriginY;
            public short KeyX, KeyY;
            public short WorldLeft, WorldRight, WorldTop, WorldBottom;
            public short WorldX0, WorldY0;
            private short[] Unused;

            public void Load(BinaryReaderBE reader)
            {
                Flags = reader.ReadUInt16();
                MinimumLightIntensity = reader.ReadFixed();
                BitmapIndex = reader.ReadInt16();
                OriginX = reader.ReadInt16();
                OriginY = reader.ReadInt16();
                KeyX = reader.ReadInt16();
                KeyY = reader.ReadInt16();
                WorldLeft = reader.ReadInt16();
                WorldRight = reader.ReadInt16();
                WorldTop = reader.ReadInt16();
                WorldBottom = reader.ReadInt16();
                WorldX0 = reader.ReadInt16();
                WorldY0 = reader.ReadInt16();

                Unused = new short[4];
                for (int i = 0; i < Unused.Length; i++)
                {
                    Unused[i] = reader.ReadInt16();
                }
            }
        }

	List<ColorValue[]> colorTables = new List<ColorValue[]>();
	List<Bitmap> bitmaps = new List<Bitmap>();
    	List<LowLevelShapeDefinition> lowLevelShapes = new List<LowLevelShapeDefinition>();

	public void Load(BinaryReaderBE reader) {
	    long origin = reader.BaseStream.Position;

	    Version = reader.ReadInt16();
	    Type = (CollectionType) reader.ReadInt16();
	    Flags = reader.ReadUInt16();
	    colorCount = reader.ReadInt16();
	    colorTableCount = reader.ReadInt16();
	    colorTableOffset = reader.ReadInt32();
	    highLevelShapeCount = reader.ReadInt16();
	    highLevelShapeOffsetTableOffset = reader.ReadInt32();
	    lowLevelShapeCount = reader.ReadInt16();
	    lowLevelShapeOffsetTableOffset = reader.ReadInt32();
	    bitmapCount = reader.ReadInt16();
	    bitmapOffsetTableOffset = reader.ReadInt32();
	    pixelsToWorld = reader.ReadInt16();
	    size = reader.ReadInt32();
	    reader.BaseStream.Seek(253 * 2, SeekOrigin.Current);

	    colorTables.Clear();
	    reader.BaseStream.Seek(origin + colorTableOffset, SeekOrigin.Begin);
	    for (int i = 0; i < colorTableCount; ++i) {
		ColorValue[] table = new ColorValue[colorCount];
		for (int j = 0; j < colorCount; ++j) {
		    table[j].Load(reader);
		}
		colorTables.Add(table);
	    }

	    bitmaps.Clear();
            reader.BaseStream.Seek(origin + bitmapOffsetTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < bitmapCount; ++i)
            {
                int bitmapPosition = reader.ReadInt32();
                long nextPositionInOffsetTable = reader.BaseStream.Position;

                reader.BaseStream.Seek(origin + bitmapPosition, SeekOrigin.Begin);

                Bitmap bitmap = new Bitmap();
                bitmap.Load(reader);
                bitmaps.Add(bitmap);

                reader.BaseStream.Seek(nextPositionInOffsetTable, SeekOrigin.Begin);
            }

            lowLevelShapes.Clear();
            reader.BaseStream.Seek(origin + lowLevelShapeOffsetTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < lowLevelShapeCount; i++)
            {
                long shapeDefinitionPosition = origin + reader.ReadInt32();
                long nextPositionInOffsetTable = reader.BaseStream.Position;

                reader.BaseStream.Seek(shapeDefinitionPosition, SeekOrigin.Begin);

                LowLevelShapeDefinition lowLevelShapeDefinition = new LowLevelShapeDefinition();
                lowLevelShapeDefinition.Load(reader);
                lowLevelShapes.Add(lowLevelShapeDefinition);

                reader.BaseStream.Seek(nextPositionInOffsetTable, SeekOrigin.Begin);
            }
	}

	public System.Drawing.Bitmap GetShape(byte ColorTableIndex, byte BitmapIndex) {
	    Bitmap bitmap = Type == CollectionType.Wall && BitmapIndex < lowLevelShapeCount - 1 ? bitmaps[lowLevelShapes[BitmapIndex].BitmapIndex] : bitmaps[BitmapIndex];
	    ColorValue[] colorTable = colorTables[ColorTableIndex];
	    Color[] colors = new Color[colorTable.Length];
	    for (int i = 0; i < colorTable.Length; ++i) {
		ColorValue color = colorTable[i];
		colors[i] = Color.FromArgb(color.Red >> 8,
					   color.Green >> 8,
					   color.Blue >> 8);
	    }
	    
	    System.Drawing.Bitmap result = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height);
	    for (int x = 0; x < bitmap.Width; ++x) {
		for (int y = 0; y < bitmap.Height; ++y) {
		    result.SetPixel(x, y, colors[bitmap.Data[x + y * bitmap.Width]]);
		}
	    }
	    
	    return result;
	}
    }
}
