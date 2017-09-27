using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using UnityEngine.UI;
using TessOCR;

/* TODO

   Bei gleichem Punktestand: unten/oben/mitte/Zufall?

   Bis Fr. EOB


===

Show HighScore-List on Tablet:
- Request List-Update from Server
- Servers sends highscores
- Server calls ShowList on Sender Tablet 
   http://answers.unity3d.com/questions/371390/get-rpc-senders-id-or-ip.html
  */

public class HighScoreKeeper : MonoBehaviour
{

    public List<HighScore> highscores = new List<HighScore>();
    public TMPro.TextMeshProUGUI nrListLabelPhone;
    public TMPro.TextMeshProUGUI playerListLabelPhone;
    public TMPro.TextMeshProUGUI scoreListLabelPhone;
    public Canvas highScoreListCanvasPhone;
    public Canvas highScoreListCanvasTablet;
    public Canvas highScoreListCanvasServer; // Highscore-Server
    public Canvas newHighScoreCanvas;
    public Canvas editPlayerNameCanvas;
    //public Canvas editPlayerEmailCanvas;
    public Canvas confirmCanvas;
    public TMPro.TextMeshProUGUI newHighScoreScoreLabel;
    public UnityEngine.UI.InputField playerNameInputField;
    public UnityEngine.UI.InputField playerEmailInputField;
    public UnityEngine.UI.InputField playerNameEditField;
    public UnityEngine.UI.InputField playerEmailEditField;
    public TMPro.TextMeshProUGUI playerNameLabel;
    public TMPro.TextMeshProUGUI playerEmailLabel;

	public Toggle NameInputToggle;
	public Toggle EmailEditToggle;
	public Toggle NameEditToggle;
	public Toggle EmailInputToggle;

	private string rootPath="/storage/emulated/0/VISARD/";  
	WebCamTexture camTexture;
	public Image img;
	public int editModel=0;
	public GameObject QuickEditPanel;
	public GameObject WordEdit;
	public GameObject CameraCanvas;

    public Transform tabletScrollList;

    public List<ScrollListEntry> scrollListCache;

    public GameObject scrollListEntry;
    public RenderOnce scrollListRenderCamera;
    public string playerName = "unknown";
    public string playerEmail = "unknown";

    private string fileDir;
    private string filename = "highscores.xml";
    private static int MAX_SCORES_TO_RENDER = 100;
    private static int MIN_SCORES = 10; // minimum list size

    private long startTicks;
    private bool sendPlayerName = false;
    private HighScore lastScore = new HighScore();

    public bool highScoreListDirty = false; // do we need to render a new high score list?

    // Use this for initialization
    void Start()
    {
        fileDir = Application.persistentDataPath + "/";
        //		AddScore ("foobar", 99);
        //		SaveXML();
        // TODO
        //		if(Network.isServer) {
        //			LoadXML();
        //		}
        if (File.Exists(fileDir + filename))
        {
            LoadXML();
        }
        else
        {
            for (int n = 0; n < 100; n++)
            {
                //       AddScore("Dummy" + n, "Dummy" + n + "@Dummy.com", n - 2);  Disable dummy scores
            }
        }
        highScoreListDirty = true;
        highScoreListCanvasPhone.enabled =  false;
        highScoreListCanvasTablet.enabled = false;
        highScoreListCanvasServer.enabled = false;
        editPlayerNameCanvas.enabled = false;
        confirmCanvas.enabled = false;
        newHighScoreCanvas.enabled = false;
        //		ShowTop10();
        startTicks = System.DateTime.Now.Ticks;
        //Invoke ("TestHighScorePanel",2.0f);

        if (VraSettings.instance.isHighscoreServer)
        {
            highScoreListCanvasServer.enabled = true;
        }
    }

    public void OnPlayerConnected()
    {
        sendPlayerName = true;
    }

    float LastSaveTime;


    // Update is called once per frame
    void Update()
    {
        if (sendPlayerName == true)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("NetworkSync");
            if (obj != null)
            {
                var nws = obj.GetComponent<NetworkSync>();
                nws.SetPlayerName(playerName);
                nws.SetPlayerEmail(playerEmail);

                nws.SetLastScore(lastScore.player, lastScore.email, lastScore.score);
                sendPlayerName = false;
            }
        }
        if (highScoreListDirty)
        {
            highScoreListDirty = false;
            UpdateHighScorePanels();
        }

        if (VraSettings.instance.isHighscoreServer)
        {
            if (Time.realtimeSinceStartup - LastSaveTime > 60)
            {
                LastSaveTime = Time.realtimeSinceStartup;
                SaveXML();
            }
        }
        /*
     if(Time.frameCount % 10 == 0)
        {
            TestHighScorePanel();
        }*/
    }

    public void SetLastScore(string player, string email, int name)
    {
        lastScore = new HighScore(player, email, name);
    }

    void OnEnable()
    {
        HighscoreServerNetworkManager.OnNewScoreRecieved += NewScoreRecieved;
    }

    void OnDisable()
    {
        HighscoreServerNetworkManager.OnNewScoreRecieved -= NewScoreRecieved;
    }

    private void NewScoreRecieved(HighscoreServerNetworkManager.ScoreMessage newscore)
    {
        AddScore(newscore.playerName, newscore.playerEmail, newscore.score);
    }

    public void AddScore(string player, string email, int score)
    {
        Debug.Log("HighScoreKeeper: AddScore(" + player + "," + email + "," + score + ") (" + ((System.DateTime.Now.Ticks - startTicks) / 10000) + " ms)");
        HighScore newScore = new HighScore(player, email, score);

        highscores.Add(newScore);

        Debug.Log("HighScoreKeeper: List size: " + highscores.Count);
        highScoreListDirty = true;
    }

    // Called on phone (Server) to add a new High Score with entered name
    public void SaveHighScore(string player, string email, int newScore)
    {
        Debug.Log("HighScoreKeeper: SaveHighScore(" + player + "," + newScore + ")");
        SetPlayerName(player);
        AddScore(player, email, newScore);
        HighscoreServerNetworkManager.DoNewHighScoreByUser(newScore, player, email);
        SaveXML();
        lastScore = new HighScore(playerName, email, newScore);

        // Set Gamestate, Show HighScore List
        //GameObject obj = GameObject.Find("GameController");
        //if (obj != null)
        //{
        //    obj.GetComponent<GameController>().ShowHighScores();
        //}

        GameObject obj = GameObject.Find("DiamondGame");
        if (obj != null)
        {
            obj.GetComponent<DiamondGame>().ShowHighScores();
        }
        //ShowHighScoresPhone();
        //ShowHighScoresTablet();
        sendPlayerName = true;
        SendHighScoreListXML();
    }

    //	// Send HighScoreList to others
    //	// For large numbers of scores, this could probably optimized by sending the score
    //	// in one long string containing the serialized list (XML).
    //	public void SendHighScoreList() {
    //		// Send HighScore List to Network Players
    //		GameObject obj = GameObject.FindGameObjectWithTag ("NetworkSync");
    //		if (obj != null) {
    //			var nws = obj.GetComponent<NetworkSync> ();
    //			nws.ResetHighScores();
    //			long start = System.DateTime.Now.Ticks;
    //			foreach(var score in highscores) {
    //				Debug.Log ("HighScoreKeeper: Sending Score("+score.player+","+score.score+")");
    //				nws.AddScore (score.player, score.score);
    //			}
    //			long diff = (System.DateTime.Now.Ticks - start) / 10000;
    //			Debug.Log ("HighScoreKeeper: Sending highscores took "+diff+" ms");
    //			nws.UpdateHighScorePanels();
    //		}
    //	}
    //
    public void SendHighScoreListXML()
    {
        // Send HighScore List to Network Players
        GameObject obj = GameObject.FindGameObjectWithTag("NetworkSync");
        if (obj != null)
        {
            var nws = obj.GetComponent<NetworkSync>();
            nws.ResetHighScores();
            HighScoreContainer hsc = new HighScoreContainer();
            hsc.highscores = highscores.ToArray();
            nws.SendXML(GetBytes(hsc.SaveToText()));
            nws.UpdateHighScorePanels();
        }
    }

    public string GetHighScoreListXML() // Prepare highscore list for sending from highscore server to all phones
    {
        HighScoreContainer hsc = new HighScoreContainer();
        hsc.highscores = highscores.ToArray();
        return hsc.SaveToText();
    }

    public void RequestHighScoreList()
    {
        startTicks = System.DateTime.Now.Ticks;
        Debug.Log("HighScoreKeeper: RequestHighScoreList (" + Time.time * 1000 + ")");
        GameObject obj = GameObject.FindGameObjectWithTag("NetworkSync");
        if (obj != null)
        {
            var nws = obj.GetComponent<NetworkSync>();
            nws.RequestHighScoreList();
        }
        highScoreListCanvasTablet.enabled = true;
        UpdateHighScorePanels(); // Show cached entries.
    }

    public bool isNewHighScore(int score)
    {
        return true;
    }

    // Called on tablet to open player name input window
    public void NewHighScore(int score)
    {
        Debug.Log("HighScoreKeeper: NewHighScore");
        if (!newHighScoreCanvas.enabled)
        {
            playerNameInputField.text = playerName;
            newHighScoreCanvas.enabled = true;
            newHighScoreScoreLabel.text = "" + score;
        }
    }

    public void ClosePanels()
    {
        highScoreListCanvasPhone.enabled = false;
        highScoreListCanvasTablet.enabled = false;
        confirmCanvas.enabled = false;
        newHighScoreCanvas.enabled = false;
        //   scrollListRenderCamera.StopRendering();
    }

    // Called on Tablet to push Score to phone
    public void HighScoreButtonOK()
    {
        newHighScoreCanvas.enabled = false;
        int score = System.Int32.Parse(newHighScoreScoreLabel.text);
        SetPlayerName(playerNameInputField.text);
        SetPlayerEmail(playerEmailInputField.text);
        Debug.Log("HighScoreKeeper: HighScoreButtonSave(" + playerName + "," + score + ")");
        GameObject obj = GameObject.FindGameObjectWithTag("NetworkSync");
        if (obj != null)
        {
            var nws = obj.GetComponent<NetworkSync>();
            nws.SaveHighScore(playerName, playerEmail, score);
            lastScore = new HighScore(playerName, playerEmail, score);
        }
    }

    public void EditPlayerName()
    {
        Debug.Log("HighScoreKeeper: EditPlayerName");
        if (playerName != "unknown")
            playerNameEditField.text = playerName;
        else
            playerNameEditField.text = "";
        editPlayerNameCanvas.enabled = true;

        if (playerEmail != "unknown")
            playerEmailEditField.text = playerEmail;
        else
            playerEmailEditField.text = "";
    }

    public void EditPlayerNameButtonOK()
    {
        Debug.Log("HighScoreKeeper: EditPlayerNameButtonOK");
        editPlayerNameCanvas.enabled = false;
        SetPlayerName(playerNameEditField.text);
        SetPlayerEmail(playerEmailEditField.text);
        Debug.Log("HighScoreKeeper: EditPlayerNameButtonOK, new name: " + playerName);
        GameObject obj = GameObject.FindGameObjectWithTag("NetworkSync");
        if (obj != null)
        {
            var nws = obj.GetComponent<NetworkSync>();
            nws.SetPlayerName(playerName);
            nws.SetPlayerEmail(playerEmail);
        }
    }



    public void SetPlayerName(string name)
    {
        if (!editPlayerNameCanvas.enabled && !newHighScoreCanvas.enabled)
        {
            playerName = name;
            if (string.IsNullOrEmpty(playerName))
                playerName = "unknown";
        }
        if (playerNameLabel != null)
        {
            playerNameLabel.text = "Name: " + playerName;
        }
    }

	


    public void SetPlayerEmail(string email)
    {
        if (!editPlayerNameCanvas.enabled && !newHighScoreCanvas.enabled)
        {
            playerEmail = email;
            if (playerEmail == null || playerEmail.Length == 0)
                playerEmail = "unknown";
        }
        if (playerEmailLabel != null)
        {
            playerEmailLabel.text = "Email: " + playerEmail;
        }
    }
    

    public void ShowHighScoresTablet()
    {
        Debug.Log("HighScoreKeeper: ShowHighScoresTablet (" + Time.time * 1000 + ")");
        UpdateHighScorePanels();
        highScoreListCanvasTablet.enabled = true;
    }

    public void ShowHighScoresPhone()
    {
        highScoreListCanvasPhone.enabled = true;
        UpdatePhoneHighScorePanel();
    }

    public void UpdateHighScorePanels()
    {
        if (highScoreListCanvasPhone.enabled)
            UpdatePhoneHighScorePanel();
        if (highScoreListCanvasTablet.enabled)
            UpdateTabletHighScorePanel();
        if (highScoreListCanvasServer.enabled)
            UpdateServerHighScorePanel();
    }



    public void TestHighScorePanel()
    {
        // highScoreListCanvasPhone.enabled = true;
        lastScore = new HighScore("Dummyxxx1", "Dummyxxx1", 1);
        highscores.Add(lastScore);
        UpdateHighScorePanels();
    }

    public void UpdatePhoneHighScorePanel()
    {
        Debug.Log("HighScoreKeeper: UpdatePhoneHighScorePanel");
        string nrList = "";
        string playerList = "";
        string scoreList = "";
        string colorPrefix = "<#ff0000>";
        string colorSuffix = "</color>";
        var sortedScores = (from s in highscores orderby s.score descending select s).ToList<HighScore>();
        int lastScoreNr = -1;
        int i = 1;
        foreach (HighScore score in sortedScores)
        {
            if (score.player == lastScore.player && score.score == lastScore.score)
            {
                lastScoreNr = i;
                break;
            }
            i++;
        }
        Debug.Log("HighScoreKeeper: Found lastScore at " + lastScoreNr);
        int nr = 1;
        foreach (HighScore score in sortedScores)
        {
            if (score.player == lastScore.player && score.score == lastScore.score)
            {
                nrList += colorPrefix + nr + "." + colorSuffix + "\n";
                playerList += colorPrefix + score.player + colorSuffix + "\n";
                scoreList += colorPrefix + score.score + colorSuffix + "\n";
            }
            else
            {
                nrList += nr + "." + "\n";
                playerList += score.player + "\n";
                scoreList += score.score + "\n";
            }
            nr++;
            if (nr > 20 || (nr > 10 && lastScoreNr > 20))
                break;
        }
        // Lower half shows lastScore
        if (nr < 20 && lastScoreNr > 20)
        {
            nrList += "...\n";
            playerList += "...\n";
            scoreList += "...\n";
            int startNr = lastScoreNr - 4 - 1;
            if (startNr > (sortedScores.Count() - 9))
                startNr = (sortedScores.Count() - 9);
            for (int j = 0; j < 9; j++)
            {
                HighScore score = sortedScores.ElementAt(startNr + j);
                nr = startNr + j + 1;
                if (score.player == lastScore.player && score.score == lastScore.score)
                {
                    nrList += colorPrefix + nr + "." + colorSuffix + "\n";
                    playerList += colorPrefix + score.player + colorSuffix + "\n";
                    scoreList += colorPrefix + score.score + colorSuffix + "\n";
                }
                else
                {
                    nrList += nr + "." + "\n";
                    playerList += score.player + "\n";
                    scoreList += score.score + "\n";
                }
            }
        }
        // Make sure the list contains at least MIN_SCORES entries.
        if (sortedScores.Count() < MIN_SCORES)
        {
            for (int n = sortedScores.Count(); n < MIN_SCORES; n++)
            {
                nrList += n + "." + "\n";
                playerList += "-\n";
                scoreList += "-\n";
            }
        }
        nrListLabelPhone.text = nrList;
        playerListLabelPhone.text = playerList;
        scoreListLabelPhone.text = scoreList;
    }

    public void UpdateTabletHighScorePanel()
    {
        if (scrollListCache == null)
        {
            scrollListCache = new List<ScrollListEntry>();
        }

        var sortedScores = from s in highscores orderby s.score descending select s;
        int nr = 1;
        // Make sure the list contains at least MIN_SCORES entries.
        for (int n = scrollListCache.Count; n < Mathf.Clamp(sortedScores.Count(),MIN_SCORES,MAX_SCORES_TO_RENDER); n++)
        {
            GameObject newline = GameObject.Instantiate(scrollListEntry);
            ScrollListEntry entry = newline.GetComponent<ScrollListEntry>();
            newline.transform.SetParent(tabletScrollList, false);
            scrollListCache.Add(entry);
        }

        Debug.Log("HighScoreKeeper: UpdateTabletHighScorePanel w/ cache size " + scrollListCache.Count);

        int i = 0;
        foreach (HighScore score in sortedScores)
        {
            if (i < 100)
            {
                scrollListCache[i].transform.SetAsLastSibling();

                scrollListCache[i].SetScore((nr++) + ".", score.player, score.score.ToString());
                i++;
            }

        }

        // double check other entries are blank and in place;
        for (int n = i; n < scrollListCache.Count; n++)
        {
            scrollListCache[n].transform.SetAsLastSibling();
            scrollListCache[n].SetScore((nr++) + ".", "-", "-");
        }

        scrollListRenderCamera.RenderAtSize(Mathf.Max(sortedScores.Count(), MIN_SCORES) * 40);
    }

    public HighScoreServerEntry[] ServerListEntries;

    public void UpdateServerHighScorePanel()
    {


        var sortedScores = from s in highscores orderby s.score descending select s;
        int nr = 0;
        foreach (HighScore score in sortedScores)
        {
            if (nr < 9)
            {
                ServerListEntries[nr].SetScore((nr + 1) + ".    " + score.player, score.score.ToString());
                nr++;
            }
            else if (nr < 10)
            {
                ServerListEntries[nr].SetScore((nr + 1) + ".  " + score.player, score.score.ToString());
                nr++;
            }

        }
        while (nr < 10)
        {
            ServerListEntries[nr].SetScore((nr + 1) + ".  ", "");
            nr++;
        }
    }


    public void ResetHighScores()
    {
        Debug.Log("HighScoreKeeper: ResetHighScores");
        if (File.Exists(fileDir + filename))
        {
            string newname = "highscores_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml";
            Debug.Log("HighScoreKeeper: Keeping old list in " + newname);
            File.Copy(fileDir + filename, fileDir + newname, true);
        }
        highscores = new List<HighScore>();
        if (Network.isServer)
        {
            SaveXML();
        }
    }

    public void ResetHighScoreButton()
    {
        Debug.Log("HighScoreKeeper: ResetHighScoreButton");
        confirmCanvas.enabled = true;
    }

    public void CancelConfirmButton()
    {
        Debug.Log("HighScoreKeeper: CancelConfirmButton");
        confirmCanvas.enabled = false;
    }

    public void ResetConfirmButton()
    {
        GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
        if (nws != null)
        {
            nws.GetComponent<NetworkSync>().ResetHighScores();
            confirmCanvas.enabled = false;
            RequestHighScoreList();
        }
    }


    public void ButtonServerResetHighScores()
    {
        highscores = new List<HighScore>();
        SaveXML();
        highScoreListDirty = true;
    }

    public void LoadXML(byte[] bytes)
    {
        LoadXML(GetString(bytes));
    }

    public void LoadXML(string text)
    {
        Debug.Log("LoadXML: (" + text.Length + " chars)\n" + text);
        HighScoreContainer hsc = HighScoreContainer.LoadFromText(text);
        Debug.Log("HighScoreKeeper found " + hsc.highscores.Count() + " highscores");
        highscores = new List<HighScore>();
        foreach (HighScore score in hsc.highscores)
        {
            highscores.Add(score);
        }
        if (VraSettings.instance.isHeadset)
        {
            SaveXML();
            SendHighScoreListXML();
        }
    }


    public void LoadXML()
    {

        if (File.Exists(fileDir + filename))
        {
            HighScoreContainer hsc = HighScoreContainer.Load(fileDir + filename);
            Debug.Log("HighScoreKeeper found " + hsc.highscores.Count() + " highscores");
            highscores = new List<HighScore>();
            foreach (HighScore score in hsc.highscores)
            {
                highscores.Add(score);
            }


        }
        else
        {
            Debug.LogError("HighScoreKeeper: " + fileDir + filename + " not found!");
        }
    }

    public void SaveXML()
    {
        Debug.Log("HighScoreKeeper: Saving HighScores to " + fileDir + filename);
        HighScoreContainer hsc = new HighScoreContainer();
        hsc.highscores = highscores.ToArray();
        hsc.Save(fileDir + filename);
    }

    static byte[] GetBytes(string str)
    {
        byte[] bytes = new byte[str.Length * sizeof(char)];
        System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        return bytes;
    }

    static string GetString(byte[] bytes)
    {
        char[] chars = new char[bytes.Length / sizeof(char)];
        System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
        return new string(chars);
    }

    void OnApplicationQuit()
    {
        SaveXML();
    }

    public void ButtonQuitApp()
    {
        Application.Quit();
    }

    public void ShareHighScoreTextFile()
    {

        string Path = Application.persistentDataPath + "/ScoreList.txt";


        if (File.Exists(Path))
        {
            File.Delete(Path);
        }
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("HIGH SCORE LIST:");
        for (int i = 0; i < highscores.Count; i++)
        {
            builder.Append(i + 1);
            builder.Append(",");
            builder.Append(highscores[i].score);
            builder.Append(",");
            builder.Append(highscores[i].player);
            builder.Append(",");
            builder.AppendLine(highscores[i].email);
        }

        System.IO.File.WriteAllText(Path, builder.ToString());

        //   Debug.Log(@"file://" + Path);
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
        AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
        AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + Path);
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
        intentObject.Call<AndroidJavaObject>("setType", "image/png");


        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "High Score List");
        currentActivity.Call("startActivity", jChooser);
    }

	public void EditPlayerNameButtonOpenCamera ()
	{
		HideAllElements ();

		StartCoroutine (CallCamera ());
	}

	/// please hide all elemnets and show camera in this function
	public void HideAllElements(){
		CameraCanvas.SetActive (true);
        //newHighScoreCanvas.GetComponent<Canvas>
	}

	/// please show edit panel and hide camera in this function
	public void ShowEditPanel(){  
		CameraCanvas.SetActive (false);
	}

	void SetPath (string path)
	{
		rootPath =path;
	}

	IEnumerator CallCamera ()
	{
		yield return Application.RequestUserAuthorization (UserAuthorization.WebCam);
		if (Application.HasUserAuthorization (UserAuthorization.WebCam)) {
			if (camTexture != null)
				camTexture.Stop ();

			WebCamDevice [] cameraDevices = WebCamTexture.devices;
			string deviceName = cameraDevices [0].name; 
			camTexture = new WebCamTexture(deviceName, Screen.width, Screen.height, 60); 
			img.canvasRenderer.SetTexture (camTexture);
			camTexture.Play ();
		}
	}
	public void TakePhoto(){
		StartCoroutine (TakePhotoAndSave ());
	}
	public IEnumerator TakePhotoAndSave ()
	{
		yield return new WaitForEndOfFrame ();
		Texture2D t; 
		t = new Texture2D (camTexture.width, camTexture.height); 
		t.SetPixels (camTexture.GetPixels ()); 
		t.Apply ();

		byte [] byt = t.EncodeToJPG ();
		var time = Time.time.ToString ();
		time = time.Replace (".", "");
		string imgName = time + ".jpg";
		var imgPath = rootPath + imgName;
		File.WriteAllBytes (imgPath, byt);
		camTexture.Stop ();

		Debug.Log (imgPath); 
		GetRecognizeText (rootPath, imgPath, imgName); 
	}
	public void GetRecognizeText (string root, string imgPath, string filename)
	{
		//HideAllElements ();
		ShowEditPanel ();
		string bcname = string.Empty;
		string bcemail = string.Empty;

		if (!TesseractAPI.Inited) {
			TesseractAPI.Init (root, "eng");
			//Notice.text = imgPath;
		}

		if (TesseractAPI.Inited) {

			List<TextLine> textLines = null;
			///get string from plugin api
			var text = TesseractAPI.Getutf8String (imgPath, true);

			///decode json array to TextData object, TextData.TextLine is a array
			textLines = Utils.GetTextLine (text);

			if (textLines != null) {
				///remove the quick edit canvas's children if it have
				QuickEditPanel.transform.DetachChildren ();

				///  find email by @
				foreach (var line in textLines) {
					var email = Utils.checkEmail (line.Text);
					if (!string.IsNullOrEmpty (email)) {
						///some time the dot . will decode to ` ' ` 
						if (email.Contains ("'"))
							email = email.Replace ("'", ".");
						if (email.Contains ("`"))
							email = email.Replace ("`", ".");
						if (email.Contains (","))
							email = email.Replace (",", ".");
                        playerEmail = email;
                        SetEmail();
						bcemail = email;
						break;
					}

				}
				///find name by email
				if (!string.IsNullOrEmpty (bcemail)) {
					foreach (var line in textLines) {
						bcname = Utils.checkName (bcemail, line.Text);
						if (!string.IsNullOrEmpty (bcname) && !bcname.Contains("@")) {
                            playerName = bcname;
                            SetName();
							break;
						}

					}
					//bcname = bcemail.ToUpper();
				}

                QuickEditPanel.SetActive(true);
				/// splite words
				foreach (var item in textLines) {
					var ef = string.IsNullOrEmpty (bcemail) ? false : true;
					SpliteWords (item.Text, false, ef);
				}
			} else {
				//NameText.text = "Json faild";
				//EmailText.text = "json faild";
			}
		}

		//add the @ . com to quick edit
		SpliteWords ("@", true);
		SpliteWords (".", true);
		SpliteWords ("com", true);

		/// load the card image , if the picture save as image1.jpg, the card picture's name is vimage1.jpg
		/// card image means only have the card area by clip and perspective transform
		//StartCoroutine (loadTexture (root + "v" + filename));

	}
	//splite word from text line 
	public void SpliteWords (string text, bool add = false, bool findemail = true)
	{

		if (add) {
			AddQuickEditWordButton (text);
			return;
		}
		if (!string.IsNullOrEmpty (text)) {
			if (text.Contains (" ") || text.Contains (".")) {
				var words = text.Split (' ', '.');
				foreach (var w in words) {
					if (Utils.IsWord (w, findemail)) {
						AddQuickEditWordButton (w);
					}
				}
			} else if (text.Length >= 2 && Utils.IsWord (text, findemail)) {
				AddQuickEditWordButton (text);
			}
		}

	}
	//add a button with word to panel for quick edit
	void AddQuickEditWordButton (string text)
	{
		var wb = Instantiate (WordEdit);
		wb.transform.parent = QuickEditPanel.transform;
		var wbtn = wb.GetComponent<Button> ();

		var t = wbtn.transform.Find ("Text");
		var tt = t.GetComponent<Text> ();
		tt.text = text;
		wbtn.onClick.AddListener (delegate () { onTextChoose (tt.text); });
	}
	//load picture by www
	IEnumerator loadTexture (string imgpath)
	{
		WWW www = new WWW ("file://" + imgpath);
		yield return www;
		if (www != null && string.IsNullOrEmpty (www.error)) {
			var texture = www.texture;
			var sp = Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), new Vector2 (0.5f, 0.5f));
		  
			//Card.sprite = sp; 
		}
	}

	//quick edit name or email
	public void onTextChoose (string text)
	{
      
		if (editModel == 1) {
			if (playerName.Length > 0)
                playerName += " " + text;
			else playerName = text;
		} 
		else if (editModel == 2) {
			if (playerEmail.Length > 0)
                playerEmail += text;
			else playerEmail = text;
		} 
		else if (editModel == 3) {
			
			if (playerName.Length > 0)
				playerName += " " + text;
			else playerName = text;

			if (playerEmail.Length > 0)
                playerEmail += text;
			else playerEmail = text;
		}
        SetName();
        SetEmail();
    }


	public void SetQuickEditMode ()
	{
		if(editPlayerNameCanvas.enabled && !newHighScoreCanvas.enabled){
			if (NameEditToggle.isOn && NameEditToggle.isOn) {

				editModel = 3;

			} else if (NameEditToggle.isOn) {
				editModel = 1;
			} else if (EmailEditToggle.isOn) {
				editModel = 2;
			} else {
				editModel = 0;
			}
		}
		else if(newHighScoreCanvas.enabled && !editPlayerNameCanvas.enabled){
			if (NameEditToggle.isOn && NameEditToggle.isOn) {

				editModel = 3;

			} else if (NameInputToggle.isOn) {
				editModel = 1;
			} else if (EmailInputToggle.isOn) {
				editModel = 2;
			} else {
				editModel = 0;
			}
		}

	}

	public void QuickDeleteName(){

		QuickDeleteWord (1);
	}

	public void QuickDeleteEmail(){
		QuickDeleteWord (2);
	}

	///quick delete word
	void QuickDeleteWord (int v)
	{
        var na = playerName;
		var em = playerEmail;
		if (v == 1 && na.Length > 0) {
			var text = na;
			var n = text.Split (' ');
			if (n.Length > 1)
				playerName = text.Replace (" " + n [n.Length - 1], "");
			else playerName = "";

		} else if (v == 2) {
			var text = playerEmail;
			if (text.Length > 0) {
				var n = text.Split ('.', '@');
				var l = n [n.Length - 1];
				if (n.Length > 1) {
					if (text.Contains ("." + l))
                        playerEmail = text.Replace ("." + l, "");
					else if (text.Contains ("@" + l))
                        playerEmail = text.Replace ("@" + l, "");
					else playerEmail = text.Replace (l, "");
				} else playerEmail = "";
			}
		}
        SetName();
        SetEmail();
	}
    void SetName()
    {
        if (editPlayerNameCanvas.enabled && !newHighScoreCanvas.enabled)
        {
            playerNameEditField.text = playerName;
        }
        else
        {
            playerNameInputField.text = playerName;
        }
    }
    void SetEmail()
    {
        if (editPlayerNameCanvas.enabled && !newHighScoreCanvas.enabled)
        {
            playerEmailEditField.text = playerEmail;
        }
        else
        {
            playerEmailInputField.text = playerEmail;
        }
    }

}

