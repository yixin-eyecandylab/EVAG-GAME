using UnityEngine;
using System.Collections;

public class LoadFrameKeeper : MonoBehaviour {
	public GameObject frameKeeperPrefab;
	public bool preload = false;

	// Use this for initialization
	void Awake () {
		if (GameObject.Find ("FrameKeeper") == null) {
			GameObject frameKeeper = (GameObject) Instantiate (frameKeeperPrefab);
			frameKeeper.name = "FrameKeeper";
			//if(preload)
			//	frameKeeper.GetComponent<FrameKeeper>().Preload();
		}

	}

}
