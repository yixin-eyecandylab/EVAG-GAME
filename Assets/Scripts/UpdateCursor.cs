using UnityEngine;
using System.Collections;

public class UpdateCursor : MonoBehaviour {

	public float rotateTime = 3.0f;
	public GameObject instantiatedCursorTimer;

	private Material cursorTimerMaterial = null;

	// Use this for initialization
	void Awake () {
		cursorTimerMaterial = instantiatedCursorTimer.GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update () {
		UpdateCursorTimer ((Time.time % rotateTime));
	}

	void UpdateCursorTimer(float timerRotateRatio)
	{
		if (instantiatedCursorTimer != null)
		{
			instantiatedCursorTimer.GetComponent<Renderer>().enabled = true;
			
			// Clamp the rotation ratio to avoid rendering artifacts
			float alphaAmount = Mathf.Clamp(1.0f - timerRotateRatio, 0.0f, 1.0f);
			cursorTimerMaterial.SetFloat ( "_Cutoff", alphaAmount );
			
		}
	}

}
