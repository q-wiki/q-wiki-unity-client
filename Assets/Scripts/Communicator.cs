using System;
using UnityEngine.Networking;

public class Communicator
{

    UnityWebRequest webRequest = new UnityWebRequest();

    // TODO: connect to REST API to really "communicate"


    public Communicator()
    {

    }

    public string GetMinigame()
    {
        // TODO: GET-REQUEST an Backend schicken
        // return JsonUtility.FromJson<Minigame>(json);

        return null;

    }

    public bool checkAnswer()
    {
        // TODO: Post-Request an Backend, um MinigameResult zu erhalten
        return false;
    }
}