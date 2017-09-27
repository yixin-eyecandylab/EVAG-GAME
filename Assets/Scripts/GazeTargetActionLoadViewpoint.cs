using UnityEngine;
using System.Collections;

public class GazeTargetActionLoadViewpoint : GazeTargetAction {

	public int viewpointId;
	public string tag;
	public bool recenter;

	public override void PerformAction() {
		Debug.Log("GazeTargetActionLoadViewpoint invoking ViewpointManager.LoadViewpoint("+viewpointId+")");
		GameObject.Find ("ViewpointManager").GetComponent<ViewpointManager>().LoadViewpoint (tag, recenter);
	}


}
