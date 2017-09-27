using UnityEngine;
using System.Collections;
using System.IO;

public class FrameManager : MonoBehaviour
{
	public GameObject leftSphere, rightSphere;  // the two spheres
	public GameObject frameCursor;
	public Vector3[] cursorPositions;
	public int[] frameRotations;

	private int currentFrame = 0;
	private FrameKeeper frameKeeper;

	void Start ()
	{
		frameKeeper = GameObject.Find ("FrameKeeper").GetComponent<FrameKeeper> ();
		if (frameKeeper == null)
			Debug.LogError ("FrameManger: Could not find FrameKeeper!");
		else
			LoadFrame (currentFrame);
	}
	
	public void NextFrame () 	{
		currentFrame += 1;
	//	if (currentFrame >= frameKeeper.frames.Length)
		//	currentFrame = 0;
		LoadFrame (currentFrame);
	}

	public void LoadFrame(int number) {
		Debug.Log ("Loading frame " + number);
	//	leftSphere.GetComponent<Renderer> ().material.mainTexture = frameKeeper.frames [number];
	//	rightSphere.GetComponent<Renderer> ().material.mainTexture = frameKeeper.frames [number];
		//		rend.material.mainTexture = frames[currentFrame];
		frameCursor.transform.position = cursorPositions [number];
		leftSphere.transform.rotation = Quaternion.Euler (270, frameRotations [number], 0);
		rightSphere.transform.rotation = Quaternion.Euler (270, frameRotations [number], 0);
	}

}