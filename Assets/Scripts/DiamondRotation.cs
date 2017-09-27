using UnityEngine;
using System.Collections;

public class DiamondRotation : MonoBehaviour {

    private Vector3 rotation = new Vector3(0.0f, 0.0f, 10.0f);
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        this.gameObject.transform.Rotate(rotation * Time.deltaTime);
    }
}
