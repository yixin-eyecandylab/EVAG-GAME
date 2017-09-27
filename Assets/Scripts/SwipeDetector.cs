using UnityEngine;
using System.Collections;

public class SwipeDetector : MonoBehaviour {

	private Vector2 direction;
	private bool buttonDown = false;

	void Update() {
		if (Input.GetMouseButton (0)) {
			if(buttonDown) {
				float h = Input.GetAxis("Mouse X");
				float v = Input.GetAxis("Mouse Y");
				direction += new Vector2(h*Time.deltaTime, v*Time.deltaTime);
			} else {
				buttonDown = true;
				direction = new Vector2(0.0f,0.0f);
			}
		} else if(buttonDown) { // So, button just lifted
			float length = Mathf.Sqrt(Mathf.Pow(direction.x, 2) + Mathf.Pow (direction.y, 2));
			//direction.Normalize();
			Debug.Log ("Swipe direction:" + direction.x + " " + direction.y + " length: "+length);
			buttonDown = false;
			if(length > 0.5f) {
				// Swipe long enough
				if(direction.y > -0.2f && direction.y < 0.2f) {
					// Horizontal swipe
					if (direction.x < -0.5f ) { 
						Debug.Log ("Swipe detected: BACK");
					} else if(direction.x > 0.5f ) { 
						Debug.Log ("Swipe detected: FORTH");
					}
				}
			}
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
}
