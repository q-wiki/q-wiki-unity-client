using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningHandler : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {

        // initialize server session and restore previous game if there is one
        Debug.Log("Trying to restore previous game…");
        await Communicator.SetupApiConnection();
        var previousGame = await Communicator.RestorePreviousGame();

        string scene = previousGame == null ? "StartScene" : "GameScene";
        Debug.Log($"Switching to {scene}");
        StartCoroutine(LoadScene(scene));
    }

    IEnumerator LoadScene(string scene)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene);
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
