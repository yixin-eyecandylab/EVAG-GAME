using UnityEngine;
using System.Collections;

public class GazeTargetActionPauseGame : GazeTargetAction {

	public override void PerformAction() {
		GameObject.Find ("GameController").GetComponent<GameController>().Pause();
	}

}
