using System;
using System.IO;
using System.Collections.Generic;

namespace Weland {
    public partial class Level {
	public List<Point> Endpoints = new List<Point> ();
	public List<Line> Lines = new List<Line>();
	public List<Polygon> Polygons = new List<Polygon>();
	public List<MapObject> Objects = new List<MapObject>();

	public const uint Tag = 0x4d696e66; // Minf
	public MapInfo MapInfo = new MapInfo();

	public void Load(Wadfile.DirectoryEntry wad) {
	    if (wad.Chunks.ContainsKey(MapInfo.Tag)) {
		BinaryReaderBE reader = new BinaryReaderBE(new MemoryStream(wad.Chunks[Tag]));
		MapInfo.Load(reader);
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

	    if (wad.Chunks.ContainsKey(MapObject.Tag)) {
		BinaryReaderBE reader = new BinaryReaderBE(new MemoryStream(wad.Chunks[MapObject.Tag]));
		Objects.Clear();
		while (reader.BaseStream.Position < reader.BaseStream.Length) {
		    MapObject mapObject = new MapObject();
		    mapObject.Load(reader);
		    Objects.Add(mapObject);
		}
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

