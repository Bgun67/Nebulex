using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blast_Controller : MonoBehaviour {
	public float explosionForce;
	public float damagePower;
	public int fromID;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void OnParticleCollision (GameObject other) {
		int distance = (int)Vector3.Distance (this.transform.position, other.transform.position) + 1;
		try {
			try {
				other.transform.GetComponent<Damage> ().TakeDamage ((int)(damagePower / distance), fromID);

			} catch {
				try {
					other.transform.parent.GetComponent<Damage> ().TakeDamage ((int)(damagePower / distance), fromID);

				} catch {
					other.transform.root.GetComponent<Damage> ().TakeDamage ((int)(damagePower / distance), fromID);

				}
			}
		} catch {
		}
		if (other.transform.root.GetComponent<Rigidbody> () != null) {
			//other.transform.root.GetComponent<Rigidbody> ().AddForceAtPosition (explosionForce * Vector3.up, this.transform.position);
			other.transform.root.GetComponent<Rigidbody> ().AddExplosionForce (explosionForce, this.transform.position, 30f, 3f);
		}

	}

}
