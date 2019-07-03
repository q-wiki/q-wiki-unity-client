using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Minigame
{
    public class MinigameSort : MonoBehaviour, IMinigame
    {
        /**
         * public fields
         */

        public List<GameObject> choices;
        public GameObject description;
        public Sprite boxSprite;
        public GameObject sendButton;

        /**
         * private fields
         */

        private string _id;
        private string _taskDescription;
        private IList<string> _answerOptions;

        public void Initialize(string miniGameId, string taskDescription, IList<string> answerOptions)
        {

            Reset();
            
            _id = miniGameId;
            _taskDescription = taskDescription;
            _answerOptions = answerOptions;
            AssignDescription(_taskDescription);
            AssignChoices(_answerOptions);
        }
        
        /**
       * function to reset text colors before showing the game to user
       */
        private void Reset()
        {
            foreach (var item in choices)
            {
                item.GetComponentInChildren<Text>().color = Color.black;
            }
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

        public async void Send()
        {
            if (!Communicator.IsConnected())
            {
                Debug.Log("You are not connected to any game");
                return;
            }

            Debug.Log("Handling minigame result");
            List<string> answers = new List<string>();
            foreach (var choice in choices)
            {
                Text text = choice.GetComponentInChildren<Text>();
                answers.Add(text.text);
            }

            // TODO: Result contains new game state
            var result = await Communicator.AnswerMinigame(_id, answers);

            // Check result and display feedback to user
            var correctAnswer = result.CorrectAnswer;
            Debug.Log($"Chosen answer: {answers}, Correct answer: {correctAnswer}");

            var correctAnswerColor = new Color32(0x11, 0xA0, 0x4F, 0xFF);

            if (answers.SequenceEqual(correctAnswer))
            {
                // sequence is correct
                choices.ForEach(c => c.GetComponentInChildren<Text>().color = correctAnswerColor);
            }
            else
            {
                // sequence has wrong elements => highlight right and wrong elements
                for (var i = 0; i < choices.Count; i++)
                {
                    var c = choices[i];
                    Text text = c.GetComponentInChildren<Text>();
                    if (answers[i] == correctAnswer[i])
                        text.color = correctAnswerColor;
                    else
                        text.color = Color.red;
                }
            }

            await Task.Delay(5000);
            MenuController.instance.RefreshGameState();
            gameObject.SetActive(false);

        }
    }
}
