using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class TabletNetworkManager : MonoBehaviour
{
	// Keeps Host information about found servers
	class SimpleHostData {
		public string Address;
		public string Name;
		public float seenAt;

		public override string ToString()
		{
			return this.Address + " " + this.Name;
		}

		public override bool Equals (object obj)
		{
			return this.ToString () == obj.ToString ();
		}

	}

	class HostListSorter : IComparer  {
		
		// Calls CaseInsensitiveComparer.Compare with the parameters reversed.
		int IComparer.Compare( System.Object x, System.Object y )  {
			return( ((SimpleHostData) x).Address.CompareTo(((SimpleHostData) y).Address) );
		}
		
	}

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
	
	public Vector3 startPosition = new Vector3 (0f, 0f, 0f);
	public Canvas menuCanvas;
	public Canvas inGameCanvas;
	public GameObject menuBackground;
	public GameObject listParent;
	public GameObject ConnectButtonPrefab;
	public Text statusText;
	public GameObject networkAvatarPrefab; // Unused, it seems, but it has to be referenced to be an available on the tablet.


	private string masterServerIp = "192.168.137.1";
//	private string masterServerIp = "127.0.0.1";
	private string hostIP = "";
	private float lastListUpdate = 0.0f;
	private bool waitingForConnection = false;

	public void setAutoconnect(bool connect) {
		autoConnect = connect;
	}

	public void AutoConnect() {
		if(autoConnect && !waitingForConnection && hostList.Count > 0 && !Network.isClient && !Network.isServer) {
			string address = ((SimpleHostData) hostList[0]).Address;
			Debug.Log ("TabletNetworkManager: Autoconnecting");
			Connect(address,NETWORK_PORT);
		}
	}

	public void Connect(String address, int port) {
		Debug.Log ("TabletNetworkManager: Connecting to " + address + ":" + port);
		statusText.text = "Connecting to " + address + "...";
		waitingForConnection = true;
		Network.Connect (address,port);
	}

	void Start ()
	{
//		Debug.Log ("NetworkManager, deviceName: " + SystemInfo.deviceName);
//		Debug.Log ("deviceUniqueIdentifier:" + SystemInfo.deviceUniqueIdentifier);
		Network.sendRate = 30;
		ReceiveMessages ();
		// ! Server
		statusText.text = "";
		showMenu();
		RequestServerList ();

		StartCoroutine (LogPingTimes());
	}

	private IEnumerator LogPingTimes() 	{
		while (true) {
			int i = 0;
			while (i < Network.connections.Length) {
				Debug.Log (": ping time player " + Network.connections [i] + ": " + Network.GetLastPing (Network.connections [i]) + " ms");
				i++;
			}
			yield return new WaitForSeconds (2.0f);
		}
	}

	void OnConnectedToServer ()
	{
		Debug.Log ("TabletNetworkManager: Connected to Server");
//		SpawnPlayer ();
		// Disable screen dimming
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		hideMenu ();
		inGameCanvas.enabled = true;
		waitingForConnection = false;
	}
	
	void OnDisconnectedFromServer (NetworkDisconnection info)
	{
		ShowError ("Disconnected from Server");
		if (Network.isServer)
			ShowError("Local server connection disconnected");
		else
			if (info == NetworkDisconnection.LostConnection)
				ShowError("Lost connection to the server");
		// Re-enable screen dimming
		Screen.sleepTimeout = SleepTimeout.SystemSetting;
		waitingForConnection = false;
		inGameCanvas.enabled = false;
		showMenu ();
		Debug.Log("Reloading Level/Scene Network_Overserver");
		Application.LoadLevel("Network_Observer");
	}
	
	public void showMenu() {
		Camera.main.transform.rotation = Quaternion.Euler (0.0f, 0.0f, 0.0f);
		menuBackground.SetActive (true);
        menuCanvas.enabled =  true;
		GameObject vpm = GameObject.FindGameObjectWithTag("ViewpointManager");
		if(vpm==null)
			Debug.LogError("TabletNetworkManager: Could not find ViewpointManager!");
		else
			vpm.GetComponent<ViewpointManager> ().ShowDefaultPicture ();
		if(hostList!=null)
			hostList.Clear ();
	}

	public void hideMenu() {
		menuBackground.SetActive (false);
		statusText.text = "";
		menuCanvas.enabled = false;
	}

	void OnFailedToConnect (NetworkConnectionError info)
	{
		ShowError ("Could not connect to server: " + info);
		Network.Disconnect ();
		inGameCanvas.enabled = false;
		showMenu ();
		waitingForConnection = false;
	}
	
	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("Player disconnected, cleaning up after player " + player);
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}
	
	public void ShowError (string message)
	{
		Debug.LogError ("ShowError: "+message);
		statusText.text = message;
		Invoke ("ClearStatusText", 2.0f);
	}

	private void ClearStatusText() {
		statusText.text = "";
	}
	
	void Update () {
//		if(!Network.isClient && !Network.isServer && Input.GetKeyDown( KeyCode.Escape ) ) 
//		{
//			Quit ();
//		}
		// Restart Network Server (if necessary) when returning to start point.
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if(Network.isClient) {
				Debug.Log("TabletNetworkManager: Escape");
				NetworkPlayer player =  Network.connections[0];
				if(player!=null) {
					Debug.Log("TabletNetworkManager: CloseConnection");
					Network.CloseConnection(player, true);
				} else {
					Debug.Log("TabletNetworkManager: Disconnect");
					Network.Disconnect();
				}
			}
		}

		// Update server list
		if (!waitingForConnection && !Network.isClient && menuCanvas.enabled) {
			if (Time.time > lastListUpdate + 2.0f) {
				// Update-Interval 2 seconds
				UpdateHostList();
				RequestServerList ();
				lastListUpdate = Time.time;
				Invoke ("AutoConnect",1.9f);
			}
		}

	}

	private void UpdateHostList ()
	{
		if (hostList != null) {
			lock (hostList) {
				if(hostList.Count == 0) {
					statusText.text = "Waiting for VRA...";
				} else {
					statusText.text = "";
				}
				int childCount = listParent.transform.childCount;
				for (int i=0; i<childCount; i++) {
					GameObject.Destroy (listParent.transform.GetChild (i).gameObject);
				}
				hostList.Sort (new HostListSorter ());
				for (int i = 0; i < hostList.Count; i++) {
					GameObject newButton = Instantiate (ConnectButtonPrefab) as GameObject;
					newButton.transform.SetParent (listParent.transform, false);
					ConnectButton connectButton = newButton.GetComponent<ConnectButton> ();
					connectButton.address = ((SimpleHostData)hostList [i]).Address;
					connectButton.port = NETWORK_PORT;
					Text buttonText = newButton.GetComponentInChildren<Text> ();
					buttonText.text = "Connect to " + ((SimpleHostData)hostList [i]).Address;
				}
			}
		}
	}
	
	public void Quit ()
	{
		Debug.Log("TabletNetworkManager.Quit");
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

	void OnDestroy() {
		Debug.Log("Stop UDP client");
		client.Close ();
	}

	public void RequestServerList() {
		if (hostList == null)
			hostList = new ArrayList ();
		lock (hostList) {
			bool hasPurged = true;
			while (hasPurged) {
				hasPurged = false;
				foreach (System.Object entry in hostList) {
					SimpleHostData hostData = (SimpleHostData)entry;
					if (hostData.seenAt == 0.0f)
						hostData.seenAt = Time.time; // Seen for the first time;
					if (Time.time > hostData.seenAt + 5.0f) {
						// Purge old entry from list, timeout 5 seconds
						hostList.Remove (hostData);
						hasPurged = true;
						break;
					}
				}
			}
			SendMessage (MESSAGE_TOKEN + " " + REQUEST_TOKEN, new IPEndPoint (IPAddress.Broadcast, BROADCAST_PORT));
		}
	}

	/*
	 * UDP Discovery
	 */

	// Start UDP server (to receive replies)
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
		if(receiveString.StartsWith(MESSAGE_TOKEN+" "+REPLY_TOKEN)) {
			Debug.LogError("Got request reply");
			lock(hostList) {
				SimpleHostData hostData = new SimpleHostData();
				hostData.Address = remoteEP.Address.ToString();
				hostData.Name = "foobar";
				hostData.seenAt = 0.0f;
				if(hostList.Contains(hostData))
					hostList.Remove(hostData); // Prevent duplicates
				hostList.Add (hostData); // And insert with seenAt=0 ("fresh")
			}
		} else {
			Debug.LogError("Message ignored");
		}
		client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
		Debug.LogError("Continue listening for messages");
	}

	// Send out messages
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
