using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameMultipleChoice : MonoBehaviour
{
    public Sprite boxSprite;
    public Sprite checkSprite;
    public GameObject warningMessage;

    public GameObject sendButton;

    private GameObject checkedChoice;

    // Start is called before the first frame update
    void Start()
    {
        checkedChoice = null;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Process(GameObject selected)
    {

        if(checkedChoice == null)
        {
            Select(selected);
            checkedChoice = selected;
        } else if(checkedChoice.Equals(selected))
        {
            Deselect(checkedChoice);
            checkedChoice = null;
        }
        else
        {
            Deselect(checkedChoice);
            Select(selected);
            checkedChoice = selected;
        }
    }

    private void Select(GameObject g)
    {
        g.GetComponentInChildren<Image>().sprite = checkSprite;
    }

    private void Deselect(GameObject g)
    {
        g.GetComponentInChildren<Image>().sprite = boxSprite;
  
    }

    public void Send()
    {
        if (checkedChoice == null)
        {
            sendButton.GetComponent<Button>().interactable = false;
            gameObject.transform.Find("Layout").GetComponent<CanvasGroup>().blocksRaycasts = false;
            CanvasGroup canvasGroup = warningMessage.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;

        }
        else
        {
            String answer = checkedChoice.GetComponentInChildren<Text>().text;
            Debug.Log("SEND ANSWER TO BACKEND: " + answer);

            //TODO: Send to backend
        }
    }
}
