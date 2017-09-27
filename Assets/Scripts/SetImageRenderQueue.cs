/*
	SetRenderQueue.cs
 
	Sets the RenderQueue of an object's materials on Awake. This will instance
	the materials, so the script won't interfere with other renderers that
	reference the same materials.
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[AddComponentMenu("Rendering/SetCanvasRenderQueue")]

public class SetImageRenderQueue : MonoBehaviour {
	
	[SerializeField]
	protected int[] m_queues = new int[]{3000};
	
	protected void Awake() {
		Image img = GetComponent<UnityEngine.UI.Image>();
		if(img.material!=null)
			img.material.renderQueue = m_queues[0];
	}
}