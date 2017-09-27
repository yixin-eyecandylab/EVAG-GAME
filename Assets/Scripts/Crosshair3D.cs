/************************************************************************************

Filename    :   Crosshair3D.cs
Content     :   An example of a 3D cursor in the world based on player view
Created     :   June 30, 2014
Authors     :   Andrew Welch

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.


************************************************************************************/

// uncomment this to test the different modes.
#define CROSSHAIR_TESTING

using UnityEngine;
using System.Collections;				// required for Coroutines

public class Crosshair3D : MonoBehaviour
{

	// NOTE: three different crosshair methods are shown here.  The most comfortable for the
	// user is going to be when the crosshair is located in the world (or slightly in front of)
	// the position where the user's gaze is.  Positioning the cursor a fixed distance from the
	// camera inbetween the camera and the player's gaze will be uncomfortable and unfocused.
	public enum CrosshairMode
	{
		Dynamic = 0,			// cursor positions itself in 3D based on raycasts into the scene
		DynamicObjects = 1,		// similar to Dynamic but cursor is only visible for objects in a specific layer
		FixedDepth = 2,			// cursor positions itself based on camera forward and draws at a fixed depth
	}

	public CrosshairMode				mode = CrosshairMode.Dynamic;
	public int							objectLayer = 8;
	public float						offsetFromObjects = 0.1f;
	public float						fixedDepth = 3.0f;
	public float						maxDepth = 4.0f;
	public Transform					cameraTransform = null;
	public float raiseAngle = 0.0f;

	private Transform					thisTransform = null;
	private Material					crosshairMaterial = null;

	/// <summary>
	/// Initialize the crosshair
	/// </summary>
	void Awake()
	{
		thisTransform = transform;
//		if(VraSettings.instance.isGearVR) {
//			OVRCameraRig cameraController = GameObject.Find ("OVRCameraRig").GetComponent<OVRCameraRig> ();
//			if (cameraController == null)
//				Debug.LogError ("Crosshair3D: Could not find OVRCameraRig!");
//			else
//				cameraTransform = cameraController.centerEyeAnchor;
//		} else if (VraSettings.instance.isCardboard) {
//			GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
//			if (camera == null)
//				Debug.LogError ("Crosshair3D: Could not find MainCamera");
//			else
//				cameraTransform = camera.transform;
//		}

		// clone the crosshair material
		crosshairMaterial = GetComponent<Renderer>().material;
	}

	/// <summary>
	/// Cleans up the cloned material
	/// </summary>
	void OnDestroy()
	{
		if (crosshairMaterial != null)
		{
			Destroy(crosshairMaterial);
		}
	}

	/// <summary>
	/// Updates the position of the crosshair.
	/// </summary>
	void LateUpdate()
	{
#if CROSSHAIR_TESTING
		if (Input.GetButtonDown("Right Shoulder"))
		{
			//*************************
			// toggle the crosshair mode .. dynamic -> dynamic objects -> fixed depth
			//*************************
			switch(mode)
			{
			case CrosshairMode.Dynamic:
				mode = CrosshairMode.DynamicObjects;
				crosshairMaterial.color = Color.red;
				break;
			case CrosshairMode.DynamicObjects:
				mode = CrosshairMode.FixedDepth;
				crosshairMaterial.color = Color.blue;
				break;
			case CrosshairMode.FixedDepth:
				mode = CrosshairMode.Dynamic;
				crosshairMaterial.color = Color.white;
				break;
			}
			Debug.Log("Mode: " + mode);
		}
#endif
		Ray ray;
		RaycastHit hit;

		if (VraSettings.instance.isCardboard || (VraSettings.instance.isGearVR && OVRManager.display.isPresent))
		{
			// get the camera forward vector and position
			Vector3 cameraPosition = cameraTransform.position;
			Vector3 cameraForward = Vector3.RotateTowards(cameraTransform.forward, cameraTransform.up, raiseAngle*Mathf.Deg2Rad, 0.0f);
			GetComponent<Renderer>().enabled = true;

			//*************************
			// position the cursor based on the mode
			//*************************
			switch (mode)
			{
			case CrosshairMode.Dynamic:
				// cursor positions itself in 3D based on raycasts into the scene
				// trace to the spot that the player is looking at
				ray = new Ray(cameraPosition, cameraForward);
				if ( Physics.Raycast(ray, out hit))
				{
					if(maxDepth > 0 && hit.distance > maxDepth) {
						thisTransform.position = cameraPosition + (cameraForward * maxDepth);
					} else {
						thisTransform.position = hit.point + (-cameraForward * offsetFromObjects);
					}
				} else {
					// cursor positions itself based on camera forward and draws at a fixed depth
					thisTransform.position = cameraPosition + (cameraForward * fixedDepth);
				}
				thisTransform.forward = -cameraForward;
				break;
			case CrosshairMode.DynamicObjects:
				// similar to Dynamic but cursor is only visible for objects in a specific layer
				ray = new Ray(cameraPosition, cameraForward);
				if (Physics.Raycast(ray, out hit))
				{
					if (hit.transform.gameObject.layer != objectLayer)
					{
						GetComponent<Renderer>().enabled = false;
					}
					else
					{
						thisTransform.position = hit.point + (-cameraForward * offsetFromObjects);
						thisTransform.forward = -cameraForward;
						GetComponent<Renderer>().enabled = true;
					}
				} else {
					GetComponent<Renderer>().enabled = false;
				}
					thisTransform.localScale = Vector3.one * 0.04f * (Mathf.Floor(hit.distance)+1);
//				if(hit.distance < 2.0f) 
//					thisTransform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
//				else
//					thisTransform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
				break;
			case CrosshairMode.FixedDepth:
				// cursor positions itself based on camera forward and draws at a fixed depth
				thisTransform.position = cameraPosition + (cameraForward * fixedDepth);
				thisTransform.forward = -cameraForward;
				break;
			}

			if (Input.GetButtonDown(OVRGamepadController.ButtonNames[(int)OVRGamepadController.Button.A]))
			{
				ray = new Ray(cameraPosition, cameraForward);
				if (Physics.Raycast(ray, out hit))
				{
					hit.transform.gameObject.BroadcastMessage("OnClick", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		else
		{
			GetComponent<Renderer>().enabled = false;
		}
	}
}
