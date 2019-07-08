using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Minigame
{
    public class Timer : MonoBehaviour
    {
        /**
         * public fields
         **/
    
        public bool isInterrupted;

        /**
         * private fields
         **/

        private int _milliseconds;
        private IMinigame _minigameInstance;
        private readonly float x = 0.0f;
        private readonly float y = -155.0f;
    
        public void Start()
        {
            transform.localPosition = new Vector3(x, y);
        }

        public void Initialize(IMinigame miniGame, int milliseconds)
        {
            _minigameInstance = miniGame;
        
            Slider slider = GetComponent<Slider>();
            slider.maxValue = milliseconds;
            slider.value = milliseconds;
        }

        public async Task Countdown()
        {
            Slider slider = GetComponent<Slider>();
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
