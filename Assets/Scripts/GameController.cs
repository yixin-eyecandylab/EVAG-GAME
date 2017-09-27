using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class GameController : MonoBehaviour {
/*
	Define start game target (= LoadLevel-Target?)

	spheres front/rear
	backgroundimage
	mediaplayer
	game video list
	current game video
	score
	bottle status lists definition
	return to level
	return to viewpoint
*/

	public GameObject gameSyncPrefab;
	public LEDController ledController;
	public ScoreKeeper scoreKeeper;
	public AudioClip machineIdleClip, machineRunningClip;
	public AudioSource machineAudioSource, gameAudioSource;
	public GameObject retryTargetPrefab;
	public GameObject pauseTargetPrefab;
	public GameObject continueTargetPrefab;
	public GameObject tutorialMessage;
	public AudioClip gameWonClip, gameOverClip, levelUpClip, beepClip;
	public TMPro.TextMeshPro speedLabel;
	public int currentLevel = 99; // 0, 1, 2, 3

	private ViewpointManager viewpointManager;
	private int videoNr=9999;
	private string rearFilename = "Game_mode_still.jpg";
	private string tempFilename = "Game_mode_temp.jpg";
	private bool vpmSwipe;
	private GameObject retryTarget;
	private GameObject pauseTarget;
	private GameObject continueTarget;
	private bool invokedLevelUp = false;
	private int rewindVideoPos=9999;
	private bool waitForRewind = false;
	private HighScoreKeeper highScoreKeeper;

//	Original Sequences:
//	public static string[][] all_videos = new string[][]
//	{
//		new string[] {"Tutorial.mp4"}, // Level 0, Tutorial
//		new string[] {"Gaming_11.mp4", "Gaming_12.mp4", "Gaming_13.mp4"}, // Level 1
//		new string[] {"Gaming_21.mp4", "Gaming_22.mp4", "Gaming_23.mp4"}, // Level 2
//		new string[] {"Gaming_31.mp4", "Gaming_32.mp4", "Gaming_33.mp4"}  // Level 3
//	};
//	
//	// Defines each of the avaible sequences
//	private int[][] defect_sequences = new int[][]
//	{	
//		new int[] { 0,0 },
//		new int[] { 0,0,0,0,0,0,1 },
//		new int[] { 0,0,0,0,0,0,1 },
//		new int[] { 0,0,0,0,0,0,1 },
//		new int[] { 0,0,0,0,0,0,1,0,0,1 },
//		new int[] { 0,0,0,0,0,0,1,0,0,1 },
//		new int[] { 0,0,1,1,1,0,0 }, // Tutorial
//	};
//
//	Defines with sequences were used to make up a video
//	private int[][][] video_sequences =  new int[][][]
//	{
//		new int[][] { // Level 0 - Tutorial
//			new int[] { 6 }
//		},		
//		new int[][] { // Level 1
//			new int[] { 2,1,3,0,2,4,0,1,0,5,0 },
//			new int[] { 0,1,2,0,2,5,3,1,0,4,0 },
//			new int[] { 3,2,0,0,4,1,5,0,2,3,0 }
//		},
//		new int[][] { // Level 2
//			new int[] { 3,2,0,1,0,4,1,2,5,1,0,3,1,0 },
//			new int[] { 1,3,0,4,1,0,5,0,2,4,2,3,1,0 },
//			new int[] { 0,3,2,0,5,3,0,2,4,1,2,5,1,0 }
//		},
//		new int[][] { // Level 3
//			new int[] { 3,2,0,2,1,4,2,0,1,2,0,3,5,0,2,0,5,2,0,4,1,0 },
//			new int[] { 2,0,3,2,0,4,2,0,3,2,0,5,1,0,1,5,0,2,0,4,1,0 },
//			new int[] { 0,5,1,3,1,5,0,0,3,2,0,2,4,2,0,4,2,0,2,0,1,0 }
//		}
//	};

	// New Sequences, 20160622
	public static string[][] all_videos = new string[][]
	{
		new string[] {"Tutorial.mp4"}, // Level 0, Tutorial
		new string[] {"Gaming_Level01_1.mp4", "Gaming_Level01_1.mp4", "Gaming_Level01_1.mp4"}, // Level 1
		new string[] {"Gaming_Level02_1.mp4", "Gaming_Level02_1.mp4", "Gaming_Level02_1.mp4"}, // Level 2
		new string[] {"Gaming_Level03_1.mp4", "Gaming_Level03_1.mp4", "Gaming_Level03_1.mp4"}  // Level 3
	};
	
	// Defines each of the avaible sequences
	private int[][] defect_sequences = new int[][]
	{	
		new int[] { 0,0 },                 // #0
		new int[] { 0,0,0,0,0,0,1 },       // #1 Crack in glass
		new int[] { 0,0,0,0,0,0,1 },       // #2 Missing Cap
		new int[] { 0,0,0,0,0,0,1 },       // #3 Particle
		new int[] { 0 },                   // #4 No Defect
		new int[] { 0,0,0,0,0,0,1,0,0,1 }, // #5 Double Defect
		new int[] { 0,0,1,1,1,0,0 },       // #6 Tutorial
		new int[] { 0,0,0,0,0,0,1,0 },     // #7
		new int[] { 0,0,0,0,0,0,1,0,0,0 }, // #8
	};

	// Defines with sequences were used to make up a video
	private int[][][] video_sequences =  new int[][][]
	{
		new int[][] { // Level 0 - Tutorial
			new int[] { 6 }
		},		
		new int[][] { // Level 1
			new int[] { 1,2,3,4,5,1,2,3,4,5,0,0 },
			new int[] { 1,2,3,4,5,1,2,3,4,5,0,0 },
			new int[] { 1,2,3,4,5,1,2,3,4,5,0,0 },
		},
		new int[][] { // Level 2
			new int[] { 7,7,7,4,5,7,7,7,4,5,0,0 },
			new int[] { 7,7,7,4,5,7,7,7,4,5,0,0 },
			new int[] { 7,7,7,4,5,7,7,7,4,5,0,0 },
		},
		new int[][] { // Level 3
			new int[] { 7,7,7,4,5,7,7,7,4,5,0,0 },
			new int[] { 7,7,7,4,5,7,7,7,4,5,0,0 },
			new int[] { 7,7,7,4,5,7,7,7,4,5,0,0 },
		}
	};

//	private float startVideoTime;
	private int videoDuration = 0;

	public enum GAMESTATE {
		STARTING = 0, // Start phase, "Level, 3, 2, 1" messages
		RUNNING = 1, // Game in progress, Video running
		GAMEOVER = 2, // Game Over
		HIGHSCORE = 3, // Showing High Score List 
		PAUSED = 4 // Paused
	}

	private GAMESTATE gamestate;

	private float lastSync;
	private int defectsInThisLevel = 0;

	// States to sync:
	// LevelNr, VideoNr (chosen randomly on server)
	// Score (missed, found)
	// Level-Start-Sequence "Level <n>", "3", "2", "1"
	// gamestate
	// Video seek position

	// Use this for initialization
	void Start () {
		Debug.Log ("GameController starting up");
		viewpointManager = GameObject.Find ("ViewpointManager").GetComponent<ViewpointManager> ();
		if (viewpointManager == null) {
			Debug.LogError ("GameController: Can't find ViewpointManager!");
			return;
		}
		GameObject obj = GameObject.Find("HighScoreKeeper");
		if (obj != null) {
			highScoreKeeper = obj.GetComponent<HighScoreKeeper>();
		}

		// Disable Swipe Input in game mode
		vpmSwipe = viewpointManager.swipeInput;
		viewpointManager.swipeInput = false;

		retryTarget = GameObject.Instantiate(retryTargetPrefab);
		retryTarget.transform.SetParent(this.gameObject.transform);
		retryTarget.SetActive(false);
		pauseTarget = GameObject.Instantiate(pauseTargetPrefab);
		pauseTarget.transform.SetParent(this.gameObject.transform);
		pauseTarget.SetActive(false);
		continueTarget = GameObject.Instantiate(continueTargetPrefab);
		continueTarget.transform.SetParent(this.gameObject.transform);
		continueTarget.SetActive(false);
		if(viewpointManager.lang==ViewpointManager.LANGUAGE.GERMAN) {
			retryTarget.GetComponent<GazeTarget> ().SetLabel ("Nochmal spielen");
			tutorialMessage.GetComponent<TMPro.TextMeshPro>().text="Markiere alle\nfehlerhaften\nBehälter";
		} else if (viewpointManager.lang==ViewpointManager.LANGUAGE.ENGLISH) {
			retryTarget.GetComponent<GazeTarget> ().SetLabel ("Play again");
			tutorialMessage.GetComponent<TMPro.TextMeshPro>().text="Mark all\ndefective\ncontainers";
		}
		tutorialMessage.SetActive(false);

	//	CheckFiles();
		Debug.Log ("GameController start checkpoint 1");

		if (Network.isServer) {
			GameObject gsync = GameObject.Find ("GameSync");
			if (gsync != null) {
				Debug.Log ("GameController: Reusing existing GameSync object");
			} else {
				gsync = (GameObject)Network.Instantiate (gameSyncPrefab, Vector3.zero, Quaternion.identity, 0);
				gsync.name = "GameSync";
			}
		}
		Debug.Log ("GameController start checkpoint 2");
		if (VraSettings.instance.isHeadset) {
			scoreKeeper.SetScore (0,0,0,0,0,0,3);
			// Start Level 0!
			StartLevel(0);
		}
		Debug.Log ("GameController start complete");
	}

	public void OnDestroy() {
		GameObject gsync = GameObject.Find ("GameSync");
		if (gsync != null) {
			Debug.Log ("GameController: Network.Destroy(GameSync)");
			Network.Destroy(gsync);
		}
		viewpointManager.swipeInput = vpmSwipe;
	}

	public void CheckFiles() {
		string filePath;
		foreach(string video in all_videos.SelectMany(a=>a)) {
			Debug.Log ("GameController checking file "+video);
			filePath =  viewpointManager.VideoFilePath(video);
			if(!File.Exists(filePath)) {
				viewpointManager.ShowErrorMessage(filePath+" not found!");
			}
		}
		filePath =  viewpointManager.FilePath(tempFilename);
		if(!File.Exists(filePath)) {
			viewpointManager.ShowErrorMessage(filePath+" not found!");
		}
		filePath =  viewpointManager.FilePath(rearFilename);
		if(!File.Exists(filePath)) {
			viewpointManager.ShowErrorMessage(filePath+" not found!");
		}
	}

	// Update is called once per frame
	void Update () {
//		// Pause video shortly after it's ready and playing
//		if (startVideoTime == 0 && viewpointManager.isPlayingVideo()) {
//			startVideoTime = Time.time;
//		}
//		if(!gameRunning && !gameFinished && viewpointManager.isPlayingVideo() && Time.time > startVideoTime+0.1f ) {

		if(viewpointManager.isPlayingVideo()) {
			int seekPos = viewpointManager.VideoSeekPosition(); // ms
			if( seekPos < rewindVideoPos )
				waitForRewind = false;
			if(gamestate==GAMESTATE.STARTING && !waitForRewind && viewpointManager.hasVideoTexture ) { // Did video properly (re-)start?
				Debug.Log ("GameController: Video reached "+seekPos+" ms, Pause video ("+Time.time*1000+")");
				viewpointManager.PauseVideo();
				ledController.SyncPosition();
				ledController.UpdateLEDs();
			}
		}

		// Game Sync
		if(VraSettings.instance.isHeadset && Time.time > lastSync + 1.0f) {
			GameSyncHere();
		}

		// Level Up || Game Over!
		if(VraSettings.instance.isHeadset && gamestate==GAMESTATE.RUNNING  && viewpointManager.isPlayingVideo()) {
			if(videoDuration == 0) 
				videoDuration = viewpointManager.mediaPlayer.GetDuration();
//			if (currentLevel > 0 && viewpointManager.VideoSeekPosition() > videoDuration - 200 && !gameFinished && !invokedLevelUp) {
//				Debug.Log ("GameController: Video reached end, LevelUp! ("+Time.time*1000+")");
//				invokedLevelUp = true;
//				LevelUp();
//			}
			if((scoreKeeper.defectsMissedInThisLevel + scoreKeeper.defectsFoundInThisLevel) >= defectsInThisLevel && !invokedLevelUp ) {
				Invoke("LevelUp", 1.0f);
				invokedLevelUp = true;
			}
// Turbo-/Cheat mode for debugging:
//			if(scoreKeeper.defectsFoundInThisLevel > 0 && !invokedLevelUp  ) {
//				Invoke("LevelUp", 1.0f);
//				invokedLevelUp = true;
//			}
		}
	}

	public void LevelUp() {
		invokedLevelUp = false;
		if(currentLevel > 0 )
			gameAudioSource.PlayOneShot(levelUpClip);
		StartLevel(currentLevel+1);
	}

	public void StartLevel(int level) {
		Debug.Log ("GameController: Start Level "+level+" ("+Time.time*1000+")");
		if (level == 4) {
			GameOver ();
			return;
		}
		videoDuration = 0;
//		startVideoTime = 0.0f;
		gamestate = GAMESTATE.STARTING;
		currentLevel = level;
		ledController.Reset();
		retryTarget.SetActive(false);
		if (VraSettings.instance.isHeadset) {
			machineAudioSource.Stop ();
			GameObject gs = GameObject.Find ("GameSync");
			if (gs != null) {
				gs.GetComponent<GameSync> ().StartLevel(level);
			}
		}
		highScoreKeeper.ClosePanels();
		if(level == 0) {
			// Start Level 0 (Tutorial)
			SetVideo (0, 0);
			scoreKeeper.SetScore (0,0,0,0,0,0,3);
			gamestate = GAMESTATE.RUNNING; // Tutorial starts immediatly, with running video
			ledController.containerSpeed = 0.0f; 
			ledController.containerOffset = -2; 
			tutorialMessage.SetActive(true);
			speedLabel.text = "";
			if (VraSettings.instance.isHeadset) {
				machineAudioSource.clip = machineIdleClip;
				machineAudioSource.Play ();
				machineAudioSource.loop = true;
				Debug.Log ("GameController: Tutorial, start video ("+Time.time*1000+")");
				viewpointManager.PlayVideo();
			}
			Invoke("HideTutorialMessage", 4.0f);
		} else if(level == 1) {
			// Start Level 1
			tutorialMessage.SetActive(false);
			scoreKeeper.SetScore (0,0,0,0,0,0,3);
			SetVideo (level, Mathf.FloorToInt (Random.value * 2.999f)); 
			viewpointManager.ShowPicture( tempFilename, rearFilename); 
//			ledController.containerSpeed = 1.0f; 
//			ledController.containerOffset = 4; // Shifts the LEDs more to the left
			ledController.containerSpeed = 0.75f; //45/min.
			ledController.containerOffset = 4; // Shifts the LEDs more to the left
			speedLabel.text = "Level 1";
			if (VraSettings.instance.isHeadset) {
				scoreKeeper.ShowMessage ("Level 1",2.0f);
				Invoke ("MessageThree",2.0f);
			}
		} else if(level == 2) {
			// Start Level 2
			SetVideo (level, Mathf.FloorToInt (Random.value * 2.999f));
			viewpointManager.ShowPicture( tempFilename, rearFilename); 
//			ledController.containerSpeed = 2.0f; 
//			ledController.containerOffset = 6; 
			ledController.containerSpeed = 1.0f; //60/min.
			ledController.containerOffset = 4; 
			speedLabel.text = "Level 2";
			if (VraSettings.instance.isHeadset) {
				scoreKeeper.ShowMessage ("Level 2",2.0f);
				Invoke ("MessageThree",2.0f);
			}
		} else if (level == 3) {
			// Start Level 3
			SetVideo (level, Mathf.FloorToInt (Random.value * 2.999f));
			viewpointManager.ShowPicture( tempFilename, rearFilename); 
//			ledController.containerSpeed = 3.0f; 
//			ledController.containerOffset = 7; 
			ledController.containerSpeed = 1.25f; // 75/min.
			ledController.containerOffset = 4; 
			speedLabel.text = "Level 3";
			if (VraSettings.instance.isHeadset) {
				scoreKeeper.ShowMessage ("Level 3",2.0f);
				Invoke ("MessageThree",2.0f);
			}
		}
		scoreKeeper.StartNewLevel(defectsInThisLevel);
		GameSyncHere();
	}

	public void HideTutorialMessage() {
			tutorialMessage.SetActive(false);
	}

	public void GameOver() {
        GameSyncHere();
		Debug.Log ("GameController: Game Over!");
		viewpointManager.PauseVideo ();
		pauseTarget.SetActive(false);
		scoreKeeper.ShowFinalScore ();
		//		Destroy (ledController.gameObject);
		gamestate = GAMESTATE.GAMEOVER;
		machineAudioSource.Stop();
		if(!highScoreKeeper.isNewHighScore(scoreKeeper.Score())) {
			retryTarget.SetActive(true);
		}
		if(VraSettings.instance.isHeadset) {
			if(scoreKeeper.lives > 0) {
				Invoke("PlayGameWonClip",0.5f);
			} else {
				Invoke("PlayGameOverClip",0.5f);
			}
		}
		if (Network.isServer) { 
			GameObject gs = GameObject.Find ("GameSync");
			if (gs != null) {
				gs.GetComponent<GameSync> ().GameOver ();
			}
		}
		// This is not ideal. The Retry-Button should only disappear when there really is a Tablet connected
		if(Network.connections==null || Network.connections.Count() == 0) {
			retryTarget.SetActive(true);
		}
	}

	public void PlayGameOverClip() {
		gameAudioSource.PlayOneShot(gameOverClip);
	}

	public void PlayGameWonClip() {
		gameAudioSource.PlayOneShot(gameWonClip);
	}


	public void SetVideo(int level, int videoNr) {
		Debug.Log ("GameController: SetVideo ("+level+","+videoNr+")");
		this.videoNr = videoNr;
		videoDuration = 0; // Re-fetch
		ledController.containerstates = ContainerStates(level, videoNr); 
		viewpointManager.PlayVideo(all_videos [level][videoNr], rearFilename, tempFilename); // Load video (prepare), show temporary image
		defectsInThisLevel = CountDefects(ledController.containerstates);
		rewindVideoPos = viewpointManager.VideoSeekPosition();
		if(rewindVideoPos < 200)
			rewindVideoPos = 9999;
		waitForRewind = true;
		Debug.Log ("GameController: RewindVideoPos: "+rewindVideoPos);
	}

	// Initialize container_state list
	public int[] ContainerStates(int level, int videoNr) {
		var list = new List<int>();
		int[] video_sequence = video_sequences[level][videoNr];
		for(int n=0; n < video_sequence.Length; n++) {
			list.AddRange(defect_sequences[video_sequence[n]]);
		}
		list.AddRange(new int[] { 0,0,0,0,0,0,0,0 }); // Pseudo sequence as filler at the end
		return list.ToArray();
	}

	public int CountDefects(int[] containerStates) {
		int result = 0;
		for(int n=0; n<containerStates.Length; n++) {
			result += ( containerStates[n] > 0 ) ? 1 : 0;
		}
		return result;
	}

	public void MessageThree() {
		scoreKeeper.ShowMessage ("3",1.0f);
		if (VraSettings.instance.isHeadset) {
			gameAudioSource.PlayOneShot(beepClip);
		}
		Invoke ("MessageTwo",1.0f);
	}
	
	public void MessageTwo() {
		scoreKeeper.ShowMessage ("2",1.0f);
		if (VraSettings.instance.isHeadset) {
			gameAudioSource.PlayOneShot(beepClip);
		}
		Invoke ("MessageOne",1.0f);
	}

	public void MessageOne() {
		scoreKeeper.ShowMessage ("1",1.0f);
		if (VraSettings.instance.isHeadset) {
			gameAudioSource.PlayOneShot(beepClip);
		}
//		Invoke ("StartGame",0.5f); // Starting the video will add some extra delay.
		Invoke ("StartGame",1.0f); // Starting the video will add some extra delay.
	}

	public void StartGame() {
		gamestate = GAMESTATE.RUNNING;
		Debug.Log ("GameController: start game, start video ("+Time.time*1000+")");
//		SetVideo (currentLevel, Mathf.FloorToInt (Random.value * 2.999f));
		viewpointManager.PlayVideo();
		if (VraSettings.instance.isHeadset) {
			machineAudioSource.clip = machineRunningClip;
			machineAudioSource.Play ();
			machineAudioSource.loop = true;
		}
		pauseTarget.SetActive(true);
		GameSyncHere();
	}

	public bool GameIsRunning() {
		return (gamestate == GAMESTATE.RUNNING);
	}

	public void OnPlayerConnected() {
		Debug.Log ("GameController: New Player (Tablet) connected.");
//		scoreKeeper.SyncScore();
//		if (isServer) {
//			GameSyncHere();
//		}
	}

	// Called by GameSync on Tablet VRA to update game state
	public void GameStateSync(int level, int videoNr, int gamestate, int videoPosition) {
		Debug.Log("GameController StateSync level: "+level+", videoNr: "+videoNr+", gamestate: "+gamestate
		          +", videoPosition: "+videoPosition+" ("+Time.time*1000+")");
		if(level != currentLevel)
			StartLevel(level);
		if(videoNr != this.videoNr || (viewpointManager.currentMediaPlayerFileName != viewpointManager.VideoFilePath(all_videos [level][videoNr]))) 
			SetVideo(level,videoNr);
		this.gamestate = (GAMESTATE) gamestate;
		if(this.gamestate==GAMESTATE.RUNNING) {
			retryTarget.SetActive(false);
			highScoreKeeper.ClosePanels();
			if(viewpointManager.isPlayingVideo() ) {
				int seekPos = viewpointManager.VideoSeekPosition();
				if (seekPos > 0 && Mathf.Abs (seekPos - videoPosition) > 500) {
					Debug.Log("GameController Syncing Video, mediaPlayer.SeekTo("+videoPosition+"+250)");
					viewpointManager.mediaPlayer.SeekTo(videoPosition+250);
				}
			} else {
				viewpointManager.PlayVideo();
			}
			if(!machineAudioSource.isPlaying)
				machineAudioSource.Play();
			if(level>0) {
				pauseTarget.SetActive(true);
			}
			continueTarget.SetActive(false);
		}
		if(this.gamestate==GAMESTATE.GAMEOVER) {
			//scoreKeeper.ShowFinalScore();
			if(viewpointManager.isPlayingVideo()) {
				viewpointManager.PauseVideo();
			}
			if(!highScoreKeeper.isNewHighScore(scoreKeeper.Score())) {
				retryTarget.SetActive(true);
			}
	//		highScoreKeeper.ClosePanels();
		}
		if(this.gamestate==GAMESTATE.HIGHSCORE) {
			// Todo: Update Scores!
			ShowHighScores();
		}
		if(this.gamestate==GAMESTATE.PAUSED) {
			viewpointManager.PauseVideo();
			pauseTarget.SetActive(false);
			continueTarget.SetActive(true);
		}
	}

	public void ShowHighScores() {
		gamestate = GAMESTATE.HIGHSCORE;
		scoreKeeper.HideFinalScore();
		if(viewpointManager.isPlayingVideo()) {
			viewpointManager.PauseVideo();
		}
		retryTarget.SetActive(true);
		highScoreKeeper.ShowHighScoresPhone();
	}

	// Called on phone VRA to push game state to Tablet VRA.
	public void GameSyncHere() {
		lastSync = Time.time;
		GameObject obj = GameObject.Find("GameSync");
		if (obj != null) {
			GameSync gs = obj.GetComponent<GameSync> ();
			int seekPos = viewpointManager.VideoSeekPosition();
			Debug.Log("GameController GameSyncHere level: "+currentLevel+", videoNr: "+videoNr+", gamestate: "+gamestate
			          +", videoPosition: "+seekPos+" ("+Time.time*1000+")");
			gs.SetScore (scoreKeeper.defectsFound, scoreKeeper.defectsMissed, scoreKeeper.validPassed, 
                       scoreKeeper.validMarked, scoreKeeper.bottleStatesFound, scoreKeeper.bottleStatesMissed, scoreKeeper.lives);
			gs.GameStateSync (currentLevel, videoNr, (int) gamestate, seekPos);
		}
	}

	public void Retry() {
		Debug.Log ("GameController: Retry!");
		viewpointManager.StopVideo();
		viewpointManager.LoadViewpoint("vp04");
//		scoreKeeper.SetScore (0,0,0,0);
//		scoreKeeper.HideFinalScore();
//		StartLevel(1);
	}

	public void Pause() {
		Debug.Log ("GameController: Pause");
		viewpointManager.PauseVideo();
		machineAudioSource.Stop ();
		pauseTarget.SetActive(false);
		continueTarget.SetActive(true);
		this.gamestate = GAMESTATE.PAUSED;
		GameSyncHere();
	}

	public void Continue() {
		Debug.Log ("GameController: Continue");
		viewpointManager.PlayVideo();
		machineAudioSource.Play ();
		pauseTarget.SetActive(true);
		continueTarget.SetActive(false);
		this.gamestate = GAMESTATE.RUNNING;
		GameSyncHere();
	}


}
