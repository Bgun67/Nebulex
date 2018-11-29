using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frag_Grenade : MonoBehaviour {
	[Tooltip("If using particles ensure the damage is for each particle")]
	public int damagePower;
	public float explosionForce;
	public GameObject blastSystem;
	public int fromID;

	void Awake(){
		Invoke ("Explode", 5f);
	}

	void Explode(){
		Blast_Controller blastScript = Instantiate (blastSystem, this.transform.position, Quaternion.identity).GetComponent<Blast_Controller> ();
		blastScript.transform.localScale = Vector3.one * 10f;
		blastScript.fromID = fromID;
		blastScript.damagePower = damagePower;
		blastScript.explosionForce = explosionForce;
		Destroy (this.gameObject);
	}
}
