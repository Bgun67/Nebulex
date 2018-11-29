using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glass_Shatter : MonoBehaviour {

	public enum GlassType
	{
		Bulletproof,
		Permeable,
		Shatter

	}
	public GameObject crackPrefab;
	public GameObject solidCollider;
	public ParticleSystem shatterSystem;
	public GlassType type;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void OnTriggerEnter (Collider other) {
		Vector3 position = other.transform.position;
		print ("Coll");
		if (other.GetComponent<Bullet_Controller> ()) {
			print ("foudn Bullet");
			if (type == GlassType.Permeable) {
				Instantiate (crackPrefab, this.GetComponent<Collider> ().ClosestPoint (position), Quaternion.identity);
				
			} else if (type == GlassType.Bulletproof) {
				GameObject crack = Instantiate (crackPrefab, this.GetComponent<Collider> ().ClosestPoint (position), Quaternion.identity);
				crack.transform.parent = this.transform;
				Destroy(crack, 20f);
				Destroy (other.gameObject);
			} else if (type == GlassType.Shatter) {
				if (shatterSystem != null) {
					shatterSystem.Play ();
				}
				solidCollider.SetActive (false);

			}
		}
	}
	void OnCollisionEnter (Collision other) {
		Vector3 position = other.contacts[0].point;
		print ("Coll");
		if (other.transform.root.GetComponent<Bullet_Controller> ()) {
			print ("foudn Bullet");
			 if (type == GlassType.Bulletproof) {
				GameObject crack = Instantiate (crackPrefab, position, Quaternion.identity);
				crack.transform.parent = this.transform;
				Destroy(crack, 20f);
			} 
		}
	}
	
}
