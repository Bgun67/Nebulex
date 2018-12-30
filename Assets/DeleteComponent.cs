using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DeleteComponent : MonoBehaviour {

	public bool delete = false;
	// Use this for initialization
	void OnGUI () {
		if(delete){
			delete = false;
			foreach (PrefabLightmapData comp in this.gameObject.GetComponents<PrefabLightmapData>()){
				DestroyImmediate(comp);
			}
		}
	}
	
	
}
