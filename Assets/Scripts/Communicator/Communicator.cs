using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using UnityEngine;
using WikidataGame;
using WikidataGame.Models;

public class Communicator : MonoBehaviour
{
    /**
     * all Functions and Variables are static at the moment,
     * so that you don't need to provide an instance for every GameObject you want to use the Communicator on
     */
    
    public static bool isConnected;
    private static AuthInfo _auth { get; set; }
    private static WikidataGameAPI _gameApi;
    private static GameInfo _gameInfo;

    /**
     * 
     */
    /* async void Start()
     {
         await Connect();
     }*/



    /**
     * Use this function to connect to the backend
     * var _isConnected is a good indicator to check if the connection was successfully built
     * Use it like this when player hits the "Start new game"-Button: await Communicator.Connect();
     */
    public static async Task Connect()
    {
        await getToken();
        isConnected = true;
       // Debug.Log("isConnected: " + isConnected);
       // Debug.Log($"Bearer {_auth.Bearer}");
        await CreateGame(_auth);
        Debug.Log($"Waiting for Opponent {_gameInfo.IsAwaitingOpponentToJoin}.");
    }
    
    
   /**
    * Use this function to create a new minigame by providing a Tile object and the categoryId
    * Question: does this need to be async as well?
    */
    public static async Task<MiniGame> InitializeMinigame(string tileId, string categoryId)
    {
        MiniGameInit init = new MiniGameInit(tileId, categoryId);
        return await _gameApi.InitalizeMinigameAsync(_gameInfo.GameId, init);
    }


    /**
     * Use this function to get needed information about the minigame you just created
     */
    public static async Task<MiniGame> RetrieveMinigameInfo(string minigameId)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        var miniGame = await _gameApi.RetrieveMinigameInfoAsync(_gameInfo.GameId, minigameId, cts.Token);
        return miniGame;
    }
    
    /**
    * Use this function to POST answers for a minigame to the backend
    * you are getting a MiniGameResult back, which indicates if the answer was right or wrong
    */
    public static async Task<MiniGameResult> AnswerMinigame(string minigameId, IList<string> answers)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        var result = await _gameApi.AnswerMinigameAsync(_gameInfo.GameId, minigameId, answers, cts.Token);
        return result;
    }

    /**
     * Use this function to delete the current game
     * I made this private for now because I am not sure if a player should have the ability to delete a game on his/her own
     */
    private static async void DeleteGame()
    {
        await _gameApi.DeleteGameAsync(_gameInfo.GameId);
        //Debug.Log("Game " + _gameInfo.GameId + " successfully deleted.");
    }
    

    /**
     * Use this function to get the game state of the current game
     */
    public static async Task<Game> GetCurrentGameState()
    {
        CancellationTokenSource cts3 = new CancellationTokenSource();
        var game = await _gameApi.RetrieveGameStateAsync(_gameInfo.GameId, cts3.Token);
       // Debug.Log($"My player id is {game.Me.Id}.");
       // Debug.Log($"My opponents id is {game.Opponent.Id}.");
        return game;
    }

    /**
     * This function is used by the Connect()-function to create a new game
     */
    private static async Task CreateGame(AuthInfo auth)
    {
        //Create a new api client with the obtained bearer token for all other (authorized) requests
        _gameApi = new WikidataGameAPI(new Uri("https://wikidatagame.azurewebsites.net/"), new TokenCredentials(auth.Bearer));
        //Debug.Log(_gameApi);

        CancellationTokenSource cts2 = new CancellationTokenSource();
        //Debug.Log(cts2);
       // Debug.Log("trying to start game...");
        _gameInfo = await _gameApi.CreateNewGameAsync(cts2.Token);
    }

    /**
     * This function is used by the Connect()-function to generate a Bearer token
     */
    private static async Task getToken()
    {
        //Authentication
        var apiClient = new WikidataGameAPI(new Uri("https://wikidatagame.azurewebsites.net/"), new TokenCredentials("auth"));

        CancellationTokenSource cts = new CancellationTokenSource(); // <-- Cancellation Token if you want to cancel the request, user quits, etc. [cts.Cancel()]
        _auth = await apiClient.AuthenticateAsync("123", "test", cts.Token);

    }
}