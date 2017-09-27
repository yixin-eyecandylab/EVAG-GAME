using UnityEngine;
using System.Collections;

public class Viewpoint : MonoBehaviour {

//	public int number;
	public bool isVideo;
	public string filename;
	public float frameRotation;
	public bool isStereoscopic;
	public bool isOverUnder;
	public AudioClip audioClip;
	public bool loopAudio = false;

	// audio file to play
	  // Starttime, stoptime

	// public bool autoJump
	// public float autoJumpTimeout

//	public int nextViewpoint;
	public Vector3 nextPosition;
	public Vector3 previousPosition;

//	public int[] nextViewpoints;
//	public Vector3[] nextPositions;

//	public void LoadViewpoint() {
//		ViewpointManager viewpointManager = GetComponent<ViewpointManager> ();
//		if (isVideo) {
//			viewpointManager.PlayVideo(filename, isStereoscopic);
//		} else {
//			viewpointManager.ShowPicture (filename, frameRotation, isStereoscopic, isOverUnder);
//		}
//		viewpointManager.SetCursorPosition (nextPosition);
//	}
//

}
