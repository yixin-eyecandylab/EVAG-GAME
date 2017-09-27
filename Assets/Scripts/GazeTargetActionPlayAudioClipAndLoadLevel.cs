using UnityEngine;
using System.Collections;

public class GazeTargetActionPlayAudioClipAndLoadLevel : GazeTargetAction {

	public string levelName;
	public float delay;

	public override void PerformAction() {
		Debug.Log("GazeTargetActionPlayAudioClip");
		gameObject.GetComponent<AudioSource> ().Play ();
		Invoke ("NextLevel", delay);
	}

	public void NextLevel() {
		Application.LoadLevelAsync(levelName);
	}
}
