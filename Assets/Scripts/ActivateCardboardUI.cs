using UnityEngine;
using System.Collections;

public class ActivateCardboardUI : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Invoke ("ActivateUI", 2.0f);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ActivateUI() {
		Debug.Log ("Trying to activate Cardboard UI");
		Cardboard.SDK.EnableSettingsButton = true;
		Cardboard.SDK.EnableAlignmentMarker = true;
	}
}
