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
        private Image Image => GetComponentInChildren<Image>();
        
        private Button addButton;
        private Button reportButton;
        
        public Hideable userPanel;

        /// <summary>
        /// AddButton and reportButton are assigned.
        /// </summary>
        public void Awake()
        {
            addButton = userPanel.transform
                .Find("Layout/UserGrid/AddButton")
                .GetComponentInChildren<Button>();

            reportButton = userPanel.transform
                .Find("Layout/ReportUserButton")
                .GetComponentInChildren<Button>();
        }

        /// <summary>
        /// Set avatar for the opponent.
        /// Currently in an update function because I am not really sure how to make sure that avatar is right every time.
        /// </summary>
        private void Update()
        {
            if (GameManager.Opponent() == null) return;
            HelperMethods.SetImage(
                Image,
                GameManager.Opponent().Name);
        }

        /// <summary>
        /// When the icon is clicked, a simple user menu is shown.
        /// </summary>
        /// <param name="eventData"></param>
        public async void OnPointerDown(PointerEventData eventData)
        {
            if (!userPanel.IsVisible)
            {
                var opponent = GameManager.Opponent();
                var userName = userPanel.GetComponentInChildren<Text>();
                var userAvatar = userPanel.transform
                    .Find("Layout/UserGrid/User")
                    .GetComponentInChildren<Image>();
                
                userName.text = opponent.Name;

                addButton.interactable = true;
                
                reportButton.onClick.AddListener(delegate { GameManager.ReportUser(opponent.Id, opponent.Name); });

                var isFriend = await GameManager.IsFriend(opponent.Id);
                if (!isFriend)
                {
                    addButton.GetComponentInChildren<Text>().text = "Add as Friend";
                    addButton.onClick.AddListener(delegate
                    {
                        GameManager.AddFriend(opponent.Id);
                        addButton.interactable = false;
                    });
                }
                else
                {
                    addButton.GetComponentInChildren<Text>().text = "Remove Friend";
                    addButton.onClick.AddListener(delegate
                    {
                        GameManager.DeleteFriend(opponent.Id);
                        addButton.interactable = false;
                    });
                }
                
                HelperMethods.SetImage(userAvatar, opponent.Name);
                userPanel.Show();
            }
            else
            {
                addButton.onClick.RemoveAllListeners();
                userPanel.Hide();
            }
        }
    }
}