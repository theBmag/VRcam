using System;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Windows.Forms;

namespace VRCam
{
    public class ViewfinderScript : Script
    {
        private bool viewfinderEnabled = false; // Start with the viewfinder disabled.
        private int aspectIndex = 0;
        private float currentBaseHeight;
        private float targetBaseHeight;
        private int zoomIndex = 1;
        private float currentZoom = VRCamModConfiguration.DefaultZoom;
        private float targetZoom = VRCamModConfiguration.DefaultZoom;
        private bool ruleOfThirdsEnabled = true;
        private ViewfinderText viewFinderText;

        public ViewfinderScript()
        {
            currentBaseHeight = VRCamModConfiguration.PresetHeights[aspectIndex];
            targetBaseHeight = VRCamModConfiguration.PresetHeights[aspectIndex];

            // Initialize with a default position.
            viewFinderText = new ViewfinderText(0.5f, 0.75f);

            Tick += OnTick;
            KeyDown += OnKeyDown;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!viewfinderEnabled)
                return;

            // Apply smoothing.
            currentZoom = currentZoom + (targetZoom - currentZoom) * VRCamModConfiguration.ViewfinderSmoothingFactor;
            currentBaseHeight = currentBaseHeight + (targetBaseHeight - currentBaseHeight) * VRCamModConfiguration.AspectSmoothingFactor;

            // Calculate viewfinder boundaries.
            float effectiveWidth = VRCamModConfiguration.BaseWidth / currentZoom;
            float effectiveHeight = currentBaseHeight / currentZoom;
            float vfLeft = 0.5f - effectiveWidth / 2f;
            float vfRight = 0.5f + effectiveWidth / 2f;
            float vfTop = 0.5f - effectiveHeight / 2f;
            float vfBottom = 0.5f + effectiveHeight / 2f;

            DrawRectangles(vfLeft, vfRight, vfTop, vfBottom, effectiveWidth, effectiveHeight);

            // Update text position below the viewfinder.
            viewFinderText.UpdatePosition(0.5f, vfBottom + 0.01f);

            // Update the info text using the global smoothing flag.
            viewFinderText.UpdateInfo(currentZoom,
                                      VRCamModConfiguration.AspectRatioLabels[aspectIndex]);
            viewFinderText.Display();
        }

        private void DrawRectangles(float vfLeft, float vfRight, float vfTop, float vfBottom, float effectiveWidth, float effectiveHeight)
        {
            DrawRectangle(0.5f, vfTop / 2f, 1.0f, vfTop);
            DrawRectangle(0.5f, (vfBottom + 1.0f) / 2f, 1.0f, 1.0f - vfBottom);
            DrawRectangle(vfLeft / 2f, 0.5f, vfLeft, effectiveHeight);
            DrawRectangle((vfRight + 1.0f) / 2f, 0.5f, 1.0f - vfRight, effectiveHeight);
            DrawBorder(vfTop, vfBottom, vfLeft, vfRight, effectiveWidth, effectiveHeight);

            if (ruleOfThirdsEnabled)
            {
                DrawGrid(vfTop, vfBottom, vfLeft, vfRight, effectiveWidth, effectiveHeight);
            }
        }

        private void DrawRectangle(float centerX, float centerY, float width, float height, int r = 0, int g = 0, int b = 0, int a = 150)
        {
            Function.Call(Hash.DRAW_RECT, centerX, centerY, width, height, r, g, b, a);
        }

        private void DrawBorder(float vfTop, float vfBottom, float vfLeft, float vfRight, float effectiveWidth, float effectiveHeight)
        {
            float borderThickness = 0.002f;
            int borderR = 255, borderG = 255, borderB = 255, borderA = 200;

            DrawRectangle(0.5f, vfTop + borderThickness / 2f, effectiveWidth, borderThickness, borderR, borderG, borderB, borderA);
            DrawRectangle(0.5f, vfBottom - borderThickness / 2f, effectiveWidth, borderThickness, borderR, borderG, borderB, borderA);
            DrawRectangle(vfLeft + borderThickness / 2f, 0.5f, borderThickness, effectiveHeight, borderR, borderG, borderB, borderA);
            DrawRectangle(vfRight - borderThickness / 2f, 0.5f, borderThickness, effectiveHeight, borderR, borderG, borderB, borderA);
        }

        private void DrawGrid(float vfTop, float vfBottom, float vfLeft, float vfRight, float effectiveWidth, float effectiveHeight)
        {
            float gridThickness = 0.002f;
            int gridR = 200, gridG = 200, gridB = 200, gridA = 150;

            float verticalLine1X = vfLeft + effectiveWidth / 3f;
            float verticalLine2X = vfLeft + 2f * effectiveWidth / 3f;
            float verticalCenterY = (vfTop + vfBottom) / 2f;

            DrawRectangle(verticalLine1X, verticalCenterY, gridThickness, effectiveHeight, gridR, gridG, gridB, gridA);
            DrawRectangle(verticalLine2X, verticalCenterY, gridThickness, effectiveHeight, gridR, gridG, gridB, gridA);

            float horizontalLine1Y = vfTop + effectiveHeight / 3f;
            float horizontalLine2Y = vfTop + 2f * effectiveHeight / 3f;
            float horizontalCenterX = (vfLeft + vfRight) / 2f;

            DrawRectangle(horizontalCenterX, horizontalLine1Y, effectiveWidth, gridThickness, gridR, gridG, gridB, gridA);
            DrawRectangle(horizontalCenterX, horizontalLine2Y, effectiveWidth, gridThickness, gridR, gridG, gridB, gridA);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.L) // Press "L" to toggle the viewfinder.
            {
                viewfinderEnabled = !viewfinderEnabled;
            }
            else if (e.KeyCode == Keys.J)
            {
                if (zoomIndex > 0)
                {
                    zoomIndex--;
                    targetZoom = VRCamModConfiguration.ZoomLevels[zoomIndex];
                }
            }
            else if (e.KeyCode == Keys.K)
            {
                if (zoomIndex < VRCamModConfiguration.ZoomLevels.Length - 1)
                {
                    zoomIndex++;
                    targetZoom = VRCamModConfiguration.ZoomLevels[zoomIndex];
                }
            }
            else if (e.KeyCode == Keys.H)
            {
                aspectIndex = (aspectIndex + 1) % VRCamModConfiguration.PresetHeights.Length;
                targetBaseHeight = VRCamModConfiguration.PresetHeights[aspectIndex];
                GTA.UI.Screen.ShowSubtitle("Aspect Ratio: " + VRCamModConfiguration.AspectRatioLabels[aspectIndex], 2000);
            }
            else if (e.KeyCode == Keys.U)
            {
                ruleOfThirdsEnabled = !ruleOfThirdsEnabled;
                GTA.UI.Screen.ShowSubtitle("Rule of Thirds Grid " + (ruleOfThirdsEnabled ? "Enabled" : "Disabled"), 2000);
            }
        }
    }
}