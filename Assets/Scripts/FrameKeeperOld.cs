using UnityEngine;
using System.Collections;
using System.IO;

public class FrameKeeperOld : MonoBehaviour
{
	public int numberOfFrames = 5;
	public Texture[] frames;
	public bool isInitialized = false;

	void Start ()
	{
		frames = new Texture2D[numberOfFrames];
		InitializeFrames ();
		DontDestroyOnLoad (this.gameObject);
	}
	
	public void InitializeFrames() {
		for (int i = 0; i < numberOfFrames; ++i) {
			#if UNITY_EDITOR
			string filePath = string.Format ("sdCard/Comberry/VRA/0{0:d1}.jpg", i + 1);
			# else
			string filePath = string.Format ("/storage/extSdCard/Comberry/VRA/0{0:d1}.jpg", i + 1);
			# endif
			if (File.Exists (filePath)) {
				Debug.Log (" FrameManager: Loading " + filePath);
				Texture2D tex = new Texture2D (2, 2, TextureFormat.RGB24, false);
				tex.LoadImage (File.ReadAllBytes (filePath));
				frames [i] = tex;
			} else {
				Debug.LogError ("FrameManager: " + filePath + " not found!");
			}
		}
		isInitialized = true;
	}

}