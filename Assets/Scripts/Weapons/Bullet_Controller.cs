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
	TrailRenderer trail;
	public int fromID;
	Rigidbody rb;

	// Use this for initialization
	void OnEnable () {
		if(trail == null)
			trail = this.GetComponent<TrailRenderer>();
	}

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

		//Damage is only calculated on the server
		Damage _damageScript;
		//Step through the gameobject's hierarchy looking for damage scripts
		_damageScript = other.collider.GetComponent<Damage> ();
		if(_damageScript == null)
			_damageScript = other.collider.GetComponentInParent<Damage> ();
		if(_damageScript == null)
			_damageScript = other.transform.root.GetComponent<Damage> ();
		if(_damageScript != null){
			if (fromID == Game_Controller.Instance.GetLocalPlayer()){
				UI_Manager._instance.ShowHit();
			}
			_damageScript.TakeDamage(damagePower, fromID, other.transform.position + other.relativeVelocity);
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
