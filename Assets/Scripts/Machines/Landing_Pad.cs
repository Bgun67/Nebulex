using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landing_Pad : MonoBehaviour {

	public bool hasShip = false;
	public Ship_Controller ship;
	public Rigidbody shipRb;

	void Update(){
		if (ship != null && ship.enabled == false) {
			shipRb.transform.position = Vector3.Lerp(shipRb.transform.position, this.transform.position + Vector3.up * 3f, 0.1f);
			hasShip = true;
		}
		if (ship != null && ship.enabled == true) {
			hasShip = false;
		}

	}

	void OnTriggerEnter(Collider other){
		if (ship != null) {
			return;
		}

		ship = other.transform.root.GetComponent<Ship_Controller> ();

		if (ship != null) {
			shipRb = ship.GetComponent<Rigidbody> ();
		}

	}

	void OnTriggerExit(Collider other){
		if (ship == null) {
			return;
		}
		if (other.transform.root.gameObject == ship.gameObject) {
			ship = null;
			shipRb = null;
		}
	}


}
