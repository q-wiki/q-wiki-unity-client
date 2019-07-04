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
        await Task.Delay(5000);
        LoadingIndicator.Instance.Hide();
        SceneManager.LoadSceneAsync("Menu");
    }

}
