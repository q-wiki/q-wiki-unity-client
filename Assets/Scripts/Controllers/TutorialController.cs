using Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour {

    [SerializeField] private Hideable tutorialPanel;
    [SerializeField] private Text tutorialTextField;
    [SerializeField] private Hideable actionPointsHighlight;
    [SerializeField] private Hideable turnsHighlight;
    [SerializeField] private Button okButton;

    private static bool tutorialPage2NotDisplayedYet = true;
    private static bool tutorialPage4NotDisplayedYet = true;
    private static bool tutorialIdNotSet = true;
    internal static int pageCounter { get; private set; }
    internal static bool displayTutorial { get; private set; }
    internal static string tutorialID;

    private const string TUT_00_MESSAGE = "a";
    private const string TUT_01_MESSAGE = "b";
    private const string TUT_02_MESSAGE = "c";
    private const string TUT_03_MESSAGE = "d";
    private const string TUT_04_MESSAGE = "e";
    private const string TUT_05_MESSAGE = "f";

    internal const string PLAYERPREFS_TUTORIAL = "displayTutorial";
    private const string PLAYERPREFS_TUTORIAL_PAGE = "tutorialPage";
    internal const string PLAYERPREFS_TUTORIAL_ID = "tutorialId";

    private static GameManager GameManager => GameManager.Instance;
    private static ActionPointHandler ActionPointHandler => ActionPointHandler.Instance;

    // Start is called before the first frame update
    void Start() {

        //if the player opens the game for the first time, the tutorial is active by default, otherwise it checks if the tutorial has been dismissed/finished yet
        int? displayTutorialPlayerprefs = PlayerPrefs.GetInt(PLAYERPREFS_TUTORIAL, -1);
        if(displayTutorialPlayerprefs == -1) {
            ActivateTutorial();
        }
        else {
            displayTutorial = Convert.ToBoolean(displayTutorialPlayerprefs);
        }

        //if the player aleady has a tutorial started, it continues with the next page, otherwise it starts from the beginning
        int? tutorialPage = PlayerPrefs.GetInt(PLAYERPREFS_TUTORIAL_PAGE, -1);
        if (tutorialPage == -1) {
            tutorialPage = 0;
        }
        else {
            pageCounter = (int)tutorialPage;
        }

        //There should only be one tutorial game active at a time. The first game you start after activating the tutorial will show the tutorial panels.
        tutorialID = PlayerPrefs.GetString(PLAYERPREFS_TUTORIAL_ID);
        if (!string.IsNullOrEmpty(tutorialID)) {
            tutorialIdNotSet = false;
        }

        Debug.Log($"{pageCounter}:{displayTutorial}:{tutorialID}");


        GameManager.tutorialController = this;
    }

    // Update is called once per frame
    void Update() {

        //wait for the _game to be correctly initialized, then set the currently running game as the tutorial game
        if(GameManager.GetGameID() == null) return;
        else if(tutorialIdNotSet){
            tutorialIdNotSet = false;
            tutorialID = GameManager.GetGameID();
            Debug.Log($"Tutorial_ID: {tutorialID}");
        }

        //show Tutorial panels only if the currently running game is the tutorial game
        if (GameManager.GetGameID() != tutorialID) return;

        //if no tutorial panel has been shown yet, show the first page
        if(pageCounter == 0) {
            ShowAppropriateTutorialPanel();
        }

        //the first time you spend an action point panel 2 will be shown
        if (displayTutorial && tutorialPage2NotDisplayedYet && PlayerPrefs.GetInt($"{GameManager.GetGameID()}/{ActionPointHandler.PLAYERPREFS_REMAINING_ACTION_POINTS}", -1) == 2 && pageCounter == 2) {
            ShowAppropriateTutorialPanel();
            tutorialPage2NotDisplayedYet = false;
        }

        //the first time you've spent all action points and your opponent has taken their turn, show panel 4
        if (displayTutorial && tutorialPage4NotDisplayedYet && PlayerPrefs.GetInt($"{GameManager.GetGameID()}/{ActionPointHandler.PLAYERPREFS_REMAINING_ACTION_POINTS}", -1) == 3 && pageCounter == 4) {
            ShowAppropriateTutorialPanel();
            tutorialPage4NotDisplayedYet = false;
        }
    }

    internal void ShowAppropriateTutorialPanel() {
        if (displayTutorial) {
            SelectTutorialMessage(pageCounter);
            pageCounter++;
            PlayerPrefs.SetInt(PLAYERPREFS_TUTORIAL_PAGE, pageCounter);
        }
    }

    private void SelectTutorialMessage(int panelNumber = 0) {
        switch(panelNumber){
            case 0: DisplayTutorialMessage(TUT_00_MESSAGE, ShowAppropriateTutorialPanel, "Next");  break;
            case 1: DisplayTutorialMessage(TUT_01_MESSAGE, HideTutorial, "Ok"); break; 
            case 2: DisplayTutorialMessage(TUT_02_MESSAGE, ShowAppropriateTutorialPanel, "Next");
                actionPointsHighlight.Show();
                break;
            case 3: DisplayTutorialMessage(TUT_03_MESSAGE, HideTutorial, "Ok"); break;
            case 4: DisplayTutorialMessage(TUT_04_MESSAGE, ShowAppropriateTutorialPanel, "Next");
                turnsHighlight.Show();
                break;
            case 5: DisplayTutorialMessage(TUT_05_MESSAGE, delegate{ DismissTutorial(); HideTutorial(); }, "Ok"); break;
            default: break;
        }
    }

    private void DisplayTutorialMessage(string message, Action buttonFunction, string okButtonText = "Ok") {
        tutorialTextField.text = message;

        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(delegate{ buttonFunction(); });

        okButton.transform.Find("Text").GetComponent<Text>().text = okButtonText;

        actionPointsHighlight.Hide();
        turnsHighlight.Hide();
        tutorialPanel.Show();
    }

    public void HideTutorial() {
        tutorialPanel.Hide();
        actionPointsHighlight.Hide();
        turnsHighlight.Hide();
    }

    public static void DismissTutorial() {
        displayTutorial = false;
        PlayerPrefs.SetInt(PLAYERPREFS_TUTORIAL, 0);
    }

    public static void ActivateTutorial() {
        displayTutorial = true;
        tutorialPage2NotDisplayedYet = true;
        tutorialPage4NotDisplayedYet = true;
        tutorialIdNotSet = true;
        pageCounter = 0;
        tutorialID = null;
        PlayerPrefs.SetInt(PLAYERPREFS_TUTORIAL, 1);
        PlayerPrefs.SetInt(PLAYERPREFS_TUTORIAL_PAGE, 0);
        PlayerPrefs.SetString(PLAYERPREFS_TUTORIAL_ID, null);
    }
}
