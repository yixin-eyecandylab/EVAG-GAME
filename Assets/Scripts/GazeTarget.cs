using UnityEngine;
using System.Collections;

public class GazeTarget : MonoBehaviour {

	public Texture selectedTexture;
	public Texture unselectedTexture;
    public bool scaleWhenHit = true;
	public GameObject cursor;
    public float focusTime = 1.0f;
	public bool actOnGaze = true;
	public bool actOnMouseClick = false;
	public bool actOnSwipeForward = false;
	public string title = "weiter";
	public float ignoreTimeAfterAction = 2.0f;
	public bool isIgnoring = false;
	GazeTargetAction targetAction;
	bool invokedAction = false;
	Renderer rend;
	public bool isSelected = false;
	public bool isFocused = false;
	public TextMesh label;
	public TMPro.TextMeshPro tmpLabel;
	public bool enableAudio = false;
	public AudioClip hitClip;
	public AudioClip actionClip;

	// Swipe Detection
	private Vector2 direction;
	private bool buttonDown = false;
	private ViewpointManager vpm;
	private bool isMine = true;
	private string basename;
	NetworkView nw;
		
	// I know, I suck at state machines...
	// The idea is: Keep looking at the GazeTarget for <focusTime> seconds without looking away, then someting happens.

	// Use this for initialization
	void Awake () {
		if(cursor!=null) 
			rend = cursor.GetComponent<Renderer> ();
		nw = GetComponent<NetworkView> ();
		if (nw != null && !nw.isMine) 
			isMine = false;
		targetAction = gameObject.GetComponent<GazeTargetAction> ();
		SetUnselected ();
		// Find ViewpointManager (vpm)
		GameObject vpmObject = GameObject.FindGameObjectWithTag ("ViewpointManager"); 
		if (vpmObject == null) {
			Debug.LogError ("GazeTarget: Could not find ViewpointManager");
		} else {
			vpm = vpmObject.GetComponent<ViewpointManager>();
		}
		basename = gameObject.name;
	}

	private void PlayAudioClip(AudioClip clip) {
		if(enableAudio && clip != null) {
			AudioSource source = GetComponent<AudioSource>();
			if(source != null) {
				source.PlayOneShot(clip);
			}
		}
	}

// Called when target has been hit by gaze cursor
	public void Hit() {
		if (isIgnoring)
			return;
		if(!isSelected) {
			PlayAudioClip(hitClip);
		}
		SetSelected ();
		if (isMine) {
			if (actOnGaze && !invokedAction) {
				Invoke ("Action", focusTime); // Invoke defined action in focusTime...
				invokedAction = true;
			}
//			if(nw != null)
//				nw.RPC ("HitRemote", RPCMode.Others);
		}
		// Has Gaze Focus
		isFocused = true;
		Invoke("SetUnfocused", 0.066f);
	}

    // Called when target has been hit by gaze cursor
    public void DestroyPulse()
    {
        //if (isIgnoring)
        //    return;
        //if (!isSelected)
        //{
        //    PlayAudioClip(hitClip);
        //}
        SetSelected();
        if (isMine)
        {
            if (actOnGaze && !invokedAction)
            {
                Invoke("Action", focusTime); // Invoke defined action in focusTime...
                invokedAction = true;
            }
            //			if(nw != null)
            //				nw.RPC ("HitRemote", RPCMode.Others);
        }
        // Has Gaze Focus
        isFocused = true;
        Invoke("SetUnfocused", 0.066f);
    }

    public void SetUnfocused() {
		isFocused = false;
	}

	[RPC] public void HitRemote() {
		Hit ();
	}	

	// Sets the GazeTarget to be visually selected.
	public void SetSelected() {
		if(rend!=null)
			rend.material.mainTexture = selectedTexture;
		isSelected = true;
		CancelInvoke("SetUnselected");
		Invoke("SetUnselected", 0.2f); // Automatically unselect
		if(isMine && nw != null) {
			nw.RPC ("SetSelectedRemote", RPCMode.Others);
		}
	}

	[RPC] public void SetSelectedRemote() {
		SetSelected ();
	}	

	// Visually resets the GazeTarget
	public void SetUnselected() {
        if (rend != null)
            rend.material.mainTexture = unselectedTexture;
        if (cursor != null && scaleWhenHit)
            cursor.transform.localScale = new Vector3(1, 1, 1);
        CancelInvoke("Action");
		isSelected = false;
		invokedAction = false;
		isIgnoring = false;
	}

	public void Action() {
		if (isMine) {
			isIgnoring = true; 
			PlayAudioClip(actionClip);
			if(targetAction != null) {
				Debug.Log (gameObject.name + " invoking GazeTargetAction");
				targetAction.PerformAction ();
			}
			invokedAction = false;
			CancelInvoke ("SetUnselected");
			CancelInvoke("Action");
			Invoke ("SetUnselected", ignoreTimeAfterAction); 
		}
	}

	public void Update() {
        if (cursor != null && scaleWhenHit)
        {
            Vector3 scale = cursor.transform.localScale;
            if (isSelected && scale.x < 2.8)
                cursor.transform.localScale = scale * (1 + Time.deltaTime);
        }

        if (targetAction != null && targetAction.isPerforming ())
			SetSelected ();

		if (isMine) {
			if (actOnMouseClick && isFocused && Input.GetMouseButtonDown(0)) {
				Debug.Log (gameObject.name + ": GetMouseButtonDown, Action!");
				Action ();
			}

			if (actOnSwipeForward) {
				// Swipe detection
				if (Input.GetMouseButton (0)) {
					if (buttonDown) {
						float h = Input.GetAxis ("Mouse X");
						float v = Input.GetAxis ("Mouse Y");
						direction += new Vector2 (h * Time.deltaTime, v * Time.deltaTime);
					} else {
						buttonDown = true;
						direction = new Vector2 (0.0f, 0.0f);
					}
				} else if (buttonDown) { // Button just lifted
					float length = Mathf.Sqrt (Mathf.Pow (direction.x, 2) + Mathf.Pow (direction.y, 2));
					float angle = Mathf.Atan (direction.y / direction.x) * (180.0f / Mathf.PI);
					Debug.Log ("Swipe direction:" + direction.x + " " + direction.y + " length: " + length + " angle: " + angle);
					buttonDown = false;
					if (length > 0.5f) {
						// Swipe long enough
						if (Mathf.Abs (angle) < 30) {
							// Horizontal swipe
							if (direction.x < 0.0f) { // x is inversed on the Gear VR touchpad
								Debug.Log ("Swipe detected: FORTH");
								Action ();
							} else if (direction.x > 0.0f) { 
								Debug.Log ("Swipe detected: BACK");
							}
						}
					}
				}
			}
		}
		if (!isMine && !Network.isClient) {
			Network.Destroy (gameObject);
		}
	}

	// Resets and activates a GazeTarget at new position
	public void SetPosition(Vector3 newPos) {
		gameObject.transform.position = newPos;
		gameObject.transform.LookAt (new Vector3 (0, 0, 0));
		SetUnselected ();
		isIgnoring = true;
		Invoke ("CancelIgnoring", 1.0f); // Activate GazeTarget automatically after placing it somewhere.
		if(isMine && nw != null) {
			Network.RemoveRPCs (nw.viewID);
			nw.RPC ("SetPositionRemote", RPCMode.Others, newPos);
		}
	}

	[RPC] public void SetPositionRemote(Vector3 newPos) {
		SetPosition(newPos);
	}

    public void SetPulsePosition(Vector3 newPos)
    {
        gameObject.transform.position = newPos;
        //gameObject.transform.LookAt(new Vector3(0, 0, 0));
        if (isMine && nw != null)
        {
            Network.RemoveRPCs(nw.viewID);
            nw.RPC("SetPulsePositionRemote", RPCMode.Others, newPos);
        }
    }

    [RPC]
    public void SetPulsePositionRemote(Vector3 newPos)
    {
        SetPulsePosition(newPos);
    }

    
    public void SetPulseRotate(Vector3 newRot)
    {
        gameObject.transform.Rotate(newRot);// = newRot;
        //gameObject.transform.LookAt(new Vector3(0, 0, 0));
        if (isMine && nw != null)
        {
            Network.RemoveRPCs(nw.viewID);
            nw.RPC("SetPulseRotateRemote", RPCMode.Others, newRot);
        }
    }

    [RPC]
    public void SetPulseRotateRemote(Vector3 newRot)
    {
        SetPulseRotate(newRot);
    }

    public void SetDiamondColor(float Temperature)
    {
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(1.0f, Temperature, Temperature);
        if (isMine && nw != null)
        {
            Network.RemoveRPCs(nw.viewID);
            nw.RPC("SetDiamondColorRemote", RPCMode.Others, Temperature);
        }
    }

    [RPC]
    public void SetDiamondColorRemote(float Temperature)
    {
        SetDiamondColor(Temperature);
    }
    // Set scale of the GazeTarget
    public void SetScale(float scale) {
		Vector3 orgScale = this.transform.localScale;
		this.transform.localScale = new Vector3(orgScale.x*scale, orgScale.y*scale, orgScale.z*scale);
		if(isMine && nw != null) {
			Network.RemoveRPCs (nw.viewID);
			nw.RPC ("SetScaleRemote", RPCMode.Others, scale);
		}
	}
	
	[RPC] public void SetScaleRemote(float scale) {
		SetScale(scale);
	}	

	public void SetLabel(string newText) {
		gameObject.name = basename+newText;
		if(label != null)
			label.text = newText;
		if (tmpLabel != null)
			tmpLabel.text = newText;
		if(isMine && nw != null) {
			nw.RPC ("SetLabelRemote", RPCMode.Others, newText);
		}
	}
	
	[RPC] public void SetLabelRemote(string newText) {
		SetLabel(newText);
	}	
	
	private void CancelIgnoring() {
		isIgnoring = false;
	}

	public void SetActive(bool active) {
		gameObject.SetActive (active);
		if (isMine && nw != null) {
			nw.RPC ("SetActiveRemote", RPCMode.Others,active);
		}
	}

	[RPC] public void SetActiveRemote(bool active) {
		gameObject.SetActive (active);
	}

    //Set Game Pulses positions

    // Resets and activates a GazeTarget at new position
    public void SetGamePulsePosition(Vector3 newPos, Vector3 newRot)
    {
        gameObject.transform.position = newPos;
        gameObject.transform.Rotate(newRot);
        //gameObject.transform.LookAt(new Vector3(0, 0, 0));
        //SetUnselected();
        //isIgnoring = true;
        //Invoke("CancelIgnoring", 1.0f); // Activate GazeTarget automatically after placing it somewhere.
        if (isMine && nw != null)
        {
            Network.RemoveRPCs(nw.viewID);
            nw.RPC("SetGamePulsePositionRemote", RPCMode.Others, newPos, newRot);
        }
    }

    [RPC]
    public void SetGamePulsePositionRemote(Vector3 newPos, Vector3 newRot)
    {
        SetGamePulsePosition(newPos, newRot);
    }

}
