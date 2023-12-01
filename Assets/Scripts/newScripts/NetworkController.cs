using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;
using SimpleJSON;

public class NetworkController : Photon.PunBehaviour
{
    public string gameVersion= "1.105";
    public bool isInLobby = false;
    public bool roomOwner;
    public static NetworkController instance;
    public bool isConnected = false;
    public float timeDuration = 5;

    void Awake()
    {
        instance = this;
        PhotonNetwork.PhotonServerSettings.HostType = ServerSettings.HostingOption.PhotonCloud;
        PhotonNetwork.PhotonServerSettings.PreferredRegion = CloudRegionCode.eu;
        Debug.Log("resion name" + PhotonNetwork.PhotonServerSettings.PreferredRegion);
        PhotonNetwork.autoJoinLobby = false;

        PhotonNetwork.automaticallySyncScene = true;
        Application.targetFrameRate = 60;
        Debug.Log("frame rate" + Application.targetFrameRate);
        ConnectPhoton();
    }
    
    void ConnectPhoton()
    {
        //Profile.ProfileData.nameofLEftPlayer = new List<string>();
        isLoadScene = false;
        PhotonNetwork.PhotonServerSettings.RunInBackground = true;
        Debug.Log("game version" + gameVersion);
        PhotonNetwork.ConnectUsingSettings(gameVersion);
        PhotonNetwork.BackgroundTimeout = 300;

    }
    private void Update()
    {
        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            if (!PhotonNetwork.connected)
            {
                PhotonNetwork.ConnectUsingSettings(gameVersion);
            }
            if (PhotonNetwork.connectionStateDetailed == ClientState.ConnectedToNameServer)
            {
                OnConnectedToMaster();
            }
        }
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        isLoadScene = false;
        isInLobby = true;
    }

    public void JoinRoomAndStartGame()
    {
        UIManager.Instance.loadingPopupPannel.SetActive(true);
        //  roomName = string.Empty;
        isInLobby = true;
        PhotonNetwork.playerName = UIManager.Instance.userDetail.userName;
        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() {
         {"m", GameManager.Instance.mode.ToString()+UIManager.Instance.userDetail.level.ToString()}
         };

        //// PhotonNetwork.CurrentRoom.SetCustomProperties(expectedCustomRoomProperties);
        StartCoroutine(TryToJoinRandomRoom(expectedCustomRoomProperties));

    }
    public IEnumerator TryToJoinRandomRoom(ExitGames.Client.Photon.Hashtable roomOptions)
    {
        while (true)
        {
            if (isInLobby)
            {
                PhotonNetwork.JoinRandomRoom(roomOptions, 0);
                break;
            }
            else
            {
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
    public void OnPhotonRandomJoinFailed()
    {
        Debug.Log("faild room");
      //  UIManager.Instance.loadingPopupPannel.SetActive(false);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = new String[] { "m", "v" };

        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() {
            { "m",  GameManager.Instance.mode.ToString()+UIManager.Instance.userDetail.level.ToString()},
            {"fp", UnityEngine.Random.Range(0, GameManager.Instance.requiredPlayers)}
         };
        roomOptions.MaxPlayers = (byte)GameManager.Instance.requiredPlayers;
        //roomOptions.IsVisible = true;
        StartCoroutine(TryToCreateGameAfterFailedToJoinRandom(roomOptions));
    }
    public IEnumerator TryToCreateGameAfterFailedToJoinRandom(RoomOptions roomOptions)
    {
        while (true)
        {
            if (isInLobby)// && isInMaster)
            {
                PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);

                break;
            }
            else
            {
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    public override void OnJoinedRoom()
    {
        isInLobby = true;
        UIManager.Instance.loadingPopupPannel.SetActive(false);
        Debug.Log("wait for another player" + PhotonNetwork.room.PlayerCount);
        if (PhotonNetwork.playerList.Length == 1)
        {
            GameManager.Instance.PlayerCount++;
            NetworkController.instance.playerName.text = "Player  Name:" + UIManager.Instance.userDetail.userName;
            UIManager.Instance.waitingScreen.SetActive(true);
            isConnected = true;
        }
        else if (PhotonNetwork.playerList.Length >= GameManager.Instance.requiredPlayers)
        {
            Debug.Log("is found player");
            //  isLoadScene = false;
            PhotonNetwork.room.IsOpen = false;
            PhotonNetwork.room.IsVisible = false;
        }

        if (!roomOwner)
        {
            GameManager.Instance.PlayerCount++;
            UIManager.Instance.waitingScreen.SetActive(true);
            for (int i = 0; i < PhotonNetwork.otherPlayers.Length; i++)
            {
                Debug.Log("Players in room " + PhotonNetwork.otherPlayers[i].NickName);

                GameManager.Instance.table.PlayerJoined(PhotonNetwork.otherPlayers[i].NickName);
            }
            isConnected = true;
            // player jwaiting screen
            //set another
        }
    }

    public void CancelPlayer()
    {
        Debug.Log("cancel player");
       
        PhotonNetwork.LeaveRoom();
        isLoadBoat = false;
        isConnected = false;
        isBot = false;
        GameManager.Instance.PlayerCount--;
        GameManager.Instance.PlayerCount = 0;
    }
    bool isLoadBoat = false;
    public void LoadSceneWithBot()
    {
        if (!isLoadBoat)
        {
            isLoadBoat = true;
            if (PhotonNetwork.isMasterClient)
            {
                if (GameManager.Instance.PlayerCount < GameManager.Instance.requiredPlayers)
                {
                    Debug.Log("Master Client");
                    AddBots();
                }

            }
            else
            {
                Debug.Log("Not Master client");
            }
        }
    }

    public void AddBots()
    {
        // Add Bots here

        if (PhotonNetwork.room.PlayerCount <GameManager.Instance.requiredPlayers)
        {

            if (PhotonNetwork.isMasterClient)
            {
                //PhotonNetwork.RaiseEvent((int)EnumPhoton.StartWithBots, null, true, null);
                // start with bot
            }
           // Debug.Log("need player " + (Profile.ProfileData.requiredPlayers - PhotonNetwork.room.PlayerCount));
            for (int i = 0; i < GameManager.Instance.requiredPlayers - PhotonNetwork.room.PlayerCount; i++)
            {

                if (GameManager.Instance.PlayerCount < GameManager.Instance.requiredPlayers)
                {
                    StartCoroutine(AddBot(i));
                }
            }
        }

    }
    public Text playerName;
    public Text opponentName;
    public string opNmae;
      public TextAsset botPlayerName;
    public IEnumerator AddBot(int i)
    {
        yield return new WaitForSeconds(i + UnityEngine.Random.Range(0.0f, 0.9f));
        string[] wordList = botPlayerName.text.Split('\n');
        string NameofPlayer = wordList[UnityEngine.Random.Range(0, wordList.Length - 1)];// "User"  +  UnityEngine.Random.Range(100000, 999999) + ",bot";
        opNmae = NameofPlayer;
      //  Debug.Log("bot wait" + Profile.ProfileData.boatPlayer);
        photonView.RPC("JoinPlayer", PhotonTargets.All, NameofPlayer );

    }
    [PunRPC]
    void JoinPlayer(string playerName)
    {
        //Profile.ProfileData.boatPlayer.Add(playerName);
        //Profile.ProfileData.boatimage.Add(image);
        GameManager.Instance.PlayerCount++;
        GameManager.Instance.table.BotPlayerJoind(playerName);
        // bot joined screen
    }

    [PunRPC]
    private void SendLevelData()
    {
        Debug.Log("receive");
        // game play screen
        if (PhotonNetwork.playerList.Length >= GameManager.Instance.requiredPlayers)
        {
            StartCoroutine(LoadScene());
        }
        else if (GameManager.Instance.PlayerCount >= GameManager.Instance.requiredPlayers)
        {
            StartCoroutine(LoadScene());
        }
    }
    public bool isBot = false;
    public IEnumerator LoadBot()
    {
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("Add Bots with delay" + GameManager.Instance.PlayerCount + "required player" + GameManager.Instance.requiredPlayers);
            if (GameManager.Instance.PlayerCount >= GameManager.Instance.requiredPlayers)
            {
                yield return new WaitForSeconds(1);
                isBot = true;
                photonView.RPC("SendLevelData", PhotonTargets.All);
            }
        }
    }
    public bool isLoadScene = false;
    IEnumerator LoadScene()
    {
        if (isLoadScene == false)
        {
            PhotonNetwork.room.IsOpen = false;
            PhotonNetwork.room.IsVisible = false;
            Debug.Log("loading scene");
            isLoadScene = true;
            isGameStart = true;
           // UIController.Instance.loadingScreen.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            //PhotonNetwork.isMessageQueueRunning = false;
          //  Profile.ProfileData.type = UIController.Instance.type;
            //PhotonNetwork.BackgroundTimeout = 60;
            UIManager.Instance.SetAcitveObject(UIManager.Instance.multiplayerGameScene);
        }
    }
   

    public override void OnCreatedRoom()
    {
        Debug.Log("is join private table");
        //UIController.Instance.roomID = roomName;
        roomOwner = true;
    }
    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom called");
        GameManager.Instance.isRoomCreated = false;

        UIManager.Instance.loadingPopupPannel.SetActive(false);
        if (GameManager.Instance.isMultiplayer)
             UIManager.Instance.SetAcitveObject(UIManager.Instance.selectionPopupPanel);
        roomOwner = false;
    }
    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
       // Profile.ProfileData.PlayerCount--;
    }


    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        //UIController.Instance.loadingScreen.SetActive(false);
        //UIController.Instance.mainScreen.SetActive(true);
        Debug.Log("Failed to join room");
    }
    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log("Players Count: " + PhotonNetwork.playerList.Length);
        GameManager.Instance.table.PlayerJoined(newPlayer.NickName);
        LoadDatas();
    }
    void LoadDatas()
    {
        if (PhotonNetwork.isMasterClient && PhotonNetwork.playerList.Length >= GameManager.Instance.requiredPlayers)
        {
            PhotonNetwork.room.IsOpen = false;
            PhotonNetwork.room.IsVisible = false;
            isBot = false;
            StartCoroutine("SendData");
        }
    }
    IEnumerator SendData()
    {
        yield return new WaitForSeconds(1f);
        if (roomOwner)
        {
            Debug.Log("is update player" + PhotonNetwork.playerName);
            //Profile.ProfileData.Credit -= UIController.Instance.totalBat;
            //Profile.ProfileData.SaveProfile();
            PhotonNetwork.room.IsOpen = false;
            PhotonNetwork.room.IsVisible = false;
            photonView.RPC("SendLevelData", PhotonTargets.All);
          
        }

    }
    private void OnEvent(byte eventcode, object content, int senderid)
    {
        Debug.Log("Received event: " + (int)eventcode + " Sender ID: " + senderid);
    }

    bool isGameStart = false;
   
    public override void OnDisconnectedFromPhoton()
    {
            CancelPlayer();
        Debug.Log("Disconnected from photon");
    }
    public void DisconnecteFromPhoton()
    {
       // UIController.Instance.loadingScreen.SetActive(false);
        isLoadScene = false;
        Debug.Log("disconnecting");
    }
}

