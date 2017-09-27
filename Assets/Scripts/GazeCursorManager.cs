using UnityEngine;
using System.Collections;

public class GazeCursorManager : MonoBehaviour {

	private float hitTime=0;
	private float hitDelay = 0.1f;
	public Transform cameraTransform = null;
	public float raiseAngle = 0.0f;

	// Update is called once per frame
	void Update () {
		Vector3 cameraPosition = cameraTransform.position;
		Vector3 cameraForward = Vector3.RotateTowards(cameraTransform.forward, cameraTransform.up, raiseAngle*Mathf.Deg2Rad, 0.0f);

		Ray ray = new Ray (cameraPosition, cameraForward);
		RaycastHit hit;
//		if (Time.time > (hitTime + hitDelay)) {
			if (Physics.Raycast (ray, out hit, 100.0F, 1)) { // Layermask = 1, only Default layer, i.e. ignore GazeHelpers
//				transform.position = ray.GetPoint (100.0F);
//			Debug.Log ("Hit something! "+hit.collider.gameObject.name);
				GameObject other = hit.collider.gameObject;
            if (other.tag == "GazeTarget")
            {
                //					Debug.Log ("Hit a GazeTarget");
                other.GetComponent<GazeTarget>().Hit();
            }
            else
                if (other.tag == "Pulse")
                other.GetComponent<GazeTarget>().Hit();// Action();
            hitTime = Time.time;
			}
		}
//	}
}
