using UnityEngine;

namespace DefaultNamespace
{
    public class ScreenshotHandler : Singleton<ScreenshotHandler>
    {
        [SerializeField] private string folder = "Screenshots";
        [SerializeField] private int index;

        /// <summary>
        /// Simple function to take in-game screenshots by pressing 'K'. 
        /// </summary>
        public void Update()
        {
            if (Input.GetKeyUp(KeyCode.K))
            {
                ScreenCapture.CaptureScreenshot($"{folder}/screen_q-wiki_{index}.png", 4);
                index++;
            }
        }
    }
}