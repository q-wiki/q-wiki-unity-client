using Controllers;
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
    [SerializeField] private GameObject scrollViewContent;

    // Start is called before the first frame update
    async void Start()
    {
        await RetrieveFriends();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /// <summary>
    /// Search for users matching the name from the input
    /// </summary>
    public async void FindUsers() {
        string username = (_uiController.findUserInput.text == "") ? "Anonymous User" : _uiController.findUserInput.text;
        
        Task<IList<Player>> findUsers = Communicator.FindUsers(username);
        IList<Player> response = await findUsers;

        if (response == null) {
            Debug.LogError("Couldn't retrieve users");
        }
        else {
            DisplayUsersInScrollView(response, userPrefab, "AddFriendButton", AddFriend);
        }
    }

    /// <summary>
    /// Retrieve the friends of the player that is currently logged in
    /// </summary>
    public async Task<bool> RetrieveFriends() {

        Task<IList<Player>> retrieveFriends = Communicator.RetrieveFriends();
        IList<Player> response = await retrieveFriends;

        if (response == null) {
            Debug.LogError("Couldn't retrieve friends");
            return false;
        }
        else {
            friendsList = (List<Player>)response;
            DisplayUsersInScrollView(response, friendPrefab, "DeleteFriendButton", DeleteFriend);
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
        foreach (Transform child in scrollViewContent.transform) {
            Destroy(child.gameObject);
        }
        foreach (Player player in players) {
            GameObject user = Instantiate(prefab, scrollViewContent.transform);
            user.GetComponent<Button>().onClick.AddListener(delegate { ChallengeUser(player.Id); });
            user.transform.Find(buttonName).GetComponent<Button>().onClick.AddListener(delegate { buttonFunction(player.Id); });
            user.transform.Find("Text").GetComponent<Text>().text = player.Name;
        }
    }

    public async void ChallengeUser(string userID) {
        Debug.LogError("Challenging Player not implemented");
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
}
