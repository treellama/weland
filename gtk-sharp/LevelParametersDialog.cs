using Gtk;
using System;

using Widget = Gtk.Builder.ObjectAttribute;

namespace Weland
{
    public class LevelParametersDialog
    {
        public LevelParametersDialog(Window parent, Level theLevel)
        {
            level = theLevel;
            var builder = new Builder("levelparameters.glade");
            builder.Autoconnect(this);
            dialog1.TransientFor = parent;
        }

        protected void OnSongIndexM1(object sender, EventArgs args)
        {
            songIndexEntry.Sensitive = songIndexM1.Active;
            songIndexLabel.Sensitive = songIndexM1.Active;
            landscape.Sensitive = !songIndexM1.Active;
            landscapeLabel.Sensitive = !songIndexM1.Active;

            if (songIndexM1.Active)
            {
                songIndexEntry.Text = level.Landscape.ToString();
                landscape.Active = -1;
            }
            else
            {
                songIndexEntry.Text = "";
                if (level.Landscape >= 0 && level.Landscape < 4)
                {
                    landscape.Active = level.Landscape;
                }
                else
                {
                    landscape.Active = 0;
                }
            }
        }

        public int Run()
        {
            levelName.Text = level.Name;
            environment.Active = level.Environment;

            if (level.SongIndexM1)
            {
                songIndexEntry.Sensitive = true;
                songIndexLabel.Sensitive = true;
                songIndexEntry.Text = level.Landscape.ToString();

                landscape.Sensitive = false;
                landscapeLabel.Sensitive = false;
            }
            else
            {
                landscape.Sensitive = true;
                landscapeLabel.Sensitive = true;
                landscape.Active = level.Landscape;

                songIndexEntry.Sensitive = false;
                songIndexLabel.Sensitive = false;
            }

            vacuum.Active = level.Vacuum;
            magnetic.Active = level.Magnetic;
            rebellion.Active = level.Rebellion;
            lowGravity.Active = level.LowGravity;
            rebellionM1.Active = level.RebellionM1;

            extermination.Active = level.Extermination;
            exploration.Active = level.Exploration;
            retrieval.Active = level.Retrieval;
            repair.Active = level.Repair;
            rescue.Active = level.Rescue;
            explorationM1.Active = level.ExplorationM1;
            rescueM1.Active = level.RescueM1;
            repairM1.Active = level.RepairM1;

            solo.Active = level.SinglePlayer;
            coop.Active = level.MultiplayerCooperative;
            emfh.Active = level.MultiplayerCarnage;
            ktmwtb.Active = level.KillTheManWithTheBall;
            koth.Active = level.KingOfTheHill;
            rugby.Active = level.Rugby;
            ctf.Active = level.CaptureTheFlag;

            glueM1.Active = level.GlueM1;
            ouchM1.Active = level.OuchM1;
            songIndexM1.Active = level.SongIndexM1;
            terminalsStopTime.Active = level.TerminalsStopTime;
            m1ActivationRange.Active = level.M1ActivationRange;
            m1Weapons.Active = level.M1Weapons;

            int response = dialog1.Run();

            if (response == (int)ResponseType.Ok)
            {
                level.Name = levelName.Text;
                level.Environment = (short)environment.Active;

                if (songIndexM1.Active)
                {
                    short s;
                    if (Int16.TryParse(songIndexEntry.Text, out s))
                    {
                        level.Landscape = s;
                    }
                }
                else
                {
                    level.Landscape = (short)landscape.Active;
                }

                level.Vacuum = vacuum.Active;
                level.Magnetic = magnetic.Active;
                level.Rebellion = rebellion.Active;
                level.LowGravity = lowGravity.Active;
                level.RebellionM1 = rebellionM1.Active;

                level.Extermination = extermination.Active;
                level.Exploration = exploration.Active;
                level.Retrieval = retrieval.Active;
                level.Repair = repair.Active;
                level.Rescue = rescue.Active;
                level.ExplorationM1 = explorationM1.Active;
                level.RescueM1 = rescueM1.Active;
                level.RepairM1 = repairM1.Active;

                level.SinglePlayer = solo.Active;
                level.MultiplayerCooperative = coop.Active;
                level.MultiplayerCarnage = emfh.Active;
                level.KillTheManWithTheBall = ktmwtb.Active;
                level.KingOfTheHill = koth.Active;
                level.Rugby = rugby.Active;
                level.CaptureTheFlag = ctf.Active;

                level.GlueM1 = glueM1.Active;
                level.OuchM1 = ouchM1.Active;
                level.SongIndexM1 = songIndexM1.Active;
                level.TerminalsStopTime = terminalsStopTime.Active;
                level.M1ActivationRange = m1ActivationRange.Active;
                level.M1Weapons = m1Weapons.Active;
            }

            dialog1.Destroy();
            return response;
        }

        Level level;

    #pragma warning disable 0649
        [Widget] Dialog dialog1;

        [Widget] Entry levelName;
        [Widget] ComboBox environment;
        [Widget] ComboBox landscape;
        [Widget] Entry songIndexEntry;
        [Widget] Label landscapeLabel;
        [Widget] Label songIndexLabel;

        [Widget] CheckButton vacuum;
        [Widget] CheckButton magnetic;
        [Widget] CheckButton rebellion;
        [Widget] CheckButton lowGravity;
        [Widget] CheckButton rebellionM1;

        [Widget] CheckButton extermination;
        [Widget] CheckButton exploration;
        [Widget] CheckButton retrieval;
        [Widget] CheckButton repair;
        [Widget] CheckButton rescue;
        [Widget] CheckButton explorationM1;
        [Widget] CheckButton rescueM1;
        [Widget] CheckButton repairM1;

        [Widget] CheckButton solo;
        [Widget] CheckButton coop;
        [Widget] CheckButton emfh;
        [Widget] CheckButton ktmwtb;
        [Widget] CheckButton koth;
        [Widget] CheckButton rugby;
        [Widget] CheckButton ctf;

        [Widget] CheckButton glueM1;
        [Widget] CheckButton ouchM1;
        [Widget] CheckButton songIndexM1;
        [Widget] CheckButton terminalsStopTime;
        [Widget] CheckButton m1ActivationRange;
        [Widget] CheckButton m1Weapons;
    #pragma warning restore 0649
    }
}
