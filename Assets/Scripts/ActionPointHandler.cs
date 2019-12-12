using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     This class handles all action point behaviour.
/// </summary>
public class ActionPointHandler : Singleton<ActionPointHandler>
{
    /**
     * constant fields
     */

    private const int TOTAL_ACTION_POINTS = 3;
    private const string REMAINING_ACTION_POINTS = "REMAINING_ACTION_POINTS";

    /**
     * private fields
     */

    private int _remainingActionPoints;
    public List<GameObject> actionPoints;

    private int resetFailedCount;
    /**
     * public fields
     */

    public Text turnIndicator;
    private CanvasGroup _canvasGroup => GetComponent<CanvasGroup>();


    /// <summary>
    ///     Actions points are set to their maximum value.
    /// </summary>
    public void Awake()
    {
        resetFailedCount = 0;
        Reset();
    }

    /// <summary>
    ///     This function is used to remove an action point.
    /// </summary>
    /// <exception cref="Exception">When there is no action point, this function should not be called.</exception>
    private void RemoveActionPoint()
    {
        var size = actionPoints.Count;

        if (size < 1)
            throw new Exception("There needs to be at least one action point in total.");

        if (_remainingActionPoints == 0)
            throw new Exception("There is no action point left to remove.");

        actionPoints[_remainingActionPoints - 1].SetActive(false);
        _remainingActionPoints--;
        PlayerPrefs.SetInt(REMAINING_ACTION_POINTS, _remainingActionPoints);
    }

    /// <summary>
    ///     When a new turn begins, this function is used to reset action points for a user.
    /// </summary>
    private void ResetActionPoints()
    {
        foreach (var actionPoint in actionPoints) actionPoint.SetActive(true);
        _remainingActionPoints = 3;
        PlayerPrefs.SetInt(REMAINING_ACTION_POINTS, _remainingActionPoints);
    }

    /// <summary>
    ///     When starting, try to reset action points by looking them up in the PlayerPrefs.
    ///     Currently limited to one game at a time, not sure how script behaves if otherwise.
    /// </summary>
    private void Reset()
    {
        var fetchedPoints = PlayerPrefs.GetInt(REMAINING_ACTION_POINTS, -1);

        // if fetched points are not found, use default setting
        if (fetchedPoints == -1)
        {
            _remainingActionPoints = TOTAL_ACTION_POINTS;
            turnIndicator.text = "your turn:";
            foreach (var actionPoint in actionPoints)
                actionPoint.SetActive(true);
        }
        else
        {
            _remainingActionPoints = fetchedPoints;
            if (_remainingActionPoints > 0)
            {
                turnIndicator.text = "your turn:";
                for (var i = 0; i < actionPoints.Count; i++) actionPoints[i].SetActive(i < _remainingActionPoints);
            }
            else
            {
                resetFailedCount++;

                turnIndicator.text = "your turn:";
                foreach (var actionPoint in actionPoints)
                    actionPoint.SetActive(false);

                if (resetFailedCount < 3)
                    Reset();
                else
                    resetFailedCount = 0;
            }
        }
    }

    /// <summary>
    ///     This function is called in the GameController.
    ///     It is only called after a MiniGame on this client side is finished.
    ///     Resetting the state has to be handled elsewhere (not sure where).
    /// </summary>
    /// <param name="myId">ID of the player.</param>
    /// <param name="otherId">ID of the other player.</param>
    /// <param name="isNewTurn">Indicates if a new turn has just started.</param>
    public void UpdateState(Guid? myId, Guid? otherId, bool isNewTurn)
    {
        if (!isNewTurn)
            RemoveActionPoint();
        else
            ResetActionPoints();

        Debug.Log($"myID:{myId} // otherId: {otherId}");
        Debug.Log($"actionPoints Remaining:{_remainingActionPoints}");
        if (myId != otherId)
            Hide();
        else
            Show();
    }

    /// <summary>
    ///     Tihs function is used to delete information about the action points from the PlayerPrefs.
    /// </summary>
    public void DeleteKey()
    {
        PlayerPrefs.DeleteKey(REMAINING_ACTION_POINTS);
    }

    /// <summary>
    ///     This is used to show the GameStateCanvas.
    /// </summary>
    public void Show()
    {
        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    ///     This is used to hide the GameStateCanvas.
    /// </summary>
    public void Hide()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
    }
}