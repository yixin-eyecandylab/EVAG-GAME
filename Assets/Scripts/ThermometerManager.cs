using UnityEngine;
using System.Collections;

public class ThermometerManager : MonoBehaviour {


    private OVRCameraRig cameraController = null;
    private Transform cameraTransform = null;

    Vector3 pivot = new Vector3(0.0f, -2.0f, 0.0f);
    Vector3 Yaxis = new Vector3(0.0f, 1.0f, 0.0f);
    float OldRotationY = 0.0f, NewRotationY;
    Quaternion prevRotation;

    private bool isMine = true;
    NetworkView nw;

    private void Awake()
    {
        nw = GetComponent<NetworkView>();
        if (nw != null && !nw.isMine)
            isMine = false;
    }
    // Use this for initialization
    void Start () {

        if (VraSettings.instance.isGearVR)
        {
            cameraController = GameObject.Find("OVRCameraRig").GetComponent<OVRCameraRig>();
            if (cameraController == null)
                Debug.LogError("Could not find OVRCameraRig!");
            else
                cameraTransform = cameraController.centerEyeAnchor;

            this.transform.RotateAround(pivot, Yaxis, cameraTransform.rotation.eulerAngles.y);

            //GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
            //if (nws != null)
            //{
            //    nws.GetComponent<NetworkSync>().UpdateTemperaturePositionTablet(cameraTransform.rotation.eulerAngles.y);
            //}
            if (isMine && nw != null)
            {
                Network.RemoveRPCs(nw.viewID);
                nw.RPC("SetThermomenterPositionRemote", RPCMode.Others, cameraTransform.rotation.eulerAngles.y);
            }
            prevRotation = cameraTransform.rotation;
        }
    }

    // Update is called once per frame

	void Update () {

        if (VraSettings.instance.isHeadset)
        {

            if (prevRotation != cameraTransform.rotation)
            {
                float Angle = cameraTransform.rotation.eulerAngles.y - prevRotation.eulerAngles.y;
                this.transform.RotateAround(pivot, Yaxis, Angle);

                if (isMine && nw != null)
                {
                    Network.RemoveRPCs(nw.viewID);
                    nw.RPC("SetThermomenterPositionRemote", RPCMode.Others, Angle);
                }
                //GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
                //if (nws != null)
                //{
                //    nws.GetComponent<NetworkSync>().UpdateTemperaturePositionTablet(Angle);
                //}
            }
            prevRotation = cameraTransform.rotation;


        }


    }

    [RPC]
    public void SetThermomenterPositionRemote(float AngleY)
    {
        this.transform.RotateAround(pivot, Yaxis, AngleY);
    }
    //public void UpdateTemperaturePositionTablet(float Angle)
    //{
    //    this.transform.RotateAround(pivot, Yaxis, Angle);
    //}

   

}
