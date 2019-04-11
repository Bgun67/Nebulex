using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_Controller : MonoBehaviour {
	[Tooltip("If using particles ensure the damage is for each particle")]
	public int damagePower;
	[Tooltip("Auto Destroy time in seconds")]
	public float range = 4f;
	public bool isExplosive = false;
	public float explosionForce;
	public GameObject blastSystem;
	public int fromID;

	// Use this for initialization
	void Start () {
			Invoke ("DisableBullet", range);

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
			Invoke ("DisableBullet", 0.1f);
			Destroy(blastScript.gameObject, 5f);
		}
		try {
			try {
				other.collider.GetComponent<Damage> ().TakeDamage (damagePower, fromID, transform.position);


			} catch {
				try {
					other.collider.GetComponentInParent<Damage> ().TakeDamage (damagePower, fromID, transform.position);
				} catch {
					other.transform.root.GetComponent<Damage> ().TakeDamage (damagePower, fromID, transform.position);
				}
			}
		} catch {

		}

		this.enabled = false;

		Invoke ("DisableBullet", 0.1f);
		/*
		if (other.collider.tag == checkTag1) {
			RaycastHit hit;
			Physics.Raycast (this.transform.position, Vector3.down, out hit, 10f);
			print (hit.normal);
			Instantiate (effect1, other.collider.ClosestPoint (this.transform.position), Quaternion.Euler (hit.normal));

		} else if (other.collider.tag == checkTag2) {
		} else {
		}*/



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
		this.gameObject.SetActive (false);
	}

}
