using UnityEngine;
using System.Collections;

public class ScrollListEntry : MonoBehaviour {
	public TMPro.TextMeshProUGUI numberLabel;
	public TMPro.TextMeshProUGUI playerLabel;
	public TMPro.TextMeshProUGUI scoreLabel;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetScore(string nr, string player, string score) {
        if (nr != numberLabel.text)
        {
            numberLabel.text = nr;
        }
        if (player != playerLabel.text)
        {
            playerLabel.text = player;
        }
        if (score != scoreLabel.text)
        {
            scoreLabel.text = score;
        }

	}
}
