using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pneumo_Tube_Controller : MonoBehaviour {

	public List<Rigidbody> occupants = new List<Rigidbody>();
	public bool isActive = false;
	float lastTime = 0;
	//How long the tube should stay on for
	float onTime = 5f;

	void FixedUpdate(){
		if (isActive && Time.time - lastTime > onTime) {
			isActive = false;
		}

		if (isActive) {
			foreach (Rigidbody rb in occupants) {
				rb.AddForce (Vector3.up * rb.mass * 40f);
			}
		}
	}

	void OnTriggerEnter(Collider other){
		if (other.attachedRigidbody != null) {
			while(occupants.Contains(other.attachedRigidbody)){
				occupants.Remove(other.attachedRigidbody);
			}
			occupants.Add (other.attachedRigidbody);
		}
	}

	void OnTriggerExit(Collider other){
		if (other.attachedRigidbody != null) {
			while(occupants.Contains(other.attachedRigidbody)){
				occupants.Remove(other.attachedRigidbody);
			}
		}
	}

	public void Activate(){
		isActive = true;
		lastTime = Time.time;
	}

		
}
