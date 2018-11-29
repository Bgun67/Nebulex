using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soccer_Ball : MonoBehaviour {

	Rigidbody rb;
	
	// Update is called once per frame
	void Update () {
		if(rb != null){
			rb.velocity = Vector3.ClampMagnitude(rb.velocity,30f);
		}
		else{
			this.rb = this.GetComponent<Rigidbody>();
		}
	}
}
