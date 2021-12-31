using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Frag_Grenade : NetworkBehaviour {
	[Tooltip("If using particles ensure the damage is for each particle")]
	public int damagePower;
	public float explosionForce;
	public GameObject blastSystem;
	[SyncVar]
	public int fromID;
	[SyncVar]//[HideInInspector]
	public Vector3 initialVelocity;

	void Awake(){
		Invoke ("Explode", 5f);
	}
	void Start(){
		GetComponent<Rigidbody>().velocity = initialVelocity;
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
