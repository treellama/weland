using Gtk;
using System;

using Widget = Gtk.Builder.ObjectAttribute;

namespace Weland
{
    public class LightParametersDialog
    {
        public LightParametersDialog(Window parent, Level theLevel, short index)
        {
            level = theLevel;
            light_index = index;
            var builder = new Builder("lightparameters.glade");
            builder.Autoconnect(this);
            dialog1.TransientFor = parent;
        }

        void Load(Light light)
        {
            preset.Active = (int)light.Type;
            phase.Text = String.Format("{0}", light.Phase);
            initiallyActive.Active = light.InitiallyActive;
            stateless.Active = light.Stateless;
            tag.Active = light.TagIndex;

            function1.Active = (int)light.BecomingActive.LightingFunction;
            period1.Text = String.Format("{0}", light.BecomingActive.Period);
            deltaPeriod1.Text = String.Format("{0}", light.BecomingActive.DeltaPeriod);
            intensity1.Text = String.Format("{0:0}", light.BecomingActive.Intensity * 100);
            deltaIntensity1.Text = String.Format("{0:0}", light.BecomingActive.DeltaIntensity * 100);

            function2.Active = (int)light.PrimaryActive.LightingFunction;
            period2.Text = String.Format("{0}", light.PrimaryActive.Period);
            deltaPeriod2.Text = String.Format("{0}", light.PrimaryActive.DeltaPeriod);
            intensity2.Text = String.Format("{0:0}", light.PrimaryActive.Intensity * 100);
            deltaIntensity2.Text = String.Format("{0:0}", light.PrimaryActive.DeltaIntensity * 100);

            function3.Active = (int)light.SecondaryActive.LightingFunction;
            period3.Text = String.Format("{0}", light.SecondaryActive.Period);
            deltaPeriod3.Text = String.Format("{0}", light.SecondaryActive.DeltaPeriod);
            intensity3.Text = String.Format("{0:0}", light.SecondaryActive.Intensity * 100);
            deltaIntensity3.Text = String.Format("{0:0}", light.SecondaryActive.DeltaIntensity * 100);

            function4.Active = (int)light.BecomingInactive.LightingFunction;
            period4.Text = String.Format("{0}", light.BecomingInactive.Period);
            deltaPeriod4.Text = String.Format("{0}", light.BecomingInactive.DeltaPeriod);
            intensity4.Text = String.Format("{0:0}", light.BecomingInactive.Intensity * 100);
            deltaIntensity4.Text = String.Format("{0:0}", light.BecomingInactive.DeltaIntensity * 100);

            function5.Active = (int)light.PrimaryInactive.LightingFunction;
            period5.Text = String.Format("{0}", light.PrimaryInactive.Period);
            deltaPeriod5.Text = String.Format("{0}", light.PrimaryInactive.DeltaPeriod);
            intensity5.Text = String.Format("{0:0}", light.PrimaryInactive.Intensity * 100);
            deltaIntensity5.Text = String.Format("{0:0}", light.PrimaryInactive.DeltaIntensity * 100);

            function6.Active = (int)light.SecondaryInactive.LightingFunction;
            period6.Text = String.Format("{0}", light.SecondaryInactive.Period);
            deltaPeriod6.Text = String.Format("{0}", light.SecondaryInactive.DeltaPeriod);
            intensity6.Text = String.Format("{0:0}", light.SecondaryInactive.Intensity * 100);
            deltaIntensity6.Text = String.Format("{0:0}", light.SecondaryInactive.DeltaIntensity * 100);
        }

        void Save(Light light)
        {
            light.Type = (LightType)preset.Active;
            short i = 0;
            if (short.TryParse(phase.Text, out i))
            {
                light.Phase = i;
            }

            light.InitiallyActive = initiallyActive.Active;
            light.Stateless = stateless.Active;
            light.TagIndex = (short)tag.Active;

            light.BecomingActive.LightingFunction = (LightingFunction)function1.Active;
            if (short.TryParse(period1.Text, out i))
            {
                light.BecomingActive.Period = i;
            }
            if (short.TryParse(deltaPeriod1.Text, out i))
            {
                light.BecomingActive.DeltaPeriod = i;
            }
            double d = 0;
            if (double.TryParse(intensity1.Text, out d) && d >= 0 && d <= 100)
            {
                light.BecomingActive.Intensity = d / 100;
            }
            if (double.TryParse(deltaIntensity1.Text, out d) && d >= 0 && d <= 100)
            {
                light.BecomingActive.DeltaIntensity = d / 100;
            }

            light.PrimaryActive.LightingFunction = (LightingFunction)function2.Active;
            if (short.TryParse(period2.Text, out i))
            {
                light.PrimaryActive.Period = i;
            }
            if (short.TryParse(deltaPeriod2.Text, out i))
            {
                light.PrimaryActive.DeltaPeriod = i;
            }
            if (double.TryParse(intensity2.Text, out d) && d >= 0 && d <= 100)
            {
                light.PrimaryActive.Intensity = d / 100;
            }
            if (double.TryParse(deltaIntensity2.Text, out d) && d >= 0 && d <= 100)
            {
                light.PrimaryActive.DeltaIntensity = d / 100;
            }

            light.SecondaryActive.LightingFunction = (LightingFunction)function3.Active;
            if (short.TryParse(period3.Text, out i))
            {
                light.SecondaryActive.Period = i;
            }
            if (short.TryParse(deltaPeriod3.Text, out i))
            {
                light.SecondaryActive.DeltaPeriod = i;
            }
            if (double.TryParse(intensity3.Text, out d) && d >= 0 && d <= 100)
            {
                light.SecondaryActive.Intensity = d / 100;
            }
            if (double.TryParse(deltaIntensity3.Text, out d) && d >= 0 && d <= 100)
            {
                light.SecondaryActive.DeltaIntensity = d / 100;
            }

            light.BecomingInactive.LightingFunction = (LightingFunction)function4.Active;
            if (short.TryParse(period4.Text, out i))
            {
                light.BecomingInactive.Period = i;
            }
            if (short.TryParse(deltaPeriod4.Text, out i))
            {
                light.BecomingInactive.DeltaPeriod = i;
            }
            if (double.TryParse(intensity4.Text, out d) && d >= 0 && d <= 100)
            {
                light.BecomingInactive.Intensity = d / 100;
            }
            if (double.TryParse(deltaIntensity4.Text, out d) && d >= 0 && d <= 100)
            {
                light.BecomingInactive.DeltaIntensity = d / 100;
            }

            light.PrimaryInactive.LightingFunction = (LightingFunction)function5.Active;
            if (short.TryParse(period5.Text, out i))
            {
                light.PrimaryInactive.Period = i;
            }
            if (short.TryParse(deltaPeriod5.Text, out i))
            {
                light.PrimaryInactive.DeltaPeriod = i;
            }
            if (double.TryParse(intensity5.Text, out d) && d >= 0 && d <= 100)
            {
                light.PrimaryInactive.Intensity = d / 100;
            }
            if (double.TryParse(deltaIntensity5.Text, out d) && d >= 0 && d <= 100)
            {
                light.PrimaryInactive.DeltaIntensity = d / 100;
            }

            light.SecondaryInactive.LightingFunction = (LightingFunction)function6.Active;
            if (short.TryParse(period6.Text, out i))
            {
                light.SecondaryInactive.Period = i;
            }
            if (short.TryParse(deltaPeriod6.Text, out i))
            {
                light.SecondaryInactive.DeltaPeriod = i;
            }
            if (double.TryParse(intensity6.Text, out d) && d >= 0 && d <= 100)
            {
                light.SecondaryInactive.Intensity = d / 100;
            }
            if (double.TryParse(deltaIntensity6.Text, out d) && d >= 0 && d <= 100)
            {
                light.SecondaryInactive.DeltaIntensity = d / 100;
            }
        }

        public int Run()
        {
            basedOn.Model = basedOnStore;
            CellRendererText text = new CellRendererText();
            basedOn.PackStart(text, false);
            basedOn.AddAttribute(text, "text", 0);

            for (int i = 0; i < level.Lights.Count; ++i)
            {
                basedOnStore.AppendValues(i);
            }

            basedOn.Active = light_index;

            dialog1.ShowAll();
            dialog1.Show();
            int response = dialog1.Run();
            if (response == (int)ResponseType.Ok)
            {
                Save(level.Lights[light_index]);
            }
            dialog1.Destroy();

            return response;
        }

        protected void OnBasedOnChanged(object obj, EventArgs args)
        {
            Load(level.Lights[basedOn.Active]);
        }

        protected void OnPresetChanged(object obj, EventArgs args)
        {
            Light light = new Light();
            light.SetTypeWithDefaults((LightType)preset.Active);
            Load(light);
        }

    #pragma warning disable 0649
        [Widget] Dialog dialog1;

        [Widget] ComboBox preset;
        [Widget] ComboBox basedOn;
        [Widget] Entry phase;
        [Widget] CheckButton initiallyActive;
        [Widget] CheckButton stateless;
        [Widget] ComboBox tag;

        [Widget] ComboBox function1;
        [Widget] Entry period1;
        [Widget] Entry deltaPeriod1;
        [Widget] Entry intensity1;
        [Widget] Entry deltaIntensity1;

        [Widget] ComboBox function2;
        [Widget] Entry period2;
        [Widget] Entry deltaPeriod2;
        [Widget] Entry intensity2;
        [Widget] Entry deltaIntensity2;

        [Widget] ComboBox function3;
        [Widget] Entry period3;
        [Widget] Entry deltaPeriod3;
        [Widget] Entry intensity3;
        [Widget] Entry deltaIntensity3;

        [Widget] ComboBox function4;
        [Widget] Entry period4;
        [Widget] Entry deltaPeriod4;
        [Widget] Entry intensity4;
        [Widget] Entry deltaIntensity4;

        [Widget] ComboBox function5;
        [Widget] Entry period5;
        [Widget] Entry deltaPeriod5;
        [Widget] Entry intensity5;
        [Widget] Entry deltaIntensity5;

        [Widget] ComboBox function6;
        [Widget] Entry period6;
        [Widget] Entry deltaPeriod6;
        [Widget] Entry intensity6;
        [Widget] Entry deltaIntensity6;
    #pragma warning restore 0649

        ListStore basedOnStore = new ListStore(typeof(int));

        Level level;
        short light_index;
    }
}
