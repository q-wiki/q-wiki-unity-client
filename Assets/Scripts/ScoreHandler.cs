using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WikidataGame.Models;

public class ScoreHandler : MonoBehaviour
{
    
    /**
     * private fields
     */

    private long _playerScore;
    private long _opponentScore;

    private CanvasGroup _canvasGroup => GetComponent<CanvasGroup>();
    
    private Text _playerScoreText;
    private Text _opponentScoreText;
    private Text _turnsPlayedText;

    private int _turnsPlayed;
    
    private const string CURRENT_GAME_TURNS_PLAYED = "CURRENT_GAME_TURNS_PLAYED";

    /**
     * static fields
     */

    public static ScoreHandler Instance;

    public void Awake()
    {
        Instance = this;
        _playerScoreText = transform.Find("You").GetComponentInChildren<Text>();
        _opponentScoreText = transform.Find("They").GetComponentInChildren<Text>();
        _turnsPlayedText = transform.Find("Turns").GetComponentInChildren<Text>();
        _turnsPlayed = PlayerPrefs.GetInt(CURRENT_GAME_TURNS_PLAYED, 0);
    }

    public void Start()
    {
        _turnsPlayedText.text = $"{_turnsPlayed + 1} / 6 Turns";
    }

    public void UpdatePoints(IList<IList<Tile>> t, string myId, string opponentId)
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

            string ownerId = tile.OwnerId;
            long difficulty = tile.Difficulty + 1 ?? 0;

            if (String.IsNullOrEmpty(ownerId))
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

    public void UpdateTurns()
    {
        _turnsPlayed++;
        PlayerPrefs.SetInt(CURRENT_GAME_TURNS_PLAYED, _turnsPlayed);
        _turnsPlayedText.text = $"{_turnsPlayed + 1} / 6 Turns";

    } 
    
    public void Show()
    {
        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
    }
    
}
