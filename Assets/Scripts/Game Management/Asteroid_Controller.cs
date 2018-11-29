using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid_Controller : MonoBehaviour {
	public Transform carrierOne;
	public Transform carrierTwo;
	[Tooltip("The sphere crated around the two ships, thiccness gives the radius outside the sphere in which the asteriod will be respawned")]
	public float boundingSphereThickness;
	Asteroid_Field_Manager fieldManager;
	public bool junk;
	// Use this for initialization
	void Start () {
		fieldManager = FindObjectOfType<Asteroid_Field_Manager> ();
		if (junk) {
			fieldManager.RelaunchJunk (this.gameObject);
		} else {
			fieldManager.Relaunch (this.gameObject);
		}
		InvokeRepeating ("CheckDistance", 5f, 5f);
		boundingSphereThickness = fieldManager.boundingSphereThickness;
		carrierOne = fieldManager.carrierOne;
		carrierTwo = fieldManager.carrierTwo;
	}
	
	void CheckDistance(){
		Vector3 carriersCenter = carrierOne.position + carrierTwo.position / 2f;
			float distanceToAsteroid = Vector3.Distance(carriersCenter, transform.position);
		float maxDistance = Vector3.Distance (carrierOne.position, carriersCenter) + boundingSphereThickness;
		if (distanceToAsteroid > maxDistance) {
			if (junk) {
				fieldManager.RelaunchJunk (this.gameObject);
			} else {
				fieldManager.Relaunch (this.gameObject);
			}
		}


	}
	void OnCollisionEnter(Collision other){

		if (other.transform.root.tag != "Player") {
			other.transform.root.GetComponent<Damage> ().TakeDamage ((int)(0.1f*this.GetComponent<Rigidbody>().mass),0);
		}

	}
	[MRPC]
	public void RPC_Relaunch(float scaleFactor, float mass){
		this.transform.localScale = Vector3.one * scaleFactor;
		this.GetComponent<Rigidbody> ().mass = mass;

	}

}
