using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI {
    public class StartUIController : MonoBehaviour, IUIController {
        public Button newGameButton;
        public Button signInAnonButton;
        public Hideable cancelGameButton;
        public Hideable creditsPanel;
        public Hideable legalNoticePanel;
        public Hideable settingsPanel;
        public Hideable startPanel;
        public Hideable usernamePanel;
        public Hideable loginPanel;
        public Hideable qrPanel;

        [SerializeField] private Hideable accountPanel;
        [SerializeField] private Hideable gameRequestPanel;
        [SerializeField] private Text scrollviewText;
        [SerializeField] private GameObject accountHeader;

        [SerializeField] private Button gameViewButton;
        [SerializeField] private Button profileViewButton;
        [SerializeField] private Button gameRequestViewButton;
        [SerializeField] private Button qrViewButton;
        [SerializeField] private Button highscoreViewButton;
        [SerializeField] private Button settingsViewButton;

        [SerializeField] private Color inactiveColor;
        [SerializeField] private Color activeColor;

        private bool _settingsToggle;

        public InputField usernameInput;
        public InputField findUserInput;
        public Text googleAuthButtonText;
        public Text anonAuthButtonText;
        public Text usernameTakenMessage;
        public Text invalidCharactersMessage;
        public Text usernameTooShortMessage;
        public GameObject sidebar;
        private const string USERSEARCH_SCROLLVIEW_TEXT = "Searching for users";
        private const string FRIENDSLIST_SCROLLVIEW_TEXT = "Friends";

        private static GameManager GameManager => GameManager.Instance;

        /// <summary>
        ///     This function is used to initialize a new game.
        /// </summary>
        public async void InitializeGame() {
            // disable all buttons so we don't initialize multiple games
            var startGameText = newGameButton.GetComponentInChildren<Text>().text;
            newGameButton.GetComponentInChildren<Text>().text = "Searching for Opponent...";
            newGameButton.GetComponentInChildren<Text>().fontSize = 56;

            LoadingIndicator.Instance.ShowWithoutBlockingUI();
            newGameButton.enabled = false;

            if (!Communicator.IsConnected()) {
                Debug.Log("You are not connected to any game");
                // reset the interface so we can try initializing a game again
                newGameButton.GetComponentInChildren<Text>().text = startGameText;
                newGameButton.enabled = true;
                return;
            }

            // show button to abort the game initialization
            cancelGameButton.Show();

            var isStartingNewGame = await GameManager.WaitForOpponent(true);

            if (isStartingNewGame) {

                LoadingIndicator.Instance.Hide();

            }
            else {
                // reset the interface so we can try initializing a game again
                newGameButton.GetComponentInChildren<Text>().text = startGameText;
                newGameButton.enabled = true;

                // make the abort button invisible again
                cancelGameButton.Hide();

                LoadingIndicator.Instance.Hide();
            }
        }

        /// <summary>
        ///     Stop searching for an opponent by setting the respective variable false.
        /// </summary>
        public void CancelGameInitialization() {
            GameManager.SetWaitingForOpponent(false);
        }

        /// <summary>
        ///     This function is used to show or hide the settings panel while being in the main menu.
        /// </summary>
        public void ToggleSettings() {
            _settingsToggle = !_settingsToggle;

            if (_settingsToggle) {
                settingsPanel.Show();
                startPanel.Hide();

                creditsPanel.Hide();
                legalNoticePanel.Hide();
                loginPanel.Hide();
                usernamePanel.Hide();
                gameRequestPanel.Hide();
                accountPanel.Hide();
            }
            else {
                settingsPanel.Hide();
                startPanel.Show();

                creditsPanel.Hide();
                legalNoticePanel.Hide();
                loginPanel.Hide();
                usernamePanel.Hide();
                gameRequestPanel.Hide();
                accountPanel.Hide();
            }
        }

        /// <summary>
        ///     This function is used to turn sound on or off.
        /// </summary>
        public void ToggleSound() {
            Debug.LogWarning("This function is not implemented yet.");
        }

        /// <summary>
        ///     This function is used to turn notifications on or off.
        /// </summary>
        public void ToggleNotifications() {
            Debug.LogWarning("This function is not implemented yet.");
        }

        /// <summary>
        ///     This function is used to show the login panel while being in the settings menu.
        /// </summary>

        public void DisplayLoginStart() {
            loginPanel.Show();
            usernamePanel.Hide();
            settingsPanel.Hide();
            startPanel.Hide();
        }

        /// <summary>
        ///     This function is used to show and hide the user profile.
        /// </summary>

        public void ToggleUserProfile() {
            if (accountPanel.IsVisible) {
                accountPanel.Hide();
                startPanel.Show();
            }
            else {
                accountPanel.Show();

                settingsPanel.Hide();
                startPanel.Hide();
                creditsPanel.Hide();
                legalNoticePanel.Hide();
                loginPanel.Hide();
                usernamePanel.Hide();
                gameRequestPanel.Hide();

                DisplayFriendsListUI();
            }
        }

        /// <summary>
        ///     This function is responsable for minor adjustments in the UI when the player searches for Users.
        /// </summary>
        public void DisplayUserSearchUI() {
            scrollviewText.text = USERSEARCH_SCROLLVIEW_TEXT;
            accountHeader.gameObject.SetActive(false);
        }

        /// <summary>
        ///     This function is responsable for minor adjustments in the UI when the player displays his friends list.
        /// </summary>
        public void DisplayFriendsListUI() {
            findUserInput.text = "";
            scrollviewText.text = FRIENDSLIST_SCROLLVIEW_TEXT;
            accountHeader.gameObject.SetActive(true);
        }

        /// <summary>
        ///     This function is used to display the game panel.
        /// </summary>
        public void DisplayGameView() {
            DisplayView(startPanel, gameViewButton);
        }

        /// <summary>
        ///     This function is used to display the profile panel.
        /// </summary>
        public void DisplayProfileView() {
            DisplayView(accountPanel, profileViewButton);
            DisplayFriendsListUI();
        }

        /// <summary>
        ///     This function is used to display the game request panel.
        /// </summary>
        public void DisplayGameRequestView() {
            DisplayView(gameRequestPanel, gameRequestViewButton);
        }

        /// <summary>
        ///     This function is used to display the QR-Challenge panel.
        /// </summary>
        public void DisplayQRView() {
            DisplayView(qrPanel, qrViewButton);
        }

        /// <summary>
        ///     This function is used to display the game Highscore panel.
        /// </summary>
        public void DisplayHighscoreView() {

        }

        public void HighscoreButtonSetActiveState(bool state) {
            highscoreViewButton.interactable = state;
        }

        /// <summary>
        ///     This function is used to display the settings panel.
        /// </summary>
        public void DisplaySettingsView() {
            DisplayView(settingsPanel, settingsViewButton);
        }

        private void DisplayView(Hideable view, Button button) {
            gameRequestPanel.Hide();
            settingsPanel.Hide();
            startPanel.Hide();
            creditsPanel.Hide();
            legalNoticePanel.Hide();
            loginPanel.Hide();
            usernamePanel.Hide();
            accountPanel.Hide();

            view.Show();


            gameViewButton.GetComponent<Image>().color = inactiveColor;
            profileViewButton.GetComponent<Image>().color = inactiveColor;
            gameRequestViewButton.GetComponent<Image>().color = inactiveColor;
            highscoreViewButton.GetComponent<Image>().color = inactiveColor;
            settingsViewButton.GetComponent<Image>().color = inactiveColor;

            button.GetComponent<Image>().color = activeColor;
        }


        /// <summary>
        ///     This function is used to show and hide the game request panel.
        /// </summary>

        public void ToggleGameRequestPanel() {
            if (gameRequestPanel.IsVisible) {
                gameRequestPanel.Hide();
                startPanel.Show();
            }
            else {
                gameRequestPanel.Show();

                settingsPanel.Hide();
                startPanel.Hide();
                creditsPanel.Hide();
                legalNoticePanel.Hide();
                loginPanel.Hide();
                usernamePanel.Hide();
                accountPanel.Hide();
            }
        }

        /// <summary>
        ///     This function is used to open the CreditsPanel in the settings.
        /// </summary>
        public void OpenCreditsPanel() {
            creditsPanel.Show();
            settingsPanel.Hide();
        }

        /// <summary>
        ///     This function is used to open the UsernamePanel from the LoginPanel.
        /// </summary>
        public void OpenUsernamePanel() {
            usernameTakenMessage.gameObject.SetActive(false);
            invalidCharactersMessage.gameObject.SetActive(false);
            usernameTooShortMessage.gameObject.SetActive(false);
            usernamePanel.Show();
            loginPanel.Hide();
        }

        /// <summary>
        ///     This function is used to close the UsernamePanel and return to the LoginPanel.
        /// </summary>
        public void CloseUsernamePanel() {
            usernamePanel.Hide();
            loginPanel.Show();
        }

        /// <summary>
        ///     This function is used to close the LoginPanel and return to the StartPanel.
        /// </summary>
        public void CloseLoginPanel() {
            loginPanel.Hide();
            startPanel.Show();
        }

        /// <summary>
        ///     This function is used to open the LegalNoticePanel in the settings.
        /// </summary>
        public void OpenLegalNoticePanel() {
            legalNoticePanel.Show();
            settingsPanel.Hide();
        }

        /// <summary>
        ///     This function is used to open the privacy policy of the game.
        ///     It is opened in a web browser.
        /// </summary>
        public void OpenPrivacyPolicy() {
            Application.OpenURL("https://wikidatagame.azurewebsites.net/privacy");
        }

        /// <summary>
        /// Indicates if settings are visible to the user.
        /// </summary>
        /// <returns>If settings are visible.</returns>
        public bool AreSettingsVisible() {
            return _settingsToggle;
        }

        /// <summary>
        /// Currently, the game finished function does nothing if called inside the main menu.
        /// </summary>
        /// <param name="index"></param>
        public void HandleGameFinished(short index) {
            Debug.LogWarning("This function is not yet implemented.");
        }
    }

}