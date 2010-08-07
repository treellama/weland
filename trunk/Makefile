CC= mcs

VERSION := $(shell grep "^\[assembly\:AssemblyVersionAttribute" Assembly.cs | awk -F\" '{ print $$2 }' - | awk -F\. '{printf "%s.%s", $$1, $$2; if ($$3 != '0') printf ".%s", $$3}')
BUILD_ZIP_DIR=.build-zip
ZIP_DIR=weland-$(VERSION)

create-zipdir=mkdir -p "$(BUILD_ZIP_DIR)/$(ZIP_DIR)"
remove-zipdir=rm -rf "$(BUILD_ZIP_DIR)"

define copy-readme
	cp COPYING.txt "$(BUILD_ZIP_DIR)/$(ZIP_DIR)"
	cp README.txt "$(BUILD_ZIP_DIR)/$(ZIP_DIR)"
endef

all: .FORCE
	gmcs @weland.rsp
.FORCE:

windows: .FORCE
	gmcs @windows.rsp
Weland.app: .FORCE
	gmcs @weland.rsp
	rm -rf Weland.app
	mkdir -p Weland.app/Contents
	sed -e 's/WELAND_VERSION/$(VERSION)/g' mac/Info.plist > Weland.app/Contents/Info.plist
	mkdir -p Weland.app/Contents/Resources
	cp icons/Weland.icns Weland.app/Contents/Resources/
	mkdir -p Weland.app/Contents/MacOS
	cp mac/weland.sh Weland.app/Contents/MacOS/weland
	chmod u+x Weland.app/Contents/MacOS/weland
	cp Weland.exe Weland.app/Contents/MacOS/Weland.exe

winzip: windows-corflags
	$(create-zipdir)
	$(copy-readme)
	cp Weland.exe "$(BUILD_ZIP_DIR)/$(ZIP_DIR)"
	cd "$(BUILD_ZIP_DIR)" && zip -r "../weland-$(VERSION)-win.zip" "$(ZIP_DIR)"
	$(remove-zipdir)
maczip: Weland.app
	$(create-zipdir)
	$(copy-readme)
	cp -r Weland.app "$(BUILD_ZIP_DIR)/$(ZIP_DIR)"
	cd "$(BUILD_ZIP_DIR)" && zip -y -r "../weland-$(VERSION)-mac.zip" "$(ZIP_DIR)"
	$(remove-zipdir)
dist:
	$(create-zipdir)
	zip -r "$(BUILD_ZIP_DIR)/tmp.zip" . -i Makefile -i \*.cs -i \*.png -i \*.glade -i \*.txt -i \*.rsp -i mac/Info.plist -i mac/weland.sh -i icons/Weland.icns -i icons/Weland.ico
	unzip -d "$(BUILD_ZIP_DIR)/$(ZIP_DIR)" "$(BUILD_ZIP_DIR)/tmp.zip"
	cd "$(BUILD_ZIP_DIR)" && zip -r "../weland-$(VERSION)-src.zip" "$(ZIP_DIR)"
	$(remove-zipdir)
release: winzip maczip dist
.PHONY: clean
clean:
	$(remove-zipdir)
	rm -f Weland.exe
	rm -rf Weland.app
	rm -rf *.zip
	rm -rf *.mdb
