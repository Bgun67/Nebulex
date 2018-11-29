using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Night_Vision_Effects : MonoBehaviour {

	public Material material;

	//PostProcess the image
	void OnRenderImage(RenderTexture source, RenderTexture destination){
		Graphics.Blit (source, destination, material);
	}
}
