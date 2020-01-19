using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class Configuration : Singleton<Configuration>
{
    [SerializeField] private string serverURL;

    public string ServerURL => serverURL;


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
