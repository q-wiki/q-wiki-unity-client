using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///     This class is used to handle the opening screen / splash screen of the app.
///     It preloads a scene and shows a loading indicator.
/// </summary>
public class OpeningHandler : MonoBehaviour
{
    // Start is called before the first frame update
    /// <summary>
    ///     When the app is started, try to restore a previous game.
    ///     If it does not exist, show the start screen of the game.
    /// </summary>
    private async void Start()
    {
        // Check if Google Play Service is up-to-date
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                //   app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });

        // initialize server session and restore previous game if there is one
        Debug.Log("Trying to restore previous game…");
        await Communicator.SetupApiConnection();
        var previousGame = await Communicator.RestorePreviousGame();

        string scene;

        if (previousGame == null)
        {
            scene = "StartScene";
        }
        else
        {
            /**
             * if there was no opponent found in the previous game, delete the game from backend / player prefs
             */
            if (previousGame.AwaitingOpponentToJoin == true)
            {
                scene = "StartScene";
                await Communicator.AbortCurrentGame();
                PlayerPrefs.DeleteKey("CURRENT_GAME_ID");
                Debug.Log("Deleted previous game");
            }
            else
            {
                scene = "GameScene";
            }
        }

        Debug.Log($"Switching to {scene}");
        StartCoroutine(LoadScene(scene));
    }

    /// <summary>
    ///     Load the provided scene asynchronously.
    /// </summary>
    /// <param name="scene">The scene to load.</param>
    /// <returns>A coroutine.</returns>
    private IEnumerator LoadScene(string scene)
    {
        var operation = SceneManager.LoadSceneAsync(scene);
        operation.allowSceneActivation = false;
        while (operation.progress < 0.9f)
            yield return null;

        if (operation.progress >= 0.9f)
        {
            LoadingIndicator.Instance.Hide();
            operation.allowSceneActivation = true;
        }
    }
}