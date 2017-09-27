using UnityEngine;
using System.Collections;

public class WiggleObject: MonoBehaviour {
	public float angle;
	public float duration;
	public float currentAngle;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		currentAngle = Mathf.Sin ((Time.time/duration)*2*Mathf.PI) * angle;
//		currentAngle = Time.time;
		transform.localRotation = Quaternion.Euler(0.0f,currentAngle,0.0f);
	}

}
