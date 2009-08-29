using System;
using System.IO;
using System.Collections.Generic;

namespace Weland {
    interface ISerializableBE {
	void Load(BinaryReaderBE reader);
	void Save(BinaryWriterBE writer);
    }

    public partial class Level {
	public List<Point> Endpoints = new List<Point> ();
	public List<Line> Lines = new List<Line>();
	public List<Polygon> Polygons = new List<Polygon>();
	public List<MapObject> Objects = new List<MapObject>();
	public List<Side> Sides = new List<Side>();
	public List<Platform> Platforms = new List<Platform>();
	public Dictionary<uint, byte[]> Chunks = new Dictionary<uint, byte[]>();

	public MapInfo MapInfo = new MapInfo();

	// stuff for the editor, not saved to file
	public short TemporaryLineStartIndex = -1;
	public Point TemporaryLineEnd;

	public short SelectedPoint = -1;

	List<uint> ChunkFilter = new List<uint> {
	    Wadfile.Chunk("iidx"),
	    Endpoint.Tag,
	    Platform.DynamicTag
	};

	void LoadChunk(ISerializableBE chunk, byte[] data) {
	    chunk.Load(new BinaryReaderBE(new MemoryStream(data)));
	}
	
	byte[] SaveChunk(ISerializableBE chunk) {
	    MemoryStream stream = new MemoryStream();
	    BinaryWriterBE writer = new BinaryWriterBE(stream);
	    chunk.Save(writer);
	    return stream.ToArray();
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

	byte[] SaveChunk<T>(List<T> list) where T : ISerializableBE, new() {
		MemoryStream stream = new MemoryStream();
		BinaryWriterBE writer = new BinaryWriterBE(stream);
		foreach (T t in list) {
		    t.Save(writer);
		}
		
		return stream.ToArray();
	    }
	
	public void Load(Wadfile.DirectoryEntry wad) {
	    Chunks = wad.Chunks;
	    
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

	    if (wad.Chunks.ContainsKey(Side.Tag)) {
		LoadChunkList<Side>(Sides, wad.Chunks[Side.Tag]);
	    }

	    if (wad.Chunks.ContainsKey(Platform.StaticTag)) {
		LoadChunkList<Platform>(Platforms, wad.Chunks[Platform.StaticTag]);
	    } else if (wad.Chunks.ContainsKey(Platform.DynamicTag)) {
		BinaryReaderBE reader = new BinaryReaderBE(new MemoryStream(wad.Chunks[Platform.DynamicTag]));
		Platforms.Clear();
		while (reader.BaseStream.Position < reader.BaseStream.Length) {
		    Platform platform = new Platform();
		    platform.LoadDynamic(reader);
		    Platforms.Add(platform);

		    // open up the polygon
		    if (platform.PolygonIndex >= 0 && platform.PolygonIndex < Polygons.Count) {
			Polygon polygon = Polygons[platform.PolygonIndex];
			if (platform.ComesFromFloor) {
			    polygon.FloorHeight = platform.MinimumHeight;
			}
			if (platform.ComesFromCeiling) {
			    polygon.CeilingHeight = platform.MaximumHeight;
			}
		    }
		}
	    }

	    foreach (Polygon polygon in Polygons) {
		UpdatePolygonConcavity(polygon);
	    }

	    foreach (Line line in Lines) {
		Polygon p1 = null;
		Polygon p2 = null;
		if (line.ClockwisePolygonOwner != -1) {
		    p1 = Polygons[line.ClockwisePolygonOwner];
		}
		if (line.CounterclockwisePolygonOwner != -1) {
		    p2 = Polygons[line.CounterclockwisePolygonOwner];
		}

		if (p1 != null && p2 != null) {
		    line.HighestAdjacentFloor = Math.Max(p1.FloorHeight, p2.FloorHeight);
		    line.LowestAdjacentCeiling = Math.Min(p1.CeilingHeight, p2.CeilingHeight);
		} else if (p1 != null) {
		    line.HighestAdjacentFloor = p1.FloorHeight;
		    line.LowestAdjacentCeiling = p1.CeilingHeight;
		} else if (p2 != null) {
		    line.HighestAdjacentFloor = p2.FloorHeight;
		    line.LowestAdjacentCeiling = p2.CeilingHeight;
		} else {
		    line.HighestAdjacentFloor = 0;
		    line.LowestAdjacentCeiling = 0;
		}

		if ((line.Flags & LineFlags.VariableElevation) == LineFlags.VariableElevation) {
		    if (line.HighestAdjacentFloor >= line.LowestAdjacentCeiling) {
			line.Flags |= LineFlags.Solid;
		    } else {
			line.Flags &= ~LineFlags.Solid;
		    }
		}
	    }

	    if (wad.Chunks.ContainsKey(MapObject.Tag)) {
		LoadChunkList<MapObject>(Objects, wad.Chunks[MapObject.Tag]);
	    } else {
		throw new Wadfile.BadMapException("Incomplete level: missing map objects chunk");
	    }
	}

	public Wadfile.DirectoryEntry Save() {
	    Wadfile.DirectoryEntry wad = new Wadfile.DirectoryEntry();
	    wad.Chunks = Chunks;
	    wad.Chunks[MapInfo.Tag] = SaveChunk(MapInfo);
	    wad.Chunks[Point.Tag] = SaveChunk(Endpoints);
	    wad.Chunks[Line.Tag] = SaveChunk(Lines);
	    wad.Chunks[Polygon.Tag] = SaveChunk(Polygons);
	    wad.Chunks[Side.Tag] = SaveChunk(Sides);
	    wad.Chunks[MapObject.Tag] = SaveChunk(Objects);
	    wad.Chunks[Platform.StaticTag] = SaveChunk(Platforms);
	    
	    // remove merge-type chunks
	    foreach (uint tag in ChunkFilter) {
		wad.Chunks.Remove(tag);
	    }

	    return wad;
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

