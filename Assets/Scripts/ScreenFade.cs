// Based on OVRScreenFade

using UnityEngine;
using System.Collections; // required for Coroutines

/// <summary>
/// Fades the screen from black after a new scene is loaded.
/// </summary>
public class ScreenFade : MonoBehaviour
{
	/// <summary>
	/// How long it takes to fade.
	/// </summary>
	public float fadeTime = 1.0f;

	/// <summary>
	/// The initial screen color.
	/// </summary>
	public Color fadeColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

	/// <summary>
	/// The shader to use when rendering the fade.
	/// </summary>
	public Shader fadeShader = null;

	protected Material fadeMaterial = null;
	protected bool isFading = false;

	/// <summary>
	/// Initialize.
	/// </summary>
	void Awake()
	{
		// create the fade material
		fadeMaterial = (fadeShader != null) ? new Material(fadeShader) : new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
	}

	/// <summary>
	/// Starts the fade in
	/// </summary>
	void OnEnable()
	{
		StartCoroutine(FadeIn());

//#if UNITY_ANDROID && !UNITY_EDITOR
//		// Add a listener to OVRPostRender for custom postrender work
//		OVRPostRender.OnCustomPostRender += OnCustomPostRender;
//#endif
	}

	public void StartFadeIn(float fTime) {
		fadeTime = fTime;
		StartCoroutine(FadeIn());
	}

	public void StartFadeOut(float fTime) {
		fadeTime = fTime;
		StartCoroutine(FadeOut());
	}
	

	void OnDisable()
	{
//#if UNITY_ANDROID && !UNITY_EDITOR
//		if(VraSettings.instance.isGearVR) {
//		// Remove listener on OVRPostRender for custom postrender work
//		OVRPostRender.OnCustomPostRender -= OnCustomPostRender;
//		}
//#endif
	}

	/// <summary>
	/// Starts a fade in when a new level is loaded
	/// </summary>
	void OnLevelWasLoaded(int level)
	{
		StartCoroutine(FadeIn());
	}

	/// <summary>
	/// Cleans up the fade material
	/// </summary>
	void OnDestroy()
	{
		if (fadeMaterial != null)
		{
			Destroy(fadeMaterial);
		}
	}

	/// <summary>
	/// Fades alpha from 1.0 to 0.0
	/// </summary>
	IEnumerator FadeIn()
	{
		float elapsedTime = 0.0f;
		Color color = fadeMaterial.color = fadeColor;
		isFading = true;
		while (elapsedTime < fadeTime)
		{
			yield return new WaitForEndOfFrame();
			elapsedTime += Time.deltaTime;
            color.a =  1.0f - Mathf.Clamp01(elapsedTime / fadeTime);
			fadeMaterial.color = color;
//			Debug.Log ("FadeIn ("+color+")");
		}
		isFading = false;
	}


	/// <summary>
	/// Fades alpha from 0.0 to 1.0
	/// </summary>
	IEnumerator FadeOut()
	{
		float elapsedTime = 0.0f;
		Color color =  fadeColor;
		isFading = true;
		while (elapsedTime < fadeTime)
		{
			yield return new WaitForEndOfFrame();
			elapsedTime += Time.deltaTime;
			color.a = Mathf.Clamp01(elapsedTime / fadeTime);
//			Debug.Log ("FadeOut ("+color+")");
			fadeMaterial.color = color;
		}
		color.a = 1.0f;
		fadeMaterial.color = color;
		//		isFading = false; // Stay true, screen remains faded.
	}

	/// <summary>
	/// Renders the fade overlay when attached to a camera object
	/// </summary>
//#if UNITY_ANDROID && !UNITY_EDITOR
//	public void OnCustomPostRender()
//#else
	void OnPostRender()
//#endif
	{
		if (isFading)
		{
			fadeMaterial.SetPass(0);
			GL.PushMatrix();
			GL.LoadOrtho();
			GL.Color(fadeMaterial.color);
			GL.Begin(GL.QUADS);
			GL.Vertex3(0f, 0f, -12f);
			GL.Vertex3(0f, 1f, -12f);
			GL.Vertex3(1f, 1f, -12f);
			GL.Vertex3(1f, 0f, -12f);
			GL.End();
			GL.PopMatrix();
		}
	}
}
