using UnityEngine;
using System.Collections;

public class OpenTouchScreenKeyboard : MonoBehaviour {

	public UnityEngine.UI.InputField inputField;
	private TouchScreenKeyboard keyboard = null;


	// Use this for initialization
	void Start () {
		if(TouchScreenKeyboard.isSupported)
			keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
	}
	
	void OpenKeyboard () {
		if(TouchScreenKeyboard.isSupported)
			keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
	}

	// Update is called once per frame
	void Update () {
		if (keyboard != null && keyboard.done)
		{
			inputField.text = keyboard.text;
			Debug.Log("User input is: " + keyboard.text);
		}
	}
}
