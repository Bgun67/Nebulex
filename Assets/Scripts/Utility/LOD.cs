using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LOD : MonoBehaviour {

	public GameObject[] objectsToDisable;

	// Use this for initialization
	void Start () {
		if (Application.isPlaying) {
			foreach (GameObject _gameObject in objectsToDisable) {
				try{
					_gameObject.GetComponent<MeshRenderer>().enabled = (false);
				}
				catch{
					_gameObject.GetComponent<Light>().enabled = (false);
				}
				finally{

				}
			}
		}
	}

	void Update(){


	}

	void OnGUI(){
		if (Application.isPlaying){
			return;
		}
			

		for(int i = 0; i<objectsToDisable.Length; i++){
			if (objectsToDisable[i] == null || objectsToDisable[i].GetComponent<MeshRenderer> () == null && objectsToDisable[i].GetComponent<Light>() == null) {
				List<GameObject> objsList = new List<GameObject> (objectsToDisable);
				objsList.Remove (objectsToDisable[i]);
				objectsToDisable = objsList.ToArray ();
			}
		}

	}

	void OnTriggerEnter(Collider other){
		if (other.transform.root.GetComponentInChildren<Camera> () == null) {
			return;
		}

		if (other.transform.root.GetComponentInChildren<Camera> ().enabled == true) {
			foreach (GameObject _gameObject in objectsToDisable) {
				try{
					_gameObject.GetComponent<MeshRenderer>().enabled = (true);
				}
				catch{
					_gameObject.GetComponent<Light>().enabled = (true);
				}
			}
		}
	}


	void OnTriggerExit(Collider other){
		if (other.transform.root.GetComponentInChildren<Camera> () == null) {
			return;
		}

		if (other.transform.root.GetComponentInChildren<Camera> ().enabled == true) {
			foreach (GameObject _gameObject in objectsToDisable) {
				try{
					_gameObject.GetComponent<MeshRenderer>().enabled = (false);
				}
				catch{
					_gameObject.GetComponent<Light>().enabled = (false);
				}
			}
		}
	}


}
