using Gtk;
using System;

using Widget = Gtk.Builder.ObjectAttribute;

namespace Weland
{
    public class PlatformParametersDialog
    {
        public PlatformParametersDialog(Window parent, Level theLevel, short index)
        {
            level = theLevel;
            platform_index = index;
            var builder = new Builder("platformparameters.glade");
            builder.Autoconnect(this);
            dialog1.TransientFor = parent;
        }

        void Load(Platform platform)
        {
            platformType.Active = (int)platform.Type;
            speed.Text = String.Format("{0:0.000}", (double)platform.Speed / 30);
            delay.Text = String.Format("{0:0.000}", (double)platform.Delay / 30);
            if (platform.MinimumHeight == -1)
            {
                autocalcMinHeight.Active = true;
                minHeight.Text = String.Format("{0:0.000}", World.ToDouble(level.AutocalPlatformMinimum(platform_index)));
                minHeight.Sensitive = false;
            }
            else
            {
                autocalcMinHeight.Active = false;
                minHeight.Text = String.Format("{0:0.000}", World.ToDouble(platform.MinimumHeight));
                minHeight.Sensitive = true;
            }

            if (platform.MaximumHeight == -1)
            {
                autocalcMaxHeight.Active = true;
                maxHeight.Text = String.Format("{0:0.000}", World.ToDouble(level.AutocalPlatformMaximum(platform_index)));
                maxHeight.Sensitive = false;
            }
            else
            {
                autocalcMaxHeight.Active = false;
                maxHeight.Text = String.Format("{0:0.000}", World.ToDouble(platform.MaximumHeight));
                maxHeight.Sensitive = true;
            }

            initiallyActive.Active = platform.InitiallyActive;
            initiallyExtended.Active = platform.InitiallyExtended;
            controllableByPlayers.Active = platform.IsPlayerControllable;
            controllableByMonsters.Active = platform.IsMonsterControllable;
            causesDamage.Active = platform.CausesDamage;
            reversesDirection.Active = platform.ReversesDirectionWhenObstructed;
            isADoor.Active = platform.IsDoor;
            if (platform.ComesFromFloor && platform.ComesFromCeiling)
            {
                fromBoth.Active = true;
            }
            else if (platform.ComesFromCeiling)
            {
                fromCeiling.Active = true;
            }
            else
            {
                fromFloor.Active = true;
            }
            floorToCeiling.Active = platform.ExtendsFloorToCeiling;

            onlyOnce.Active = platform.ActivatesOnlyOnce;
            activatesPolygonLights.Active = platform.ActivatesLight;
            activatesAdjacentPlatform.Active = platform.ActivatesAdjacentPlatformsWhenActivating;
            deactivatesAdjacentPlatform.Active = platform.DeactivatesAdjacentPlatformsWhenActivating;
            adjacentAtEachLevel.Active = platform.ActivatesAdjacantPlatformsAtEachLevel;
            if (platform.DeactivatesAtEachLevel)
            {
                deactivatesAtEachLevel.Active = true;
            }
            else if (platform.DeactivatesAtInitialLevel)
            {
                deactivatesAtInitialLevel.Active = true;
            }
            else
            {
                neverDeactivates.Active = true;
            }

            deactivatesPolygonLights.Active = platform.DeactivatesLight;
            deactivatesAdjacentPlatform2.Active = platform.DeactivatesAdjacentPlatformsWhenDeactivating;
            activatesAdjacentPlatform2.Active = platform.ActivatesAdjacentPlatformsWhenDeactivating;

            cantDeactivateExternally.Active = platform.CannotBeExternallyDeactivated;
            usesNativePolygonHeights.Active = platform.UsesNativePolygonHeights;
            delayBeforeActivation.Active = platform.DelaysBeforeActivation;
            doesntActivateParent.Active = platform.DoesNotActivateParent;
            contractsSlower.Active = platform.ContractsSlower;
            lockedDoor.Active = platform.IsLocked;
            secret.Active = platform.IsSecret;
            tag.Active = platform.Tag;
        }

        void Save(Platform platform)
        {
            platform.Type = (PlatformType)platformType.Active;
            double d = 0;
            if (double.TryParse(speed.Text, out d))
            {
                platform.Speed = (short)Math.Round(d * 30);
            }
            if (double.TryParse(delay.Text, out d))
            {
                platform.Delay = (short)Math.Round(d * 30);
            }

            if (autocalcMinHeight.Active)
            {
                platform.MinimumHeight = -1;
            }
            else if (double.TryParse(minHeight.Text, out d))
            {
                platform.MinimumHeight = World.FromDouble(d);
            }

            if (autocalcMaxHeight.Active)
            {
                platform.MaximumHeight = -1;
            }
            else if (double.TryParse(maxHeight.Text, out d))
            {
                platform.MaximumHeight = World.FromDouble(d);
            }

            platform.InitiallyActive = initiallyActive.Active;
            platform.InitiallyExtended = initiallyExtended.Active;
            platform.IsPlayerControllable = controllableByPlayers.Active;
            platform.IsMonsterControllable = controllableByMonsters.Active;
            platform.CausesDamage = causesDamage.Active;
            platform.ReversesDirectionWhenObstructed = reversesDirection.Active;
            platform.IsDoor = isADoor.Active;
            platform.ComesFromFloor = (fromFloor.Active || fromBoth.Active);
            platform.ComesFromCeiling = (fromCeiling.Active || fromBoth.Active);
            platform.ExtendsFloorToCeiling = floorToCeiling.Active;
            platform.ActivatesOnlyOnce = onlyOnce.Active;
            platform.ActivatesLight = activatesPolygonLights.Active;
            platform.ActivatesAdjacentPlatformsWhenActivating = activatesAdjacentPlatform.Active;
            platform.DeactivatesAdjacentPlatformsWhenActivating = deactivatesAdjacentPlatform.Active;
            platform.ActivatesAdjacantPlatformsAtEachLevel = adjacentAtEachLevel.Active;
            platform.DeactivatesAtEachLevel = deactivatesAtEachLevel.Active;
            platform.DeactivatesAtInitialLevel = deactivatesAtInitialLevel.Active;
            platform.DeactivatesLight = deactivatesPolygonLights.Active;
            platform.DeactivatesAdjacentPlatformsWhenDeactivating = deactivatesAdjacentPlatform2.Active;
            platform.ActivatesAdjacentPlatformsWhenDeactivating = activatesAdjacentPlatform2.Active;
            platform.CannotBeExternallyDeactivated = cantDeactivateExternally.Active;
            platform.UsesNativePolygonHeights = usesNativePolygonHeights.Active;
            platform.DelaysBeforeActivation = delayBeforeActivation.Active;
            platform.DoesNotActivateParent = doesntActivateParent.Active;
            platform.ContractsSlower = contractsSlower.Active;
            platform.IsLocked = lockedDoor.Active;
            platform.IsSecret = secret.Active;
            platform.Tag = (short)tag.Active;
        }

        public void Run()
        {
            basedOn.Model = basedOnStore;
            CellRendererText text = new CellRendererText();
            basedOn.PackStart(text, false);
            basedOn.AddAttribute(text, "text", 0);

            TreeIter activeIt = new TreeIter();
            for (int i = 0; i < level.Polygons.Count; ++i)
            {
                Polygon polygon = level.Polygons[i];
                if (polygon.Type == PolygonType.Platform && polygon.Permutation >= 0 && polygon.Permutation < level.Platforms.Count)
                {
                    TreeIter it = basedOnStore.AppendValues(i);
                    if (polygon.Permutation == platform_index)
                    {
                        activeIt = it;
                    }
                }
            }
            basedOn.SetActiveIter(activeIt);

            dialog1.ShowAll();
            dialog1.Show();
            if (dialog1.Run() == (int)ResponseType.Ok)
            {
                Save(level.Platforms[platform_index]);
            }
            dialog1.Destroy();
        }

        protected void OnAutocalcMinHeightToggled(object obj, EventArgs args)
        {
            minHeight.Text = String.Format("{0:0.000}", World.ToDouble(level.AutocalPlatformMinimum(platform_index)));
            minHeight.Sensitive = !autocalcMinHeight.Active;
        }

        protected void OnAutocalcMaxHeightToggled(object obj, EventArgs args)
        {
            maxHeight.Text = String.Format("{0:0.000}", World.ToDouble(level.AutocalPlatformMaximum(platform_index)));
            maxHeight.Sensitive = !autocalcMaxHeight.Active;
        }

        protected void OnBasedOnChanged(object obj, EventArgs args)
        {
            TreeIter it;
            if (basedOn.GetActiveIter(out it))
            {
                Load(level.Platforms[level.Polygons[(int)basedOn.Model.GetValue(it, 0)].Permutation]);
            }
        }

        protected void OnDefaults(object obj, EventArgs args)
        {
            Platform p = new Platform();
            p.SetTypeWithDefaults((PlatformType)platformType.Active);
            Load(p);
        }

        Level level;
        short platform_index;

        ListStore basedOnStore = new ListStore(typeof(int));

        [Widget] Dialog dialog1;

        [Widget] ComboBox platformType;
        [Widget] ComboBox basedOn;
        [Widget] Entry speed;
        [Widget] Entry delay;
        [Widget] CheckButton autocalcMinHeight;
        [Widget] Entry minHeight;
        [Widget] CheckButton autocalcMaxHeight;
        [Widget] Entry maxHeight;
        [Widget] CheckButton initiallyActive;
        [Widget] CheckButton initiallyExtended;
        [Widget] CheckButton controllableByPlayers;
        [Widget] CheckButton controllableByMonsters;
        [Widget] CheckButton causesDamage;
        [Widget] CheckButton reversesDirection;
        [Widget] CheckButton isADoor;
        [Widget] RadioButton fromFloor;
        [Widget] RadioButton fromCeiling;
        [Widget] RadioButton fromBoth;
        [Widget] CheckButton floorToCeiling;
        [Widget] CheckButton onlyOnce;
        [Widget] CheckButton activatesPolygonLights;
        [Widget] CheckButton activatesAdjacentPlatform;
        [Widget] CheckButton deactivatesAdjacentPlatform;
        [Widget] CheckButton adjacentAtEachLevel;
        [Widget] RadioButton neverDeactivates;
        [Widget] RadioButton deactivatesAtEachLevel;
        [Widget] RadioButton deactivatesAtInitialLevel;
        [Widget] CheckButton deactivatesPolygonLights;
        [Widget] CheckButton deactivatesAdjacentPlatform2;
        [Widget] CheckButton activatesAdjacentPlatform2;
        [Widget] CheckButton cantDeactivateExternally;
        [Widget] CheckButton usesNativePolygonHeights;
        [Widget] CheckButton delayBeforeActivation;
        [Widget] CheckButton doesntActivateParent;
        [Widget] CheckButton contractsSlower;
        [Widget] CheckButton lockedDoor;
        [Widget] CheckButton secret;
        [Widget] ComboBox tag;
    }
}