using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

    private static WikidataGameAPI _gameApi;

    private const string SERVER_URL = "https://wikidatagame.azurewebsites.net/";
    private const string AUTH_TOKEN = "AUTH_TOKEN";
    private const string CURRENT_GAME_ID = "CURRENT_GAME_ID";

    private static string _authToken { get; set; }

    private static string _currentGameId;

    public static bool IsConnected()
    {
        return _gameApi != null;
    }

    /**
     * This function checks whether we have an auth token and restores it; if
     * not, it will request a new token from the server.
     *
     * This means: Before you do anything else, call this method. :)
     */
    public static async Task SetupApiConnection()
    {
        // have we already set up the api connection?
        if (_gameApi != null) return;
        
        // do we have an auth token that's saved?
        Debug.Log("Trying to restore previously saved auth token…");
        var authToken = PlayerPrefs.GetString(AUTH_TOKEN);

        if (string.IsNullOrEmpty(authToken)) {
            Debug.Log("No auth token in PlayerPrefs, fetching new token from server");
            var apiClient = new WikidataGameAPI(new Uri(SERVER_URL), new TokenCredentials("auth"));
            // CancellationTokenSource cts = new CancellationTokenSource(); // <-- Cancellation Token if you want to cancel the request, user quits, etc. [cts.Cancel()]
            var pushToken = PushHandler.Instance.pushToken ?? "";
            var authResponse = await apiClient.AuthenticateAsync(SystemInfo.deviceUniqueIdentifier, pushToken);
            authToken = authResponse.Bearer;
            PlayerPrefs.SetString(AUTH_TOKEN, authToken);
        }

        Debug.Log($"Auth token: {authToken}");

        // this _gameApi can now be used by all other methods
        _gameApi = new WikidataGameAPI(new Uri(SERVER_URL), new TokenCredentials(authToken));
    }
    
    /*
     * This function updates the auth token when a push token is received
     */
    public static async Task UpdateApiConnection(string token)
    {
        /**
         * set current auth token to null to prevent inconsistencies
         */
        PlayerPrefs.SetString(AUTH_TOKEN, null);
        
        /**
         * if game API was not built yet, use standard setup function
         */
        if (_gameApi == null)
        {
            await SetupApiConnection();
            return;
        }
        
        Debug.Log("Rebuilding auth token with newly generated push token");
        
        /**
         * regenerate API and auth process
         */
        
        var apiClient = new WikidataGameAPI(new Uri(SERVER_URL), new TokenCredentials("auth"));
        var pushToken = token;
        var authResponse = await apiClient.AuthenticateAsync(SystemInfo.deviceUniqueIdentifier, pushToken);
        var authToken = authResponse.Bearer;
        
        Debug.Log($"Saving new auth token {authToken} in Player Prefs");
        PlayerPrefs.SetString(AUTH_TOKEN, authToken);
        
        /**
         * reset game api with new auth token
         */
        
        _gameApi = new WikidataGameAPI(new Uri(SERVER_URL), new TokenCredentials(authToken));
    }

    //Creates a game when no previous game was found or joins a game 
    public static async Task CreateOrJoinGame()
    {
        var gameInfo = await _gameApi.CreateNewGameAsync(8, 8, 50);
        _currentGameId = gameInfo.GameId;
        Debug.Log($"Initialized new game with id: {_currentGameId}");
        PlayerPrefs.SetString(CURRENT_GAME_ID, _currentGameId);
    }


    //Checks if a game already exists
    public static async Task<Game> RestorePreviousGame()
    {
        var previousGameId = PlayerPrefs.GetString(CURRENT_GAME_ID);
        if (!string.IsNullOrEmpty(previousGameId))
        {
            try
            {
                _currentGameId = previousGameId;
                var state = await GetCurrentGameState();
                return state;
            }
            catch (Exception e)
            {
                _currentGameId = null;
                Debug.LogError(e);
                Debug.Log($"Game with ID {_currentGameId} could not be restored - deleting from player prefs");
                PlayerPrefs.DeleteKey(CURRENT_GAME_ID);
                return null;

            } 
        }

        return null;
    }

   /**
    * Use this function to create a new minigame by providing a Tile object and the categoryId
    * Question: does this need to be async as well?
    */
    public static async Task<MiniGame> InitializeMinigame(string tileId, string categoryId)
    {
        MiniGameInit init = new MiniGameInit(tileId, categoryId);
        var _minigame = await _gameApi.InitalizeMinigameAsync(_currentGameId, init);
        Debug.Log($"TASK:{_minigame.TaskDescription}");
        Debug.Log($"Started minigame with id {_minigame.Id} on tile {tileId} with category {categoryId}");
        return _minigame;
    }


    /**
     * Use this function to get needed information about the minigame you just created
     */
    public static async Task<MiniGame> RetrieveMinigameInfo(string minigameId)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        var miniGame = await _gameApi.RetrieveMinigameInfoAsync(_currentGameId, minigameId, cts.Token);
        return miniGame;
    }

    /**
    * Use this function to POST answers for a minigame to the backend
    * you are getting a MiniGameResult back, which indicates if the answer was right or wrong
    */
    public static async Task<MiniGameResult> AnswerMinigame(string minigameId, IList<string> answers)
    {
        var result = await _gameApi.AnswerMinigameAsync(_currentGameId, minigameId, answers);
        return result;
    }

    /**
     * Use this function to delete the current game
     * I made this private for now because I am not sure if a player should have the ability to delete a game on his/her own
     */
    public static async Task AbortCurrentGame()
    {
        PlayerPrefs.DeleteKey(CURRENT_GAME_ID);
        await _gameApi.DeleteGameAsync(_currentGameId);
    }


    /**
     * Use this function to get the game state of the current game
     */
    public static async Task<Game> GetCurrentGameState()
    {
        return await _gameApi.RetrieveGameStateAsync(_currentGameId);
    }
}
