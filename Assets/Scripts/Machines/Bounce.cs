using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : MonoBehaviour {
	//add to any bouncing objects including the  launchers and flippers the left one you can add yourself and rescale
	//change this beased on how fast you want then to get sprung ive found 50000 is good for boingers, while 100000 is good for launcchers
	public float bounceFactor = 100000f;
	//percentage of randomness;
	public float randomness = 0.1f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}
	void OnTriggerEnter (Collider other) {
		if (other.GetComponent<Soccer_Ball> () != null) {
			print ("adding FOrece");
			//other.gameObject.GetComponent<Rigidbody> ().AddForce (-bounceFactor * Random.Range (1 - randomness, 1 + randomness) * (-10f*other.transform.position-this.transform.position));
			other.GetComponent<Rigidbody>().velocity = (-10f*Vector3.Normalize(this.transform.position-other.transform.position)*bounceFactor);
			//print (-bounceFactor * Random.Range (1f - randomness, 1f + randomness) * other.contacts [0].normal);
		} else {
			print ("Not a ball");

		}

	}

}
