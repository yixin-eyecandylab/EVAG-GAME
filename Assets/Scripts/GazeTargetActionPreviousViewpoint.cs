using UnityEngine;
using System.Collections;

public class GazeTargetActionPreviousViewpoint : GazeTargetAction {

	public override void PerformAction() {
		Debug.Log("GazeTargetActionPreviousViewpoint invoking ViewpointManager.PreviousViewpoint()");
		GameObject.Find ("ViewpointManager").GetComponent<ViewpointManager>().PreviousViewpoint ();
	}


}
