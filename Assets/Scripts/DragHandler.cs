using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///     This class is used to handle drag events.
/// </summary>
public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static GameObject itemBeingDragged;
    private Transform startParent;
    private Vector3 startPosition;

    /// <summary>
    ///     When a drag event begins, the start position is used for reference.
    /// </summary>
    /// <param name="eventData">Event data about the drag event</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        itemBeingDragged = gameObject;
        startPosition = transform.position;
        startParent = transform.parent;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    /// <summary>
    ///     When a drag event occurs, the position of the attached object is synchronized with the position of the GameObject.
    /// </summary>
    /// <param name="eventData">Event data about the drag event</param>
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    /// <summary>
    ///     When a drag event ends, the state of the class is reset.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        itemBeingDragged = null;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        transform.position = startPosition;
    }
}