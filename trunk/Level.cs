using System;
using System.IO;
using System.Collections.Generic;

namespace Weland {
    interface ISerializableBE {
	void Load(BinaryReaderBE reader);
	void Save(BinaryWriterBE writer);
    }

    public class World {
	public const short One = 1024;
	public static short FromDouble(double d) {
	    return (short) Math.Round(d * World.One);
	}

	public static double ToDouble(short w) {
	    return ((double) w / World.One);
	}
    }

    public partial class Level {
	public List<Point> Endpoints = new List<Point> ();
	public List<Line> Lines = new List<Line>();
	public List<Polygon> Polygons = new List<Polygon>();
	public List<MapObject> Objects = new List<MapObject>();
	public List<Side> Sides = new List<Side>();
	public List<Platform> Platforms = new List<Platform>();
	public List<Light> Lights = new List<Light>();
	public Dictionary<uint, byte[]> Chunks = new Dictionary<uint, byte[]>();
	public List<Placement> ItemPlacement = new List<Placement>();
	public List<Placement> MonsterPlacement = new List<Placement>();

	MapInfo mapInfo = new MapInfo();

	// stuff for the editor, not saved to file
	public short TemporaryLineStartIndex = -1;
	public Point TemporaryLineEnd;

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

	public Level() {
	    // build a Forge-style light list
	    for (int i = 0; i <= 20; ++i) {
		Light light = new Light((double) (20 - i) / 20);
		Lights.Add(light);
	    }

	    for (int i = 0; i < Placement.Count; ++i) {
		ItemPlacement.Add(new Placement());
		MonsterPlacement.Add(new Placement());
	    }
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
		LoadChunk(mapInfo, wad.Chunks[MapInfo.Tag]);
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
	    
	    if (wad.Chunks.ContainsKey(Light.Tag)) {
		LoadChunkList<Light>(Lights, wad.Chunks[Light.Tag]);
	    }

	    if (wad.Chunks.ContainsKey(Placement.Tag)) {
		BinaryReaderBE reader = new BinaryReaderBE(new MemoryStream(wad.Chunks[Placement.Tag]));
		ItemPlacement.Clear();
		for (int i = 0; i < Placement.Count; ++i) {
		    Placement placement = new Placement();
		    placement.Load(reader);
		    ItemPlacement.Add(placement);
		}

		MonsterPlacement.Clear();
		for (int i = 0; i < Placement.Count; ++i) {
		    Placement placement = new Placement();
		    placement.Load(reader);
		    MonsterPlacement.Add(placement);
		}
	    }

	    foreach (Polygon polygon in Polygons) {
		UpdatePolygonConcavity(polygon);
	    }

	    for (int i = 0; i < Polygons.Count; ++i) {
		Polygon polygon = Polygons[i];
		if (polygon.Type == PolygonType.Platform) {
		    bool found = false;
		    for (int j = 0; j < Platforms.Count; ++j) {
			Platform platform = Platforms[j];
			if (platform.PolygonIndex == i) {
			    polygon.Permutation = (short) j;
			    found = true;
			    break;
			}
		    }
		    if (!found) {
			Platform platform = new Platform();
			platform.SetTypeWithDefaults(PlatformType.SphtDoor);
			platform.PolygonIndex = (short) i;
			polygon.Permutation = (short) Platforms.Count;
			Platforms.Add(platform);
		    }
		}
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

		if (line.VariableElevation) {
		    line.Solid = (line.HighestAdjacentFloor >= line.LowestAdjacentCeiling);
		}
	    }

	    if (wad.Chunks.ContainsKey(MapObject.Tag)) {
		LoadChunkList<MapObject>(Objects, wad.Chunks[MapObject.Tag]);
	    } else {
		throw new Wadfile.BadMapException("Incomplete level: missing map objects chunk");
	    }
	}

	public void AssurePlayerStart() {
	    bool found_start = false;
	    foreach (MapObject obj in Objects) {
		if (obj.Type == ObjectType.Player) {
		    found_start = true;
		    break;
		}
	    }

	    if (Polygons.Count > 0 && !found_start) {
		MapObject obj = new MapObject();
		obj.Type = ObjectType.Player;
		Point center = PolygonCenter(Polygons[0]);
		obj.X = center.X;
		obj.Y = center.Y;
		Objects.Add(obj);
	    }	    
	}

	public Wadfile.DirectoryEntry Save() {
	    Wadfile.DirectoryEntry wad = new Wadfile.DirectoryEntry();
	    wad.Chunks = Chunks;
	    wad.Chunks[MapInfo.Tag] = SaveChunk(mapInfo);
	    wad.Chunks[Point.Tag] = SaveChunk(Endpoints);
	    wad.Chunks[Line.Tag] = SaveChunk(Lines);
	    wad.Chunks[Polygon.Tag] = SaveChunk(Polygons);
	    wad.Chunks[Side.Tag] = SaveChunk(Sides);
	    wad.Chunks[MapObject.Tag] = SaveChunk(Objects);
	    wad.Chunks[Platform.StaticTag] = SaveChunk(Platforms);
	    wad.Chunks[Light.Tag] = SaveChunk(Lights);

	    {
		MemoryStream stream = new MemoryStream();
		BinaryWriterBE writer = new BinaryWriterBE(stream);
		foreach (Placement placement in ItemPlacement) {
		    placement.Save(writer);
		}
		foreach (Placement placement in MonsterPlacement) {
		    placement.Save(writer);
		}
		
		wad.Chunks[Placement.Tag] = stream.ToArray();
	    }
	    
	    
	    // remove merge-type chunks
	    foreach (uint tag in ChunkFilter) {
		wad.Chunks.Remove(tag);
	    }

	    return wad;
	}

	public string Name {
	    get {
		return mapInfo.Name;
	    }
	    set {
		mapInfo.Name = value;
	    }
	}

	public short Environment {
	    get {
		return mapInfo.Environment;
	    }
	    set {
		mapInfo.Environment = value;
	    }
	}

	public short Landscape {
	    get {
		return mapInfo.Landscape;
	    } 
	    set {
		mapInfo.Landscape = value;
	    }
	}
	
	public bool Vacuum {
	    get {
		return GetEnvironmentFlag(EnvironmentFlags.Vacuum);
	    } 
	    set {
		SetEnvironmentFlag(EnvironmentFlags.Vacuum, value);
	    }
	}

	public bool Magnetic {
	    get {
		return GetEnvironmentFlag(EnvironmentFlags.Magnetic);
	    }
	    set {
		SetEnvironmentFlag(EnvironmentFlags.Magnetic, value);
	    }
	}

	public bool Rebellion {
	    get {
		return GetEnvironmentFlag(EnvironmentFlags.Rebellion);
	    }
	    set {
		SetEnvironmentFlag(EnvironmentFlags.Rebellion, value);
	    }
	}

	public bool LowGravity {
	    get {
		return GetEnvironmentFlag(EnvironmentFlags.LowGravity);
	    }
	    set {
		SetEnvironmentFlag(EnvironmentFlags.LowGravity, value);
	    }
	}

	public bool Extermination {
	    get {
		return GetMissionFlag(MissionFlags.Extermination);
	    }
	    set {
		SetMissionFlag(MissionFlags.Extermination, value);
	    }
	}

	public bool Exploration {
	    get {
		return GetMissionFlag(MissionFlags.Exploration);
	    }
	    set {
		SetMissionFlag(MissionFlags.Exploration, value);
	    }
	}

	public bool Retrieval {
	    get {
		return GetMissionFlag(MissionFlags.Retrieval);
	    }
	    set {
		SetMissionFlag(MissionFlags.Retrieval, value);
	    }
	}

	public bool Repair {
	    get {
		return GetMissionFlag(MissionFlags.Repair);
	    }
	    set {
		SetMissionFlag(MissionFlags.Repair, value);
	    }
	}

	public bool Rescue {
	    get {
		return GetMissionFlag(MissionFlags.Rescue);
	    }
	    set {
		SetMissionFlag(MissionFlags.Rescue, value);
	    }
	}

	public bool SinglePlayer {
	    get {
		return GetEntryPointFlag(EntryPointFlags.SinglePlayer);
	    }
	    set {
		SetEntryPointFlag(EntryPointFlags.SinglePlayer, value);
	    }
	}

	public bool MultiplayerCooperative {
	    get {
		return GetEntryPointFlag(EntryPointFlags.MultiplayerCooperative);
	    }
	    set {
		SetEntryPointFlag(EntryPointFlags.MultiplayerCooperative, value);
	    }
	}

	public bool MultiplayerCarnage {
	    get {
		return GetEntryPointFlag(EntryPointFlags.MultiplayerCarnage);
	    }
	    set {
		SetEntryPointFlag(EntryPointFlags.MultiplayerCarnage, value);
	    }
	}

	public bool KillTheManWithTheBall {
	    get {
		return GetEntryPointFlag(EntryPointFlags.KillTheManWithTheBall);
	    }
	    set {
		SetEntryPointFlag(EntryPointFlags.KillTheManWithTheBall, value);
	    }
	}

	public bool KingOfTheHill {
	    get {
		return GetEntryPointFlag(EntryPointFlags.KingOfTheHill);
	    }
	    set {
		SetEntryPointFlag(EntryPointFlags.KingOfTheHill, value);
	    }
	}

	public bool Defense {
	    get {
		return GetEntryPointFlag(EntryPointFlags.Defense);
	    }
	    set {
		SetEntryPointFlag(EntryPointFlags.Defense, value);
	    }
	}

	public bool Rugby {
	    get {
		return GetEntryPointFlag(EntryPointFlags.Rugby);
	    }
	    set {
		SetEntryPointFlag(EntryPointFlags.Rugby, value);
	    }
	}

	public bool CaptureTheFlag {
	    get {
		return GetEntryPointFlag(EntryPointFlags.CaptureTheFlag);
	    }
	    set {
		SetEntryPointFlag(EntryPointFlags.CaptureTheFlag, value);
	    }
	}
	
	void SetEnvironmentFlag(EnvironmentFlags flag, bool value) {
	    if (value) {
		mapInfo.EnvironmentFlags |= flag;
	    } else {
		mapInfo.EnvironmentFlags &= ~flag;
	    }
	}

	bool GetEnvironmentFlag(EnvironmentFlags flag) {
	    return (mapInfo.EnvironmentFlags & flag) != 0;
	}

	void SetMissionFlag(MissionFlags flag, bool value) {
	    if (value) {
		mapInfo.MissionFlags |= flag;
	    } else {
		mapInfo.MissionFlags &= ~flag;
	    }
	}

	bool GetMissionFlag(MissionFlags flag) {
	    return (mapInfo.MissionFlags & flag) != 0;
	}

	void SetEntryPointFlag(EntryPointFlags flag, bool value) {
	    if (value) {
		mapInfo.EntryPointFlags |= flag;
	    } else {
		mapInfo.EntryPointFlags &= ~flag;
	    }
	}

	bool GetEntryPointFlag(EntryPointFlags flag) {
	    return (mapInfo.EntryPointFlags & flag) != 0;
	}

	static public void Main(string[] args) {
	    if (args.Length == 1) {
		Wadfile wadfile = new Wadfile();
		wadfile.Load(args[0]);

		Level level = new Level();
		level.Load(wadfile.Directory[0]);
		Console.WriteLine("\"{0}\"", level.mapInfo.Name);
		Console.WriteLine("{0} Points", level.Endpoints.Count);
		Console.WriteLine("{0} Lines", level.Lines.Count);
		Console.WriteLine("{0} Polygons", level.Polygons.Count);
	    } else {
		Console.WriteLine("Test usage: wadfile.exe <wadfile>");
	    }
	}
    }
}

