using Gtk;
using System;

using Widget = Gtk.Builder.ObjectAttribute;

namespace Weland {
    public class AmbientSoundParametersDialog {
	public AmbientSoundParametersDialog(Window parent, AmbientSound theSound) {
	    sound = theSound;
	    Glade.XML gxml = new Glade.XML(null, "ambientsoundparameters.glade", "dialog1", null);
	    gxml.Autoconnect(this);
	    dialog1.TransientFor = parent;
	}

	public int Run() {
	    type.Active = (int) sound.SoundIndex;
	    volume.Text = String.Format("{0}", sound.Volume);
	    int response = dialog1.Run();
	    if (response == (int) ResponseType.Ok) {
		sound.SoundIndex = (short) type.Active;
		int i;
		if (int.TryParse(volume.Text, out i) && i >= 0 && i <= 100) {
		    sound.Volume = i;
		}
	    }
	    dialog1.Destroy();
	    return response;
	}

	[Widget] Dialog dialog1;
	[Widget] ComboBox type;
	[Widget] Entry volume;

	AmbientSound sound;
    }

    public class RandomSoundParametersDialog {
	public RandomSoundParametersDialog(Window parent, RandomSound theSound) {
	    sound = theSound;
	    Glade.XML gxml = new Glade.XML(null, "randomsoundparameters.glade", "dialog1", null);
	    gxml.Autoconnect(this);
	    dialog1.TransientFor = parent;
	}

	public int Run() {
	    type.Active = (int) sound.SoundIndex;
	    volume.Text = String.Format("{0}", sound.Volume);
	    deltaVolume.Text = String.Format("{0}", sound.DeltaVolume);
	    period.Text = String.Format("{0}", sound.Period);
	    deltaPeriod.Text = String.Format("{0}", sound.DeltaPeriod);
	    pitch.Text = String.Format("{0:0.00}", sound.Pitch);
	    deltaPitch.Text = String.Format("{0:0.00}", sound.DeltaPitch);
	    nondirectional.Active = sound.NonDirectional;
	    direction.Value = sound.Direction;
	    deltaDirection.Value = sound.DeltaDirection;
	    
	    int response = dialog1.Run();
	    if (response == (int) ResponseType.Ok) {
		sound.SoundIndex = (short) type.Active;

		int i;
		if (int.TryParse(volume.Text, out i) && i >= 0 && i <= 100) {
		    sound.Volume = i;
		}
		if (int.TryParse(deltaVolume.Text, out i) && i >= 0 && i <= 100) {
		    sound.DeltaVolume = i;
		}

		short s;
		if (short.TryParse(period.Text, out s) && s >= 0) {
		    sound.Period = s;
		}
		if (short.TryParse(deltaPeriod.Text, out s) && s >= 0) {
		    sound.DeltaPeriod = s;
		}
		
		double d;
		if (double.TryParse(pitch.Text, out d)) {
		    sound.Pitch = d;
		}
		if (double.TryParse(deltaPitch.Text, out d)) {
		    sound.DeltaPitch = d;
		}

		sound.Direction = direction.Value;
		sound.DeltaDirection = deltaDirection.Value;
		sound.NonDirectional = nondirectional.Active;
	    }
		
	    dialog1.Destroy();
	    return response;
	}

	[Widget] ComboBox type;
	[Widget] Entry volume;
	[Widget] Entry deltaVolume;
	[Widget] Entry period;
	[Widget] Entry deltaPeriod;
	[Widget] Entry pitch;
	[Widget] Entry deltaPitch;
	[Widget] CheckButton nondirectional;
	[Widget] HScale direction;
	[Widget] HScale deltaDirection;
	[Widget] Dialog dialog1;

	RandomSound sound;
    }
}