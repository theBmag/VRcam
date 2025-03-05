using System;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;

namespace VRCam
{
    public class AttachRatToCamera : Script
    {
        Ped rat;
        Vector3 offset = new Vector3(0.0f, 0.0f, 0.0f); // No additional offset

        // Adjust this value to fine-tune how much the parent's inertia influences the rat.
        private float vehicleInertiaCompensationFactor = 0.18f;

        public AttachRatToCamera()
        {
            Tick += OnTick;
            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.L) // Spawn rat.
            {
                Script.Wait(100); // Fix invisible indoor issue

                if (rat == null || !rat.Exists())
                {
                    SpawnRat();
                }
            }
            /*
            else if (e.KeyCode == Keys.O) // Toggle smoothing.
            {
                bool wasDisabled = VRCamModConfiguration.DisableSmoothing;
                VRCamModConfiguration.DisableSmoothing = !VRCamModConfiguration.DisableSmoothing;
                string state = VRCamModConfiguration.DisableSmoothing ? "disabled" : "enabled";
                GTA.UI.Screen.ShowSubtitle("Smoothing " + state, 2000);

                // When re-enabling smoothing, delete and re-spawn the rat.
                if (!VRCamModConfiguration.DisableSmoothing && wasDisabled)
                {
                    if (rat != null && rat.Exists())
                    {
                        rat.Delete();
                    }
                    SpawnRat();
                }
            }
            */
        }

        private void SpawnRat()
        {
            Model ratModel = new Model(PedHash.Rat);
            ratModel.Request(500);

            if (ratModel.IsInCdImage && ratModel.IsValid)
            {
                while (!ratModel.IsLoaded)
                {
                    Script.Wait(100);
                }
                rat = World.CreatePed(ratModel, Game.Player.Character.Position + Game.Player.Character.ForwardVector * 2);
                Function.Call(Hash.SET_ENTITY_COLLISION, rat.Handle, false, false);
                Function.Call(Hash.SET_ENTITY_HAS_GRAVITY, rat.Handle, false);
                Function.Call(Hash.SET_ENTITY_VISIBLE, rat.Handle, false, false);
                rat.Task.ClearAll();
                Function.Call(Hash.SET_PED_TO_RAGDOLL, rat.Handle, 10000, 10000, 0, true, true, false);
                ratModel.MarkAsNoLongerNeeded();
                //Screen.ShowSubtitle("Rat spawned", 3000);
            }
        }

        // Computes the forward vector of the camera.
        private Vector3 GetForwardVector(Camera cam)
        {
            float pitchRad = cam.Rotation.X * (float)Math.PI / 180f;
            float yawRad = cam.Rotation.Z * (float)Math.PI / 180f;
            return new Vector3(
                (float)(-Math.Sin(yawRad) * Math.Cos(pitchRad)),
                (float)(Math.Cos(yawRad) * Math.Cos(pitchRad)),
                (float)Math.Sin(pitchRad)
            );
        }

        // New helper method to get the proper inertia velocity.
        // If the player's vehicle is attached to another vehicle,
        // use the parent's velocity; otherwise, use the player's vehicle velocity.
        private Vector3 GetInertiaVelocity()
        {
            Vehicle currentVehicle = Game.Player.Character.CurrentVehicle;
            if (currentVehicle == null || !currentVehicle.Exists())
                return Vector3.Zero;

            // Get the handle of the entity attached to the current vehicle.
            int parentHandle = Function.Call<int>(Hash.GET_ENTITY_ATTACHED_TO, currentVehicle.Handle);
            if (parentHandle != 0)
            {
                int modelHash = Function.Call<int>(Hash.GET_ENTITY_MODEL, parentHandle);
                bool isVehicle = Function.Call<bool>(Hash.IS_MODEL_A_VEHICLE, modelHash);

                if (isVehicle)
                {
                    // Retrieve the parent's velocity directly using a native call.
                    Vector3 parentVelocity = Function.Call<Vector3>(Hash.GET_ENTITY_VELOCITY, parentHandle);
                    return parentVelocity;
                }
            }
            return currentVehicle.Velocity;
        }



        private void OnTick(object sender, EventArgs e)
        {
            if (rat != null && rat.Exists())
            {
                // Ensure the rat remains in ragdoll state.
                rat.Health = 10;
                Function.Call(Hash.SET_ENTITY_COLLISION, rat.Handle, false, false);
                Function.Call(Hash.SET_ENTITY_HAS_GRAVITY, rat.Handle, false);
                Function.Call(Hash.SET_ENTITY_DYNAMIC, rat.Handle, true);
                Function.Call(Hash.SET_PED_TO_RAGDOLL, rat.Handle, 10000, 10000, 0, true, true, false);

                Camera cam = World.RenderingCamera;
                if (cam != null)
                {
                    Vector3 camPos = cam.Position;
                    Vector3 camRot = cam.Rotation;
                    Vector3 camForward = GetForwardVector(cam);
                    Vector3 targetPos = camPos + camForward + offset;

                    bool isInVehicle = Game.Player.Character.IsInVehicle();
                    float activeSmoothingFactor = VRCamModConfiguration.PositionalSmoothingFactor;
                    float activeRotationSmoothingFactor = VRCamModConfiguration.RotationSmoothingFactor;

                    // If the player is in a vehicle (or attached to one), adjust the target position
                    // by adding an offset based on the inertia (velocity) of the parent vehicle if available.
                    if (isInVehicle)
                    {
                        Vector3 inertiaVelocity = GetInertiaVelocity();
                        targetPos += inertiaVelocity * vehicleInertiaCompensationFactor;
                    }

                    // Update position.
                    if (!VRCamModConfiguration.DisableSmoothing)
                    {
                        Vector3 currentPos = rat.Position;
                        Vector3 delta = targetPos - currentPos;
                        Vector3 desiredVelocity = delta * activeSmoothingFactor;

                        // Also add the inertia influence using the parent's velocity if applicable.
                        if (isInVehicle)
                        {
                            desiredVelocity += GetInertiaVelocity() * vehicleInertiaCompensationFactor;
                        }
                        Function.Call(Hash.SET_ENTITY_VELOCITY, rat.Handle, desiredVelocity.X, desiredVelocity.Y, desiredVelocity.Z);
                    }
                    else
                    {
                        if (isInVehicle)
                        {
                            rat.Position = targetPos + GetInertiaVelocity() * vehicleInertiaCompensationFactor;
                        }
                        else
                        {
                            rat.Position = targetPos;
                        }
                    }

                    // Update rotation.
                    if (!VRCamModConfiguration.DisableSmoothing)
                    {
                        Vector3 currentRotation = rat.Rotation;
                        Vector3 smoothedRotation = LerpRotation(currentRotation, camRot, activeRotationSmoothingFactor);
                        rat.Rotation = smoothedRotation;
                    }
                    else
                    {
                        rat.Rotation = camRot;
                    }
                }
            }
        }

        private Vector3 LerpRotation(Vector3 current, Vector3 target, float factor)
        {
            return new Vector3(
                LerpAngle(current.X, target.X, factor),
                LerpAngle(current.Y, target.Y, factor),
                LerpAngle(current.Z, target.Z, factor)
            );
        }

        private float LerpAngle(float current, float target, float factor)
        {
            float delta = (target - current + 540) % 360 - 180;
            return current + delta * factor;
        }
    }
}
