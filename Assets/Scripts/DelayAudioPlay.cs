using UnityEngine;
using System.Collections;

public class DelayAudioPlay : MonoBehaviour {
	public float delay = 1.0f;

	// Use this for initialization
	void Start () {
		Invoke ("Play", delay);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void Play() 
	{
		gameObject.GetComponent<AudioSource> ().Play ();	
	}
}
