using System;
using System.IO;

namespace Weland
{
    public class ShapesFile
    {
        CollectionHeader[] collectionHeaders;
        Collection[] collections;

        public void Load(BinaryReaderBE reader)
        {
            long origin = reader.BaseStream.Position;
            collectionHeaders = new CollectionHeader[ShapeDescriptor.MaximumCollections];
            for (int i = 0; i < collectionHeaders.Length; ++i)
            {
                collectionHeaders[i] = new CollectionHeader();
                collectionHeaders[i].Load(reader);
            }

            collections = new Collection[collectionHeaders.Length];
            for (int i = 0; i < collectionHeaders.Length; ++i)
            {
                collections[i] = new Collection();
                if (collectionHeaders[i].Offset > 0)
                {
                    reader.BaseStream.Seek(origin + collectionHeaders[i].Offset, SeekOrigin.Begin);
                    collections[i].Load(reader);
                }
            }
        }

        public void Load(string filename)
        {
            try
            {
                using (BinaryReaderBE reader = new BinaryReaderBE(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    Load(reader);
                }
            }
            catch (Exception)
            {
                collectionHeaders = new CollectionHeader[ShapeDescriptor.MaximumCollections];
                collections = new Collection[collectionHeaders.Length];
                for (int i = 0; i < collectionHeaders.Length; ++i)
                {
                    collectionHeaders[i] = new CollectionHeader();
                    collections[i] = new Collection();
                }
            }
        }

        public Collection GetCollection(int n)
        {
            return collections[n];
        }

        public Image GetShape(ShapeDescriptor d)
        {
            Collection coll = collections[d.Collection];
            if (d.Bitmap < coll.BitmapCount && d.CLUT < coll.ColorTableCount)
            {
                return coll.GetShape(d.CLUT, d.Bitmap);
            }
            else
            {
                var bitmap = new Image(128, 128);
                int dst = 0;
                while (dst < bitmap.Data.Length)
                {
                    bitmap.Data[dst++] = 127;
                }

                return bitmap;
            }
        }
    }
}
