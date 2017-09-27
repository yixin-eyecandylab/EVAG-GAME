using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices; // required for DllImport

public class ViewpointPreloadSim : MonoBehaviour
{
	public GameObject frameKeeperPrefab;

	private FrameKeeper frameKeeper;
	public void Start ()
	{
		if (GameObject.Find ("FrameKeeper") == null) {
			Debug.Log ("ViewpointPreload: Instantiating FrameKeeper");
			GameObject fkObj = (GameObject) Instantiate (frameKeeperPrefab);
			fkObj.name = "FrameKeeper";
			frameKeeper = fkObj.GetComponent<FrameKeeper> ();
		} else {
			Debug.Log ("Existing FrameKeeper found");
			frameKeeper = GameObject.Find ("FrameKeeper").GetComponent<FrameKeeper> ();
		}
      //  frameKeeper.TextureForFile("Game_mode_still.jpg");
        //StartCoroutine(frameKeeper.TextureFromWWW ("Game_mode_still.jpg"));
    }


}