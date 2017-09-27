using UnityEngine;
using System.Collections;

public class GazeTargetActionDestroy : GazeTargetAction
{
    public int PulseID;
    public int Gender;


    public override void PerformAction()
    {
        //GameObject.Find("DiamondGame").GetComponent<DiamondGame>().DestroyPulse(PulseID);
        //Network.Destroy(this.gameObject);
        //if (Input.GetMouseButtonDown(0)&& (Gender != 0))//(Input.GetMouseButton(0))

        if(/*Input.GetMouseButtonDown(0) &&*/ (Gender != 0))
        {

            
            Network.Destroy(this.gameObject);//.SetActive(false);

            //GameObject.Find("DiamondGame").GetComponent<DiamondGame>().UpdateScore(/*Gender*/);

            //GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
            //if (nws != null)
            //{
            //    nws.GetComponent<NetworkSync>().DesactivatePulseTablet(PulseID, Gender);
            //}
        }
    }
}
