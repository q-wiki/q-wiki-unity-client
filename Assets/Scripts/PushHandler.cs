using System;
using Firebase;
using Firebase.Messaging;
using UnityEngine;

/// <summary>
///     This class handles communication with the Google Firebase API to enable push notifications.
/// </summary>
public class PushHandler : Singleton<PushHandler>
{

    /**
     * public fields
     */

    public bool isUsable;
    public string pushToken;

    /// <summary>
    ///     Dependencies are checked and the app is registered within the Firebase API.
    /// </summary>
    public async void Start()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Firebase was setup successfully - you should receive a token soon");

            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;

            var app = FirebaseApp.DefaultInstance;

            if(app == null)
                throw new Exception("FirebaseApp is not allowed to be null at this point.");
            
            Debug.Log($"Firebase: Registered app name is {app.Name}");
            Debug.Log($"Firebase: Registered database URL is {app.Options.DatabaseUrl}");

            // Set a flag here to indicate whether Firebase is ready to use by your app.
            isUsable = true;
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            isUsable = false;
        }
    }

    /// <summary>
    ///     If a Firebase token is received, the connection the Wikidata backend API is updated.
    /// </summary>
    /// <param name="sender">The sender of the token</param>
    /// <param name="token">Arguments related to the received token</param>
    private async void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log("Firebase: Received Registration Token: " + token.Token);
        pushToken = token.Token;
        await Communicator.UpdateApiConnection(pushToken);
    }

    /// <summary>
    ///     This function is called whenever a message is received.
    /// </summary>
    /// <param name="sender">The sender of the message</param>
    /// <param name="e">Arguments related to the received message</param>
    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log($"Firebase: Received a new message from: {e.Message.From}");
        
        var data = e.Message.Data;
        
        foreach (var entry in data)
        {
            Debug.Log($"Firebase: (Key: {entry.Key} / Value: {entry.Key})");
            
            if (entry.Key == "refresh" && 
                entry.Value == "true")
            {
                Debug.Log("Firebase: Refreshing game state of the client...");
                // GameManager.Instance.RefreshGameState(true);
                return;
            }
            
            if (entry.Key == "delete" && 
                entry.Value == "true")
            {
                Debug.Log("Firebase: Deleting game state of the client...");
                // GameManager.Instance.RefreshGameState(true);
                return;
            }

        }
    }
}