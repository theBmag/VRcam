using System;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

public class SpawnInvincibleBmxRider : Script
{
    // Flag to ensure we only run the script once.
    private bool hasPressedL = false;

    public SpawnInvincibleBmxRider()
    {
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.L)
        {
            // Check if the player's character is already invisible OR if L was already pressed.
            if (!Game.Player.Character.IsVisible || hasPressedL)
            {
                return;
            }
            hasPressedL = true;
            SpawnPedOnBMX();
        }
    }

    private void SpawnPedOnBMX()
    {
        // Cache the original player ped and position.
        Ped originalPlayerPed = Game.Player.Character;
        Vector3 spawnPos = originalPlayerPed.Position;
        float heading = originalPlayerPed.Heading;

        // --- Detection Logic for Attachment Target ---
        Entity attachTarget = null;
        Vector3 localOffset = Vector3.Zero;
        bool shouldAttach = false;
        string attachMessage = "";

        if (originalPlayerPed.IsInVehicle())
        {
            Vehicle playerVehicle = originalPlayerPed.CurrentVehicle;
            attachTarget = playerVehicle;

            Vector3 worldOffset = originalPlayerPed.Position - playerVehicle.Position;
            float headingRad = playerVehicle.Heading * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(-headingRad);
            float sin = (float)Math.Sin(-headingRad);
            localOffset = new Vector3(
                worldOffset.X * cos - worldOffset.Y * sin,
                worldOffset.X * sin + worldOffset.Y * cos,
                worldOffset.Z - 1.6f
            );
            attachMessage = "Camera attached to the vehicle.";
            shouldAttach = true;

            Function.Call(Hash.SET_ENTITY_AS_MISSION_ENTITY, attachTarget.Handle, true, true);
        }
        else
        {
            Vector3 rayStart = originalPlayerPed.Position;
            Vector3 rayEnd = rayStart - new Vector3(0f, 0f, 2f);
            RaycastResult rayResult = World.Raycast(rayStart, rayEnd, IntersectFlags.Everything, originalPlayerPed);

            if (rayResult.DidHit && rayResult.HitEntity != null)
            {
                attachTarget = rayResult.HitEntity;

                Vector3 worldOffset = originalPlayerPed.Position - attachTarget.Position;
                float targetHeadingRad = attachTarget.Heading * (float)Math.PI / 180f;
                float cos = (float)Math.Cos(-targetHeadingRad);
                float sin = (float)Math.Sin(-targetHeadingRad);
                localOffset = new Vector3(
                    worldOffset.X * cos - worldOffset.Y * sin,
                    worldOffset.X * sin + worldOffset.Y * cos,
                    worldOffset.Z - 2.2f
                );

                attachMessage = "Camera attached to the vehicle/object below.";
                shouldAttach = true;

                if (attachTarget != null && attachTarget is Vehicle)
                {
                    Function.Call(Hash.SET_ENTITY_AS_MISSION_ENTITY, attachTarget.Handle, true, true);
                }

            }
        }
        // --- End of Detection Logic ---

        // Prepare and request the ped model.
        Model pedModel = new Model("a_m_y_business_01");
        if (!pedModel.IsInCdImage || !pedModel.IsValid)
        {
            GTA.UI.Screen.ShowSubtitle("Invalid ped model: a_m_y_business_01");
            return;
        }
        pedModel.Request(5000);
        if (!pedModel.IsLoaded)
        {
            GTA.UI.Screen.ShowSubtitle("Failed to load ped model.");
            return;
        }

        // Spawn the ambient ped.
        Ped ambientPed = World.CreatePed(pedModel, spawnPos);
        if (ambientPed == null)
        {
            GTA.UI.Screen.ShowSubtitle("Failed to spawn ped.");
            return;
        }
        ambientPed.IsInvincible = true;

        if (shouldAttach)
        {
            Model bmxModel = new Model("bmx");
            if (!bmxModel.IsInCdImage || !bmxModel.IsValid)
            {
                GTA.UI.Screen.ShowSubtitle("Invalid BMX model: bmx");
                return;
            }
            bmxModel.Request(5000);
            if (!bmxModel.IsLoaded)
            {
                GTA.UI.Screen.ShowSubtitle("Failed to load BMX model.");
                return;
            }

            Vehicle bmx = World.CreateVehicle(bmxModel, spawnPos, heading);
            if (bmx == null)
            {
                GTA.UI.Screen.ShowSubtitle("Failed to create BMX.");
                return;
            }

            ambientPed.SetIntoVehicle(bmx, VehicleSeat.Driver);
            bmx.IsInvincible = true;

            Function.Call(Hash.FREEZE_ENTITY_POSITION, bmx.Handle, true);
            Function.Call(Hash.SET_ENTITY_COLLISION, bmx.Handle, false, false);

            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, bmx.Handle, attachTarget.Handle, 0,
                localOffset.X, localOffset.Y, localOffset.Z,
                0f, 0f, 0f,
                false, false, false, false, 0, true);
            GTA.UI.Screen.ShowSubtitle(attachMessage);
            Function.Call(Hash.SET_ENTITY_ALPHA, bmx.Handle, 0, false);
        }
        else
        {
            //GTA.UI.Screen.ShowSubtitle("Invincible a_m_y_business_01 spawned on foot!");
            Vector3 forwardPosition = ambientPed.Position + ambientPed.ForwardVector * -0.5f;
            ambientPed.Position = forwardPosition;
            Script.Wait(1500);
            Function.Call(Hash.FREEZE_ENTITY_POSITION, ambientPed.Handle, true);
            Function.Call(Hash.SET_ENTITY_COLLISION, ambientPed.Handle, false, false);
        }

        // Finally, switch the player control to the ambient ped and ensure original ped remains visible.
        Function.Call(Hash.CHANGE_PLAYER_PED, Game.Player, ambientPed.Handle, true, true);
        Function.Call(Hash.SET_ENTITY_VISIBLE, originalPlayerPed.Handle, true);
        Function.Call(Hash.SET_ENTITY_VISIBLE, ambientPed.Handle, false);
        Function.Call(Hash.SET_ENTITY_ALPHA, originalPlayerPed.Handle, 255, false);

    }
}
