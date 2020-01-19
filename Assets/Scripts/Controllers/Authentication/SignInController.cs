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

        public Image testImage;
        public static bool forceLogin;
        public static AuthInfo authInfo { get; set; }
        public const string METHOD_GOOGLE = "Google";
        public const string METHOD_ANONYMOUS = "Anonymous";

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
        private StartUIController _uiController => (StartUIController)GameManager.Instance.UIController();

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
        }

        private void inputChangedCallBack() {
            if (_uiController.usernameInput.text.Length > 2){
                AvatarController.Instance.SetImage(avatarPreview, _uiController.usernameInput.text);
            }
        }

        void OnDisable() {
            //Un-Register InputField Events
            //_uiController.findUserInput.onEndEdit.RemoveAllListeners();

            if (_uiController != null)
                _uiController.usernameInput.onValueChanged.RemoveAllListeners();
        }

        /// <summary>
        /// Set user name of client according to input.
        /// </summary>
        public async void SetUsername() {
            _uiController.usernameTakenMessage.gameObject.SetActive(false);
            _uiController.invalidCharactersMessage.gameObject.SetActive(false);
            _uiController.usernameTooShortMessage.gameObject.SetActive(false);
            string newUserName = (_uiController.usernameInput.text == "") ? "Anonymous User" : _uiController.usernameInput.text;

            //If the user tries to log in with it's previously used username, use the stored password
            string password = (PlayerPrefs.GetString(PLAYERPREFS_USERNAME) != null && newUserName == PlayerPrefs.GetString(PLAYERPREFS_USERNAME)) ?
                PlayerPrefs.GetString(PLAYERPREFS_PASSWORD) : CreatePassword(32);
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
        /// Generates a password of the specified length.
        /// </summary>
        public static string CreatePassword(int length) {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            string pw = "";
            while (pw.Length < length) {
                pw += valid[UnityEngine.Random.Range(0, valid.Length)];
            }
            return pw;
        }

        /// <summary>
        /// Use this to sign in a user.
        /// </summary>
        public void SignIn() {
            Debug.Log("Google Play: Authentication Status: " + PlayGamesPlatform.Instance.IsAuthenticated());
            if (!PlayGamesPlatform.Instance.IsAuthenticated()) {
                // authenticate user:
                Social.localUser.Authenticate((bool success) =>
                {
                    if (!success)
                    {
                        _uiController.googleAuthButtonText.text = SIGNED_OUT_TEXT_GOOGLE;
                        return;
                    }

                    Debug.Log("Google Play: Authentication success: " + success);
                    Debug.Log("Google Play: Username: " + Social.localUser.userName);
                    Debug.Log("Google Play: ID: " + Social.localUser.id);
                    Debug.Log("Google Play: Avatar: " + Social.localUser.image);
                    Texture2D texture = Social.localUser.image;
                    testImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                    
                    _uiController.googleAuthButtonText.text = SIGNED_IN_TEXT_GOOGLE;
                    OnSuccess();
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
            string authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
            string pushToken = PushHandler.Instance.pushToken ?? "";

            Task<string> authenticate = Communicator.Authenticate(userName, authCode, pushToken, METHOD_GOOGLE);
            string response = await authenticate;
            if (response == Communicator.USERNAME_TAKEN_ERROR_MESSAGE) {
                /**
                 * Since GooglePlay Usernames are unique (and can't contain special characters), and anonymous users get the 'anon-' prefix, this should never happen
                 */
                Debug.LogError("Google Username is already taken.");
            }
            else if (response == Communicator.INVALID_CHARACTERS_ERROR_MESSAGE) {
                /**
                 * Since GooglePlay Usernames can't contain special characters, this should never happen
                 */
                Debug.LogError("Username contains invalid characters");
            }
            else if (response == null) {
                throw new Exception("An unknown error occurred");
            }
            else {
                string authToken = response;
                PlayerPrefs.SetString(Communicator.PLAYERPREFS_AUTH_TOKEN, authToken);
                PlayerPrefs.SetString(PLAYERPREFS_USERNAME, userName);
                PlayerPrefs.SetString(PLAYERPREFS_PASSWORD, authCode);
                PlayerPrefs.SetString(Communicator.PLAYERPREFS_SIGNIN_METHOD, METHOD_GOOGLE);
                IsLoggedInGoogle = true;
                await Communicator.SetupApiConnection();
                _uiController.CloseUsernamePanel();
                _uiController.CloseLoginPanel();
                Debug.Log($"Auth token received: {authToken}");
                _uiController.googleAuthButtonText.text = (IsLoggedInGoogle) ?
                    SIGNED_IN_TEXT_GOOGLE : SIGNED_OUT_TEXT_GOOGLE;
                _uiController.signInAnonButton.gameObject.SetActive(false);
                _uiController.sidebar.SetActive(true);
                _uiController.DisplayGameView();
                _uiController.HighscoreButtonSetActiveState(true);
            }

        }

        /// <summary>
        /// Use this to sign out a user.
        /// </summary>
        private void SignOut() {
            // sign out
            PlayGamesPlatform.Instance.SignOut();
            IsLoggedInGoogle = false;
            _uiController.googleAuthButtonText.text = SIGNED_OUT_TEXT_GOOGLE;
            Debug.Log("Signing out of Google Play");

        }

        /// <summary>
        /// Use this to sign out an anonymous user.
        /// </summary>
        public void SignOutAnon() {
            // sign out
            IsLoggedInAnon = false;
            _uiController.anonAuthButtonText.text = (IsLoggedInAnon) ? SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;
            _uiController.signInAnonButton.gameObject.SetActive(true);
            Debug.Log("Signing out");

        }
    }
}
