using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Optimization_Manager : MonoBehaviour {

	public ReflectionProbe[] probes;

	// Use this for initialization
	void Start () {
		Light[] lights = GameObject.FindObjectsOfType<Light> ();
		#if UNITY_EDITOR || UNITY_EDITOR_64
		for (int i = 0; i < lights.Length; i++) {
			
			if (lights [i].lightmapBakeType == LightmapBakeType.Baked) {
				lights [i].enabled = false;
				lights [i].gameObject.SetActive (false);
			}
		}
		#endif
	}
	

}
