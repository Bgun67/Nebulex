using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Optimization_Manager : MonoBehaviour {

	public ReflectionProbe[] probes;
	public GameObject[] topMeshes;
	public bool checkTriangles = false;
	public int[] triCount;

	public bool _lights = true;
	public List<Light> lights;

	public float cullDist;

	// Use this for initialization
	void Update () {
		float[] distances = new float[32];
		distances[18] = 150f;
		foreach(Camera cam in GameObject.FindObjectsOfType<Camera>()){
			cam.layerCullDistances = distances;
		}

		
		
		
	}

	void OnGUI(){
		#if UNITY_EDITOR || UNITY_EDITOR_64
		if(Time.frameCount % 5 == 0){
			
			foreach(Light _light in GameObject.FindObjectsOfType<Light> ()) {
				if(lights.Contains(_light)) continue;
				if (_light.lightmapBakeType == LightmapBakeType.Baked) {
					
					lights.Add(_light);
					
				}
			}
			for (int i = 0; i < lights.Count; i++) {
				
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
