﻿using Controllers;
using Controllers.Authentication;
using Controllers.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WikidataGame.Models;

public class AccountController : MonoBehaviour
{
    public List<Player> friendsList { get; private set; }

    private StartUIController _uiController => (StartUIController)GameManager.Instance.UIController();
    [SerializeField] private GameObject userPrefab;
    [SerializeField] private GameObject friendPrefab;
    [SerializeField] private GameObject requestPrefab;
    [SerializeField] private GameObject gameInstancePrefab;
    [SerializeField] private GameObject userScrollViewContent;
    [SerializeField] private GameObject requestScrollViewContent;
    [SerializeField] private GameObject gamesScrollViewContent;
    [SerializeField] private Sprite incomingSprite;
    [SerializeField] private Sprite outgoingSprite;
    [SerializeField] private Text usernameText;
    [SerializeField] private Image avatarImage;
    private Color incomingColor = Color.white;
    private Color outgoingColor = Color.red;

    private const string USER_CORNERBUTTON_NAME = "AddFriendButton";
    private const string FRIEND_CORNERBUTTON_NAME = "DeleteFriendButton";

    // Start is called before the first frame update
    async void Start(){
        SetHeadline();
        await RetrieveFriends();

        //_uiController.findUserInput.onEndEdit.RemoveAllListeners();
        _uiController.findUserInput.onValueChanged.RemoveAllListeners();

        //_uiController.findUserInput.onEndEdit.AddListener(delegate { inputSubmitCallBack(); });
        _uiController.findUserInput.onValueChanged.AddListener(delegate { inputChangedCallBack(); });
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
        _uiController.findUserInput.onValueChanged.RemoveAllListeners();
    }

    private void inputSubmitCallBack(){
        FindUsers();
    }


    /// <summary>
    /// Set Username and Avatar Image (if available) in the headline
    /// </summary>
    public void SetHeadline(){
        bool usernameInPlayerprefs = !string.IsNullOrEmpty(PlayerPrefs.GetString(SignInController.PLAYERPREFS_USERNAME));
        string name = usernameInPlayerprefs ? PlayerPrefs.GetString(SignInController.PLAYERPREFS_USERNAME) : "Username";
        usernameText.text = name;

        bool avatarExists = Social.localUser.image != null;
        if (avatarExists){
            Texture2D texture = Social.localUser.image;
            avatarImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
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

        _uiController.DisplayFriendsListUI();

        Task<IList<Player>> retrieveFriends = Communicator.RetrieveFriends();
        IList<Player> response = await retrieveFriends;

        if (response == null) {
            Debug.LogError("Couldn't retrieve friends");
            return false;
        }
        else {
            friendsList = (List<Player>)response;
            DisplayUsersInScrollView(response, friendPrefab, FRIEND_CORNERBUTTON_NAME, DeleteFriend);
            return true;
        }
    }
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
            user.GetComponent<Button>().onClick.AddListener(delegate { ChallengeUser(player.Id); });
            user.transform.Find(buttonName).GetComponent<Button>().onClick.AddListener(delegate { buttonFunction(player.Id); });
            user.transform.Find("Text").GetComponent<Text>().text = player.Name;
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
            GameObject request = Instantiate(requestPrefab, requestScrollViewContent.transform);
            request.transform.Find("RefuseRequestButton").GetComponent<Button>().onClick.AddListener(delegate { DeleteRequest(incomingRequests.Id, request); });
            request.transform.Find("AcceptRequestButton").GetComponent<Button>().onClick.AddListener(delegate { AcceptRequest(incomingRequests.Id, request); });
            request.transform.Find("Text").GetComponent<Text>().text = incomingRequests.Sender.Name;
            Transform inOrOut = request.transform.Find("Image/InOrOut");
            inOrOut.GetComponent<Image>().sprite = incomingSprite;
            inOrOut.GetComponent<Image>().color = incomingColor;
        }
        foreach (GameRequest outgoingRequests in response.Outgoing)
        {
            GameObject request = Instantiate(requestPrefab, requestScrollViewContent.transform);
            request.transform.Find("RefuseRequestButton").GetComponent<Button>().onClick.AddListener(delegate { DeleteRequest(outgoingRequests.Id, request); });
            request.transform.Find("AcceptRequestButton").gameObject.SetActive(false);
            request.transform.Find("Text").GetComponent<Text>().text = outgoingRequests.Recipient.Name;
            Transform inOrOut = request.transform.Find("Image/InOrOut");
            inOrOut.GetComponent<Image>().sprite = outgoingSprite;
            inOrOut.GetComponent<Image>().color = outgoingColor;
        }
    }

    /// <summary>
    /// Clears the ScrollView and adds a list of all running games to it.
    /// </summary>
    private void DisplayGamesInScrollView(IList<GameInfo> response) {
        // Remove all Elements currently in the scrollview
        foreach (Transform child in gamesScrollViewContent.transform) {
            Destroy(child.gameObject);
        }
        foreach (GameInfo game in response) {
            GameObject currentGameObject = Instantiate(gameInstancePrefab, gamesScrollViewContent.transform);
            currentGameObject.transform.Find("ForfeitGameButton").GetComponent<Button>().onClick.AddListener(delegate { ForfeitGame(game.GameId, currentGameObject); });
            currentGameObject.GetComponent<Button>().onClick.AddListener(delegate { ContinueGame(game.GameId); });
            currentGameObject.transform.Find("Text").GetComponent<Text>().text = game.Message;
            Transform inOrOut = currentGameObject.transform.Find("InOrOut");
            inOrOut.GetComponent<Image>().sprite = incomingSprite;
            inOrOut.GetComponent<Image>().color = incomingColor;
        }
    }

    private void ContinueGame(string gameId) {
        Communicator.SetCurrentGameId(gameId);
        GameManager.Instance.ChangeToGameScene();
    }

    private async void ForfeitGame(string id, GameObject gameInstance) {
        bool success = await Communicator.DeleteGame(id);
        //TODO
        if (success) {
            Destroy(gameInstance);
            Debug.Log("TODO Add multiple game instances");
        }
        else {
            Debug.LogError("An Error occurred. Couldn't delete game.");
        }
    }

    private async void AcceptRequest(string gameId, GameObject requestObject)
    {
        GameInfo newGameInfo = await Communicator.AcceptGameRequest(gameId);
        PlayerPrefs.SetString(Communicator.PLAYERPREFS_CURRENT_GAME_ID, gameId);
        if (newGameInfo != null)
        {
            Destroy(requestObject);
            Communicator.SetCurrentGameId(gameId);
            GameManager.Instance.ChangeToGameScene();
        }
        else
        {
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

    public async void ChallengeUser(string userID) {
        GameRequest request = await Communicator.ChallengeUser(userID);
    }

    public async void AddFriend(string userID) {
        Player newFriend = await Communicator.AddFriend(userID);
        friendsList.Add(newFriend);
        //Display the friend list with the newly added friend
        DisplayUsersInScrollView(friendsList, friendPrefab, "DeleteFriendButton", DeleteFriend);
    }

    public async void DeleteFriend(string userID) {
        Player formerFriend = await Communicator.DeleteFriend(userID);
        friendsList.RemoveAll(x => x.Id == userID);
        //Display the friend list without the deleted friend
        DisplayUsersInScrollView(friendsList, friendPrefab, "DeleteFriendButton", DeleteFriend);
    }

    /// <summary>
    /// Retrieve all open games of the player that is currently logged in
    /// </summary>
    public async Task<bool> RetrieveGames() {

        Task<IList<GameInfo>> retrieveGames = Communicator.RetrieveGames();
        IList<GameInfo> response = await retrieveGames;

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
}