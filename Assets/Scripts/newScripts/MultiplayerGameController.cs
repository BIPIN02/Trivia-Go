using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class MultiplayerGameController : Photon.PunBehaviour
{
    [SerializeField] GameObject options;
    [SerializeField] Text questionText;
    [SerializeField] Text playerScore;
    [SerializeField] Text opponentScore;
    [SerializeField] Text gemsText;
    [SerializeField] Text coinText;
    [SerializeField] Transform parent;
    [SerializeField] Text playerWinningScore;
    [SerializeField] Text playerWinninggems;
    [SerializeField] Text playerWinningcoins;
    [SerializeField] Text opponentWinningScore;
    [SerializeField] Text timerText;
    [SerializeField] Image timerBar;
    [SerializeField] Text playerName;
    [SerializeField] Text OpponentName;
    [SerializeField] Text WinnerplayerName;
    [SerializeField] Text WinnerOpponentName;
    private int currentQuestionIndex;
    private void OnEnable()
    {
        AudioManager.Instance.PlayMusic(1);
        int index = (int)GameManager.Instance.mode;
        playerName.text = UIManager.Instance.userDetail.userName;
        OpponentName.text = NetworkController.instance.opNmae;
        WinnerplayerName.text = UIManager.Instance.userDetail.userName;
        WinnerOpponentName.text = NetworkController.instance.opNmae;
        GameManager.Instance.SendDataToServer();
        if (NetworkController.instance.roomOwner)
        {
            if (GameManager.Instance.isMultiplayer)
            {
                StartCoroutine(GetQuestions((int)GameManager.Instance.mode, GameManager.Instance.GetTotalQuestion()));

            }
        }
    }
    float timer = 30;
    bool isTimer = false;
    int seconds;
    private void Update()
    {
        if (NetworkController.instance.roomOwner)
        {
            if (GameManager.Instance.isMultiplayer)
            {
                if (isTimer)
                {
                    timer -= Time.deltaTime;
                    if (timer <= 0)
                    {
                        isTimer = false;
                    }
                    else
                    {
                        int minutes = Mathf.FloorToInt(timer / 60f);
                         seconds = Mathf.RoundToInt(timer % 60f);

                        if (seconds == 60)
                        {
                            seconds = 0;
                            minutes += 1;
                        }
                        timerText.text = seconds.ToString("00");
                        float progressValue = timer / Mathf.Max(20, Mathf.Epsilon);
                        timerBar.fillAmount = progressValue;
                    }
                    photonView.RPC("ReceiveQuestionTimer", PhotonTargets.All, timerText.text, timerBar.fillAmount, timer, seconds);

                }
            }
        }
    }
    int previoustime = -1;
    [PunRPC]
    void ReceiveQuestionTimer(string timer, float fillamount, float duration,int seconds)
    {
        timerText.text = timer;
        timerBar.fillAmount = fillamount;
        if (seconds != previoustime)
        {
            previoustime = seconds;
            if (seconds > 10)
                AudioManager.Instance.PlayTimer(0);
            else 
                AudioManager.Instance.PlayTimer(1);
        }
        if (duration <= 0)
        {
            currentQuestionIndex++;
            if (currentQuestionIndex < GameManager.Instance.quizQuestions.Count)
            {
                DisplayQuestion();
            }
            else
            {
                StopCoroutine("AnswerBot");
                StopCoroutine("SetNewQuestion");
                playerWinningScore.text = "Score : " + UIManager.Instance.userDetail.coins;
                playerWinninggems.text = "" + UIManager.Instance.userDetail.gems;
                playerWinningcoins.text = "" + UIManager.Instance.userDetail.coins;
                opponentWinningScore.text = "Score : " + opScore;

                UIManager.Instance.SetAcitveObject(UIManager.Instance.WinningMultiplayerPopupPanel);
            }
        }
    }
    IEnumerator GetQuestions(int id,int questions)
    {
        // Debug.Log("getquestopm" + GetQuestion());
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
                        GameManager.Instance.quizQuestions.Add(quizQuestion);
                    }
                    break;
            }
            UIManager.Instance.loadingPopupPannel.SetActive(false);
            //UIManager.Instance.SetAcitveObject(UIManager.Instance.gamePlayScreenPanel);


            string serializedData = JsonConvert.SerializeObject(GameManager.Instance.quizQuestions);
            Debug.Log("serilie" + serializedData);
            // Send the serialized list to the opponent using PhotonNetwork
            photonView.RPC("ReceiveQuestionsFromOpponent", PhotonTargets.Others, serializedData);
            DisplayQuestion();

        }
    }
    [PunRPC]
    void ReceiveQuestionsFromOpponent(string serializedData)
    {
        GameManager.Instance.quizQuestions = JsonConvert.DeserializeObject<List<QuizQuestion>>(serializedData);
        DisplayQuestion();
    }
    void DisplayQuestion()
    {
        isTimer = true;
        timer = 20;
        isClickBot = false;
        playerScore.text = "Score : " + UIManager.Instance.userDetail.score;
        opponentScore.text = "Score :" + opScore;
        gemsText.text = "" + UIManager.Instance.userDetail.gems;
        coinText.text = "" + UIManager.Instance.userDetail.coins;
        questionText.text = GameManager.Instance.quizQuestions[currentQuestionIndex].question;
        string answer = GameManager.Instance.quizQuestions[currentQuestionIndex].answer;
        GameManager.Instance.wrongAnswerButton = new List<Button>();
        foreach (Transform tr in parent)
        {
            Destroy(tr.gameObject);
        }
        for (int i = 0; i < GameManager.Instance.quizQuestions[currentQuestionIndex].wrong_answers.Count; i++)
        {
            Button optionButton = Instantiate(options).GetComponent<Button>();
            optionButton.transform.SetParent(parent, false);
            if (GameManager.Instance.quizQuestions[currentQuestionIndex].wrong_answers[i] != answer)
                GameManager.Instance.wrongAnswerButton.Add(optionButton);
            else
                GameManager.Instance.checkAnswerButton = optionButton;
            optionButton.transform.GetChild(0).GetComponent<Text>().text = GameManager.Instance.quizQuestions[currentQuestionIndex].wrong_answers[i];
            optionButton.name = GameManager.Instance.quizQuestions[currentQuestionIndex].wrong_answers[i];
            optionButton.onClick.AddListener(() => CheckAnswer(optionButton.name, answer, optionButton));

        }
        int random = Random.Range(0, GameManager.Instance.quizQuestions[currentQuestionIndex].wrong_answers.Count);
        StartCoroutine(AnswerBot(GameManager.Instance.quizQuestions[currentQuestionIndex].wrong_answers[random], answer));

    }
    bool isClickBot = false;
    IEnumerator AnswerBot(string clickOption, string currectAnswer)
    {
        if (NetworkController.instance.isBot)
        {
            int random = Random.Range(6, 18);
            yield return new WaitForSeconds(random);
            Transform bt = parent.Find(clickOption);
            if (bt != null)
            {
                isClickBot = true;
                CheckAnswer(clickOption, currectAnswer, bt.GetComponent<Button>());
            }
        }
    }
    int opScore = 0;
    private void CheckAnswer(string selectedAnswer, string correctAnswer, Button bt)
    {
        StopCoroutine("AnswerBot");
        int index = 0;
        if (selectedAnswer == correctAnswer)
        {
            Debug.Log("Correct answer!");
            AudioManager.Instance.PlayAnswer(0);
            index = 0;
            // bt.image.color = color;
            if (!isClickBot)
            {
                UIManager.Instance.userDetail.score += Service.score;
                UIManager.Instance.userDetail.gems += Service.gems;
                UIManager.Instance.userDetail.coins += Service.coins;
                playerScore.text = "Score : " + UIManager.Instance.userDetail.score;
                gemsText.text = "" + UIManager.Instance.userDetail.gems;
                coinText.text = "" + UIManager.Instance.userDetail.coins;
                UIManager.Instance.userDetail.correctAnser += 1;
                if (UIManager.Instance.userDetail.correctAnser % 100 == 0)
                    UIManager.Instance.userDetail.level += 1;
                // Handle a correct answer, for example, increase the score.
                GameManager.Instance.SendDataToServer();
                photonView.RPC("SendScoreToOther", PhotonTargets.Others, UIManager.Instance.userDetail.score);
            }
            else
            {
                opScore += 1;
                opponentScore.text = "Score :-" + opScore;
            }
        }
        else
        {
            AudioManager.Instance.PlayAnswer(1);
            index = 1;
            // bt.image.color = color;
            Debug.Log("Wrong answer.");
            // Handle a wrong answer, for example, display a message to the player.
        }
        photonView.RPC("OpponentAnser", PhotonTargets.All, selectedAnswer, index);
    }
    [PunRPC]
    void SendScoreToOther(int score)
    {
        opponentScore.text = "Score :-" + score;

    }
    [PunRPC]
    void OpponentAnser(string nameofButton, int color)
    {
        Transform bt = parent.Find(nameofButton);
        if (bt != null)
        {
            Color ct = Color.white;
            if (color == 0)
            {
                ct = Color.green;
            }
            else
            {
                ct = Color.red;
            }
            Debug.Log("name " + nameofButton);
            bt.GetComponent<Button>().image.color = ct;
        }
        StopCoroutine("SetNewQuestion");
        StartCoroutine("SetNewQuestion");
    }
    IEnumerator SetNewQuestion()
    {
        yield return new WaitForSeconds(0.5f);
        currentQuestionIndex++;
        //bt.image.color = Color.white;
        // Move to the next question.
        if (currentQuestionIndex < GameManager.Instance.quizQuestions.Count)
        {
            DisplayQuestion();
        }
        else
        {
            StopCoroutine("AnswerBot");
            StopCoroutine("SetNewQuestion");
            currentQuestionIndex = 0;
            playerWinningScore.text = "Score : " + UIManager.Instance.userDetail.coins;
            playerWinninggems.text = "" + UIManager.Instance.userDetail.gems;
            playerWinningcoins.text = "" + UIManager.Instance.userDetail.coins;
            opponentWinningScore.text = "Score : " + opScore;
            UIManager.Instance.SetAcitveObject(UIManager.Instance.WinningMultiplayerPopupPanel);
        }
    }
    public void SubmitScore()
    {
        StopCoroutine("AnswerBot");
        StopCoroutine("SetNewQuestion");
        currentQuestionIndex = 0;
        photonView.RPC("OtherWinner", PhotonTargets.All);
    }
    [PunRPC]
    void OtherWinner()
    {
        playerWinningScore.text = "Score : " + UIManager.Instance.userDetail.score;
        playerWinninggems.text = "" + UIManager.Instance.userDetail.gems;
        playerWinningcoins.text = "" + UIManager.Instance.userDetail.coins;
        opponentWinningScore.text = "Score : " + opScore;
        if (!GameManager.Instance.isWinner)
        {
            GameManager.Instance.isWinner = true;
            if (UIManager.Instance.userDetail.score>opScore)
                 AudioManager.Instance.PlayWinning(0);
            else
                AudioManager.Instance.PlayWinning(1);
        }
        UIManager.Instance.SetAcitveObject(UIManager.Instance.WinningMultiplayerPopupPanel);
    }

    public void BackToSelection()
    {
        AudioManager.Instance.PlayMusic(0);
        currentQuestionIndex = 0;
        //UIManager.Instance.SetAcitveObject(UIManager.Instance.selectionPopupPanel);
        photonView.RPC("OtherWinner", PhotonTargets.Others);
        NetworkController.instance.CancelPlayer();

    }

}


