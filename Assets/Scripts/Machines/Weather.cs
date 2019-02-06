using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weather : MonoBehaviour {
	
	public ParticleSystem [] allSystems;
	public int weatherType;
	// Use this for initialization
	void Activate () {
		weatherType = Random.Range (0, allSystems.Length);
		StartWeather (weatherType);

	}
	
	// Update is called once per frame


	public void StartWeather(int _type){
		foreach (ParticleSystem system in allSystems) {
			system.Stop ();
		}
		allSystems [_type].Play ();
	}


}
