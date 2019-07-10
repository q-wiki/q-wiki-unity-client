using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WikidataGame.Models;

namespace Minigame
{
    public class MinigameMultipleChoice : MonoBehaviour, IMinigame
    {
 
        /**
         * public fields
         */
        
        public GameObject menuController;
        public List<GameObject> choices;
        public GameObject description;
        public Sprite boxSprite;
        public Sprite checkSprite;
        public GameObject sendButton;
        public Timer timerPrefab;
        public Image sendButtonImage;
        public Sprite closeButtonSprite;
        public Sprite sendButtonSprite;

        /**
         * private fields
         */

        private GameObject _checkedChoice;
        private string _id;
        private string _taskDescription;
        private IList<string> _answerOptions;
        private Timer _timer;

        private GameObject ClosePanel => transform.Find("ClosePanel").gameObject;

        public void Update()
        {
            if (_checkedChoice == null)
                sendButton.GetComponent<Button>().interactable = false;
            else
            {
                sendButton.GetComponent<Button>().interactable = true;
            }
        }

        public async void Initialize(string miniGameId, string taskDescription, IList<string> answerOptions, int difficulty)
        {
            Reset();

            _checkedChoice = null;
            _id = miniGameId;
            _taskDescription = taskDescription;
            _answerOptions = answerOptions;
            AssignDescription(_taskDescription);
            AssignChoices(_answerOptions);
            
            /**
             * match difficulty to timer value
             * */

            int milliseconds;
            
            if (difficulty == 0)
                milliseconds = 30000;
            else if (difficulty == 1)
                milliseconds = 20000;
            else if (difficulty == 2)
                milliseconds = 10000;
            else throw new Exception("Difficulty could not be used to determine milliseconds");

            /**
             * instantiate timer and let it count down x milliseconds
             */
            
            _timer = Instantiate(timerPrefab, transform.Find("Layout"));
            _timer.Initialize(this,  milliseconds);
            await _timer.Countdown();
            
        }
        
        /**
       * function to reset text colors and sprites before showing the game to user
       */
        private void Reset()
        {
            sendButton.GetComponent<Image>().color = new Color32(80, 158, 158, 255);
            sendButton.GetComponentInChildren<Text>().text = "Send";
            sendButtonImage.sprite = sendButtonSprite;

            foreach (var item in choices)
            {
                item.GetComponentInChildren<Text>().color = Color.black;
                item.GetComponentInChildren<Image>().sprite = boxSprite;
            }
        }

        private void AssignChoices(IList<string> answerOptions)
        {
            // TODO: Set correct answers
            for (var i = 0; i < choices.Count; i++)
            {
                var text = choices[i].transform.Find("Text");
                text.GetComponent<Text>().text = answerOptions[i];
            }
        }

        private void AssignDescription(string desc)
        {
            // TODO: Set correct description??
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

        public void Close()
        {
            menuController.GetComponent<MenuController>().RefreshGameState(false);
            menuController.GetComponent<MenuController>().ToggleCameraBehaviour();
            gameObject.SetActive(false);
            ClosePanel.SetActive(false);
        }

        public async void ForceQuit()
        {
            Debug.Log("Sorry, you were too slow");
            var result = await Communicator.AnswerMinigame(_id, new List<string> {});
            ClosePanel.SetActive(true);
        }

        public async void Send()
        {
            if (_checkedChoice == null)
                throw new Exception("It should not be possible to press the send button.");
            
            _timer.isInterrupted = true;
            Destroy(_timer.gameObject);

            Debug.Log("Handling minigame result");
            // hide minigame canvas and return to map
            var chosenAnswer = _checkedChoice.GetComponentInChildren<Text>();
            
            LoadingIndicator.Instance.Show();
            var result = await Communicator.AnswerMinigame(_id, new List<string> {chosenAnswer.text});
            LoadingIndicator.Instance.Hide();

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
                    GameObject text = choice
                        .transform
                        .Find("Text")
                        .gameObject;
                    if (text.GetComponent<Text>().text == correctAnswer)
                    {
                        text.GetComponent<Text>().color = correctAnswerColor;
                    }
                }
            }

            ClosePanel.SetActive(true);
            sendButton.GetComponent<Image>().color = new Color32(195, 98, 98, 255);
            sendButton.GetComponentInChildren<Text>().text = "Close";
            sendButtonImage.sprite = closeButtonSprite;
        }
    }
}
