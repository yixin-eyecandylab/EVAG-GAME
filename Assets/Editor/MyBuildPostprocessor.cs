using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class MyBuildPostprocessor {
	[PostProcessBuildAttribute(1)]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
		Debug.Log("My Build Post Process: "+pathToBuiltProject );
		PlayerSettings.Android.bundleVersionCode++;
		PlayerSettings.bundleVersion = "0." + PlayerSettings.Android.bundleVersionCode.ToString ();
		Debug.Log("bundleVersionCode: "+PlayerSettings.Android.bundleVersionCode);
		Debug.Log("bundleVersion: "+PlayerSettings.bundleVersion);
	}
}