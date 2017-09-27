using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices; // required for DllImport

public class ViewpointPreload : MonoBehaviour
{
    public string filename = "viewpoints.xml";
    public GameObject frameKeeperPrefab;
    public ProgressBar progressBar;
    public string sceneToLoad = string.Empty;
    public GameObject WelcomeMsg;
    public GameObject LookToStartMsg;
    public GameObject ProtectionMsg;
    public GameObject BlockLaserPulseMsg;
    public GameObject WatchOutBackwardMsg;
    public GameObject DiamondStartScreenPrefab;
    private GameObject DiamondStartScreen;
    public GameObject LongLaserPulsePrefab;
    private GameObject LongLaserPulse;
    public GameObject BackwardLaserPulsePrefab;
    private GameObject BackwardLaserPulse;
    public TextMesh errorLog;

    private FrameKeeper frameKeeper;
    private ViewpointXML[] viewpoints;
    private int items = 0, itemsTotal = 0;
    private string sourceDir = "";
    private string targetDir = "";

    private string fileDir = "";

    public IEnumerator Start()
    {
        WelcomeMsg.SetActive(true);
        ProtectionMsg.SetActive(false);

        sourceDir = Application.streamingAssetsPath + "/";
        targetDir = Application.persistentDataPath + "/";

#if (UNITY_ANDROID && !UNITY_EDITOR)
		fileDir = targetDir;
#else
        fileDir = sourceDir;
#endif

        Debug.Log("ViewpointPreload Video source dir: " + sourceDir);
        Debug.Log("ViewpointPreload Video target dir: " + targetDir);



        // Load XML
#if (UNITY_ANDROID && !UNITY_EDITOR)
		yield return StartCoroutine(FetchIfMissing(filename));
#endif
        ViewpointContainer vc = ViewpointContainer.Load(fileDir + filename);
        viewpoints = vc.viewpoints;

        if (viewpoints != null)
        {
            Debug.Log("ViewpointPreload: found " + viewpoints.Length + " viewpoints");
        }
        else
        {
            Debug.LogError("ViewpointPreload: failed to load viewpoint definition XML (" + filename + ")");
        }
        yield return StartCoroutine(Preload());
    }

    //
#if (UNITY_ANDROID && !UNITY_EDITOR)
	[DllImport("OculusPlugin")]
	// Set the fixed CPU clock level.
	private static extern void OVR_VrModeParms_SetCpuLevel(int cpuLevel);
	
	[DllImport("OculusPlugin")]
	// Set the fixed GPU clock level.
	private static extern void OVR_VrModeParms_SetGpuLevel(int gpuLevel);
#endif

    private IEnumerator Preload()
    {
        // Set CPU Clock to max to improve startup speed
#if (UNITY_ANDROID && !UNITY_EDITOR)
		if(VraSettings.instance.isGearVR) {
			OVR_VrModeParms_SetCpuLevel(3);
			OVR_VrModeParms_SetGpuLevel(1);
			OVRPluginEvent.Issue(RenderEventType.ResetVrModeParms);
		}
#endif

        yield return new WaitForSeconds(0.66f); // allow bar to fade in



        // Fetch game videos from JAR and store locally
        IEnumerable all_videos = GameController.all_videos.SelectMany(a => a);
        IEnumerator enumerator = all_videos.GetEnumerator();
        while (enumerator.MoveNext())
        {
            itemsTotal++;
        }
        // ProgessBar items
        items = 1;
        itemsTotal += GameController.all_videos.Length;


        foreach (string video in all_videos)
        {
            Debug.Log("ViewpointPreload FetchVideo: " + video);
#if (UNITY_ANDROID && !UNITY_EDITOR)
        			yield return StartCoroutine(FetchIfMissing(video));
#endif
            // Don't copy the videos to local storage on IOS. 
            // It's not necessary, and they might end up in the user's iCloud storage...
            ProgressBarNext();
        }


        // Fetch viewpoint videos and pre-load Image Textures
        //Debug.Log("ViewpointPreload ItemsTotal: " + itemsTotal);


        //itemsTotal += viewpoints.Length + 2;
        foreach (ViewpointXML vp in viewpoints)
        {
            if (vp.filename != null && vp.filename != "")
            {
                if (vp.isVideo)
                {
                    // Fetch Videos from JAR and store locally
                    yield return StartCoroutine(FetchIfMissing(vp.filename));
                }
                else
                {
                    yield return StartCoroutine(FetchRawTextureIfMissing(vp.filename));

                }
            }
            if (vp.rearFilename != null && vp.rearFilename != "")
            {

                yield return StartCoroutine(FetchRawTextureIfMissing(vp.rearFilename));
            }
            //ProgressBarNext();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame(); // 2 Frames
        }
        yield return StartCoroutine(FetchRawTextureIfMissing("Game_mode_temp.jpg"));
        ProgressBarNext();

        if (GameObject.Find("FrameKeeper") == null)
        {
            Debug.Log("ViewpointPreload: Instantiating FrameKeeper");
            GameObject fkObj = (GameObject)Instantiate(frameKeeperPrefab);
            fkObj.name = "FrameKeeper";
            frameKeeper = fkObj.GetComponent<FrameKeeper>();
        }
        else
        {
            Debug.Log("Existing FrameKeeper found");
            frameKeeper = GameObject.Find("FrameKeeper").GetComponent<FrameKeeper>();
        }
        AsyncOperation async;
        // Load main scene
        if (VraSettings.instance.isTablet)
            async = Application.LoadLevelAsync(sceneToLoad);
        else { 
            GameObject.Find("ProgressBar").SetActive(false);
            WelcomeMsg.SetActive(false);
            LookToStartMsg.SetActive(true);
            DiamondStartScreen = Instantiate(DiamondStartScreenPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            DiamondStartScreen.transform.position = new Vector3(0.0f, -1.0f, 2.0f);
        }
        
        
        //yield return async;

        // CPU Clock Back to Default 
#if (UNITY_ANDROID && !UNITY_EDITOR)
		if(VraSettings.instance.isGearVR) {
			OVR_VrModeParms_SetCpuLevel(2);
			OVR_VrModeParms_SetGpuLevel(2);
			OVRPluginEvent.Issue(RenderEventType.ResetVrModeParms);
		}
#endif

    }

    public void WelcomeMessageDisable()
    {
        LookToStartMsg.SetActive(false);     
    }

    public void ProtectMessageEnable()
    {
        ProtectionMsg.SetActive(true);
        StartCoroutine(BlockLaserPulse(4));
    }

    IEnumerator BlockLaserPulse(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ProtectionMsg.SetActive(false);
        DiamondStartScreen.SetActive(false);
        BlockLaserPulseMsg.SetActive(true);
        //LongLaserPulse = Instantiate(LongLaserPulsePrefab, Vector3.zero, Quaternion.identity) as GameObject;
        //LongLaserPulse.transform.position = new Vector3(-7.5f, -1.5f, 5.0f);
        //LongLaserPulse.transform.Rotate(new Vector3(0.0f, -90.0f, 0.0f));
        LongLaserPulse = GameObject.Instantiate(LongLaserPulsePrefab);
        LongLaserPulse.transform.SetParent(this.gameObject.transform);

        yield return new WaitForSeconds(seconds);
        BlockLaserPulseMsg.SetActive(false);
        LongLaserPulse.SetActive(false);
        WatchOutBackwardMsg.SetActive(true);
        //BackwardLaserPulse = Instantiate(BackwardLaserPulsePrefab, Vector3.zero, Quaternion.identity) as GameObject;
        //BackwardLaserPulse.transform.position = new Vector3(-6.0f, -1.5f, 5.0f);
        //BackwardLaserPulse.transform.Rotate(new Vector3(0.0f, -90.0f, 0.0f));
        BackwardLaserPulse = GameObject.Instantiate(BackwardLaserPulsePrefab);
        BackwardLaserPulse.transform.SetParent(this.gameObject.transform);

        yield return new WaitForSeconds(seconds);
        WatchOutBackwardMsg.SetActive(false);
        BackwardLaserPulse.SetActive(false);
        AsyncOperation async = Application.LoadLevelAsync(sceneToLoad);
    }
    private void ProgressBarNext()
    {
        if (progressBar != null)
            progressBar.progress = items / (itemsTotal * 1.0f);
        items++;
    }

    private IEnumerator FetchIfMissing(string filename)
    {
        Debug.Log("ViewpointPreload: FetchIfMissing " + filename);
        if (File.Exists(targetDir + filename))
        {
            Debug.Log("ViewpointPreload: already in target dir, skipped");
        }
        else
        {
            string url = sourceDir + filename;
#if UNITY_EDITOR
            url = @"file://" + url;
#endif
            Debug.Log("ViewpointPreload loading " + url);
            WWW www = new WWW(url);
            yield return www;
            if (www.error == null || www.error.Length == 0)
            {
                BinaryWriter writer = new BinaryWriter(File.Open(targetDir + filename, FileMode.Create));
                writer.Write(www.bytes);
                writer.Close();
                Debug.Log("ViewpointPreload: copied " + www.bytes.Length + " bytes");
            }
            else
            {
                Debug.LogError("ViewpointPreload www ERROR: " + www.error);
            }
        }
    }

    private IEnumerator FetchRawTextureIfMissing(string filename)
    {
        Debug.Log("ViewpointPreload: FetchIfMissing " + filename);
        if (File.Exists(targetDir + filename + ".raw"))
        {
            Debug.Log("ViewpointPreload: already in target dir, skipped" + targetDir + filename);
        }
        else
        {
            string url = sourceDir + filename;
#if UNITY_EDITOR
            url = @"file://" + url;
#endif
            Debug.Log("ViewpointPreload loading " + url);
            WWW www = new WWW(url);
            yield return www;
            if (www.error == null || www.error.Length == 0)
            {

                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
                www.LoadImageIntoTexture(tex);
                File.WriteAllBytes(targetDir + filename + ".raw", tex.GetRawTextureData());
                PlayerPrefs.SetInt(targetDir + filename + ".raw" + "_width", tex.width);
                PlayerPrefs.SetInt(targetDir + filename + ".raw" + "_height", tex.height);
                if (!PlayerPrefs.GetString("REQUIRED_TEXTURES", "").Contains("_" + tex.width + "_" + tex.height + "_"))
                {
                    PlayerPrefs.SetString("REQUIRED_TEXTURES", PlayerPrefs.GetString("REQUIRED_TEXTURES", "") + "_" + tex.width + "_" + tex.height + "_" + "|");
                }
                PlayerPrefs.Save();
                Debug.Log("ViewpointPreload: copied " + www.bytes.Length + " bytes" + " x:" + tex.width + " y:" + tex.height);
				Destroy(tex);
            }
            else
            {
                Debug.LogError("ViewpointPreload www ERROR: " + www.error);
            }
        }
    }


}