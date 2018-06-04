using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Weland {
    enum WadfileVersion {
	PreEntryPoint,
	HasDirectoryEntry,
	SupportsOverlays,
	HasInfinityStuff
    }

    enum WadfileDataVersion {
	Marathon,
	MarathonTwo
    }

    public class Wadfile {
	const int headerSize = 128;

	public static uint Chunk(string s) {
	    return (uint) ((((byte) s[0]) << 24) | (((byte) s[1]) << 16) | (((byte) s[2]) << 8) | ((byte) s[3]));
	}

	public short Version {
	    get {
		return version;
	    }
	}
	short version;

	public short DataVersion;
	public string Filename;
	const int maxFilename = 64;
	public uint Checksum {
	    get {
		return checksum;
	    }
	}
	uint checksum;

	int directoryOffset;
	public int WadCount {
	    get {
		return Directory.Count;
	    }
	}

	protected short applicationSpecificDirectoryDataSize;
	short entryHeaderSize;
	short directoryEntryBaseSize;
	public uint ParentChecksum;
	
	public class BadMapException : Exception {
	    public BadMapException()
		{ 
		}
			
	    public BadMapException(string message) : base(message)
		{
		}
			
	    public BadMapException(string message, Exception inner) : base(message, inner)
		{
		}
	}

	public class DirectoryEntry {
	    internal const short BaseSize = 10;

	    public Dictionary<uint, byte[]> Chunks = new Dictionary<uint, byte[]> ();
	    internal const short HeaderSize = 16;
	    internal int Offset;
	    internal short Index;
	    public int Size {
		get {
		    int total = 0;
		    foreach (var kvp in Chunks) {
			total += kvp.Value.Length + HeaderSize;
		    }
		    return total;
		}
	    }

 	    internal void LoadEntry(BinaryReaderBE reader) {
		Offset = reader.ReadInt32();
		reader.ReadInt32(); // size
		Index = reader.ReadInt16();
	    }
	    
	    internal void LoadChunks(BinaryReaderBE reader) {
		long position = reader.BaseStream.Position;
		int nextOffset;
		do {
		    uint tag = reader.ReadUInt32();
		    nextOffset = reader.ReadInt32();
		    int length = reader.ReadInt32();
		    reader.ReadInt32(); // offset
					
		    Chunks[tag] = reader.ReadBytes(length);
					
		    if (nextOffset > 0) 
			reader.BaseStream.Seek(position + nextOffset, SeekOrigin.Begin);
		} while (nextOffset > 0);
	    }

	    internal void SaveEntry(BinaryWriterBE writer) {
		writer.Write(Offset);
		writer.Write((int) Size);
		writer.Write(Index);
	    }	

	    internal void SaveChunks(BinaryWriterBE writer, uint[] tagOrder) {
		// build a list of tags to write in order
		HashSet<uint> Used = new HashSet<uint>();
		List<uint> Tags = new List<uint>();

		foreach (uint tag in tagOrder) {
		    if (Chunks.ContainsKey(tag)) {
			Tags.Add(tag);
			Used.Add(tag);
		    }
		}

		foreach (var kvp in Chunks) {
		    if (!Used.Contains(kvp.Key)) {
			Tags.Add(kvp.Key);
			Used.Add(kvp.Key);
		    }
		}

		int offset = 0;

		foreach (uint tag in Tags) {
		    writer.Write(tag);
		    if (tag == Tags[Tags.Count - 1]) {
			writer.Write((uint) 0);
		    } else {
			writer.Write((int) offset + HeaderSize + Chunks[tag].Length);
		    }
		    writer.Write((int) Chunks[tag].Length);
		    writer.Write((int) 0);
		    writer.Write(Chunks[tag]);
		    offset += Chunks[tag].Length + HeaderSize;
		}
	    }

	    public DirectoryEntry Clone() {
		DirectoryEntry clone = (DirectoryEntry) MemberwiseClone();
		clone.Chunks = new Dictionary<uint, byte[]>();
		foreach (var kvp in Chunks) {
		    clone.Chunks[kvp.Key] = (byte[]) kvp.Value.Clone();
		}
		return clone;
	    }
	}

	bool MacBinaryHeader(byte[] header) {
	    if (header[0] != 0 || header[1] > 64 || header[74] != 0 || header[123] > 0x81)
		return false;

	    ushort crc = 0;
	    for (int i = 0; i < 124; ++i) {
		ushort data = (ushort) (header[i] << 8);
		for (int j = 0; j < 8; ++j) {
		    if (((data ^ crc) & 0x8000) == 0x8000)
			crc = (ushort) ((crc << 1) ^ 0x1021);
		    else
			crc <<= 1;
		    data <<= 1;
		}
	    }

	    if (crc != ((header[124] << 8) | header[125]))
		return false;

	    return true;
	}

	public SortedDictionary<int, DirectoryEntry> Directory = new SortedDictionary<int, DirectoryEntry> ();

        protected virtual void LoadApplicationSpecificDirectoryData(BinaryReaderBE reader, int index) {
            reader.ReadBytes(applicationSpecificDirectoryDataSize);
        }

        protected virtual void SaveApplicationSpecificDirectoryData(BinaryWriterBE writer, int index) {

        }

        protected virtual void SetApplicationSpecificDirectoryDataSize() {
            applicationSpecificDirectoryDataSize = 0;
        }

        protected virtual uint[] GetTagOrder() {
            return new uint[] { };
        }

	public virtual void Load(string filename) {
	    BinaryReaderBE reader = new BinaryReaderBE(File.Open(filename, FileMode.Open));
	    try {
		// is it MacBinary?
		int fork_start = 0;
		if (MacBinaryHeader(reader.ReadBytes(128))) {
		    fork_start = 128;
		}
		reader.BaseStream.Seek(fork_start, SeekOrigin.Begin);
		
		// read the header
		version = reader.ReadInt16();
		DataVersion = reader.ReadInt16();
		Filename = reader.ReadMacString(maxFilename);
		checksum = reader.ReadUInt32();
		directoryOffset = reader.ReadInt32();
		short wadCount = reader.ReadInt16();
		applicationSpecificDirectoryDataSize = reader.ReadInt16();
		entryHeaderSize = reader.ReadInt16();
		
		directoryEntryBaseSize = reader.ReadInt16();

		// sanity check the map
		if (Version < 2 || entryHeaderSize != 16 || directoryEntryBaseSize != 10) {
		    throw new BadMapException("Only Marathon 2 and higher maps are supported");
		}
		
		ParentChecksum = reader.ReadUInt32();
		reader.ReadBytes(2 * 20); // unused
		
		// load the directory
		reader.BaseStream.Seek(directoryOffset + fork_start, SeekOrigin.Begin);
		for (int i = 0; i < wadCount; ++i) {
		    DirectoryEntry entry = new DirectoryEntry();
		    entry.LoadEntry(reader);
		    Directory[entry.Index] = entry;

                    LoadApplicationSpecificDirectoryData(reader, entry.Index);
		}
		
		// load all the wads(!)
		foreach (KeyValuePair<int, DirectoryEntry> kvp in Directory) {
		    reader.BaseStream.Seek(kvp.Value.Offset + fork_start, SeekOrigin.Begin);
		    kvp.Value.LoadChunks(reader);
		}
	    } finally {
		reader.Close();
	    }
	}

	public void Save(string filename) {
	    using (FileStream fs = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write)) {
		CrcStream crcStream = new CrcStream(fs);
		BinaryWriterBE writer = new BinaryWriterBE(crcStream);

		// set up the header
		if (Directory.Count == 1) {
		    version = (short) WadfileVersion.SupportsOverlays;
		} else {
		    version = (short) WadfileVersion.HasInfinityStuff;
		}
	    
		DataVersion = (short) WadfileDataVersion.MarathonTwo;
		checksum = 0;
		directoryOffset = headerSize;
		foreach (var kvp in Directory) {
		    kvp.Value.Offset = directoryOffset;
		    kvp.Value.Index = (short) kvp.Key;
		    directoryOffset += kvp.Value.Size;
		}

                SetApplicationSpecificDirectoryDataSize();
		entryHeaderSize = DirectoryEntry.HeaderSize;
		directoryEntryBaseSize = DirectoryEntry.BaseSize;
		ParentChecksum = 0;
	    
		// write the header
		writer.Write(version);
		writer.Write(DataVersion);
		writer.WriteMacString(filename.Split('.')[0], maxFilename);
		writer.Write(checksum);
		writer.Write(directoryOffset);
		writer.Write((short) Directory.Count);
		writer.Write(applicationSpecificDirectoryDataSize);
		writer.Write(entryHeaderSize);
		writer.Write(directoryEntryBaseSize);
		writer.Write(ParentChecksum);
		writer.Write(new byte[2 * 20]);

		// write wads
		foreach (var kvp in Directory) {
		    kvp.Value.SaveChunks(writer, GetTagOrder());
		}

		// write directory
		foreach (var kvp in Directory) {
		    kvp.Value.SaveEntry(writer);
                    SaveApplicationSpecificDirectoryData(writer, kvp.Value.Index);
                }

		// fix the checksum!
		checksum = crcStream.GetCRC();
		fs.Seek(68, SeekOrigin.Begin);
		writer.Write(checksum);
	    }
	}

	static public void Main(string[] args) {
	    if (args.Length == 2) {
		Wadfile wadfile = new Wadfile();
		wadfile.Load(args[0]);

		Wadfile export = new Wadfile();
		export.Directory[0] = wadfile.Directory[0];
		export.Save(args[1]);
		Console.WriteLine("DirectoryOffset: {0}", export.directoryOffset);
	    } else {
		Console.WriteLine("Test usage: wadfile.exe <wadfile> <export>");
	    }
	}
    }
}
