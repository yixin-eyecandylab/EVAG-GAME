using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HighScoreServerEntry : MonoBehaviour {

    public Text Name;
    public Text Score;


    public void SetScore(string name, string score)
    {
        Name.text = name;
        Score.text = score;
    }
}
