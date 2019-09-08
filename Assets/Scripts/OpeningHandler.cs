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
        await Communicator.Communicator.SetupApiConnection();
        var previousGame = await Communicator.Communicator.RestorePreviousGame();

        string scene;
        
        if (previousGame == null)
            scene = "StartScene";
        else
        {
            /**
             * if there was no opponent found in the previous game, delete the game from backend / player prefs
             */
            if (previousGame.AwaitingOpponentToJoin == true)
            {
                scene = "StartScene";
                await Communicator.Communicator.AbortCurrentGame();
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
