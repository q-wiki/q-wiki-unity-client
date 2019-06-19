using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WikidataGame;
using WikidataGame.Models;

public class TileController : MonoBehaviour
{
    public string id;
    public string ownerId;

    public int difficulty;
    public IList<WikidataGame.Models.Category> availableCategories;
    public WikidataGame.Models.Category chosenCategories;

    void Start()
    {
    }

    void Update()
    {
        
    }
}
