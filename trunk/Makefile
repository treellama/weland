CC= mcs

all: .FORCE
	gmcs @weland.rsp

.FORCE:

WelandMac.exe: .FORCE
	gmcs @mac.rsp

Weland.app: WelandMac.exe
	rm -rf Weland.app
	macpack -n Weland -m cocoa -i icons/Weland.icns WelandMac.exe

clean: .FORCE
	rm -f Weland.exe
	rm -f WelandMac.exe
	rm -rf Weland.app
