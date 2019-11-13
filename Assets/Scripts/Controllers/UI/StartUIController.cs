using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class StartUIController : MonoBehaviour, IUIController
    {
        public Button newGameButton;
        public Button settingsButton;
        public Hideable cancelGameButton;
        public Hideable creditsPanel;
        public Hideable legalNoticePanel;
        public Hideable settingsPanel;
        public Hideable startPanel;

        private bool _settingsToggle;

        private static GameManager GameManager => GameManager.Instance;

        /// <summary>
        ///     This function is used to initialize a new game.
        /// </summary>
        public async void InitializeGame()
        {
            // disable all buttons so we don't initialize multiple games
            var startGameText = newGameButton.GetComponentInChildren<Text>().text;
            newGameButton.GetComponentInChildren<Text>().text = "Searching for Opponent...";
            newGameButton.GetComponentInChildren<Text>().fontSize = 56;

            LoadingIndicator.Instance.ShowWithoutBlockingUI();
            newGameButton.enabled = false;
            
            if (!Communicator.IsConnected())
            {
                Debug.Log("You are not connected to any game");
                // reset the interface so we can try initializing a game again
                newGameButton.GetComponentInChildren<Text>().text = startGameText;
                newGameButton.enabled = true;
                return;
            }
            
            // show button to abort the game initialization
            cancelGameButton.Show();

            // prevent user from hitting settings button (because loading indicator overlays that panel)
            settingsButton.interactable = false;
            
            var isStartingNewGame = await GameManager.WaitForOpponent(true);

            if (isStartingNewGame)
            {
                // settings button needs to be made interactable again
                settingsButton.interactable = true;
                
                LoadingIndicator.Instance.Hide();

            }
            else
            {
                // reset the interface so we can try initializing a game again
                newGameButton.GetComponentInChildren<Text>().text = startGameText;
                newGameButton.enabled = true;

                // make the abort button invisible again
                cancelGameButton.Hide();

                // settings button needs to be made interactable again
                settingsButton.interactable = true;
                
                LoadingIndicator.Instance.Hide();
            }
        }
        
        /// <summary>
        ///     Stop searching for an opponent by setting the respective variable false.
        /// </summary>
        public void CancelGameInitialization()
        {
           GameManager.SetWaitingForOpponent(false);
        }
        
        /// <summary>
        ///     This function is used to show or hide the settings panel while being in the main menu.
        /// </summary>
        public void ToggleSettings()
        {
            _settingsToggle = !_settingsToggle;

            if (_settingsToggle)
            {
                settingsPanel.Show();
                startPanel.Hide();
                creditsPanel.Hide();
                legalNoticePanel.Hide();
            }
            else
            {
                settingsPanel.GetComponent<Hideable>().Hide();
                startPanel.GetComponent<Hideable>().Show();
                creditsPanel.Hide();
                legalNoticePanel.Hide();
            }
        }
        
        /// <summary>
        ///     This function is used to turn sound on or off.
        /// </summary>
        public void ToggleSound()
        {
            Debug.LogWarning("This function is not implemented yet.");
        }

        /// <summary>
        ///     This function is used to turn notifications on or off.
        /// </summary>
        public void ToggleNotifications()
        {
            Debug.LogWarning("This function is not implemented yet.");
        }
        
        /// <summary>
        ///     This function is used to open the CreditsPanel in the settings.
        /// </summary>
        public void OpenCreditsPanel()
        {
            creditsPanel.Show();
            settingsPanel.Hide();
        }

        /// <summary>
        ///     This function is used to open the LegalNoticePanel in the settings.
        /// </summary>
        public void OpenLegalNoticePanel()
        {
           legalNoticePanel.Show();
           settingsPanel.Hide();
        }
        
        /// <summary>
        ///     This function is used to open the privacy policy of the game.
        ///     It is opened in a web browser.
        /// </summary>
        public void OpenPrivacyPolicy()
        {
            Application.OpenURL("https://wikidatagame.azurewebsites.net/privacy");
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
        /// Currently, the game finished function does nothing if called inside the main menu.
        /// </summary>
        /// <param name="index"></param>
        public void HandleGameFinished(short index)
        {
            Debug.LogWarning("This function is not yet implemented.");
        }
    }
    
}