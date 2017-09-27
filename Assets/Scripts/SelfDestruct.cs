using UnityEngine;
using System.Collections;

public class SelfDestruct : MonoBehaviour {
	public float lifeTime = 2.0f;
	public float networkTimeout = 1.0f;

	private float startTime;

	// Use this for initialization
	void Start () {
		startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		//if (Network.isServer && Time.time > startTime+lifeTime) {
		if (Time.time > startTime+lifeTime) {
			NetworkView nView = GetComponent<NetworkView>();
			Debug.Log ("Removing RPCs für NetworkView "+nView.viewID);
			Network.RemoveRPCsInGroup(777);
			if(nView.isMine) {
				Network.Destroy(gameObject);
			}
		}

		if (Time.time > startTime + lifeTime + networkTimeout) {
			Debug.Log ("TouchMarker still alive? Commiting suicide.");
			Destroy (gameObject);
		}

	}

}
