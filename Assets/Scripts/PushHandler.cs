using Firebase;
using Firebase.Messaging;
using UnityEngine;

/// <summary>
///     This class handles communication with the Google Firebase API to enable push notifications.
/// </summary>
public class PushHandler : MonoBehaviour
{
    /**
     * static fields
     */

    public static PushHandler Instance;

    /**
     * public fields
     */

    public bool isUsable;
    public string pushToken;

    /// <summary>
    ///     The awake function is used to make sure the PushHandler acts as a singleton.
    /// </summary>
    public void Awake()
    {
        var objs = GameObject.FindGameObjectsWithTag("PushHandler");
        if (objs.Length > 1)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        Instance = this;
    }

    /// <summary>
    ///     Dependencies are checked and the app is registered within the Firebase API.
    /// </summary>
    public async void Start()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Firebase was setup succesfully - you should receive a token soon");

            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;

            var app = FirebaseApp.DefaultInstance;
            Debug.Log($"Firebase: Registered app name is {app.Name}");
            Debug.Log($"Firebase: Registered database URL is {app.Options.DatabaseUrl}");

            // Set a flag here to indicate whether Firebase is ready to use by your app.
            isUsable = true;
        }
        else
        {
            Debug.LogError(string.Format(
                "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
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
        Debug.Log("Received Registration Token: " + token.Token);
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
        Debug.Log("Received a new message from: " + e.Message.From);
    }
}