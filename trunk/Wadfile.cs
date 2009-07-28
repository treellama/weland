using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Weland {
    public class Wadfile {
	public short Version {
	    get {
		return version;
	    }
	}
	short version;

	public short DataVersion;
	public string Filename;
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

	short applicationSpecificDirectoryDataSize;
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
	    public short MissionFlags;
	    public short EnvironmentFlags;
	    public int EntryPointFlags;
	    public string LevelName;
	    public Dictionary<uint, byte[]> Chunks = new Dictionary<uint, byte[]> ();
		
	    internal const int DataSize = 74;
	    internal int Offset;
	    internal int Size;
	    internal short Index;

	    internal void LoadEntry(BinaryReaderBE reader) {
		Offset = reader.ReadInt32();
		Size = reader.ReadInt32();
		Index = reader.ReadInt16();
	    }
			
	    internal void LoadData(BinaryReaderBE reader) {
		MissionFlags = reader.ReadInt16();
		EnvironmentFlags = reader.ReadInt16();
		EntryPointFlags = reader.ReadInt32();
		const int kLevelNameLength = 66;
		LevelName = reader.ReadMacString(kLevelNameLength);
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

	public void Load(string filename) {
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
		Filename = reader.ReadMacString(64);
		checksum = reader.ReadUInt32();
		directoryOffset = reader.ReadInt32();
		short wadCount = reader.ReadInt16();
		applicationSpecificDirectoryDataSize = reader.ReadInt16();
		entryHeaderSize = reader.ReadInt16();
		
		directoryEntryBaseSize = reader.ReadInt16();
		
		// sanity check the map
		if (Version < 2 || DataVersion < 1 || entryHeaderSize != 16 || directoryEntryBaseSize != 10) {
		    throw new BadMapException("Only Marathon 2 and higher maps are supported");
		}
		
		ParentChecksum = reader.ReadUInt32();
		reader.ReadBytes(2 * 20); // unused
		
		// load the directory
		reader.BaseStream.Seek(directoryOffset + fork_start, SeekOrigin.Begin);
		for (int i = 0; i < wadCount; ++i) {
		    DirectoryEntry entry = new DirectoryEntry();
		    entry.LoadEntry(reader);
		    
		    if (applicationSpecificDirectoryDataSize == DirectoryEntry.DataSize) {
			entry.LoadData(reader);
		    } else {
			reader.ReadBytes(applicationSpecificDirectoryDataSize);
		    }		
		    Directory[entry.Index] = entry;
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

	static public void Main(string[] args) {
	    if (args.Length == 1) {
		Wadfile wadfile = new Wadfile();
		wadfile.Load(args[0]);
		Console.WriteLine("{0}: 0x{1:x}", wadfile.Filename, wadfile.Checksum);
		foreach (KeyValuePair<int, DirectoryEntry> kvp in wadfile.Directory) {
		    Console.WriteLine("{0}\t{1}\t{2}", kvp.Key, kvp.Value.Chunks.Count, kvp.Value.LevelName);
		}
	    } else {
		Console.WriteLine("Test usage: wadfile.exe <wadfile>");
	    }
	}
    }
}