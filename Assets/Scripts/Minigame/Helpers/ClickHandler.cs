using UnityEngine;
using UnityEngine.EventSystems;

namespace Minigame.Helpers
{
    /// <summary>
    ///     This class is used to handle click events in the multiple choice and image MiniGame.
    /// </summary>
    public class ClickHandler : MonoBehaviour, IPointerDownHandler
    {
        /// <summary>
        ///     When a click is detected, it is processed by the MiniGame.
        /// </summary>
        /// <param name="eventData">Event data about the click event.</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            var handler = GetComponentInParent<IMinigame>();
            handler.Process(gameObject);
        }
    }
}