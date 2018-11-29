using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchor : MonoBehaviour {

	public Vector3 direction = Vector3.down;
	public float distanceThreshold = 3f;
	public Rigidbody rb;

	public bool fire;

	void Update(){
		if (fire) {
			fire = false;
			FireAnchor ();
		}
	}

	public bool FireAnchor(){
		RaycastHit hit;

		if (Physics.Linecast (this.transform.position, transform.position + direction * distanceThreshold, out hit)) {
			Rigidbody otherRb = hit.collider.GetComponentInParent<Rigidbody> ();
			rb.gameObject.AddComponent<FixedJoint> ().connectedBody = otherRb;
			return true;
		} else {
			return false;
		}
	}
}
