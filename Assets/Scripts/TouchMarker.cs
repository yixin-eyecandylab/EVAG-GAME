using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class TouchMarker : MonoBehaviour {
	public GameObject markerPrefab;
	public float depth=20.0f;
//	public float lifetime = 2.0f;

//	private float timeout = 0.0f;
	private GameObject marker;
	private FollowCameraRig fcr;

	void Start () {
	}
	
	void Update () {
		if (VraSettings.instance.isTablet) {
			if(Input.touchCount > 0 ) {
				bool freeze = false;
				for (int i = 0; i < Input.touchCount; ++i) {
					Touch touch = Input.GetTouch (i);
					if(! EventSystem.current.IsPointerOverGameObject(touch.fingerId) && touch.phase!=TouchPhase.Ended) {
						Debug.Log ("Touch!");
						freeze = true;
						Vector2 touchpos = touch.position;
						Debug.Log ("Position: " + touchpos.ToString ());
						Vector3 pos = Camera.main.ScreenToWorldPoint (new Vector3 (touchpos.x, touchpos.y, depth));
						if (marker == null) {
							// Create a new marker
							Debug.Log ("Instantiate Network marker at position: " + touchpos.ToString ());
							marker = (GameObject)Network.Instantiate (markerPrefab, pos, Quaternion.identity, 777);
						} else {
							// Move existing marker to draw line (trail)
							marker.transform.position = pos;
						}
					}
				}
				// Cam Freeze
				if(freeze) {
					if(fcr==null) {
						GameObject obj = GameObject.FindGameObjectWithTag("NetworkSync");
						if(obj == null) {
							Debug.LogError ("TouchMarker: NetworkSync object not found");
						} else {
							fcr = obj.GetComponent<FollowCameraRig> ();
							if(fcr == null) {
								Debug.LogError ("TouchMarker: FollowCameraRig component not found");
							}
						}
					}
					if (fcr != null) {
						fcr.CamFreeze();
					}
				}
			} else {
				marker = null;  // Will autodestruct after timeout
			}
		}
	}
}
