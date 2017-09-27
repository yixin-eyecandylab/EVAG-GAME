using UnityEngine;
using System.Collections;

public class QuitOnPause : MonoBehaviour {
	public void OnApplicationPause () {
		Application.Quit();
	}
}
