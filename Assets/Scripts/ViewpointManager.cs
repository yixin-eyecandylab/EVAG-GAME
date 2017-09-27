using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices; // required for DllImport

public class ViewpointManager : MonoBehaviour
{
	public GameObject cameraRig;   // Camera Rig (OVR or non-OVR)
	public GameObject leftSphere;  // the two spheres
	public GameObject rightSphere;  // the two spheres
	public ScreenFade leftFade, rightFade; // Object holding the ScreenFade behaviour (LeftEye/RightEyeAnchor)
	public bool swipeInput = true; // Allow swipe gestures
	public float fadeOutTime = 0.1f;
	public float fadeInTime = 0.2f;
    //	public GameObject nextTargetPrefab;
    //	public GameObject previousTargetPrefab;
    private GameObject ShortLaser;
    public GameObject jumpTargetPrefab;
	public GameObject startButtonPrefab;
	public GameObject stopButtonPrefab;
	public GameObject prefTargetPrefab;
	public GameObject prefDETargetPrefab;
	public GameObject prefENTargetPrefab;
	public GameObject frameKeeperPrefab;
	public GameObject mediaPlayerCtrlPrefab;
	public GameObject infoPointPrefab;
	public GameObject labelPrefab;
	public GameObject loadLevelPrefab;
    public GameObject bannerPrefab;
    public GameObject simulatorPrefab;
    public GameObject DiamondGamePrefab;
	public AudioSource audioSource;
	public AudioSource backgroundAudioSource;
	public string filename = "viewpoints.xml";
	public OverlayAnimator overlayAnim;
	public UnityEngine.UI.Text errorMessage;
	public Canvas errorMessageCanvas;

	// Public, but not meant to be set in the editor
	public int currentViewpoint = 0;
	public int firstViewpoint = 0;
	public MediaPlayerCtrl mediaPlayer;
	public bool hasVideoTexture=false;
	public string targetMediaPlayerFileName = "";
	public string currentMediaPlayerFileName = "";
	public LANGUAGE lang = LANGUAGE.ENGLISH;
	public bool isCardboard = false;

	private GameObject[] targets = new GameObject[10];
	private GameObject simulator;
    private GameObject DiamondGame;
	private FrameKeeper frameKeeper;
	private ViewpointXML[] viewpoints;
	private ViewpointXML vp;

    private Material leftFrontMat, leftRearMat, rightFrontMat, rightRearMat;
	private bool videoShouldPlay = false;
	private bool videoPaused = false;
	private bool hasTemporaryVideoTexture = false;
	private Texture defaultTexture;
	private bool isFadedOut;
	private int videoDuration = 0;
	private int mediaPlayerErrorCount = 0;
	private float lastMediaPlayerLoad = 0;
	private bool applicationPaused = false;
	private int skippedFrames = 0;

	// Swipe Detection
	private Vector2 direction;
	private bool buttonDown = false;

    private bool ViewpointLoadedOnGearvr = false;

	public enum LANGUAGE {
		GERMAN = 0,
		ENGLISH = 1
	}

//	#if UNITY_EDITOR
//	private string fileDir = "sdCard/Comberry/VRA-V90/";
//	# else
//	private string fileDir = "/storage/sdcard0/Comberry/VRA-V90/";
//	# endif
	private string fileDir;
	private string videoDir = null;

	public IEnumerator Start ()
	{
		Debug.Log ("VPM: Comberry Seidenader VRA-V90 starting, " + Application.productName + ", Version: " + Application.version);
		Debug.Log ("VPM: Application.persistentDataPath: "+Application.persistentDataPath);
		Debug.Log ("VPM: Application.streamingAssetsPath: "+Application.streamingAssetsPath);

		leftFrontMat = leftSphere.GetComponent<Renderer> ().materials[0];
		leftRearMat = leftSphere.GetComponent<Renderer> ().materials[1];
		if(rightSphere != null) {
			rightRearMat = rightSphere.GetComponent<Renderer> ().materials[1];
			rightFrontMat = rightSphere.GetComponent<Renderer> ().materials[0];
		}
		defaultTexture = leftFrontMat.mainTexture;

		fileDir = Application.streamingAssetsPath+"/";
		Debug.Log ("VPM: fileDir: "+fileDir);

//		if(Network.isServer)
//			videoDir = fileDir;
//		else
//			videoDir = fileDir + "Tablet-Videos/";
//		videoDir = ""; // Search in Streaming-Assets
		#if (UNITY_ANDROID && !UNITY_EDITOR)
		videoDir = Application.persistentDataPath+"/";
		# else
		videoDir = fileDir;
		# endif
		Debug.Log ("VPM: videoDir: "+videoDir);

		// Instantiate or reuse FrameKeeper
		if (GameObject.Find ("FrameKeeper") == null) {
			Debug.Log ("VPM: Instantiating FrameKeeper");
			GameObject fkObj = (GameObject) Instantiate (frameKeeperPrefab);
			fkObj.name = "FrameKeeper";
			frameKeeper = fkObj.GetComponent<FrameKeeper> ();
		} else {
			Debug.Log ("VPM: Existing FrameKeeper found");
			frameKeeper = GameObject.Find ("FrameKeeper").GetComponent<FrameKeeper> ();
		}
		// Instantiate or reuse MediaPlayer
		if (GameObject.Find ("MediaPlayerCtrl") == null) {
			Debug.Log ("VPM: Instantiating MediaPlayerCtrl");
			GameObject mpObj = (GameObject) Instantiate (mediaPlayerCtrlPrefab);
			mpObj.name = "MediaPlayerCtrl";
			mediaPlayer = mpObj.GetComponent<MediaPlayerCtrl> ();
			DontDestroyOnLoad(mpObj);
		} else {
			Debug.Log ("VPM: Existing MediaPlayerCtrl found");
			GameObject mpObj = GameObject.Find ("MediaPlayerCtrl");
			mediaPlayer = mpObj.GetComponent<MediaPlayerCtrl> ();
			DontDestroyOnLoad(mpObj);
		}
		PlayerPrefs.SetString ("lang", "EN"); // Force language to english
		yield return new WaitForSeconds(0.066f);
		Reset ();
//		SaveXML ();
	}

	public void LoadLevel(string levelName) {
		Debug.Log ("VPM: LoadLevel "+levelName);
		StopVideo();
		GameObject nws = GameObject.FindGameObjectWithTag ("NetworkSync");
		if (nws != null) {
			nws.GetComponent<NetworkSync> ().LoadLevel (levelName); // Will only happen when nws.isMine
		}
		Application.LoadLevel(levelName);
		FadeOut();
	}

	public void Reset() {
		Debug.Log ("VPM: Reset");
		FadeOutNow (); // Black, immediately
		ShowErrorMessage("");
        //GameObject obj = GameObject.FindGameObjectWithTag("NewHightScroeKeeper");
        //if (obj != null)
        //{
        //    HighScoreKeeper hsk = obj.GetComponent<HighScoreKeeper>();
        //    hsk.ClosePanels();
        //}

        // Load XML
        //		WWW www = new WWW(fileDir+filename);
        //		yield return www;
        //		ViewpointContainer vc = ViewpointContainer.LoadFromText(www.text);
        ViewpointContainer vc = ViewpointContainer.Load(videoDir+filename);
		viewpoints = vc.viewpoints;
		Debug.Log ("VPM: found " + viewpoints.Length + " viewpoints");

		// Initialize first viewpoint (without Fade-Out!)
		StopVideo();
		Debug.Log ("VPM: Network Sync");
		if(Network.isServer || Network.isClient) {
			GameObject nws = GameObject.FindGameObjectWithTag ("NetworkSync");
			if (nws != null) {
				nws.GetComponent<NetworkSync> ().LoadViewpoint (0);
			}
		}
		LoadViewpointInt (firstViewpoint, true);
	}

	private void Recenter() {
	}

//	public void LoadXML() {
//		ViewpointContainer vc = ViewpointContainer.Load (fileDir + filename);
//		viewpoints = vc.viewpoints;
//	}

	// Was used for initial XML creation.
//	public void SaveXML() {
//		ViewpointContainer vc = new ViewpointContainer();
//		ViewpointXML[] xmlvp = new ViewpointXML[20];
//		int i = 0;
//		foreach (Viewpoint vp in viewpoints) {
//			xmlvp[i] = new ViewpointXML();
//			xmlvp[i].isVideo = vp.isVideo;
//			xmlvp[i].filename = vp.filename;
//			xmlvp[i].frameRotation = vp.frameRotation;
//			xmlvp[i].isStereoscopic = vp.isStereoscopic;
//			if(vp.audioClip!=null)
//				xmlvp[i].audioClip = vp.audioClip.name;
//			xmlvp[i].loopAudio = vp.loopAudio;
//			xmlvp[i].nextPosition = vp.nextPosition;
//			xmlvp[i].previousPosition = vp.previousPosition;
//			i++;
//		}
//		vc.viewpoints=xmlvp;
//		vc.Save (fileDir+"viewpoints.xml");
//	}

	public int nextViewpointNumber() {
		return (currentViewpoint < (viewpoints.Length - 1)) ? currentViewpoint + 1 : 0;
	}
	
	
	public int previousViewpointNumber() {
		return (currentViewpoint > 0) ? currentViewpoint - 1 : viewpoints.Length -1 ;
	}
	
	public void ReloadViewpoint() {
		LoadViewpoint (currentViewpoint);
	}

	public void NextViewpoint () 	{
		LoadViewpoint (nextViewpointNumber());
	}

	public void PreviousViewpoint () 	{
		LoadViewpoint (previousViewpointNumber());
	}

	public void NextViewpointTablet () 	{
		GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
		if (nws != null) {
			nws.GetComponent<NetworkSync> ().NextViewpoint ();
		}
	}
	
	public void PreviousViewpointTablet () 	{
		GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
		if (nws != null) {
			nws.GetComponent<NetworkSync> ().PreviousViewpoint ();
		}
	}
	
	public void LoadViewpoint(string tag, bool recenter=false) {
		Debug.Log ("VPM: Load Viewpoint tagged " + tag);
		for(int i=0; i < viewpoints.Length; i++) {
			if(viewpoints[i].tag == tag) {
				LoadViewpoint(i, recenter);
				return;
			}
		}
		Debug.LogError ("VPM: Viewpoint with tag " + tag + " not found!");
	}
	
	public void LoadViewpoint (int number, bool recenter=false) 	{
		StartCoroutine (FadeAndLoadRoutine (number, recenter));
	}
	
	private IEnumerator FadeAndLoadRoutine (int number, bool recenter) 	{
		Debug.Log ("VPM: Fading out viewpoint " + currentViewpoint);
		videoShouldPlay = false; 
		FadeOut();
		// Network Sync
		Debug.Log ("VPM: Network Sync");
		GameObject nwsObj = GameObject.FindGameObjectWithTag("NetworkSync");
		if (nwsObj != null) {
			NetworkSync nws = nwsObj.GetComponent<NetworkSync> ();
			if (nws != null) {
				nws.LoadViewpoint (number);
			} else {
				Debug.Log ("VPM: FadeAndLoadRoutine NetworkSync found but Component missing.");
			}
		}
		yield return new WaitForSeconds (fadeOutTime+0.033f); // Add one frame to make sure it's completely faded.
//		yield return new WaitForSeconds (fadeOutTime+0.099f); // Add one frame to make sure it's completely faded.
        //if(VraSettings.instance.isTablet)
        //    yield return new WaitForSeconds(2.0f);

        if(VraSettings.instance.isHeadset)
            LoadViewpointInt(number, recenter);
	}


	/* 
	 * LOAD VIEWPOINT
	 */ 
	private void LoadViewpointInt(int number, bool recenter) {
		Debug.Log ("VPM: Loading Viewpoint " + number +" ("+Time.time*1000+")");
		currentViewpoint = number;
		Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
		LoadPrefs ();
		vp = viewpoints [number];
		// Media
		if(vp.filename != null && vp.filename.Length > 0) {
			// Don't show anything when filename is not set. SimulatorPrefab will take care of that. 
			if (vp.isVideo) {
				#if UNITY_EDITOR
				ShowPicture(vp.rearFilename);
				#else
				PlayVideo (vp.filename, vp.rearFilename);
				#endif
			} else {
				if (vp.is180) {
					ShowPicture(vp.filename, vp.rearFilename);
				} else {
					ShowPicture (vp.filename);
				}
				Invoke("setPerfLow",2.0f);
			}
		}
		videoDuration = 0;
		if (vp.loopSeekTo > 0) {
			Debug.Log ("VPM: loopSeekTo "+vp.loopSeekTo+", disabling auto-loop");
			mediaPlayer.m_bLoop = false;
		} else {
			mediaPlayer.m_bLoop = true;
		}
		// Stereo/Mono/360/180 mode
		if (vp.isStereoscopic) {
			if (vp.is180)
				SetStereo180();
			else
				SetStereo360 ();
		} else {
			if (vp.is180) {
				#if UNITY_EDITOR
					SetMono360 (); // No video in editor, show background image all around
				#else
					SetMono180 ();
				#endif
			} else
				SetMono360 ();
		}

		// Rotation
		if(VraSettings.instance.isHeadset) {
			cameraRig.transform.rotation = Quaternion.Euler (0, vp.camRotation, 0);
			if(recenter) {
				if (VraSettings.instance.isGearVR) {
					Debug.Log ("VPM: Recenter");
					OVRManager.display.RecenterPose ();
				} else if (VraSettings.instance.isCardboard) {
					Cardboard.SDK.Recenter (); 
				}
			}
		}
		if (Network.isServer) {
			GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
			if (nws != null) {
				nws.GetComponent<FollowCameraRig> ().SyncNow();
			}
		}

		// Gaze Target
		if (VraSettings.instance.isHeadset) {
			UpdateGazeTargets(vp);
		}

		// Audio
		Debug.Log ("VPM: Background-Audio");
		if(backgroundAudioSource != null) {
			if ((vp.audioClip != null && (lang == LANGUAGE.GERMAN || vp.audioClipEN == null))) {
				//			audioSource.clip = Resources.Load(vp.audioClip.name) as AudioClip;
				backgroundAudioSource.clip = Resources.Load (vp.audioClip) as AudioClip;
				Invoke ("PlayBackgroundAudio", 0.2f); //Todo: vp.audioDelay
				backgroundAudioSource.loop = vp.loopAudio;
			} else if (vp.audioClipEN != null && lang==LANGUAGE.ENGLISH) {
				//			audioSource.clip = Resources.Load(vp.audioClip.name) as AudioClip;
				backgroundAudioSource.clip = Resources.Load (vp.audioClipEN) as AudioClip;
				Invoke ("PlayBackgroundAudio", 0.2f); //Todo: vp.audioDelay
				backgroundAudioSource.loop = vp.loopAudio;
			} else {
				backgroundAudioSource.Stop();
			}
		}	
			// Overlay
		if(overlayAnim != null) {
			if (vp.hasOverlay)
				overlayAnim.Play ();
			else
				overlayAnim.Stop ();
		}

//		// Prepare video for next VP if this one is a picture
//		if (!vp.isVideo && viewpoints[nextViewpointNumber()].isVideo) {
//			PrepareVideo (viewpoints[nextViewpointNumber()].filename);
//		}

		// RemoveTouchMarkers
		foreach(GameObject marker in GameObject.FindGameObjectsWithTag("TouchMarker")) {
			NetworkView nview = marker.GetComponent<NetworkView>();
			if(nview != null && nview.isMine) {
				Network.Destroy(marker);
			}
		}
		if(Network.isServer || Network.isClient) {
			Network.RemoveRPCsInGroup(777); // Get rid of leftover touch markers.
		}

		// Monscopic
		if (VraSettings.instance.isGearVR) {
			OVRManager.instance.monoscopic = vp.monoscopic;
		}

		// Simulator?
		if(simulator != null) 
			GameObject.Destroy(simulator);
		if(vp.loadPrefab == "Simulator") {
			Invoke("LoadSimulator",0.033f); // Wait one Frame before instiating
		}

        if (DiamondGame != null)
        {
            Network.Destroy(DiamondGame);
            //if ((VraSettings.instance.isTablet) && (DiamondGame != null))
            //    GameObject.Destroy(DiamondGame);
        }

        //Network.Destroy(DiamondGame);

        if (vp.loadPrefab == "DiamondGame")
        {
            Invoke("LoadDiamondGame", 0.033f); // Wait one Frame before instiating
        }
        // Close HighScore Panels
        //GameObject obj = GameObject.FindGameObjectWithTag("NewHightScroeKeeper");
        //if (obj != null)
        //{
        //    HighScoreKeeper hsk = obj.GetComponent<HighScoreKeeper>();
        //    hsk.ClosePanels();
        //}

    }

	public void LoadSimulator() {
		simulator = GameObject.Instantiate(simulatorPrefab);
	}

    public void LoadDiamondGame()
    {
        //DiamondGame = GameObject.Instantiate(DiamondGamePrefab);
        if (VraSettings.instance.isHeadset)
            DiamondGame = (GameObject)Network.Instantiate(DiamondGamePrefab, Vector3.zero, Quaternion.identity, 0);
    }

    public void UpdateGazeTargets(ViewpointXML vp) {
		// Gaze targets
		Debug.Log ("VPM: Gaze targets");
		GazeTarget gt;
		Debug.Log ("VPM: Targets");
		foreach (GameObject ipObj in targets) {
			if (ipObj != null) {
				Network.Destroy (ipObj);
			}
		}
		int i = 0;
		if (vp.targets != null) { 
			foreach (Target target in vp.targets) {
				Debug.Log ("VPM: Target " + target.style + " " + target.label);
				GameObject obj = null;
//				if(target.style == Target.TARGET_STYLE.NEXT)
//					obj = (GameObject) Network.Instantiate (nextTargetPrefab, Vector3.zero, Quaternion.identity, 0);
				/*else*/ if(target.style == Target.TARGET_STYLE.JUMP)
					obj = (GameObject) Network.Instantiate (jumpTargetPrefab, Vector3.zero, Quaternion.identity, 0);
//				else if(target.style == Target.TARGET_STYLE.PREVIOUS)
//					obj = (GameObject) Network.Instantiate (previousTargetPrefab, Vector3.zero, Quaternion.identity, 0);
				else if(target.style == Target.TARGET_STYLE.INFO)
					obj = (GameObject) Network.Instantiate (infoPointPrefab, Vector3.zero, Quaternion.identity, 0);
				else if(target.style == Target.TARGET_STYLE.START)
					obj = (GameObject) Network.Instantiate (startButtonPrefab, Vector3.zero, Quaternion.identity, 0);
				else if(target.style == Target.TARGET_STYLE.STOP)
					obj = (GameObject) Network.Instantiate (stopButtonPrefab, Vector3.zero, Quaternion.identity, 0);
				else if(target.style == Target.TARGET_STYLE.PREF)
					obj = (GameObject) Network.Instantiate (prefTargetPrefab, Vector3.zero, Quaternion.identity, 0);
				else if(target.style == Target.TARGET_STYLE.PREF_DE)
					obj = (GameObject) Network.Instantiate (prefDETargetPrefab, Vector3.zero, Quaternion.identity, 0);
				else if(target.style == Target.TARGET_STYLE.PREF_EN)
					obj = (GameObject) Network.Instantiate (prefENTargetPrefab, Vector3.zero, Quaternion.identity, 0);
				else if(target.style == Target.TARGET_STYLE.LABEL)
					obj = (GameObject) Network.Instantiate (labelPrefab, Vector3.zero, Quaternion.identity, 0);
				else if(target.style == Target.TARGET_STYLE.LOAD_LEVEL)
					obj = (GameObject) Network.Instantiate (loadLevelPrefab, Vector3.zero, Quaternion.identity, 0);
                else if (target.style == Target.TARGET_STYLE.BANNER)
                    obj = (GameObject)Network.Instantiate(bannerPrefab, Vector3.zero, Quaternion.identity, 0);
                targets [i++] = obj;
				// Scale
				obj.GetComponent<GazeTarget> ().SetScale(target.scale);
				// Position
				obj.GetComponent<GazeTarget> ().SetPosition (target.position);
				// Label
				if(lang==LANGUAGE.GERMAN || target.labelEN==null)
					obj.GetComponent<GazeTarget> ().SetLabel (target.label);
				else if (lang==LANGUAGE.ENGLISH)
					obj.GetComponent<GazeTarget> ().SetLabel (target.labelEN);
				// Audio
				if(lang == LANGUAGE.GERMAN && target.audioClip != null) {
//					AudioClip clip = Resources.Load (target.audioClip) as AudioClip;
//					obj.GetComponent<GazeTargetActionPlayAudioClip> ().audioClip = clip;
					obj.GetComponent<GazeTargetActionPlayAudioClip> ().LoadAudioClip(target.audioClip);
				} else if(lang == LANGUAGE.ENGLISH && target.audioClipEN != null) {
//					AudioClip clip = Resources.Load (target.audioClipEN) as AudioClip;
//					obj.GetComponent<GazeTargetActionPlayAudioClip> ().audioClip = clip;
					obj.GetComponent<GazeTargetActionPlayAudioClip> ().LoadAudioClip(target.audioClipEN);
				}
				// Jump / Stop Target
				if(obj.GetComponent<GazeTargetActionLoadViewpoint>() != null) {
					obj.GetComponent<GazeTargetActionLoadViewpoint>().tag = target.tag;
					obj.GetComponent<GazeTargetActionLoadViewpoint>().recenter = target.recenter;
				}
				// Pref Target
				if(target.style == Target.TARGET_STYLE.PREF) {
					obj.GetComponent<GazeTargetActionSetPlayerPref>().key = target.pref_key;
					obj.GetComponent<GazeTargetActionSetPlayerPref>().value = target.pref_value;
				}
				// LoadLevel Target
				if(target.style == Target.TARGET_STYLE.LOAD_LEVEL) {
					obj.GetComponent<GazeTargetActionLoadLevel>().levelName = target.levelname;
				}
			}
		}
	}

    public void OnPlayerConnected()
    {
        //Debug.Log("VPM: New Player connected, update his viewpoint");
        //Network Sync;
        //GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
        //if (nws != null)
        //{
        //    nws.GetComponent<NetworkSync>().LoadViewpoint(currentViewpoint);
        //}
        //UpdateGazeTargets(viewpoints[currentViewpoint]);
    }

    public void OnConnectedToServer()
    {
        if (VraSettings.instance.isTablet)
            LoadViewpointInt(0, false);
    }

    public void OnDisconnectedFromServer() {
		ShowDefaultPicture();
	}

	public void OnApplicationPause(bool paused) {
		Debug.Log ("VPM: ApplicationPause("+paused+") ("+Time.time*1000+")");
		if(paused) {
			Debug.Log ("VPM: mediaPlayer.UnLoad()");
			//			Debug.Log ("VPM: ApplicationPaused");
			mediaPlayer.UnLoad();
		} else if(applicationPaused) {
			Debug.Log ("VPM: => Resume");
			LoadViewpoint(currentViewpoint);
		}
		applicationPaused = paused;
	}

	public void OnDestroy() {
		Debug.Log ("VPM: OnDestroy, cleaning up ("+Time.time*1000+")");
		Debug.Log ("VPM: mediaPlayer.UnLoad()");
		mediaPlayer.UnLoad();
	}

	private void PlayAudio() {
		audioSource.Play ();
	}

	private void PlayBackgroundAudio() {
		backgroundAudioSource.Play ();
	}
	
	public void ShowDefaultPicture() {
		Debug.Log ("VPM showing default picture");
		leftFrontMat.mainTexture = leftRearMat.mainTexture = defaultTexture;
		if(rightFrontMat != null && rightRearMat != null) {
			rightFrontMat.mainTexture = rightRearMat.mainTexture = defaultTexture;
		}
		StopVideo ();
		if(simulator != null) 
			GameObject.Destroy(simulator);
		if(isFadedOut)
			FadeIn ();
	}

	public void ShowPicture(string filename) {
		Debug.Log ("VPM showing picture " + filename);
		leftFrontMat.mainTexture = leftRearMat.mainTexture = frameKeeper.TextureForFile (filename);
		if(rightFrontMat != null && rightRearMat != null) {
			rightFrontMat.mainTexture = rightRearMat.mainTexture = leftFrontMat.mainTexture;
		}
		StopVideo ();
		if(isFadedOut)
			FadeIn ();
	}

	public void ShowPicture(string front, string rear) {
		Debug.Log ("VPM showing picture " + filename);
		leftFrontMat.mainTexture = frameKeeper.TextureForFile (front);
		leftRearMat.mainTexture = frameKeeper.TextureForFile (rear);
		if(rightFrontMat != null && rightRearMat != null) {
			rightFrontMat.mainTexture = leftFrontMat.mainTexture; 
			rightRearMat.mainTexture = leftRearMat.mainTexture;
		}
		StopVideo ();
		if(isFadedOut)
			FadeIn ();
	}

	/* For the V90+ VRA, we will find the following combinations:
		360° stereo (still): One 4k*4k texture, over-under stereo, 360°
		180° stereo video: One 4K texture with 180° side-by-side stereo video for the front. Rear from above.
		360° mono (vidoe/still): One texture 4k 360° mono
		180° mono (video): One 2k texture with 180° mono video for the front, rear from above

		The following methods are made for a sphere (same left+right) using 
		two materials with acommon UV map (front-rear side-by-side).
	 */

	// Front: 180° mono (video), rear: part of 360° mono (still)
	public void SetMono180() {
		leftFrontMat.mainTextureScale = new Vector2 (2.0f, 1.0f);
		leftFrontMat.mainTextureOffset = new Vector2 (0.0f, 0.0f);
		leftRearMat.mainTextureScale = new Vector2 (1.0f, 1.0f);
		leftRearMat.mainTextureOffset = new Vector2 (0.75f, 0.0f);
		if(rightFrontMat != null && rightRearMat != null) {
			rightFrontMat.mainTextureScale = new Vector2 (2.0f, 1.0f);
			rightFrontMat.mainTextureOffset = new Vector2 (0.0f, 0.0f);
			rightRearMat.mainTextureScale = new Vector2 (1.0f, 1.0f);
			rightRearMat.mainTextureOffset = new Vector2 (0.75f, 0.0f);
		}
	}

	// Front+rear: 360° mono (still/video)
	public void SetMono360() {
		leftFrontMat.mainTextureScale = new Vector2 (1.0f, 1.0f);
		leftFrontMat.mainTextureOffset = new Vector2 (0.75f, 0.0f);
		leftRearMat.mainTextureScale = new Vector2 (1.0f, 1.0f);
		leftRearMat.mainTextureOffset = new Vector2 (0.75f, 0.0f);
		if(rightFrontMat != null && rightRearMat != null) {
			rightFrontMat.mainTextureScale = new Vector2 (1.0f, 1.0f);
			rightFrontMat.mainTextureOffset = new Vector2 (0.75f, 0.0f);
			rightRearMat.mainTextureScale = new Vector2 (1.0f, 1.0f);
			rightRearMat.mainTextureOffset = new Vector2 (0.75f, 0.0f);
		}
	}

	// One 4k*4k texture, over-under stereo, 360°
	public void SetStereo360() {
		leftFrontMat.mainTextureScale = new Vector2 (1.0f, 0.5f);
		leftFrontMat.mainTextureOffset = new Vector2 (0.75f, 0.5f);
		leftRearMat.mainTextureScale = new Vector2 (1.0f, 0.5f);
		leftRearMat.mainTextureOffset = new Vector2 (0.75f, 0.5f);
		if(rightFrontMat != null && rightRearMat != null) {
			rightFrontMat.mainTextureScale = new Vector2 (1.0f, 0.5f);
			rightFrontMat.mainTextureOffset = new Vector2 (0.75f, 0.0f);
			rightRearMat.mainTextureScale = new Vector2 (1.0f, 0.5f);
			rightRearMat.mainTextureOffset = new Vector2 (0.75f, 0.0f);
		}
	}

	// One 180° texture for front (side-by-side stereo), part of one 360° texture for the rear
	public void SetStereo180() {
		leftFrontMat.mainTextureScale = new Vector2 (1.0f, 1.0f);
		leftFrontMat.mainTextureOffset = new Vector2 (0.5f, 0.0f);
		leftRearMat.mainTextureScale = new Vector2 (1.0f, 0.5f);
		leftRearMat.mainTextureOffset = new Vector2 (0.75f, 0.5f);
		//		rightFrontMat.mainTextureOffset = new Vector2 (0.5f, 0.0f);
		if(rightFrontMat != null && rightRearMat != null) {
			rightFrontMat.mainTextureScale = new Vector2 (1.0f, 1.0f);
			rightFrontMat.mainTextureOffset = new Vector2 (0.0f, 0.0f);
			rightRearMat.mainTextureScale = new Vector2 (1.0f, 0.5f);
			rightRearMat.mainTextureOffset = new Vector2 (0.75f, 0.0f);
		}
	}


	public string VideoFilePath(string filename) {
		return videoDir + filename;
	}

	public string FilePath(string filename) {
		return fileDir + filename;
	}


	//
	// MediaPlayer Handling
	//

//	public void PrepareVideo(string filename) {		
//		perfBoost ();
//		string filePath = videoDir  + filename;
//		if (mediaPlayer.m_strFileName != filePath) {
//			Debug.Log ("VPM: PrepareVideo " + filePath + " ("+Time.time*1000+")");
//			mediaPlayerFileName = filePath;
//			if(mediaPlayer.GetCurrentState () == MediaPlayerCtrl.MEDIAPLAYER_STATE.STOPPED) {
//				Debug.Log ("VPM: MediaPlayerState is STOPPED, let's give it some extra time to settle");
//				Invoke("LoadVideo", 0.099f);
//			} else {
//				Invoke("LoadVideo", 0.066f);
//			}
//		} else {
//			Debug.Log ("VPM: Video already loaded (" + filePath + ") ("+Time.time*1000+")");
//		}
//	}

	public void PlayVideo(string filename, string rearFilename, string tempFilename=null) {
		perfBoost ();
		leftRearMat.mainTexture = frameKeeper.TextureForFile (rearFilename);
		if(rightRearMat != null) {
			rightRearMat.mainTexture = leftRearMat.mainTexture;
		}
		if(tempFilename!=null) {
			leftFrontMat.mainTexture = frameKeeper.TextureForFile (tempFilename);
			leftFrontMat.mainTexture.wrapMode = TextureWrapMode.Repeat;
			hasTemporaryVideoTexture = true;
			if(rightFrontMat != null) {
				rightFrontMat.mainTexture = leftFrontMat.mainTexture;
				rightFrontMat.mainTexture.wrapMode = TextureWrapMode.Repeat;
			}
		} else {
			leftFrontMat.mainTexture = leftRearMat.mainTexture;
			if(rightFrontMat != null) {
				rightFrontMat.mainTexture = leftFrontMat.mainTexture;
			}
		}
//		leftRearMat.mainTexture = rightRearMat.mainTexture = frameKeeper.TextureForFile (rearFilename);
		string filePath = videoDir + filename;
		if (mediaPlayer.m_strFileName != filePath) {
			Debug.Log ("VPM: PlayVideo " + filePath + " ("+Time.time*1000+")");
			targetMediaPlayerFileName = filePath;
			if(mediaPlayer.GetCurrentState () == MediaPlayerCtrl.MEDIAPLAYER_STATE.STOPPED) {
				Debug.Log ("VPM: MediaPlayerState is STOPPED, let's give it some extra time to settle");
				Invoke("LoadVideo", 0.099f);
				Invoke("PlayVideo", 0.198f);
			} else {
				Invoke("LoadVideo", 0.033f);
				Invoke("PlayVideo", 0.132f);
			}
		} else {
			Debug.Log ("VPM: Video already loaded (" + filePath + ") ("+Time.time*1000+")");
			if (mediaPlayer.GetCurrentState () == MediaPlayerCtrl.MEDIAPLAYER_STATE.PAUSED || mediaPlayer.GetCurrentState () == MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING) {
				Debug.Log ("VPM: Has been playing, restart, mediaPlayerSeekTo(0)");
				mediaPlayer.SeekTo(0);
			}
		}
//		videoShouldPlay = true;
	}

	public void LoadVideo() {
		if(Time.time < lastMediaPlayerLoad + 0.099f) {
			Debug.LogError("LoadVideo rejected, last mediaPlayer.Load was "+lastMediaPlayerLoad+ ") ("+Time.time*1000+")");
			return;
		}
		Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
		Debug.Log ("VPM: LoadVideo, mediaPlayer.Load(" + targetMediaPlayerFileName + ") ("+Time.time*1000+")");
//		if(File.Exists(targetMediaPlayerFileName)) {
			long start = System.DateTime.Now.Ticks;
// Local files:
			mediaPlayer.Load ("file://" + targetMediaPlayerFileName);
// StreamingAssets:
//			mediaPlayer.Load (targetMediaPlayerFileName);
			currentMediaPlayerFileName = targetMediaPlayerFileName;
			long diff = (System.DateTime.Now.Ticks - start) / 10000;
			Debug.Log ("VPM: "+diff+" ms");
			hasVideoTexture = false;
		//		} else {
//			Debug.LogError ("VPM: LoadVideo, File not found! ("+Time.time*1000+")");
//			ShowErrorMessage(targetMediaPlayerFileName + " not found!");
//		}
		lastMediaPlayerLoad = Time.time;
	}

	public void StopVideo() {
		Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
		Debug.Log ("VPM: StopVideo, mediaPlayer.Stop() ("+Time.time*1000+")");
		long start = System.DateTime.Now.Ticks;
		videoShouldPlay = false;
		videoPaused = false;
		mediaPlayer.Stop ();
//		mediaPlayer.UnLoad ();
		long diff = (System.DateTime.Now.Ticks - start) / 10000;
		Debug.Log ("VPM: "+diff+" ms");
		hasVideoTexture = false;
	}

	public void SetVideoTextures() {
		Texture2D videoTexture = mediaPlayer.GetVideoTexture ();
		if (videoTexture != null) {
			skippedFrames++;
			if(skippedFrames > 2 && mediaPlayer.GetSeekPosition() > 66) {
				leftFrontMat.mainTexture = videoTexture;
				leftFrontMat.mainTexture.wrapMode = TextureWrapMode.Repeat;
				if(rightFrontMat != null) {
					rightFrontMat.mainTexture = videoTexture;
					rightFrontMat.mainTexture.wrapMode = TextureWrapMode.Repeat;
				}
				if (!vp.is180) { // Rear video!
					leftRearMat.mainTexture = videoTexture;
					leftRearMat.mainTexture.wrapMode = TextureWrapMode.Repeat;
					if(rightRearMat != null) {
						rightRearMat.mainTexture = videoTexture;
						rightRearMat.mainTexture.wrapMode = TextureWrapMode.Repeat;
					}
				}
				hasTemporaryVideoTexture = false;
				hasVideoTexture = true;
				skippedFrames = 0;
				mediaPlayerErrorCount = 0;
				ShowErrorMessage("");
				Debug.Log("VPM: SetVideoTexture succeeded");
			} else {
				Debug.Log ("VPM: SetVideoTextures skipped "+skippedFrames);
				hasVideoTexture = false;
			}
		} else {
			Debug.Log("VPM: SetVideoTexture did not get videoTexture");
			hasVideoTexture = false;
		}
	}
	public void PauseVideo() {
		Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
		Debug.Log ("VPM: PauseVideo, mediaPlayer.Pause() ("+Time.time*1000+")");
		mediaPlayer.Pause ();
		videoPaused = true;
		videoShouldPlay = false;
		setPerfLow();
	}
	
	public void PlayVideo() {
		Debug.Log ("VPM: PlayVideo, videoShouldPlay => true ("+Time.time*1000+")");
		//		Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
		//		Debug.Log ("VPM: PlayVideo, mediaPlayer.Play() ("+Time.time*1000+")");
		//		mediaPlayer.Play ();
		videoPaused = false;
		hasVideoTexture = false;
		videoShouldPlay = true;
		setPerfMid();
	}
	
	public bool isPlayingVideo() {
		return (mediaPlayer.GetCurrentState () == MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING);
	}

	public int VideoSeekPosition() {
		if(isPlayingVideo()) {
			return mediaPlayer.GetSeekPosition ();
		} else {
			return 0;
		}
	}

	//
	// Fade In, Fade out
	//

	private void FadeIn() {
		Debug.Log("VPM: FadeIn");
		if (leftFade != null)
			leftFade.StartFadeIn (fadeInTime);
		if (rightFade != null)
			rightFade.StartFadeIn (fadeInTime);
		isFadedOut = false;
	}

	private void FadeOut() {
		Debug.Log("VPM: FadeOut");
		if (leftFade != null)
			leftFade.StartFadeOut (fadeOutTime);
		if (rightFade != null)
			rightFade.StartFadeOut (fadeOutTime);
		isFadedOut = true;
	}

	private void FadeOutNow() {
		Debug.Log("VPM: FadeOutNow");
		if (leftFade != null)
			leftFade.StartFadeOut (0.0f);
		if (rightFade != null)
			rightFade.StartFadeOut (0.0f);
		isFadedOut = true;
	}
	

	public void Update() {
        // Debug.Log("VPM: MediaPlayer state: "+mediaPlayer.GetCurrentState ()+", videoShouldPlay: "+videoShouldPlay+", videoPaused: "+videoPaused+" ("+Time.time*1000+")");

      //  ShowPicture("Game_mode_temp.jpg", "Game_mode_still.jpg");


        if (videoPaused && mediaPlayer.isActiveAndEnabled && (leftFrontMat.mainTexture == null || (rightFrontMat != null && rightFrontMat.mainTexture == null))) {
			Debug.Log ("VPM: videoPaused, left||right.mainTexture==null ("+Time.time*1000+")");
			SetVideoTextures();
		}

		if (videoPaused && isFadedOut && mediaPlayer.GetCurrentState () == MediaPlayerCtrl.MEDIAPLAYER_STATE.PAUSED) {
			Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
			Debug.Log ("VPM: Video Paused, Fade In ("+Time.time*1000+")");
			SetVideoTextures ();
			FadeIn();
		}
		
		if(videoShouldPlay) {
			if (mediaPlayer.isActiveAndEnabled && (leftFrontMat.mainTexture == null || (rightFrontMat != null && rightFrontMat.mainTexture == null))) {
				Debug.Log ("VPM: left||right.mainTexture==null ("+Time.time*1000+")");
				SetVideoTextures();
			}
			
			// Has to be done before FadeIn started, otherwise FadeIn is pausing in the middle
			//		if (prepareVideo && mediaPlayer.GetCurrentState () == MediaPlayerCtrl.MEDIAPLAYER_STATE.READY) {
			//			Debug.Log ("VPM: Video Ready => Play, but don't set Textures ("+Time.time*1000+")");
			//			mediaPlayer.Play ();
			//			prepareVideo=false;
			//			setPerf (0,1); // Should be Delayed
			//		}
			
			if (mediaPlayer.GetCurrentState () == MediaPlayerCtrl.MEDIAPLAYER_STATE.READY) {
				Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
				Debug.Log ("VPM: Video Ready => mediaPlayer.Play ("+Time.time*1000+")");
				mediaPlayer.Play ();
				videoPaused = false;
				if(!hasTemporaryVideoTexture)
					SetVideoTextures ();
			}
			
			if (!isFadedOut && mediaPlayer.GetCurrentState () == MediaPlayerCtrl.MEDIAPLAYER_STATE.NOT_READY && !hasTemporaryVideoTexture) {
				Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
				Debug.Log ("VPM: MediaPlayer NOT_READY, Fade Out ("+Time.time*1000+")");
				FadeOutNow();
			}
			
			if (mediaPlayer.GetCurrentState () == MediaPlayerCtrl.MEDIAPLAYER_STATE.STOPPED) {
				Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
				Debug.Log ("VPM: Video Stopped => mediaPlayer.Play ("+Time.time*1000+")");
				mediaPlayer.Play ();
				videoPaused = false;
				SetVideoTextures ();
			}
			
			if (videoShouldPlay && mediaPlayer.GetCurrentState () == MediaPlayerCtrl.MEDIAPLAYER_STATE.PAUSED) {
				Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
				Debug.Log ("VPM: Video Paused => mediaPlayer.Play ("+Time.time*1000+")");
				mediaPlayer.Play ();
				videoPaused = false;
				SetVideoTextures ();
			}
			
			if (mediaPlayer.GetCurrentState () == MediaPlayerCtrl.MEDIAPLAYER_STATE.ERROR) {
				mediaPlayerErrorCount++;
				Debug.Log ("VPM: MEDIAPLAYER_STATE.ERROR #"+mediaPlayerErrorCount+" ("+Time.time*1000+")");
				if(mediaPlayerErrorCount == 30) {
					Debug.Log ("VPM: MediaPlayer reported ERROR 30 times, trying mediaPlayer.Load(...) ("+Time.time*1000+")");
					LoadVideo();
				}
				if(mediaPlayerErrorCount == 60) {
					Debug.Log ("VPM: MediaPlayer reported ERROR 60 times, trying mediaPlayer.Stop() ("+Time.time*1000+")");
					mediaPlayer.Stop();
				}
				if(mediaPlayerErrorCount >= 120) {
					Debug.Log ("VPM: MediaPlayer reported ERROR 120 times, QUIT ("+Time.time*1000+")");
					// TODO: Display Error Message
					ShowErrorMessage("MediaPlayer ERROR\n\nPlease close all apps and start again.");
				}
			}
			if (mediaPlayer.GetCurrentState () == MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING) {
				if(mediaPlayerErrorCount > 0) {
					Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
					Debug.Log ("VPM: Resetting error counter ("+Time.time*1000+")");
					mediaPlayerErrorCount=0;
				}
				if (isFadedOut && hasVideoTexture) {
					Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
					Debug.Log ("VPM: HasVideoTexture, Fade In ("+Time.time*1000+")");
					//SetVideoTextures ();
					FadeIn();
				}
				if (hasTemporaryVideoTexture) {
					Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
					Debug.Log ("VPM: Video Playing, Replace temporary texture ("+Time.time*1000+")");
					SetVideoTextures ();
				}
				if (!hasVideoTexture) {
					Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
					Debug.Log ("VPM: Video Playing && !hasVideoTexture ("+Time.time*1000+")");
					SetVideoTextures ();
				}
				if (vp.loopSeekTo > 0) {
					if(videoDuration == 0) 
						videoDuration = mediaPlayer.GetDuration();
					if(mediaPlayer.GetSeekPosition() > videoDuration - 500) {
						Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
						Debug.Log ("VPM: Restarting video loop at "+vp.loopSeekTo);
						mediaPlayer.SeekTo(vp.loopSeekTo);
					}
				}
				if (vp.playOnceAndReturn) {
					if(videoDuration == 0) 
						videoDuration = mediaPlayer.GetDuration();
					if(mediaPlayer.GetSeekPosition() > videoDuration - 500) {
						Debug.Log ("VPM: MediaPlayerState is " + mediaPlayer.GetCurrentState ());
						Debug.Log ("VPM: Video reached end, returning to previous viewpoint");
						PreviousViewpoint ();
					}
				}
			}
		}

		if (VraSettings.instance.isHeadset) {
			if (Input.GetKeyDown (KeyCode.Escape)) {
				Debug.Log ("VPM: Escape! ("+Time.time*1000+")");
				Reset ();
			}
		}

		if (swipeInput) {
			// Swipe detection
			if (Input.GetMouseButton (0)) {
				if (buttonDown) {
					float h = Input.GetAxis ("Mouse X");
					float v = Input.GetAxis ("Mouse Y");
					direction += new Vector2 (h * Time.deltaTime, v * Time.deltaTime);
				} else {
					buttonDown = true;
					direction = new Vector2 (0.0f, 0.0f);
					audioSource.Stop(); // Up the shut fuck, you must!
				}
			} else if (buttonDown) { // Button just lifted
				float length = Mathf.Sqrt (Mathf.Pow (direction.x, 2) + Mathf.Pow (direction.y, 2));
				float angle = Mathf.Atan (direction.y/direction.x) * (180.0f/Mathf.PI);
				Debug.Log ("VPM: Swipe direction:" + direction.x + " " + direction.y + " length: "+length + " angle: "+angle);
				buttonDown = false;
				if (length > 0.5f) {
					// Swipe long enough
					if (Mathf.Abs(angle) < 30) {
						// Horizontal swipe
						if (direction.x < 0.0f) { // x is inversed on the Gear VR touchpad
							Debug.Log ("VPM: Swipe detected: FORTH ("+Time.time*1000+")");
							NextViewpoint ();
						} else if (direction.x > 0.0f) { 
							Debug.Log ("VPM: Swipe detected: BACK ("+Time.time*1000+")");
							PreviousViewpoint ();
						}
					}
				}
			}
		}

	}

	public void LoadPrefs() {
		switch (PlayerPrefs.GetString ("lang", "DE")) {
		case "DE":	
			lang = LANGUAGE.GERMAN;
			break;
		case "EN":
			lang = LANGUAGE.ENGLISH;
			break;
		default:
			lang = LANGUAGE.GERMAN;
			break;
		}
	}
	
//	public void SavePrefs() {
//		switch(lang) {
//		case LANGUAGE.GERMAN: 
//			PlayerPrefs.SetString("lang","DE");
//			break;
//		case LANGUAGE.ENGLISH: 
//			PlayerPrefs.SetString("lang","EN");
//			break;
//		}
//		PlayerPrefs.Save ();
//	}

	#if (UNITY_ANDROID && !UNITY_EDITOR)
	[DllImport("OculusPlugin")]
	// Set the fixed CPU clock level.
	private static extern void OVR_VrModeParms_SetCpuLevel(int cpuLevel);
	
	[DllImport("OculusPlugin")]
	// Set the fixed GPU clock level.
	private static extern void OVR_VrModeParms_SetGpuLevel(int gpuLevel);
	#endif

	private void setPerf(int cpuLevel, int gpuLevel) {
		Debug.Log ("VPM: Setting CPU/GPU Clock Level (" + cpuLevel + "," + gpuLevel + ")");
		#if (UNITY_ANDROID && !UNITY_EDITOR)
		if(VraSettings.instance.isGearVR) {
			OVR_VrModeParms_SetCpuLevel(cpuLevel);
			OVR_VrModeParms_SetGpuLevel(gpuLevel);
			OVRPluginEvent.Issue(RenderEventType.ResetVrModeParms); //Todo: Wird das noch gebraucht...?
		}
		#endif
	}

	private void setPerfLow() {
		Debug.Log ("VPM: Clock setPerfLow");
		setPerf (1, 1);
	}

	private void setPerfMid() {
		Debug.Log ("VPM: Clock setPerfMid");
		setPerf (2, 2);
	}

	private void setPerfHigh() {
		Debug.Log ("VPM: Clock setPerfHigh");
		setPerf (3, 3);
	}

	private void perfBoost() {
		Debug.Log ("VPM: Clock perfBoost");
		setPerfHigh();
		Invoke ("setPerfMid", 1.0f);
	}

	public void ShowErrorMessage(string message) {
		if(errorMessage != null)
			errorMessage.text = message;
		if(message != null && message.Length > 0) {
			errorMessageCanvas.enabled = true;
			ShowDefaultPicture();
		} else {
			errorMessageCanvas.enabled = false;
		}
	}

    //public void DiamondCreation()
    //{
    //    //Diamond.transform.SetParent(this.gameObject.transform);

    //    //Diamond = (GameObject)Network.Instantiate(ShortLaserPrefab, Vector3.zero, Quaternion.identity, 0);

    //    ShortLaser = (GameObject)Network.Instantiate(ShortLaserPrefab, Vector3.zero, Quaternion.identity, 0);

    //    //Diamond.GetComponent<DiamondGame>().SetPosition(new Vector3(10, 3, 10.0f), new Vector3(-90.0f, 0.0f, 0.0f));

    //    //SetPosition(new Vector3(0.0f, 0.0f, 10.0f));

    //    //Diamond = Instantiate(DiamondPrefab, Vector3.zero, Quaternion.identity) as GameObject;
    //    //Diamond.transform.position = new Vector3(10, 3, 10.0f);
    //    //Diamond.transform.Rotate(-90.0f, 0.0f, 0.0f);
    //    //GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
    //    //if (nws != null)
    //    //{
    //    //    nws.GetComponent<NetworkSync>().DiamondCreationTablet();
    //    //}
    //}
}