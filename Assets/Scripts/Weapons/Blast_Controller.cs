using System.Collections;
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
	}

	IEnumerator CastDamageRays(){
		for(int i = 0; i< 10; i++){
			for(int j = 0; j< 50; j++){
				RaycastHit _hit;

				if(Physics.SphereCast(transform.position+Vector3.up * 0.1f, 0.05f,new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),Random.Range(-1f,1f)).normalized,out _hit, layerMask)){
					float distance = Mathf.Max(1.1f,(_hit.distance));
					Debug.DrawLine(transform.position, _hit.point, Color.green, 1f);
					GameObject other = _hit.collider.gameObject;
					try {
						try {
							other.transform.GetComponent<Damage> ().TakeDamage ((int)(damagePower / distance), fromID,transform.position);

						} catch {
							try {
								other.transform.parent.GetComponent<Damage> ().TakeDamage ((int)(damagePower / distance), fromID, transform.position);

							} catch {
								other.transform.root.GetComponent<Damage> ().TakeDamage ((int)(damagePower / distance), fromID, transform.position);

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
			yield return new WaitForSecondsRealtime(0.0010f);
		}
	}
	
	// Update is called once per frame
	/*void OnParticleCollision (GameObject other) {
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

	}*/

}
