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

        if (previousGame != null)
        {
            Debug.Log($"Previous game {previousGame.Id} restored successfully, changing to game scene");
            SceneManager.LoadSceneAsync("GameScene");
        }
        else
        {
            Debug.Log($"No previous game found, showing start scene");
            SceneManager.LoadSceneAsync("StartScene");
        }
        
        LoadingIndicator.Instance.Hide();
    }
}
