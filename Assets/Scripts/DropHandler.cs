using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropHandler : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject item = DragHandler.itemBeingDragged;
        Text otherText = item.transform.Find("Text").GetComponent<Text>();
        Text thisText = transform.Find("Text").GetComponent<Text>();
        
        string str = otherText.text;
        otherText.text = thisText.text;
        thisText.text = str;
    }
}
