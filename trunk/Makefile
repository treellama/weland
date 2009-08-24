CC= mcs

all: .FORCE
	gmcs @weland.rsp

.FORCE:

windows: .FORCE
	gmcs @windows.rsp
Weland.app: .FORCE
	gmcs @weland.rsp
	rm -rf Weland.app
	macpack -n Weland -m cocoa -i icons/Weland.icns Weland.exe

clean: .FORCE
	rm -f Weland.exe
	rm -rf Weland.app
