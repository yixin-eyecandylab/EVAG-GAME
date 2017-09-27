using UnityEngine;
using System.Collections;

//
// All RPCs go through this object that is created als Network Player instance on the server.
//
public class NetworkSync : MonoBehaviour
{

    private HighScoreKeeper hsk;
    private NetworkView nview;

    public void Awake()
    {
        Debug.Log("NetworkSync awaking");
    }

    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        gameObject.name = "NetworkSync";
    }

    public void Update()
    {
        if (hsk == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("HighScoreKeeper");
            if (obj != null)
            {
                hsk = obj.GetComponent<HighScoreKeeper>();
            }
            else
            {
                Debug.LogError("NetworkSync: HighScoreKeeper not found!");
            }
        }
        if (nview == null)
        {
            nview = GetComponent<NetworkView>();
        }
    }

    public void LoadViewpoint(int number)
    {
        if (GetComponent<NetworkView>().isMine)
        {
            GetComponent<NetworkView>().RPC("LoadViewpointRemote", RPCMode.Others, number);
        }
    }

    [RPC]
    public void LoadViewpointRemote(int number)
    {
        GameObject vpm = GameObject.FindGameObjectWithTag("ViewpointManager");
        vpm.GetComponent<ViewpointManager>().LoadViewpoint(number);
    }

    public void LoadViewpoint(string tag)
    {
        if (GetComponent<NetworkView>().isMine)
        {
            GetComponent<NetworkView>().RPC("LoadViewpointRemote", RPCMode.Others, tag);
        }
    }

    [RPC]
    public void LoadViewpointRemote(string tag)
    {
        GameObject vpm = GameObject.FindGameObjectWithTag("ViewpointManager");
        vpm.GetComponent<ViewpointManager>().LoadViewpoint(tag);
    }

    public void NextViewpoint()
    {
        GetComponent<NetworkView>().RPC("NextViewpointRemote", RPCMode.Others);
    }

    [RPC]
    public void NextViewpointRemote()
    {
        GameObject vpm = GameObject.FindGameObjectWithTag("ViewpointManager");
        vpm.GetComponent<ViewpointManager>().NextViewpoint();
    }

    public void PreviousViewpoint()
    {
        GetComponent<NetworkView>().RPC("PreviousViewpointRemote", RPCMode.Others);
    }

    [RPC]
    public void PreviousViewpointRemote()
    {
        GameObject vpm = GameObject.FindGameObjectWithTag("ViewpointManager");
        vpm.GetComponent<ViewpointManager>().PreviousViewpoint();
    }

    public void LoadLevel(string levelName)
    {
        if (GetComponent<NetworkView>().isMine)
        {
            GetComponent<NetworkView>().RPC("LoadLevelRemote", RPCMode.Others, levelName);
        }
    }

    [RPC]
    public void LoadLevelRemote(string levelName)
    {
        GameObject vpm = GameObject.FindGameObjectWithTag("ViewpointManager");
        vpm.GetComponent<ViewpointManager>().LoadLevel(levelName);
    }

    //
    // HighScore Handling
    //

    // Push a Score date from Tablet to Phone (Server) and vice versa
    public void AddScore(string player, string email, int score)
    {
        nview.RPC("AddScoreRemote", RPCMode.Others, player, email, score);
    }

    public void AddScore(string player, string email, int score, NetworkPlayer networkPlayer)
    {
        nview.RPC("AddScoreRemote", networkPlayer, player, email, score);
    }

    [RPC]
    public void AddScoreRemote(string player, string email, int score)
    {
        Debug.Log("NetworkSync AddScoreRemote");
        hsk.AddScore(player, email, score);
    }


    // Push a new High Score (with entered Name) to Server
    public void SaveHighScore(string player, string email, int score)
    {
        nview.RPC("SaveHighScoreRemote", RPCMode.Server, player, email, score);
    }

    [RPC]
    public void SaveHighScoreRemote(string player, string email, int score)
    {
        hsk.SaveHighScore(player, email, score);
    }

    //	// Show HighScore List (InGame/Phone Version)
    //	public void ShowHighScores() {
    //		nview.RPC ("ShowHighScoresRemote", RPCMode.Others);
    //	}	
    //	
    //	[RPC] public void ShowHighScoresRemote() {
    //		hsk.ShowHighScoresPhone();
    //	}	

    // Request Scores for Tablet HighScore List

    // Request ==calls==> Send!
    public void RequestHighScoreList()
    {
        nview.RPC("SendHighScoreList", RPCMode.Server);
    }

    // The server (here) will rpc AddScore (multiple times) and ShowHighScores on the sender
    [RPC]
    public void SendHighScoreList()
    {
        hsk.SendHighScoreListXML();
    }

    public void UpdateHighScorePanels()
    {
        nview.RPC("UpdateHighScorePanelsRemote", RPCMode.Others);
    }

    [RPC]
    public void UpdateHighScorePanelsRemote()
    {
        hsk.UpdateHighScorePanels();
    }


    // Reset Score List
    public void ResetHighScores()
    {
        nview.RPC("ResetHighScoresRemote", RPCMode.Others);
    }

    public void ResetHighScores(NetworkPlayer networkPlayer)
    {
        nview.RPC("ResetHighScoresRemote", networkPlayer);
    }

    [RPC]
    public void ResetHighScoresRemote(NetworkMessageInfo info)
    {
        hsk.ResetHighScores();
    }

    public void SendXML(byte[] highScoreListXML)
    {
        nview.RPC("SendXMLRemote", RPCMode.Others, highScoreListXML);
    }

    [RPC]
    public void SendXMLRemote(byte[] highScoreListXML)
    {
        Debug.Log("NetworkSync SendXMLRemote received " + highScoreListXML.Length + " bytes");
        hsk.LoadXML(highScoreListXML);
        hsk.SaveXML();
    }

    public void SetPlayerName(string name)
    {
        nview.RPC("SetPlayerNameRemote", RPCMode.Others, name);
    }

    [RPC]
    public void SetPlayerNameRemote(string name)
    {
        hsk.SetPlayerName(name);
    }

    public void SetPlayerEmail(string email)
    {
        nview.RPC("SetPlayerEmailRemote", RPCMode.Others, email);
    }

    [RPC]
    public void SetPlayerEmailRemote(string email)
    {
        hsk.SetPlayerEmail(email);
    }


    public void SetLastScore(string name, string email, int score)
    {
        nview.RPC("SetLastScoreRemote", RPCMode.Others, name, email, score);
    }

    [RPC]
    public void SetLastScoreRemote(string name, string email, int score)
    {
        hsk.SetLastScore(name, email, score);
    }

    /////////////////////

    //public void UpdateTemperatureTablet(int i, float Temperature, int TemperatureDegree)
    //{
    //    if (GetComponent<NetworkView>().isMine)
    //    {
    //        GetComponent<NetworkView>().RPC("UpdateTemperatureTabletRemote", RPCMode.Others, i, Temperature, TemperatureDegree);
    //    }
    //}

    //[RPC]
    //public void UpdateTemperatureTabletRemote(int i, float Temperature, int TemperatureDegree)
    //{
    //    GameObject vpm = GameObject.FindGameObjectWithTag("DiamondGame");
    //    vpm.GetComponent<DiamondGame>().UpdateTemperatureTablet(i,Temperature, TemperatureDegree);
    //}

    public void ShowHighScoresTablet()
    {
        if (GetComponent<NetworkView>().isMine)
        {
            GetComponent<NetworkView>().RPC("ShowHighScoresTabletRemote", RPCMode.Others);
        }
    }

    [RPC]
    public void ShowHighScoresTabletRemote()
    {
        GameObject vpm = GameObject.FindGameObjectWithTag("DiamondGame");
        vpm.GetComponent<DiamondGame>().ShowHighScoresTablet();
    }

    //public void DestroyGameObjectTablet()
    //{
    //    if (GetComponent<NetworkView>().isMine)
    //    {
    //        GetComponent<NetworkView>().RPC("DestroyGameObjectTabletRemote", RPCMode.Others);
    //    }
    //}

    //[RPC]
    //public void DestroyGameObjectTabletRemote()
    //{
    //    GameObject vpm = GameObject.FindGameObjectWithTag("DiamondGame");
    //    vpm.GetComponent<DiamondGame>().DestroyGameObjectTablet();
    //}

    public void UpdateScoreTablet(int score)
    {
        if (GetComponent<NetworkView>().isMine)
        {
            GetComponent<NetworkView>().RPC("UpdateScoreTabletRemote", RPCMode.Others, score);
        }
    }

    [RPC]
    public void UpdateScoreTabletRemote(int score)
    {
        GameObject vpm = GameObject.FindGameObjectWithTag("DiamondGame");
        vpm.GetComponent<DiamondGame>().UpdateScoreTablet(score);
    }

    //public void RetryTablet()
    //{
    //    if (GetComponent<NetworkView>().isMine)
    //    {
    //        GetComponent<NetworkView>().RPC("RetryTabletRemote", RPCMode.Others);
    //    }
    //}

    //[RPC]
    //public void RetryTabletRemote()
    //{
    //    GameObject vpm = GameObject.FindGameObjectWithTag("TargetRetryGame");
    //    vpm.GetComponent<DiamondGame>().Retry();
    //}

    public void LaunchPulseTablet(int rnd_S, int rnd_L)
    {
        if (GetComponent<NetworkView>().isMine)
        {
            GetComponent<NetworkView>().RPC("LaunchPulseTabletRemote", RPCMode.Others, rnd_S, rnd_L);
        }
    }

    [RPC]
    public void LaunchPulseTabletRemote(int rnd_S, int rnd_L)
    {
        GameObject vpm = GameObject.FindGameObjectWithTag("DiamondGame");
        vpm.GetComponent<DiamondGame>().LaunchPulseTablet(rnd_S, rnd_L);
    }

    public void AlarmTablet(bool Alarm)
    {
        if (GetComponent<NetworkView>().isMine)
        {
            GetComponent<NetworkView>().RPC("AlarmTabletRemote", RPCMode.Others, Alarm);
        }
    }

    [RPC]
    public void AlarmTabletRemote(bool Alarm)
    {
        GameObject vpm = GameObject.FindGameObjectWithTag("DiamondGame");
        vpm.GetComponent<DiamondGame>().AlarmTablet(Alarm);
    }

    public void LaserCreationTablet()
    {
        if (GetComponent<NetworkView>().isMine)
        {
            GetComponent<NetworkView>().RPC("LaserCreationTabletRemote", RPCMode.Others);
        }
    }

    [RPC]
    public void LaserCreationTabletRemote()
    {
        GameObject vpm = GameObject.FindGameObjectWithTag("DiamondGame");
        vpm.GetComponent<DiamondGame>().LaserCreation();
    }

    public void DiamondCreationTablet()
    {
        if (GetComponent<NetworkView>().isMine)
        {
            GetComponent<NetworkView>().RPC("DiamondCreationTabletRemote", RPCMode.Others);
        }
    }

    [RPC]
    public void DiamondCreationTabletRemote()
    {
        GameObject vpm = GameObject.FindGameObjectWithTag("DiamondGame");
        vpm.GetComponent<DiamondGame>().DiamondCreation();
    }

    //public void UpdateTemperaturePositionTablet(float Angle)
    //{
    //    if (GetComponent<NetworkView>().isMine)
    //    {
    //        GetComponent<NetworkView>().RPC("UpdateTemperaturePositionTabletRemote", RPCMode.Others, Angle);
    //    }
    //}

    //[RPC]
    //public void UpdateTemperaturePositionTabletRemote(float Angle)
    //{
    //    GameObject vpm = GameObject.FindGameObjectWithTag("Thermometer");
    //    vpm.GetComponent<ThermometerManager>().UpdateTemperaturePositionTablet(Angle);
    //}

    public void UpdateTemperatureTablet(float Temperature, int TemperatureDegree)
    {
        if (GetComponent<NetworkView>().isMine)
        {
            GetComponent<NetworkView>().RPC("UpdateTemperatureTabletRemote", RPCMode.Others, Temperature, TemperatureDegree);
        }
    }

    [RPC]
    public void UpdateTemperatureTabletRemote(float Temperature, int TemperatureDegree)
    {
        GameObject vpm = GameObject.FindGameObjectWithTag("DiamondGame");
        vpm.GetComponent<DiamondGame>().UpdateTemperatureTablet(Temperature, TemperatureDegree);
    }
}
