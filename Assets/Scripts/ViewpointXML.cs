using UnityEngine;
using System.Collections;

public class ViewpointXML {

//	public int id;
	public string tag;
	public bool isVideo;
	public string filename; // Frontal 180° or 360°
	public float camRotation;
	public bool isStereoscopic;
	public bool isOverUnder; // Ignored here
	public bool is180 = false;
	public string rearFilename = ""; // for the rear 180°
	public string audioClip;
	public string audioClipEN;
	public bool loopAudio = false;
	public bool hasOverlay = false;
	public int loopSeekTo = -1;
	public Target[] targets;
	public bool playOnceAndReturn = false;
	public string loadPrefab = "";
	public bool monoscopic = false;
}

