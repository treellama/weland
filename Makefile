CC= mcs

SOURCES=BinaryReaderBE.cs Level.cs Line.cs MapDrawingArea.cs	\
MapWindow.cs Point.cs Polygon.cs Wadfile.cs Weland.cs

all:
	gmcs -pkg:gtk-sharp-2.0 -r:Mono.Cairo -out:Weland.exe -main:Weland.Weland $(SOURCES)
