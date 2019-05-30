using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using UnityEngine;
using WikidataGame;
using WikidataGame.Models;

public class Communicator : MonoBehaviour
{

    void Start()
    {
        RunApiRequests();
    }

    static async Task RunApiRequests()
    {
        //Authentication
        WikidataGameAPI apiClient = new WikidataGameAPI(new Uri("https://wikidatagame.azurewebsites.net/"), new TokenCredentials("auth"));

        CancellationTokenSource cts = new CancellationTokenSource(); // <-- Cancellation Token if you want to cancel the request, user quits, etc. [cts.Cancel()]
        AuthInfo auth = await apiClient.AuthenticateAsync("123", "test", cts.Token);
        Debug.Log($"Bearer {auth.Bearer}");

        //Create a new api client with the obtained bearer token for all other (authorized) requests
        WikidataGameAPI apiClient2 = new WikidataGameAPI(new Uri("https://wikidatagame.azurewebsites.net/"), new TokenCredentials(auth.Bearer));

        CancellationTokenSource cts2 = new CancellationTokenSource();
        GameInfo game = await apiClient2.CreateNewGameAsync(cts2.Token);
        Debug.Log($"Started game {game.GameId}.");

        CancellationTokenSource cts3 = new CancellationTokenSource();
        Game fullGame = await apiClient2.RetrieveGameStateAsync(game.GameId, cts3.Token);
        Debug.Log($"My player id is {fullGame.Me.DeviceId}.");
        // Console.ReadLine();

    }
}