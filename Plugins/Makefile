all: EnumerateTags.dll Shade.dll

EnumerateTags.dll: EnumerateTags.cs
	gmcs -r:../Weland.exe -target:library EnumerateTags.cs

Shade.dll: Shade.cs
	gmcs -r:../Weland.exe -target:library Shade.cs

clean:
	rm -f *.dll
