using UnityEngine;
using UnityEngine.UI;

public class ErrorDialog : MonoBehaviour
{
    private Text _text;
    private Button _okButton;
    
    /// <summary>
    /// Instantiate error dialog with message.
    /// </summary>
    /// <param name="message">Message to be displayed to the user</param>
    public void Instantiate(string message)
    {
        _text = transform.Find("Panel/Message")
            .GetComponent<Text>();
        _okButton = transform.Find("Panel/MenuGrid/OKButton")
            .GetComponent<Button>();
        
        _text.text = message;
        _okButton.onClick.AddListener(delegate { Destroy(gameObject); });

    }
    
}
