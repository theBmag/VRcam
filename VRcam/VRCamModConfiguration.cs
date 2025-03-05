using System;
using System.IO;
using System.Linq;
using System.Globalization;

namespace VRCam
{
    public static class VRCamModConfiguration
    {
        // Global switch for smoothing on shared mods.
        public static bool DisableSmoothing { get; set; } = false;

        // Viewfinder configuration.
        public static float BaseWidth { get; set; } = 0.5f;
        public static float[] PresetHeights { get; set; } = new float[] { 0.35f, 0.27f };
        public static string[] AspectRatioLabels { get; set; } = new string[] { "16:9", "21:9" };

        // Smoothing factors for the viewfinder transitions.
        public static float ViewfinderSmoothingFactor { get; set; } = 0.1f;
        public static float AspectSmoothingFactor { get; set; } = 0.1f;

        // Zoom settings.
        public static float DefaultZoom { get; set; } = 1.0f;
        public static float[] ZoomLevels { get; set; } = new float[] { 0.80f, 1.0f, 1.5f, 2.0f, 3.0f, 4.0f };

        // Settings for rat attachment smoothing.
        public static float PositionalSmoothingFactor { get; set; } = 5.0f;
        public static float RotationSmoothingFactor { get; set; } = 0.1f;

        // Removed Vehicle Smoothing Factors

        // Static constructor loads the INI file once at startup.
        static VRCamModConfiguration()
        {
            Load("VRcam.ini");
        }

        /// <summary>
        /// Loads configuration values from the specified INI file.
        /// If the file is not found, default values remain in effect.
        /// 
        /// Example VRcam.ini file:
        /// DisableSmoothing=false
        /// BaseWidth=0.5
        /// PresetHeights=0.35,0.27
        /// AspectRatioLabels=16:9,21:9
        /// ViewfinderSmoothingFactor=0.1
        /// AspectSmoothingFactor=0.1
        /// DefaultZoom=1.0
        /// ZoomLevels=0.80,1.0,1.5,2.0,3.0,4.0
        /// PositionalSmoothingFactor=5.0
        /// RotationSmoothingFactor=0.1
        /// 
        /// Use '.' as the decimal separator.
        /// </summary>
        public static void Load(string iniPath)
        {
            if (!File.Exists(iniPath))
            {
                GTA.UI.Screen.ShowSubtitle("VRcam.ini not found!");
                return;
            }

            string[] lines = File.ReadAllLines(iniPath);

            foreach (var line in lines)
            {
                // Trim and skip blank lines or comment lines (e.g., those starting with ';')
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";"))
                    continue;

                // Expecting each line in the form key=value.
                int idx = trimmed.IndexOf('=');
                if (idx < 0)
                    continue;

                string key = trimmed.Substring(0, idx).Trim();
                string value = trimmed.Substring(idx + 1).Trim();

                switch (key)
                {
                    case "DisableSmoothing":
                        if (bool.TryParse(value, out bool bVal))
                            DisableSmoothing = bVal;
                        break;
                    case "BaseWidth":
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float fVal))
                            BaseWidth = fVal;
                        break;
                    case "PresetHeights":
                        // Expected format: "0.35,0.27"
                        PresetHeights = value.Split(',')
                            .Select(s =>
                            {
                                float result;
                                return float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out result) ? result : 0f;
                            }).ToArray();
                        break;
                    case "AspectRatioLabels":
                        // Expected format: "16:9,21:9"
                        AspectRatioLabels = value.Split(',')
                            .Select(s => s.Trim()).ToArray();
                        break;
                    case "ViewfinderSmoothingFactor":
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out fVal))
                            ViewfinderSmoothingFactor = fVal;
                        break;
                    case "AspectSmoothingFactor":
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out fVal))
                            AspectSmoothingFactor = fVal;
                        break;
                    case "DefaultZoom":
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out fVal))
                            DefaultZoom = fVal;
                        break;
                    case "ZoomLevels":
                        // Expected format: "0.80,1.0,1.5,2.0,3.0,4.0"
                        ZoomLevels = value.Split(',')
                            .Select(s =>
                            {
                                float result;
                                return float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out result) ? result : 1.0f;
                            }).ToArray();
                        break;
                    case "PositionalSmoothingFactor":
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out fVal))
                            PositionalSmoothingFactor = fVal;
                        break;
                    case "RotationSmoothingFactor":
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out fVal))
                            RotationSmoothingFactor = fVal;
                        break;
                    default:
                        // Unknown key—ignore.
                        break;
                }
            }
        }
    }
}
