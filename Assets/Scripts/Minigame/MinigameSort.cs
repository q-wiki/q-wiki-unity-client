using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Minigame
{
    public class MinigameSort : MonoBehaviour, Minigame
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
            _id = miniGameId;
            _taskDescription = taskDescription;
            _answerOptions = answerOptions;
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

        public async void Send()
        {
            throw new System.NotImplementedException();
        }
    }
}
