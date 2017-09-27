using UnityEngine;
using System.Collections;

public class AndroidIdleTimeout : MonoBehaviour {
	private float maxAcc=0.0f, maxRot=0.0f;
	private float lastCheck;
	public float interval = 1.0f;
	public float rotLimit, accLimit;

	// Use this for initialization
	void Start () {
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}
	
	// Update is called once per frame
	void Update () {
		float acc = Input.gyro.userAcceleration.sqrMagnitude;
		if (acc > maxAcc)
			maxAcc = acc;
		float rot = Input.gyro.rotationRate.sqrMagnitude;
		if (rot > maxRot)
			maxRot = rot;

		if (Time.time > lastCheck + interval) {
			lastCheck = Time.time;
			Debug.Log ("maxAcc: "+maxAcc+", maxRot: "+maxRot);
			maxAcc=maxRot=0.0f;
		}
	}
}
