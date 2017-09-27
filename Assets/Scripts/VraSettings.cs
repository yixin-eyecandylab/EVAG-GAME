using UnityEngine;
using System.Collections;

public class VraSettings : MonoBehaviour {

	/// Gets the singleton instance.
	/// </summary>
	public static VraSettings instance { get; private set; }

	public bool isHeadset;
	public bool isCardboard;
	public bool isGearVR;
	public bool isTablet;
	public bool isHighscoreServer;

	public void Awake() {
		// Only allow one instance at runtime.
		if (instance != null)
		{
			enabled = false;
			DestroyImmediate(this);
			return;
		}
		instance = this;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
