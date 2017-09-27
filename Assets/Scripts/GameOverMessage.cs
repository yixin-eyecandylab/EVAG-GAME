using UnityEngine;
using System.Collections;

public class GameOverMessage : MonoBehaviour
{

    public Canvas GameOverMessageCanvas;
    public TMPro.TextMeshProUGUI ScoreMsg;
    private bool isMine = true;
    NetworkView nw;
    //public int score;
    // Use this for initialization

    private void Awake()
    {
        nw = GetComponent<NetworkView>();
        if (nw != null && !nw.isMine)
            isMine = false;
    }

    void Start()
    {
        //GameOverMessageCanvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DisplayGameOverMessage(int score)
    {
        //GameOverMessageCanvas.enabled = true;
        ScoreMsg.text = "" + score;

        if (isMine && nw != null)
        {
            nw.RPC("DisplayGameOverMessageRemote", RPCMode.Others, score);
        }
    }

    [RPC]
    public void DisplayGameOverMessageRemote(int score)
    {
        DisplayGameOverMessage(score);
    }
}
