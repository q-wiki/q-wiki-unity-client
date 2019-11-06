using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
////using Firebase;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;
using UnityEngine.SocialPlatforms;
using System;

public class SignInController : MonoBehaviour
{
    public Text GoogleAuthButtonText;

    private string SIGNED_IN_TEXT = "Sign Out";
    private string SIGNED_OUT_TEXT = "Sign In With Google";
    // Start is called before the first frame update
    void Start()
    {
        // recommended for debugging:
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }

    // Update is called once per frame
    void Update()
    {
        
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
                GoogleAuthButtonText.text = success ? SIGNED_IN_TEXT : SIGNED_OUT_TEXT;
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
    }

    public void SignOut()
    {
        // sign out
        PlayGamesPlatform.Instance.SignOut();
        GoogleAuthButtonText.text = SIGNED_OUT_TEXT;
        Debug.Log("Signing out of Google Play");
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
        Social.ReportProgress("CgkI-f_-2q4eEAIQAg", 10.0f, (bool success) => {
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
