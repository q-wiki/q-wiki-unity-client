using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WikidataGame.Models;

public class MinigameMultipleChoice : MonoBehaviour
{

    public List<GameObject> choices;
    public GameObject description;
    
    public Sprite boxSprite;
    public Sprite checkSprite;
    public GameObject warningMessage;
    public GameObject sendButton;

    private GameObject _checkedChoice;
    private string _id;
    private string _taskDescription;
    private IList<string> _answerOptions;

    public async void Initialize(string miniGameId, string taskDescription, IList<string> answerOptions)
    {
        _checkedChoice = null;
        _id = miniGameId;
        _taskDescription = taskDescription;
        _answerOptions = answerOptions;
        AssignDescription(_taskDescription);
        AssignChoices(_answerOptions);

    }

    private void AssignChoices(IList<string> answerOptions)
    {
        for (var i = 0; i < choices.Count; i++)
        {
            var text = choices[i].transform.Find("Text");
            text.GetComponent<Text>().text = answerOptions[i];
        }
    }

    private void AssignDescription(string desc)
    {
        description.GetComponent<Text>().text = desc;
    }

    public void Process(GameObject selected)
    {

        if(_checkedChoice == null)
        {
            Select(selected);
            _checkedChoice = selected;
        } else if(_checkedChoice.Equals(selected))
        {
            Deselect(_checkedChoice);
            _checkedChoice = null;
        }
        else
        {
            Deselect(_checkedChoice);
            Select(selected);
            _checkedChoice = selected;
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

    public async void Send()
    {
        
        if (!Communicator.isConnected)
        {
            Debug.Log("You are not connected to any game");
            return;
        }

        if (_checkedChoice == null)
        {
            sendButton.GetComponent<Button>().interactable = false;
            transform.Find("Layout").GetComponent<CanvasGroup>().blocksRaycasts = false;
            CanvasGroup canvasGroup = warningMessage.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;

        }
        else
        {
            String answer = _checkedChoice.GetComponentInChildren<Text>().text;
            Debug.Log("SEND ANSWER TO BACKEND: " + answer);

            var result = await Communicator.AnswerMinigame(_id, new List<string> {answer});
            Debug.Log(result);
            // TODO: Show result to user ==> continue game
        }
    }


  
}