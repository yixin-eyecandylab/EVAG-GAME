using UnityEngine;
using System.Collections;
[RequireComponent( typeof( NetworkView ) )]

public class GameSync : MonoBehaviour {

	MouseInput mouseInput=null;

	[RPC] public void SetScoreRemote(int defectsFound, int defectsMissed, int validPassed, int validMarked,  int bottleStatesFound, int bottleStatesMissed, int lives) {
		GameObject scoreKeeper = GameObject.Find ("ScoreKeeper"); 
		if(scoreKeeper!=null) {
			scoreKeeper.GetComponent<ScoreKeeper>().SetScore(defectsFound, defectsMissed, validPassed, validMarked, bottleStatesFound, bottleStatesMissed, lives);
		}
	}	
	
	public void SetScore(int defectsFound, int defectsMissed, int validPassed, int validMarked,  int bottleStatesFound, int bottleStatesMissed, int lives) {
		GetComponent<NetworkView>().RPC ("SetScoreRemote", RPCMode.Others, defectsFound, defectsMissed, validPassed, validMarked, bottleStatesFound, bottleStatesMissed, lives);
	}
	
	[RPC] public void GameStateSyncRemote(int level, int videoNr, int gamestate, int videoPosition) {
		GameObject gameController = GameObject.Find ("GameController"); 
		if(gameController!=null) {
			gameController.GetComponent<GameController>().GameStateSync(level, videoNr, gamestate, videoPosition);
		}
	}	

	public void GameStateSync(int level, int videoNr, int gamestate, int videoPosition) {
		GetComponent<NetworkView>().RPC ("GameStateSyncRemote", RPCMode.Others, level, videoNr, gamestate, videoPosition);
	}	

	[RPC] public void MarkContainerRemote(int containerNr, int state) {
		GameObject gameController = GameObject.Find ("GameController"); 
		if(gameController!=null) {
			gameController.GetComponent<GameController>().ledController.MarkContainer(containerNr, state);
		}
	}	
	
	public void MarkContainer(int containerNr, int state) {
		GetComponent<NetworkView>().RPC ("MarkContainerRemote", RPCMode.Others, containerNr, state);
	}	
	

	[RPC] public void ShowMessageRemote(string message, float duration) {
		GameObject gameController = GameObject.Find ("GameController"); 
		if(gameController!=null) {
			gameController.GetComponent<GameController>().scoreKeeper.ShowMessage(message, duration);
		}
	}	
	
	public void ShowMessage(string message, float duration) {
		GetComponent<NetworkView>().RPC ("ShowMessageRemote", RPCMode.Others, message, duration);
	}	
	

	[RPC] public void GameOverRemote() {
		GameObject gameController = GameObject.Find ("GameController"); 
		if(gameController!=null) {
			gameController.GetComponent<GameController>().GameOver();
		}
	}	
	
	public void GameOver() {
		GetComponent<NetworkView>().RPC ("GameOverRemote", RPCMode.Others);
	}	
	
	[RPC] public void StartLevelRemote(int level) {
		GameObject gameController = GameObject.Find ("GameController"); 
		if(gameController!=null) {
			gameController.GetComponent<GameController>().StartLevel(level);
		}
	}	
	
	public void StartLevel(int level) {
		GetComponent<NetworkView>().RPC ("StartLevelRemote", RPCMode.Others, level);
	}	

	// Smiley Joy
	[RPC] public void ShowSmileyJoyRemote() {
		GameObject gameController = GameObject.Find ("ScoreKeeper"); 
		if(gameController!=null) {
			gameController.GetComponent<ScoreKeeper>().ShowSmileyJoy();
		}
	}	
	
	public void ShowSmileyJoy() {
		GetComponent<NetworkView>().RPC ("ShowSmileyJoyRemote", RPCMode.Others);
	}	

	// Smiley Neutral
	[RPC] public void ShowSmileyNeutralRemote() {
		GameObject gameController = GameObject.Find ("ScoreKeeper"); 
		if(gameController!=null) {
			gameController.GetComponent<ScoreKeeper>().ShowSmileyNeutral();
		}
	}	
	
	public void ShowSmileyNeutral() {
		GetComponent<NetworkView>().RPC ("ShowSmileyNeutralRemote", RPCMode.Others);
	}	

	// Smiley Frown
	[RPC] public void ShowSmileyFrownRemote() {
		GameObject gameController = GameObject.Find ("ScoreKeeper"); 
		if(gameController!=null) {
			gameController.GetComponent<ScoreKeeper>().ShowSmileyFrown();
		}
	}	
	
	public void ShowSmileyFrown() {
		GetComponent<NetworkView>().RPC ("ShowSmileyFrownRemote", RPCMode.Others);
	}	


	// Mouse (Marker) position
	[RPC] public void SetMousePositionRemote(float position) {
		if(mouseInput==null) {
			GameObject obj = GameObject.Find("JogShuttleMouse");
			if(obj!=null) {
				mouseInput = obj.GetComponent<MouseInput> ();
			}
		} else {
			mouseInput.SetMousePosition(position);
		}
	}	
	
	public void SetMousePosition(float position) {
		GetComponent<NetworkView>().RPC ("SetMousePositionRemote", RPCMode.Others, position);
	}	
	

}
