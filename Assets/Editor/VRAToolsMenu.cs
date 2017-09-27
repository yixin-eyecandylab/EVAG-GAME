using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Text.RegularExpressions;

// TODO: Copy contents of StreamingAssetsCommon and [StreamingAssetsPhone|StreamingAssetsTablet] into StreamingAssets folder

public class MenuItems
{
    [MenuItem("VRA-Build/Build only for GearVR and Tablet")]
    private static void BuildOnlyGearVRAndTablet()
    {
        //use the current bundle version also for the tablet, so keep it!
        string bundleVersion = PlayerSettings.bundleVersion;

        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCommon", "Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsPhone", "Assets/StreamingAssets");

        BuildGearVR(BuildOptions.ShowBuiltPlayer);

        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCommon", "Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsTablet", "Assets/StreamingAssets");

        BuildTablet(BuildOptions.ShowBuiltPlayer, bundleVersion);
    }

    [MenuItem("VRA-Build/GearVR/Build only")]
    private static void BuildOnlyGearVROption()
    {
        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCommon", "Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsPhone", "Assets/StreamingAssets");

        BuildGearVR(BuildOptions.ShowBuiltPlayer);
    }

    [MenuItem("VRA-Build/GearVR/Build only without Videos")]
    private static void BuildOnlyGearVROptionWA()
    {
        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCommon", "Assets/StreamingAssets");

        BuildGearVR(BuildOptions.ShowBuiltPlayer);
    }

    [MenuItem("VRA-Build/GearVR/Build+Run")]
    private static void BuildGearVROption()
    {
        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCommon", "Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsPhone", "Assets/StreamingAssets");

        BuildGearVR(BuildOptions.AutoRunPlayer);
    }

    [MenuItem("VRA-Build/GearVR/Build+Run without Videos")]
    private static void BuildGearVROptionWA()
    {
        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCommon", "Assets/StreamingAssets");

        BuildGearVR(BuildOptions.AutoRunPlayer);
    }

    private static void BuildGearVR(BuildOptions buildOpts)
    {
        string cleanedProductName = "EWAG LLU GAME_GearVR";//Regex.Replace(PlayerSettings.productName, "[^a-zA-Z_0-9]", "_");
        //remove "VRA" in front of cleaned name so it does not appear twice in file- and bundle names
        //if (cleanedProductName.StartsWith("VRA_"))
        //{
        //    cleanedProductName = cleanedProductName.Substring(4);
        //}
        PlayerSettings.productName = "EWAG LLU GAME_GearVR";
        //PlayerSettings.bundleIdentifier = "com.Comberry.VRA_" + cleanedProductName + "_GearVR";
        PlayerSettings.bundleIdentifier = "com.Comberry.EWAG_LLU_GAME_GearVR";
        string[] levels = new string[] {"Assets/Scenes/Splash_Screen.unity",
            "Assets/Scenes/Viewpoints.unity" };

        // Save settings for re-build.
        EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[2];
        newSettings[0] = new EditorBuildSettingsScene(levels[0], true);
        newSettings[1] = new EditorBuildSettingsScene(levels[1], true);
        EditorBuildSettings.scenes = newSettings;

        // Copy over the correct AndroidManifest
        string src = "Assets/Plugins/Android/AndroidManifest-Gear.xml";
        string dst = "Assets/Plugins/Android/AndroidManifest.xml";
        File.Copy(src, dst, true);

        // Start build
        //BuildPipeline.BuildPlayer(levels, "Builds/VRA-" + cleanedProductName + "-GearVR.apk", BuildTarget.Android, buildOpts);
        BuildPipeline.BuildPlayer(levels, "Builds/EWAG-LLU-GAME-GearVR.apk", BuildTarget.Android, buildOpts);
    }




    [MenuItem("VRA-Build/Cardboard/Build only")]
    private static void BuildOnlyCardboardOption()
    {
        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        //		CopyDir("External_Assets/StreamingAssetsCommon","Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCardboard", "Assets/StreamingAssets");
        BuildCardboard(BuildOptions.ShowBuiltPlayer);
    }

    [MenuItem("VRA-Build/Cardboard/Build only without Videos")]
    private static void BuildOnlyCardboardOptionWA()
    {
        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCardboardNoVideo", "Assets/StreamingAssets");
        //		CopyDir("External_Assets/StreamingAssetsCommon","Assets/StreamingAssets");
        BuildCardboard(BuildOptions.ShowBuiltPlayer);
    }

    [MenuItem("VRA-Build/Cardboard/Build only for iOS")]
    private static void BuildCardboardIOSOption()
    {
        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        //		CopyDir("External_Assets/StreamingAssetsCommon","Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCardboardIOS", "Assets/StreamingAssets");

        string cleanedProductName = Regex.Replace(PlayerSettings.productName, "[^a-zA-Z_0-9]", "_");
        //remove "VRA" in front of cleaned name so it does not appear twice in file- and bundle names
        if (cleanedProductName.StartsWith("VRA_"))
        {
            cleanedProductName = cleanedProductName.Substring(4);
        }

        PlayerSettings.bundleIdentifier = "com.Comberry.VRA_" + cleanedProductName + "_Cardboard";
        string[] levels = new string[] { "Assets/Scenes/Splash_Screen_Cardboard.unity",
            "Assets/Scenes/Viewpoints_Cardboard.unity" };

        // Save settings for re-build.
        EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[2];
        newSettings[0] = new EditorBuildSettingsScene(levels[0], true);
        newSettings[1] = new EditorBuildSettingsScene(levels[1], true);
        EditorBuildSettings.scenes = newSettings;

        // Start build
        BuildPipeline.BuildPlayer(levels, "Builds/VRA-" + cleanedProductName + "-XCode/", BuildTarget.iOS, BuildOptions.ShowBuiltPlayer);
    }

    [MenuItem("VRA-Build/Cardboard/Build+Run")]
    private static void BuildCardboardOption()
    {
        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        //		CopyDir("External_Assets/StreamingAssetsCommon","Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCardboard", "Assets/StreamingAssets");
        BuildCardboard(BuildOptions.AutoRunPlayer);
    }

    [MenuItem("VRA-Build/Cardboard/Build+Run without Videos")]
    private static void BuildCardboardOptionWA()
    {
        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCardboardNoVideo", "Assets/StreamingAssets");
        //		CopyDir("External_Assets/StreamingAssetsCommon","Assets/StreamingAssets");
        BuildCardboard(BuildOptions.AutoRunPlayer);
    }

    private static void BuildCardboard(BuildOptions opts)
    {
        string cleanedProductName = Regex.Replace(PlayerSettings.productName, "[^a-zA-Z_0-9]", "_");
        //remove "VRA" in front of cleaned name so it does not appear twice in file- and bundle names
        if (cleanedProductName.StartsWith("VRA_"))
        {
            cleanedProductName = cleanedProductName.Substring(4);
        }

        PlayerSettings.bundleIdentifier = "com.Comberry.VRA_" + cleanedProductName + "_Cardboard";
        string[] levels = new string[] { "Assets/Scenes/Splash_Screen_Cardboard.unity",
                "Assets/Scenes/Viewpoints_Cardboard.unity" };

        // Save settings for re-build.
        EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[2];
        newSettings[0] = new EditorBuildSettingsScene(levels[0], true);
        newSettings[1] = new EditorBuildSettingsScene(levels[1], true);
        EditorBuildSettings.scenes = newSettings;

        // Copy over the correct AndroidManifest
        string src = "Assets/Plugins/Android/AndroidManifest-Cardboard.xml";
        string dst = "Assets/Plugins/Android/AndroidManifest.xml";
        File.Copy(src, dst, true);

        // Start build
        BuildPipeline.BuildPlayer(levels, "Builds/VRA-" + cleanedProductName + "-Cardboard.apk", BuildTarget.Android, opts);
    }




    [MenuItem("VRA-Build/Tablet/Build only")]
    private static void BuildOnlyTabletOption()
    {
        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCommon", "Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsTablet", "Assets/StreamingAssets");

        BuildTablet(BuildOptions.ShowBuiltPlayer);
    }

    [MenuItem("VRA-Build/Tablet/Build only without Videos")]
    private static void BuildOnlyTabletOptionWA()
    {
        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCommon", "Assets/StreamingAssets");

        BuildTablet(BuildOptions.ShowBuiltPlayer);
    }

    [MenuItem("VRA-Build/Tablet/Build+Run")]
    private static void BuildTabletOption()
    {
        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCommon", "Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsTablet", "Assets/StreamingAssets");

        BuildTablet(BuildOptions.AutoRunPlayer);
    }

    [MenuItem("VRA-Build/Tablet/Build+Run without Videos")]
    private static void BuildTabletOptionWA()
    {
        // Push content into StreamingAssets directory 
        PurgeDir("Assets/StreamingAssets");
        CopyDir("External_Assets/StreamingAssetsCommon", "Assets/StreamingAssets");

        BuildTablet(BuildOptions.AutoRunPlayer);
    }

    /// <summary>
    /// Prepends a "Tablet" in front of the apps name so it does not interfere with the VR-apps.
    /// The product name does not have to be changed extra for building the tablet version.
    /// </summary>
    private static void BuildTablet(BuildOptions opts, string overrideBundleVersion = null)
    {
        string productName = PlayerSettings.productName;
        string cleanedProductName = Regex.Replace(PlayerSettings.productName, "[^a-zA-Z_0-9]", "_");
        //remove "VRA" in front of cleaned name so it does not appear twice in file- and bundle names
        if (cleanedProductName.StartsWith("VRA_"))
        {
            cleanedProductName = cleanedProductName.Substring(4);
        }

        //PlayerSettings.productName = "Tablet " + productName;
        PlayerSettings.productName = "EWAG LLU GAME_Tablet";
        string bundleVersion = PlayerSettings.bundleVersion;

        if (overrideBundleVersion != null && overrideBundleVersion != "")
        {
            PlayerSettings.bundleVersion = overrideBundleVersion + "t"; // add a "t" to the bundle version, to indicate, that this is a tablet build
        }

        //PlayerSettings.bundleIdentifier = "com.Comberry.Tablet_VRA_" + cleanedProductName;
        PlayerSettings.bundleIdentifier = "com.Comberry.EWAG_LLU_GAME_Tablet";
        string[] levels = new string[] {"Assets/Scenes/Splash_Screen_Tablet.unity",
            "Assets/Scenes/Network_Observer.unity" };

        // Save settings for re-build.
        EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[2];
        newSettings[0] = new EditorBuildSettingsScene(levels[0], true);
        newSettings[1] = new EditorBuildSettingsScene(levels[1], true);
        EditorBuildSettings.scenes = newSettings;

        // Copy over the correct AndroidManifest
        string src = "Assets/Plugins/Android/AndroidManifest-Tablet.xml";
        string dst = "Assets/Plugins/Android/AndroidManifest.xml";
        File.Copy(src, dst, true);

        // Start build
        //BuildPipeline.BuildPlayer(levels, "Builds/Tablet-VRA-" + cleanedProductName + ".apk", BuildTarget.Android, opts);
        BuildPipeline.BuildPlayer(levels, "Builds/EWAG-LLU-GAME-Tablet.apk", BuildTarget.Android, opts);
        //// Reset product name to what it was originally
        //PlayerSettings.productName = productName;
        //PlayerSettings.bundleIdentifier = "com.Comberry.VRA_" + cleanedProductName;

        ////only reset bundle version if it was overridden
        //if (overrideBundleVersion != null && overrideBundleVersion != "")
        //{
        //    PlayerSettings.bundleVersion = bundleVersion;
        //}
    }

    [MenuItem("VRA-Build/Image Viewer/Build+Run")]
    private static void BuildImageViewerOption()
    {
        string productName = PlayerSettings.productName;
        string cleanedProductName = Regex.Replace(PlayerSettings.productName, "[^a-zA-Z_0-9]", "_");
        //remove "VRA" in front of cleaned name so it does not appear twice in file- and bundle names
        if (cleanedProductName.StartsWith("VRA_"))
        {
            cleanedProductName = cleanedProductName.Substring(4);
        }

        PlayerSettings.productName = "ImageViewer " + productName;
        string bundleVersion = PlayerSettings.bundleVersion;
        PlayerSettings.bundleVersion = bundleVersion + "i"; // add a "i" to the bundle version, to indicate, that this is an image viewer build
        PlayerSettings.bundleIdentifier = "com.Comberry.ImageViewer_VRA_" + cleanedProductName;
        string[] levels = new string[] { "Assets/Scenes/Image_Viewer.unity" };

        // Save settings for re-build.
        EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[1];
        newSettings[0] = new EditorBuildSettingsScene(levels[0], true);
        EditorBuildSettings.scenes = newSettings;

        // Copy over the correct AndroidManifest
        string src = "Assets/Plugins/Android/AndroidManifest-Gear.xml";
        string dst = "Assets/Plugins/Android/AndroidManifest.xml";
        File.Copy(src, dst, true);

        // Start build
        BuildPipeline.BuildPlayer(levels, "Builds/ImageViewer-VRA-" + cleanedProductName + ".apk", BuildTarget.Android, BuildOptions.AutoRunPlayer);

        // Reset product name to what it was originally
        PlayerSettings.productName = productName;
        PlayerSettings.bundleVersion = bundleVersion;
        PlayerSettings.bundleIdentifier = "com.Comberry.VRA_" + cleanedProductName;
    }


    [MenuItem("VRA-Build/Highscore Server/Build+Run")]
    private static void BuildHighscoreServer()
    {
        PlayerSettings.productName = "EWAG LLU High Score Server";
        PlayerSettings.bundleIdentifier = "com.Comberry.EWAG_LLU_High_Score_Server";
        string[] levels = new string[] { "Assets/Scenes/Highscore_Server.unity" };
        PurgeDir("Assets/StreamingAssets");
        // Save settings for re-build.
        EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[1];
        newSettings[0] = new EditorBuildSettingsScene(levels[0], true);
        EditorBuildSettings.scenes = newSettings;

        // Copy over the correct AndroidManifest
        string src = "Assets/Plugins/Android/AndroidManifest-Tablet.xml";
        string dst = "Assets/Plugins/Android/AndroidManifest.xml";
        System.IO.File.Copy(src, dst, true);

        // Start build
        BuildPipeline.BuildPlayer(levels, "Builds/EWAG-LLU-High-Score-Server.apk", BuildTarget.Android, BuildOptions.AutoRunPlayer);
    }

    private static void PurgeDir(string purgeDir)
    {

        Debug.Log("Trying to purge directory " + purgeDir + "...");
        try
        {
            string[] fileList = Directory.GetFiles(purgeDir, "*");
            foreach (string f in fileList)
            {
                File.Delete(f);
                Debug.Log("Deleted file " + f);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }

    }

    private static void CopyDir(string sourceDir, string destDir, bool createDirIfNonexistent = true)
    {

        if (!Directory.Exists(sourceDir))
        {
            Debug.LogError("Cannot copy files, because the source directory does not exist!");
        }

        if (!Directory.Exists(destDir))
        {
            if (!createDirIfNonexistent)
            {
                Debug.LogError("Cannot copy files because the destination directory does not exists and may not be created!");
                return;
            }
            else
            {
                Directory.CreateDirectory(destDir);
            }
        }

        try
        {
            string[] fileList = Directory.GetFiles(sourceDir, "*");

            foreach (string f in fileList)
            {
                // Remove path from the file name.
                string fName = f.Substring(sourceDir.Length + 1);

                // Use the Path.Combine method to safely append the file name to the path.
                // Will overwrite if the destination file already exists.
                File.Copy(Path.Combine(sourceDir, fName), Path.Combine(destDir, fName), true);
                Debug.Log("Copied " + fName + " from " + sourceDir + " to " + destDir);
            }

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }

    }
}


