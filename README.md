# Overview

Weland is a Marathon map editor by Gregory Smith, written in C#, and
using GTK#

It is available under the GNU General Public License, see the file
COPYING for details.

# Install

To run Weland, you will need a C# runtime (either .NET 4 or Mono 2.10 or
higher), as well as GTK#

## Windows

1. Make sure you have the .NET runtime installed

    https://www.microsoft.com/en-in/download/details.aspx?id=30653
    
2. Install the GTK# for .NET package. _(This version seems to work best with Windows, and the official installers have gone offline, so I added it to the Weland releases page)_

    https://github.com/treellama/weland/releases/download/gtk-sharp/gtk-sharp-2.12.26.msi

3. Open Weland.exe like any other app

## Mac OS X

1. Download the Mono runtime, which includes everything you need:

    https://www.mono-project.com/download/stable/#download-mac
    
2. Open Weland.exe like any other app

## Linux

1. Install Mono and gtk-sharp2 from your distribution's package system. Then:

    ```
    make
    mono Weland.exe
    ```

# Visual Mode

Weland does not come with a built-in Visual Mode, but it can use the new command line arguments in Aleph One 1.4 or higher to pass maps into Aleph One to texture with a visual mode Lua script. 

## Setup 

1. In Weland's preferences, choose a shapes file, the scenario you want to use, and a copy of Aleph One 1.4 or higher.
2. Press "Edit Preferences" in the Visual Mode section of Weland's preferences.
3. Aleph One will start up--configure it with the window size you want, turn sound off if for the authentic Visual Mode experience, and most importantly, choose a texture editing script like Visual Mode.lua or the Vasara plugin (both downloaded separately)
4. Quit Aleph One
5. When you want to texture a map, choose Visual Mode from the View menu. Make any changes to the map, and quit Aleph One. Texture changes will automatically be imported back into Weland.


