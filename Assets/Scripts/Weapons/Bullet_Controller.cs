using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet_Controller : NetworkBehaviour {
	[Tooltip("If using particles ensure the damage is for each particle")]
	public int damagePower;
	[Tooltip("Auto Destroy time in seconds")]
	public float range = 4f;
	public bool isExplosive = false;
	public float explosionForce;
	public GameObject blastSystem;
	TrailRenderer trail;
	public int fromID;
Rigidbody rb;

	// Use this for initialization
	void OnEnable () {
		if(trail == null)
			trail = this.GetComponent<TrailRenderer>();
	}
	

	// Update is called once per frame
	 void OnCollisionEnter(Collision other)
	{
		BulletHit(other);
	}
	public void BulletHit(Collision other){
		if (isExplosive) {
			Blast_Controller blastScript = Instantiate (blastSystem, other.contacts[0].point, Quaternion.identity).GetComponent<Blast_Controller> ();
			blastScript.transform.localScale = Vector3.one * 10f;
			blastScript.fromID = fromID;
			blastScript.damagePower = damagePower;
			blastScript.explosionForce = explosionForce;
			//Invoke ("DisableBullet", 0.1f);
			DisableBullet();
			Destroy(blastScript.gameObject, 5f);
		}
		try {
			try {
				if(isServer)
					other.collider.GetComponent<Damage> ().TakeDamage (damagePower, fromID, other.transform.position+other.relativeVelocity);


			} catch {
				try {
					if(isServer)
						other.collider.GetComponentInParent<Damage> ().TakeDamage (damagePower, fromID, other.transform.position+other.relativeVelocity);
				} catch {
					if(isServer)
						other.transform.root.GetComponent<Damage> ().TakeDamage (damagePower, fromID,other.transform.position+other.relativeVelocity);
				}
			}
		} catch {

		}


		DisableBullet();
		



	}
	public void DamageEffect1(){

	}
	public void DamageEffect2(){

	}
	void OnParticleCollision(GameObject other){
		print (other.name);
		try {
			try {
				other.GetComponent<Damage> ().TakeDamage (damagePower, fromID, transform.position);
			} catch {
				try {
					other.transform.parent.GetComponent<Damage> ().TakeDamage (damagePower, fromID,transform.position);
				} catch {
					other.transform.root.GetComponent<Damage> ().TakeDamage (damagePower, fromID,transform.position);
				}
			}
		} catch {
		}


	}
	public void DisableBullet(){
		if(rb == null){
			rb = GetComponent<Rigidbody>();
		}
		this.gameObject.SetActive (false);
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		rb.useGravity = false;
		if(trail != null)
			trail.Clear();
	}

}
