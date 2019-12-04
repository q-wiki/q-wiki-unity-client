using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minigame
{
    /// <summary>
    ///     Frontend implementation of the sorting MiniGame
    /// </summary>
    public class MinigameSort : MonoBehaviour, IMinigame
    {
        private IList<string> _answerOptions;

        /**
         * private fields
         */

        private string _id;
        private string _taskDescription;
        private Timer _timer;

        public Sprite boxSprite;
        /**
         * public fields
         */

        public List<GameObject> choices;
        public List<Sprite> numbersSprites;
        public List<Sprite> numbersSpritesCorrect;
        public Sprite unselectedSprite;
        public Sprite closeButtonSprite;
        public GameObject description;
        public Button sendButton;
        public Image sendButtonImage;
        public Sprite sendButtonSprite;
        public Timer timerPrefab;
        private List<GameObject> sortedChoices = new List<GameObject>();
        private GameObject ClosePanel => transform.Find("ClosePanel").gameObject;

        /// <summary>
        ///     Initialize MiniGame in the frontend
        /// </summary>
        /// <param name="miniGameId">ID of the current MiniGame</param>
        /// <param name="taskDescription">Description of the current MiniGame</param>
        /// <param name="answerOptions">Provided answer options</param>
        /// <param name="difficulty">Provided difficulty</param>
        /// <exception cref="Exception">Timer could not be set properly</exception>
        public async void Initialize(string miniGameId, string taskDescription, IList<string> answerOptions,
            int difficulty)
        {
            Reset();

            _id = miniGameId;
            _taskDescription = taskDescription;
            _answerOptions = answerOptions;
            AssignDescription(_taskDescription);
            AssignChoices(_answerOptions);

            /**
             * match difficulty to timer value
             */

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
            _timer.Initialize(this, milliseconds);
            await _timer.Countdown();
        }

        /// <summary>
        ///     Visual representation of selected choice, when a new choice is clicked.
        ///     Updating sorted list to relfect changes.
        /// </summary>
        public void ChoiceSortOnClick()
        {
            GameObject clickedChoice = EventSystem.current.currentSelectedGameObject;  

            if (sortedChoices.Contains(clickedChoice))
            {
                clickedChoice.transform.GetChild(0).GetComponent<Image>().sprite = null;
                sortedChoices.Remove(clickedChoice);
                clickedChoice.transform.GetChild(0).GetComponent<Image>().sprite = unselectedSprite;

                foreach (GameObject choice in sortedChoices)
                {
                    choice.transform.GetChild(0).GetComponent<Image>().sprite = numbersSprites[sortedChoices.IndexOf(choice)];
                }
            }
            else
            {
                sortedChoices.Add(clickedChoice);
                clickedChoice.transform.GetChild(0).GetComponent<Image>().sprite = numbersSprites[sortedChoices.IndexOf(clickedChoice)];
            }

        }

        /// <summary>
        ///     This is used to force a shutdown of the MiniGame when timer reaches null
        /// </summary>
        public async void ForceQuit()
        {
            Debug.Log("Sorry, you were too slow");
            transform.Find("BlockPanel").GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;
            sendButton.GetComponent<Image>().color = new Color32(195, 98, 98, 255);
            sendButton.GetComponentInChildren<Text>().text = "Close Minigame";
            sendButtonImage.sprite = closeButtonSprite;
            Send();
        }

        /// <summary>
        ///     This is used to send answer options to the backend
        /// </summary>
        public async void Send()
        {
            if (!Communicator.IsConnected())
            {
                Debug.Log("You are not connected to any game");
                return;
            }

            _timer.isInterrupted = true;
            Destroy(_timer.gameObject);

            Debug.Log("Handling minigame result");
            var answers = new List<string>();
            // if not all answers selected : fill sorted list with missing answers
            if (sortedChoices.Count < 4)
            {
                foreach(var choice in choices)
                {
                    if (!sortedChoices.Contains(choice))
                    {
                        sortedChoices.Add(choice);
                    }
                }
            }
            foreach (var choice in sortedChoices)
            {
                var text = choice.GetComponentInChildren<Text>();
                answers.Add(text.text);
            }

            LoadingIndicator.Instance.Show();
            var result = await Communicator.AnswerMinigame(_id, answers);
            LoadingIndicator.Instance.Hide();

            /**
             * use block panel to block further interaction by user
             */

            transform.Find("BlockPanel").GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;

            // Check result and display feedback to user
            var correctAnswer = result.CorrectAnswer;
            Debug.Log($"Chosen answer: {answers}, Correct answer: {correctAnswer}");

            var correctAnswerColor = new Color32(0x11, 0xA0, 0x4F, 0xFF);

            if (answers.SequenceEqual(correctAnswer))
                // sequence is correct
                choices.ForEach(c => c.GetComponentInChildren<Text>().color = correctAnswerColor);
            else
                // sequence has wrong elements => highlight right and wrong elements
                for (var i = 0; i < sortedChoices.Count; i++)
                {
                    var c = sortedChoices[i];
                    var text = c.GetComponentInChildren<Text>();
                    int index = correctAnswer.IndexOf(text.text);
                    var child = c.transform.GetChild(2).gameObject;
                    child.SetActive(true);
                    child.GetComponent<Image>().sprite = numbersSpritesCorrect[correctAnswer.IndexOf(text.text)];

                    if (text.text == correctAnswer[i])
                    {
                        text.color = correctAnswerColor;
                    }
                    else
                    {
                        text.color = Color.red;
                    }
                }

            ClosePanel.SetActive(true);
            sendButton.GetComponent<Image>().color = new Color32(195, 98, 98, 255);
            sendButton.GetComponentInChildren<Text>().text = "Close Minigame";
            sendButtonImage.sprite = closeButtonSprite;
        }

        /// <summary>
        ///     This is used to close the MiniGame
        /// </summary>
        public void Close()
        {
            GameManager.Instance.RefreshGameState(false);
            CameraBehaviour.Instance.Toggle();
            
            transform.Find("BlockPanel").GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;
            gameObject.SetActive(false);
            foreach(var c in choices)
            {
                c.transform.GetChild(2).gameObject.SetActive(false);
            }
            ClosePanel.SetActive(false);
        }

        /// <summary>
        ///     function to reset text colors and sprites before showing the game to user
        /// </summary>
        private void Reset()
        {
            sendButton.GetComponent<Image>().color = new Color32(80, 158, 158, 255);
            sendButton.GetComponentInChildren<Text>().text = "Send";
            sendButtonImage.sprite = sendButtonSprite;
            sortedChoices.Clear();

            foreach (var item in choices)

            {
                item.transform.GetChild(0).GetComponent<Image>().sprite = null;
                item.transform.GetChild(0).GetComponent<Image>().sprite = unselectedSprite;
                item.GetComponentInChildren<Text>().color = Color.white;
            }

        }

        /// <summary>
        ///     Assign provided answer options to on-screen placeholders
        /// </summary>
        /// <param name="answerOptions">Provided answer options</param>
        private void AssignChoices(IList<string> answerOptions)
        {
            for (var i = 0; i < choices.Count; i++)
            {
                var text = choices[i].transform.Find("Text");
                text.GetComponent<Text>().text = answerOptions[i];
            }
        }

        /// <summary>
        ///     Assign provided description to on-screen placeholder
        /// </summary>
        /// <param name="desc">Provided description</param>
        private void AssignDescription(string desc)
        {
            description.GetComponent<Text>().text = desc;
        }
    }
}