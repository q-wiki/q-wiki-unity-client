using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Minigame
{
    public class MinigameMultipleChoice : MonoBehaviour, IMinigame
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

        public void Initialize(string miniGameId, string taskDescription, IList<string> answerOptions)
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

            if (!Communicator.IsConnected())
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
                Debug.Log("Handling minigame result");
                // hide minigame canvas and return to map
                var chosenAnswer = _checkedChoice.GetComponentInChildren<Text>();

                // TODO: Result contains new game state
                var result = await Communicator.AnswerMinigame(_id, new List<string> {chosenAnswer.text});

                // Check result and display feedback to user
                var correctAnswer = result.CorrectAnswer[0];
                Debug.Log($"Chosen answer: {chosenAnswer.text}, Correct answer: {correctAnswer}");

                var correctAnswerColor = new Color32(0x11, 0xA0, 0x4F, 0xFF);
                if (correctAnswer == chosenAnswer.text)
                {
                    // yay
                    chosenAnswer.color = correctAnswerColor;
                }
                else
                {
                    // nay
                    chosenAnswer.color = Color.red;
                    foreach (var choice in choices)
                    {
                        Text text = choice.GetComponentInChildren<Text>();
                        if (text.text == correctAnswer)
                        {
                            text.color = correctAnswerColor;
                        }
                    }
                }

                await Task.Delay(5000);
                MenuController.instance.RefreshGameState();
                gameObject.SetActive(false);
            }
        }
    }
}
