* Installing Plugins

You can install Weland plugins in one of two places. The easiest is to
put them in a Plugins directory in the same directory as Weland.

You can also install them in a Plugins directory in Weland's
application data directory:

Windows: C:\Documents and Settings\Weland\Application Data\Plugins
Mac OS X: /Users/<username>/.config/Weland/Plugins
Linux: ~/.config/Weland/Plugins

* Building Plugins

If you want to build your own plugins for Weland, you can use the included plugins as a guide. 

** Generic Plugins

For plugins that need no UI, you can implement the Run() static
method. Any output you write to System.Console will show up in a log
window after the plugin is run.

gmcs -r:/path/to/Weland.exe -target:library YourPlugin.css

** GTK plugins

If you need a UI, you should implement the GtkRun() static method
instead.

gmcs -r:/path/to/Weland.exe -pkg:gtk-sharp-2.0 -target:library YourGtkPlugin.cs
