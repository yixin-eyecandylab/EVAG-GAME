using UnityEngine;
using System.Collections;

public class ProgressBar : MonoBehaviour { 

	public float progress;
	private float lastProgress = -1;

	public void LateUpdate() {
		if(progress != lastProgress) {
			Vector2 uvOffset = new Vector2 (0.0f, progress * 0.5f);
			GetComponent<Renderer>().materials [0].SetTextureOffset ("_MainTex", uvOffset);
			lastProgress = progress;
		}
	}

	public void SetColor(Color newColor) {
		if (GetComponent<Renderer>().enabled) {
			GetComponent<Renderer>().materials [0].color = newColor;
		}
	}

}