using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices; // required for DllImport

public class PhotoViewpoint : MonoBehaviour
{
	public GameObject leftSphere;  // the two spheres
	public GameObject rightSphere;  // the two spheres
	public string pictureFilename = "stereo.jpg";

	private Material leftMat, rightMat;


	#if UNITY_EDITOR
	private string fileDir = "sdCard/Comberry/Image_Viewer/";
	# else
	private string fileDir = "/storage/sdcard0/Comberry/Image_Viewer/";
	# endif


	void Start ()
	{
		Debug.Log ("VPM: Comberry Seidenader VRA starting, " + Application.productName + ", Version: " + Application.version);
		leftMat = leftSphere.GetComponent<Renderer> ().material;
		rightMat = rightSphere.GetComponent<Renderer> ().material;
		ShowPicture (pictureFilename);
		SetStereoOverUnder ();
	}


	public void ShowPicture(string filename) {
		string filePath = fileDir + filename;
		Debug.Log ("VPM showing picture " + filePath);
		setPerfHigh();
		Texture2D tex = new Texture2D (4, 4, TextureFormat.RGB24, false);
		tex.LoadImage (File.ReadAllBytes (filePath));
		leftMat.mainTexture = rightMat.mainTexture = tex;
		setPerfLow();
	}

	public void SetStereoOverUnder() {
		leftMat.mainTextureScale = new Vector2 (1.0f, 0.5f);
		leftMat.mainTextureOffset = new Vector2 (0.0f, 0.5f);
		rightMat.mainTextureScale = new Vector2 (1.0f, 0.5f);
		rightMat.mainTextureOffset = new Vector2 (0.0f, 0.0f);
	}


	public void Update() {

	}

	#if (UNITY_ANDROID && !UNITY_EDITOR)
	[DllImport("OculusPlugin")]
	// Set the fixed CPU clock level.
	private static extern void OVR_VrModeParms_SetCpuLevel(int cpuLevel);
	
	[DllImport("OculusPlugin")]
	// Set the fixed GPU clock level.
	private static extern void OVR_VrModeParms_SetGpuLevel(int gpuLevel);
	#endif
	
	private void setPerf(int cpuLevel, int gpuLevel) {
		Debug.Log ("VPM: Setting CPU/GPU Clock Level (" + cpuLevel + "," + gpuLevel + ")");
		#if (UNITY_ANDROID && !UNITY_EDITOR)
		OVR_VrModeParms_SetCpuLevel(cpuLevel);
		OVR_VrModeParms_SetGpuLevel(gpuLevel);
		OVRPluginEvent.Issue(RenderEventType.ResetVrModeParms);
		#endif
	}

	private void setPerfLow() {
		Debug.Log ("VPM: Clock setPerfLow");
		setPerf (1, 2);
	}

	private void setPerfHigh() {
		Debug.Log ("VPM: Clock setPerfHigh");
		setPerf (3, 3);
	}

	private void perfBoost() {
		Debug.Log ("VPM: Clock perfBoost");
		setPerfHigh();
		Invoke ("setPerfMid", 1.0f);
	}

}