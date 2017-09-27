using UnityEngine;
using System.Collections;

public class ShowWhenConnected : MonoBehaviour {

	private Renderer rend;
	public bool whenConnected = true;

	// Use this for initialization
	void Start () {
		rend = gameObject.GetComponent<Renderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Network.isClient || Network.isServer)
			rend.enabled = whenConnected;
		else 
			rend.enabled = !whenConnected;
	}
}
