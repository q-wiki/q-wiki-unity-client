using System;
using UnityEngine;
using UnityEngine.Android;
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
        public Hideable highscorePanel;
        public Hideable dialogPanel;

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

        [SerializeField] private Sprite cancelIcon;
        [SerializeField] private Sprite findGameIcon;

        [SerializeField] private Color inactiveColor;
        [SerializeField] private Color activeColor;
        [SerializeField] private Color abortGameColor;

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

        public void Start()
        {
            newGameButton.onClick.AddListener(delegate { InitializeGame(true); });
        }

        /// <summary>
        ///     This function is used to initialize a new game.
        /// </summary>
        public async void InitializeGame(bool isNewGame) {
            
            
            if (!Communicator.IsConnected()) {
                Debug.Log("You are not connected to any game");
                return;
            }
            
            // disable all buttons so we don't initialize multiple games
            newGameButton.GetComponentInChildren<Text>().text = "";
            newGameButton.transform.Find("Image").GetComponent<Image>().sprite = cancelIcon;
            newGameButton.GetComponentInChildren<Text>().fontSize = 56;
            
            // show button to abort the game initialization
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(CancelGameInitialization);
            newGameButton.GetComponent<Image>().color = abortGameColor;

            LoadingIndicator.Instance.ShowWithoutBlockingUI();


            var isStartingNewGame = await GameManager.WaitForOpponent(isNewGame);

            if (isStartingNewGame) {
                LoadingIndicator.Instance.Hide();
            }
            else {
                // reset the interface so we can try initializing a game again

                newGameButton.GetComponentInChildren<Text>().text = "Find";
                newGameButton.onClick.RemoveAllListeners();
                newGameButton.onClick.AddListener(delegate { InitializeGame(true); });
                newGameButton.GetComponent<Image>().color = inactiveColor;
                newGameButton.transform.Find("Image").GetComponent<Image>().sprite = findGameIcon;

                LoadingIndicator.Instance.Hide();
            }
        }

        /// <summary>
        /// This function is used to initialize a new game with AI opponent.
        /// In-game, it's called a practise match.
        /// </summary>
        public async void InitializeGameWithAiOpponent()
        {
            if (!Communicator.IsConnected()) {
                Debug.Log("You are not connected to any game");
                return;
            }
            
            LoadingIndicator.Instance.Show();
            await GameManager.CreateNewGameWithAIOpponent();
            LoadingIndicator.Instance.Hide();

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
        ///     This function is used to show the login panel.
        /// </summary>

        public void DisplayLoginStart() {
            DisplayView(loginPanel, settingsViewButton);
        }

        /// <summary>
        ///     This function is used to close the dialog box.
        /// </summary>
        public void CloseDialog() {
            dialogPanel.Hide();
        }

        /// <summary>
        ///     This function is used to open a confirmation dialog for a given method.
        /// </summary>
        public void OpenConfirmDialog(string headline, string message, UnityEngine.Events.UnityAction functionOnConfirm) {
            Button button = dialogPanel.transform.Find("MenuGrid/ConfirmButton").GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(functionOnConfirm);
            button.onClick.AddListener(delegate { CloseDialog(); });
            dialogPanel.transform.Find("Headline/Text").GetComponent<Text>().text = headline;
            dialogPanel.transform.Find("Text").GetComponent<Text>().text = message;
            dialogPanel.Show();
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
            DisplayView(highscorePanel, highscoreViewButton);
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
            qrPanel.Hide();
            highscorePanel.Hide();
            
            LoadingIndicator.Instance.Hide();
            if (view.name == "StartPanel" && GameManager.IsWaitingForOpponent())
                LoadingIndicator.Instance.ShowWithoutBlockingUI();
            
            view.Show();


            gameViewButton.GetComponent<Image>().color = inactiveColor;
            profileViewButton.GetComponent<Image>().color = inactiveColor;
            gameRequestViewButton.GetComponent<Image>().color = inactiveColor;
            highscoreViewButton.GetComponent<Image>().color = inactiveColor;
            settingsViewButton.GetComponent<Image>().color = inactiveColor;
            qrViewButton.GetComponent<Image>().color = inactiveColor;

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