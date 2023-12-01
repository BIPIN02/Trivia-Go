using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using UnityEngine.UI;
using System.Collections;
using SimpleJSON;
using System;
using UnityEngine.Networking;
using Facebook.MiniJSON;
using Firebase.Auth;

public class FBController :MonoBehaviour
{
    void Awake()
    {
        //PlayerPrefs.DeleteAll();
        if (!FB.IsInitialized)
        {
            FB.Init(Initialize, OnHideUnity);
        }
    }
    void Initialize()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    void OnHideUnity(bool isUnityShown)
    {
        if (!isUnityShown)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

}


