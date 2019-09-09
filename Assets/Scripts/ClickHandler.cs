using Minigame;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///     This class is used to handle click events in the multiple choice MiniGame.
/// </summary>
public class ClickHandler : MonoBehaviour, IPointerDownHandler
{
    /// <summary>
    ///     When a click is detected, it is processed by the MiniGame.
    /// </summary>
    /// <param name="eventData">Event data about the click event.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        var handler =
            GameObject
                .Find("MinigameMchoiceCanvas")
                .GetComponent<MinigameMultipleChoice>();
        handler.Process(gameObject);
    }
}