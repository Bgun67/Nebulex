using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poynting_Shield : MonoBehaviour {

	public Rigidbody rb;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other){
		print ("Hit");

		Rigidbody otherRb = other.GetComponentInParent<Rigidbody> ();

		if (otherRb == null) {
			return;
		}

		if (Vector3.Dot (otherRb.velocity.normalized, this.transform.forward) <= 0) {
			otherRb.velocity = rb.velocity;
		} else {
			print("Unable");
		}
	}
}
