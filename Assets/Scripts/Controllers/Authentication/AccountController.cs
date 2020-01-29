using Controllers;
using Controllers.Authentication;
using Controllers.UI;
using GooglePlayGames;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Controllers.UI.User;
using UnityEngine;
using UnityEngine.UI;
using WikidataGame.Models;
using System.Linq;

public class AccountController : MonoBehaviour
{
    public List<Player> FriendsList { get; private set; }

    private static List<FinishedGame> gameHistory;

    private StartUIController _uiController;

    [SerializeField] private GameObject userPrefab;
    [SerializeField] private GameObject friendPrefab;
    [SerializeField] private GameObject requestPrefab;
    [SerializeField] private GameObject gameInstancePrefab;
    [SerializeField] private GameObject gameHistoryPrefab;
    [SerializeField] private GameObject userScrollViewContent;
    [SerializeField] private GameObject requestScrollViewContent;
    [SerializeField] private GameObject gamesScrollViewContent;
    [SerializeField] private Text usernameText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Color yourTurnColor;
    [SerializeField] private Color oppentTurnColor;
    private const string BOT_NAME = "AI Bot";
    private const string GAME_MATCH_MESSAGE = "You matched with ";
    private const string USER_CORNERBUTTON_NAME = "AddFriendButton";
    private const string FRIEND_CORNERBUTTON_NAME = "DeleteFriendButton";

    private const string PLAYERPREFS_GAMEHISTORY = "GameHistory";

    // Start is called before the first frame update
    async void Start(){

        gameHistory = new List<FinishedGame>();

        _uiController = GameManager.Instance.UIController() as StartUIController;
        if (_uiController == null)
            throw new Exception("Start UI Controller is not allowed to be null");
        
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString(Communicator.PLAYERPREFS_AUTH_TOKEN))) {
            InitialSetup();
        }

        //_uiController.findUserInput.onEndEdit.RemoveAllListeners();
        _uiController.findUserInput.onValueChanged.RemoveAllListeners();

        //_uiController.findUserInput.onEndEdit.AddListener(delegate { inputSubmitCallBack(); });
        _uiController.findUserInput.onValueChanged.AddListener(delegate { inputChangedCallBack(); });
    }

    public async void InitialSetup() {
            SetHeadline();
            await RetrieveGames();
            await RetrieveGameRequests();
            await RetrieveFriends();
            RetrieveGamesForGameHistory();
    }

    // Update is called once per frame
    void Update(){
    }

    void OnEnable(){
        //Register InputField Events
        if (_uiController == null)
        {
            return;
        }
       // _uiController.findUserInput.onEndEdit.AddListener(delegate { inputSubmitCallBack(); });
        _uiController.findUserInput.onValueChanged.AddListener(delegate { inputChangedCallBack(); });
    }

    private void inputChangedCallBack() {
        switch (_uiController.findUserInput.text.Length)
        {
            case 0: RetrieveFriendsVoid(); break;
            case 1: break;
            case 2: break;
            default: FindUsers(); break;
        }
    }

    void OnDisable(){
        //Un-Register InputField Events
        //_uiController.findUserInput.onEndEdit.RemoveAllListeners();

        if(_uiController != null)
            _uiController.findUserInput.onValueChanged.RemoveAllListeners();
    }

    /// <summary>
    /// Set Username and Avatar Image (if available) in the headline
    /// </summary>
    public void SetHeadline(){
        bool usernameInPlayerprefs = !string.IsNullOrEmpty(PlayerPrefs.GetString(SignInController.PLAYERPREFS_USERNAME));
        string username = usernameInPlayerprefs ? PlayerPrefs.GetString(SignInController.PLAYERPREFS_USERNAME) : "Username";
        usernameText.text = username;

        if (Social.localUser.authenticated){
            Debug.Log("Setting Google Avatar");
            HelperMethods.SetImage(avatarImage, username);
        }
        else {
            Debug.Log("Setting Default Avatar");
            HelperMethods.SetImage(avatarImage, "anon-" + username);
        }
    }


    /// <summary>
    /// Search for users matching the name from the input
    /// </summary>
    public async void FindUsers() {

        _uiController.DisplayUserSearchUI();
        string username = (_uiController.findUserInput.text == "") ? "Anonymous User" : _uiController.findUserInput.text;

        Task<IList<Player>> findUsers = Communicator.FindUsers(username);
        IList<Player> response = await findUsers;

        if (response == null) {
            Debug.LogError("Couldn't retrieve users");
        }
        else {
            DisplayUsersInScrollView(response, userPrefab, USER_CORNERBUTTON_NAME, AddFriend);
        }
    }

    /// <summary>
    /// Retrieve the friends of the player that is currently logged in
    /// </summary>
    public async Task<bool> RetrieveFriends() {

        SetHeadline();
        _uiController.DisplayFriendsListUI();

        Task<IList<Player>> retrieveFriends = Communicator.RetrieveFriends();
        IList<Player> response = await retrieveFriends;

        if (response == null) {
            Debug.LogError("Couldn't retrieve friends");
            return false;
        }
        else {
            FriendsList = (List<Player>)response;
            DisplayUsersInScrollView(response, friendPrefab, FRIEND_CORNERBUTTON_NAME, DeleteFriend);
            return true;
        }
    }

    /// <summary>
    /// Used to call the RetrieveFriends Function via the Unity UI
    /// </summary>
    public async void RetrieveFriendsVoid() {
        await RetrieveFriends();
    }
    /// <summary>
    /// Clears the ScrollView and adds a list of users to it.
    /// These are either the friends of the player that is currently logged in, or up to 10 users matching the name from the input field
    /// Tapping on the user will start a direct match challenge, and the button in the corner will either Add this user to or Delete it from the current players friends list
    /// </summary>
    private void DisplayUsersInScrollView(IList<Player> players, GameObject prefab, string buttonName, Action<string> buttonFunction) {
        // Remove all Elements currently in the scrollview
        foreach (Transform child in userScrollViewContent.transform) {
            Destroy(child.gameObject);
        }
        foreach (Player player in players) {
            GameObject user = Instantiate(prefab, userScrollViewContent.transform);
            string username = HelperMethods.GetUsernameWithoutPrefix(player.Name);
            Button challengeButton = user.transform.Find("ChallengeFriendButton").GetComponent<Button>();
            challengeButton.onClick.AddListener(delegate { ChallengeUser(player.Id, challengeButton.gameObject); });
            user.transform.Find(buttonName).GetComponent<Button>().onClick.AddListener(delegate { buttonFunction(player.Id); });
            user.transform.Find("Text").GetComponent<Text>().text = username; 
            HelperMethods.SetImage(user.transform.Find("Image").GetComponent<Image>(), player.Name);
        }
    }

    /// <summary>
    /// Clears the ScrollView and adds a list of all open game requests to it.
    /// </summary>
    private void DisplayGameRequestsInScrollView(GameRequestList response)
    {

        // Remove all Elements currently in the scrollview
        foreach (Transform child in requestScrollViewContent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GameRequest incomingRequests in response.Incoming)
        {
            string username = HelperMethods.GetUsernameWithoutPrefix(incomingRequests.Sender.Name);
            GameObject request = Instantiate(requestPrefab, requestScrollViewContent.transform);
            request.transform.Find("RefuseRequestButton").GetComponent<Button>().onClick.AddListener(delegate { DeleteRequest(incomingRequests.Id, request); });
            request.transform.Find("AcceptRequestButton").GetComponent<Button>().onClick.AddListener(delegate { AcceptRequest(incomingRequests.Id, request); });
            request.transform.Find("Text").GetComponent<Text>().text = username;
            HelperMethods.SetImage(request.transform.Find("Image").GetComponent<Image>(), incomingRequests.Sender.Name);
        }
        foreach (GameRequest outgoingRequests in response.Outgoing)
        {
            string username = HelperMethods.GetUsernameWithoutPrefix(outgoingRequests.Recipient.Name);
            GameObject request = Instantiate(requestPrefab, requestScrollViewContent.transform);
            request.transform.Find("RefuseRequestButton").GetComponent<Button>().onClick.AddListener(delegate { DeleteRequest(outgoingRequests.Id, request); });
            request.transform.Find("AcceptRequestButton").gameObject.SetActive(false);
            request.transform.Find("Text").GetComponent<Text>().text = username;
            HelperMethods.SetImage(request.transform.Find("Image").GetComponent<Image>(), outgoingRequests.Recipient.Name);
        }
    }

    /// <summary>
    /// Clears the ScrollView and adds a list of all running games to it.
    /// </summary>
    private void DisplayGamesInScrollView(IList<WikidataGame.Models.GameInfo> response) {
        // Remove all Elements currently in the scrollview
        foreach (Transform child in gamesScrollViewContent.transform) {
            Destroy(child.gameObject);
        }
        foreach (var game in response)
        {
            if (game.IsAwaitingOpponentToJoin == true) continue;
            GameObject currentGameObject = Instantiate(gameInstancePrefab, gamesScrollViewContent.transform);
            string username = HelperMethods.GetUsernameWithoutPrefix(game.Opponent.Name);
            currentGameObject.transform.Find("ForfeitGameButton").GetComponent<Button>().onClick.AddListener(delegate { ForfeitGame(game.GameId, currentGameObject, game.Opponent); });
            currentGameObject.GetComponent<Button>().onClick.AddListener(delegate { ContinueGame(game.GameId); });
            currentGameObject.transform.Find("Text").GetComponent<Text>().text = GAME_MATCH_MESSAGE + username;
            HelperMethods.SetImage(currentGameObject.transform.Find("Image").GetComponent<Image>(), game.Opponent.Name);
            bool yourTurn = game.NextMovePlayerId != game.Opponent.Id;
            currentGameObject.GetComponent<Image>().color = yourTurn ? yourTurnColor : oppentTurnColor;
        }
    }

    /// <summary>
    /// Clears the ScrollView and adds an overview of the last 5 games you played.
    /// </summary>
    public void DisplayGameHistoryInScrollView() {
        _uiController.DisplayGameHistoryTabUI();

        // Remove all Elements currently in the scrollview
        foreach (Transform child in gamesScrollViewContent.transform) {
            Destroy(child.gameObject);
        }
        foreach (FinishedGame game in gameHistory) {
            GameObject currentGameObject = Instantiate(gameHistoryPrefab, gamesScrollViewContent.transform);
            currentGameObject.transform.SetAsFirstSibling();
            currentGameObject.transform.Find("Text").GetComponent<Text>().text = game.GameDescription;
            Button rematchButton = currentGameObject.transform.Find("RematchButton").GetComponent<Button>();
            rematchButton.onClick.AddListener(delegate { ChallengeUser(game.OpponentID, rematchButton.gameObject); });
            rematchButton.gameObject.SetActive(game.OpponentName != BOT_NAME);
            Button addFriendButton = currentGameObject.transform.Find("AddFriendButton").GetComponent<Button>();
            addFriendButton.onClick.AddListener(delegate { 
                AddFriend(game.OpponentID);
                addFriendButton.gameObject.SetActive(false);
            });
            bool alreadyFriends = FriendsList.Any(friend => friend.Id == game.OpponentID);
            addFriendButton.gameObject.SetActive(!alreadyFriends && game.OpponentName != BOT_NAME);

            HelperMethods.SetImage(currentGameObject.transform.Find("Image").GetComponent<Image>(), game.OpponentName);
            currentGameObject.GetComponent<Image>().color = game.ResultColor;
        }
    }

    private void ContinueGame(string gameId) {
        Communicator.SetCurrentGameId(gameId);
        GameManager.Instance.ChangeToGameScene();
    }

    private void ForfeitGame(string id, GameObject gameInstance, Player opponent) {
        string headline = "Forfeit game";
        string message = "Are you sure you want to forfeit this game? This counts as a loss.";
        _uiController.OpenConfirmDialog(headline, message, delegate { ForfeitGameConfirm(id, gameInstance, opponent); });
    }

    private async void ForfeitGameConfirm(string id, GameObject gameInstance, Player opponent) {
        bool success = await Communicator.DeleteGame(id);
        if (success) {
            AddGameToHistory(opponent, 0, 60);
            Destroy(gameInstance);
        }
        else {
            Debug.LogError("An Error occurred. Couldn't delete game.");
        }
    }

    private async void AcceptRequest(string requestId, GameObject requestObject)
    {
        WikidataGame.Models.GameInfo newGameInfo = await Communicator.AcceptGameRequest(requestId);
        if (newGameInfo != null){
            Destroy(requestObject);
            ContinueGame(newGameInfo.GameId);
        }
        else{
            Debug.LogError("An Error occurred. Couldn't accept game request");
        }
    }

    private async void DeleteRequest(string id, GameObject requestObject)
    {
        bool succesful = await Communicator.DeleteGameRequest(id);
        if (succesful){
            Destroy(requestObject);
        }
        else{
            Debug.LogError("An Error occurred. Couldn't delete game request");
        }
    }

    public async void ChallengeUser(string userID, GameObject challengeButton) {
        challengeButton.SetActive(false);
        GameRequest request = await Communicator.ChallengeUser(userID);
    }

    public async void AddFriend(string userID) {
        Player newFriend = await Communicator.AddFriend(userID);
        FriendsList.Add(newFriend);
        //Display the friend list with the newly added friend
        DisplayUsersInScrollView(FriendsList, friendPrefab, FRIEND_CORNERBUTTON_NAME, DeleteFriend);
        _uiController.DisplayFriendsListUI();
    }

    public void DeleteFriend(string userID) {
        string headline = "Remove user from friends";
        string message = "Are you sure you want to remove this user from your friends list?";
        _uiController.OpenConfirmDialog(headline, message, delegate { DeleteFriendConfirm(userID); });
    }

    private async void DeleteFriendConfirm(string userID) {
        Player formerFriend = await Communicator.DeleteFriend(userID);
        FriendsList.RemoveAll(x => x.Id == userID);
        //Display the friend list without the deleted friend
        DisplayUsersInScrollView(FriendsList, friendPrefab, FRIEND_CORNERBUTTON_NAME, DeleteFriend);
    }


    /// <summary>
    /// Retrieve all open games of the player that is currently logged in
    /// </summary>
    public async Task<bool> RetrieveGames() {

        Task<IList<WikidataGame.Models.GameInfo>> retrieveGames = Communicator.RetrieveGames();
        IList<WikidataGame.Models.GameInfo> response = await retrieveGames;

        if (response == null) {
            Debug.LogError("Couldn't retrieve running games");
            return false;
        }
        else {
            DisplayGamesInScrollView(response);
            return true;
        }
    }

    /// <summary>
    /// Retrieve all open game requests of the player that is currently logged in
    /// </summary>
    public async Task<bool> RetrieveGameRequests()
    {

        Task<GameRequestList> retrieveGameRequests = Communicator.RetrieveGameRequests();
        GameRequestList response = await retrieveGameRequests;

        if (response == null)
        {
            Debug.LogError("Couldn't retrieve game requests");
            return false;
        }
        else
        {
            DisplayGameRequestsInScrollView(response);
            return true;
        }
    }

    public async void RetrieveGameRequestsVoid()
    {
        await RetrieveGameRequests();
    }

    public async void RetrieveRunningGamesVoid() {
        await RetrieveGames();
    }


    /// <summary>
    /// Use this to show the leaderboard.
    /// </summary>
    public void ShowLeaderboard() {
        Social.ShowLeaderboardUI();
    }

    /// <summary>
    /// Use this to show the achievements.
    /// </summary>
    public void ShowAchievements() {
        Social.ShowAchievementsUI();
    }

    /// <summary>
    /// Use this to unlock the explorer achievements.
    /// </summary>
    public static void UnlockExplorerAchievement() {
        PlayGamesPlatform.Instance.ReportProgress("CgkI-f_-2q4eEAIQAg", 100.0f, (bool success) => {
            // handle success or failure
        });
    }

    /// <summary>
    /// Use this to unlock the explorer achievements.
    /// </summary>
    public static void UnlockDominatorAchievement() {
        PlayGamesPlatform.Instance.ReportProgress("CgkI-f_-2q4eEAIQBg", 100.0f, (bool success) => {
            // handle success or failure
        });
    }

    /// <summary>
    /// Use this to unlock the explorer achievements.
    /// </summary>
    public static void UnlockContributorAchievement() {
        PlayGamesPlatform.Instance.ReportProgress("CgkI-f_-2q4eEAIQBw", 100.0f, (bool success) => {
            // handle success or failure
        });
    }


    /// <summary>
    /// Use this to post the score.
    /// </summary>
    public static void PostScore(long score) {
        Social.ReportScore((int)score, "CgkI-f_-2q4eEAIQAQ", (bool success) => {
            Debug.Log($"Score posted to Leaderboard: {(int)score}");
            // handle success or failure
        });
    }

    private bool RetrieveGamesForGameHistory() {
        string history = PlayerPrefs.GetString(PLAYERPREFS_GAMEHISTORY);

        if (string.IsNullOrEmpty(history)) {
            Debug.Log("Game History not found"); 
            return false; 
        }
        Debug.Log(history);
        List<string> finishedGames = history.Split(';').ToList();

        foreach(string finishedGameAsString in finishedGames) {
            Debug.Log(finishedGameAsString);
            List<string> gameAsStrings = finishedGameAsString.Split(':').ToList();
            string opponentID = gameAsStrings[0];
            string opponentName = gameAsStrings[1];
            bool isParsable1 = int.TryParse(gameAsStrings[2], out int playerScore);
            bool isParsable2 = int.TryParse(gameAsStrings[3], out int opponentScore);

            if(isParsable1 && isParsable2) {
                FinishedGame finishedGame = new FinishedGame(opponentID, opponentName, playerScore, opponentScore );
                gameHistory.Add(finishedGame);
            }
            else {
                Debug.LogError("Couldn't parse scores for GameHistory.");
                return false;
            }
        }
        return true;

    }


    /// <summary>
    /// Add the recently finished game to the players game history
    /// </summary>
    public static void AddGameToHistory(Player opponent, int playerScore, int opponentScore) {
        gameHistory.Add(new FinishedGame(opponent.Id, opponent.Name, playerScore, opponentScore));
        if(gameHistory.Count > 5) {
            gameHistory.RemoveAt(0);
        }

        string updatedPlayerprefs = "";
        foreach(FinishedGame finishedGame in gameHistory) {
            updatedPlayerprefs += finishedGame.ToString();
        }
        updatedPlayerprefs = updatedPlayerprefs.Substring(0, updatedPlayerprefs.Length - 1);
        PlayerPrefs.SetString(PLAYERPREFS_GAMEHISTORY, updatedPlayerprefs);
    }
}





internal class FinishedGame {
    internal string OpponentName { get; }
    internal string OpponentID { get; }
    internal int OpponentScore { get; }
    internal int PlayerScore { get; }
    internal int GameResult { get { return PlayerScore - OpponentScore; } }

    internal string GameDescription {
        get {
            if (GameResult < 0) {
                return $"You lost {PlayerScore} : {OpponentScore} vs {HelperMethods.GetUsernameWithoutPrefix(OpponentName)}";
            }
            else if (GameResult > 0) {
                return $"You won {PlayerScore} : {OpponentScore} vs {HelperMethods.GetUsernameWithoutPrefix(OpponentName)}";
            }
            else {
                return $"Draw! {PlayerScore} : {OpponentScore} vs {HelperMethods.GetUsernameWithoutPrefix(OpponentName)}";
            }
        }
    }

    private Color drawColor = new Color(187f/255, 204f/255, 170f/255);
    private Color winColor = new Color(80f/255, 158f/255, 158f/255);
    private Color loseColor = new Color(79f/255, 106f/255, 106f/255, 212f/255);

    internal Color ResultColor {
        get {
            if (GameResult < 0) {
                return loseColor;
            }
            else if (GameResult > 0) {
                return winColor;
            }
            else {
                return drawColor;
            }
        }
    }
    public FinishedGame(string opponentID, string opponentName, int playerScore, int opponentScore) {
        OpponentID = opponentID;
        OpponentName = opponentName;
        OpponentScore = opponentScore;
        PlayerScore = playerScore;
    }

    public override string ToString() {
        return $"{OpponentID}:{OpponentName}:{PlayerScore}:{OpponentScore};";
    }
}