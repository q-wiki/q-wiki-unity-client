using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Messaging;
using Firebase.Unity.Editor;

public class PushHandler : MonoBehaviour {
    
    /**
     * public fields
     */

    public bool isUsable;
    public string pushToken;
    
    /**
     * static fields
     */

    public static PushHandler Instance;
    
    public void Awake()
    { 
        GameObject[] objs = GameObject.FindGameObjectsWithTag("PushHandler");
        if(objs.Length > 1)
            Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
        
        Instance = this;
    }
    
    public async void Start()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            
            Debug.Log($"Firebase was setup succesfully - you should receive a token soon");
            
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;

            FirebaseApp app = FirebaseApp.DefaultInstance;
            Debug.Log($"Firebase: Registered app name is {app.Name}");
            Debug.Log($"Firebase: Registered database URL is {app.Options.DatabaseUrl}");
            
            // Set a flag here to indicate whether Firebase is ready to use by your app.
            isUsable = true;
        }
        else
        {
            Debug.LogError(System.String.Format(
                "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            isUsable = false;
        }
    }

    private async void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log("Received Registration Token: " + token.Token);
        pushToken = token.Token;
        await Communicator.Communicator.UpdateApiConnection(pushToken);
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("Received a new message from: " + e.Message.From);
    }

}

