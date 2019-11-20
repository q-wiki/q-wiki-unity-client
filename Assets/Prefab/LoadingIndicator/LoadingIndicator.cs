using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingIndicator : MonoBehaviour
{

    /**
     * private fields
     */
    private CanvasGroup _canvasGroup => GetComponentInChildren<CanvasGroup>();
    private Animator _animator => GetComponentInChildren<Animator>();
    
    /**
     * static fields
     */

    public static LoadingIndicator Instance;

    public void Awake()
    { 
        GameObject[] objs = GameObject.FindGameObjectsWithTag("LoadingIndicator");
        if(objs.Length > 1)
            Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
        
        Instance = this;
    }

    public void Show()
    {
        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
        _animator.enabled = true;
    }

    public void Hide()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _animator.enabled = false;
    }
}
