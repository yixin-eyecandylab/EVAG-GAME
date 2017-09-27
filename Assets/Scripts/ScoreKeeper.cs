using UnityEngine;
using System.Collections;

public class ScoreKeeper: MonoBehaviour {
	public GameObject bottlePrefab;
	public Material goldMaterial;
	public Material glassMaterial;
	public Material redBottleMaterial;
	public Transform bottleStart;
	public GameObject heartPrefab;
	public Transform heartStart;
	public TMPro.TextMeshProUGUI mainScoreLabel, foundScoreLabel, missedScoreLabel;
	public Canvas finalScoreCanvas;
	public TMPro.TextMeshPro messageLabel;
	public int defectsFound, defectsMissed, validPassed, validMarked; // Total, not only in this level
	public int bottleStatesFound = 0; // 32 Bit field, 1 found, 0 whatever
	public int bottleStatesMissed = 0; // 32 Bit field, 1 missed, 0 whatever
	public AudioClip defectFoundClip, defectMissedClip, validPassedClip, validMarkedClip, messageBeepClip;
	public GameObject smileyJoyPrefab, smileyNeutralPrefab, smileyFrownPrefab;
	public GameObject smileyJoy, smileyNeutral, smileyFrown;
	public GameObject finalScoreSmileys;
	public GameController gameController;
	public int defectsFoundInThisLevel = 0;
	public int defectsMissedInThisLevel = 0;

	private float offset = 0.45f;
	private int bottleCount = 10;
	private static int MAX_BOTTLES = 20;
	private float heartOffset = 0.55f;
	private static int MAX_LIVES = 3;
	public int lives = MAX_LIVES;
	private AudioSource audioSource;
	private GameObject[] bottles = new GameObject[MAX_BOTTLES];
	private GameObject[] hearts = new GameObject[MAX_LIVES];
	private int nextBottle = 0;

	// Use this for initialization
	void Start () {
		audioSource = gameObject.GetComponent<AudioSource> ();
		SyncScore ();
		ShowBottles ();
		HideMessage ();
		finalScoreCanvas.enabled = false;
		finalScoreSmileys.SetActive(false);
		//ShowFinalScore ();
		smileyJoy = GameObject.Instantiate(smileyJoyPrefab);
		smileyJoy.transform.SetParent (gameObject.transform);
		smileyNeutral = GameObject.Instantiate(smileyNeutralPrefab);
		smileyNeutral.transform.SetParent (gameObject.transform);
		smileyFrown = GameObject.Instantiate(smileyFrownPrefab);
		smileyFrown.transform.SetParent (gameObject.transform);
		HideSmiley();
	}

	// Update is called once per frame
	void Update () {
	
	}

	// And Reset
	public void StartNewLevel(int bottleCount) {
		this.bottleCount = bottleCount; // Defects, actually. But that's == bottles shown
		nextBottle = 0;
		//lives = MAX_LIVES; // Set twice, by SetScore also....
		defectsFoundInThisLevel=0;
		defectsMissedInThisLevel=0;
		bottleStatesFound=0;
		bottleStatesMissed=0;
		ShowBottles();
	}

	public void SetScore(int defectsFound, int defectsMissed, int validPassed, int validMarked, int bottleStatesFound, int bottleStatesMissed, int lives) {
		this.defectsFound = defectsFound;
		this.defectsMissed = defectsMissed;
		this.validPassed = validPassed;
		this.validMarked = validMarked;
		this.bottleStatesFound = bottleStatesFound;
		this.bottleStatesMissed = bottleStatesMissed;
		this.lives = lives;
		ShowBottles();
		SyncScore ();
	}

    public void DefectFound() {
		Debug.Log ("ScoreKeeper: DefectFound");
		defectsFound++;
		defectsFoundInThisLevel++;
		audioSource.PlayOneShot(defectFoundClip);
		bottleStatesFound |= (1 << nextBottle);
		nextBottle++;
		SyncScore();
		Invoke ("ShowSmileyJoy", 0.35f);
		Invoke ("ShowBottles",0.5f);
		ShowBottles();
		if(gameController.currentLevel==0) {
			gameController.tutorialMessage.SetActive(false);
		}
	}

	public void DefectMissed() {
		Debug.Log ("ScoreKeeper: DefectMissed"); 
		defectsMissed++;
		defectsMissedInThisLevel++;
		lives--;
		audioSource.PlayOneShot(defectMissedClip);
		SyncScore();
		bottleStatesMissed |= (1 << nextBottle);
		nextBottle++;
		ShowSmileyFrown();
		if(lives < 0) {
			gameController.GameOver();
		}
		ShowBottles();
	}

	public void DelayedGameOver() {
		gameController.GameOver();
	}

	public void ValidPassed() {
//		validPassed++;
//		audioSource.PlayOneShot(validPassedClip);
//		ShowScore();
	}

	public void ValidMarked() {
		validMarked++;
		lives--;
		audioSource.PlayOneShot(validMarkedClip);
		//		ShowMessage("Zuviel markiert!");
		if(gameController.currentLevel==0) {
			gameController.tutorialMessage.SetActive(false);
		}
		if(lives < 0) {
			Invoke("DelayedGameOver", 0.2f);
			Invoke ("ShowBottles",0.2f);
		} else {
			Invoke ("ShowSmileyFrown", 0.2f);
			Invoke ("ShowBottles",0.5f);
		}
	}

	public void ShowSmileyJoy() {
		HideSmiley();
		smileyJoy.SetActive(true);
		Invoke ("HideSmiley", 0.8f);
		if(Network.isServer) {
			GameObject gs = GameObject.Find("GameSync");
			if (gs != null) {
				gs.GetComponent<GameSync> ().ShowSmileyJoy();
			}
		}
	}

	public void ShowSmileyNeutral() {
		HideSmiley();
		smileyNeutral.SetActive(true);
		Invoke ("HideSmiley", 0.8f);
		if(Network.isServer) {
			GameObject gs = GameObject.Find("GameSync");
			if (gs != null) {
				gs.GetComponent<GameSync> ().ShowSmileyNeutral();
			}
		}
	}
	
	public void ShowSmileyFrown() {
		HideSmiley();
		smileyFrown.SetActive(true);
		Invoke ("HideSmiley", 0.8f);
		if(Network.isServer) {
			GameObject gs = GameObject.Find("GameSync");
			if (gs != null) {
				gs.GetComponent<GameSync> ().ShowSmileyFrown();
			}
		}
	}

	private void HideSmiley() {
		smileyJoy.SetActive(false);
		smileyNeutral.SetActive(false);
		smileyFrown.SetActive(false);
	}
	
	public void ShowMessage(string message, float duration = 1.0f) {
		Debug.Log("ScoreKeeper ShowMessage("+message+")");
		CancelInvoke("HideMessage");
		messageLabel.text = message;
		Invoke("HideMessage", duration);
		if(Network.isServer) {
			GameObject gs = GameObject.Find("GameSync");
			if (gs != null) {
				gs.GetComponent<GameSync> ().ShowMessage (message, duration);
			}
		}
	}

	public void HideMessage() {
		messageLabel.text = "";
	}

	public int Score() {
		int score = defectsFound - defectsMissed - validMarked;
		if(score < 0)
			score = 0;
		return score;
	}

	public void SyncScore() {
//		scoreLabel.text = "Score: " + Score ();
		if(Network.isServer) {
			GameObject gs = GameObject.Find("GameSync");
			if (gs != null) {
				gs.GetComponent<GameSync> ().SetScore (defectsFound, defectsMissed, validPassed, validMarked, bottleStatesFound, bottleStatesMissed, lives);
			}
		}
	}

	public void ShowFinalScore() {
		HideMessage ();
		finalScoreCanvas.enabled = true;
		int score = Score ();
		mainScoreLabel.text = "" + score;
		foundScoreLabel.text = "" + defectsFound;
		missedScoreLabel.text = "" + (defectsMissed + validMarked);
		//finalScoreSmileys.SetActive(true);
		//// Do we have a winner?
		//if(VraSettings.instance.isTablet) {
		//	GameObject obj = GameObject.Find("HighScoreKeeper");
		//	if (obj != null) {
		//		HighScoreKeeper hsk = obj.GetComponent<HighScoreKeeper>();
		//		if(hsk.isNewHighScore(score)) {
		//			hsk.NewHighScore(score);
		//		}
		//	}
		//}
	}

	public void HideFinalScore() {
		HideMessage ();
		finalScoreCanvas.enabled = false;
		mainScoreLabel.text = "";
		foundScoreLabel.text = "";
		missedScoreLabel.text = "";
		finalScoreSmileys.SetActive(false);
	}


	// Show Bottles and Hearts
	public void ShowBottles() {
		Debug.Log ("ScoreKeeper: ShowBottles, bottleStatesFound: "+bottleStatesFound+", bottleStatesMissed: "+bottleStatesMissed+", nextBottle: "+nextBottle+", lives: "+lives);
		for (int n=0; n<bottleCount; n++) {
			if(bottles[n]==null) {
				bottles[n] = GameObject.Instantiate (bottlePrefab);
				bottles[n].transform.SetParent (bottleStart, false);
				bottles[n].transform.localPosition = new Vector3 (n * offset, 0.0f, 0.0f);
			}
			bottles[n].SetActive (true);
			if((bottleStatesFound & (1 << n)) > 0) {
				bottles[n].GetComponent<Renderer>().material = goldMaterial;
			} else if((bottleStatesMissed & (1 << n)) > 0) {
				bottles[n].GetComponent<Renderer>().material = redBottleMaterial;
			} else {
				bottles[n].GetComponent<Renderer>().material = glassMaterial;
			}
		}
		for (int n=bottleCount; n<MAX_BOTTLES; n++) {
			if(bottles[n]!=null) {
				bottles [n].SetActive (false);
			}
		}
		for (int n=0; n < MAX_LIVES; n++) {
			if(hearts[n]==null) {
				hearts[n] = GameObject.Instantiate (heartPrefab);
				hearts[n].transform.SetParent (heartStart, false);
				hearts[n].transform.localPosition = new Vector3 (-1.0f * n * heartOffset, 0.0f, 0.0f);
			}
			if(n<lives) {
				hearts[n].SetActive (true);
			} else {
				hearts[n].SetActive (false);
			}		
		}
		//		for(int n=0; n<3; n++) {
//			if(n < (3 - defectsMissed) ) 
//				bottles[n].SetActive(true);
//			else
//				bottles[n].SetActive(false);
//		}
	}

}
