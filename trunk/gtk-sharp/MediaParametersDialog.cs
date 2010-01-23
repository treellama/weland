using Gtk;
using Glade;
using System;

namespace Weland {
    public class MediaParametersDialog {
	public MediaParametersDialog(Window parent, Level theLevel, short index) {
	    level = theLevel;
	    media_index = index;
	    Glade.XML gxml = new Glade.XML(null, "mediaparameters.glade", "dialog1", null);
	    gxml.Autoconnect(this);
	    dialog1.TransientFor = parent;
	}
	
	void Load(Media media) {
	    type.Active = (int) media.Type;
	    tide.Active = media.LightIndex;
	    angle.Value = media.Direction;
	    currentMagnitude.Text = String.Format("{0:0.000}", World.ToDouble(media.CurrentMagnitude));
	    low.Text = String.Format("{0:0.000}", World.ToDouble(media.Low));
	    high.Text = String.Format("{0:0.000}", World.ToDouble(media.High));
	    obstructed.Active = media.SoundObstructedByFloor;
	}

	void Save(Media media) { 
	    media.Type = (MediaType) type.Active;
	    media.LightIndex = (short) tide.Active;
	    media.Direction = angle.Value;
	    double d = 0;
	    if (double.TryParse(currentMagnitude.Text, out d)) {
		media.CurrentMagnitude = World.FromDouble(d);
	    }
	    if (double.TryParse(low.Text, out d)) {
		media.Low = World.FromDouble(d);
	    }
	    if (double.TryParse(high.Text, out d)) {
		media.High = World.FromDouble(d);
	    }
	    media.SoundObstructedByFloor = obstructed.Active;
	}

	protected void OnBasedOnChanged(object obj, EventArgs args) {
	    Load(level.Medias[basedOn.Active]);
	}

	public int Run() {
	    basedOn.Model = basedOnStore;
	    CellRendererText text = new CellRendererText();
	    basedOn.PackStart(text, false);
	    basedOn.AddAttribute(text, "text", 0);

	    for (int i = 0; i < level.Medias.Count; ++i) {
		basedOnStore.AppendValues(i);
	    }
	    basedOn.Active = media_index;

	    tide.Model = tideStore;
	    text = new CellRendererText();
	    tide.PackStart(text, false);
	    tide.AddAttribute(text, "text", 0);

	    for (int i = 0; i < level.Lights.Count; ++i) {
		tideStore.AppendValues(i);
	    }

	    Load(level.Medias[media_index]);

	    dialog1.ShowAll();
	    dialog1.Show();
	    int response = dialog1.Run();
	    if (response == (int) ResponseType.Ok) {
		Save(level.Medias[media_index]);
	    }
	    dialog1.Destroy();

	    return response;
	}

	Level level;
	short media_index;

	[Widget] Dialog dialog1;
	[Widget] ComboBox type;
	[Widget] ComboBox basedOn;
	[Widget] ComboBox tide;
	[Widget] HScale angle;
	[Widget] Entry currentMagnitude;
	[Widget] Entry low;
	[Widget] Entry high;
	[Widget] CheckButton obstructed;

	ListStore basedOnStore = new ListStore(typeof(int));
	ListStore tideStore = new ListStore(typeof(int));
    }
}