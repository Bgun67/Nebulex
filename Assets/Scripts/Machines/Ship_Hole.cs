using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship_Hole : MonoBehaviour {

	public GameObject attachedCarrier;
	// Use this for initialization
	void Start () {
		print (GameObject.FindObjectOfType<Game_Controller> ().localPlayer.name);
		Navigation.RegisterTarget (this.transform, "Weak Point",5f, Color.blue);
	}
	
	// Update is called once per frame
	public void DestroyHole () {
		GetComponent<MetworkView> ().RPC ("RPC_DestroyHole", MRPCMode.AllBuffered, new object[]{ });
	}
	[MRPC]
	public void RPC_DestroyHole(){
		Navigation.DeregisterTarget (this.transform);
		Destroy (this.gameObject);

	}
}
