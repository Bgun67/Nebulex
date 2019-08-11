using System.Collections;
using System.Collections.Generic;
using UnityEngine;

<<<<<<< HEAD
=======

>>>>>>> Local-Git
[ExecuteInEditMode]
public class Optimization_Manager : MonoBehaviour {

	public ReflectionProbe[] probes;
	public GameObject[] topMeshes;
	public bool checkTriangles = false;
	public int[] triCount;

	public bool _lights = true;
<<<<<<< HEAD
	public Light[] lights;

	// Use this for initialization
	void Start () {
=======
	public List<Light> lights;

	public float cullDist;

	void Start(){
		if(Application.isPlaying){
			foreach(MeshRenderer renderer in GameObject.FindObjectsOfType<MeshRenderer>()){
				renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
			}
			foreach(SkinnedMeshRenderer renderer in GameObject.FindObjectsOfType<SkinnedMeshRenderer>()){
				renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
			}
		}
	}

	// Use this for initialization
	void Update () {
		float[] distances = new float[32];
		distances[18] = 150f;
		foreach(Camera cam in GameObject.FindObjectsOfType<Camera>()){
			cam.layerCullDistances = distances;
		}

>>>>>>> Local-Git
		
		
		
	}

	void OnGUI(){
		#if UNITY_EDITOR || UNITY_EDITOR_64
<<<<<<< HEAD
		if(Time.frameCount % 20 == 0){
			lights = GameObject.FindObjectsOfType<Light> ();
			for (int i = 0; i < lights.Length; i++) {
				
				if (lights [i].lightmapBakeType == LightmapBakeType.Baked) {
=======
		if(Time.frameCount % 2 == 0){
			
			foreach(Light _light in GameObject.FindObjectsOfType<Light> ()) {
				if(lights.Contains(_light)) continue;
				if (_light.lightmapBakeType == LightmapBakeType.Baked) {
					
					lights.Add(_light);
					
				}
			}
			for (int i = 0; i < lights.Count; i++) {
				if(lights[i] == null){
					lights.RemoveAt(i);
					//break;
				}
				if (lights [i].lightmapBakeType == LightmapBakeType.Baked) {

>>>>>>> Local-Git
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
