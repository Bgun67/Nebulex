using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PromoCamera : MonoBehaviour {
	Camera cam;
	public Transform targetObject;
	public bool isRecordingVideo = false;
	// Use this for initialization
	void Start () {
		cam = this.GetComponent<Camera>();
		cam.depth = -10;
		cam.gameObject.SetActive(true);
		
	}
	
	void Update(){
		cam.transform.LookAt(targetObject);
	}
	void OnRenderImage (RenderTexture _source, RenderTexture _destination) {

		if(!isRecordingVideo && !Input.GetKey("o")){
			Graphics.Blit(_source, _destination);
			return;
		}
        // Create a texture the size of the screen, RGB24 format
        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Read screen contents into the texture
		RenderTexture.active = _source;
        tex.ReadPixels(new Rect(0, 0, _source.width, _source.height), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Object.Destroy(tex);

        // For testing purposes, also write to a file in the project folder
        File.WriteAllBytes(Application.dataPath+"/ScreenCaps/SavedScreen" + Time.time.ToString() + ".png", bytes);

		Graphics.Blit(_source, _destination);
	}
}
