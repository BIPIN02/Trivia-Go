using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class TableTimer : Photon.PunBehaviour
{
    public Text timerText;
    public Image progressBar;
    public string[] playerName;

    void OnEnable()
    {
        if (GameManager.Instance.isRoomCreated == false)
        {
            isActive = false;
            GameManager.Instance.isRoomCreated = true;
            NetworkController.instance.timeDuration = 20;
           
        }
    }
    void Update()
    {
        if (NetworkController.instance.isLoadScene == false)
        {
            if (NetworkController.instance.roomOwner)
            {
                TimerStart();
            }
        }
    }
    [PunRPC]
    public void ReceiveTime(string timer, float progressAmount, float timeduration,int seconds)
    {
        NetworkController.instance.timeDuration = timeduration;
        timerText.text = timer;
        progressBar.fillAmount = progressAmount;
        if (seconds != previoustime)
        {
            previoustime = seconds;
            if (seconds > 10)
                AudioManager.Instance.PlayTimer(0);
            else 
                AudioManager.Instance.PlayTimer(1);
        }

    }
    int previoustime = -1;
    void TimerStart()
    {
        NetworkController.instance.timeDuration -= Time.deltaTime;
        if (NetworkController.instance.timeDuration > 0)
        {
            int minutes = Mathf.FloorToInt(NetworkController.instance.timeDuration / 60f);
            int seconds = Mathf.RoundToInt(NetworkController.instance.timeDuration % 60f);

            if (seconds == 60)
            {
                seconds = 0;
                minutes += 1;
            }
            timerText.text =  seconds.ToString("00");

            float progressValue = NetworkController.instance.timeDuration / Mathf.Max(20, Mathf.Epsilon);

            progressBar.fillAmount = progressValue;
            photonView.RPC("ReceiveTime", PhotonTargets.All, timerText.text, progressBar.fillAmount, NetworkController.instance.timeDuration, seconds);
        }
        else
        {
           //  NetworkController.instance.LoadSceneWithBot();
           
            if (!isActive)
            {
                isActive = true;
                GameManager.Instance.isMultiplayer = false;
                 NetworkController.instance.CancelPlayer();
                GameManager.Instance.StartGame();
            }
        }
    }
    bool isActive = false;
    public void PlayerJoined(string nameofPlayer)
    {
        UIManager.Instance.loadingPopupPannel.SetActive(false);
        GameManager.Instance.PlayerCount++;
        Debug.Log("is count" + GameManager.Instance.PlayerCount);
        NetworkController.instance.opNmae = nameofPlayer;
        NetworkController.instance.opponentName.text = "Opponent  Name:" + nameofPlayer;
    }

    public void BotPlayerJoind(string nameOfPlayer)
    {
        Debug.Log("name of player" + nameOfPlayer);
        NetworkController.instance.opponentName.text = "Opponent  Name:" + nameOfPlayer; 
        UIManager.Instance.loadingPopupPannel.SetActive(false);
        NetworkController.instance.opNmae = nameOfPlayer;

        StartCoroutine(NetworkController.instance.LoadBot());
    }
}

