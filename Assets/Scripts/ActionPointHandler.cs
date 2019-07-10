
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionPointHandler : MonoBehaviour
{
	/**
	 * public fields
	 */

	public Text turnIndicator; 
	public List<GameObject> actionPoints;

	/**
	 * private fields
	 */

	private int _remainingActionPoints;
	private CanvasGroup _canvasGroup => GetComponent<CanvasGroup>();
	private int resetFailedCount;

	/**
	 * static fields
	 */

	public static ActionPointHandler Instance;

	/**
	 * constant fields
	 */

	private const int TOTAL_ACTION_POINTS = 3;
	private const string REMAINING_ACTION_POINTS = "REMAINING_ACTION_POINTS";

	/**
	 * set action points to max value
	 */
	public void Awake()
	{
		Instance = this;
		resetFailedCount = 0;
		Reset();
	}

	private void RemoveActionPoint()
	{
		int size = actionPoints.Count;
		
		if (size < 1)
			throw new Exception("There needs to be at least one action point in total.");
		
		if(_remainingActionPoints == 0)
			throw new Exception("There is no action point left to remove.");
		
		actionPoints[_remainingActionPoints - 1].SetActive(false);
		_remainingActionPoints--;
		PlayerPrefs.SetInt(REMAINING_ACTION_POINTS, _remainingActionPoints);

	}

	/**
	 * when a new turn begins,
	 * reset all action points in UI and the PlayerPrefs
	 */
	private void ResetActionPoints()
	{
		foreach(var actionPoint in actionPoints) actionPoint.SetActive(true);
		_remainingActionPoints = 3;
		PlayerPrefs.SetInt(REMAINING_ACTION_POINTS, _remainingActionPoints);
	}

	/**
	 * when starting, try to reset action points by looking them up in the player prefs
	 * currently limited to one game at a time, not sure how script behaves if otherwise
	 */
	private void Reset()
	{
		var fetchedPoints = PlayerPrefs.GetInt(REMAINING_ACTION_POINTS, -1);
		
		// if fetched points are not found, use default setting
		if (fetchedPoints == -1)
		{
			_remainingActionPoints = TOTAL_ACTION_POINTS;
			turnIndicator.text = "your turn:";
			foreach(var actionPoint in actionPoints)
				actionPoint.SetActive(true);
		}
		else
		{
			_remainingActionPoints = fetchedPoints;
			if (_remainingActionPoints > 0)
			{
				turnIndicator.text = "your turn:";
				for (int i = 0; i < actionPoints.Count; i++)
				{
					actionPoints[i].SetActive(i < _remainingActionPoints);
				}
			}
			else
			{
				resetFailedCount++;
				
				turnIndicator.text = "your turn:";
				foreach(var actionPoint in actionPoints) 
					actionPoint.SetActive(false);
				
				if(resetFailedCount < 3) 
					Reset();
				else
					resetFailedCount = 0;

			}

		}
	}

	/**
	 * this function is called in the MenuController
	 * it is only called after a minigame on this client side is finished
	 * resetting the state has to be handled elsewhere (not sure where)
	 */
	public void UpdateState(string myId, string otherId, bool isNewTurn)
	{
		if(!isNewTurn) 
			RemoveActionPoint();
		else
		{
			ResetActionPoints();
		}
		
		Debug.Log($"myID:{myId} // otherId: {otherId}");
		Debug.Log($"actionPoints Remaining:{_remainingActionPoints}");
		if (myId != otherId)
			Hide();
		else
			Show();
		

	}
	
	public void DeleteKey()
	{
		PlayerPrefs.DeleteKey(REMAINING_ACTION_POINTS);
	}
	
	/**
	 * show GameStateCanvas
	 */
	public void Show()
	{
		_canvasGroup.alpha = 1;
		_canvasGroup.blocksRaycasts = true;
	}
	
	/**
	 * hide GameStateCanvas
	 */
	public void Hide()
	{
		_canvasGroup.alpha = 0;
		_canvasGroup.blocksRaycasts = false;
	}
	
}
