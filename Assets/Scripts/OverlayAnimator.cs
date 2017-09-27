// Based on OVRScreenFade

using UnityEngine;
using System.Collections; // required for Coroutines

public class OverlayAnimator : MonoBehaviour
{

	public Color colorStart = Color.black;
	public Color colorEnd = Color.white;
	public float duration = 1.0f;
	public GameObject leftSphere, rightSphere;

	private Material leftMat, rightMat;

	private bool isAnimating = false;

	public void Start() {
		leftMat = leftSphere.GetComponent<Renderer> ().material;
		rightMat = rightSphere.GetComponent<Renderer> ().material;
	}

	public void Update() {
		if (isAnimating) {
			float lerp = Mathf.PingPong (Time.time, duration) / duration;
			leftMat.color = rightMat.color = Color.Lerp (colorStart, colorEnd, lerp);
		}
	}

	public void Play() {
		leftMat.color = rightMat.color = colorStart;
		isAnimating = true;
	}

	public void Stop() {
		leftMat.color = rightMat.color = colorStart;
		isAnimating = false;
	}

}
