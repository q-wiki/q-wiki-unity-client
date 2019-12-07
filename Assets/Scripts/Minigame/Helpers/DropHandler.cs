using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minigame.Helpers
{
    /// <summary>
    ///     This class is used to handle all drop events.
    /// </summary>
    public class DropHandler : MonoBehaviour, IDropHandler
    {
        /// <summary>
        ///     When a drop event occurs, the text of the event object is pasted on the GameObject.
        /// </summary>
        /// <param name="eventData">Data about the drop event</param>
        public void OnDrop(PointerEventData eventData)
        {
            var item = DragHandler.itemBeingDragged;
            var otherText = item.transform.Find("Text").GetComponent<Text>();
            var thisText = transform.Find("Text").GetComponent<Text>();

            var str = otherText.text;
            otherText.text = thisText.text;
            thisText.text = str;
        }
    }
}