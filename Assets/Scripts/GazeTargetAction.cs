using UnityEngine;
using System.Collections;

public abstract class GazeTargetAction : MonoBehaviour {
	// Doing what he do...
	public virtual void PerformAction() {

	}

	public virtual void Start() {
	}

	public virtual bool isPerforming() {
		return false;
	}

}
