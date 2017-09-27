using UnityEngine;
using System.Collections;

public class SwitchOnTap : MonoBehaviour {
	public GameObject leftSphereOne;
	public GameObject rightSphereOne;
	public GameObject leftSphereTwo;
	public GameObject rightSphereTwo;
	public GameObject leftSphereThree;
	public GameObject rightSphereThree;

	private int mode = 1;

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			if(mode == 3) {
				mode = 1;
				leftSphereOne.SetActive (true);
				rightSphereOne.SetActive (true);
				leftSphereTwo.SetActive (false);
				rightSphereTwo.SetActive (false);
				leftSphereThree.SetActive (false);
				rightSphereThree.SetActive (false);
			} else if (mode == 1) {
				mode = 2;
				leftSphereOne.SetActive (false);
				rightSphereOne.SetActive (false);
				leftSphereTwo.SetActive (true);
				rightSphereTwo.SetActive (true);
				leftSphereThree.SetActive (false);
				rightSphereThree.SetActive (false);
			} else if (mode == 2) {
				mode = 3;
				leftSphereOne.SetActive (false);
				rightSphereOne.SetActive (false);
				leftSphereTwo.SetActive (false);
				rightSphereTwo.SetActive (false);
				leftSphereThree.SetActive (true);
				rightSphereThree.SetActive (true);
			}
		}
	}
}
