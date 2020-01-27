using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Controllers;
using Handlers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WikidataGame.Models;

namespace Minigame
{
    /// <summary>
    ///     Frontend implementation of the 'Guess the image' MiniGame
    /// </summary>
    public class MinigameGuessImage : MonoBehaviour, IMinigame
    {
        private IList<string> _answerOptions;

        /**
         * private fields
         */
        
        private GameObject _checkedChoice;
        private string _id;
        private string _taskDescription;
        private Timer _timer;
        private Sprite _sprite;
        private ImageInfo _imageInfo;
        private bool _gameOver;


        /**
         * public fields
         */

        public Image imagePlaceholder;
        public Button licenseImageButton;
        public TextMeshProUGUI licensePlaceholder;
        public Sprite boxSprite;
        public Sprite checkSprite;
        public List<GameObject> choices;
        public Sprite closeButtonSprite;
        public GameObject description;
        public Button sendButton;
        public Image sendButtonImage;
        public Sprite sendButtonSprite;
        public Timer timerPrefab;

        private Hideable FeedbackButton => transform.Find("FeedbackButton")
            .GetComponent<Hideable>();

        /// <summary>
        ///     Initialize MiniGame in the frontend
        /// </summary>
        /// <param name="miniGameId">ID of the current MiniGame</param>
        /// <param name="taskDescription">Description of the current MiniGame</param>
        /// <param name="answerOptions">Provided answer options</param>
        /// <param name="difficulty">Provided difficulty</param>
        /// <param name="minigameImage">Provided minigame image</param>
        /// <exception cref="Exception">Timer could not be set properly</exception>
        public async void Initialize(string miniGameId, string taskDescription, IList<string> answerOptions, int difficulty, MinigameImage minigameImage)
        {
            Reset();

            _checkedChoice = null;
            _id = miniGameId;
            _taskDescription = taskDescription;
            _answerOptions = answerOptions;
            _sprite = minigameImage.Sprite;
            _imageInfo = minigameImage.ImageInfo;
            AssignDescription(_taskDescription);
            AssignImage(_sprite);
            AssignImageInfo(_imageInfo);
            AssignChoices(_answerOptions);
            
            /* reset listeners */
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(Submit);

            FeedbackButton.Hide();

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

        }

        /// <summary>
        ///     This is used to force a shutdown of the MiniGame when timer reaches null
        /// </summary>
        public async void ForceQuit()
        {
            
            if(_id == null)
                throw new Exception("Id cannot be null at this point.");

            _gameOver = true;
            
            Debug.Log("Sorry, you were too slow");
            transform.Find("BlockPanel").GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;

            /* reset listeners */
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(Close);
            sendButton.interactable = true;
            sendButton.GetComponent<Image>().color = new Color32(84, 84, 84, 255);
            sendButton.GetComponentInChildren<Text>().text = "Continue";
            sendButtonImage.sprite = closeButtonSprite;

            ReplaceImageNameInLicense();

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
                Submit();
            }
        }

        /// <summary>
        ///     This is used to send an answer option to the backend
        /// </summary>
        /// <exception cref="Exception">No selection was made, therefore the send button should not be clickable.</exception>
        public async void Submit()
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
            
            FeedbackButton.Show();
            ReplaceImageNameInLicense();

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

            
            /* reset listeners */
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(Close);
            sendButton.GetComponent<Image>().color = new Color32(84, 84, 84, 255);
            sendButton.GetComponentInChildren<Text>().text = "Continue";
            sendButtonImage.sprite = closeButtonSprite;
        }

        /// <summary>
        ///     Update is called every frame
        /// </summary>
        public void Update()
        {
            if (!_gameOver && _checkedChoice == null)
                sendButton.GetComponent<Button>().interactable = false;
            else
                sendButton.GetComponent<Button>().interactable = true;
        }

        /// <summary>
        ///     function to reset text colors and sprites before showing the game to user
        /// </summary>
        private void Reset()
        {
            imagePlaceholder.sprite = null;
            sendButton.GetComponent<Image>().color = new Color32(80, 158, 158, 255);
            sendButton.GetComponentInChildren<Text>().text = "Submit";
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
        ///     Assign provided sprite to on-screen placeholder
        /// </summary>
        /// <param name="sprite">Provided sprite</param>
        private void AssignImage(Sprite sprite)
        {
            imagePlaceholder.sprite = sprite;
            imagePlaceholder.preserveAspect = true;
        }
        
        /// <summary>
        ///     Assign provided license to on-screen placeholder
        /// </summary>
        /// <param name="imageInfo">Provided license</param>
        private void AssignImageInfo(ImageInfo imageInfo)
        {
            licenseImageButton.onClick.RemoveAllListeners();
            
            var text = licenseImageButton.transform.Find("Alt")
                .GetComponent<Text>();

            var imageName = imageInfo.Name;
            var author = imageInfo.Artist;
            var licenseName = imageInfo.LicenseName;

            licensePlaceholder.text = GetOwnerOfImage(author) +
                                      "\r\n" +
                                      GetHiddenImageLink(imageName);
            
            var sprite = GetSpriteForLicense(licenseName);

            /* if there is no sprite, use text component to display license */
            if (sprite == null)
            {
                licenseImageButton.GetComponent<Image>().color = new Color(68f, 68f, 68f);
                text.gameObject.SetActive(true);
                text.text = licenseName;
            }
            
            /* if there is a sprite, use it and disable text component */
            else
            {
                licenseImageButton.GetComponent<Image>().color = Color.white;
                licenseImageButton.GetComponent<Image>()
                    .sprite = sprite;
                text.gameObject.SetActive(false);
            }
            
            if (!String.IsNullOrEmpty(imageInfo.LicenseUrl))
            {
                Debug.Log($"Applying link {imageInfo.LicenseUrl} to button");
                licenseImageButton
                    .onClick.AddListener(() => Application.OpenURL(imageInfo.LicenseUrl));
            }
        }
        
        /// <summary>
        /// Replace the hidden name of the image with its original name
        /// </summary>
        private void ReplaceImageNameInLicense()
        {
            licensePlaceholder.text = GetOwnerOfImage(_imageInfo.Artist) +
                                      "\r\n" +
                                      GetNameOfImage(_imageInfo.Name);
        }

        /// <summary>
        /// Returns a sprite for the provided license
        /// </summary>
        /// <param name="str">License as string</param>
        /// <returns>Sprite associated with the according license</returns>
        private Sprite GetSpriteForLicense(string str)
        {

            if (HelperMethods.Contains(str, "Public Domain", StringComparison.OrdinalIgnoreCase))
                return Resources.Load<Sprite>("CC/publicdomain");


            var value = str;
            Debug.Log($"Value of CC: {value}");
            if (HelperMethods.Contains(value, "BY-SA", StringComparison.OrdinalIgnoreCase))
                return Resources.Load<Sprite>("CC/by-sa");
            if (HelperMethods.Contains(value, "BY-ND", StringComparison.OrdinalIgnoreCase))
                return Resources.Load<Sprite>("CC/by-nd");
            if (HelperMethods.Contains(value, "BY", StringComparison.OrdinalIgnoreCase))
                return Resources.Load<Sprite>("CC/by");
            if (HelperMethods.Contains(value, "CC0", StringComparison.OrdinalIgnoreCase))
                return Resources.Load<Sprite>("CC/cc-zero");
            return HelperMethods.Contains(value, "Public Domain", StringComparison.OrdinalIgnoreCase) ? Resources.Load<Sprite>("CC/publicdomain") : null;
        }

        /// <summary>
        /// Returns the owner of the image as TMP string
        /// </summary>
        /// <param name="str">Owner as string</param>
        /// <returns>Owner as TMP string</returns>
        private string GetOwnerOfImage(string str)
        {
            var tuple = LicenseHandler.GetLinkAndValue(str);
            
            if (tuple.Item1 == null)
                return tuple.Item2;

            return $"by <style=NameLink>{str}</style>";
        }
        
        /// <summary>
        /// Returns the name of the image as a hidden TMP variant
        /// </summary>
        /// <param name="str">The name of the image as string</param>
        /// <returns>The name of the image as hidden TMP variant</returns>
        private string GetHiddenImageLink(string str)
        {
            var tuple = LicenseHandler.GetLinkAndValue(str);
            
            if (tuple.Item1 == null)
                return tuple.Item2;

            return $"<style=Link><link=\"{tuple.Item1}\">(show name of image)</link></style>";
        }

        /// <summary>
        /// Returns the name of the image as TMP string
        /// </summary>
        /// <param name="str">The name of the image as string</param>
        /// <returns>The name of the image as TMP string</returns>
        private string GetNameOfImage(string str)
        {
            var tuple = LicenseHandler.GetLinkAndValue(str);
            
            if (tuple.Item1 == null)
                return tuple.Item2;

            return $"<style=NameLink>({str})</style>";
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
        
        /// <summary>
        ///     This is used to send feedback for this mini game to the platform.
        /// </summary>
        public void SendFeedbackToPlatform()
        {
            Communicator.SendFeedbackToPlatform(_id);
        }
    }
}