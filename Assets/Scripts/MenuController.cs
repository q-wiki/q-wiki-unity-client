using System.Threading.Tasks;
using Minigame;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WikidataGame.Models;

/// <summary>
///     This is the game controller for QWiki, containing important functions for user interaction
///     as well as functionality for frontend and backend
/// </summary>
public class MenuController : MonoBehaviour
{
    /// <summary>
    ///     update method listens for changes in the game state when it's currently not your turn
    /// </summary>
    public void Update()
    {
        if (_isWaitingState && !_isHandling) HandleWaitingState();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_settingsToggle)
            {
                if (currentScene.name.Contains("Game"))
                    ToggleSettingsGame();
                else if (currentScene.name.Contains("Start")) ToggleSettingsStart();
            }
            else
            {
                if (!hasActiveMinigamePanel())
                {
                    Debug.Log("Closing App ...");
                    Application.Quit();
                }
            }
        }
    }

    /// <summary>
    ///     This function checks if there is currently a MiniGame displayed to the user
    /// </summary>
    /// <returns>if a MiniGame canvas is active</returns>
    private bool hasActiveMinigamePanel()
    {
        foreach (var panel in miniGameCanvases)
            if (panel.active)
                return true;
        return false;
    }

    /// <summary>
    ///     The awake function gets called before the Start()-function
    /// </summary>
    private void Awake()
    {
        Instance = this;
        _settingsToggle = true;
    }

    /// <summary>
    ///     The start function is used to arrange some things depending on the scene
    /// </summary>
    private async void Start()
    {
        gameObject.AddComponent<AudioSource>();
        Source.clip = clickSound;
        Source.playOnAwake = false;

        currentScene = SceneManager.GetActiveScene();
        var sceneName = currentScene.name;

        if (sceneName == "StartScene")
        {
            Debug.Log("Start scene succesfully set up");

            // we don't have a running game, just show the normal start screen
            _startPanel = startPanelStart;
            _settingsPanel = settingsPanelStart;

            /**
             * delete action point indicator from player prefs to prevent inconsistencies
             */

            PlayerPrefs.DeleteKey("REMAINING_ACTION_POINTS");
            PlayerPrefs.DeleteKey("CURRENT_GAME_TURNS_PLAYED");
        }
        else if (sceneName == "GameScene")
        {
            Debug.Log("Game scene succesfully set up");

            _settingsPanel = settingsPanelContainerGame;

            _game = await Communicator.GetCurrentGameState();

            /* when in gamescene, check game over state */

            HandleGameOverState();

            /**
             * generate grid by reading tiles from game object
             */
            grid.GetComponent<GridController>().GenerateGrid(_game.Tiles);

            /**
             * update points to show an updated state
             */

            ScoreHandler.Instance.UpdatePoints(_game.Tiles, _game.Me.Id, _game.Opponent.Id);

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

            if (PlayerPrefs.GetInt("REMAINING_ACTION_POINTS", -1) == 0)
                ActionPointHandler.Instance.UpdateState(_game.Me.Id, _game.NextMovePlayerId, true);
            ActionPointHandler.Instance.Show();

            /**
             * update turn UI when it is the player's move directly after opening the app
             */

            if (PlayerPrefs.GetInt(CURRENT_GAME_BLOCK_TURN_UPDATE, 0) == 0)
                ScoreHandler.Instance.UpdateTurns();

            /*
             * highlight possible moves for current player
             */

            grid.GetComponent<GridController>().ShowPossibleMoves(_game.Me.Id);
        }
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

    /// <summary>
    ///     This function is called when it's not the client's move.
    ///     A block panel is shown to prevent input by the client.
    /// </summary>
    private async void HandleWaitingState()
    {
        _isHandling = true;

        // make blockActionPanel visible and prevent user from selecting anything in the game
        blockActionPanel.alpha = 1;
        blockActionPanel.blocksRaycasts = true;

        Debug.Log("Wait for 10 seconds");
        await Task.Delay(10000);

        _game = await Communicator.GetCurrentGameState();

        /**
         * check if game is over
         * if game is deleted, we assume for now, that the other player deleted it and show the winning screen
         */
        if (_game == null)
        {
            Debug.Log("Game is null - the other player probably deleted it");
            gameOverText.text = "You won! Congratulations!";
            blockActionPanel.alpha = 0;
            blockActionPanel.blocksRaycasts = false;
            gameOverCanvas.SetActive(true);
            return;
        }

        /**
         * check if game is over
         */
        HandleGameOverState();

        if (_game.NextMovePlayerId == _game.Me.Id)
        {
            blockActionPanel.alpha = 0;
            blockActionPanel.blocksRaycasts = false;
            _isWaitingState = false;
            RefreshGameState(true);
        }

        _isHandling = false;
    }

    /// <summary>
    ///     This function is used to handle the end of the game.
    ///     An appropriate message is shown to the user depending on the outcome.
    /// </summary>
    private void HandleGameOverState()
    {
        if (_game.WinningPlayerIds != null && _game.WinningPlayerIds.Count > 0)
        {
            Debug.Log("Checking Game Over State!");

            gameOverCanvas.SetActive(true);

            if (_game.WinningPlayerIds.Count == 1 && _game.WinningPlayerIds.Contains(_game.Me.Id))
            {
                gameOverText.text = "You won! Congratulations!";
                Debug.Log("You won!");
            }
            else if (_game.WinningPlayerIds.Count == 1 && _game.WinningPlayerIds.Contains(_game.Opponent.Id))
            {
                gameOverText.text = "You Lost!";
                Debug.Log("You Lost! Try again!");
            }
            else if (_game.WinningPlayerIds.Count == 2 && _game.WinningPlayerIds.Contains(_game.Opponent.Id) &&
                     _game.WinningPlayerIds.Contains(_game.Me.Id))
            {
                gameOverText.text = "Draw! \nTry again!";
                Debug.Log("Draw!");
            }
        }
    }

    /// <summary>
    ///     This function is used to refresh the current state of the game.
    ///     This is done by receiving current information from the backend.
    ///     Depending on the state, different functions are called.
    /// </summary>
    /// <param name="isNewTurn"></param>
    public async void RefreshGameState(bool isNewTurn)
    {
        Debug.Log($"isNewTurn:{isNewTurn}");

        /*
         * this is called whenever something happens (MiniGame finished, player made a turn, etc.)
         */

        _game = await Communicator.GetCurrentGameState();

        /*
         * redraw the grid
         *
        
        foreach (Transform child in grid.transform) Destroy(child.gameObject);
        grid.GetComponent<GridController>().GenerateGrid(_game.Tiles);

        /**
         * adjust UI to new score
         */

        ScoreHandler.Instance.Show();
        ScoreHandler.Instance.UpdatePoints(_game.Tiles, _game.Me.Id, _game.Opponent.Id);

        /**
         * if current update happens in a new turn, update turn count
         */

        if (isNewTurn)
            ScoreHandler.Instance.UpdateTurns();


        /**
         * simple function to update action points in game controller
         */

        ActionPointHandler.Instance.UpdateState(_game.Me.Id, _game.NextMovePlayerId, isNewTurn);

        /**
        * check for game over
        */

        HandleGameOverState();

        /*
         * highlight possible moves for current player
         * */

        grid.GetComponent<GridController>().ShowPossibleMoves(_game.Me.Id);


        /**
         * if current player is not me, go to loop / block interaction
         */

        if (_game.NextMovePlayerId != _game.Me.Id) _isWaitingState = true;
    }

    /// <summary>
    ///     This function is called when the client wants to start a new game.
    /// </summary>
    public async void Send()
    {
        // disable all buttons so we don't initialize multiple games
        var startGameText = newGameButton.GetComponentInChildren<Text>().text;
        newGameButton.GetComponentInChildren<Text>().text = "Searching for Opponent...";
        newGameButton.GetComponentInChildren<Text>().fontSize = 56;

        LoadingIndicator.Instance.Show();
        newGameButton.enabled = false;


        if (!Communicator.IsConnected())
        {
            Debug.Log("You are not connected to any game");
            // reset the interface so we can try initializing a game again
            newGameButton.GetComponentInChildren<Text>().text = startGameText;
            newGameButton.enabled = true;
            return;
        }

        await Communicator.CreateOrJoinGame();
        _game = await Communicator.GetCurrentGameState();

        // we'll be checking the game state until another player joins
        while (_game.AwaitingOpponentToJoin ?? true)
        {
            Debug.Log("Waiting for Opponent.");

            // wait for 5 seconds
            await Task.Delay(5000);
            _game = await Communicator.GetCurrentGameState();
        }

        // another player joined :)
        Debug.Log("Found opponent, starting game.");

        // 🚀
        LoadingIndicator.Instance.Hide();
        ChangeToGameScene();
    }

    /// <summary>
    ///     Returns the ID of the client.
    /// </summary>
    /// <returns>The ID of the client.</returns>
    public string PlayerId()
    {
        return _game.Me.Id;
    }

    /// <summary>
    ///     A click sound is played.
    /// </summary>
    public void PlaySound()
    {
        Source.PlayOneShot(clickSound);
    }

    /// <summary>
    ///     Camera behaviour is enabled or disabled.
    /// </summary>
    public void ToggleCameraBehaviour()
    {
        if (camera.GetComponent<CameraBehavior>().enabled)
            camera.GetComponent<CameraBehavior>().enabled = false;
        else
            camera.GetComponent<CameraBehavior>().enabled = true;
    }

    /// <summary>
    ///     Use this to load the GameScene.
    /// </summary>
    public void ChangeToGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    ///     This is used to initialize a MiniGame and display it on-screen.
    /// </summary>
    /// <param name="categoryId">The ID of the chosen category</param>
    public async void StartMiniGame(string categoryId)
    {
        Debug.Log("Trying to initialize minigame");

        LoadingIndicator.Instance.Show();
        var miniGame =
            await Communicator.InitializeMinigame(selectedTile.GetComponent<TileController>().id, categoryId);
        LoadingIndicator.Instance.Hide();

        if (miniGame.Type == null)
            return;

        // using IMinigame interface to get miniGame depending on given type
        // 0: Sort, 1: Blurry (not implemented), 2: Multiple Choice
        var miniGameCanvas = miniGameCanvases[miniGame.Type.Value];
        var miniGameInstance = miniGameCanvas.GetComponent<IMinigame>();

        /**
         * extended logging to check for string length issues
         */

        Debug.Log($"miniGame.task: {miniGame.TaskDescription}");
        foreach (var answer in miniGame.AnswerOptions) Debug.Log($"answerOption: {answer}");


        /**
         * get difficulty level from tile controller and initialize miniGame with it
         */

        var tile = selectedTile.GetComponentInChildren<TileController>();

        miniGameInstance.Initialize(miniGame.Id, miniGame.TaskDescription, miniGame.AnswerOptions, tile.difficulty);

        ToggleCameraBehaviour();
        ActionPointHandler.Instance.Hide();
        ScoreHandler.Instance.Hide();
        miniGameCanvas.SetActive(true);
        categoryCanvas.SetActive(false);
    }

    /// <summary>
    ///     This function is called when a tile is already occupied.
    ///     It distinguishes between an own tile and an opponent's tile.
    /// </summary>
    public void LevelUpOrAttackTile()
    {
        var chosenCategoryId = selectedTile.GetComponent<TileController>().chosenCategoryId;
        if (string.IsNullOrEmpty(chosenCategoryId))
        {
            // we're on our start tile
            ShowCategoryPanel();
        }
        else
        {
            actionPanel.SetActive(false);
            StartMiniGame(chosenCategoryId);
        }
    }

    /// <summary>
    ///     This function is used to show the CategoryPanel.
    /// </summary>
    public void ShowCategoryPanel()
    {
        // We're trying to capture it for the first time
        actionPanel.SetActive(false);
        categoryPanel.SetActive(true);

        var availableCategories = selectedTile.GetComponent<TileController>().availableCategories;

        c1.GetComponentInChildren<Text>().text = availableCategories[0].Title;
        c2.GetComponentInChildren<Text>().text = availableCategories[1].Title;
        c3.GetComponentInChildren<Text>().text = availableCategories[2].Title;

        c1.onClick.RemoveAllListeners();
        c2.onClick.RemoveAllListeners();
        c3.onClick.RemoveAllListeners();

        c1.onClick.AddListener(() => { StartMiniGame(availableCategories[0].Id); });
        c2.onClick.AddListener(() => { StartMiniGame(availableCategories[1].Id); });
        c3.onClick.AddListener(() => { StartMiniGame(availableCategories[2].Id); });
    }

    /// <summary>
    ///     This function is called to close the CategoryPanel as well as the ActionPanel.
    /// </summary>
    public void CloseCategoryAndActionPanel()
    {
        categoryPanel.SetActive(false);
        actionPanel.SetActive(false);

        if (string.IsNullOrEmpty(selectedTile.GetComponent<TileController>().ownerId))
            selectedTile.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = tileMaterials[3];
        else if (selectedTile.GetComponent<TileController>().ownerId == PlayerId())
            selectedTile.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = tileMaterials[0];
        else
            selectedTile.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = tileMaterials[1];
    }

    /// <summary>
    ///     This function gets called after a game is finished.
    ///     Current game ID is deleted from PlayerPrefs to prevent looping.
    /// </summary>
    public void HandleGameFinished()
    {
        LoadingIndicator.Instance.Show();
        PlayerPrefs.DeleteKey("CURRENT_GAME_ID");
        LoadingIndicator.Instance.Hide();
        ChangeToStartScene();
    }

    /// <summary>
    ///     This function is used to open the CreditsPanel in the settings.
    /// </summary>
    public void OpenCreditsPanel()
    {
        creditsPanel.SetActive(true);
        _settingsPanel.GetComponent<CanvasGroup>().alpha = 0;
        _settingsPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    /// <summary>
    ///     This function is used to open the LegalNoticePanel in the settings.
    /// </summary>
    public void OpenLegalNoticePanel()
    {
        legalNoticePanel.SetActive(true);
        _settingsPanel.GetComponent<CanvasGroup>().alpha = 0;
        _settingsPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }


    /// <summary>
    ///     This function is used to show or hide the settings panel while being in-game.
    /// </summary>
    public void ToggleSettingsGame()
    {
        _settingsToggle = !_settingsToggle;

        if (_settingsToggle)
        {
            ToggleCameraBehaviour();
            ScoreHandler.Instance.Show();
            _settingsPanel.GetComponent<CanvasGroup>().alpha = 0;
            _settingsPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        else
        {
            ToggleCameraBehaviour();
            ScoreHandler.Instance.Hide();
            _settingsPanel.GetComponent<CanvasGroup>().alpha = 1;
            _settingsPanel.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }

    /// <summary>
    ///     This function is used to show or hide the settings panel while being in the main menu.
    /// </summary>
    public void ToggleSettingsStart()
    {
        _settingsToggle = !_settingsToggle;

        if (_settingsPanel == null)
            _settingsPanel = GameObject.Find("SettingsPanel");

        if (_startPanel == null)
            _startPanel = GameObject.Find("StartPanel");

        if (_settingsToggle)
        {
            _settingsPanel.GetComponent<CanvasGroup>().alpha = 0;
            _settingsPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
            _startPanel.GetComponent<CanvasGroup>().alpha = 1;
            _startPanel.GetComponent<CanvasGroup>().blocksRaycasts = true;

            creditsPanel.SetActive(false);
            legalNoticePanel.SetActive(false);
        }
        else
        {
            _settingsPanel.GetComponent<CanvasGroup>().alpha = 1;
            _settingsPanel.GetComponent<CanvasGroup>().blocksRaycasts = true;
            _startPanel.GetComponent<CanvasGroup>().alpha = 0;
            _startPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    /// <summary>
    ///     This function is used to turn sound on or off.
    /// </summary>
    public void ToggleSound()
    {
        _soundToggle = !_soundToggle;

        if (_soundToggle)
        {
            //audioSource.SetActive(false);
            AudioListener.volume = 0;
            soundButtonIcon.GetComponent<Image>().sprite = soundOff;
        }
        else
        {
            audioSource.SetActive(true);
            AudioListener.volume = 1;
            soundButtonIcon.GetComponent<Image>().sprite = soundOn;
        }
    }

    /// <summary>
    ///     This function is used to turn notifications on or off.
    /// </summary>
    public void ToggleNotification()
    {
        _notificationToggle = !_notificationToggle;

        if (_notificationToggle)
        {
            Debug.Log("Notification Off");
            notificationButtonIcon.GetComponent<Image>().sprite = notificationOff;
        }
        else
        {
            Debug.Log("Notification On");
            notificationButtonIcon.GetComponent<Image>().sprite = notificationOn;
        }
    }

    /// <summary>
    ///     This function is used to open the privacy policy of the game.
    ///     It is opened in a web browser.
    /// </summary>
    public void OpenPrivacyPolicy()
    {
        Application.OpenURL("https://wikidatagame.azurewebsites.net/privacy");
    }

    /// <summary>
    ///     These are the public fields of the game controller.
    ///     They can be accessed via Unity Editor or other classes.
    /// </summary>

    #region public_fields

    public GameObject grid;

    public AudioClip clickSound;
    public GameObject audioSource;
    public GameObject soundButtonIcon;
    public GameObject notificationButtonIcon;
    public GameObject vibrationButtonIcon;
    public GameObject[] miniGameCanvases;
    public GameObject categoryCanvas;
    public GameObject buttonPrefab;
    public GameObject menuGrid;
    public CanvasGroup blockActionPanel;
    public Button newGameButton;
    public Sprite soundOff;
    public Sprite soundOn;
    public Sprite notificationOff;
    public Sprite notificationOn;
    public Sprite vibrationOff;
    public Sprite vibrationOn;
    public Sprite opponentImage;
    public Sprite myImage;
    public Sprite newGameButtonGrey;
    public Sprite newGameIconGrey;
    public bool awaitingOpponentToJoin;
    public Camera camera;
    public Material[] tileMaterials;
    public GameObject categoryPanel;
    public GameObject actionPanel;
    public GameObject settingsPanel;
    public GameObject confirmationPanel;
    public GameObject settingsPanelContainerGame;
    public GameObject settingsPanelStart;
    public GameObject startPanelStart;
    public Text levelText;
    public GameObject selectedTile;
    public GameObject gameOverCanvas;
    public Text gameOverText;
    public GameObject legalNoticePanel;
    public GameObject creditsPanel;

    #endregion

    /// <summary>
    ///     These are private fields of the game controller.
    ///     They can only be accessed from within the class.
    /// </summary>

    #region private_fields

    private static MenuController Instance;

    private static Game _game;
    private GameObject _startPanel;
    private GameObject _settingsPanel;
    public Button c1, c2, c3;
    private bool _soundToggle;
    private bool _notificationToggle;
    private bool _vibrationToggle;
    private bool _settingsToggle;
    private bool _isWaitingState;
    private bool _isHandling;
    private Scene currentScene;
    private readonly string CURRENT_GAME_BLOCK_TURN_UPDATE = "CURRENT_GAME_BLOCK_TURN_UPDATE";
    private AudioSource Source => GetComponent<AudioSource>();

    #endregion

    #region cancellationPanel

    /// <summary>
    ///     This function is used to handle the abort game panel.
    /// </summary>
    public void HandleAbortGamePanel()
    {
        settingsPanel.SetActive(false);
        confirmationPanel.SetActive(true);
    }

    /// <summary>
    ///     This function is used to delete / leave a game.
    /// </summary>
    public async void LeaveGame()
    {
        Debug.Log("Trying to delete game");
        LoadingIndicator.Instance.Show();
        await Communicator.AbortCurrentGame();
        LoadingIndicator.Instance.Hide();
        Debug.Log("Game deleted");
        ChangeToStartScene();
    }

    /// <summary>
    ///     This function is used to stay in-game when the abort game panel is shown.
    /// </summary>
    public void StayInGame()
    {
        ToggleCameraBehaviour();
        settingsPanel.SetActive(true);
        confirmationPanel.SetActive(false);

        _settingsPanel.GetComponent<CanvasGroup>().alpha = 0;
        _settingsPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;

        _settingsToggle = !_settingsToggle;
    }

    /// <summary>
    ///     This function is used to load the StartScene of the game.
    /// </summary>
    public void ChangeToStartScene()
    {
        SceneManager.LoadScene("StartScene");
    }

    #endregion
}