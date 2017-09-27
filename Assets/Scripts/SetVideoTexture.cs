using UnityEngine;
using System.Collections;

public class SetVideoTexture : MonoBehaviour {
	public GameObject playerObject;
	Material mat;
	MediaPlayerCtrl player;

	// Use this for initialization
	void Start () {
		player = playerObject.GetComponent<MediaPlayerCtrl> ();
		mat = GetComponent<Renderer> ().material;
		mat.mainTexture = player.GetVideoTexture ();
	}


	void Update () {
		if (mat.mainTexture == null) {
			mat.mainTexture = player.GetVideoTexture ();
		}
	}


}
