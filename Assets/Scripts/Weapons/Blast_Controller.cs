﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blast_Controller : MonoBehaviour {
	public float explosionForce;
	public float damagePower;


	public int fromID;
	
	public LayerMask layerMask;

	// Use this for initialization
	void Start () {
		StartCoroutine(CastDamageRays());
		Destroy(this.gameObject, 10f);
	}

	IEnumerator CastDamageRays(){
		//Initial Concussion
		//if(Physics.SphereCast(transform.position+Vector3.up * 0.1f, 0.05f,new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),Random.Range(-1f,1f)).normalized,out _hit, layerMask)){
		//	OnRaycastHit(ref _hit);
		//}

		for(int i = 0; i< 10; i++){
			for(int j = 0; j< 50; j++){
				RaycastHit _hit;

				if(Physics.SphereCast(transform.position+Vector3.up * 0.1f, 0.05f,new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),Random.Range(-1f,1f)).normalized,out _hit, layerMask)){
					OnRaycastHit(ref _hit);
				}
			}
			yield return new WaitForSecondsRealtime(0.0010f);
		}
	}

	void OnRaycastHit(ref RaycastHit _hit){
		float distance = Mathf.Max(1.1f,(_hit.distance));

		GameObject other = _hit.collider.gameObject;
		try {
			try {
				other.transform.GetComponent<Damage> ().TakeDamage ((int)(damagePower / distance), fromID,transform.position, true);

			} catch {
				try {
					other.transform.parent.GetComponent<Damage> ().TakeDamage ((int)(damagePower / distance), fromID, transform.position, true);

				} catch {
					other.transform.root.GetComponent<Damage> ().TakeDamage ((int)(damagePower / distance), fromID, transform.position,true);

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
