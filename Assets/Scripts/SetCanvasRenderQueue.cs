/*
	SetRenderQueue.cs
 
	Sets the RenderQueue of an object's materials on Awake. This will instance
	the materials, so the script won't interfere with other renderers that
	reference the same materials.
*/

using UnityEngine;
using System.Collections;

[AddComponentMenu("Rendering/SetCanvasRenderQueue")]

public class SetCanvasRenderQueue : MonoBehaviour {
	
	[SerializeField]
	protected int[] m_queues = new int[]{3000};
	
	protected void Awake() {
		CanvasRenderer rend = GetComponent<CanvasRenderer>();
		for (int i = 0; i < rend.materialCount && i < m_queues.Length; ++i) {
			rend.GetMaterial(i).renderQueue = m_queues[i];
		}
	}
}