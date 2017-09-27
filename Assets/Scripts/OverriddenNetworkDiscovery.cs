using UnityEngine;
using UnityEngine.Networking;

public class OverriddenNetworkDiscovery : NetworkDiscovery
{

	public override void OnReceivedBroadcast (string fromAddress, string data)
	{
		if(GetComponent<HighscoreServerNetworkManager>().isAtStartup) {
			Debug.Log("NetworkDiscovery received broadcast, trying to join");
			GetComponent<HighscoreServerNetworkManager>().ClientConnectTo(fromAddress);
		} else {
			Debug.Log("NetworkDiscovery received broadcast (ignored)");
		}
	}
	
}