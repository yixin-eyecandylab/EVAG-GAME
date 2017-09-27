using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LEDController : MonoBehaviour {
	public GameObject LEDPrefab;
	public Vector3 offset = new Vector3(0.00635f,0.0f,0.0f); // 1/8 inch
	public Vector3 offsetY = new Vector3(0.0f,0.01f,0.0f);
	public Vector3 stretchX = new Vector3(0.0f,0.0f,0.0f);
	public float scaleX = 0.5f;
	public int count = 56; // The real bar has 56 LEDs, but not all of them might be visible.
	public Color colorRed = Color.red;
	public Color colorBlue = Color.blue;
	public Color colorGreen = Color.green;
	public Color colorWhite = Color.white;
	public Color colorYellow = Color.yellow;
	public ScoreKeeper scoreKeeper;
	public GameController gameController;

	public float containerSpeed = 1.0f; // Containers per Second
	public int containerOffset = 3; // LED offset

	private List<LED> LEDs = new List<LED>();
	private MediaPlayerCtrl mediaPlayer;
	public float rotateDelay = 0.125f;  // 8 LEDs/second

	public int position = 0;

	enum CONTAINER_STATE { UNKNOWN,	GOOD, BAD }
	private Color[] state_colors;

//	private List<CONTAINER_STATE> statelist = new List<CONTAINER_STATE>();

	public int[] containerstates;
	private int[] ledstates = new int[500];  // Keeps the list of logical states

	// Use this for initialization
	void Awake () {
		state_colors = new Color[3];
		state_colors[0] = colorBlue;
		state_colors[1] = colorGreen;
		state_colors[2] = colorRed;
	}

	void Start () {
		// Build LED strip from prefabs
		for(int i =0; i < count; i++) {
			float y = Mathf.Cos ((i - (count/2.0f))/count * 180.0f * Mathf.Deg2Rad) * 1.0f;
			float x = Mathf.Cos ((((i - (count/2.0f))/count * 180.0f) - 90.0f) * Mathf.Deg2Rad) * 1.0f;
//			Debug.Log ("i: "+i+", y: "+y+", x: "+x);
			GameObject ledObj = (GameObject) Instantiate(LEDPrefab, (offset * i) + (offsetY * y) + (stretchX * x), Quaternion.identity);
			Vector3 scale = ledObj.transform.localScale;
			ledObj.transform.localScale = new Vector3(scale.x*(1+(scaleX * y)),scale.y,scale.z);
			ledObj.transform.SetParent(this.transform, false);
			LED led = (LED) ledObj.GetComponent<LED>();
			LEDs.Add (led);
			led.Off ();
		}

	}

	// Sync LED position with video timecode
	public void SyncPosition() {
		int seekPos = mediaPlayer.GetSeekPosition();
		position = Mathf.FloorToInt(seekPos * 8.0f * containerSpeed / 1000.0f) - containerOffset;
		if(position > (ledstates.Length * 8 - count)) {
			position = ledstates.Length * 8 - count;
		}
	}

	// Update is called once per frame
	void LateUpdate () {
		if(mediaPlayer != null) {
			// position is the number of the first LED state to be visible on the strip
			// There are 8 LEDs per container, 5 lid, 3 spacers
			// Video speed will be 1 container per second
			// LEDs position is derived from Video seek position to keep it coordinated.
			if(gameController.GameIsRunning() && mediaPlayer.GetCurrentState () == MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING) {
				SyncPosition();
				if((VraSettings.instance.isHeadset) && gameController.GameIsRunning()) {
					// Set color green, if blue and container reaches the end
					if(position >= 0) {
						int containerNr = position/8;
						//Debug.Log ("LEDController containerNr: "+containerNr);
						if (ledstates[containerNr] == (int) CONTAINER_STATE.UNKNOWN) {
							if(containerstates[containerNr] != 0) {
								// BAD container was missed
								MarkContainer(containerNr, (int) CONTAINER_STATE.BAD);
								scoreKeeper.DefectMissed();
							} else {
								// GOOD container passed
								MarkContainer(containerNr, (int) CONTAINER_STATE.GOOD);
								scoreKeeper.ValidPassed();
							}
						}
					}
				}
				UpdateLEDs();
			}
		} else {
			GameObject mpObj = GameObject.Find ("MediaPlayerCtrl");
			if (mpObj!=null)
				mediaPlayer = mpObj.GetComponent<MediaPlayerCtrl> ();
		}
	}

	public void UpdateLEDs() {
		if(LEDs.Count == 0) // Not initialized, yet
			return;
		//Debug.Log ("LEDController UpdateLEDs");
		Color col;
		for(int n = 0; n < count; n++) {
			if((position+n) % 8 < 5) {
				int containerNr = (position+n)/8;
				if(containerNr >= 0) {
					if(containerNr < ledstates.Length)
						col = state_colors[ledstates[containerNr]];
					else
						col = state_colors[(int) CONTAINER_STATE.UNKNOWN];
					LEDs[count-1-n].SetColor(col);
				}
			} else {
				LEDs[count-1-n].Off();
			}
		}
	}

//	void Rotate() {
//		// Rotate LEDs
//		Color lastColor = LEDs[count-1].GetColor();
//		for(int i = count-1; i > 0; i--) {
//			LEDs[i].SetColor (LEDs[i-1].GetColor());
//		}
//		LEDs[0].SetColor (lastColor);
//	}

	// Mark Container at x-position as defective
	public void SetDefective(float markX) {
		if (!gameController.GameIsRunning ()) 
			return;
		float localX = (markX-transform.position.x)/transform.localScale.x/transform.parent.localScale.x;
		Debug.Log ("LEDController SetDefective: "+markX+", Local X: "+localX);
		int ledNR = (int) Mathf.Floor(localX/offset.x);
		if(ledNR > 0 && ledNR < count) {
			int containerNr = (position + (count-1-ledNR)) / 8;
			if(ledstates[containerNr] == 0) {
				ledstates[containerNr] = (int) CONTAINER_STATE.BAD;
				if(VraSettings.instance.isHeadset) {
					if(containerstates[containerNr] != 0) {
						// Marked a defective one
						scoreKeeper.DefectFound();
					} else if(containerstates[containerNr] == 0) {
						// Marked a good container
						scoreKeeper.ValidMarked();
					}
				}
				if(Network.isServer) {
					GameObject gs = GameObject.Find("GameSync");
					if (gs != null) {
						gs.GetComponent<GameSync> ().MarkContainer(containerNr,(int) CONTAINER_STATE.BAD);
					}
				}
			}
		}
	}

	public void MarkContainer(int containerNr, int state) {
		ledstates [containerNr] = state;
		if (Network.isServer) {
			GameObject gs = GameObject.Find ("GameSync");
			if (gs != null) {
				gs.GetComponent<GameSync> ().MarkContainer (containerNr, state);
			}
		}
	}

	public void Reset() {
		ledstates = new int[500];
		position = 0;
		UpdateLEDs();
	}

}
