using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Controllers.Authentication;
using Microsoft.Rest;
using UnityEngine;
using WikidataGame;
using WikidataGame.Models;

/// <summary>
///     The communicator acts as a connector between backend and frontend.
///     All functions are static for now to ensure simple usage
/// </summary>
public class Communicator : MonoBehaviour
{
    public const string USERNAME_TAKEN_ERROR_MESSAGE = "Username is already taken";
    public const string USERNAME_TOO_SHORT_MESSAGE = "Username must have at least 3 characters";
    public const string INVALID_CHARACTERS_ERROR_MESSAGE = "User name can only contain letters or digits.";
    public const string SUCCESS_MESSAGE = "SignIn successful";
    public const string PLAYERPREFS_AUTH_TOKEN = "AUTH_TOKEN";
    public const string PLAYERPREFS_AUTH_EXPIRY = "AUTH_EXPIRY";
    public const string PLAYERPREFS_USERNAME = "USERNAME";
    public const string PLAYERPREFS_PASSWORD = "PASSWORD";
    public const string PLAYERPREFS_SIGNIN_METHOD = "SIGNIN_METHOD";
    internal const string PLAYERPREFS_CURRENT_GAME_ID = "CURRENT_GAME_ID";
    private static WikidataGameAPI _gameApi;
    private static string _currentGameId;
    private static string _authToken { get; set; }
    private static string SERVER_URL => Configuration.Instance.ServerURL;

    /// <summary>
    ///     Indicates if client is connected to WikiData API
    /// </summary>
    /// <returns>if client is connected</returns>
    public static bool IsConnected()
    {
        return _gameApi != null;
    }

    /// <summary>
    ///     This function is used to authenticate the client within the API.
    ///     If something goes wrong, an error message is printed to the console.
    /// </summary>
    /// <param name="userName">Provided user name</param>
    /// <param name="password">Provided password</param>
    /// <param name="pushToken">Provided pushToken</param>
    /// <returns>AuthToken of the client</returns>
    public static async Task<string> Authenticate(string userName, string password, string pushToken, string method)
    {
        Debug.Log($"Push token is {pushToken}");
        Debug.Log($"Password is {password}");
        Debug.Log($"Username is {userName}");
        
        var apiClient = new WikidataGameAPI(new Uri(SERVER_URL), new TokenCredentials("auth"));
        
        try
        {
            HttpOperationResponse<AuthInfo> response = null;
            if (method == SignInController.method_anonymous) {
                response = await apiClient.AuthenticateWithHttpMessagesAsync(userName, password, pushToken);
                SignInController.isLoggedInAnon = true;
            }
            else if (method == SignInController.method_google) {
                response = await apiClient.AuthenticateGooglePlayWithHttpMessagesAsync(userName, password, pushToken);
                SignInController.isLoggedInGoogle = true;
            }
            else {
                Debug.LogError("Not a valid SignIn method");
            }
            var authResponse = response.Body;
            SignInController.authInfo = authResponse;
            Debug.Log($"Saving new auth token {authResponse.Bearer} in Player Prefs");
            PlayerPrefs.SetString(PLAYERPREFS_AUTH_TOKEN, authResponse.Bearer);
            PlayerPrefs.SetString(PLAYERPREFS_AUTH_EXPIRY, authResponse.Expires.Value.ToBinary().ToString());
            PlayerPrefs.SetString(PLAYERPREFS_SIGNIN_METHOD, method);
            
            return authResponse.Bearer;
        }
        catch (HttpOperationException e)
        {
            var response = e.Response;
            Debug.LogError(
                $"Error while trying to connect to API: {response.StatusCode} ({(int) response.StatusCode}) / {e.Response.Content}");
            Debug.LogError(e.StackTrace);

            if (e.Response.Content == $"User name 'anon-{userName}' is already taken." || response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                return USERNAME_TAKEN_ERROR_MESSAGE;
            }
            else if(response.Content.Contains("is invalid, can only contain letters or digits.")) {
                return INVALID_CHARACTERS_ERROR_MESSAGE;
            }
            else if (response.Content == "Username must have at least 3 characters"){
                return USERNAME_TOO_SHORT_MESSAGE;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadGateway){
                return USERNAME_TAKEN_ERROR_MESSAGE;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.OK){
                return SUCCESS_MESSAGE;
            }
            Debug.LogError("Response could not be interpreted correctly.");
            return null;
        }
    }

    /// <summary>
    ///     This function checks whether we have an auth token and restores it;
    ///     if not, it will request a new token from the server
    ///     This means: Before you do anything else, call this method.
    /// </summary>
    /// <returns>if client is successfully connected to the API</returns>
    public static async Task<bool> SetupApiConnection()
    {
        // have we already set up the api connection?
        if (_gameApi != null) return true;

        // do we have an auth token that's saved? Is it expired or still valid?
        Debug.Log("Trying to restore previously saved auth token…");
        var authToken = PlayerPrefs.GetString(PLAYERPREFS_AUTH_TOKEN);
        string userName = PlayerPrefs.GetString(PLAYERPREFS_USERNAME);
        string password = PlayerPrefs.GetString(PLAYERPREFS_PASSWORD);
        string method = PlayerPrefs.GetString(PLAYERPREFS_SIGNIN_METHOD);
        DateTime expiryDate = string.IsNullOrEmpty(PlayerPrefs.GetString(PLAYERPREFS_AUTH_EXPIRY)) ? 
            DateTime.Now : DateTime.FromBinary(Convert.ToInt64(PlayerPrefs.GetString(PLAYERPREFS_AUTH_EXPIRY)));
        bool token_expired = expiryDate < DateTime.Now;

        if (string.IsNullOrEmpty(authToken)){
            Debug.Log("No auth token in PlayerPrefs, user will have to authenticate before playing");
            return false;
        }
        else if (token_expired  && userName != null && password != null) {
            /**
             * Re-Authenticate (Update Auth Token)
             */
            Debug.Log("Token expired. Trying to re-authenticate...");
            var pushToken = PushHandler.Instance.pushToken ?? "";
            if(method == SignInController.method_anonymous) {
                authToken = await Authenticate(
                    userName,
                    password,
                    pushToken,
                    method);
            }
            else if (method == SignInController.method_google) {
                SignInController.reauthenticateWithGoogle = true;
            }

            if (authToken == null) return false;
        }
        else {
            if(method == SignInController.method_anonymous) {
                SignInController.isLoggedInAnon = true;
                Debug.Log("Already Signed In anonymously");
            }
            else if(method == SignInController.method_google) {
                SignInController.isLoggedInGoogle = true;
                Debug.Log("Already Signed In with Google");
            }
        }
        
        Debug.Log($"Auth token: {authToken}");

        // this _gameApi can now be used by all other methods
        _gameApi = new WikidataGameAPI(new Uri(SERVER_URL), new TokenCredentials(authToken));

        return true;

    }

    /// <summary>
    ///     This function updates the auth token when a push token is received
    /// </summary>
    /// <param name="token">Authentication token</param>
    /// <returns>if client is successfully connected to the API</returns>
    public static async Task<bool> UpdateApiConnection(string token)
    {

        /**
         * if game API was not built yet, use standard setup function
         */
        if (_gameApi == null)
        {
            return await SetupApiConnection();
        }

        Debug.Log("Rebuilding auth token with newly generated push token");

        /**
         * regenerate API and auth process
         */

        string username = PlayerPrefs.GetString(PLAYERPREFS_USERNAME);
        string method = PlayerPrefs.GetString(PLAYERPREFS_SIGNIN_METHOD);
        string password = PlayerPrefs.GetString(PLAYERPREFS_PASSWORD);

        var authToken = await Authenticate(
            username,
            password,
            token,
            method);

        if (authToken == null) return false;

        /**
         * reset game api with new auth token
         */

        _gameApi = new WikidataGameAPI(new Uri(SERVER_URL), new TokenCredentials(authToken));
        return true;
    }

    /// <summary>
    ///     Retrieves all open game requests of the player that is currently logged in
    /// </summary>
    /// <returns>asynchronous Task</returns>
    internal static async Task<GameRequestList> RetrieveGameRequests()
    {
        Debug.Log("Retrieving Open Game Requests");

        try
        {
            HttpOperationResponse<GameRequestList> response = null;
            response = await _gameApi.GetGameRequestsWithHttpMessagesAsync();
            var gameRequestList = response.Body;

            return gameRequestList;
        }
        catch (HttpOperationException e)
        {
            var response = e.Response;
            Debug.LogError(
                $"Error while trying to connect to API: {response.StatusCode} ({(int)response.StatusCode}) / {e.Response.Content}");
            Debug.LogError(e.StackTrace);
            return null;
        }
    }

    /// <summary>
    ///     Send a game request to a user
    /// </summary>
    /// <returns>asynchronous Task</returns>
    internal static async Task<GameRequest> ChallengeUser(string userID)
    {
        Debug.Log($"Sending challenge to user {userID}");

        try
        {
            HttpOperationResponse<GameRequest> response = null;
            response = await _gameApi.RequestMatchWithHttpMessagesAsync(userID);
            var request = response.Body;

            return request;
        }
        catch (HttpOperationException e)
        {
            var response = e.Response;
            Debug.LogError(
                $"Error while trying to connect to API: {response.StatusCode} ({(int)response.StatusCode}) / {e.Response.Content}");
            Debug.LogError(e.StackTrace);
            return null;
        }
    }

    /// <summary>
    ///     Deletes an outgoing or incoming game request
    /// </summary>
    /// <returns>asynchronous Task</returns>
    internal static async Task<bool> DeleteGameRequest(string requestID)
    {
        Debug.Log($"Deleting Game Request {requestID}");

        try
        {
            HttpOperationResponse response = null;
            response = await _gameApi.DeleteGameRequestWithHttpMessagesAsync(requestID);

            return true;
        }
        catch (HttpOperationException e)
        {
            var response = e.Response;
            Debug.LogError(
                $"Error while trying to connect to API: {response.StatusCode} ({(int)response.StatusCode}) / {e.Response.Content}");
            Debug.LogError(e.StackTrace);
            return false;
        }
    }

    /// <summary>
    ///     Accepts an incoming game request
    /// </summary>
    /// <returns>asynchronous Task</returns>
    internal static async Task<GameInfo> AcceptGameRequest(string requestID)
    {
        Debug.Log($"Accepting Game Request {requestID}");
        try
        {
            HttpOperationResponse<GameInfo> response = null;
            response = await _gameApi.CreateNewGameByRequestWithHttpMessagesAsync(requestID);

            return response.Body;
        }
        catch (HttpOperationException e)
        {
            var response = e.Response;
            Debug.LogError(
                $"Error while trying to connect to API: {response.StatusCode} ({(int)response.StatusCode}) / {e.Response.Content}");
            Debug.LogError(e.StackTrace);
            return null;
        }
    }


    /// <summary>
    ///     Retrieves users (limit 10) with a username similar to the query string
    /// </summary>
    /// <returns>asynchronous Task</returns>
    public static async Task<IList<Player>> FindUsers(string userName) {
        Debug.Log($"Searching for user: {userName}");

        try {
            HttpOperationResponse<IList<Player>> response = null;
            response = await _gameApi.GetFindFriendsWithHttpMessagesAsync(userName);
            var userResponse = response.Body;

            return userResponse;
        }
        catch (HttpOperationException e) {
            var response = e.Response;
            Debug.LogError(
                $"Error while trying to connect to API: {response.StatusCode} ({(int)response.StatusCode}) / {e.Response.Content}");
            Debug.LogError(e.StackTrace);
            return null;
        }
    }

    /// <summary>
    ///     Retrieves all friends of the player that is currently logged in
    /// </summary>
    /// <returns>asynchronous Task</returns>
    internal static async Task<IList<Player>> RetrieveFriends() {
        Debug.Log("Retrieving Friend List");

        try {
            HttpOperationResponse<IList<Player>> response = null;
            response = await _gameApi.GetFriendsWithHttpMessagesAsync();
            var friendList = response.Body;

            return friendList;
        }
        catch (HttpOperationException e) {
            var response = e.Response;
            Debug.LogError(
                $"Error while trying to connect to API: {response.StatusCode} ({(int)response.StatusCode}) / {e.Response.Content}");
            Debug.LogError(e.StackTrace);
            return null;
        }
    }

    /// <summary>
    ///     Retrieves all games of the player that is currently logged in
    /// </summary>
    /// <returns>asynchronous Task</returns>
    internal static async Task<IList<GameInfo>> RetrieveGames() {
        Debug.Log("Retrieving Game List");

        try {
            HttpOperationResponse<IList<GameInfo>> response = null;
            response = await _gameApi.GetGamesWithHttpMessagesAsync();
            var gameList = response.Body;

            return gameList;
        }
        catch (HttpOperationException e) {
            var response = e.Response;
            Debug.LogError(
                $"Error while trying to connect to API: {response.StatusCode} ({(int)response.StatusCode}) / {e.Response.Content}");
            Debug.LogError(e.StackTrace);
            return null;
        }
    }

    /// <summary>
    ///     Deletes the specified game
    /// </summary>
    /// <returns>asynchronous Task</returns>
    internal static async Task<bool> DeleteGame(string gameId) {
        Debug.Log("Deleting Game");

        try {
            HttpOperationResponse response = null;
            response = await _gameApi.DeleteGameWithHttpMessagesAsync(gameId);

            return true;
        }
        catch (HttpOperationException e) {
            var response = e.Response;
            Debug.LogError(
                $"Error while trying to connect to API: {response.StatusCode} ({(int)response.StatusCode}) / {e.Response.Content}");
            Debug.LogError(e.StackTrace);
            return false;
        }
    }

    /// <summary>
    ///     Adds user as a friend
    /// </summary>
    /// <returns>asynchronous Task</returns>
    internal static async Task<Player> AddFriend(string userID) {
        Debug.Log("Retrieving Friend List");

        try {
            HttpOperationResponse<Player> response = null;
            response = await _gameApi.PostFriendWithHttpMessagesAsync(userID);
            var player = response.Body;

            return player;
        }
        catch (HttpOperationException e) {
            var response = e.Response;
            Debug.LogError(
                $"Error while trying to connect to API: {response.StatusCode} ({(int)response.StatusCode}) / {e.Response.Content}");
            Debug.LogError(e.StackTrace);
            return null;
        }
    }

    /// <summary>
    ///     Adds user as a friend
    /// </summary>
    /// <returns>asynchronous Task</returns>
    internal static async Task<Player> DeleteFriend(string userID) {
        Debug.Log("Retrieving Friend List");

        try {
            HttpOperationResponse<Player> response = null;
            response = await _gameApi.DeleteFriendWithHttpMessagesAsync(userID);
            var player = response.Body;

            return player;
        }
        catch (HttpOperationException e) {
            var response = e.Response;
            Debug.LogError(
                $"Error while trying to connect to API: {response.StatusCode} ({(int)response.StatusCode}) / {e.Response.Content}");
            Debug.LogError(e.StackTrace);
            return null;
        }
    }

    /// <summary>
    ///     Creates a game when no previous game was found or joins a game
    /// </summary>
    /// <returns>asynchronous Task</returns>
    public static async Task<bool> CreateOrJoinGame()
    {
        try
        {
            var response = await _gameApi.CreateNewGameWithHttpMessagesAsync();
            var gameInfo = response.Body;
            _currentGameId = gameInfo.GameId;
            Debug.Log($"Initialized new game with id: {_currentGameId}");
            PlayerPrefs.SetString(PLAYERPREFS_CURRENT_GAME_ID, _currentGameId.ToString());
            return true;
        }
        catch (HttpOperationException e)
        {
            var response = e.Response;
            Debug.LogError(
                $"Error while trying to create or join a game: {response.StatusCode} ({(int) response.StatusCode}) / {e.Response.Content}");
            Debug.LogError(e.StackTrace);
            return false;
        }
    }

    /// <summary>
    ///     Checks if a game already exists
    /// </summary>
    /// <returns>previous game if it exists, otherwise null</returns>
    public static async Task<Game> RestorePreviousGame()
    {
        var previousGameId = PlayerPrefs.GetString(PLAYERPREFS_CURRENT_GAME_ID);
        if (!string.IsNullOrEmpty(previousGameId))
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
                PlayerPrefs.DeleteKey(PLAYERPREFS_CURRENT_GAME_ID);
                return null;
            }

        return null;
    }

    /// <summary>
    ///     Use this function to create a new minigame by providing a Tile object and the categoryId
    /// </summary>
    /// <param name="tileId">ID of current tile</param>
    /// <param name="categoryId">ID of selected category</param>
    /// <returns>new MiniGame</returns>
    public static async Task<MiniGame> InitializeMinigame(string tileId, string categoryId)
    {
        if (_currentGameId == null)
            throw new Exception("Client is not part of any game.");
        
        var init = new MiniGameInit(tileId, categoryId);
        var minigame = await _gameApi.InitalizeMinigameAsync(_currentGameId, init);
        Debug.Log($"TASK:{minigame.TaskDescription}");
        Debug.Log($"Started minigame with id {minigame.Id} on tile {tileId} with category {categoryId}");
        return minigame;
    }

    /// <summary>
    ///     Use this function to get needed information about the minigame you just created
    /// </summary>
    /// <param name="minigameId">ID of the current MiniGame</param>
    /// <returns>MiniGame reference</returns>
    public static async Task<MiniGame> RetrieveMinigameInfo(string minigameId)
    {
        if (_currentGameId == null)
            throw new Exception("Client is not part of any game.");
        
        var cts = new CancellationTokenSource();
        var miniGame = await _gameApi.RetrieveMinigameInfoAsync(_currentGameId, minigameId, cts.Token);
        return miniGame;
    }

    /// <summary>
    ///     Use this function to POST answers for a minigame to the backend
    ///     you are getting a MiniGameResult back, which indicates if the answer was right or wrong
    /// </summary>
    /// <param name="minigameId">ID of the current MiniGame</param>
    /// <param name="answers">One or more answers</param>
    /// <returns>result of the MiniGame</returns>
    public static async Task<MiniGameResult> AnswerMinigame(string minigameId, IList<string> answers)
    {
        if (_currentGameId == null)
            throw new Exception("Client is not part of any game.");
        
        var result = await _gameApi.AnswerMinigameAsync(_currentGameId, minigameId, answers);
        return result;
    }

    /// <summary>
    ///     Use this function to delete the current game
    /// </summary>
    /// <returns>asynchronous Task</returns>
    public static async Task AbortCurrentGame()
    {
        if (_currentGameId == null)
            throw new Exception("Client is not part of any game.");
        
        PlayerPrefs.DeleteKey(PLAYERPREFS_CURRENT_GAME_ID);
        await _gameApi.DeleteGameAsync(_currentGameId);
    }

    /// <summary>
    ///     Use this function to get the game state of the current game
    /// </summary>
    /// <returns>state of the current game</returns>
    public static async Task<Game> GetCurrentGameState()
    {
        if (_currentGameId == null)
            throw new Exception("Client is not part of any game.");
        
        return await _gameApi.RetrieveGameStateAsync(_currentGameId);
    }

    public static void SetCurrentGameId(string currentGameId)
    {
        _currentGameId = currentGameId;
        PlayerPrefs.SetString(PLAYERPREFS_CURRENT_GAME_ID, currentGameId);
    }
}