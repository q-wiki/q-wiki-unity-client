using System;
using System.Threading;
using System.Threading.Tasks;
using Controllers.Map;
using Controllers.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using WikidataGame.Models;

namespace Controllers
{
    public class GameManager : Singleton<GameManager>
    {
        
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
        private const string CURRENT_GAME_TURNS_PLAYED = "CURRENT_GAME_TURNS_PLAYED";

        /// <summary>
        ///     When game controller is enabled, an event handler to handle scene changes is registered.
        /// </summary>
        public void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("Event handler successfully attached to SceneManager.sceneLoaded.");
        }

        /// <summary>
        ///     Update method listens for changes in the game state when it's currently not your turn
        /// </summary>
        public void Update()
        {

            HandleAppClosing();
            
            // player is waiting for their turn
            if (_isWaitingState && !_isHandling
                && _currentScene.name == "GameScene")
                HandleWaitingState();
        }

        /// <summary>
        ///     EventHandler for handling scene changes and initializing several parts of the game accordingly.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
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
        private void InitializeStartScene()
        {
            Debug.Log("Start scene successfully set up");

            /*
             * delete action point indicator from player prefs to prevent inconsistencies
             */

            PlayerPrefs.DeleteKey(REMAINING_ACTION_POINTS);
            PlayerPrefs.DeleteKey(CURRENT_GAME_TURNS_PLAYED);

            if (PlayerPrefs.GetInt(IS_WAITING_FOR_OPPONENT, 0) == 1) 
                WaitForOpponent(false);
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

            ScoreHandler.UpdatePoints(_game.Tiles, PlayerId(), _game.Opponent.Id);

            /**
             * if current player is not me, go to loop / block interaction
             */

            if (_game.NextMovePlayerId != _game.Me.Id)
            {
                _isWaitingState = true;
                return;
            }

            /**
             * show action point indicator in UI
             */

            if (PlayerPrefs.GetInt(REMAINING_ACTION_POINTS, -1) == 0)
                ActionPointHandler.UpdateState(PlayerId(), _game.NextMovePlayerId, true);
            ActionPointHandler.Show();

            /**
             * update turn UI when it is the player's move directly after opening the app
             */

            if (PlayerPrefs.GetInt(CURRENT_GAME_BLOCK_TURN_UPDATE, 0) == 0)
                ScoreHandler.UpdateTurns();

            /*
             * highlight possible moves for current player
             */

            _gridController.ShowPossibleMoves(PlayerId());
            
        }
        
        /// <summary>
        ///     This function is called when the client wants to start a new game.
        /// </summary>
        public async Task<bool> WaitForOpponent(bool createNewGame)
        {
            if (createNewGame)
                await Communicator.CreateOrJoinGame();
            else
                await Communicator.RestorePreviousGame();

            _game = await Communicator.GetCurrentGameState();

            // indicate that client waits for an opponent
            SetWaitingForOpponent(true);

            // we'll be checking the game state until another player joins
            while (_game.AwaitingOpponentToJoin ?? true)
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

            // another player joined 
            Debug.Log("Found opponent, starting game.");
            SetWaitingForOpponent(false);

            // change to game scene
            ChangeToGameScene();
            
            return true;
        }

        /// <summary>
        ///     This function is called when it's not the client's move.
        ///     A block panel is shown to prevent input by the client.
        /// </summary>
        private async void HandleWaitingState()
        {
            _isHandling = true;

            var gameUiController = _uiController as GameUIController;
            if(gameUiController == null)
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
            if (_game == null)
            {
                Debug.Log("Game is null - the other player probably deleted it");
                _uiController.HandleGameFinished(0);
                gameUiController.Unblock();
                return;
            }

            /**
             * check if game is over
             */
            HandleGameFinishedState();

            if (_game.NextMovePlayerId == _game.Me.Id)
            {
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

            ScoreHandler.Instance.Show();
            ScoreHandler.Instance.UpdatePoints(_game.Tiles, PlayerId(), _game.Opponent.Id);

            /**
             * if current update happens in a new turn, update turn count
             */

            if (isNewTurn)
                ScoreHandler.Instance.UpdateTurns();


            /**
             * simple function to update action points in game controller
             */

            ActionPointHandler.Instance.UpdateState(PlayerId(), _game.NextMovePlayerId, isNewTurn);

            /**
            * check for game over
            */

            HandleGameFinishedState();

            /*
             * highlight possible moves for current player
             * */

            _gridController.ShowPossibleMoves(PlayerId());


            /**
             * if current player is not me, go to loop / block interaction
             */

            if (_game.NextMovePlayerId != _game.Me.Id) _isWaitingState = true;
        }

        /// <summary>
        ///     This function is used to handle the end of the game.
        ///     An appropriate message is shown to the user depending on the outcome.
        /// </summary>
        private void HandleGameFinishedState()
        {
            if (_game.WinningPlayerIds != null && _game.WinningPlayerIds.Count > 0)
            {
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
                
                _uiController.HandleGameFinished(state);
            }
        }
        
        /// <summary>
        ///     Initialize a new MiniGame by using the Communicator.
        /// </summary>
        /// <param name="tileId">The respective tile ID</param>
        /// <param name="categoryId">The respective category ID</param>
        /// <returns>The initialized MiniGame</returns>
        public async Task<MiniGame> InitializeMinigame(Guid? tileId, Guid? categoryId)
        {
            return await Communicator.InitializeMinigame(tileId, categoryId);
        }
        
        /// <summary>
        ///     This function is used to delete / leave a game.
        /// </summary>
        public async Task LeaveGame()
        {
            LoadingIndicator.Instance.Show();
            if (_game == null)
            {
                Debug.Log("Game was already deleted.");
            }
            else
            {
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
        public void ChangeToStartScene()
        {
            SceneManager.LoadScene("StartScene");
        }
        
        /// <summary>
        ///     Use this to load the GameScene.
        /// </summary>
        public void ChangeToGameScene()
        {
            SceneManager.LoadScene("GameScene");
        }
        
        /// <summary>
        ///     This function is called when the client taps the 'Home' button on Android.
        /// </summary>
        private void HandleAppClosing()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!_uiController.AreSettingsVisible()) _uiController.ToggleSettings();
                else
                {
                    var interactionController = InteractionController.Instance;
                    if (interactionController != null
                        && interactionController.HasActiveMinigamePanel())
                    {
                        Debug.LogWarning("App is not closed because there is still an active MiniGame.");
                        return;
                    }
                    
                    Debug.Log("Closing App...");
                    Application.Quit();
                    
                }
            }
        }

        /// <summary>
        ///     This function is used to indicate if client is currently searching for an opponent.
        /// </summary>
        /// <param name="isWaitingForOpponent">Is client waiting for opponent?</param>
        public void SetWaitingForOpponent(bool isWaitingForOpponent)
        {
            _isWaitingForOpponent = isWaitingForOpponent;
            PlayerPrefs.SetInt(IS_WAITING_FOR_OPPONENT, 
                isWaitingForOpponent ? 1 : 0);
        }

        /// <summary>
        ///     Return the player id of the client if existent.
        /// </summary>
        /// <returns>The id of the client.</returns>
        public Guid? PlayerId()
        {
            return _game?.Me.Id;
        }

        /// <summary>
        ///     Returns the current UIController.
        /// </summary>
        /// <returns>the current UI controller</returns>
        public IUIController UIController()
        {
            return _uiController;
        }

        /// <summary>
        ///     Returns the current GridController.
        /// </summary>
        /// <returns>the current GridController</returns>
        public GridController GridController()
        {
            return _gridController;
        }
        
        
#if UNITY_EDITOR
        /// <summary>
        ///     In the Unity Editor:
        ///     Reset turn update values when application is shut down.
        ///     If application is quit while user is searching for an opponent, keep on searching.
        /// </summary>
        public async void OnApplicationQuit()
        {
            if (_game != null)
            {
                if (_game.NextMovePlayerId == _game.Me.Id)
                    PlayerPrefs.SetInt(CURRENT_GAME_BLOCK_TURN_UPDATE, 1);
                else
                    PlayerPrefs.SetInt(CURRENT_GAME_BLOCK_TURN_UPDATE, 0);
            }
        }
#endif

#if UNITY_ANDROID
        
        /// <summary>
        ///     This function can be called on adroid phones to manually update the game state in the game manager.
        /// </summary>
        public async void UpdateGameStateManually()
        {
            _game = await Communicator.GetCurrentGameState();
        }
        
        /// <summary>
        ///     On an Android phone:
        ///     Reset turn update values when application is shut down.
        ///     If application is quit while user is searching for an opponent, keep on searching.
        /// </summary>
        public async void OnApplicationPause()
        {
            if (_game != null)
            {
                if (_game.NextMovePlayerId == _game.Me.Id)
                    PlayerPrefs.SetInt(CURRENT_GAME_BLOCK_TURN_UPDATE, 1);
                else
                    PlayerPrefs.SetInt(CURRENT_GAME_BLOCK_TURN_UPDATE, 0);
            }
        }
#endif

    }
}