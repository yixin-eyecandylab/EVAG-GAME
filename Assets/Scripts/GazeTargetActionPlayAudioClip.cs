using UnityEngine;
using System.Collections;

public class GazeTargetActionPlayAudioClip : GazeTargetAction {

	public AudioClip audioClip;

	private AudioSource audioSource;
	private GazeTarget gt;

	public override void PerformAction() {
		Debug.Log("GazeTargetActionPlayAudioClip");
		GameObject ausObject = GameObject.FindGameObjectWithTag ("AudioSource"); // NarratorAudioSource?
		if (ausObject == null)
			Debug.LogError ("GazeTargetActionPlayAudioClip: Could not find AudioSource");
		audioSource = ausObject.GetComponent<AudioSource> ();
		audioSource.clip = audioClip;
		audioSource.Play ();
	}

	//TODO: Disable GazeTarget while audio is playing.

	public override bool isPerforming() {
		return audioSource != null && audioSource.clip == audioClip && audioSource.isPlaying;
	}

	// Load Audio Clip from streaming assets
	public void LoadAudioClip (string filename) {
		StartCoroutine(LoadAudioClipRoutine(filename));
	}

	public IEnumerator LoadAudioClipRoutine(string filename) {
		string filepath = Application.streamingAssetsPath+"/"+filename+".wav";
		Debug.Log ("Loading audio file from "+filepath);
		WWW request = new WWW (filepath);
		yield return request;
		audioClip = request.GetAudioClip (false, false);   
	}


}
