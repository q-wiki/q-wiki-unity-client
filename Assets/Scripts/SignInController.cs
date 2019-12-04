using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;
using UnityEngine.SocialPlatforms;
using System;
using System.Threading.Tasks;
using Controllers.UI;
using Controllers;

public class SignInController : MonoBehaviour
{

    private Firebase.Auth.FirebaseAuth auth = null;
    private Firebase.Auth.FirebaseUser user = null;

    private bool forceLogin = false;
    private bool firebaseNotInitialized = true;

    private bool isLoggedIn = false;
    private bool isLoggedInAnon = false;

    private StartUIController _uiController => (StartUIController) GameManager.Instance.UIController();


    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("UsernamePanel").SetActive(false);


        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
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


        _uiController.googleAuthButtonText.text = PlayGamesPlatform.Instance.IsAuthenticated() ? StartUIController.SIGNED_IN_TEXT_GOOGLE : StartUIController.SIGNED_OUT_TEXT_GOOGLE;

    }

    // Update is called once per frame
    void Update()
    {
        if (firebaseNotInitialized && PushHandler.Instance.isUsable) {
            firebaseNotInitialized = false;
            InitializeFirebase();
            if (auth.CurrentUser == null && !PlayGamesPlatform.Instance.IsAuthenticated()) {
                forceLogin = true;
            }

            _uiController.anonAuthButtonText.text = (auth.CurrentUser != null) ? StartUIController.SIGNED_IN_TEXT_ANON : StartUIController.SIGNED_OUT_TEXT_ANON;

            if (forceLogin)
            {
                ForceLogin();
                forceLogin = false;
            }

        }
    }

    void ForceLogin()
    {
        if (auth != null && auth.CurrentUser == null && !PlayGamesPlatform.Instance.IsAuthenticated())
        {
            Debug.Log("User is asked to chose a SignIn Method");
            _uiController.ToggleSettings();
            _uiController.DisplayLoginStart();
            _uiController.settingsButton.gameObject.SetActive(false);
        }
    }

    // Handle initialization of the necessary firebase modules:
    void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    // Track state changes of the auth object.
    void AuthStateChanged(object sender, EventArgs eventArgs)
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

    void OnDestroy(){
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

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
        Task<bool> checkIfTaken = CheckUsernameAvailabilityAsync(newUserName);
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

        _uiController.anonAuthButtonText.text = (isLoggedInAnon) ? StartUIController.SIGNED_IN_TEXT_ANON : StartUIController.SIGNED_OUT_TEXT_ANON;
    }

    private Task CreateAnonUserAsync(string newUserName) {
        return Task.FromResult(true);
    }

    private Task ChangeUsernameAsync(string newUserName) {
        return Task.FromResult(true);
    }

    private Task<bool> CheckUsernameAvailabilityAsync(string username) {
        if(username == "golem") {
            return Task.FromResult(false);
        }
        else return Task.FromResult(true);
    }

    string authCode;

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

                _uiController.googleAuthButtonText.text = success ? StartUIController.SIGNED_IN_TEXT_GOOGLE : StartUIController.SIGNED_OUT_TEXT_GOOGLE;
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

    private void OnSuccess()
    {
        Debug.Log("Successfully Signed In");
        _uiController.settingsButton.gameObject.SetActive(true);
        Debug.Log("UDEBUG: AuthCode: " + PlayGamesPlatform.Instance.GetServerAuthCode());
        PlayGamesPlatform.Instance.GetAnotherServerAuthCode(true, (string code) => {
            Debug.Log("UDEBUG: AuthCodeAsync: " + code);
            authCode = code;
        });
    }

    public void SignOut()
    {
        // sign out
        PlayGamesPlatform.Instance.SignOut();
        _uiController.googleAuthButtonText.text = StartUIController.SIGNED_OUT_TEXT_GOOGLE;
        Debug.Log("Signing out of Google Play");
    }

    public void SignOutAnon()
    {
        // sign out
        auth.SignOut();
        isLoggedInAnon = false;
        _uiController.anonAuthButtonText.text = (isLoggedInAnon) ? StartUIController.SIGNED_IN_TEXT_ANON : StartUIController.SIGNED_OUT_TEXT_ANON;
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

    public void ShowLeaderboard()
    {
        Social.ShowLeaderboardUI();
    }

    public void ShowAchievements()
    {
        Social.ShowAchievementsUI();
    }

    public void UnlockAchievements()
    {
        PlayGamesPlatform.Instance.IncrementAchievement("CgkI-f_-2q4eEAIQAg", 10, (bool success) => {
            // handle success or failure
        });
    }

    public void PostScore()
    {
        Social.ReportScore(4, "CgkI-f_-2q4eEAIQAQ", (bool success) => {
            // handle success or failure
        });
    }
}
