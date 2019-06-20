using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WikidataGame;
using WikidataGame.Models;

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
    public GameObject miniGameCanvas;
    public GameObject categoryCanvas;
    public GameObject newGameButtonPlayImage;
    public GameObject loadingDots;
    public GameObject buttonPrefab;
    public GameObject menuGrid;
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

    /**
     * private fields
     */
    
    private static Game _game;
    private GameObject _startPanel;
    private GameObject _settingsPanel;
    private bool _soundToggle;
    private bool _notificationToggle;
    private bool _vibrationToggle;
    private bool _settingsToggle;
    
    private AudioSource Source => GetComponent<AudioSource>();

    void Awake()
    {
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
            //Debug.Log("StartScene");
            _startPanel = GameObject.Find("StartPanel");
            _settingsPanel = GameObject.Find("SettingsPanel");
            _settingsPanel.SetActive(false);

        }
        else if (sceneName == "GameScene")
        {
            //Debug.Log("GameScene");
            _settingsPanel = GameObject.Find("SettingsPanelContainer");
            _settingsPanel.SetActive(false);

            await Communicator.Connect();
            _game = await Communicator.GetCurrentGameState();
            Debug.Log(_game.Tiles);
            grid.GetComponent<GridController>().GenerateGrid(_game.Tiles);

        }

    }

    public async void Send()
    {
        await Communicator.Connect();


        if (!Communicator.isConnected)
        {
            Debug.Log("You are not connected to any game");
            return;
        }
        else
        {
            _game = await Communicator.GetCurrentGameState();
            //Debug.Log($"Started game {game.Id}.");
            //Debug.Log($"AwaitingOpponentToJoin {game.AwaitingOpponentToJoin}.");

            //game.AwaitingOpponentToJoin = awaitingOpponentToJoin;


            if (_game.AwaitingOpponentToJoin ?? true)
            {
                Debug.Log($"True: Waiting for Opponent {_game.AwaitingOpponentToJoin}.");
                newGameButton.GetComponentInChildren<Text>().text = "Searching for \nOpponent";
                newGameButtonPlayImage.SetActive(false);
                loadingDots.SetActive(true);
                newGameButton.enabled = false;
            }

            else
            {
                newGameButton.enabled = false;
                Debug.Log($"False: Waiting for Opponent {_game.AwaitingOpponentToJoin}.");
                newGameButton.GetComponent<Image>().sprite = newGameButtonGrey;
                newGameButtonPlayImage.SetActive(true);
                newGameButtonPlayImage.GetComponent<Image>().sprite = newGameIconGrey;
                loadingDots.SetActive(false);

                var newButtonContainer = Instantiate(buttonPrefab, menuGrid.transform, false);
                Vector3 pos = newButtonContainer.transform.localPosition;
                newButtonContainer.transform.localPosition = new Vector3(0, 0, 0);


                GameObject newButton = newButtonContainer.transform.GetChild(0).gameObject;
                newButton.GetComponentInChildren<Button>().onClick.AddListener(() => ChangeToGameScene()); 
                GameObject childImageInNewButton = newButton.transform.GetChild(1).gameObject;

                //game.NextMovePlayerId = game.Opponent.Id; 
                if (_game.NextMovePlayerId == _game.Me.Id)
                {
                    newButton.GetComponentInChildren<Text>().text = "Your Turn!";
                    childImageInNewButton.GetComponent<Image>().sprite = myImage;
                }
                else if (_game.NextMovePlayerId == _game.Opponent.Id)
                {
                    newButton.GetComponentInChildren<Text>().text = "Waiting for \nOpponent...";
                    childImageInNewButton.GetComponent<Image>().sprite = opponentImage;
                    newButton.GetComponent<Button>().enabled = false;



                }


            }

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



    public void ChangeToGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

   /**
    * 
    */ 
    public async void StartMiniGame()
    {
        // get current game state from backend
        var game = await Communicator.GetCurrentGameState();

        Debug.Log("Trying to initialize minigame");
        var miniGame = await Communicator.InitializeMinigame(game.Tiles[0][0].Id, "0");
        
        // TODO: auf Grundlage von miniGame.Type entsprechendes Game öffnen 
        MinigameMultipleChoice instance = miniGameCanvas.GetComponent<MinigameMultipleChoice>();
        instance.Initialize(miniGame.Id, miniGame.TaskDescription, miniGame.AnswerOptions);
        
        miniGameCanvas.SetActive(true);
        categoryCanvas.SetActive(false);
    }


    public void ToggleSettingsGame()
    {
       
        _settingsToggle = !_settingsToggle;

        if (_settingsToggle)
        {
            camera.GetComponent<CameraBehavior>().enabled = true;
            _settingsPanel.SetActive(false);
        }
        else
        {
            camera.GetComponent<CameraBehavior>().enabled = false;
            _settingsPanel.SetActive(true);
        }
    }



    public void ToggleSettingsStart()
    {
        _settingsToggle = !_settingsToggle;

        if (_settingsToggle)
        {
            _settingsPanel.SetActive(false);
            _startPanel.SetActive(true);
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

    public void ToggleVibration()
    {
        _vibrationToggle = !_vibrationToggle;

        if (_vibrationToggle)
        {
            Debug.Log("Vibration Off");
            vibrationButtonIcon.GetComponent<Image>().sprite = vibrationOff;
        }
        else
        {
            Debug.Log("Vibration On");
            vibrationButtonIcon.GetComponent<Image>().sprite = vibrationOn;
        }
    }

    public void ToggleCreditsPanel()
    {
        Debug.Log("Credits");
    }
}
