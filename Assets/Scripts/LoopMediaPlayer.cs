using UnityEngine;
using System.Collections;

public class LoopMediaPlayer : MonoBehaviour {
	public int seekPos = 5000; // ms
	public float length = 6.0f; // Seconds

	private float startTime;
	MediaPlayerCtrl playerctrl;

	// Use this for initialization
	void Start () {
		playerctrl = gameObject.GetComponent<MediaPlayerCtrl> ();
		startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if(Time.time > startTime + length) {
			Debug.Log("Restarting MediaPlayer Loop");
			// Restart
			playerctrl.SeekTo(seekPos);
			startTime = Time.time;
		}

	}
}
