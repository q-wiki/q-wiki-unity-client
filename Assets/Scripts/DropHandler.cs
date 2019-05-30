using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropHandler : MonoBehaviour, IDropHandler
{

    #region IDropHandler implementation
    public void OnDrop(PointerEventData eventData)
    {
        GameObject item = DragHandler.itemBeingDragged;
        Image image = item.GetComponentInChildren<Image>();
        gameObject.GetComponentInChildren<Image>().sprite = image.sprite;
        image.sprite = null;
    }
    #endregion

}
