namespace Weland {
    public struct ShapeDescriptor {
	const int ShapeBits = 8;
	const int CollectionBits = 5;
	const int CLUTBits = 3;
	public static readonly int MaximumCollections = 1 << CollectionBits;
	public static readonly int MaximumShapes = 1 << ShapeBits;
	public static readonly int MaximumCLUTs = 1 << CLUTBits;

	public static readonly ShapeDescriptor Empty = new ShapeDescriptor(ushort.MaxValue);

	public ShapeDescriptor(ushort value) {
	    descriptor = value;
	}

	public static explicit operator ushort(ShapeDescriptor sd) {
	    return sd.descriptor;
	}

	public static explicit operator ShapeDescriptor(ushort value) {
	    return new ShapeDescriptor(value);
	}

	public bool IsEmpty() {
	    return descriptor == ushort.MaxValue;
	}

	public byte CLUT {
	    get {
		return (byte) ((descriptor >> (CollectionBits + ShapeBits)) & (MaximumCLUTs - 1));
	    }
	    set {
		if (IsEmpty()) descriptor = 0;
		descriptor = (ushort) (value << (CollectionBits + ShapeBits) | Collection << ShapeBits | Bitmap);
	    }
	}

	public byte Collection {
	    get {
		return (byte) ((descriptor >> ShapeBits) & (MaximumCollections - 1));
	    }
	    set {
		if (IsEmpty()) descriptor = 0;
		descriptor = (ushort) (CLUT << (CollectionBits + ShapeBits) | value << ShapeBits | Bitmap);
	    }
	}

	public byte Bitmap {
	    get {
		return (byte) (descriptor & (MaximumShapes - 1));
	    }
	    set {
		if (IsEmpty()) descriptor = 0;
		descriptor = (ushort) (CLUT << (CollectionBits + ShapeBits) | Collection << ShapeBits | value);
	    }
	}
	
	ushort descriptor;
    }
}