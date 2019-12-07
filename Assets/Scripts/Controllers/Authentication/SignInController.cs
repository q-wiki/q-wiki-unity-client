using System;
using System.Threading.Tasks;
using Controllers.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.Authentication
{
    /// <summary>
    /// The SignIn Controller handles Firebase and Google Play authentication for the app.
    /// </summary>
    public class SignInController : MonoBehaviour
    {

        public Image testImage;

        /* private fields */
        private Firebase.Auth.FirebaseAuth auth;
        private Firebase.Auth.FirebaseUser user;
        private bool forceLogin;
        private bool firebaseNotInitialized = true;
        private bool isLoggedIn;
        private bool isLoggedInAnon;
        private string authCode;
    
        private StartUIController _uiController => (StartUIController) GameManager.Instance.UIController();

        /* private constants */
        private const string SIGNED_IN_TEXT_GOOGLE = "Sign Out";
        private const string SIGNED_OUT_TEXT_GOOGLE = "Sign In With Google";
        private const string SIGNED_IN_TEXT_ANON = "Change Username";
        private const string SIGNED_OUT_TEXT_ANON = "Sign in Anonymously";
    
        /// <summary>
        /// Start is called before the first frame update.
        /// When the SignInController is initialized, the play games API is constructed.
        /// </summary>
        void Start()
        {
            testImage = GameObject.Find("UserAvatar").GetComponent<Image>();

            var config = new PlayGamesClientConfiguration.Builder()
                .RequestServerAuthCode(true)
                .Build();
            PlayGamesPlatform.InitializeInstance(config);

            // recommended for debugging:
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();

            //// Check if Google Play Service is up-to-date
            //FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            //    var dependencyStatus = task.Result;
            //    if (dependencyStatus == DependencyStatus.Available)
            //    {
            //        // Create and hold a reference to your FirebaseApp,
            //        // where app is a Firebase.FirebaseApp property of your application class.
            //        //   app = Firebase.FirebaseApp.DefaultInstance;

            //        // Set a flag here to indicate whether Firebase is ready to use by your app.
            //        InitializeFirebase();
            //        Debug.Log(auth);
            //        if (auth.CurrentUser == null && !PlayGamesPlatform.Instance.IsAuthenticated())
            //        {
            //            forceLogin = true;
            //        }

            //        anonAuthButtonText.text = (auth.CurrentUser != null) ? SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;
            //    }
            //    else
            //    {
            //        Debug.LogError(System.String.Format(
            //          "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            //        // Firebase Unity SDK is not safe to use here.
            //    }
            //});
        
            _uiController.googleAuthButtonText.text = PlayGamesPlatform.Instance.IsAuthenticated() ? SIGNED_IN_TEXT_GOOGLE : SIGNED_OUT_TEXT_GOOGLE;

        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            if (firebaseNotInitialized && PushHandler.Instance.isUsable) {
                firebaseNotInitialized = false;
                InitializeFirebase();
                if (auth.CurrentUser == null && !PlayGamesPlatform.Instance.IsAuthenticated()) {
                    forceLogin = true;
                }

                _uiController.anonAuthButtonText.text = (auth.CurrentUser != null) ? 
                    SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;

                if (forceLogin)
                {
                    ForceLogin();
                    forceLogin = false;
                }

            }
        }

        /// <summary>
        /// Forcing user to login.
        /// </summary>
        private void ForceLogin()
        {
            if (auth != null && auth.CurrentUser == null && !PlayGamesPlatform.Instance.IsAuthenticated())
            {
                Debug.Log("User is asked to choose a SignIn Method");
                _uiController.ToggleSettings();
                _uiController.DisplayLoginStart();
                _uiController.ShowSettingsButton(false);
            }
        }

        /// <summary>
        /// Handle initialization of the necessary firebase modules.
        /// </summary>
        private void InitializeFirebase()
        {
            Debug.Log("Setting up Firebase Auth");
            auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            auth.StateChanged += AuthStateChanged;
            AuthStateChanged(this, null);
        }

        /// <summary>
        /// Track state changes of the auth object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AuthStateChanged(object sender, EventArgs eventArgs)
        {
            if (auth.CurrentUser != user)
            {
                bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
                if (!signedIn && user != null)
                {
                    Debug.Log("Signed out " + user.UserId);
                }
                user = auth.CurrentUser;
                if (signedIn)
                {
                    Debug.Log("Signed in " + user.UserId);
                }
            }
        }

        /// <summary>
        /// This is called when the object is destroyed.
        /// </summary>
        private void OnDestroy(){
            auth.StateChanged -= AuthStateChanged;
            auth = null;
        }

        /// <summary>
        /// Sign in client anonymously.
        /// </summary>
        public void SignInAnonymously(){
            ////TODO remove block
            //if(false && auth.CurrentUser == null)
            //{
            //    auth.SignInAnonymouslyAsync().ContinueWith(task => {
            //        if (task.IsCanceled)
            //        {
            //            Debug.LogError("SignInAnonymouslyAsync was canceled.");
            //            return;
            //        }
            //        if (task.IsFaulted)
            //        {
            //            Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
            //            return;
            //        }

            //        Firebase.Auth.FirebaseUser newUser = task.Result;
            //        user = newUser;
            //        Debug.LogFormat("User signed in successfully: {0} ({1})",
            //            newUser.DisplayName, newUser.UserId);
            //        auth.CurrentUser.TokenAsync(false).ContinueWith((task2) => {
            //            Debug.Log(task2.Result);
            //        });
            //    });
            //}
            _uiController.OpenUsernamePanel();

        }


        /// <summary>
        /// Set user name of client according to input.
        /// </summary>
        public async void SetUsername(){
            _uiController.usernameTakenMessage.gameObject.SetActive(false);
            string newUserName = _uiController.usernameInput.text;
            if (newUserName == "") { newUserName = "Anonymous User"; }
        
            //Firebase.Auth.FirebaseUser user = auth.CurrentUser;
            //if (user != null)
            //{
            //    Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
            //    {
            //        DisplayName = newUserName,
            //    };
            //    user.UpdateUserProfileAsync(profile).ContinueWith(task =>
            //    {
            //        if (task.IsCanceled)
            //        {
            //            Debug.LogError("UpdateUserProfileAsync was canceled.");
            //            return;
            //        }
            //        if (task.IsFaulted)
            //        {
            //            Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
            //            return;
            //        }

            //        Debug.Log("User profile updated successfully.");
            //        Debug.Log("New Username: " + profile.DisplayName);
            //    });
            //    anonAuthButtonText.text = (auth.CurrentUser != null) ? SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;
            //    menuController.CloseUsernamePanel();
            //}
        
            bool usernameAvailable = false;
            var checkIfTaken = CheckUsernameAvailabilityAsync(newUserName);
            usernameAvailable = await checkIfTaken;

            if (usernameAvailable) {
                if (isLoggedInAnon) {
                    Task changeUsernameAsync = ChangeUsernameAsync(newUserName);
                    await changeUsernameAsync;
                    if(changeUsernameAsync.Status == TaskStatus.Faulted) {
                        //handle failure
                    }
                    else {
                        _uiController.CloseUsernamePanel();
                    }
                }
                else {
                    Task createAnonUserAsync = CreateAnonUserAsync(newUserName);
                    await createAnonUserAsync;
                    if (createAnonUserAsync.Status == TaskStatus.Faulted) {
                        //handle failure
                    }
                    else {
                        isLoggedIn = true;
                        isLoggedInAnon = true;
                        _uiController.CloseUsernamePanel();
                    }
                }
            }
            else {
                _uiController.usernameTakenMessage.gameObject.SetActive(true);
            }

            _uiController.anonAuthButtonText.text = (isLoggedInAnon) ? 
                SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;
        }

        /// <summary>
        /// Create an anonymous user.
        /// </summary>
        /// <param name="newUserName">Provided user name</param>
        /// <returns></returns>
        private Task CreateAnonUserAsync(string newUserName) {
            return Task.FromResult(true);
        }

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
            if(username == "golem") {
                return Task.FromResult(false);
            }
            else return Task.FromResult(true);
        }

        /// <summary>
        /// Use this to sign in a user.
        /// </summary>
        public void SignIn()
        {
            Debug.Log("UDEBUG: Authentication Status: " + PlayGamesPlatform.Instance.IsAuthenticated());
            if (!PlayGamesPlatform.Instance.IsAuthenticated())
            {
                // authenticate user:
                Social.localUser.Authenticate((bool success) =>
                {
                    Debug.Log("UDEBUG: Authentication success: " + success);
                    Debug.Log("UDEBUG: Username: " + Social.localUser.userName);
                    Debug.Log("UDEBUG: ID: " + Social.localUser.id);
                    Debug.Log("UDEBUG: Avatar: " + Social.localUser.image.ToString());
                    Texture2D texture = Social.localUser.image;
                    testImage.sprite = Sprite.Create(texture, new Rect(0,0,texture.width,texture.height), new Vector2(0.5f, 0.5f));


                    _uiController.googleAuthButtonText.text = success ? SIGNED_IN_TEXT_GOOGLE : SIGNED_OUT_TEXT_GOOGLE;
                    if (success)
                    {
                        OnSuccess();
                    }
                    // handle success or failure
                });
            }
            else
            {
                SignOut();
            }

        }

        /// <summary>
        /// This is called when user sign-in was successfully.
        /// </summary>
        private void OnSuccess()
        {
            Debug.Log("Successfully Signed In");
            _uiController.ShowSettingsButton(true);
            Debug.Log("UDEBUG: AuthCode: " + PlayGamesPlatform.Instance.GetServerAuthCode());
            PlayGamesPlatform.Instance.GetAnotherServerAuthCode(true, (string code) => {
                Debug.Log("UDEBUG: AuthCodeAsync: " + code);
                authCode = code;
            });
            Debug.Log("UDEBUG: ID Token: " + PlayGamesPlatform.Instance.GetIdToken());
        }

        /// <summary>
        /// Use this to sign out a user.
        /// </summary>
        private void SignOut()
        {
            // sign out
            PlayGamesPlatform.Instance.SignOut();
            _uiController.googleAuthButtonText.text = SIGNED_OUT_TEXT_GOOGLE;
            Debug.Log("Signing out of Google Play");

        }

        /// <summary>
        /// Use this to sign out an anonymous user.
        /// </summary>
        public void SignOutAnon()
        {
            // sign out
            auth.SignOut();
            isLoggedInAnon = false;
            _uiController.anonAuthButtonText.text = (isLoggedInAnon) ? SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;
            Debug.Log("Signing out of Firebase");

        }
    
        //public void SignInWithFirebase() {
        //    if (auth.CurrentUser == null) {
        //        auth.SignInAnonymouslyAsync().ContinueWith(task => {
        //            if (task.IsCanceled) {
        //                Debug.LogError("SignInAnonymouslyAsync was canceled.");
        //                return;
        //            }
        //            if (task.IsFaulted) {
        //                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
        //                return;
        //            }

        //            Firebase.Auth.FirebaseUser newUser = task.Result;
        //            user = newUser;
        //            Debug.LogFormat("User signed in successfully: {0} ({1})",
        //                newUser.DisplayName, newUser.UserId);
        //        });
        //    }
        //    menuController.OpenUsernamePanel();
        //}

        /// <summary>
        /// Use this to show the leaderboard.
        /// </summary>
        public void ShowLeaderboard()
        {
            Social.ShowLeaderboardUI();
        }

        /// <summary>
        /// Use this to show the achievements.
        /// </summary>
        public void ShowAchievements()
        {
            Social.ShowAchievementsUI();
        }

        /// <summary>
        /// Use this to unlock achievements.
        /// </summary>
        public void UnlockAchievements()
        {
            PlayGamesPlatform.Instance.IncrementAchievement("CgkI-f_-2q4eEAIQAg", 10, (bool success) => {
                // handle success or failure
            });
        }

        /// <summary>
        /// Use this to post the score.
        /// </summary>
        public void PostScore()
        {
            Social.ReportScore(4, "CgkI-f_-2q4eEAIQAQ", (bool success) => {
                // handle success or failure
            });
        }
    }
}
