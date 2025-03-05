using System;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace VRCam
{
    public class HideGTAVivePistol : Script
    {
        // The model hash for the target prop.
        private const int TargetPropHash = unchecked((int)0x5778A9B1);

        public HideGTAVivePistol()
        {
            // Listen for key presses.
            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // If L is pressed, wait 1 second and then execute DeleteNearestSpecificProp.
            if (e.KeyCode == Keys.L)
            {
                Wait(100);
                DeleteNearestSpecificProp();
            }
        }

        private void DeleteNearestSpecificProp()
        {
            Ped player = Game.Player.Character;
            Vector3 playerPos = player.Position;

            Prop nearestProp = null;
            float nearestDistance = float.MaxValue;

            foreach (Prop prop in World.GetAllProps())
            {
                if (!prop.Exists())
                    continue;

                if (prop.Model.Hash == TargetPropHash)
                {
                    float distance = playerPos.DistanceTo(prop.Position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestProp = prop;
                    }
                }
            }

            if (nearestProp != null)
            {
                nearestProp.Delete();
                // Uncomment below for debug output:
                //GTA.UI.Screen.ShowSubtitle("Deleted prop 0x5778A9B1", 5000);
            }
            else
            {
                // Uncomment below for debug output:
                //GTA.UI.Screen.ShowSubtitle("No prop with hash 0x5778A9B1 found near player", 5000);
            }
        }
    }
}
