using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GooglePlayGames;
using UnityEngine.SocialPlatforms;

public class SignInController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SignIn()
    {
        // authenticate user:
        Social.localUser.Authenticate((bool success) => {
            Debug.log(success);
            // handle success or failure
        });

    }
}
