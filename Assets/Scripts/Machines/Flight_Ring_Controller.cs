using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flight_Ring_Controller : MonoBehaviour {
	public MetworkView netView;
	// Use this for initialization
	void Start () {
		netView = this.GetComponent<MetworkView> ();
	}

	// Update is called once per frame
	void OnTriggerEnter (Collider other) {
		if (other.transform.root.GetComponent<Ship_Controller> ()) {
			print ("Changing COlor");
			this.GetComponent<MeshRenderer> ().material.color = new Color(0f,1f,0f);
			//TODO other.transform.root.GetComponent<Ship_Controller> ().thrustToMassRatio += 0.3f;
			other.transform.root.GetComponentInChildren<Fire> ().damageFactor += 0.1f;
			Invoke ("ResetColor", 5f);

			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netView.RPC ("RPC_ChangeColorGreen", MRPCMode.Others, new object[]{});
			} 
		}

	}
	[MRPC]
	void RPC_ChangeColorGreen(){
		this.GetComponent<MeshRenderer> ().material.color = new Color(0f,1f,0f);
	}
	void OnCollisionEnter(Collision other){
		if (other.transform.root.GetComponent<Ship_Controller> ()) {
			this.GetComponent<MeshRenderer> ().material.color = new Color(1f,0f,0f);
			//TODO other.transform.root.GetComponent<Ship_Controller> ().thrustToMassRatio = 1f;
			other.transform.root.GetComponentInChildren<Fire> ().damageFactor = 1;
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netView.RPC ("RPC_ChangeColorRed", MRPCMode.Others, new object[]{});
			}

		}
	}
	[MRPC]
	void RPC_ChangeColorRed(){
		this.GetComponent<MeshRenderer> ().material.color = new Color (0f, 1f, 0f);

	}

	void ResetColor(){
		this.GetComponent<MeshRenderer> ().material.color = new Color(0f,0f,1f);
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("RPC_ResetColor", MRPCMode.Others, new object[]{});
		} 

	}
	[MRPC]
	void RPC_ResetColor(){
		this.GetComponent<MeshRenderer> ().material.color = new Color(0f,0f,1f);

	}
}
