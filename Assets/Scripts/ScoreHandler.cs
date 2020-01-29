using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WikidataGame.Models;

/// <summary>
///     This class is used to handle the score of the client.
///     It also handles the number of remaining turns and shows it to the user.
/// </summary>
public class ScoreHandler : Singleton<ScoreHandler>
{
    private const string PLAYERPREFS_CURRENT_GAME_TURNS_PLAYED = "CURRENT_GAME_TURNS_PLAYED";
    
    /*
     * public fields
     */ 
    
    public long playerScore;
    public long opponentScore;
    internal int turnsPlayed;
    
    /**
     * private fields
     */

    private Text _opponentScoreText;
    private Text _playerScoreText;
    private Text _turnsPlayedText;
    private string _gameId;
    private CanvasGroup _canvasGroup => GetComponent<CanvasGroup>();

    /// <summary>
    ///     On awake, some variables get filled with components.
    /// </summary>
    public void Awake()
    {
        _playerScoreText = transform.Find("You").GetComponentInChildren<Text>();
        _opponentScoreText = transform.Find("They").GetComponentInChildren<Text>();
        _turnsPlayedText = transform.Find("Turns").GetComponentInChildren<Text>();
        turnsPlayed = PlayerPrefs.GetInt($"{_gameId}/{PLAYERPREFS_CURRENT_GAME_TURNS_PLAYED}", 0);
    }

    /// <summary>
    ///     When the ScoreHandler gets called, the text of the turn indicator is set.
    /// </summary>
    public void Start()
    {
        _turnsPlayedText.text = $"{turnsPlayed} / 6 Turns";
    }
    
    /// <summary>
    ///     Sets the current game id.
    /// </summary>
    /// <param name="gameId">Current game ID</param>
    public void SetGameId(string gameId)
    {
        _gameId = gameId;
    }
    
    /// <summary>
    /// Reads the current turn count from player prefs
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void ReadCurrentTurnCountFromPrefs()
    {
        turnsPlayed = PlayerPrefs.GetInt($"{_gameId}/{PLAYERPREFS_CURRENT_GAME_TURNS_PLAYED}", 0);
        _turnsPlayedText.text = $"{turnsPlayed} / 6 Turns";
    }

    /// <summary>
    ///     This function is used to update the current score of the client.
    /// </summary>
    /// <param name="t">The grid as a two-dimensional array of tiles</param>
    /// <param name="myId">The ID of the client</param>
    /// <param name="opponentId">The ID of the opponent</param>
    public void UpdatePoints(IList<IList<Tile>> t, string myId, string opponentId)
    {
        /**
         * reset player scores before counting again
         */
        playerScore = 0;
        opponentScore = 0;

        // flatten input to one dimension
        var tiles = t.SelectMany(x => x);

        /**
         * check every tile if it is valid / has an owner
         * add (difficulty + 1) to player's score
         */
        foreach (var tile in tiles)
        {
            if (tile == null)
                continue;

            var ownerId = tile.OwnerId;
            long difficulty = tile.Difficulty + 1 ?? 0;

            if (string.IsNullOrEmpty(ownerId))
                continue;

            if (tile.OwnerId == myId)
                playerScore += difficulty;
            else
                opponentScore += difficulty;
        }

        /**
         * update UI text
         */

        _playerScoreText.text = $"{playerScore}";
        _opponentScoreText.text = $"{opponentScore}";
    }

    /// <summary>
    ///     This function is used to update the remaining turns of the client.
    /// </summary>
    public void UpdateTurns()
    {
        turnsPlayed++;
        PlayerPrefs.SetInt($"{_gameId}/{PLAYERPREFS_CURRENT_GAME_TURNS_PLAYED}", turnsPlayed);
        _turnsPlayedText.text = $"{turnsPlayed} / 6 Turns";
    }

    /// <summary>
    ///     This function is used to show score and remaining points to the user.
    /// </summary>
    public void Show()
    {
        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    ///     This function is used to hide score and remaining points from the user.
    /// </summary>
    public void Hide()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
    }

}