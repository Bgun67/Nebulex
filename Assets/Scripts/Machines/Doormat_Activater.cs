using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Doormat_Activater : MonoBehaviour {
	[Tooltip("Required BUTTON pressed leave blank for none")]
	public string inputRequired;
	[Tooltip("Text to display when player enters leave blank for none")]
	public string displayedText;
	public bool entered;
	public GameObject enteringPlayer;
	public MonoBehaviour[] scriptsToActivate;
	public Text displayTxt;

	public MetworkView netView;
	// Use this for initialization
	void Start () {
		netView = this.GetComponent<MetworkView> ();
	}
	
	//needs local component
	void OnTriggerEnter (Collider other) {
		if (other.tag == "Player") {
			if (inputRequired == "") {
				ActivateScript (other.gameObject);
			} else {
				entered = true;
				enteringPlayer = other.gameObject;
				DisplayText ();

			}

		}
	}
	void OnTriggerExit (Collider other) {
		if (other.tag == "Player") {
			if (inputRequired == "") {
			} else {
				entered = false;
			}
		}
		HideText ();

	}
	void Update(){
		if (entered) {
			if (Input.GetButtonDown (inputRequired)) {
				HideText ();
				entered = false;
				ActivateScript (enteringPlayer);


			}
		}
	}
	public void ActivateScript(GameObject player){
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("RPC_ActivateScript", MRPCMode.AllBuffered, new object[]{ player.GetComponent<Metwork_Object> ().netID });
		} else {
			RPC_ActivateScript (player.GetComponent<Metwork_Object> ().netID);
		}

	

	}
	[MRPC]
	public void RPC_ActivateScript(int _netID){
		GameObject player = Game_Controller.GetGameObjectFromNetID (_netID);

		foreach(MonoBehaviour scriptToActivate in scriptsToActivate){
			print ("Activating" + scriptToActivate.name);
			scriptToActivate.enabled = true;
			scriptToActivate.SendMessage ("Activate", player);

		}
		return;
	}

	public void DisplayText(){
		displayTxt.gameObject.SetActive (true);
		displayTxt.text = displayedText;
	}
	public void HideText(){
		displayTxt.gameObject.SetActive (false);
	}
}
