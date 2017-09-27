using UnityEngine;
using System.Collections;

public class LED : MonoBehaviour {
	private Renderer rend;
	private Renderer haloRend;
	private GameObject halo;
	private Color color;

	// Use this for initialization
	void Awake () {
		rend = this.GetComponent<Renderer>();
		halo = transform.GetChild(0).gameObject;
		haloRend = halo.GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetColor(Color newColor) {
		color = newColor;
		rend.material.SetColor("_EmissionColor",color);
		haloRend.material.SetColor("_Color",color);
		if(color == Color.black)
			halo.SetActive(false);
		else
			halo.SetActive(true);
	}

	public Color GetColor () {
		return color;
	}

	public void Off() {
		color=Color.black;
		rend.material.SetColor("_EmissionColor",Color.black);
		halo.SetActive(false);
	}
}
