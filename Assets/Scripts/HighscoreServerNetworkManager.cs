using System;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HighscoreServerNetworkManager : MonoBehaviour
{

    public bool isAtStartup = true;

    NetworkClient myClient;

    public HighScoreKeeper highScoreKeeper;

    int MaxConnections = 1000;
    ConnectionConfig config;
    int ReliableChannel;
    int ReliableFragmentedChannel;

    void OnEnable()
    {
        if (VraSettings.instance.isHighscoreServer)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        if (VraSettings.instance.isHeadset)
        {
            OnNewHighScoreByUser += HighScoreByUser;
        }

    }
    void OnDisable()
    {
        if (VraSettings.instance.isHighscoreServer)
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
        if (VraSettings.instance.isHeadset)
        {
            OnNewHighScoreByUser -= HighScoreByUser;
        }
    }

    private void HighScoreByUser(int score, string playerName, string playerEmail)
    {
        if (myClient != null)
        {
            SendScore(score, playerName, playerEmail);
        }
    }

    void Awake()
    {
        config = new ConnectionConfig();
        ReliableChannel = config.AddChannel(QosType.Reliable);
        ReliableFragmentedChannel = config.AddChannel(QosType.ReliableFragmented);
    }

    public TextMeshProUGUI NumberOfPlayersText;

    void Update()
    {
        if (isAtStartup)
        {
            if (VraSettings.instance.isHighscoreServer)
            {
                SetupServer();
            }
            else if (VraSettings.instance.isHeadset)
            {
                SetupNetworkDiscoveryClient();
            }
        }
 
        
        }
    /*
    void OnGUI()
    {
        if (isAtStartup)
        {
            GUI.Label(new Rect(2, 10, 150, 100), "Press S for server");
            GUI.Label(new Rect(2, 50, 150, 100), "Press C for client");
        }
    }
    */
    // Create a server and listen on a port
    public void SetupServer()
    {
        NetworkServer.Configure(config, MaxConnections);
        NetworkServer.Listen(4444);

        isAtStartup = false;
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnect);
        NetworkServer.RegisterHandler(MyMsgType.Score, OnScore);


        Debug.Log("MessageType ID: " + MyMsgType.Score);

        Debug.Log("NetworkManager: Starting NetworkDiscovery as Server");
        NetworkDiscovery nd = GetComponent<NetworkDiscovery>();
        nd.Initialize();
        nd.StartAsServer();
    }


    // client function
    public void OnConnected(NetworkMessage netMsg)
    {
        if (NetworkServer.active)
        {
            Debug.Log("Connected to client");
            SendScoreXmlToClient(netMsg.conn);

            UpdateNumberOfConnections();

            // Send latest scores
        }
        else
        {
            Debug.Log("Connected to server");
            if (myClient != null)
            {
                // we are ready
            }
            //  KeepSendingScores();
        }

    }



    public class MyMsgType
    {
        public static short Score = MsgType.Highest + 1;
        public static short XmlScores = MsgType.Highest + 2;
    };

    public class ScoreMessage : MessageBase
    {
        public int score;
        public string playerName;
        public string playerEmail;
    }

    public class ScoreXmlMessage : MessageBase
    {
        public string scoreXml;
    }

    public void SendScore(int score, string playerName, string playerEmail)
    {
        ScoreMessage msg = new ScoreMessage();
        msg.score = score;
        msg.playerName = playerName;
        msg.playerEmail = playerEmail;

        Debug.Log("Sending Score");
        //		NetworkServer.SendToAll(MyMsgType.Score, msg);
        myClient.Send(MyMsgType.Score, msg);
    }


    private void SendScoreXmlToClient(NetworkConnection conn) // on connection, give client the latest xml
    {
        ScoreXmlMessage msg = new ScoreXmlMessage();
        msg.scoreXml = GetLatestXml();
        conn.SendByChannel(MyMsgType.XmlScores, msg, ReliableFragmentedChannel);
        Debug.Log("Client Connected, Send latest XML");
    }



    public void SendScoreXml(string xml) // when the xml changes, update all clients
    {
        ScoreXmlMessage msg = new ScoreXmlMessage();
        msg.scoreXml = xml;
        NetworkServer.SendByChannelToAll(MyMsgType.XmlScores, msg, ReliableFragmentedChannel);
        Debug.Log("Xml Changed, Send latest XML");
    }

    // Setup Network Discovery Client

    public void SetupNetworkDiscoveryClient()
    {
        NetworkDiscovery nd = GetComponent<NetworkDiscovery>();
        if (!nd.isClient && !nd.isServer)
        {
            nd.Initialize();
            nd.StartAsClient();
        }

    }

    // Create a client and connect to the server port
    public void SetupClient()
    {

        myClient = new NetworkClient();
        myClient.Configure(config, MaxConnections);
        myClient.RegisterHandler(MsgType.Disconnect, OnDisconnect);
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        myClient.RegisterHandler(MyMsgType.Score, OnScore);
        Debug.Log("MessageType ID: " + MyMsgType.Score);
        Debug.Log("NetworkManager: Starting NetworkDiscovery as Local Client");
        myClient.Connect("127.0.0.1", 4444);

        isAtStartup = false;

        NetworkDiscovery nd = GetComponent<NetworkDiscovery>();
        nd.Initialize();
        nd.StartAsClient();
    }

    // Create a client and connect to the server port
    public void ClientConnectTo(string ipAddress)
    {
        myClient = new NetworkClient();
        myClient.Configure(config, MaxConnections);
        myClient.RegisterHandler(MsgType.Disconnect, OnDisconnect);
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        myClient.RegisterHandler(MyMsgType.Score, OnScore);
        myClient.RegisterHandler(MyMsgType.XmlScores, OnScoreXml);
        Debug.Log("MessageType ID for Score message is: " + MyMsgType.Score);
        Debug.Log("Trying to connect to " + ipAddress);
        myClient.Connect(ipAddress, 4444);
        isAtStartup = false;
    }

    private void OnDisconnect(NetworkMessage netMsg)
    {
        if (VraSettings.instance.isHighscoreServer)
        {
            UpdateNumberOfConnections();
        } else
        {
 isAtStartup = true;
        }
       
    }

    private void UpdateNumberOfConnections()
    {
        if (VraSettings.instance.isHighscoreServer && NumberOfPlayersText != null)
        {
            int number = 0;
            for (int i=0;i< NetworkServer.connections.Count; i++)
            {
                if (NetworkServer.connections[i] != null)
                {
                    number++;
                }
            }
            NumberOfPlayersText.text = string.Format("There are {0} players connected", number);
        }
    }

    public void KeepSendingScores()
    {
        if (myClient != null)
        {
            SendScore(UnityEngine.Random.Range(0, 99), "foobar", null);
        }

        Invoke("KeepSendingScores", 1.0f);
    }

    private string GetLatestXml()
    {
        GetHighScoreKeeper();
        return highScoreKeeper.GetHighScoreListXML();
    }

    public void OnScore(NetworkMessage netMsg)
    {
        ScoreMessage msg = netMsg.ReadMessage<ScoreMessage>();
        DoNewScoreRecieved(msg);
        SendScoreXml(GetLatestXml());
    }

    private void OnScoreXml(NetworkMessage netMsg)
    {
        ScoreXmlMessage msg = netMsg.ReadMessage<ScoreXmlMessage>();
        GetHighScoreKeeper();
        highScoreKeeper.LoadXML(msg.scoreXml);
        Debug.Log("xml scores received" + msg.scoreXml);
    }

    void GetHighScoreKeeper()
    {
        if (highScoreKeeper == null)
        {
            GameObject obj = GameObject.Find("HighScoreKeeper");
            if (obj)
            {
                highScoreKeeper = obj.GetComponent<HighScoreKeeper>();
            }
        }

    }




    public delegate void NewScoreRecieved(ScoreMessage newscore);
    public static event NewScoreRecieved OnNewScoreRecieved;


    public static void DoNewScoreRecieved(ScoreMessage newscore)
    {
        Debug.Log("OnScoreMessage " + newscore.score + " " + newscore.playerName + " " + newscore.playerEmail);
        if (OnNewScoreRecieved != null)
        {
            OnNewScoreRecieved(newscore);

        }
    }


    public delegate void NewHighScoreByUser(int score, string playerName, string playerEmail);
    public static event NewHighScoreByUser OnNewHighScoreByUser;


    public static void DoNewHighScoreByUser(int score, string playerName, string playerEmail)
    {
        if (OnNewHighScoreByUser != null)
        {
            OnNewHighScoreByUser(score, playerName, playerEmail);
        }
    }

}