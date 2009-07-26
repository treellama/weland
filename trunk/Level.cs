using System;
using System.IO;
using System.Collections.Generic;

namespace Weland {
    public class Level {
	public List<Point> Endpoints = new List<Point> ();
	public List<Line> Lines = new List<Line>();
	public List<Polygon> Polygons = new List<Polygon>();

	public const uint Tag = 0x4d696e66; // Minf
	public short Environment;
	public short PhysicsModel;
	public short Landscape;
	public short MissionFlags;
	public short EnvironmentFlags;
	
	public string Name;
	public uint EntryPointFlags;

	public void Load(Wadfile.DirectoryEntry wad) {
	    if (wad.Chunks.ContainsKey(Tag)) {
		BinaryReaderBE reader = new BinaryReaderBE(new MemoryStream(wad.Chunks[Tag]));
		Environment = reader.ReadInt16();
		PhysicsModel = reader.ReadInt16();
		Landscape = reader.ReadInt16();
		MissionFlags = reader.ReadInt16();
		EnvironmentFlags = reader.ReadInt16();
		
		reader.BaseStream.Seek(8, SeekOrigin.Current);
		const int kLevelNameLength = 66;
		Name = reader.ReadMacString(kLevelNameLength);
		EntryPointFlags = reader.ReadUInt32();
	    }

	    if (wad.Chunks.ContainsKey(Point.Tag)) {
		BinaryReaderBE reader = new BinaryReaderBE(new MemoryStream(wad.Chunks[Point.Tag]));
		Endpoints.Clear();
		while (reader.BaseStream.Position < reader.BaseStream.Length) {
		    Point point = new Point();
		    point.Load(reader);
		    Endpoints.Add(point);
		}
	    } else if (wad.Chunks.ContainsKey(Endpoint.Tag)) {
		BinaryReaderBE reader = new BinaryReaderBE(new MemoryStream(wad.Chunks[Endpoint.Tag]));
		Endpoints.Clear();
		while (reader.BaseStream.Position < reader.BaseStream.Length) {
		    Endpoint endpoint = new Endpoint();
		    endpoint.Load(reader);
		    Endpoints.Add(endpoint.Vertex);
		}
	    } else {
		Console.WriteLine("Directory Entry contains no points!");
	    }

	    if (wad.Chunks.ContainsKey(Line.Tag)) {
		BinaryReaderBE reader = new BinaryReaderBE(new MemoryStream(wad.Chunks[Line.Tag]));
		Lines.Clear();
		while (reader.BaseStream.Position < reader.BaseStream.Length) {
		    Line line = new Line();
		    line.Load(reader);
		    Lines.Add(line);
		}
	    }

	    if (wad.Chunks.ContainsKey(Polygon.Tag)) {
		BinaryReaderBE reader = new BinaryReaderBE(new MemoryStream(wad.Chunks[Polygon.Tag]));
		Polygons.Clear();
		while (reader.BaseStream.Position < reader.BaseStream.Length) {
		    Polygon polygon = new Polygon();
		    polygon.Load(reader);
		    Polygons.Add(polygon);
		}
	    }
	}
	
	static public void Main(string[] args) {
	    if (args.Length == 1) {
		Wadfile wadfile = new Wadfile();
		wadfile.Load(args[0]);

		Level level = new Level();
		level.Load(wadfile.Directory[0]);
		Console.WriteLine("\"{0}\"", level.Name);
		Console.WriteLine("{0} Points", level.Endpoints.Count);
		Console.WriteLine("{0} Lines", level.Lines.Count);
		Console.WriteLine("{0} Polygons", level.Polygons.Count);
	    } else {
		Console.WriteLine("Test usage: wadfile.exe <wadfile>");
	    }
	}	
    }
}

