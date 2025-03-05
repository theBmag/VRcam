using GTA;
using GTA.Native;
using System;

namespace VRCam
{
    public class ViewfinderText
    {
        private float textPosX;
        private float textPosY;
        private float textScale;
        // The default text color when the aspect ratio is correct.
        private int textR, textG, textB, textA;

        // This field stores the formatted info text.
        private string infoText;

        public ViewfinderText(float posX, float posY, float scale = 0.35f, int r = 255, int g = 255, int b = 255, int a = 255)
        {
            textPosX = posX;
            textPosY = posY;
            textScale = scale;
            textR = r;
            textG = g;
            textB = b;
            textA = a;
        }

        /// <summary>
        /// Updates the info text based on the current zoom, provided aspect label,
        /// and replay recording status.
        /// 
        /// If the current aspect ratio (obtained via GET_ASPECT_RATIO) is not 5:4 (≈1.25),
        /// then the info text is replaced with a warning message and its color is set to slight yellow.
        /// Otherwise, if recording is active, the text color is set to red; if not, white.
        /// </summary>
        public void UpdateInfo(float zoom, string aspectLabel)
        {
            bool isRecording = Function.Call<bool>(Hash.IS_REPLAY_RECORDING);
            string recordingStatus = isRecording ? "RECORDING" : "Inactive";

            // Get the current aspect ratio.
            float currentAspectRatio = Function.Call<float>(Hash.GET_ASPECT_RATIO);

            // 5:4 is 1.25. Allow a small tolerance.
            if (Math.Abs(currentAspectRatio - 1.25f) > 0.01f)
            {
                infoText = "Please change the aspect ratio to 5:4 in the graphics settings!";
                // Set text color to slight yellow.
                textR = 255;
                textG = 255;
                textB = 153;
            }
            else
            {
                infoText = string.Format("{0:F2}x  {1}  Recording: {2}",
                                           zoom, aspectLabel, recordingStatus);
                // If recording is active, set text color to red; otherwise white.
                if (isRecording)
                {
                    textR = 255;
                    textG = 100;
                    textB = 100;
                }
                else
                {
                    textR = 255;
                    textG = 255;
                    textB = 255;
                }
            }
        }

        /// <summary>
        /// Updates the text position.
        /// </summary>
        public void UpdatePosition(float posX, float posY)
        {
            textPosX = posX;
            textPosY = posY;
        }

        /// <summary>
        /// Displays the text on the screen.
        /// </summary>
        public void Display()
        {
            Function.Call(Hash.SET_TEXT_FONT, 0);
            Function.Call(Hash.SET_TEXT_SCALE, textScale, textScale);
            Function.Call(Hash.SET_TEXT_COLOUR, textR, textG, textB, textA);
            Function.Call(Hash.SET_TEXT_CENTRE, true);

            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, infoText);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, textPosX, textPosY);
        }
    }
}
