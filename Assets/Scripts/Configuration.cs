using System;
using UnityEngine;
using UnityEngine.Serialization;


public class Configuration : Singleton<Configuration>
{
    [SerializeField] private string serverURL;
    [SerializeField] private string userName;

    public string ServerURL => serverURL;
    public string UserName => userName;

    /// <summary>
    /// Check if server URL was set, otherwise force quit the app.
    /// </summary>
    /// <exception cref="Exception">Server URL is not set.</exception>
    private void OnEnable()
    {
        if (string.IsNullOrEmpty(serverURL))
        {
            throw new Exception("SERVER_URL was not set.");
        }
        
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
