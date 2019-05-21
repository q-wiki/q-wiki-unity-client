using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource source { get { return GetComponent<AudioSource>(); } }
    public GameObject audioSource;
    public GameObject soundButtonIcon, notificationButtonIcon, vibrationButtonIcon;
    private GameObject startPanel, settingsPanel;
    public Sprite soundOff, soundOn, notifiactionOff, notificationOn, vibrationOff, vibrationOn;
    bool soundToggle, notificationToggle, vibrationToggle, settingsToggle = true;

    // Start is called before the first frame update
    void Start()
    {
        
        gameObject.AddComponent<AudioSource>();
        source.clip = clickSound;
        source.playOnAwake = false;

        Scene currentScene = SceneManager.GetActiveScene();

        string sceneName = currentScene.name;

        if (sceneName == "StartScene")
        {
            Debug.Log("StartScene");
            startPanel = GameObject.Find("StartPanel");
            settingsPanel = GameObject.Find("SettingsPanel");
            settingsPanel.SetActive(false);

        }
        else if (sceneName == "GameScene")
        {
            Debug.Log("GameScene");
            settingsPanel = GameObject.Find("SettingsPanelContainer");
            settingsPanel.SetActive(false);

        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlaySound()
    {
        source.PlayOneShot(clickSound);
    }



    public void ChangeScene(string sceneName)
    {

        SceneManager.LoadScene(sceneName);
    }

    public void ToggleSettingsGame()
    {
        settingsToggle = !settingsToggle;

        if (settingsToggle)
        {
            settingsPanel.SetActive(false);
        }
        else
        {
            settingsPanel.SetActive(true);
        }
    }



    public void ToggleSettingsStart()
    {
        settingsToggle = !settingsToggle;

        if (settingsToggle)
        {
            settingsPanel.SetActive(false);
            startPanel.SetActive(true);
        }
        else
        {
            settingsPanel.SetActive(true);
            startPanel.SetActive(false);
        }
    }


    public void ToggleSound()
    {
        soundToggle = !soundToggle;

        if (soundToggle)
        {
            //audioSource.SetActive(false);
            AudioListener.volume = 0;
            soundButtonIcon.GetComponent<Image>().sprite = soundOff;
        }
        else
        {
            audioSource.SetActive(true);
            AudioListener.volume = 1;
            soundButtonIcon.GetComponent<Image>().sprite = soundOn;
        }
    }

    public void ToggleNotification()
    {
        notificationToggle = !notificationToggle;

        if (notificationToggle)
        {
            Debug.Log("Notification Off");
            notificationButtonIcon.GetComponent<Image>().sprite = notifiactionOff;
        }
        else
        {
            Debug.Log("Notification On");
            notificationButtonIcon.GetComponent<Image>().sprite = notificationOn;
        }
    }

    public void ToggleVibration()
    {
        vibrationToggle = !vibrationToggle;

        if (vibrationToggle)
        {
            Debug.Log("Vibration Off");
            vibrationButtonIcon.GetComponent<Image>().sprite = vibrationOff;
        }
        else
        {
            Debug.Log("Vibration On");
            vibrationButtonIcon.GetComponent<Image>().sprite = vibrationOn;
        }
    }

    public void ToggleCreditsPanel()
    {
        Debug.Log("Credits");
    }
}
