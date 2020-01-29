using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Controllers.Map;
using Controllers.UI.User;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class GameUIController : MonoBehaviour, IUIController
    {
        public Hideable blockActionPanel;
        public Hideable settingsPanel;
        public Hideable settingsContainer;
        public Hideable cancellationPanel;
        public Hideable userPanel;
        public Hideable settingsButton;
        public InteractableIcon userIcon;
        public GameObject gameOverCanvas;
        public Text gameOverText;

        private bool _settingsToggle;

        private readonly IList<string> _states = new List<string>
        {
            "You won! Congratulations!",
            "You Lost! Try again!",
            "Draw!\nTry again!"
        };

        /// <summary>
        /// Functions that are called at the instantiation of the script
        /// </summary>
        public void Start()
        {
            AssignUserIcon();
        }

        /// <summary>
        /// Assign the avatar and name of the current opponent to the user icon / panel
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void AssignUserIcon()
        {
            var gameManager = GameManager.Instance;
            if(gameManager == null)
                throw new Exception("GameManager is not allowed to be null at this (or any) point.");

        }

        /// <summary>
        /// Handle the end of the game for the client
        /// </summary>
        /// <param name="index">An indicator which represents the state of the game when finished.</param>
        public async void HandleGameFinished(short index)
        {

            var interactionController = InteractionController.Instance;
            while (interactionController != null &&
                   interactionController.HasActiveMinigamePanel()) 
                await Task.Delay(1);

            var state = _states[index];
            Debug.Log(state);
            
            settingsButton.Hide();
            gameOverCanvas.SetActive(true);
            gameOverText.text = state;
            gameOverCanvas.transform
                    .Find("GameOverContainer/Layout/Buttons/RematchButton")
                    .GetComponent<Button>().interactable =
                GameManager.Instance.Opponent().Id != "ffffffff-ffff-ffff-ffff-ffffffffffff";
        }

        /// <summary>
        /// Indicates if settings are visible to the user.
        /// </summary>
        /// <returns>If settings are visible.</returns>
        public bool AreSettingsVisible()
        {
            return _settingsToggle;
        }

        /// <summary>
        ///     This function is used to show or hide the settings panel while being in the main menu.
        /// </summary>
        public void ToggleSettings()
        {
            _settingsToggle = !_settingsToggle;
            CameraBehaviour.Instance.Toggle();

            if (_settingsToggle)
            {
                ScoreHandler.Instance.Hide();
                settingsContainer.Show();
                settingsPanel.Show();
            }
            else
            {
                var interactionController = InteractionController.Instance;
                if(interactionController == null || !interactionController.HasActiveMinigamePanel()) 
                    ScoreHandler.Instance.Show();
                
                settingsContainer.Hide();
                settingsPanel.Hide();
            }
        }

        /// <summary>
        ///     This function is used to show the abort game panel to the user.
        /// </summary>
        public void ShowCancellationPanel()
        {
            settingsPanel.Hide();
            cancellationPanel.Show();
        }

        /// <summary>
        /// This function is used to request a rematch with the player you just played against
        /// </summary>
        /// <exception cref="Exception">Game Manager does not exist</exception>
        public async void RequestRematch()
        {
            var gameManager = GameManager.Instance;
            if(gameManager == null)
                throw new Exception("GameManager is not allowed to be null at this (or any) point.");
            
            await gameManager.RequestRematch();
            await gameManager.LeaveGameWithoutConceding();

        }
        
        /// <summary>
        ///     This function is used to go back to the main menu without leaving the current game.
        ///     The game manager is called by cross referencing.
        /// </summary>
        public void GoBackToMenu()
        {
            var gameManager = GameManager.Instance;
            if(gameManager == null)
                throw new Exception("GameManager is not allowed to be null at this (or any) point.");
            
            gameManager.CancelWaiting();
            gameManager.CheckTurnStatusForScoreHandler();
            gameManager.ChangeToStartScene();
        }

        /// <summary>
        ///     This function is used to delete / leave a game.
        ///     The game manager is called by cross referencing.
        /// </summary>
        public async void LeaveGame()
        {
            var gameManager = GameManager.Instance;
            if(gameManager == null)
                throw new Exception("GameManager is not allowed to be null at this (or any) point.");
            
            await GameManager.Instance.LeaveGame();
        }

        /// <summary>
        ///     This function is used to delete / leave a game after it has ended.
        ///     The game manager is called by cross referencing.
        /// </summary>
        public async void LeaveGameWithoutConceding() {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
                throw new Exception("GameManager is not allowed to be null at this (or any) point.");

            await GameManager.Instance.LeaveGameWithoutConceding();
        }

        /// <summary>
        ///     This function is used to stay in-game when the abort game panel is shown.
        /// </summary>
        public void HideCancellationPanel()
        {
            settingsPanel.Show();
            cancellationPanel.Hide();
        }

        /// <summary>
        /// A function to block any interaction by the user.
        /// </summary>
        public void Block()
        {
            blockActionPanel.Show();
        }
        
        /// <summary>
        /// A function to unblock any interaction by the user.
        /// </summary>
        public void Unblock()
        {
            if(blockActionPanel != null) blockActionPanel.Hide();
        }

        /// <summary>
        ///     This function is used to open the privacy policy of the game.
        ///     It is opened in a web browser.
        /// </summary>
        public void OpenPrivacyPolicy()
        {
            Application.OpenURL("https://wikidatagame.azurewebsites.net/privacy");
        }
    }
}