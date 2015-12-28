CC=mcs

all: EnumerateTags.dll Shade.dll

EnumerateTags.dll: EnumerateTags.cs
	$(CC) -r:../Weland.exe -target:library EnumerateTags.cs

Shade.dll: Shade.cs
	$(CC) -r:../Weland.exe -target:library Shade.cs

clean:
	rm -f *.dll
