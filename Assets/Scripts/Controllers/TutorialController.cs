using Controllers;
using System;
using Controllers.Map;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour {

    //constants
    private const string TUT_00_MESSAGE = "Welcome to the tutorial. In Q-Wiki you can compete with another player over the dominion of an island. You conquer land by answering questions correctly.";
    private const string TUT_01_MESSAGE = "Your starting tile is marked blue. Pick a tile adjacent to your territory to try to capture it. Higher tiles are worth more points, but you'll have less time to answer the question.";
    private const string TUT_02_MESSAGE = "Each attempt to capture a tile consumes one action point. Once you've used up all your actions your turn ends and your opponent makes their moves.";
    private const string TUT_03_MESSAGE = "You can also use your action points to level up a tile in under your control, increasing its difficulty level and point worth by one. Or you can attack an opponents tile at your border to steal away their hard-earned points by beating them in their own categories.";
    private const string TUT_04_MESSAGE = "Your opponent took their turn. After 6 turns the game ends and the player with the most points wins.";
    private const string TUT_05_MESSAGE = "Have fun conquering the island and outwitting your opponent. You can revisit this tutorial by enabling the 'Tutorial' option in the settings panel.";

    private const string PLAYERPREFS_TUTORIAL_PAGE = "tutorialPage";
    private const string PLAYERPREFS_TUTORIAL_ID = "tutorialId";
    internal const string PLAYERPREFS_TUTORIAL = "displayTutorial";

    // private Unity Components
    [SerializeField] private Hideable tutorialPanel;
    [SerializeField] private Text tutorialTextField;
    [SerializeField] private Hideable actionPointsHighlight;
    [SerializeField] private Hideable turnsHighlight;
    [SerializeField] private Button confirmationButton;

    //private static Handler instances
    private static GameManager GameManager => GameManager.Instance;
    private static ActionPointHandler ActionPointHandler => ActionPointHandler.Instance;
    private static ScoreHandler ScoreHandler => ScoreHandler.Instance;

    //private static fields
    private static bool tutorialPage2NotDisplayedYet = true;
    private static bool tutorialPage4NotDisplayedYet = true;
    private static bool displayTutorial;
    private static int pageCounter;
    private static string _tutorialID;

    //static properties
    private static bool IsNewGame {
        get { return ScoreHandler.turnsPlayed == 1 && ActionPointHandler.actionPointsRemaining == 3; }
    }
    private static bool TutorialIdNotSet {
        get { return string.IsNullOrEmpty(TutorialID); }
    }
    private static string TutorialID { 
        get { return _tutorialID; } 
        set { 
            _tutorialID = value;
            PlayerPrefs.SetString(PLAYERPREFS_TUTORIAL_ID, value);
        }
    }


    // Start is called before the first frame update
    void Start() {
        GameManager.tutorialController = this;
    }

    //This is called by the game manager when the game scene is initialized
    internal void InitialSetup() {

        //If the player opens the game for the first time, the tutorial is active by default, otherwise it checks if the tutorial has been dismissed/finished yet
        int displayTutorialPlayerprefs = PlayerPrefs.GetInt(PLAYERPREFS_TUTORIAL, -1);
        if (displayTutorialPlayerprefs == -1 && IsNewGame) {
            ActivateTutorial();
            TutorialID = GameManager.GetGameID();
        }
        else {
            displayTutorial = Convert.ToBoolean(displayTutorialPlayerprefs);
            TutorialID = PlayerPrefs.GetString(PLAYERPREFS_TUTORIAL_ID);
        }

        //Don't change any further values if the tutorial is disabled or this is not the tutorial game
        if (displayTutorial == false  ||  TutorialID != GameManager.GetGameID()) return;

        //If the player aleady has a tutorial started, it continues with the next page, otherwise it starts from the beginning
        int tutorialPage = PlayerPrefs.GetInt(PLAYERPREFS_TUTORIAL_PAGE, -1);
        if (tutorialPage == -1 && IsNewGame) {
            pageCounter = 0;
        }
        else if(tutorialPage != -1 && tutorialPage!= 0) {
            pageCounter = tutorialPage;
            ShowCurrentTutorialPanel();
        }
    }

    // Update is called once per frame
    void Update() {

        //Wait for the _game to be correctly initialized, then set the currently running game as the tutorial game (if it is a freshly started game)
        if (GameManager.GetGameID() == null) return;
        else if(TutorialIdNotSet && IsNewGame) {
            TutorialID = GameManager.GetGameID();
        }

        //Don't change any further values if the tutorial is disabled, this is not the tutorial game or the TutorialID has not been set yet
        if (displayTutorial == false  || TutorialIdNotSet || TutorialID != GameManager.GetGameID() ) return;

        //If no tutorial panel has been shown yet, show the first page
        if(pageCounter == 0 && IsNewGame) {
            ShowCurrentTutorialPanel();
        }

        //The first time you spend an action point panel 2 will be shown
        if (displayTutorial && tutorialPage2NotDisplayedYet && ActionPointHandler.actionPointsRemaining == 2 && ScoreHandler.turnsPlayed == 1 && pageCounter == 1) {
            ShowNextTutorialPanel();
        }

        //The first time you've spent all action points and your opponent has taken their turn, show panel 4
        if (displayTutorial && 
            tutorialPage4NotDisplayedYet && 
            ActionPointHandler.actionPointsRemaining == 3 && 
            ScoreHandler.turnsPlayed == 2 && pageCounter == 3 && 
            !InteractionController.Instance.HasActiveMinigamePanel()) {
            ShowNextTutorialPanel();
        }
    }

    private void ShowNextTutorialPanel() {
        pageCounter++;
        PlayerPrefs.SetInt(PLAYERPREFS_TUTORIAL_PAGE, pageCounter);
        ShowCurrentTutorialPanel();
    }

    private void ShowCurrentTutorialPanel() {
        if (displayTutorial) {
            switch(pageCounter) {
                case 0: DisplayTutorialMessage(TUT_00_MESSAGE, ShowNextTutorialPanel, "Next"); break;
                case 1: DisplayTutorialMessage(TUT_01_MESSAGE, HideTutorialPage, "Ok"); break; 
                case 2: DisplayTutorialMessage(TUT_02_MESSAGE, ShowNextTutorialPanel, "Next");
                    tutorialPage2NotDisplayedYet = false;
                    actionPointsHighlight.Show();
                    break;
                case 3: DisplayTutorialMessage(TUT_03_MESSAGE, HideTutorialPage, "Ok"); break;
                case 4: DisplayTutorialMessage(TUT_04_MESSAGE, ShowNextTutorialPanel, "Next");
                    tutorialPage4NotDisplayedYet = false;
                    turnsHighlight.Show();
                    break;
                case 5: DisplayTutorialMessage(TUT_05_MESSAGE, delegate{ DismissTutorial(); HideTutorialPage(); }, "Ok"); break;
                default: break;
            }
        }
    }

    /// <param name="message">Message displayed in the tutorial panel</param>
    /// <param name="buttonFunction">An onClick Listener with this Action will be added to the confirmation button</param>
    /// <param name="confirmationButtonText">Button Text for the confirmation button</param>
    private void DisplayTutorialMessage(string message, Action buttonFunction, string confirmationButtonText = "Ok") {
        tutorialTextField.text = message;

        confirmationButton.onClick.RemoveAllListeners();
        confirmationButton.onClick.AddListener(delegate{ buttonFunction(); });

        confirmationButton.transform.Find("Text").GetComponent<Text>().text = confirmationButtonText;

        actionPointsHighlight.Hide();
        turnsHighlight.Hide();
        tutorialPanel.Show();
    }


    /// <summary>
    /// Hides all tutorial pages but doesn't disable the tutorial
    /// </summary>
    private void HideTutorialPage() {
        tutorialPanel.Hide();
        actionPointsHighlight.Hide();
        turnsHighlight.Hide();
    }

    public void DismissTutorialFromUnityEditor() {
        DismissTutorial();
        HideTutorialPage();
    }

    /// <summary>
    /// Until reactived (by clearing cache or enabling it in the settings panel) no further tutorial pages will be shown
    /// </summary>
    public static void DismissTutorial() {
        displayTutorial = false;
        PlayerPrefs.SetInt(PLAYERPREFS_TUTORIAL, 0);
    }


    /// <summary>
    /// Resets fields, properties and the tutorial related Playerprefs so that the next new game started will be a tutorial game
    /// </summary>
    public static void ActivateTutorial() {
        displayTutorial = true;
        tutorialPage2NotDisplayedYet = true;
        tutorialPage4NotDisplayedYet = true;
        pageCounter = 0;
        TutorialID = null;
        PlayerPrefs.SetInt(PLAYERPREFS_TUTORIAL, 1);
        PlayerPrefs.SetInt(PLAYERPREFS_TUTORIAL_PAGE, 0);
        PlayerPrefs.SetString(PLAYERPREFS_TUTORIAL_ID, null);
    }
}
