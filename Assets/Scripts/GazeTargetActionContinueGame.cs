using UnityEngine;
using System.Collections;

public class GazeTargetActionContinueGame : GazeTargetAction {

	public override void PerformAction() {
		GameObject.Find ("GameController").GetComponent<GameController>().Continue();
	}

}
