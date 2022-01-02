using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Blackout_Effects : MonoBehaviour {

	public Material material;


	//PostProcess the image
	void OnRenderImage(RenderTexture source, RenderTexture destination){
		Graphics.Blit (source, destination, material);
	}
	public void ChangeConciousness(float newValue){
		material.SetFloat ("_conciousness", newValue);
	}
	public void ChangeBlood(float newValue){
		material.SetFloat ("_levelOfBlood", Mathf.Abs(newValue));
	}
}
