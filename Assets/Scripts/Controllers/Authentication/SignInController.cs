using System;
using System.Threading.Tasks;
using Controllers.UI;
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
        public const string method_google = "Google";
        public const string method_anonymous = "Anonymous";

        public static bool isLoggedInGoogle;
        public static bool isLoggedInAnon;
        public static bool reauthenticateWithGoogle = false;
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

            isLoggedInGoogle = PlayGamesPlatform.Instance.IsAuthenticated();
            _uiController.googleAuthButtonText.text = isLoggedInGoogle ? SIGNED_IN_TEXT_GOOGLE : SIGNED_OUT_TEXT_GOOGLE;
            _uiController.anonAuthButtonText.text = isLoggedInAnon ? SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;

            isLoggedInAnon = (!string.IsNullOrEmpty(PlayerPrefs.GetString(PLAYERPREFS_USERNAME)) && !isLoggedInGoogle );

            _uiController.signInAnonButton.gameObject.SetActive(!isLoggedIn);
            _uiController.sidebar.SetActive(isLoggedIn);

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
            _uiController.ToggleSettings();
            _uiController.DisplayLoginStart();
            _uiController.ShowSettingsButton(false);
        }


        /// <summary>
        /// Sign in client anonymously.
        /// </summary>
        public void SignInAnonymously() {

            _uiController.OpenUsernamePanel();
        }


        ///// <summary>
        ///// Set user name of client according to input.
        ///// </summary>
        //public async void SetUsername(){
        //    _uiController.usernameTakenMessage.gameObject.SetActive(false);
        //    string newUserName = _uiController.usernameInput.text;
        //    if (newUserName == "") { newUserName = "Anonymous User"; }

        //    bool usernameAvailable = false;
        //    var checkIfTaken = CheckUsernameAvailabilityAsync(newUserName);
        //    usernameAvailable = await checkIfTaken;

        //    if (usernameAvailable) {
        //        if (isLoggedInAnon) {
        //            Task changeUsernameAsync = ChangeUsernameAsync(newUserName);
        //            await changeUsernameAsync;
        //            if(changeUsernameAsync.Status == TaskStatus.Faulted) {
        //                //handle failure
        //            }
        //            else {
        //                _uiController.CloseUsernamePanel();
        //            }
        //        }
        //        else {
        //            Task createAnonUserAsync = CreateAnonUserAsync(newUserName);
        //            await createAnonUserAsync;
        //            if (createAnonUserAsync.Status == TaskStatus.Faulted) {
        //                //handle failure
        //            }
        //            else {
        //                isLoggedIn = true;
        //                isLoggedInAnon = true;
        //                _uiController.CloseUsernamePanel();
        //            }
        //        }
        //    }
        //    else {
        //        _uiController.usernameTakenMessage.gameObject.SetActive(true);
        //    }

        //    _uiController.anonAuthButtonText.text = (isLoggedInAnon) ? 
        //        SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;
        //}

        /// <summary>
        /// Set user name of client according to input.
        /// </summary>
        public async void SetUsername() {
            _uiController.usernameTakenMessage.gameObject.SetActive(false);
            _uiController.invalidCharactersMessage.gameObject.SetActive(false);
            string newUserName = (_uiController.usernameInput.text == "") ? "Anonymous User" : _uiController.usernameInput.text;

            //If the user tries to log in with it's previously used username, use the stored password
            string password = (PlayerPrefs.GetString(PLAYERPREFS_USERNAME) != null && newUserName == PlayerPrefs.GetString(PLAYERPREFS_USERNAME)) ?
                PlayerPrefs.GetString(PLAYERPREFS_PASSWORD) : CreatePassword(32);
            string pushToken = PushHandler.Instance.pushToken ?? "";

            //Tries to authenticate by either logging in with the stored credentials or by trying to create a new user
            Task<string> authenticate = Communicator.Authenticate(newUserName, password, pushToken, method_anonymous);
            string response = await authenticate;

            if (response == Communicator.USERNAME_TAKEN_ERROR_MESSAGE) {
                _uiController.usernameTakenMessage.gameObject.SetActive(true);
            }
            else if (response == Communicator.INVALID_CHARACTERS_ERROR_MESSAGE) {
                _uiController.invalidCharactersMessage.gameObject.SetActive(true);
            }
            else if (response == null) {
                throw new Exception("An unknown error occurred");
            }
            else {
                string authToken = response;
                PlayerPrefs.SetString(Communicator.PLAYERPREFS_AUTH_TOKEN, authToken);
                PlayerPrefs.SetString(PLAYERPREFS_USERNAME, newUserName);
                PlayerPrefs.SetString(PLAYERPREFS_PASSWORD, password);
                PlayerPrefs.SetString(Communicator.PLAYERPREFS_SIGNIN_METHOD, method_anonymous);
                isLoggedInAnon = true;
                await Communicator.SetupApiConnection();
                _uiController.CloseUsernamePanel();
                _uiController.CloseLoginPanel();
                Debug.Log($"Auth token received: {authToken}");
                Debug.Log($"Avatar: {authInfo.User.ProfileImage}");
            }

            _uiController.anonAuthButtonText.text = (isLoggedInAnon) ?
                SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;
            _uiController.signInAnonButton.gameObject.SetActive(false);
            _uiController.sidebar.SetActive(true);
            _uiController.DisplayGameView();
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
        /// Create an anonymous user.
        /// </summary>
        /// <param name="newUserName">Provided user name</param>
        /// <returns></returns>
        //private async Task<string> CreateAnonUserAsync(string newUserName) {
        //    Task<string> authenticate = Communicator.Authenticate(newUserName, "12345678", "");
        //    string response = await authenticate;
        //    return response;
        //}

        /// <summary>
        /// Change the name of an user.
        /// </summary>
        /// <param name="newUserName">Provided user name</param>
        /// <returns></returns>
        private Task ChangeUsernameAsync(string newUserName) {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Check if provided user name is available.
        /// </summary>
        /// <param name="username">Provided user name</param>
        /// <returns></returns>
        private Task<bool> CheckUsernameAvailabilityAsync(string username) {
            if (username == "golem") {
                return Task.FromResult(false);
            }
            else return Task.FromResult(true);
        }

        /// <summary>
        /// Use this to sign in a user.
        /// </summary>
        public void SignIn() {
            Debug.Log("UDEBUG: Authentication Status: " + PlayGamesPlatform.Instance.IsAuthenticated());
            if (!PlayGamesPlatform.Instance.IsAuthenticated()) {
                // authenticate user:
                Social.localUser.Authenticate((bool success) => {
                    Debug.Log("UDEBUG: Authentication success: " + success);
                    Debug.Log("UDEBUG: Username: " + Social.localUser.userName);
                    Debug.Log("UDEBUG: ID: " + Social.localUser.id);
                    Debug.Log("UDEBUG: Avatar: " + Social.localUser.image.ToString());
                    Texture2D texture = Social.localUser.image;
                    testImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));



                    _uiController.googleAuthButtonText.text = success ? SIGNED_IN_TEXT_GOOGLE : SIGNED_OUT_TEXT_GOOGLE;
                    if (success) {
                        OnSuccess();
                    }
                    // handle success or failure
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
            _uiController.ShowSettingsButton(true);
            Debug.Log("UDEBUG: AuthCode: " + PlayGamesPlatform.Instance.GetServerAuthCode());
            PlayGamesPlatform.Instance.GetAnotherServerAuthCode(true, (string code) => {
                Debug.Log("UDEBUG: AuthCodeAsync: " + code);
            });
            Debug.Log("UDEBUG: ID Token: " + PlayGamesPlatform.Instance.GetIdToken());

            string userName = Social.localUser.userName;
            string authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
            string pushToken = PushHandler.Instance.pushToken ?? "";

            Task<string> authenticate = Communicator.Authenticate(userName, authCode, pushToken, method_google);
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
                PlayerPrefs.SetString(Communicator.PLAYERPREFS_SIGNIN_METHOD, method_google);
                isLoggedInGoogle = true;
                await Communicator.SetupApiConnection();
                _uiController.CloseUsernamePanel();
                _uiController.CloseLoginPanel();
                Debug.Log($"Auth token received: {authToken}");
                _uiController.googleAuthButtonText.text = (isLoggedInGoogle) ?
                    SIGNED_IN_TEXT_GOOGLE : SIGNED_OUT_TEXT_GOOGLE;
                _uiController.signInAnonButton.gameObject.SetActive(false);
                _uiController.sidebar.SetActive(true);
                _uiController.DisplayGameView();
            }

        }

        /// <summary>
        /// Use this to sign out a user.
        /// </summary>
        private void SignOut() {
            // sign out
            PlayGamesPlatform.Instance.SignOut();
            isLoggedInGoogle = false;
            _uiController.googleAuthButtonText.text = SIGNED_OUT_TEXT_GOOGLE;
            Debug.Log("Signing out of Google Play");

        }

        /// <summary>
        /// Use this to sign out an anonymous user.
        /// </summary>
        public void SignOutAnon() {
            // sign out
            isLoggedInAnon = false;
            _uiController.anonAuthButtonText.text = (isLoggedInAnon) ? SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;
            Debug.Log("Signing out");

        }

        /// <summary>
        /// Use this to show the leaderboard.
        /// </summary>
        public void ShowLeaderboard() {
            Social.ShowLeaderboardUI();
        }

        /// <summary>
        /// Use this to show the achievements.
        /// </summary>
        public void ShowAchievements() {
            Social.ShowAchievementsUI();
        }

        /// <summary>
        /// Use this to unlock achievements.
        /// </summary>
        public void UnlockAchievements() {
            PlayGamesPlatform.Instance.IncrementAchievement("CgkI-f_-2q4eEAIQAg", 10, (bool success) => {
                // handle success or failure
            });
        }

        /// <summary>
        /// Use this to post the score.
        /// </summary>
        public void PostScore() {
            Social.ReportScore(4, "CgkI-f_-2q4eEAIQAQ", (bool success) => {
                // handle success or failure
            });
        }
    }
}
