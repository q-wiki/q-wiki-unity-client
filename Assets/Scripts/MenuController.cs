using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WikidataGame;
using WikidataGame.Models;

public class MenuController : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource source { get { return GetComponent<AudioSource>(); } }
    public GameObject audioSource;
    public GameObject soundButtonIcon, notificationButtonIcon, vibrationButtonIcon;
    private GameObject startPanel, settingsPanel;
    public GameObject miniGameCanvas, categoryCanvas;
    public GameObject newGameButtonPlayImage, loadingDots, buttonPrefab, menuGrid;
    public Button newGameButton;
    public Sprite soundOff, soundOn, notifiactionOff, notificationOn, vibrationOff, vibrationOn, opponentImage, myImage, newGameButtonGrey, newGameIconGrey;
    bool soundToggle, notificationToggle, vibrationToggle, settingsToggle = true;
    public bool awaitingOpponentToJoin = true;
    private Transform child;
    private static Game _game;


    async void Start()
    {
        
        gameObject.AddComponent<AudioSource>();
        source.clip = clickSound;
        source.playOnAwake = false;

        Scene currentScene = SceneManager.GetActiveScene();

        string sceneName = currentScene.name;

        if (sceneName == "StartScene")
        {
            //Debug.Log("StartScene");
            startPanel = GameObject.Find("StartPanel");
            settingsPanel = GameObject.Find("SettingsPanel");
            settingsPanel.SetActive(false);

        }
        else if (sceneName == "GameScene")
        {
            //Debug.Log("GameScene");
            settingsPanel = GameObject.Find("SettingsPanelContainer");
            settingsPanel.SetActive(false);
            child = gameObject.transform.GetChild(0);
            child.GetComponent<GridController>().GenerateGrid(_game.Tiles);

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
                //Debug.Log($"False: Waiting for Opponent {game.AwaitingOpponentToJoin}.");
                newGameButton.GetComponent<Image>().sprite = newGameButtonGrey;
                newGameButtonPlayImage.SetActive(true);
                newGameButtonPlayImage.GetComponent<Image>().sprite = newGameIconGrey;
                loadingDots.SetActive(false);

                var newButtonContainer = Instantiate(buttonPrefab);
                newButtonContainer.transform.SetParent(menuGrid.transform, false);
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



    public void PlaySound()
    {
        source.PlayOneShot(clickSound);
    }



    public void ChangeToGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public async void StartMiniGame()
    {
        
        Debug.Log("Getting random tile id");
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
        settingsToggle = !settingsToggle;

        if (settingsToggle)
        {
            settingsPanel.SetActive(false);
        }
        else
        {
            settingsPanel.SetActive(true);
        }
    }



    public void ToggleSettingsStart()
    {
        settingsToggle = !settingsToggle;

        if (settingsToggle)
        {
            settingsPanel.SetActive(false);
            startPanel.SetActive(true);
        }
        else
        {
            settingsPanel.SetActive(true);
            startPanel.SetActive(false);
        }
    }


    public void ToggleSound()
    {
        soundToggle = !soundToggle;

        if (soundToggle)
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
        notificationToggle = !notificationToggle;

        if (notificationToggle)
        {
            Debug.Log("Notification Off");
            notificationButtonIcon.GetComponent<Image>().sprite = notifiactionOff;
        }
        else
        {
            Debug.Log("Notification On");
            notificationButtonIcon.GetComponent<Image>().sprite = notificationOn;
        }
    }

    public void ToggleVibration()
    {
        vibrationToggle = !vibrationToggle;

        if (vibrationToggle)
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
