using System;
using System.IO;
using System.Collections.Generic;

namespace Weland {
    interface ISerializableBE {
	void Load(BinaryReaderBE reader);
	//void Save(BinaryWriterBE writer);
    }

    public partial class Level {
	public List<Point> Endpoints = new List<Point> ();
	public List<Line> Lines = new List<Line>();
	public List<Polygon> Polygons = new List<Polygon>();
	public List<MapObject> Objects = new List<MapObject>();

	public MapInfo MapInfo = new MapInfo();

	void LoadChunk(ISerializableBE chunk, byte[] data) {
	    chunk.Load(new BinaryReaderBE(new MemoryStream(data)));
	}

	void LoadChunkList<T>(List<T> list, byte[] data) where T : ISerializableBE, new() {
	    BinaryReaderBE reader = new BinaryReaderBE(new MemoryStream(data));
	    list.Clear();
	    while (reader.BaseStream.Position < reader.BaseStream.Length) {
		T t = new T();
		t.Load(reader);
		list.Add(t);
	    }
	}

	public void Load(Wadfile.DirectoryEntry wad) {
	    if (wad.Chunks.ContainsKey(MapInfo.Tag)) {
		LoadChunk(MapInfo, wad.Chunks[MapInfo.Tag]);
	    } else {
		throw new Wadfile.BadMapException("Incomplete level: missing map info chunk");
	    }

	    if (wad.Chunks.ContainsKey(Point.Tag)) {
		LoadChunkList<Point>(Endpoints, wad.Chunks[Point.Tag]);
	    } else if (wad.Chunks.ContainsKey(Endpoint.Tag)) {
		Endpoints.Clear();
		List<Endpoint> endpointList = new List<Endpoint>();
		LoadChunkList<Endpoint>(endpointList, wad.Chunks[Endpoint.Tag]);
		foreach (Endpoint e in endpointList) {
		    Endpoints.Add(e.Vertex);
		}
	    } else {
		throw new Wadfile.BadMapException("Incomplete level: missing points chunk");
	    }

	    if (wad.Chunks.ContainsKey(Line.Tag)) {
		LoadChunkList<Line>(Lines, wad.Chunks[Line.Tag]);
	    } else {
		throw new Wadfile.BadMapException("Incomplete level: missing lines chunk");
	    }

	    if (wad.Chunks.ContainsKey(Polygon.Tag)) {
		LoadChunkList<Polygon>(Polygons, wad.Chunks[Polygon.Tag]);
	    } else {
		throw new Wadfile.BadMapException("Incomplete level: missing polygons chunk");
	    }

	    if (wad.Chunks.ContainsKey(MapObject.Tag)) {
		LoadChunkList<MapObject>(Objects, wad.Chunks[MapObject.Tag]);
	    } else {
		throw new Wadfile.BadMapException("Incomplete level: missing map objects chunk");
	    }
	}
	
	static public void Main(string[] args) {
	    if (args.Length == 1) {
		Wadfile wadfile = new Wadfile();
		wadfile.Load(args[0]);

		Level level = new Level();
		level.Load(wadfile.Directory[0]);
		Console.WriteLine("\"{0}\"", level.MapInfo.Name);
		Console.WriteLine("{0} Points", level.Endpoints.Count);
		Console.WriteLine("{0} Lines", level.Lines.Count);
		Console.WriteLine("{0} Polygons", level.Polygons.Count);
	    } else {
		Console.WriteLine("Test usage: wadfile.exe <wadfile>");
	    }
	}	
    }
}

