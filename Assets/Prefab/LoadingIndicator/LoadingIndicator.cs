using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingIndicator : MonoBehaviour
{

    /**
     * private fields
     */
    private CanvasGroup _canvasGroup => GetComponentInChildren<CanvasGroup>();
    private Animator _animator => GetComponentInChildren<Animator>();
    private Image _image => 
        transform.Find("CanvasGroup/Image")
        .GetComponent<Image>();
    
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
        _image.rectTransform.sizeDelta = new Vector2(300f, 300f);
        _image.transform.localPosition = new Vector2(0f, 0f);
        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
        _animator.enabled = true;
    }
    
    public void ShowWithoutBlockingUI()
    {
        _image.rectTransform.sizeDelta = new Vector2(150f, 150f);
        _image.transform.localPosition = new Vector2(-183.0f, 213.0f);
        _canvasGroup.alpha = 1;
        _animator.enabled = true;
    }

    public void Hide()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _animator.enabled = false;
    }
}
