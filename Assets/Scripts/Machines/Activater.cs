using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Activater : MonoBehaviour {
	public MonoBehaviour[] scriptsToActivate;
	public int maxPassengers;
	public int passengers;
	public MetworkView netView;
	
	// Use this for initialization
	void Start () {
		netView = GetComponent<MetworkView> ();
	}

	public void ActivateScript(GameObject player){
		if (maxPassengers == 0) {

			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netView.RPC ("RPC_ActivateScript", MRPCMode.AllBuffered, new object[]{ player.GetComponent<Metwork_Object> ().netID });
			} else {
				RPC_ActivateScript (player.GetComponent<Metwork_Object> ().netID);
			}


		}
		else if (passengers < maxPassengers) {
			if (passengers < 1) {
				foreach (MonoBehaviour scriptToActivate in scriptsToActivate) {
					
					scriptToActivate.enabled = true;
					try {
						//	scriptToActivate.gameObject.GetComponent<Metwork_Object> ().owner = player.GetComponent<Metwork_Object> ().owner;
					} catch {

					}
					scriptToActivate.SendMessage ("Activate", player);

				}

			} else {
				//foreach (MonoBehaviour scriptToActivate in scriptsToActivate) {
					
				//	scriptToActivate.SendMessage ("AddPassenger", player);
				//}
			}
		}

		//functionToRun.Invoke(player);
	}

	[MRPC]
	public void RPC_ActivateScript(int _netID){
		GameObject player = FindObjectOfType<Game_Controller>().GetPlayerFromNetID (_netID);

		foreach(MonoBehaviour scriptToActivate in scriptsToActivate){
			print ("Activating" + scriptToActivate.name);
			scriptToActivate.enabled = true;
			scriptToActivate.SendMessage ("Activate", player);

		}
		return;
	}

	public void DeactivateScript(GameObject player){
		if (maxPassengers == 0) {
			netView.RPC ("RPC_DeactivateScript", MRPCMode.AllBuffered, new object[]{ });

		}
		if (passengers < 1) {
			foreach (MonoBehaviour scriptToActivate in scriptsToActivate) {
				
			scriptToActivate.enabled = false;
			//scriptToActivate.GetComponent<Metwork_Object> ().owner = 0;
			}
		}


	}

	[MRPC]
	public void RPC_DeactivateScript(){
		foreach (MonoBehaviour scriptToActivate in scriptsToActivate) {

			scriptToActivate.enabled = false;
		}
		return;
	}

	//[MRPC]
	public void AddPassenger(){
		passengers++;
	}
	//[MRPC]
	public void RemovePassenger(){
		passengers--;
	}
}
