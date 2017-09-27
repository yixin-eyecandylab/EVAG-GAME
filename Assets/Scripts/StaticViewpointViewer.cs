using UnityEngine;
using System.Collections;

public class StaticViewpointViewer : MonoBehaviour
{
	public GameObject leftSphere, rightSphere;  // the two spheres
	public int numberOfFrames = 5;
	public float frameRate = 0.5f;
	
	private Texture[] frames;
	private Renderer rend;
	private int currentFrame = 0;

	void Start ()
	{
		rend = GetComponent<Renderer>();
		// load the frames
		frames = new Texture2D[numberOfFrames];
		for (int i = 0; i < numberOfFrames; ++i)
			frames[i] = (Texture2D)Resources.Load(string.Format("{0:d1}", i + 1));
		leftSphere.GetComponent<Renderer>().material.mainTexture = rightSphere.GetComponent<Renderer>().material.mainTexture = frames[currentFrame];
	}
	
	void Update () 	{
		if (Input.GetMouseButtonDown (0)) {
			currentFrame += 1;
			if (currentFrame >= frames.Length)
					currentFrame = 0;
			leftSphere.GetComponent<Renderer>().material.mainTexture = rightSphere.GetComponent<Renderer>().material.mainTexture = frames [currentFrame];
			//		rend.material.mainTexture = frames[currentFrame];
			Debug.Log ("Showing frame " + currentFrame);
		}
	}

	public void NextFrame () 	{
		currentFrame += 1;
		if (currentFrame >= frames.Length)
			currentFrame = 0;
		leftSphere.GetComponent<Renderer>().material.mainTexture = rightSphere.GetComponent<Renderer>().material.mainTexture = frames [currentFrame];
		//		rend.material.mainTexture = frames[currentFrame];
		Debug.Log ("Showing frame " + currentFrame);
	}

}