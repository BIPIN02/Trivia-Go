using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuizQuestion
{
    public string cat;
    public string question;
    public string answer;
    public string difficulty;
    public string code;
    public string explan;
    public string link;
    public string image;
    public List<string> wrong_answers;
}

