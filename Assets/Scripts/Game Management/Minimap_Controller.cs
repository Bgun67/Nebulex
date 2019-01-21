using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap_Controller : MonoBehaviour {
	public Material mat;
	public int team;
	public GameObject[] icons;
	public bool Hide = true;

	// Use this for initialization
	void Start () {
		icons = GameObject.FindGameObjectsWithTag ("Icon");
	}
	void OnPreRender(){
		
		GL.wireframe = true;
	}
	void OnPostRender() {
		GL.wireframe = false;
	}
	// Update is called once per frame
	void Update () {
		if (Hide)
		{
			return;
		}
		foreach (GameObject icon in icons)
		{
			Material mat = icon.GetComponent<MeshRenderer>().material;
			if (mat.color.a >= 0.01f) {
				mat.color = mat.color - new Color (0f, 0f, 0f, 0.05f);
				icon.GetComponent<MeshRenderer>().material = mat;
			}

		}


	}
}
