using System;
using Controllers.Map;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WikidataGame.Models;

namespace Controllers.UI.User
{
    public class InteractableIcon : MonoBehaviour, IPointerDownHandler
    {
        private static GameManager GameManager => GameManager.Instance;
        private Button button;
        
        public Hideable userPanel;

        public void Start()
        {
            button = userPanel.GetComponentInChildren<Button>();
        }

        public async void OnPointerDown(PointerEventData eventData)
        {
            if (!userPanel.IsVisible)
            {
                var opponent = GameManager.Opponent();
                var userName = userPanel.GetComponentInChildren<Text>();
                userName.text = opponent.Name;

                button.interactable = true;

                var isFriend = await GameManager.IsFriend(opponent.Id);
                if (!isFriend)
                {
                    button.GetComponentInChildren<Text>().text = "Add as Friend";
                    button.onClick.AddListener(delegate
                    {
                        GameManager.AddFriend(opponent.Id);
                        button.interactable = false;
                    });
                }
                else
                {
                    button.GetComponentInChildren<Text>().text = "Remove Friend";
                    button.onClick.AddListener(delegate
                    {
                        GameManager.DeleteFriend(opponent.Id);
                        button.interactable = false;
                    });
                }

                // TODO: Image assign
                
                userPanel.Show();
            }
            else
            {
                button.onClick.RemoveAllListeners();
                userPanel.Hide();
            }
        }
    }
}