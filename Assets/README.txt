Seidenader MS-Series VRA

Unity Implementation

TODOs

- Score-Display after game
- Finish game when video reaches end
- Tablet-Synchronisation
- Cut videos, video variations
- Shrink/Fade effect for Ready! Steady! Go!


- Infopoints, when speaker parts are available

=== Requirements ===

Unity 5.2.2f1 or newer, http://unity3d.com

Android SDK, including

  Android 4.4.2 (API 19)
  SDK Build Tools  Rev 22 
  
  See http://docs.unity3d.com/Manual/android-sdksetup.html

  Details what has to be included for GearVR builds can be found in the Oculus Mobile SDK documentation.
  
=== Included in this project ==

Oculus Mobile SDK 0.5.0 Unity package, https://developer.oculus.com/
Easy Movie Texture, https://www.assetstore.unity3d.com/en/#!/content/10032
Text Mesh Pro, https://www.assetstore.unity3d.com/en/#!/content/17662

=== Scenes ===

Scenes\Splash_Screen - a small scene showing a headtracked Seidenader banner during startup
Scenes\Startroom -- the (currently unused) Startroom
Scenes\Viewpoints -- the core
Scenes\Network_Observer -- the tablet app

=== Building ===

Make sure the oculussig (https://developer.oculus.com/osig/) for your device is included in Assets/Plugins/Android/assets/

The "VRA-Build" custom menu has been created to simply building multiple apps from one Unity project.

- Building the VRA

	Select VRA-Build / Build+Run VRA for Gear VR
  		
- Building the Network Observer

	Select VRA-Build / Build+Run VRA for Tablet 
  		  	
  To run on a non GearVR-Device (and without Gear VR Developer mode), remove the following from AndroidManifest.xml:
  <meta-data android:name="com.samsung.android.vr.application.mode" android:value="vr_only"/>

=== Media Content ===

All media content (except audio for the time being) is held externaly and will not be part of the Unity build.

For development, all image and video files should be held in the directory <Unity-Project-Base>/Comberry/sdCard/VRA/.

The viewpoint definition file "viewpoints.xml" goes there as well.

=== Deployment ===

Can be done with unity (Build & Run)

Media files have to be uploaded to the Note 4 to the directory /storage/sdCard/Comberry/VRA/. 

Note: This is NOT the external SD card! (That would be "extSdCard"). 

When plugged in to a Windows PC, this directory is available as <Your Device Name>\Phone\Comberry\VRA.

=== Developing ===

The app can be started in the Unity Editor. An Oculus runtime installation might be required for this. 

To test the camera movement, you need an Oculus DK1 or DK2 headset.

Movies will not be visible on the PC, this is a limiation of Easy Movie Texture, this runs only on Android.

=== Galaxy Note 4 Setup ===

Insert GearVR SD card
Boot
Deactive WLAN
Username: “Ano Nymous”
Do not connect to any Google or Samsung or other account
Device-Name “Seidenader VRA nn” (nn = 01 … 99)

Clean up screens, remove everything except "Settings" and "Menu"

Disable Software Update (Device Properties)
Deactivate annyoing touch sound "blubb"
Disable “OK Google” in Google settings
Enable Developer mode (Device Information, tap on Buildnumber 7 times)
Enable USB debugging

Prevent Android OTA update (prevent Android 5 Lollipop installation): 
	adb shell pm block com.wssyncmldm

Block other bloatware (see block_unblock_applications.xlsx)
	com.facebook.appmanager
	com.facebook.katana
	com.facebook.system
	com.facebook.pages.app
	com.facebook.orca
	And maybe:
		com.samsung.android.app.galaxyfinder
		com.samsung.android.sconnect
		com.samsung.android.app.pinboard
		tv.peel.smartremote

Disable Settings/Security/Verify Apps 
Now you can activate WLAN
Put device in GearVR, download and install software

Get Device-ID (adb devices)
Get Oculus signature file (https://developer.oculus.com/osig/)
Put signature file to Assets/Plugins/Android/assets/

Build and Deploy

=== ADB cheat sheet ===

- List connected devices, get device ID
	adb devices

- Connect over WiFi	
	adb tcpip 5555
	adb connect <device-ip>
	
- Get logfile
	adb logcat -d >logcat.txt
	
- Clear logfile on device
	adb logcat -c
	
- Get screenshot
	adb shell screencap -p /sdcard/screen.png
	adb pull /sdcard/screen.png
	adb shell rm /sdcard/screen.png
	
- Push xml config
	adb push viewpoints.xml /storage/sdcard0/Comberry/VRA/viewpoints.xml
	

== How to add new Device ID

- Get the ID from the new Device

   a) "adb devices"
   b) Download "Device ID" by "Evozi" from Play store. Get alphanumeric value called "Hardware Serial".

- Create temporary signature file with Oculus Signature Generator (OSIG): https://developer.oculus.com/osig/

- Put downloaded signutures info folder "Assets/Plugins/Android/assets/" of the Unity projects.

- Rebuild phone APK

- Commit your changes






