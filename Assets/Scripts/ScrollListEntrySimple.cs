using UnityEngine;
using System.Collections;

public class ScrollListEntrySimple : MonoBehaviour {
	public UnityEngine.UI.Text numberLabel;
	public UnityEngine.UI.Text playerLabel;
	public UnityEngine.UI.Text scoreLabel;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetScore(string nr, string player, string score) {
		numberLabel.text = nr;
		playerLabel.text = player;
		scoreLabel.text = score;
	}
}
