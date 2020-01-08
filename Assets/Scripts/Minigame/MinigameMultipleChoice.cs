using System;
using System.Collections.Generic;
using Controllers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Minigame
{
    /// <summary>
    ///     Frontend implementation of the multiple choice MiniGame
    /// </summary>
    public class MinigameMultipleChoice : MonoBehaviour, IMinigame
    {
        private IList<string> _answerOptions;

        /**
         * private fields
         */

        private GameObject _checkedChoice;
        private string _id;
        private string _taskDescription;
        private Timer _timer;
        public Sprite boxSprite;
        public Sprite checkSprite;
        public List<GameObject> choices;
        public Sprite closeButtonSprite;
        public GameObject description;

        /**
         * public fields
         */
        
        public GameObject sendButton;
        public Image sendButtonImage;
        public Sprite sendButtonSprite;
        public Timer timerPrefab;

        private GameObject ClosePanel => transform.Find("ClosePanel").gameObject;

        /// <summary>
        ///     Initialize MiniGame in the frontend
        /// </summary>
        /// <param name="miniGameId">ID of the current MiniGame</param>
        /// <param name="taskDescription">Description of the current MiniGame</param>
        /// <param name="answerOptions">Provided answer options</param>
        /// <param name="difficulty">Provided difficulty</param>
        /// <param name="sprite">Sprite (not used here)</param>
        /// <exception cref="Exception">Timer could not be set properly</exception>
        public async void Initialize(string miniGameId, string taskDescription, IList<string> answerOptions,
            int difficulty, Sprite sprite = null)
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
            _timer.Initialize(this, milliseconds);
            await _timer.Countdown();
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
            ClosePanel.SetActive(false);
        }

        /// <summary>
        ///     This is used to force a shutdown of the MiniGame when timer reaches null
        /// </summary>
        public async void ForceQuit()
        {
            
            if(_id == null)
                throw new Exception("Id cannot be null at this point.");
            
            Debug.Log("Sorry, you were too slow");
            transform.Find("BlockPanel").GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;

            sendButton.GetComponent<Image>().color = new Color32(195, 98, 98, 255);
            sendButton.GetComponentInChildren<Text>().text = "Close Minigame";
            sendButtonImage.sprite = closeButtonSprite;

            if (_checkedChoice == null)
            {
                var result = await Communicator.AnswerMinigame(_id, new List<string>());
                var correctAnswer = result.CorrectAnswer[0];
                var correctAnswerColor = new Color32(0x11, 0xA0, 0x4F, 0xFF);

                foreach (var choice in choices)
                {
                    var text = choice
                        .transform
                        .Find("Text")
                        .gameObject;
                    if (text.GetComponent<Text>().text == correctAnswer)
                        text.GetComponent<Text>().color = correctAnswerColor;
                }


            }
            else
            {
                Send();
            }
        }

        /// <summary>
        ///     This is used to send an answer option to the backend
        /// </summary>
        /// <exception cref="Exception">No selection was made, therefore the send button should not be clickable.</exception>
        public async void Send()
        {
            if (_checkedChoice == null)
                throw new Exception("It should not be possible to press the send button.");
            
            if(_id == null)
                throw new Exception("Id cannot be null at this point.");

            _timer.isInterrupted = true;
            Destroy(_timer.gameObject);

            Debug.Log("Handling minigame result");
            // hide minigame canvas and return to map
            var chosenAnswer = _checkedChoice.GetComponentInChildren<Text>();

            LoadingIndicator.Instance.Show();
            var result = await Communicator.AnswerMinigame(_id, new List<string> {chosenAnswer.text});
            LoadingIndicator.Instance.Hide();

            /**
             * use block panel to block further interaction by user
             */

            transform.Find("BlockPanel").GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;

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
                    var text = choice
                        .transform
                        .Find("Text")
                        .gameObject;
                    if (text.GetComponent<Text>().text == correctAnswer)
                        text.GetComponent<Text>().color = correctAnswerColor;
                }
            }

            ClosePanel.SetActive(true);
            sendButton.GetComponent<Image>().color = new Color32(195, 98, 98, 255);
            sendButton.GetComponentInChildren<Text>().text = "Close Minigame";
            sendButtonImage.sprite = closeButtonSprite;
        }

        /// <summary>
        ///     Update is called every frame
        /// </summary>
        public void Update()
        {
            if (_checkedChoice == null)
                sendButton.GetComponent<Button>().interactable = false;
            else
                sendButton.GetComponent<Button>().interactable = true;
        }

        /// <summary>
        ///     function to reset text colors and sprites before showing the game to user
        /// </summary>
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

        /// <summary>
        ///     Process selection of an answer option by the user
        /// </summary>
        /// <param name="selected">Selected answer option</param>
        public void Process(GameObject selected)
        {
            if (_checkedChoice == null)
            {
                Select(selected);
                _checkedChoice = selected;
            }
            else if (_checkedChoice.Equals(selected))
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

        /// <summary>
        ///     This is used to visually select an answer option
        /// </summary>
        /// <param name="g">GameObject that should be selected on-screen</param>
        private void Select(GameObject g)
        {
            g.GetComponentInChildren<Image>().sprite = checkSprite;
        }

        /// <summary>
        ///     This is used to visually deselect an answer option
        /// </summary>
        /// <param name="g">GameObject that should be deselected on-screen</param>
        private void Deselect(GameObject g)
        {
            g.GetComponentInChildren<Image>().sprite = boxSprite;
        }
    }
}