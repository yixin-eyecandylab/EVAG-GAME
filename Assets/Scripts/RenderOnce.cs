using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RenderOnce : MonoBehaviour {

	public Canvas canvas;
	//public GameObject PleaseWaitLabel;
	public GameObject ListObject;
    RectTransform ListRect;
    RawImage ListImage;
    public GameObject ServerListObject;
    RectTransform ServerListRect;
    RawImage ServerListImage;

    private Camera camera;
	private int renderCount = 0;

	// Use this for initialization
	void Start () {
		camera = GetComponent<Camera>();
        camera.targetTexture = null;
		camera.enabled = false;
		canvas.enabled = false;

        ListRect = ListObject.GetComponent<RectTransform>();
        ListImage = ListObject.GetComponent<RawImage>();
        ServerListRect = ServerListObject.GetComponent<RectTransform>();
        ServerListImage = ServerListObject.GetComponent<RawImage>();
        ListImage.texture = null;
        ServerListImage.texture = null;
//		Invoke("Render",1.0f);
	}
	
	public void RenderAtSize(int height) { // render the scoreboard at set height
		Debug.Log("Render!");
        FramesToRender = 3; // first frame seems to be blank

        if (camera.targetTexture == null || camera.targetTexture.height != height)
        {
            RenderTexture rt = new RenderTexture(512, height, 0);
            camera.targetTexture = rt;
        }

    }

	/*public void Loading() {    no longer necessary (improved list rendering)
		PleaseWaitLabel.SetActive(true);
		ListObject.SetActive(false);
	}*/


    int FramesToRender = -1;

    void LateUpdate() // if we need to render, make sure image is rendered before switching off camera to avoid camera.render bug (why doesn't it work?)
    {
        if(FramesToRender > 0)
        {
            canvas.enabled = true;
            camera.enabled = true;
           //PleaseWaitLabel.SetActive(false);
            ListObject.SetActive(true);
            FramesToRender--;
        } else if(FramesToRender==0)
        {
            if (ListImage.texture != null && ListImage.texture!=camera.targetTexture)
            {
                Destroy(ListImage.texture);
            }
           
            ListRect.sizeDelta = new Vector2(512 * 2, camera.targetTexture.height * 2);
            ListImage.texture = camera.targetTexture;

            ServerListRect.sizeDelta = new Vector2(512 * 2, camera.targetTexture.height * 2);
            ServerListImage.texture = camera.targetTexture;
            //canvas.enabled = false;
            camera.enabled = false;
            FramesToRender--;
        }
      
    }

    /*
	public void OnPostRender() {
		if(renderCount > 0) {
			//canvas.enabled = false;
			//camera.enabled = false;
			PleaseWaitLabel.SetActive(false);
			ListObject.SetActive(true);
		}
		renderCount++;
	}*/

        /*
	public void StopRendering() {
		if(camera!=null)
			camera.enabled = false;
	}*/


}
