using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Category : MonoBehaviour
{

    public GameObject categoryPanel, actionPanel;
    public Button c1, c2, c3;
    private string[] categorys;
    // Start is called before the first frame update

    void Awake()
    {
        categorys = new string[] { "Nature", "Culture", "Geography", "Space", "Natural Sciences", "Food", "History", "Celebrities", "Entertainment", "Politics", "Sports" };
    }
    void Start()
    {


    }

    void reshuffle(string[] texts)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < texts.Length; t++)
        {
            string tmp = texts[t];
            int r = Random.Range(t, texts.Length);
            texts[t] = texts[r];
            texts[r] = tmp;
        }
    }

    public void ShowCategoryPanel()
    {
        actionPanel.SetActive(false);
        categoryPanel.SetActive(true);
        reshuffle(categorys);
        /* Debug.Log("New Categorys ");
         for (int t = 0; t < categorys.Length; t++)
          {
              Debug.Log(categorys[t]);
          }*/

        c1.GetComponentInChildren<Text>().text = categorys[0];
        c2.GetComponentInChildren<Text>().text = categorys[1];
        c3.GetComponentInChildren<Text>().text = categorys[2];
    }

    public void ClosePanels()
    {
        categoryPanel.SetActive(false);
        actionPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
