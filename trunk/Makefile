CC= mcs

SOURCES=BinaryReaderBE.cs BinaryWriterBE.cs CairoDrawer.cs Drawer.cs	\
Editor.cs GdkDrawer.cs Geometry.cs Level.cs Line.cs MapDrawingArea.cs	\
MapInfo.cs MapObject.cs MapWindow.cs Point.cs Polygon.cs		\
SystemDrawer.cs Wadfile.cs Weland.cs

all:
	gmcs -pkg:gtk-sharp-2.0 -pkg:gtk-dotnet-2.0 -r:System.Drawing -r:Mono.Cairo -out:Weland.exe -main:Weland.Weland $(SOURCES)
