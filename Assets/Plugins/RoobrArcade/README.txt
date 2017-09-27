Unity Android Plugin  - Get Bundle Version v1.0
by Roobr Arcade
RoobrArcade@gmail.com
http://www.roobr.com


WHAT DOES IT DO?
This is a plugin for Unity3D designed to retrieve the current Bundle Version and Bundle Version Code of a Unity3D Android project, exposing those values to the user at runtime. 


WHY IS IT NEEDED?
Unfortunately the Bundle Version and Bundle Version Code of an Android Unity3D project are only accessible in Editor Mode via PlayerSettings.bundleVersion / PlayerSettings.bundleIdentifier. There is no direct method of retrieving this information during runtime. This plugin was built for a project where we wanted to incorporate an update check when the app starts and notify the user that a newer version of the project was available.


HOW TO INSTALL

1. Inside Unity, under the Project tab, right-click the Assets folder and click Import Package > Custom Package.
2. Browse to where you saved roobrArcadeAPKVersion.unitypackage, select the file and click Open.
3. If you already have an AndroidManifest.xml file, uncheck the AndroidManifest.xml file
4. Click Import


HOW SET UP

1. Inside Unity, Project tab, Navigate to Assets/Plugins/Android and open AndroidManifest.xml in a text editor
2. If you did not already have an AndroidManifest.xml file and imported the one provided, skip to step 4
3. Add the following text on a new line just above the closing tag of Application (ie. </application>):
	<activity android:name="com.RoobrArcade.unityPlugin"></activity>
4. Near the top of the AndroidManifest.xml file make sure that android:versionCode="1" and android:versionName="1.0" match your current versioning information
5. Save and close the file.


HOW TO USE

1. Inside the scene that you want to access the Version information, simply drag the “apkVersion” prefab (found in Assets/Plugins/RoobrArcade) into your scene.
2. When the scene loads the prefab will grab the data and populate its public variables
3. Access the variables via: apkVersion.instance.versionID or apkVersion.instance.versionName
4. IMPORTANT: Make sure you load the apkVersion.cs script before accessing the public variables (via Edit>Project Settings>Script Execution Order


That’s it. Shoot us an email if you have any problems and we will reply promptly.