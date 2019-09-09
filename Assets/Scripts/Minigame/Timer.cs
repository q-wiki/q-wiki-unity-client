using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Minigame
{
    /// <summary>
    ///     Timer implementation to process difficulties
    /// </summary>
    public class Timer : MonoBehaviour
    {
        private readonly float x = 0.0f;
        private readonly float y = 289.0f;

        /**
         * private fields
         **/

        private int _milliseconds;

        private IMinigame _minigameInstance;
        /**
         * public fields
         **/

        public bool isInterrupted;

        /// <summary>
        ///     Start is called when timer is instantiated
        /// </summary>
        public void Start()
        {
            transform.localPosition = new Vector3(x, y);
        }

        /// <summary>
        ///     Use this to set relevant values for the timer
        /// </summary>
        /// <param name="miniGame">Current MiniGame</param>
        /// <param name="milliseconds">Maximum value of milliseconds to use</param>
        public void Initialize(IMinigame miniGame, int milliseconds)
        {
            _minigameInstance = miniGame;

            var slider = GetComponent<Slider>();
            slider.maxValue = milliseconds;
            slider.value = milliseconds;
        }

        /// <summary>
        ///     This is used to simulate a timer countdown
        /// </summary>
        /// <returns></returns>
        public async Task Countdown()
        {
            var slider = GetComponent<Slider>();
            while (!isInterrupted && slider.value > 0)
            {
                slider.value -= 50;
                await Task.Delay(50);
            }

            if (!isInterrupted)
            {
                _minigameInstance.ForceQuit();
                Destroy(gameObject);
            }
        }
    }
}