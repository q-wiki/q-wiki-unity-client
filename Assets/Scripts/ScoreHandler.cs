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
    private const string CURRENT_GAME_TURNS_PLAYED = "CURRENT_GAME_TURNS_PLAYED";

    /**
     * private fields
     */

    private long _opponentScore;
    private Text _opponentScoreText;
    private long _playerScore;
    private Text _playerScoreText;
    private int _turnsPlayed;
    private Text _turnsPlayedText;
    private CanvasGroup _canvasGroup => GetComponent<CanvasGroup>();

    /// <summary>
    ///     On awake, some variables get filled with components.
    /// </summary>
    public void Awake()
    {
        _playerScoreText = transform.Find("You").GetComponentInChildren<Text>();
        _opponentScoreText = transform.Find("They").GetComponentInChildren<Text>();
        _turnsPlayedText = transform.Find("Turns").GetComponentInChildren<Text>();
        _turnsPlayed = PlayerPrefs.GetInt(CURRENT_GAME_TURNS_PLAYED, 0);
    }

    /// <summary>
    ///     When the ScoreHandler gets called, the text of the turn indicator is set.
    /// </summary>
    public void Start()
    {
        _turnsPlayedText.text = $"{_turnsPlayed} / 6 Turns";
    }

    /// <summary>
    ///     This function is used to update the current score of the client.
    /// </summary>
    /// <param name="t">The grid as a two-dimensional array of tiles</param>
    /// <param name="myId">The ID of the client</param>
    /// <param name="opponentId">The ID of the opponent</param>
    public void UpdatePoints(IList<IList<Tile>> t, Guid? myId, Guid? opponentId)
    {
        /**
         * reset player scores before counting again
         */
        _playerScore = 0;
        _opponentScore = 0;

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

            if (!ownerId.HasValue)
                continue;

            if (tile.OwnerId == myId)
                _playerScore += difficulty;
            else
                _opponentScore += difficulty;
        }

        /**
         * update UI text
         */

        _playerScoreText.text = $"{_playerScore}";
        _opponentScoreText.text = $"{_opponentScore}";
    }

    /// <summary>
    ///     This function is used to update the remaining turns of the client.
    /// </summary>
    public void UpdateTurns()
    {
        _turnsPlayed++;
        PlayerPrefs.SetInt(CURRENT_GAME_TURNS_PLAYED, _turnsPlayed);
        _turnsPlayedText.text = $"{_turnsPlayed} / 6 Turns";
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