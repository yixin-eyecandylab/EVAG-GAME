using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;		// required for DllImport

public class TimeWarp30Hz : MonoBehaviour {

	#if (UNITY_ANDROID && !UNITY_EDITOR)
	[DllImport("OculusPlugin")]
	// Support to fix 60/30/20 FPS frame rate for consistency or power savings
	private static extern void OVR_TW_SetMinimumVsyncs( OVRTimeWarpUtils.VsyncMode mode );
	#endif

	// Best lollipop setting so far:
	// Time step 0.03333
	// Targetframerate 30
	// SetVSynvMode60
	// Bei VSyncMode30 zuppelt das rechte Bild hinterher.

	// Use this for initialization
	void Start () {
		// delay one frame because OVRCameraController initializes TimeWarp in Start()
//		Invoke("SetVSyncMode30", 0.01666f);
		Application.targetFrameRate = 30;
		Invoke("SetVSyncMode30", 0.1f);
		//		Application.targetFrameRate = 60;
	}
	
	void SetVSyncMode30()
	{
		#if (UNITY_ANDROID && !UNITY_EDITOR)
		OVR_TW_SetMinimumVsyncs(OVRTimeWarpUtils.VsyncMode.VSYNC_30FPS);
		#endif
		
	}

	void SetVSyncMode60()
	{
		#if (UNITY_ANDROID && !UNITY_EDITOR)
		OVR_TW_SetMinimumVsyncs(OVRTimeWarpUtils.VsyncMode.VSYNC_60FPS);
		#endif
		
	}
}

