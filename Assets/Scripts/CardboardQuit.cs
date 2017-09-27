using UnityEngine;
using System.Collections;

public class CardboardQuit : MonoBehaviour 
{
	void Awake() 
	{
		GetComponent<Cardboard>().OnBackButton += HandleOnBackButton;
		GetComponent<Cardboard>().OnTilt += HandleOnBackButton;
	}
	
	void HandleOnBackButton ()
	{
		Application.Quit();        
	}
}
