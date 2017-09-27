using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class FrameKeeper : MonoBehaviour
{
    // public int numberOfFrames = 5;
    // public Texture[] frames;
    //  public bool isInitialized = false;

    // private Hashtable textureByName;

    private Dictionary<string, Texture2D> textureByResolution;
  //  private Dictionary<string, Texture2D> textureByResolutionAlt;

    private string fileDir = "";
    private string cacheDir = "";
    void Awake()
    {
        fileDir = Application.streamingAssetsPath + "/";
        cacheDir = Application.persistentDataPath + "/";
        DontDestroyOnLoad(this.gameObject);

        CacheTextures();
    }

    public int Millis()
    {
        return System.DateTime.Now.Second * 1000 + System.DateTime.Now.Millisecond;
    }


    public Texture TextureForFile(string filename)
    {


        string filePath = cacheDir + filename + ".raw";
        int width = PlayerPrefs.GetInt(filePath + "_width", 3840);
        int height = PlayerPrefs.GetInt(filePath + "_height", 3840);
        Texture2D tex = GetTexture(width, height);
        if (File.Exists(filePath))
        {
            Debug.Log("fetch " + filePath);
            tex.LoadRawTextureData(File.ReadAllBytes(filePath));
            tex.Apply(false, false);

        }
        return tex;
    }

    Texture2D GetTexture(int width, int height)
    {
        string key = width + "_" + height;
        if (textureByResolution.ContainsKey(key))
        {
            return textureByResolution[key];
        }
        else
        {
            textureByResolution.Add(key, new Texture2D(width, height, TextureFormat.RGB24, false));
            return textureByResolution[key];
        }
    }

    void CacheTextures()
    {
        if (textureByResolution == null)
        {
            textureByResolution = new Dictionary<string, Texture2D>();
        }
        string TexRes = PlayerPrefs.GetString("REQUIRED_TEXTURES", "_3840_3840_|");
        string[] AllResolutions = TexRes.Split('|');
        for (int i = 0; i < AllResolutions.Length - 1; i++)
        {
            string SubNumber = AllResolutions[i].Substring(1, AllResolutions[i].Length - 2);

            if (!textureByResolution.ContainsKey(SubNumber))
            {
                string[] Numbers = SubNumber.Split('_');
                int width = int.Parse(Numbers[0]);
                int height = int.Parse(Numbers[1]);
                textureByResolution.Add(width + "_" + height, new Texture2D(width, height, TextureFormat.RGB24, false));
            }
        }
    }

    /*
    public Texture TextureForFile(string filename)
    {
        if (textureByName == null)
            textureByName = new Hashtable();
        if (textureByName.ContainsKey(filename))
        {
            Debug.Log("FrameKeeper: Fetching " + filename + " from cache");
            return (Texture)textureByName[filename];
            //		} else {
            //			string filePath = fileDir + filename;
            //			if (File.Exists (filePath)) {
            //				long start = System.DateTime.Now.Ticks;
            ////				var url = "file://"+filePath;
            ////				Texture2D tex = new Texture2D (2, 2, TextureFormat.RGB24, false);
            //////				Texture2D tex = new Texture2D (2, 2, TextureFormat.DXT1, false);
            ////				textureByName.Add(filename,tex);
            ////				return tex;
            //
            ////				Texture2D tex = new Texture2D (4096, 4096, TextureFormat.DXT1, false);
            ////				Texture2D tex = new Texture2D (2, 2, TextureFormat.DXT1, false);
            //				// File
            //				Texture2D tex = new Texture2D (4, 4, TextureFormat.RGB24, false);
            //				tex.LoadImage (File.ReadAllBytes (filePath));
            //				textureByName.Add(filename,tex);
            //				long diff = (System.DateTime.Now.Ticks - start) / 10000;
            //				Debug.Log ("FrameKeeper: Loaded " + filename + " ("+diff+" ms)");
            //				Debug.Log ("FrameKeeper: Texture Format: " + tex.format);
            //				return tex;
        }
        else
        {
            Debug.LogError("FrameKeeper: Texture " + filename + " not found! (not in cache)");
            return null;
        }
        //		}
    }*/
    /*
    public IEnumerator TextureFromWWW(string filename)
    {
        if (textureByName == null)
            textureByName = new Hashtable();
        if (textureByName.ContainsKey(filename))
        {
            Debug.Log("FrameKeeper: Already in Cache: " + filename);
            // return (Texture) textureByName[filename];
        }
        else
        {
            long start = System.DateTime.Now.Ticks;
            Texture2D tex = new Texture2D(4, 4, TextureFormat.RGB24, false);
            string filePath = fileDir + filename;
            if (filePath.Contains("://"))
            {
                // URL
                WWW www = new WWW(filePath);
                yield return www;
                www.LoadImageIntoTexture(tex);
                textureByName.Add(filename, tex);
            }
            else
            {
                // Local path
                if (File.Exists(filePath))
                {
                    tex.LoadImage(File.ReadAllBytes(filePath));
                    textureByName.Add(filename, tex);
                }
                else
                {
                    Debug.LogError("FrameKeeper: " + filePath + " not found!");
                }
            }
            long diff = (System.DateTime.Now.Ticks - start) / 10000;
            Debug.Log("FrameKeeper: Loaded " + filename + " (" + diff + " ms)");
            Debug.Log("FrameKeeper: Texture Format: " + tex.format);
        }
    }*/



    /*
    public void Preload()
    {
        string[] filePaths = Directory.GetFiles(fileDir, "*.jpg");
        foreach (string path in filePaths)
        {
            TextureForFile(Path.GetFileName(path));
        }
    }*/

}