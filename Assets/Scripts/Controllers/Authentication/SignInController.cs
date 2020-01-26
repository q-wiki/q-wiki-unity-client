using System;
using System.Threading.Tasks;
using Controllers.UI;
using Controllers.UI.User;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.UI;
using WikidataGame.Models;

namespace Controllers.Authentication {
    /// <summary>
    /// The SignIn Controller handles Firebase and Google Play authentication for the app.
    /// </summary>
    public class SignInController : MonoBehaviour {

        public static bool forceLogin;
        public static AuthInfo authInfo { get; set; }
        public const string METHOD_GOOGLE = "Google";
        public const string METHOD_ANONYMOUS = "Anonymous";
        public const string METHOD_NONE = "Not logged in";

        private static bool isLoggedInGoogle;
        public bool IsLoggedInGoogle {
            get { return isLoggedInGoogle; }
            set {
                isLoggedInGoogle = value;
                _uiController.HighscoreButtonSetActiveState(value);
            }
        }
        private static bool isLoggedInAnon;
        public bool IsLoggedInAnon {
            get { return isLoggedInAnon; }
            set {
                isLoggedInAnon = value;
                _uiController.HighscoreButtonSetActiveState(false);
            }
        }
        public static bool reauthenticateWithGoogle = false;
        [SerializeField] private Image avatarPreview;

        public static bool isLoggedIn {
            get { return isLoggedInAnon || isLoggedInGoogle; }
        }

        /* private fields */
        private StartUIController _uiController;

        /* private constants */
        private const string SIGNED_IN_TEXT_GOOGLE = "Sign Out of Google Play";
        private const string SIGNED_OUT_TEXT_GOOGLE = "Sign In With Google";
        private const string SIGNED_IN_TEXT_ANON = "Change Username";
        private const string SIGNED_OUT_TEXT_ANON = "Sign in Anonymously";
        private const string PLAYERPREFS_PASSWORD = "PASSWORD";
        public const string PLAYERPREFS_USERNAME = "USERNAME";

        /// <summary>
        /// Start is called before the first frame update.
        /// When the SignInController is initialized, the play games API is constructed.
        /// </summary>
        void Start() {
            
            _uiController = GameManager.Instance.UIController() as StartUIController;
            if (_uiController == null)
                throw new Exception("Start UI Controller is not allowed to be null");
            
            var config = new PlayGamesClientConfiguration.Builder()
                .RequestServerAuthCode(true)
                .Build();
            PlayGamesPlatform.InitializeInstance(config);

            // recommended for debugging:
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();

            IsLoggedInGoogle = PlayGamesPlatform.Instance.IsAuthenticated();
            _uiController.googleAuthButtonText.text = IsLoggedInGoogle ? SIGNED_IN_TEXT_GOOGLE : SIGNED_OUT_TEXT_GOOGLE;
            _uiController.anonAuthButtonText.text = IsLoggedInAnon ? SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;

            IsLoggedInAnon = (!string.IsNullOrEmpty(PlayerPrefs.GetString(PLAYERPREFS_USERNAME)) && !IsLoggedInGoogle );

            _uiController.signInAnonButton.gameObject.SetActive(!isLoggedIn);
            _uiController.sidebar.SetActive(isLoggedIn);

            _uiController.usernameInput.onValueChanged.RemoveAllListeners();
            _uiController.usernameInput.onValueChanged.AddListener(delegate { inputChangedCallBack(); });
            _uiController.usernameInput.onEndEdit.AddListener(delegate { inputSubmitCallBack(); });
        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>

        private void Update() {
            if (PushHandler.Instance.isUsable) {
                if (!isLoggedIn && !(_uiController.loginPanel.IsVisible || _uiController.usernamePanel.IsVisible)) {
                    forceLogin = true;
                }

                if (forceLogin) {
                    ForceLogin();
                    forceLogin = false;
                }

            }
            if (reauthenticateWithGoogle) {
                reauthenticateWithGoogle = false;
                SignIn();
            }
        }

        /// <summary>
        /// Forcing user to login.
        /// </summary>
        private void ForceLogin() {
            Debug.Log("User is asked to choose a SignIn Method");

            _uiController.googleAuthButtonText.text = (IsLoggedInGoogle) ? SIGNED_IN_TEXT_GOOGLE : SIGNED_OUT_TEXT_GOOGLE;
            _uiController.signInAnonButton.gameObject.SetActive(true);
            _uiController.sidebar.SetActive(false);

            _uiController.DisplayLoginStart();
        }


        /// <summary>
        /// Sign in client anonymously.
        /// </summary>
        public void SignInAnonymously() {

            _uiController.OpenUsernamePanel();
        }


        void OnEnable() {
            //Register InputField Events
            if (_uiController == null){
                return;
            }
            _uiController.usernameInput.onValueChanged.AddListener(delegate { inputChangedCallBack(); });
            _uiController.usernameInput.onEndEdit.AddListener(delegate { inputSubmitCallBack(); });
        }

        /// <summary>
        /// While the user chooses his username, display a preview for the generated user avatar
        /// </summary>
        private void inputChangedCallBack() {
            if (_uiController.usernameInput.text.Length > 2){
                HelperMethods.SetImage(avatarPreview, "anon-" + _uiController.usernameInput.text);
            }
        }

        void OnDisable() {
            //Un-Register InputField Events
            //_uiController.findUserInput.onEndEdit.RemoveAllListeners();

            if (_uiController != null) {
                _uiController.usernameInput.onValueChanged.RemoveAllListeners();
                _uiController.usernameInput.onEndEdit.RemoveAllListeners();
            }
        }


        /// <summary>
        /// If the users ends the input field edit with Enter, start the authentication process
        /// </summary>
        private void inputSubmitCallBack() {
            if (Input.GetButtonDown("Submit")) {
                AuthenticateWithUsername();
            }
        }

        /// <summary>
        /// Authenticate anonymously with the submitted username
        /// </summary>
        public async void AuthenticateWithUsername() {
            _uiController.usernameTakenMessage.gameObject.SetActive(false);
            _uiController.invalidCharactersMessage.gameObject.SetActive(false);
            _uiController.usernameTooShortMessage.gameObject.SetActive(false);
            string newUserName = (_uiController.usernameInput.text == "") ? "Anonymous User" : _uiController.usernameInput.text;

            //If the user tries to log in with it's previously used username, use the stored password
            string password = (PlayerPrefs.GetString(PLAYERPREFS_USERNAME) != null && newUserName == PlayerPrefs.GetString(PLAYERPREFS_USERNAME)) ?
                PlayerPrefs.GetString(PLAYERPREFS_PASSWORD) : HelperMethods.CreatePassword(32);
            string pushToken = PushHandler.Instance.pushToken ?? "";

            //Tries to authenticate by either logging in with the stored credentials or by trying to create a new user
            Task<string> authenticate = Communicator.Authenticate(newUserName, password, pushToken, METHOD_ANONYMOUS);
            string response = await authenticate;

            Debug.Log(response);

            if (response == Communicator.USERNAME_TAKEN_ERROR_MESSAGE) {
                _uiController.usernameTakenMessage.gameObject.SetActive(true);
            }
            else if (response == Communicator.INVALID_CHARACTERS_ERROR_MESSAGE) {
                _uiController.invalidCharactersMessage.gameObject.SetActive(true);
            }
            else if (response == Communicator.USERNAME_TOO_SHORT_MESSAGE){
                _uiController.usernameTooShortMessage.gameObject.SetActive(true);
            }
            else if (response == null) {
                throw new Exception("An unknown error occurred. Response was null.");
            }
            else{
                string authToken = response;
                PlayerPrefs.SetString(Communicator.PLAYERPREFS_AUTH_TOKEN, authToken);
                PlayerPrefs.SetString(PLAYERPREFS_USERNAME, newUserName);
                PlayerPrefs.SetString(PLAYERPREFS_PASSWORD, password);
                PlayerPrefs.SetString(Communicator.PLAYERPREFS_SIGNIN_METHOD, METHOD_ANONYMOUS);
                IsLoggedInAnon = true;
                await Communicator.SetupApiConnection();
                _uiController.CloseUsernamePanel();
                _uiController.CloseLoginPanel();
                GameObject.Find("AccountController").GetComponent<AccountController>().InitialSetup();
                Debug.Log($"Auth token received: {authToken}");
                Debug.Log($"Avatar: {authInfo.User.ProfileImage}");
                _uiController.anonAuthButtonText.text = (IsLoggedInAnon) ?
                    SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;
                _uiController.signInAnonButton.gameObject.SetActive(false);
                _uiController.sidebar.SetActive(true);
                _uiController.DisplayGameView();
                _uiController.HighscoreButtonSetActiveState(false);
            }

        }

        /// <summary>
        /// Use this to sign in a user.
        /// </summary>
        public void SignIn() {
            Debug.Log("Google Play: Authentication Status: " + PlayGamesPlatform.Instance.IsAuthenticated());
            if (!PlayGamesPlatform.Instance.IsAuthenticated()) {
                // authenticate user:

                Social.localUser.Authenticate((bool success) => {
                    Debug.Log("UDEBUG: Authentication success: " + success);
                    Debug.Log("UDEBUG: Username: " + Social.localUser.userName);
                    Debug.Log("UDEBUG: ID: " + Social.localUser.id);

                    _uiController.googleAuthButtonText.text = success ? SIGNED_IN_TEXT_GOOGLE : SIGNED_OUT_TEXT_GOOGLE;

                    // handle success or failure
                    if (success) {
                        OnSuccess();
                    }
                    else {
                        ForceLogin();
                    }
                });
            }
            else {
                SignOut();
            }

        }

        /// <summary>
        /// This is called when Google Play sign-in was successful.
        /// </summary>
        private async void OnSuccess() {
            Debug.Log("Successfully Signed In");
            SignOutAnon();
            Debug.Log("UDEBUG: AuthCode: " + PlayGamesPlatform.Instance.GetServerAuthCode());
            PlayGamesPlatform.Instance.GetAnotherServerAuthCode(true, (string code) => {
                Debug.Log("UDEBUG: AuthCodeAsync: " + code);
            });
            Debug.Log("UDEBUG: ID Token: " + PlayGamesPlatform.Instance.GetIdToken());

            string userName = Social.localUser.userName;
            //string authCode = string.IsNullOrEmpty(PlayerPrefs.GetString(Communicator.PLAYERPREFS_PASSWORD)) ? PlayGamesPlatform.Instance.GetServerAuthCode() : PlayerPrefs.GetString(Communicator.PLAYERPREFS_PASSWORD);
            string authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
            PlayGamesPlatform.Instance.GetAnotherServerAuthCode(true, (string code) => {
                authCode = code;
            });
            string pushToken = PushHandler.Instance.pushToken ?? "";

            Task<string> authenticate = Communicator.Authenticate(userName, authCode, pushToken, METHOD_GOOGLE);
            string response = await authenticate;
            if (response == Communicator.USERNAME_TAKEN_ERROR_MESSAGE) {
                /**
                 * Since GooglePlay Usernames are unique (and can't contain special characters), and anonymous users get the 'anon-' prefix, this should never happen
                 */
                SignOut();
                Debug.LogError("Google Username is already taken.");
            }
            else if (response == Communicator.INVALID_CHARACTERS_ERROR_MESSAGE) {
                /**
                 * Since GooglePlay Usernames can't contain special characters, this should never happen
                 */
                SignOut();
                Debug.LogError("Username contains invalid characters");
            }
            else if(!string.IsNullOrEmpty(response)) {
                string authToken = response;
                PlayerPrefs.SetString(Communicator.PLAYERPREFS_AUTH_TOKEN, authToken);
                PlayerPrefs.SetString(PLAYERPREFS_USERNAME, userName);
                PlayerPrefs.SetString(PLAYERPREFS_PASSWORD, authCode);
                PlayerPrefs.SetString(Communicator.PLAYERPREFS_SIGNIN_METHOD, METHOD_GOOGLE);
                IsLoggedInGoogle = true;
                await Communicator.SetupApiConnection();
                _uiController.CloseUsernamePanel();
                _uiController.CloseLoginPanel();
                GameObject.Find("AccountController").GetComponent<AccountController>().InitialSetup();
                Debug.Log($"Auth token received: {authToken}");
                _uiController.googleAuthButtonText.text = (IsLoggedInGoogle) ?
                    SIGNED_IN_TEXT_GOOGLE : SIGNED_OUT_TEXT_GOOGLE;
                _uiController.signInAnonButton.gameObject.SetActive(false);
                _uiController.sidebar.SetActive(true);
                _uiController.DisplayGameView();
                _uiController.HighscoreButtonSetActiveState(true);
            }
            else {
                SignOut();
                throw new Exception("An unknown error occurred");
            }
        }

        /// <summary>
        /// Use this to sign out a user.
        /// </summary>
        private void SignOut() {
            // sign out
            Debug.Log("Signing out of Google Play");
            PlayGamesPlatform.Instance.SignOut();
            PlayerPrefs.SetString(Communicator.PLAYERPREFS_SIGNIN_METHOD, METHOD_NONE);
            IsLoggedInGoogle = false;
            ForceLogin();

        }

        /// <summary>
        /// Use this to sign out an anonymous user. This is usually only called, if an anonymously authenticated user decides to switch to a Google account
        /// </summary>
        public void SignOutAnon() {
            // sign out
            PlayerPrefs.SetString(Communicator.PLAYERPREFS_SIGNIN_METHOD, METHOD_NONE);
            _uiController.anonAuthButtonText.text = (IsLoggedInAnon) ? SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;
            _uiController.signInAnonButton.gameObject.SetActive(true);
            IsLoggedInAnon = false;
            Debug.Log("Signing out");

        }
    }
}
