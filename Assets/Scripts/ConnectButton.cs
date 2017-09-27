using UnityEngine;
using UnityEngine.UI;

public class ConnectButton : MonoBehaviour {

	public string address;
	public int port;

	public void OnClick() 
	{
		GameObject obj = GameObject.FindGameObjectWithTag ("NetworkManager");
		if (obj == null) {
			Debug.LogError ("ConnectButton: NetworkManager object not found!");
			return;
		}
		TabletNetworkManager nm = obj.GetComponent<TabletNetworkManager> ();
		if (nm == null) {
			Debug.LogError ("ConnectButton: NetworkManager component not found!");
			return;
		}
		nm.Connect(address,port);
	}

	public void Awake() {
		Button btn = gameObject.GetComponent<Button>();
		btn.onClick.AddListener(delegate { OnClick(); });  
	}

}
