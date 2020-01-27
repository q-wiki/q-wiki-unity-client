using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Controllers.Authentication;
using Controllers.Map;
using Controllers.UI;
using Handlers;
using UnityEngine;
using UnityEngine.SceneManagement;
using WikidataGame.Models;

namespace Controllers {
    public class GameManager : Singleton<GameManager> {

        [SerializeField] private GameObject gridPrefab;

        private Game _game;
        private IUIController _uiController;
        private GridController _gridController;
        private bool _isWaitingForOpponent;
        private bool _isWaitingState;
        private bool _isHandling;
        private Scene _currentScene;
        private static ScoreHandler ScoreHandler => ScoreHandler.Instance;
        private static ActionPointHandler ActionPointHandler => ActionPointHandler.Instance;

        private const string IS_WAITING_FOR_OPPONENT = "IS_WAITING_FOR_OPPONENT";
        private const string CURRENT_GAME_BLOCK_TURN_UPDATE = "CURRENT_GAME_BLOCK_TURN_UPDATE";
        private const string REMAINING_ACTION_POINTS = "REMAINING_ACTION_POINTS";

        /// <summary>
        ///     When game controller is enabled, an event handler to handle scene changes is registered.
        /// </summary>
        public void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("Event handler successfully attached to SceneManager.sceneLoaded.");
        }

        /// <summary>
        ///     Update method listens for changes in the game state when it's currently not your turn
        /// </summary>
        public void Update() {

            HandleAppClosing();

            // player is waiting for their turn
            if (_isWaitingState && !_isHandling
                                && _currentScene.name == "GameScene")
                HandleWaitingState();
        }

        /// <summary>
        ///     EventHandler for handling scene changes and initializing several parts of the game accordingly.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) {
            _currentScene = scene;
            var sceneName = _currentScene.name;

            if (sceneName == "OpeningScene")
                return;

            _uiController = GameObject.FindGameObjectWithTag("UIController")
                .GetComponent<IUIController>();

            if (sceneName == "StartScene")
                InitializeStartScene();
            else if (sceneName == "GameScene") InitializeGameScene();
            else throw new Exception($"Current scene is invalid / name: {sceneName}");

        }

        /// <summary>
        ///     This function is used to initialize the start scene of the game.
        /// </summary>
        private void InitializeStartScene() {
            Debug.Log("Start scene successfully set up");

            /*
             * delete action point indicator from player prefs to prevent inconsistencies
             */

            StartUIController startUiController = _uiController as StartUIController;
            if (startUiController == null)
                throw new Exception("StartUIController should not be null at this point.");

            if (PlayerPrefs.GetInt(IS_WAITING_FOR_OPPONENT, 0) == 1)
            {
                startUiController.InitializeGame(false);
            }
        }

        /// <summary>
        ///     This function is used to initialize the game scene of the game.
        /// </summary>
        private async void InitializeGameScene()
        {
            Debug.Log("Game scene was successfully set up.");

            /* setting handling indicator false to prevent inconsistencies */
            _isHandling = false;

            /* setting up grid */

            var grid = Instantiate(gridPrefab);
            _gridController = grid.GetComponent<GridController>();

            /* checking current game state */

            _game = await Communicator.GetCurrentGameState();

            /* when in gamescene, check game over state */

            HandleGameFinishedState();

            /**
             * generate grid by reading tiles from game object
             */
            _gridController.GenerateGrid(_game.Tiles);

            /**
             * update points to show an updated state
             */

            ScoreHandler.SetGameId(_game.Id);
            ScoreHandler.ReadCurrentTurnCountFromPrefs();
            ScoreHandler.UpdatePoints(_game.Tiles, PlayerId(), _game.Opponent.Id);

            /**
             * if current player is not me, go to loop / block interaction
             */

            if (_game.NextMovePlayerId != _game.Me.Id) {
                _isWaitingState = true;
                return;
            }

            /**
             * show action point indicator in UI
             */

            ActionPointHandler.SetGameId(_game.Id);
            ActionPointHandler.RebuildActionPointsFromPrefs();
            if (PlayerPrefs.GetInt($"{_game.Id}/{REMAINING_ACTION_POINTS}", -1) == 0)
                ActionPointHandler.UpdateState(PlayerId(), _game.NextMovePlayerId, true);
            ActionPointHandler.Show();

            /**
             * update turn UI when it is the player's move directly after opening the app
             */

            if (PlayerPrefs.GetInt($"{_game.Id}/{CURRENT_GAME_BLOCK_TURN_UPDATE}", 0) == 0)
                ScoreHandler.UpdateTurns();

            /*
             * highlight possible moves for current player
             */

            _gridController.ShowPossibleMoves(PlayerId());
        }

        /// <summary>
        ///     This function is called when the client wants to start a new game.
        /// </summary>
        public async Task<bool> WaitForOpponent(bool createNewGame) {
            if (createNewGame)
                await Communicator.CreateOrJoinGame(false);
            else
                await Communicator.RestorePreviousGame();

            _game = await Communicator.GetCurrentGameState();

            // indicate that client waits for an opponent
            SetWaitingForOpponent(true);

            // we'll be checking the game state until another player joins
            while (_game.AwaitingOpponentToJoin ?? true)
            {
                if (_isWaitingForOpponent)
                {
                    Debug.Log("Waiting for Opponent.");

                    // wait for 3 seconds
                    await Task.Delay(3000);
                    _game = await Communicator.GetCurrentGameState();
                }
                else
                {
                    Debug.Log("Stop connection to game...");

                    var id = _game.Id;

                    // delete game from backend
                    await Communicator.AbortCurrentGame();
                    _game = null;

                    Debug.Log($"Game initialization of {id} successfully stopped.");

                    return false;
                }
            }

            // another player joined
            Debug.Log("Found opponent, starting game.");
            SetWaitingForOpponent(false);

            // change to game scene
            ChangeToGameScene();

            return true;
        }

        /// <summary>
        /// This function is called when the client wants to create a new game with AI opponent.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CreateNewGameWithAIOpponent()
        {
            var isNewGameCreated = await Communicator.CreateGameWithAiOpponent();
            SetWaitingForOpponent(false);
            ChangeToGameScene();
            return isNewGameCreated;
        }

        /// <summary>
        ///     This function is called when it's not the client's move.
        ///     A block panel is shown to prevent input by the client.
        /// </summary>
        private async void HandleWaitingState() {
            _isHandling = true;

            var gameUiController = _uiController as GameUIController;
            if (gameUiController == null)
                throw new Exception("User has to be in-game for this function to work.");

            /* make blockActionPanel visible and prevent user from selecting anything in the game */
            gameUiController.Block();

#if UNITY_EDITOR
            Debug.Log("Wait for 10 seconds");
            await Task.Delay(10000);

            _game = await Communicator.GetCurrentGameState();

#endif

            /**
             * check if game is over
             * if game is deleted, we assume for now, that the other player deleted it and show the winning screen
             */
            if (_game == null) {
                Debug.Log("Game is null - the other player probably deleted it");
                _uiController.HandleGameFinished(0);
                gameUiController.Unblock();
                return;
            }

            /**
             * check if game is over
             */
            HandleGameFinishedState();

            if (_game.NextMovePlayerId == _game.Me.Id) {
                gameUiController.Unblock();
                _isWaitingState = false;
                RefreshGameState(true);
            }

            _isHandling = false;
        }

        /// <summary>
        ///     This function is used to refresh the current state of the game.
        ///     This is done by receiving current information from the backend.
        ///     Depending on the state, different functions are called.
        /// </summary>
        /// <param name="isNewTurn">Indicates if a new turn started.</param>
        public async void RefreshGameState(bool isNewTurn)
        {
            while (true)
            {
                Debug.Log($"isNewTurn:{isNewTurn}");

                /*
                * this is called whenever something happens (MiniGame finished, player made a turn, etc.)
                */

                _game = await Communicator.GetCurrentGameState();

                /*
                * redraw the grid
                */

                foreach (Transform child in _gridController.transform) Destroy(child.gameObject);
                _gridController.GenerateGrid(_game.Tiles);

                /**
                 * adjust UI to new score
                */

                ScoreHandler.SetGameId(_game.Id);
                ScoreHandler.Show();
                ScoreHandler.UpdatePoints(_game.Tiles, PlayerId(), _game.Opponent.Id);


                /*
                * only for AI bot: if actions points are zero, but client has the next move, manual update
                */

                if (_game.Opponent.Id == "ffffffff-ffff-ffff-ffff-ffffffffffff")
                {
                    if (isNewTurn == false 
                        && PlayerPrefs.GetInt($"{_game.Id}/{REMAINING_ACTION_POINTS}", -1) == 1 &&
                        _game.NextMovePlayerId == _game.Me.Id)
                    {
                        isNewTurn = true;
                        continue;
                    }
                }

                /**
                * if current update happens in a new turn, update turn count
                */

                if (isNewTurn) ScoreHandler.UpdateTurns();


                /**
                * simple function to update action points in game controller
                */

                ActionPointHandler.SetGameId(_game.Id);
                ActionPointHandler.RebuildActionPointsFromPrefs();
                ActionPointHandler.Instance.UpdateState(PlayerId(), _game.NextMovePlayerId, isNewTurn);

                /**
                * check for game over
                */

                HandleGameFinishedState();

                /*
                * highlight possible moves for current player
                * */

                _gridController.ShowPossibleMoves(PlayerId());


                /*
                * if current player is not me, go to loop / block interaction
                */

                if (_game.NextMovePlayerId != _game.Me.Id) _isWaitingState = true;
                break;
            }
        }

        /// <summary>
        ///     This function is used to handle the end of the game.
        ///     An appropriate message is shown to the user depending on the outcome.
        /// </summary>
        private void HandleGameFinishedState() {
            if ( _game.WinningPlayerIds == null || _game.WinningPlayerIds.Count <= 0) return;
            
            Debug.Log("Checking state of the finished game...");

            short state;

            if (_game.WinningPlayerIds.Count == 1 && _game.WinningPlayerIds.Contains(_game.Me.Id))
                state = 0;
            else if (_game.WinningPlayerIds.Count == 1 && _game.WinningPlayerIds.Contains(_game.Opponent.Id))
                state = 1;
            else if (_game.WinningPlayerIds.Count == 2 && _game.WinningPlayerIds.Contains(_game.Opponent.Id) &&
                     _game.WinningPlayerIds.Contains(_game.Me.Id))
                state = 2;
            else throw new Exception("This game state is illegal.");

            AccountController.PostScore(ScoreHandler.Instance.playerScore);
            AccountController.AddGameToHistory(_game.Opponent, (int)ScoreHandler.Instance.playerScore, (int)ScoreHandler.Instance.opponentScore);

            _uiController.HandleGameFinished(state);
        }

        /// <summary>
        ///     Initialize a new MiniGame by using the Communicator.
        /// </summary>
        /// <param name="tileId">The respective tile ID</param>
        /// <param name="categoryId">The respective category ID</param>
        /// <returns>The initialized MiniGame</returns>

        public async Task<MiniGame> InitializeMinigame(string tileId, string categoryId) {
            return await Communicator.InitializeMinigame(tileId, categoryId);
        }

        /// <summary>
        ///     This function is used to delete / leave a game.
        /// </summary>
        public async Task LeaveGame() {
            LoadingIndicator.Instance.Show();
            if (_game == null) {
                Debug.Log("Game was already deleted.");
            }
            else {
                Debug.Log($"Trying to delete game {_game.Id}.");
                AccountController.PostScore(ScoreHandler.Instance.playerScore);
                AccountController.AddGameToHistory(_game.Opponent, 0, (int)ScoreHandler.Instance.opponentScore);
                await Communicator.AbortCurrentGame();
                Debug.Log("Game was successfully deleted.");
            }

            LoadingIndicator.Instance.Hide();
            _isWaitingState = false;
            Debug.Log("Returning to start scene...");
            ChangeToStartScene();
        }

        /// <summary>
        ///     This function is used to delete / leave a game after it has ended.
        /// </summary>
        public async Task LeaveGameWithoutConceding() {
            LoadingIndicator.Instance.Show();
            if (_game == null) {
                Debug.Log("Game was already deleted.");
            }
            else {
                Debug.Log($"Trying to delete game {_game.Id}.");
                await Communicator.AbortCurrentGame();
                Debug.Log("Game was successfully deleted.");
            }

            LoadingIndicator.Instance.Hide();
            _isWaitingState = false;
            Debug.Log("Returning to start scene...");
            ChangeToStartScene();
        }

        /// <summary>
        ///     This function is used to load the StartScene of the game.
        /// </summary>
        public void ChangeToStartScene() {
            SceneManager.LoadScene("StartScene");
        }

        /// <summary>
        ///     Use this to load the GameScene.
        /// </summary>
        public void ChangeToGameScene() {
            SceneManager.LoadScene("GameScene");
        }

        /// <summary>
        ///     This function is called when the client taps the 'Home' button on Android.
        /// </summary>
        private void HandleAppClosing() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                var interactionController = InteractionController.Instance;
                    if (interactionController != null
                        && interactionController.HasActiveMinigamePanel()) {
                        ErrorHandler.Instance.Error("App cannot be shutdown because there is still an active MiniGame.");
                        return;
                    }

                    Debug.Log("Closing App...");
                    Application.Quit();
            }
        }

        /// <summary>
        ///     This function is used to indicate if client is currently searching for an opponent.
        /// </summary>
        /// <param name="isWaitingForOpponent">Is client waiting for opponent?</param>
        public void SetWaitingForOpponent(bool isWaitingForOpponent) {
            _isWaitingForOpponent = isWaitingForOpponent;
            PlayerPrefs.SetInt(IS_WAITING_FOR_OPPONENT,
                isWaitingForOpponent ? 1 : 0);
        }
        
        /// <summary>
        /// Checks if client is waiting for opponent
        /// </summary>
        /// <returns>If client is waiting for opponent</returns>
        public bool IsWaitingForOpponent()
        {
            return _isWaitingForOpponent;
        }

        /// <summary>
        /// Use this to request a rematch while in the current game
        /// </summary>
        public async Task RequestRematch() {
            await Communicator.ChallengeUser(Opponent().Id);
        }

        /// <summary>
        /// Use this function to add a friend via the API
        /// </summary>
        /// <param name="id">ID of the player to add</param>
        public async void AddFriend(string id) {
            await Communicator.AddFriend(id);
        }

        /// <summary>
        /// Use this function to remove a friend via the API
        /// </summary>
        /// <param name="id">ID of the player to delete</param>
        public async void DeleteFriend(string id) {
            await Communicator.DeleteFriend(id);
        }

        /// <summary>
        /// Indicates if user with id is in client's friend list
        /// </summary>
        /// <param name="id">Id of another user</param>
        /// <returns>if user is in client's friend list</returns>
        public async Task<bool> IsFriend(string id) {
            var friends = await Communicator.RetrieveFriends();
            return friends
                .Select(f => f.Id).Contains(id);
        }

        /// <summary>
        /// This is used to report a user.
        /// </summary>
        /// <param name="userId">ID of the user</param>
        public void ReportUser(string userId, string userName)
        {
            Debug.Log($"Reporting user {userId} with user name {userName} to the organization.");
            HelperMethods.SendEmail(userId, userName);
        }

        /// <summary>
        /// Checks if turn handler needs to be blocked from upping turn count when game is loaded the next time
        /// </summary>
        public void CheckTurnStatusForScoreHandler()
        {
            if (_game != null)
            {
                if (_game.NextMovePlayerId == _game.Me.Id)
                    PlayerPrefs.SetInt($"{_game.Id}/{CURRENT_GAME_BLOCK_TURN_UPDATE}", 1);
                else
                    PlayerPrefs.SetInt($"{_game.Id}/{CURRENT_GAME_BLOCK_TURN_UPDATE}", 0);
            }
        }

        /// <summary>
        /// Cancel waiting state for current game.
        /// </summary>
        public void CancelWaiting()
        {
            _isWaitingState = false;
        }

        /// <summary>
        ///     Return the player id of the client if existent.
        /// </summary>
        /// <returns>The id of the client.</returns>
        public string PlayerId() {
            return _game?.Me.Id;
        }

        /// <summary>
        ///     Return the player id of the opponent if existent.
        /// </summary>
        /// <returns>The id of the opponent.</returns>
        public Player Opponent() {
            return _game?.Opponent;
        }

        /// <summary>
        ///     Returns the current UIController.
        /// </summary>
        /// <returns>the current UI controller</returns>
        public IUIController UIController() {
            return _uiController;
        }

        /// <summary>
        ///     Returns the current GridController.
        /// </summary>
        /// <returns>the current GridController</returns>
        public GridController GridController() {
            return _gridController;
        }


#if UNITY_EDITOR
        /// <summary>
        ///     In the Unity Editor:
        ///     Reset turn update values when application is shut down.
        ///     If application is quit while user is searching for an opponent, keep on searching.
        /// </summary>
        public void OnApplicationQuit() {
            CheckTurnStatusForScoreHandler();
        }
#endif

#if UNITY_ANDROID

        /// <summary>
        ///     This function can be called on android phones to manually update the game state in the game manager.
        /// </summary>
        public void Android_RefreshGame(string gameId) 
        {
            if (_game != null && _game.Id == gameId)
            {
                Debug.Log($"Refreshing game state of game with id {gameId}");
                RefreshGameState(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameId"></param>
        public void Android_GameWon(string gameId)
        {
            if (_game != null && _game.Id == gameId)
            {
                Debug.Log($"Handling won game with id {gameId}");
                AccountController.PostScore(ScoreHandler.Instance.playerScore);
                AccountController.AddGameToHistory(_game.Opponent, (int)ScoreHandler.Instance.playerScore, (int)ScoreHandler.Instance.opponentScore);
                _uiController.HandleGameFinished(0);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameId"></param>
        public void Android_GameDraw(string gameId)
        {
            if (_game != null && _game.Id == gameId)
            {
                Debug.Log($"Handling draw game with id {gameId}");
                _uiController.HandleGameFinished(1);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameId"></param>
        public void Android_GameLost(string gameId)
        {
            if (_game != null && _game.Id == gameId)
            {
                Debug.Log($"Handling lost game with id {gameId}");
                AccountController.PostScore(ScoreHandler.Instance.playerScore);
                AccountController.AddGameToHistory(_game.Opponent, (int)ScoreHandler.Instance.playerScore, (int)ScoreHandler.Instance.opponentScore);
                _uiController.HandleGameFinished(2);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Android_RequestReceived()
        {
            Debug.Log("You received a new game request.");
        }

        /// <summary>
        ///     On an Android phone:
        ///     Reset turn update values when application is shut down.
        ///     If application is quit while user is searching for an opponent, keep on searching.
        /// </summary>
        public void OnApplicationPause() {
            CheckTurnStatusForScoreHandler();
        }
#endif
    }
}