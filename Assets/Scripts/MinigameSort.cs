using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameSort : MonoBehaviour
{
    public List<GameObject> sources;
    public List<GameObject> targets;
    public GameObject property;
    public GameObject closePanel;

    // TODO: generate class for answers // setting through backend  // get-request
    private HashSet<string> answers;

    // Start is called before the first frame update
    void Start()
    {
        property.GetComponent<Text>().text = "PROPERTY";
        Set();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void  Set()
    {
       // TODO: get images from answers and inject to sources
       foreach(GameObject source in sources)
        {
            Image img = source.GetComponentInChildren<Image>();
            // img.sprite  = ...
        }
    }

    public void Send()
    {
        // TODO: post-request to  frontend --> Communicator
        foreach(GameObject target in targets)
        {
            //  target.GetComponent<Attribute>().attribute;

        }

        Debug.Log("SEND:Please implement this!");
    }
}
