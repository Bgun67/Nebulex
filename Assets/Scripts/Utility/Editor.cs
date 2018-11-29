using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Editor : MonoBehaviour {



	// Use this for initialization
	void Start () {
		#if !UNITY_EDITOR
		return;
		#endif

		foreach ( Collider collider in GameObject.FindObjectsOfType<Collider>()){
			if (collider.transform.root.GetComponent<Rigidbody> () == null) {
				//collider.gameObject.hideFlags = HideFlags.NotEditable;
				print (collider.transform.root.name + "/" + collider.name);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		#if UNITY_EDITOR
		return;
		#endif
		
	}
}
