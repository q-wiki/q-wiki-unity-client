using Controllers.Authentication;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///     This class is used to handle the opening screen / splash screen of the app.
///     It preloads a scene and shows a loading indicator.
/// </summary>
public class OpeningHandler : MonoBehaviour
{
    /// <summary>
    ///     When the app is started, try to restore a previous game.
    ///     If it does not exist, show the start screen of the game.
    /// </summary>
    private async void Start()
    {

        var isAuthenticated = await Communicator.SetupApiConnection();
        Debug.Log($"isAuthenticated: {isAuthenticated}");

        if (!isAuthenticated) SignInController.forceLogin = true;

        string scene = "StartScene";

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