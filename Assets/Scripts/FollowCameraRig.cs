using UnityEngine;
using System.Collections;
[RequireComponent( typeof( NetworkView ) )]

public class FollowCameraRig : MonoBehaviour {

//	public Vector3 cameraPosition;
//	public Vector3 cameraForward;
	public OVRCameraRig					cameraController = null;
//	public GameObject slaveCamera;
	public float raiseAngle = 5.0f;

	private bool isMaster;  // or slave
	private float smoothing = 6.0f;
	private Quaternion raiseQuat;
	private float idleSince;
	private NetworkView nview;
	private Quaternion lastRotation;
	private float idleTimeout = 5.0f;
	private bool isIdle=false;
	private bool camFrozen = false;
	private Transform cameraTransform = null;


	void Start () {
		nview = GetComponent<NetworkView> ();
		isMaster = nview.isMine;
		raiseQuat = Quaternion.Euler(-raiseAngle, 0.0f, 0.0f);
		FindCamera();
	}

	public void FindCamera() {
		if (VraSettings.instance.isGearVR) {
			cameraController = GameObject.Find ("OVRCameraRig").GetComponent<OVRCameraRig> ();
			if (cameraController == null)
				Debug.LogError ("FollowCameraRig: Could not find OVRCameraRig!");
			else
				cameraTransform = cameraController.centerEyeAnchor;
		} else if(VraSettings.instance.isCardboard) {
			GameObject cam  = GameObject.Find ("Main Camera");
			if (cam == null)
				Debug.LogError ("FollowCameraRig: Could not find Cardboard Camera!");
			else 
				cameraTransform = cam.transform;
		} else {
			GameObject cam  = GameObject.Find ("Camera");
			if (cam == null)
				Debug.LogError ("FollowCameraRig: Could not find Slave (Tablet) Camera!");
			else 
				cameraTransform = cam.transform;
		}
	}

	public void CamUnFreeze() {
		camFrozen = false;
	}

	public void CamFreeze() {
		camFrozen = true;
		CancelInvoke ("CamUnFreeze");
		Invoke ("CamUnFreeze", 0.3f);
	}

	void Update () {
		if(cameraTransform==null)
			FindCamera();
		if (VraSettings.instance.isHeadset && cameraTransform!=null) {
			if (VraSettings.instance.isCardboard || (VraSettings.instance.isGearVR && OVRManager.display.isPresent)) {
				transform.position = cameraTransform.position;
				transform.rotation = cameraTransform.rotation * raiseQuat;
			}
		} else if(VraSettings.instance.isTablet) {
			if(camFrozen)
				return;
			cameraTransform.position = transform.position;
			cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, transform.rotation, smoothing * Time.deltaTime);
			// Idle detection
			if(transform.rotation != lastRotation) {
				lastRotation = transform.rotation;
				idleSince = Time.time;
				if(isIdle) {
					Debug.Log ("FollowCameraRig: Cam stopped being idle, disabling SleepTimeout");
					Screen.sleepTimeout = SleepTimeout.NeverSleep;
					isIdle=false;
				}
			}
			if(!isIdle && Time.time > idleSince + idleTimeout) {
				Debug.Log ("FollowCameraRig: Cam seems to be idle, enabling SleepTimeout");
				Screen.sleepTimeout = SleepTimeout.SystemSetting;
				isIdle=true;
			}
		}
		if (!isMaster && !Network.isClient) {
			Destroy (gameObject);
		}
	}

	public void SyncNow() {
		if(cameraTransform == null)
			FindCamera();
		if (VraSettings.instance.isHeadset && cameraTransform != null) {
			if (VraSettings.instance.isCardboard || (VraSettings.instance.isGearVR && OVRManager.display.isPresent)) {
				transform.position = cameraTransform.position;
				transform.rotation = cameraTransform.rotation * raiseQuat;
			}
			if(nview!=null)
				nview.RPC ("SyncNowRemote", RPCMode.Others, transform.position, transform.rotation);
		}
	}

	[RPC] public void SyncNowRemote(Vector3 position, Quaternion rotation) {
		cameraTransform.position = transform.position = position;
		cameraTransform.rotation = transform.rotation = rotation;
	}

	public void OnDestroy() {
		if (!isMaster && VraSettings.instance.isTablet) {
			cameraTransform.position = Vector3.zero;
			cameraTransform.rotation = Quaternion.Euler (0.0f, 0.0f, 0.0f);
		}
	}


}
