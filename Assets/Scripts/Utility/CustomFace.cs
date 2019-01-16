using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFace : MonoBehaviour {

	public Texture2D tex;
	public bool takePicture;
	WebCamTexture webCamTex;

	// Use this for initialization
	void Start () {
		
		webCamTex = new WebCamTexture (WebCamTexture.devices[0].name,512,512);
		webCamTex.Play ();


	}

	public void CaptureImage(){
		tex = new Texture2D (webCamTex.width, webCamTex.height);
		Color[] pixels = webCamTex.GetPixels ();
		tex.SetPixels (pixels);
		tex.Apply ();
		StartCoroutine (CoCaptureImage ());
	}
	IEnumerator CoCaptureImage(){
		
		yield return new WaitForEndOfFrame ();

	}
	
	// Update is called once per frame
	void Update () {
		if (takePicture) {
			takePicture = false;
			CaptureImage ();
			this.GetComponent<SkinnedMeshRenderer> ().material.SetTexture ("_MainTex", tex);
		}
		//tex.LoadImage(System.IO.File.ReadAllBytes(Application.streamingAssetsPath + "/image.jpg"));
		//tex.Apply ();

	}
}
