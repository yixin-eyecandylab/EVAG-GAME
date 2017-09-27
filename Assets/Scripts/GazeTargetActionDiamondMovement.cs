using UnityEngine;
using System.Collections;

public class GazeTargetActionDiamondMovement : GazeTargetAction {

    bool action = false;
    float DiamondPositionY = -1.0f;
    public override void PerformAction()
    {
        GameObject.Find("ViewpointPreload").GetComponent<ViewpointPreload>().WelcomeMessageDisable();
        action = true;
        //GameObject.Find("ViewpointPreload").GetComponent<ViewpointPreload>().ProtectMessageEnable();
        //this.gameObject.SetActive(false);
    }

    //   // Use this for initialization
    //   void Start () {

    //}

    //// Update is called once per frame
    void Update()
    {
        if ((action)&&(DiamondPositionY < -0.5))
        {
            this.gameObject.transform.position = new Vector3(0.0f, DiamondPositionY, 2.0f);
            DiamondPositionY = DiamondPositionY + 0.01f;
        }
        if ((action)&&(DiamondPositionY >= -0.5))
        {
            GameObject.Find("ViewpointPreload").GetComponent<ViewpointPreload>().ProtectMessageEnable();
            action = false;
        }

    }
}
