using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

public class UIManager : MySingleton<UIManager>
{
    [Header("Panel Details")]
    [Space(10)]

    public GameObject dashboardPanel;
    public GameObject loginScreenPanel;
    public GameObject registerScreenPanel;
    public GameObject forgetPasswordPanel;
    public GameObject levelScreenPanel;
    public GameObject gamePlayScreenPanel;
    public GameObject WinningPopupPanel;
    public GameObject WinningMultiplayerPopupPanel;
    public GameObject loadingPopupPannel;
    public GameObject selectionPopupPanel;
    public GameObject emailVerificationPanel;
    public GameObject multiplayerGameScene;
    public GameObject waitingScreen;
    public GameObject leaderboardPopupPanel;
    public GameObject settingPopupPanel;
    public GameObject iapPopupPanel;
    [SerializeField] private TextMeshProUGUI emailVerificationText;
    
    [Header("Error panel ")]
    [Space(10)]

    public GameObject errorPanel;
    public Text ErrorTxt;
    public UserDetail userDetail = new UserDetail();
    public List<LeaderBoard> leaderBoard = new List<LeaderBoard>();
    public Canvas canvas;
    // Start is called before the first frame update
    void Start()
    {

    }
    public void ShowVerificationResponse(bool isEmailSent, string emailId, string errorMessage)
    {
        SetAcitveObject(emailVerificationPanel);
        if (isEmailSent)
        {
            emailVerificationText.text = $"Please verify your email address \n Verification email has been sent to {emailId}";
        }
        else
        {
            emailVerificationText.text = $"Couldn't sent email : {errorMessage}";
        }
    }

    public void SetAcitveObject(GameObject go)
    {
        Debug.Log("user name" +userDetail.userName);
        for (int i = 0;i<canvas.transform.childCount;i++)
        {
            canvas.transform.GetChild(i).gameObject.SetActive(false);
        }
        go.SetActive(true);
    }
  
    
}
[System.Serializable]
public class UserDetail
{
    public string userName { get; set; }
    public string userID { get; set; }
    public int coins { get; set; }
    public int gems { get; set; }
    public int level { get; set; }
    public int score { get; set; }
    public int correctAnser { get; set; }
    public int bomb { get; set; }
    public int checkMark { get; set; }
}
[System.Serializable]
public class LeaderBoard
{
    public string userName { get; set; }
    public int score { get; set; }

}