using UnityEngine;
using System.Collections;

public class Target {

	public enum TARGET_STYLE {
		NEXT = 0,
		PREVIOUS = 1,
		INFO = 2,
		START = 3,
		STOP = 4,
		JUMP = 5,
		PREF = 6,
		PREF_DE = 7,
		PREF_EN = 8,
		LABEL = 9,
		LOAD_LEVEL = 10,
        BANNER = 11
    }

	public Vector3 position;
	public string label;
	public string labelEN;
	public TARGET_STYLE style;
	public string audioClip;
	public string audioClipEN;
	public int viewpointId;
	public string tag;
	public string pref_key;
	public string pref_value;
	public string pref_icon;
	public bool recenter;
	public string levelname;
	public float scale = 1.0f;

}
