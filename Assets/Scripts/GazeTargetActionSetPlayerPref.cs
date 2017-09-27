using UnityEngine;
using System.Collections;

public class GazeTargetActionSetPlayerPref : GazeTargetAction {

	public string key;
	public string value;

	public override void PerformAction() {
		Debug.Log("GazeTargetActionSetPlayerPref key: "+key+", value: "+value);
		PlayerPrefs.SetString (key, value);
//		PlayerPrefs.Save ();	
		GameObject.Find ("ViewpointManager").GetComponent<ViewpointManager>().ReloadViewpoint();
	}


}
