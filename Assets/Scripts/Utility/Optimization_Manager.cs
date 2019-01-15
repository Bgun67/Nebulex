﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Optimization_Manager : MonoBehaviour {

	public ReflectionProbe[] probes;
	public GameObject[] topMeshes;
	public bool checkTriangles = false;
	public int[] triCount;

	public bool _lights = true;
	public Light[] lights;

	// Use this for initialization
	void Start () {
		
		
		
	}

	void OnGUI(){
		#if UNITY_EDITOR || UNITY_EDITOR_64
		if(Time.frameCount % 20 == 0){
			lights = GameObject.FindObjectsOfType<Light> ();
			for (int i = 0; i < lights.Length; i++) {
				
				if (lights [i].lightmapBakeType == LightmapBakeType.Baked) {
					lights [i].enabled = _lights;
					lights [i].gameObject.SetActive (_lights);
					
				}
			}
		}
		#endif
		

		if(checkTriangles){
			List<MeshFilter> meshList = new List<MeshFilter>(GameObject.FindObjectsOfType<MeshFilter>());
			for(int i = 0; i< meshList.Count; i++){
				if(meshList[i].sharedMesh == null){
					meshList.RemoveAt(i);
					i--;
				}
			}
			meshList.Sort((x,y) => (x.sharedMesh.triangles.Length.CompareTo(y.sharedMesh.triangles.Length)));
			meshList.Reverse();

			int maxMeshes = 40;

			topMeshes = new GameObject[maxMeshes];
			
			

			triCount = new int[maxMeshes];

			for(int i = 0; i < maxMeshes; i++){
				topMeshes [i] = meshList[i].gameObject;
				triCount[i] = meshList[i].sharedMesh.triangles.Length;

			}

			

			checkTriangles = false;
		}

	}


	

}
