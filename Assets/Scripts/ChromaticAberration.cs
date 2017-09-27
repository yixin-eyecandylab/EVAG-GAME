using UnityEngine;
using System.Runtime.InteropServices;		// required for DllImport

public class ChromaticAberration : MonoBehaviour {
	
#if (UNITY_ANDROID && !UNITY_EDITOR)
	[DllImport("OculusPlugin")]
	private static extern void OVR_TW_EnableChromaticAberration( bool enable );
#endif

	void Start ()
	{
#if (UNITY_ANDROID && !UNITY_EDITOR)
		// Enable/Disable Chromatic Aberration Correction.
		// NOTE: Enabling Chromatic Aberration for mobile has a large performance cost.
		OVR_TW_EnableChromaticAberration(true);
#endif
	}

}
