using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickHandler : MonoBehaviour, IPointerDownHandler
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MinigameMultipleChoice handler = 
        GameObject
            .Find("MinigameMchoiceCanvas")
            .GetComponent<MinigameMultipleChoice>();
        handler.Process(gameObject);
    }
}
