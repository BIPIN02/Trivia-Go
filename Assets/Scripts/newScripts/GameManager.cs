using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System.Linq;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using Newtonsoft.Json;

public class GameManager : MySingleton<GameManager>
{
    [SerializeField]

    public enum GameMode
    {
        Simple = 0,
        Easy = 1,
        Intermidiate =2,
        Hard = 3,
        SuperHard = 4
    }
    public enum TotalQuestion
    {
        SuperFast = 0,
        Fast = 1,
        Medium = 2,
        Long = 3,
        SuperLong = 4
    }
    public GameMode mode = GameMode.Simple;
    public TotalQuestion totalQuestion = TotalQuestion.SuperFast;
    public List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
    [SerializeField] GameObject options;
    [SerializeField] Text questionText;
    [SerializeField] Text playerScore;
    [SerializeField] Text gemsText;
    [SerializeField] Text coinText;
    [SerializeField] Transform parent;
    [SerializeField] Text playerWinningScore;
    [SerializeField] Text playerWinninggems;
    [SerializeField] Text playerWinningcoins;
    public TableTimer table;
    [SerializeField] Text timerText;
    [SerializeField] Image timerBar;
    private int currentQuestionIndex;

    [HideInInspector]
    public int PlayerCount;
    [HideInInspector]
    public int requiredPlayers = 2;
    public bool isRoomCreated;
    public bool isMultiplayer = false;

    public DatabaseReference databaseReference;
    public List<Button> wrongAnswerButton=new List<Button>();
    public Button checkAnswerButton;
    public bool isWinner = false;
    // fore leaderboard
    [SerializeField] GameObject leaderboardItem;
    [SerializeField] Transform leaderboardIparent;
    public IAPController IAPControl;
    [SerializeField] Text[] bombText;
    [SerializeField] Text[] checkText;
    public void SaveData(string key, string data)
    {
        databaseReference.Child(key).Child(UIManager.Instance.userDetail.userID).SetRawJsonValueAsync(data);

    }

    public void GetData(string key, Action<DataSnapshot> callback)
    {
        databaseReference.Child(key).Child(UIManager.Instance.userDetail.userID).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                callback(snapshot);
            }
        });
    }
   
    public void SendDataToServer()
    {
        string jsonString = JsonConvert.SerializeObject(UIManager.Instance.userDetail);
        SaveData("UserData", jsonString);
    }
    public void GetDataFromServer()
    {
        // Retrieving data
        GetData("UserData", (snapshot) =>
        {
            if (snapshot.Exists)
            {
                string jsonData = snapshot.GetRawJsonValue();
                Debug.Log("string data" + jsonData);
                UIManager.Instance.userDetail = JsonConvert.DeserializeObject<UserDetail>(jsonData);
                Debug.Log("user" + UIManager.Instance.userDetail.coins);
            }
            else
            {
                UIManager.Instance.userDetail.level = 1;
                SendDataToServer();
                Debug.Log("Data does not exist.");
            }
        });
    
    }
    public Button[] gameMode;
    public Button[] questionType;
    public void SelectMode(int index)
    {
        for(int i=0;i<gameMode.Length;i++)
        {
            if (index == i)
                gameMode[index].image.color = Color.green;
            else
                gameMode[i].image.color = Color.white;
        }
        mode = (GameMode)index;
    }
    public void SeelctTotalQuestion(int index)
    {
        for (int i = 0; i < questionType.Length; i++)
        {
            if (index == i)
                questionType[index].image.color = Color.green;
            else
                questionType[i].image.color = Color.white;
        }
        totalQuestion = (TotalQuestion)index;
    }
    public int GetTotalQuestion()
    {
        int question = 5;
        switch(totalQuestion)
        {
            case TotalQuestion.SuperFast:
                question = 5;
                break;
            case TotalQuestion.Fast:
                question = 10;
                break;
            case TotalQuestion.Medium:
                question = 25;
                break;
            case TotalQuestion.Long:
                question = 50;
                break;
            case TotalQuestion.SuperLong:
                question = 100;
                break;
            
        }
        return question;
    }
    private void Start()
    {
        for (int i = 0; i < questionType.Length; i++)
        {
            if ((int)totalQuestion == i)
                questionType[(int)totalQuestion].image.color = Color.green;
            else
                questionType[i].image.color = Color.white;
        }
        for (int i = 0; i < gameMode.Length; i++)
        {
            if ((int)mode == i)
                gameMode[(int)mode].image.color = Color.green;
            else
                gameMode[i].image.color = Color.white;
        }
    }
    public void StartGame()
    {
        for (int i = 0; i < checkText.Length; i++)
        {
            checkText[i].text = "" + UIManager.Instance.userDetail.checkMark;
        }
        for (int i = 0; i < bombText.Length; i++)
        {
            bombText[i].text = "" + UIManager.Instance.userDetail.bomb;
        }
        isWinner = false;
        quizQuestions = new List<QuizQuestion>();
        if (!isMultiplayer)
        {
            StartCoroutine(GetQuestions((int)mode, GetTotalQuestion()));
        }
        else
        {
            //mode = (GameMode)index;
            NetworkController.instance.JoinRoomAndStartGame();
        }
    }
    public void onClickBomb()
    {
        if(UIManager.Instance.userDetail.bomb>0&&wrongAnswerButton.Count>1)
        {
            UIManager.Instance.userDetail.bomb--;
            int index = UnityEngine.Random.Range(0, wrongAnswerButton.Count);
            wrongAnswerButton[index].gameObject.SetActive(false);
            wrongAnswerButton.RemoveAt(index);
            for (int i = 0; i < bombText.Length; i++)
            {
                bombText[i].text = "" + UIManager.Instance.userDetail.bomb;
            }

        }
    }
    public void onClickCheckMark()
    {
        if (UIManager.Instance.userDetail.checkMark > 0 )
        {
            UIManager.Instance.userDetail.checkMark--;
            for (int i = 0; i < checkText.Length; i++)
            {
                checkText[i].text = "" + UIManager.Instance.userDetail.checkMark;
            }
            checkAnswerButton.onClick.Invoke();
        }
    }
    bool isLeaderboard = false;
    public void onClickLeaderBoard()
    {
        isLeaderboard = true;
        databaseReference.Child("UserData").OrderByChild("score").ValueChanged += HandleLeaderboardDataChanged;

    }
    private void HandleLeaderboardDataChanged(object sender, ValueChangedEventArgs args)
    {
        if(!isLeaderboard)
            return;
        if (args.DatabaseError != null)
        {
            Debug.LogError("DatabaseError: " + args.DatabaseError.Message);
            return;
        }
        // Leaderboard data has changed; update your UI or process the data.
        if (args.Snapshot != null && args.Snapshot.ChildrenCount > 0)
        {

            UIManager.Instance.SetAcitveObject(UIManager.Instance.leaderboardPopupPanel);
            foreach (Transform tr in leaderboardIparent)
            {
                Destroy(tr.gameObject);
            }
            // Iterate through the children (leaderboard entries) in the snapshot.
            UIManager.Instance.leaderBoard = new List<LeaderBoard>();;
            foreach (var child in args.Snapshot.Children)
            {
                var data = JsonConvert.DeserializeObject<LeaderBoard>(child.GetRawJsonValue());
                UIManager.Instance.leaderBoard.Add(data);
                Debug.Log("Score: " + data.score + ", Other Property: " + data.userName);
            }
            UIManager.Instance.leaderBoard = UIManager.Instance.leaderBoard.OrderByDescending(data => data.score).ToList();
            for(int i=0; i < UIManager.Instance.leaderBoard .Count;i++)
            {
                 LeaderBoardITem item = Instantiate(leaderboardItem).GetComponent<LeaderBoardITem>();
                item.transform.SetParent(leaderboardIparent, false);
                item.indexText.text = "" + (i+1);
                item.usernNameText.text = UIManager.Instance.leaderBoard[i].userName;
                item.scoreText.text = "" + UIManager.Instance.leaderBoard[i].score;
                // Use this data to update your UI or display the leaderboard.
                //Debug.Log(username + ": " + score + "user name" + username);
            }
        }
        isLeaderboard = false;
        UIManager.Instance.loadingPopupPannel.SetActive(false);
    }
    public void SelectGameType(bool isSelection)
    {
        isMultiplayer = isSelection;
        UIManager.Instance.SetAcitveObject(UIManager.Instance.levelScreenPanel);
    }
    IEnumerator GetQuestions(int id,int questions)
    {
        string url = Service.baseurl + "mode=" + id + "&count=" + questions + "&pw=h6A5";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            UIManager.Instance.loadingPopupPannel.SetActive(true);
            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();
            UIManager.Instance.loadingPopupPannel.SetActive(false);
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    AudioManager.Instance.PlayMusic(1);
                    string responseData = webRequest.downloadHandler.text;
                    Debug.Log("quest" + responseData);
                    var jsonode = JSON.Parse(responseData);
                    foreach (var kvp in jsonode)
                    {
                        JSONNode value = kvp.Value;
                        QuizQuestion quizQuestion = new QuizQuestion();
                        {
                            quizQuestion.cat = value["question"]["cat"];
                            quizQuestion.question = value["question"]["question"];
                            quizQuestion.answer = value["question"]["answer"];
                            quizQuestion.difficulty = value["question"]["difficulty"];
                            quizQuestion.code = value["question"]["code"];
                            quizQuestion.explan = value["question"]["explan"];
                            quizQuestion.link = value["question"]["link"];
                            quizQuestion.image = kvp.Value["question"]["image"];
                        };
                        quizQuestion.wrong_answers = new List<string>();
                        JSONNode wrongAnswersArray = value["wrong_answers"];
                        foreach (JSONNode wrongAnswerNode in wrongAnswersArray)
                        {
                            string st = wrongAnswerNode;
                            List<string> options = st.Split(',').Select(item => item.Trim()).ToList();
                            //  Debug.Log("options" + options.Count);
                            foreach (var s in options)
                                quizQuestion.wrong_answers.Add(s);
                        }
                        quizQuestion.wrong_answers.Add(quizQuestion.answer);
                        quizQuestion.wrong_answers = quizQuestion.wrong_answers.OrderBy(x => UnityEngine.Random.value).ToList();
                        quizQuestions.Add(quizQuestion);
                    }
                    break;
            }
            UIManager.Instance.loadingPopupPannel.SetActive(false);
            UIManager.Instance.SetAcitveObject(UIManager.Instance.gamePlayScreenPanel);
            GameManager.Instance.SendDataToServer();
            DisplayQuestion();
        }
    }
    private void Update()
    {
        if (!isMultiplayer)
        {
            if (isTimer)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    isTimer = false;
                    currentQuestionIndex++;
                    if (currentQuestionIndex < quizQuestions.Count)
                    {
                        DisplayQuestion();
                    }
                    else
                    {
                        playerWinningScore.text = "Score : " + UIManager.Instance.userDetail.coins;
                        playerWinninggems.text = ""+ UIManager.Instance.userDetail.gems;
                        playerWinningcoins.text = "" + UIManager.Instance.userDetail.coins;
                        if (!isWinner)
                        {
                            isWinner = true;
                            AudioManager.Instance.PlayWinning(0);
                        }
                        UIManager.Instance.SetAcitveObject(UIManager.Instance.WinningPopupPanel);

                    }
                }
                else
                {
                    int minutes = Mathf.FloorToInt(timer / 60f);
                    int seconds = Mathf.RoundToInt(timer % 60f);

                    if (seconds == 60)
                    {
                        seconds = 0;
                        minutes += 1;
                    }
                    timerText.text = seconds.ToString("00");
                    float progressValue = timer / Mathf.Max(20, Mathf.Epsilon);
                    timerBar.fillAmount = progressValue;
                    if (seconds != previoustime)
                    {
                        previoustime = seconds;
                        if (seconds > 10)
                            AudioManager.Instance.PlayTimer(0);
                        else 
                            AudioManager.Instance.PlayTimer(1);
                    }
                }
            }
        }
    }
    int previoustime = -1;
    bool isTimer = false;
    float timer = 20;
    void DisplayQuestion()
    {
        isTimer = true;
        timer = 20;
        playerScore.text = "Score : " + UIManager.Instance.userDetail.score;
        gemsText.text = "" + UIManager.Instance.userDetail.gems;
        coinText.text = "" + UIManager.Instance.userDetail.coins;
        questionText.text = quizQuestions[currentQuestionIndex].question;
        string answer = quizQuestions[currentQuestionIndex].answer;
        wrongAnswerButton = new List<Button>();
        foreach (Transform tr in parent)
        {
            Destroy(tr.gameObject);
        }
        for (int i = 0; i < quizQuestions[currentQuestionIndex].wrong_answers.Count; i++)
        {
            Button optionButton = Instantiate(options).GetComponent<Button>();
            optionButton.transform.SetParent(parent, false);
            optionButton.transform.GetChild(0).GetComponent<Text>().text = quizQuestions[currentQuestionIndex].wrong_answers[i];
            optionButton.name = quizQuestions[currentQuestionIndex].wrong_answers[i];
            if (quizQuestions[currentQuestionIndex].wrong_answers[i] != answer)
                wrongAnswerButton.Add(optionButton);
            else
               checkAnswerButton = optionButton;
            optionButton.onClick.AddListener(() => CheckAnswer(optionButton.name, answer, optionButton));

        }
    }

    private void CheckAnswer(string selectedAnswer, string correctAnswer, Button bt)
    {
        if (!isMultiplayer)
        {
            if (selectedAnswer == correctAnswer)
            {
                UIManager.Instance.userDetail.correctAnser += 1;
                AudioManager.Instance.PlayAnswer(0);
                Debug.Log("Correct answer!");
                bt.image.color = Color.green;
                UIManager.Instance.userDetail.score += Service.score;
                UIManager.Instance.userDetail.gems += Service.gems;
                UIManager.Instance.userDetail.coins += Service.coins;
                playerScore.text = "Score : " + UIManager.Instance.userDetail.score;
                gemsText.text = "" + UIManager.Instance.userDetail.gems;
                coinText.text = "" + UIManager.Instance.userDetail.coins;
                if(UIManager.Instance.userDetail.correctAnser%100==0)
                    UIManager.Instance.userDetail.level += 1;
                // Handle a correct answer, for example, increase the score.
                SendDataToServer();
            }
            else
            {
                AudioManager.Instance.PlayAnswer(1);
                bt.image.color = Color.red;
                Debug.Log("Wrong answer.");
                // Handle a wrong answer, for example, display a message to the player.
            }
            StopCoroutine("SetNewQuestion");
            StartCoroutine("SetNewQuestion");
        }

    }

    IEnumerator SetNewQuestion()
    {
        yield return new WaitForSeconds(0.5f);
        currentQuestionIndex++;
        //bt.image.color = Color.white;
        // Move to the next question.
        if (currentQuestionIndex < quizQuestions.Count)
        {
            DisplayQuestion();
        }
        else
        {
            currentQuestionIndex = 0;
            timer = 20;
            isTimer = false;
            playerWinningScore.text = "Score : " + UIManager.Instance.userDetail.score;
            playerWinninggems.text = "" + UIManager.Instance.userDetail.gems;
            playerWinningcoins.text = "" + UIManager.Instance.userDetail.coins;
            if (!isWinner)
            {
                isWinner = true;
                AudioManager.Instance.PlayWinning(0);
            }
            UIManager.Instance.SetAcitveObject(UIManager.Instance.WinningPopupPanel);
        }
    }
    public void SubmitScore()
    {
        currentQuestionIndex = 0;

        playerWinningScore.text = "Score : " + UIManager.Instance.userDetail.score;
        playerWinninggems.text = "" + UIManager.Instance.userDetail.gems;
        playerWinningcoins.text = "" + UIManager.Instance.userDetail.coins;
        if (!isWinner)
        {
            isWinner = true;
            AudioManager.Instance.PlayWinning(0);
        }
        UIManager.Instance.SetAcitveObject(UIManager.Instance.WinningPopupPanel);
    }
    public void BackToSelection()
    {
        AudioManager.Instance.PlayMusic(0);
        currentQuestionIndex = 0;
        UIManager.Instance.SetAcitveObject(UIManager.Instance.selectionPopupPanel);
    }
}

public class IAPControl
{
}