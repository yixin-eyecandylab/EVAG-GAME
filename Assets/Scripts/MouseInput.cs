using UnityEngine;
using System.Collections;

public class MouseInput : MonoBehaviour {

	private Vector3 startPos;
	public LEDController ledController;
	public float scaleX;
	public bool gazeControll = true;

	private float lastMousePos;
	private int mouseMoved;
	private OVRCameraRig	cameraController = null;
	private Transform cameraTransform = null;
	private bool isServer;
	GameSync gameSync;
	private float targetPositionX;

	// Use this for initialization
	void Start () {
		Debug.LogError ("MouseInput: Start");
		startPos = transform.position;
		Debug.LogError ("MouseInput: Checkpoint 1");
		//		Cursor.visible = false;
//		Cursor.lockState = CursorLockMode.Locked;
		//isServer = Network.isServer;
		if(VraSettings.instance.isGearVR) {
			cameraController = GameObject.Find ("OVRCameraRig").GetComponent<OVRCameraRig> ();
			if (cameraController == null)
				Debug.LogError ("MouseInput: Could not find OVRCameraRig!");
			else
				cameraTransform = cameraController.centerEyeAnchor;
		} else if (VraSettings.instance.isCardboard) {
			GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
			if (camera == null)
				Debug.LogError ("MouseInput: Could not find MainCamera");
			else
				cameraTransform = camera.transform;
		}
		Debug.LogError ("MouseInput: Start complete");
	}
	
	// Update is called once per frame
	void Update () {
		if(VraSettings.instance.isHeadset) {
			float x = 0;
			if(gazeControll && Mathf.Abs (Input.mousePosition.x -lastMousePos) > 10.0f && !Input.GetMouseButton (0)) {
				lastMousePos = Input.mousePosition.x;
				mouseMoved++;
			}
			if(gazeControll && VraSettings.instance.isGearVR && mouseMoved > 10) {
				Debug.Log ("MouseInput: Mouse movement detected, deactivating GazeControl");
				gazeControll = false;
			}
			if(gazeControll && cameraTransform!=null) {
				Vector3 rot  = cameraTransform.rotation.eulerAngles;
				x = (Mathf.Sin (Mathf.Deg2Rad * rot.y) * 1.5f );
			} else {
				x = (Input.mousePosition.x/Screen.width) - 0.5f;
			}
			float orgX = x;
			x = Mathf.Clamp(x, -0.5f, 0.5f);
			transform.position = new Vector3(startPos.x + (x * scaleX), startPos.y, startPos.z);
			if (orgX > -0.6f && orgX < 0.6f && ((VraSettings.instance.isCardboard && Input.touchCount > 0) || Input.GetMouseButtonDown(0))) {
				ledController.SetDefective(transform.position.x);
			}
			if(Network.isServer) {
				if(gameSync==null) {
					GameObject obj = GameObject.Find("GameSync");
					if(obj!=null) {
						gameSync = obj.GetComponent<GameSync> ();
					}
				} else {
					gameSync.SetMousePosition(transform.position.x);
				}
			}
		} else {
			transform.position = new Vector3(Mathf.Lerp(transform.position.x, targetPositionX, Time.deltaTime*6.0f), startPos.y, startPos.z);
		}

	}
	
	public void SetMousePosition(float position) {
		targetPositionX = position;
	}


}
