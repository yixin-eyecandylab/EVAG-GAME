using UnityEngine;
using System.Collections;

public class ShowApkVersion : MonoBehaviour {
	public TextMesh versionText;

	// Use this for initialization
	void Start () {
		Invoke ("ShowVersion", 0.5f);
	}

	public void ShowVersion() {
#if ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR)
		apkVersion apkv = GetComponent<apkVersion>();
		if(apkv == null) {
			Debug.LogError ("apkVersion component not found");
		} else {
			Debug.Log("APK Version: "+ apkv.versionName);
			versionText.text = "Version "+apkv.versionName;
		}
#endif
	}

}
