using System;
using UnityEngine;

/// <summary>
/// This type represents a Canvas object which can be hidden and shown.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class Hideable : MonoBehaviour
{
    public bool IsShownInitially;

    private CanvasGroup _canvasGroup => GetComponent<CanvasGroup>();
    private bool _isVisible;
    public bool IsVisible => _isVisible;

    /// <summary>
    /// If object is highlighted for initial use, it is immediately shown.
    /// </summary>
    public void Start()
    {
        if(IsShownInitially)
            Show();
        else Hide();
    }


    /// <summary>
    /// This function is used to make the object visible.
    /// </summary>
    public void Show()
    {
        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
        _isVisible = true;
    }
    
    /// <summary>
    /// This function is used to hide the object.
    /// </summary>
    public void Hide()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _isVisible = false;
    }
    
}
