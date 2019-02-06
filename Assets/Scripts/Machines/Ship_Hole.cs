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
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			GetComponent<MetworkView>().RPC("RPC_DestroyHole", MRPCMode.AllBuffered, new object[] { });
		}
		else
		{
			RPC_DestroyHole();
		}
	}
	[MRPC]
	public void RPC_DestroyHole(){
		Destroy (this.gameObject);
	}
	void OnDestroy()
	{
		Navigation.DeregisterTarget (this.transform);
	}
}
