﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WikidataGame;
using WikidataGame.Models;
using System.Threading.Tasks;
using Minigame;

public class MenuController : MonoBehaviour
{
    /**
     * public fields
     */

    public GameObject grid;
    public AudioClip clickSound;
    public GameObject audioSource;
    public GameObject soundButtonIcon;
    public GameObject notificationButtonIcon;
    public GameObject vibrationButtonIcon;
    public GameObject[] miniGameCanvases;
    public GameObject categoryCanvas;
    public GameObject newGameButtonPlayImage;
    public GameObject loadingDots;
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

    public GameObject categoryPanel;
    public GameObject actionPanel;
    public GameObject settingsPanel;
    public GameObject confirmationPanel;
    public GameObject settingsPanelGame;
    public GameObject settingsPanelStart;
    public GameObject startPanelStart;
    
    public GameObject selectedTile;

    public GameObject legalNoticePanel;
    public GameObject creditsPanel;


    /**
     * private fields
     */

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

    private AudioSource Source => GetComponent<AudioSource>();

    public static MenuController Instance;

    /**
     * update method listens for changes in the game state when it's currently not your turn
     */

    public async void Update()
    {
        if(_isWaitingState && !_isHandling)
        {
            HandleWaitingState();
        }
    }

    void Awake()
    {
        Instance = this;
        _settingsToggle = true;
    }


    async void Start()
    {
        gameObject.AddComponent<AudioSource>();
        Source.clip = clickSound;
        Source.playOnAwake = false;

        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;

        if (sceneName == "StartScene")
        {
            // initialize server session and restore previous game if there is one
            Debug.Log("Trying to restore previous game…");
            await Communicator.SetupApiConnection();
            var previousGame = await Communicator.RestorePreviousGame();
            if (previousGame != null)
            {
                Debug.Log($"Previous game {previousGame.Id} restored successfully, changing to game scene");
                _game = previousGame;
                ChangeToGameScene();
            }
            else
            {
                Debug.Log("No previous game found, showing start scene");
                // we don't have a running game, just show the normal start screen
                _startPanel = startPanelStart;
                _settingsPanel = settingsPanelStart;

                /**
                 * delete action point indicator from player prefs to prevent inconsistencies
                 */

                PlayerPrefs.DeleteKey("REMAINING_ACTION_POINTS");
            }
        }
        else if (sceneName == "GameScene")
        {
            _settingsPanel = settingsPanelGame;

            _game = await Communicator.GetCurrentGameState();

            /**
             * generate grid by reading tiles from game object
             */
            grid.GetComponent<GridController>().GenerateGrid(_game.Tiles);
            
            /**
             * if current player is not me, go to loop / block interaction
             */

            if(_game.NextMovePlayerId != _game.Me.Id)
            {
                _isWaitingState = true;
                return;
            }
            
            /**
             * show action point indicator in UI
             */

            if(PlayerPrefs.GetInt("REMAINING_ACTION_POINTS", -1) == 0)
                ActionPointHandler.Instance.UpdateState(_game.Me.Id, _game.NextMovePlayerId, true);
            ActionPointHandler.Instance.Show();
        }
    }

    private async void HandleWaitingState()
    {

        _isHandling = true;
        
        // make blockActionPanel visible and prevent user from selecting anything in the game
        blockActionPanel.alpha = 1;
        blockActionPanel.blocksRaycasts = true;

        Debug.Log("Wait for 10 seconds");
        await Task.Delay(10000);
            
        _game = await Communicator.GetCurrentGameState();
            
        if (_game == null)
            throw new Exception("There is no game");

        if (_game.NextMovePlayerId == _game.Me.Id)
        {
            blockActionPanel.alpha = 0;
            blockActionPanel.blocksRaycasts = false;
            _isWaitingState = false;
            RefreshGameState(true);
        }

        _isHandling = false;
    }

    public async void RefreshGameState(bool isNewTurn)
    {
        // this is called whenever something happens (minigame finished, player made a turn...)
        _game = await Communicator.GetCurrentGameState();
                
        /**
         * TODO:there needs to be an UpdateGrid(_game.Tiles)-method in GridController so that it is not redrawn every time
         * currently the 'old' grid is destroyed so the new grid gets to be drawn safely
         * but it starts to drift further apart because of the addGap-function in GridController
         */
        foreach (Transform child in grid.transform) Destroy(child.gameObject);
        grid.GetComponent<GridController>().GenerateGrid(_game.Tiles);
        
        
        /**
         * simple function to update action points in game controller
         */

        ActionPointHandler.Instance.UpdateState(_game.Me.Id, _game.NextMovePlayerId, isNewTurn);
        
        /**
         * if current player is not me, go to loop / block interaction
         */

        if(_game.NextMovePlayerId != _game.Me.Id)
        {
            _isWaitingState = true;
        }

    }

    public async void Send()
    {
        // disable all buttons so we don't initialize multiple games
        var startGameText = newGameButton.GetComponentInChildren<Text>().text;
        newGameButton.GetComponentInChildren<Text>().text = "Searching for \nOpponent";
        newGameButtonPlayImage.SetActive(false);
        loadingDots.SetActive(true);
        newGameButton.enabled = false;


        if (!Communicator.IsConnected())
        {
            Debug.Log("You are not connected to any game");
            // reset the interface so we can try initializing a game again
            newGameButton.GetComponentInChildren<Text>().text = startGameText;
            newGameButtonPlayImage.SetActive(true);
            loadingDots.SetActive(false);
            newGameButton.enabled = true;
            return;
        }
        else
        {
            await Communicator.CreateOrJoinGame();
            _game = await Communicator.GetCurrentGameState();

            // we'll be checking the game state until another player joins
            while (_game.AwaitingOpponentToJoin ?? true)
            {
                Debug.Log($"Waiting for Opponent.");

                // wait for 5 seconds
                await Task.Delay(5000);
                _game = await Communicator.GetCurrentGameState();
            }

            // another player joined :)
            Debug.Log($"Found opponent, starting game.");
            newGameButton.GetComponent<Image>().sprite = newGameButtonGrey;
            newGameButtonPlayImage.SetActive(true);
            newGameButtonPlayImage.GetComponent<Image>().sprite = newGameIconGrey;
            loadingDots.SetActive(false);

            // 🚀
            ChangeToGameScene();
        }
    }

    public string PlayerId()
    {
        return _game.Me.Id;
    }

    public void PlaySound()
    {
        Source.PlayOneShot(clickSound);
    }

    public void ToggleCameraBehaviour()
    {
        if (camera.GetComponent<CameraBehavior>().enabled)
        {
            camera.GetComponent<CameraBehavior>().enabled = false;
        }
        else
        {
            camera.GetComponent<CameraBehavior>().enabled = true;
        }
    }



    public void ChangeToGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

   /**
    *
    */
    public async void StartMiniGame(string categoryId)
    {
        Debug.Log("Trying to initialize minigame");

        var miniGame = await Communicator.InitializeMinigame(selectedTile.GetComponent<TileController>().id, categoryId);

        if (miniGame.Type == null)
            return;

        // using IMinigame interface to get miniGame depending on given type
        // 0: Sort, 1: Blurry (not implemented), 2: Multiple Choice
        GameObject miniGameCanvas = miniGameCanvases[miniGame.Type.Value];
        IMinigame miniGameInstance = miniGameCanvas.GetComponent<IMinigame>();

        /**
         * extended logging to check for string length issues
         */
        
        Debug.Log($"miniGame.task: {miniGame.TaskDescription}");
        foreach(var answer in miniGame.AnswerOptions) Debug.Log($"answerOption: {answer}");
        
        miniGameInstance.Initialize(miniGame.Id, miniGame.TaskDescription, miniGame.AnswerOptions);

        ToggleCameraBehaviour();
        ActionPointHandler.Instance.Hide();
        miniGameCanvas.SetActive(true);
        categoryCanvas.SetActive(false);
    }

    public void LevelUpOrAttackTile()
    {
        var chosenCategoryId = selectedTile.GetComponent<TileController>().chosenCategoryId;
        if (String.IsNullOrEmpty(chosenCategoryId))
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

    public void CloseCategoryAndActionPamnel()
    {
        categoryPanel.SetActive(false);
        actionPanel.SetActive(false);
    }

    public void ToggleSettingsGame()
    {

        _settingsToggle = !_settingsToggle;

        if (_settingsToggle)
        {
            ToggleCameraBehaviour();
            _settingsPanel.SetActive(false);
        }
        else
        {
            ToggleCameraBehaviour();
            _settingsPanel.SetActive(true);
        }
    }

    public void OpenCreditsPanel()
    {
        creditsPanel.SetActive(true);
        _settingsPanel.SetActive(false);
    }

    public void OpenLegalNoticePanel()
    {
        legalNoticePanel.SetActive(true);
        _settingsPanel.SetActive(false);
    }

    public void ToggleSettingsStart()
    {
        _settingsToggle = !_settingsToggle;

        if (_settingsToggle)
        {
            _settingsPanel.SetActive(false);
            _startPanel.SetActive(true);
            creditsPanel.SetActive(false);
            legalNoticePanel.SetActive(false);
        }
        else
        {
            _settingsPanel.SetActive(true);
            _startPanel.SetActive(false);


        }
    }


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
    
    #region cancellationPanel
    
    public void HandleAbortGamePanel()
    {
        settingsPanel.SetActive(false);
        confirmationPanel.SetActive(true);
    }
    
    public async void LeaveGame()
    {
        Debug.Log($"Trying to delete game");
        await Communicator.AbortCurrentGame();
        Debug.Log($"Game deleted");
        ChangeToStartScene();
    }

    public void StayInGame()
    {
        ToggleCameraBehaviour();
        settingsPanel.SetActive(true);
        confirmationPanel.SetActive(false);
        _settingsPanel.SetActive(false);
        _settingsToggle = !_settingsToggle;
    }
    
    public void ChangeToStartScene()
    {
        SceneManager.LoadScene("StartScene");
    }
    
    #endregion
}
