using System.Collections.Generic;
using System.IO;

namespace Weland {
    public class MapFile : Wadfile {
        public class Overlay {
            public MissionFlags MissionFlags;
	    public EnvironmentFlags EnvironmentFlags;
	    public EntryPointFlags EntryPointFlags;
	    public string LevelName;

            internal const short DataSize = 74;

            internal void LoadData(BinaryReaderBE reader) {
		MissionFlags = (MissionFlags) reader.ReadInt16();
		EnvironmentFlags = (EnvironmentFlags) reader.ReadInt16();
		EntryPointFlags = (EntryPointFlags) reader.ReadInt32();
		LevelName = reader.ReadMacString(MapInfo.LevelNameLength);
	    }

            internal void SaveData(BinaryWriterBE writer) {
		writer.Write((short) MissionFlags);
		writer.Write((short) EnvironmentFlags);
		writer.Write((int) EntryPointFlags);
		writer.WriteMacString(LevelName, MapInfo.LevelNameLength);
	    }
        }

        public SortedDictionary<int, Overlay> Overlays = new SortedDictionary<int, Overlay>();

        protected override void SetApplicationSpecificDirectoryDataSize() {
            if (Directory.Count == 1) {
                applicationSpecificDirectoryDataSize = 0;
            } else {
                applicationSpecificDirectoryDataSize = Overlay.DataSize;
            }
        }

        protected override void LoadApplicationSpecificDirectoryData(BinaryReaderBE reader, int index)
        {
            if (applicationSpecificDirectoryDataSize == Overlay.DataSize) {
                Overlay overlay = new Overlay();
                overlay.LoadData(reader);
                Overlays[index] = overlay;
            }
        }

        protected override void SaveApplicationSpecificDirectoryData(BinaryWriterBE writer, int index)
        {
            if (applicationSpecificDirectoryDataSize == Overlay.DataSize) {
                Overlays[index].SaveData(writer);
            }
        }

        protected override uint[] GetTagOrder() {
            return new uint[] { Point.Tag, Line.Tag, Side.Tag, Polygon.Tag, Light.Tag, Annotation.Tag, MapObject.Tag, MapInfo.Tag, Placement.Tag, Platform.StaticTag, Media.Tag, AmbientSound.Tag, RandomSound.Tag };
        }

        public override void Load(string filename) {
            base.Load(filename);

            if (DataVersion < 1) {
                throw new BadMapException("Only Marathon 2 and higher maps are supported");
            }

            if (applicationSpecificDirectoryDataSize != Overlay.DataSize) {
                foreach(var kvp in Directory) {
                    if (kvp.Value.Chunks.ContainsKey(MapInfo.Tag)) {
                        MapInfo info = new MapInfo();
                        BinaryReaderBE chunkReader = new BinaryReaderBE(new MemoryStream(kvp.Value.Chunks[MapInfo.Tag]));
                        info.Load(chunkReader);

                        Overlay overlay = new Overlay();
                        overlay.MissionFlags = info.MissionFlags;
                        overlay.EnvironmentFlags = info.EnvironmentFlags;
                        overlay.EntryPointFlags = info.EntryPointFlags;
                        overlay.LevelName = info.Name;

                        Overlays[kvp.Value.Index] = overlay;
                    }
                }
            }
        }
    }
}
