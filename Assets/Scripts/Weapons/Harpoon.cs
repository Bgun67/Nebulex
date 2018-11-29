using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harpoon : MonoBehaviour {
	public TrailRenderer trailRenderer;
	public Rigidbody rb;
	public Harpoon_Gun harpoonGunScript;

	public bool hit;

	// Use this for initialization
	void Start () {
		

	}
	
	// Update is called once per frame
	void Update(){
	//	rb.AddRelativeTorque (Random.Range (-1f, 1f), Random.Range (-1f, 1f), Random.Range (-1f, 1f));
	//	rb.AddRelativeForce (Random.Range (-50f, 50f), Random.Range (-50f, 50f), 0f);

	}
	void OnCollisionEnter (Collision other) {
		if (hit != true) {
			
			print ("Hit: " + other.gameObject.name);
			harpoonGunScript.hit = true;
			trailRenderer.time = 0f;
			if (other.transform.root.GetComponent<Rigidbody> () != null) {
				this.GetComponent<Rigidbody> ().isKinematic = true;
				this.transform.parent = other.transform.root;
				harpoonGunScript.Join (other.transform.root);
				hit = true;

			}

		}
	




	}
}
