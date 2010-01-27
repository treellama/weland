using System;
using System.IO;

namespace Weland {
    public class ShapesFile {
	CollectionHeader[] collectionHeaders;
	Collection[] collections;

	public void Load(BinaryReaderBE reader) {
	    long origin = reader.BaseStream.Position;
	    collectionHeaders = new CollectionHeader[ShapeDescriptor.MaximumCollections];
	    for (int i = 0; i < collectionHeaders.Length; ++i) {
		collectionHeaders[i] = new CollectionHeader();
		collectionHeaders[i].Load(reader);
	    }

	    collections = new Collection[collectionHeaders.Length];
	    for (int i = 0; i < collectionHeaders.Length; ++i) {
		collections[i] = new Collection();
		if (collectionHeaders[i].Offset > 0) {
		    reader.BaseStream.Seek(origin + collectionHeaders[i].Offset, SeekOrigin.Begin);
		    collections[i].Load(reader);
		}
	    }
	}
    }
}