/// <summary>
///  This class retrieves the Bundle Version and Bundle Version Name of a Unity3D Android Application.
///  Version 1.0
///  Created by Roobr Arcade
///  http://www.roobr.com
///  RoobrArcade@gmail.com
/// </summary>
using UnityEngine;
using System.Collections;
using System;

public class apkVersion : MonoBehaviour
{
    private int _versionID;
    private string _versionName;

    public int versionID
    {
        get { return _versionID; }
    }

    public string versionName
    {
        get { return _versionName; }
    }

#if (UNITY_ANDROID && !UNITY_EDITOR)
	AndroidJavaClass pluginActivityJavaClass;
    AndroidJavaObject playerActivityContext = null;
#endif
    public static apkVersion instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        try
        {
#if (UNITY_ANDROID && !UNITY_EDITOR)
            AndroidJNI.AttachCurrentThread();
            pluginActivityJavaClass = new AndroidJavaClass("com.RoobrArcade.unityPlugin.ApkVersion");
            var actClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            playerActivityContext = actClass.GetStatic<AndroidJavaObject>("currentActivity");

            _versionID = pluginActivityJavaClass.CallStatic<int>("getApplicationVersionCode", playerActivityContext);
            _versionName = pluginActivityJavaClass.CallStatic<String>("getApplicationVersionName", playerActivityContext);
#else
			_versionID = -1;
			_versionName = "-1";
#endif
        }
        catch (Exception e)
        {
            _versionID = -1;
            _versionName = "-1";
        }

    }

}