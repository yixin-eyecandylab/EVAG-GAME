using UnityEngine;
using System.Collections;

public class GazeTargetActionLoadLevel : GazeTargetAction {

	public string levelName;

	public override void PerformAction() {
		GameObject.Find ("ViewpointManager").GetComponent<ViewpointManager>().LoadLevel(levelName);
	}

}
