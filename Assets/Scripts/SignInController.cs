using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using GooglePlayGames;
//using GooglePlayGames.BasicApi;
//using GooglePlayGames.BasicApi.Multiplayer;
//using UnityEngine.SocialPlatforms;
using System;

public class SignInController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Action OnSendNotification = () => SendNotification();

        //PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
        //// enables saving game progress.
        //.EnableSavedGames()
        //// registers a callback to handle game invitations received while the game is not running.
        //.WithInvitationDelegate((Invitation invitation, bool shouldAutoAccept) => { SendNotification(); })
        //// registers a callback for turn based match notifications received while the
        //// game is not running.
        //.WithMatchDelegate((TurnBasedMatch match, bool shouldAutoLaunch) => { MatchDelegate(); })
        //// requests the email address of the player be available.
        //// Will bring up a prompt for consent.
        //.RequestEmail()
        //// requests a server auth code be generated so it can be passed to an
        ////  associated back end server application and exchanged for an OAuth token.
        //.RequestServerAuthCode(false)
        //// requests an ID token be generated.  This OAuth token can be used to
        ////  identify the player to other services such as Firebase.
        //.RequestIdToken()
        //.Build();

        //PlayGamesPlatform.InitializeInstance(config);
        //// recommended for debugging:
        //PlayGamesPlatform.DebugLogEnabled = true;

        //// Activate the Google Play Games platform
        //PlayGamesPlatform.Activate();

        //PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().RequestServerAuthCode(false).Build();
        //PlayGamesPlatform.InitializeInstance(config);
        //PlayGamesPlatform.DebugLogEnabled = true;
        //PlayGamesPlatform.Activate();


    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SignIn()
    {
        //// authenticate user:
        //Social.localUser.Authenticate((bool success) => {

        //    Debug.Log(success);
        //    Debug.Log(Social.localUser.userName);
        //    // handle success or failure
        //});

    }

    public void SignIn2()
    {
        //// authenticate user:
        //Social.localUser.Authenticate((bool success) => {

        //    Debug.Log(success);
        //    Debug.Log(Social.localUser.userName);
        //    // handle success or failure
        //});

    }

    void SendNotification()
    {

    }


    void MatchDelegate()
    {

    }
}
