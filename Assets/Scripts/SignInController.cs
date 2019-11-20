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

public class SignInController : MonoBehaviour
{
    public MenuController menuController;
    public Text googleAuthButtonText;
    public Text anonAuthButtonText;
    public InputField usernameInput;
    public Button settingsButton;
    public PushHandler pushHandler;

    private const string SIGNED_IN_TEXT_GOOGLE = "Sign Out";
    private const string SIGNED_OUT_TEXT_GOOGLE = "Sign In With Google";
    private const string SIGNED_IN_TEXT_ANON = "Change Username";
    private const string SIGNED_OUT_TEXT_ANON = "Sign in Anonymously";

    private Firebase.Auth.FirebaseAuth auth = null;
    private Firebase.Auth.FirebaseUser user = null;

    private bool forceLogin = false;
    private bool firebaseNotInitialized = true;




    // Start is called before the first frame update
    void Start()
    {

        if (menuController == null) menuController = GameObject.Find("MenuController").GetComponent<MenuController>();
        if (googleAuthButtonText == null) googleAuthButtonText = GameObject.Find("SignInText").GetComponent<Text>();
        if (anonAuthButtonText == null) anonAuthButtonText = GameObject.Find("SignInAnonText").GetComponent<Text>();
        if (settingsButton == null) settingsButton = GameObject.Find("SettingsButton").GetComponent<Button>();
        if (usernameInput == null) usernameInput = GameObject.Find("UsernameInputField").GetComponent<InputField>();
        if (pushHandler == null) pushHandler = GameObject.Find("PushHandler").GetComponent<PushHandler>();
        GameObject.Find("UsernamePanel").SetActive(false);

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


        googleAuthButtonText.text = PlayGamesPlatform.Instance.IsAuthenticated() ? SIGNED_IN_TEXT_GOOGLE : SIGNED_OUT_TEXT_GOOGLE;

    }

    // Update is called once per frame
    void Update()
    {
        if (firebaseNotInitialized && pushHandler.isUsable) {
            firebaseNotInitialized = false;
            InitializeFirebase();
            Debug.Log(auth);
            if (auth.CurrentUser == null && !PlayGamesPlatform.Instance.IsAuthenticated()) {
                forceLogin = true;
            }

            anonAuthButtonText.text = (auth.CurrentUser != null) ? SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;

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
            menuController.ToggleSettingsStart();
            menuController.DisplayLoginStart();
            settingsButton.gameObject.SetActive(false);
            Debug.Log("FOOBA");
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
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
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

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    public void SignInAnonymously()
    {
        if(auth.CurrentUser == null)
        {
            auth.SignInAnonymouslyAsync().ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInAnonymouslyAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                    return;
                }

                Firebase.Auth.FirebaseUser newUser = task.Result;
                user = newUser;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    newUser.DisplayName, newUser.UserId);
            });
        }
        menuController.OpenUsernamePanel();

    }


    public void SetUsername()
    {
        string newUserName = usernameInput.text;
        if (newUserName == "") { newUserName = "Anonymous User"; }
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
            {
                DisplayName = newUserName,
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");
                Debug.Log("New Username: " + profile.DisplayName);
            });
            anonAuthButtonText.text = (auth.CurrentUser != null) ? SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;
            menuController.CloseUsernamePanel();
        }
    }


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
                googleAuthButtonText.text = success ? SIGNED_IN_TEXT_GOOGLE : SIGNED_OUT_TEXT_GOOGLE;
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
        settingsButton.gameObject.SetActive(true);
    }

    public void SignOut()
    {
        // sign out
        PlayGamesPlatform.Instance.SignOut();
        googleAuthButtonText.text = SIGNED_OUT_TEXT_GOOGLE;
        Debug.Log("Signing out of Google Play");
    }

    public void SignOutAnon()
    {
        // sign out
        auth.SignOut();
        anonAuthButtonText.text = (auth.CurrentUser != null) ? SIGNED_IN_TEXT_ANON : SIGNED_OUT_TEXT_ANON;
        Debug.Log("Signing out of Firebase");
    }

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
