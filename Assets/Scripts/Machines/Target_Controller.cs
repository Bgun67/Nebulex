using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target_Controller : MonoBehaviour {
	public Animator anim;
	public float speed = 0;
	public MetworkView netView;
	// Use this for initialization
	void Start () {
			anim.SetFloat ("Move Speed", speed);
		netView = this.GetComponent<MetworkView> ();
	}
	
	// Update is called once per frame
	public void OnCollisionEnter (Collision other) {
		if(other.transform.root.GetComponent<Bullet_Controller>()){
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netView.RPC ("RPC_Flip", MRPCMode.All, new object[]{ });
			} else {
				anim.SetTrigger ("Flip");
			}
		}
	}
	[MRPC]
	void RPC_Flip(){
		anim.SetTrigger ("Flip");

	}

	public void Die(){
		anim.SetTrigger ("Flip");

	}
}
