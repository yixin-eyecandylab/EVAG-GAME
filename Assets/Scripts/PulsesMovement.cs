using UnityEngine;
using System.Collections;

public class PulsesMovement : MonoBehaviour {

    private bool isMine = true;
    NetworkView nw;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void Awake()
    {
        nw = GetComponent<NetworkView>();
        if (nw != null && !nw.isMine)
            isMine = false;
    }

    public void SetPulsePosition(Vector3 newPos, Vector3 newRot)
    {

        //PulseQueue[LaunchPoint].Pulse.transform.position = newPos;
        gameObject.transform.position = newPos;
        gameObject.transform.Rotate(newRot);

        if (isMine && nw != null)
       // nw = GetComponent<NetworkView>();
        {
            Network.RemoveRPCs(nw.viewID);
            nw.RPC("SetPositionRemote", RPCMode.Others, newPos, newRot);
        }
    }

    [RPC]
    public void SetPositionRemote(Vector3 newPos, Vector3 newRot)
    {
        SetPulsePosition(newPos, newRot);
    }
}
