using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour {
	public Transform gate1;
	public Transform gate2;
	public int gate = 1;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void OnTriggerEnter (Collider other) {
		if (other.tag == "Player") {
			if (gate == 1) {
				other.transform.position = gate2.position;
			} else {
				other.transform.position = gate1.position;

			}
		}
	}
}
