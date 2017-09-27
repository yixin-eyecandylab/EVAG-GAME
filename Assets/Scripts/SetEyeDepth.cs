using UnityEngine;
using System.Collections;

public class SetEyeDepth : MonoBehaviour {

	// Use this for initialization
	void Start () {
//		OVRManager.instance.profile.eyeDepth = 0.0f;
		Debug.LogError("OVR IPD: "+OVRManager.profile.ipd);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
