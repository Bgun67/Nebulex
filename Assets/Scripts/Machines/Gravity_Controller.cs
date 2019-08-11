using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity_Controller : MonoBehaviour {

	public bool ignoreInternalObjects = true;

	public bool useGravity = true;
	public float factor;
	Carrier_Controller ship;
	public Rigidbody rootRB;
	void Start(){
		rootRB = transform.root.GetComponent<Rigidbody> ();
		ship = transform.root.GetComponent<Carrier_Controller> ();

	}

	void OnTriggerStay(Collider other){

		if (ignoreInternalObjects) {
			return;
		}
		if (other.transform.root.gameObject == this.gameObject) {
			return;
		}
		Rigidbody rb = other.transform.root.GetComponent<Rigidbody> ();
		if (rb != null) {
			rb.useGravity = useGravity;

			if (other.tag == "Player") {
				if (useGravity)
				{
					if (!other.GetComponent<Player_Controller>().enteringGravity)
					{
						other.GetComponent<Player_Controller>().StopCoroutine("ExitGravity");
						other.GetComponent<Player_Controller>().StartCoroutine("EnterGravity");

					}
				}
				else
				{
					other.GetComponent<Player_Controller>().StopCoroutine("EnterGravity");
					other.GetComponent<Player_Controller>().StartCoroutine("ExitGravity");

				}

			}

		}
	}

	void OnTriggerEnter(Collider other){
		if (other.transform.root.gameObject == this.gameObject) {
			return;
		}
		Rigidbody rb = other.transform.root.GetComponent<Rigidbody> ();
		if (rb != null) {
			rb.useGravity = useGravity;
			if (other.tag == "Player") {
				
				if (useGravity) {
						other.GetComponent<Player_Controller>().StartCoroutine("EnterGravity");
				} else {

				}			
			}
		}
	}

	float DotAngle(Vector3 fromVector, Vector3 axis){
		return Mathf.Cos(Vector3.Dot (fromVector, axis) / (fromVector.magnitude * axis.magnitude));
	}

	void OnTriggerExit(Collider other){
		Rigidbody rb = other.transform.root.GetComponent<Rigidbody> ();
		if (rb != null) {
			rb.useGravity = false;
			if (other.tag == "Player") {
				
				if (useGravity) {
					other.GetComponent<Player_Controller> ().StartCoroutine ("ExitGravity");
				} else {

				}
			}
		}
	}

}
