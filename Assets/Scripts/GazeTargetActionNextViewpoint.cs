using UnityEngine;
using System.Collections;

public class GazeTargetActionNextViewpoint : GazeTargetAction {

	public override void PerformAction() {
		Debug.Log("GazeTargetActionNextViewpoint invoking ViewpointManager.NextViewpoint()");
		GameObject.Find ("ViewpointManager").GetComponent<ViewpointManager>().NextViewpoint ();
	}


}
