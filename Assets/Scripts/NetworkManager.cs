using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class NetworkManager : MonoBehaviour
{

	public bool autoConnect = true;

	private ArrayList hostList;
	private const string typeName = "VRA-V90";
	private const string MESSAGE_TOKEN = "VisardSeidenaderVRAV90 ";
	private const string REQUEST_TOKEN = "Requesting List ";
	private const string REPLY_TOKEN = "Request Reply ";
	private const int NETWORK_PORT = 25090;
	private const int BROADCAST_PORT = 8178;

	private UdpClient client;
	private IPEndPoint ipEndPoint;
	
	//	private HostData[] hostList;
	public GameObject networkAvatarPrefab;
	public Vector3 startPosition = new Vector3 (0f, 0f, 0f);

	private void SpawnPlayer ()
	{
		GameObject player = (GameObject) Network.Instantiate (networkAvatarPrefab, startPosition, Quaternion.identity, 0);
	}
	
	void Start ()
	{
		if(Network.isServer) {
			// Network already started, one NetworkManager is enough
			Destroy(gameObject);
		} else {
	//		Debug.Log ("NetworkManager, deviceName: " + SystemInfo.deviceName);
	//		Debug.Log ("deviceUniqueIdentifier:" + SystemInfo.deviceUniqueIdentifier);
			Network.sendRate = 30;
			ReceiveMessages ();
			StartServer ();
			DontDestroyOnLoad(this);
			StartCoroutine (LogPingTimes());
		}
	}

	private IEnumerator LogPingTimes() 	{
		while (true) {
			int i = 0;
			while (i < Network.connections.Length) {
				Debug.Log ("NetworkManager: ping time player " + Network.connections [i] + ": " + Network.GetLastPing (Network.connections [i]) + " ms");
				i++;
			}
			yield return new WaitForSeconds (2.0f);
		}
	}

	public void StartServer ()
	{
		Debug.Log ("NetworkManager.StartServer");
		// Network.InitializeServer(4, 25000, !Network.HavePublicAddress());
		Network.InitializeServer (4, NETWORK_PORT, false); // No NAT punchthrough
		//		Network.InitializeServer(4, 25000, true); // Do NAT punchthrough
	}

	void OnServerInitialized ()
	{
		Debug.Log ("Server Initializied");
		SpawnPlayer ();
	}
	
	void Update () {
		// Restart Network Server (if necessary) when returning to start point.
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if(!Network.isServer) {
				StartServer();
			}
		}
	}

	public void Quit ()
	{
		Debug.Log("NetworkManager.Quit");
		Network.Disconnect ();
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit ();
		#endif
	}

	void OnApplicationQuit() {
		Debug.Log("Stop UDP client");
		client.Close ();
	}

	/*
	 * UDP Discovery
	 */


	// Start UDP Server 
	public void ReceiveMessages()
	{
		//		ipEndPoint = new IPEndPoint(IPAddress.Parse ("224.42.43.44"), 47890);
		ipEndPoint = new IPEndPoint(IPAddress.Any, BROADCAST_PORT);
		client = new UdpClient(ipEndPoint);
		Debug.LogError("listening for messages");
		client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
	}

	// Handle incoming packet
	public void ReceiveCallback(IAsyncResult ar)
	{
		IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, BROADCAST_PORT);
		Byte[] receiveBytes = client.EndReceive(ar, ref remoteEP);
		string receiveString = Encoding.ASCII.GetString(receiveBytes);
		Debug.LogError("Received: "+receiveString+", ipEP:"+remoteEP);
		if (receiveString.StartsWith (MESSAGE_TOKEN + " " + REQUEST_TOKEN)) {
			Debug.LogError ("Got list request");
			String reply = MESSAGE_TOKEN + " " + REPLY_TOKEN;
			Debug.LogError ("Sending reply: " + reply);
			IPEndPoint replyEP = new IPEndPoint (remoteEP.Address, BROADCAST_PORT);
			SendMessage (reply, replyEP);
		} else {
			Debug.LogError("Message ignored");
		}
		client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
		Debug.LogError("Continue listening for messages");
	}

	// Send reply
	public void SendMessage(string message, IPEndPoint ipEP)
	{
		Debug.Log("Sending message \""+message+"\", ipEP: "+ipEP);
		var data = Encoding.Default.GetBytes(message);
		using (var udpClient = new UdpClient(AddressFamily.InterNetwork))
		{
			udpClient.Send(data, data.Length, ipEP);
			//			Debug.Log ("Local port" + ((IPEndPoint) udpClient.Client.LocalEndPoint).Port);
			udpClient.Close();
		}
	}
	

}
