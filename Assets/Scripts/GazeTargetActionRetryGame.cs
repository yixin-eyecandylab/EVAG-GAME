using UnityEngine;
using System.Collections;

public class GazeTargetActionRetryGame : GazeTargetAction {

    private ViewpointManager viewpointManager;
    public GameObject GameOverMessage;

    public override void PerformAction() {
        //GameObject.Find ("GameController").GetComponent<GameController>().Retry();
        //GameObject.Find("DiamondGame").GetComponent<DiamondGame>().Retry();

        Network.Destroy(GameOverMessage);

        viewpointManager = GameObject.Find("ViewpointManager").GetComponent<ViewpointManager>();

        if (viewpointManager == null)
        {
            Debug.LogError("GameController: Can't find ViewpointManager!");
            return;
        }

        viewpointManager.LoadViewpoint("GameBackground");

        GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
        if (nws != null)
        {
            nws.GetComponent<NetworkSync>().LoadViewpoint("GameBackground");
        }

        //GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
        //if (nws != null)
        //{
        //    nws.GetComponent<NetworkSync>().RetryTablet();
        //}
    }

    //public void ReloadViewpoint(Vector3 newPos)
    //{
    //    if (isMine && nw != null)
    //    {
    //        Network.RemoveRPCs(nw.viewID);
    //        nw.RPC("SetPositionRemote", RPCMode.Others, newPos);
    //    }
    //}

    //[RPC]
    //public void SetPositionRemote(Vector3 newPos)
    //{
    //    SetPosition(newPos);
    //}
}
