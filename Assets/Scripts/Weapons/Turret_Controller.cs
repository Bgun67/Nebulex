using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret_Controller : MonoBehaviour {

	public GameObject turretPivot;
	float h;
	float v;
	public Animator anim;
	public Fire primary1;
	public Fire primary2;
	public GameObject mainCamera;
	public GameObject player;
	public MetworkView netView;
	public GameObject explosionEffect;
	public GameObject destroyedPrefab;

	public virtual void Activate(GameObject pilot){
		//Switch to the internal camera
		print(pilot.name);
		if (player == null) {
			pilot.GetComponent<Player_Controller> ().mainCamObj.SetActive (false);
			netView = this.GetComponent<MetworkView> ();


			if (pilot.GetComponent<Metwork_Object> ().isLocal) {
				this.mainCamera.SetActive (true);
				this.GetComponent<Damage> ().healthShown = true;
				this.GetComponent<Damage> ().UpdateUI ();
			

				if (Metwork.peerType != MetworkPeerType.Disconnected) {
					netView.RPC ("RPC_Activate", MRPCMode.AllBuffered, pilot.GetComponent<Metwork_Object> ().owner);
				} else {
					RPC_Activate (pilot.GetComponent<Metwork_Object> ().owner);
				}
			}
		}


	}
	[MRPC]
	public void RPC_Activate(int _pilot){
		player = FindObjectOfType<Game_Controller>().GetPlayerFromNetID (_pilot);
		player.SetActive (false);
		player.GetComponent<Player_Controller> ().inVehicle = true;
		this.enabled = true;

		primary1.playerID = _pilot;
		primary2.playerID = _pilot;

	}
	public void Exit(){
		//Switch to the player camera
		player.GetComponent<Player_Controller>().mainCamObj.SetActive (true);
		this.mainCamera.SetActive(false);

		GetComponent<Damage> ().healthShown = false;
		GetComponent<Damage> ().UpdateUI ();




		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			//Make the owner the server
			netView.RPC ("RPC_Exit", MRPCMode.AllBuffered, new object[]{});
		} else {
			RPC_Exit();
		}
		this.enabled = false;

	}
	[MRPC]
	public void RPC_Exit(){
		print (player.name);
		GetComponent<Damage> ().healthShown = false;
		GetComponent<Damage> ().UpdateUI ();
		//move player away from the object
		player.transform.position = transform.position + transform.right * 5f;
		player.gameObject.SetActive (true);
		player.GetComponent<Player_Controller> ().inVehicle = false;

		player = null;

	}
	public void Die(){
		print ("Dying: turret");

		if(Metwork.peerType  != MetworkPeerType.Disconnected){
			print ("RUnning online thing");
			if (player != null) {
				netView.RPC ("RPC_Die", MRPCMode.AllBuffered, new object[]{ player.GetComponent<Metwork_Object> ().netID });
			} else {
				netView.RPC ("RPC_Die", MRPCMode.AllBuffered, new object[]{0 });
			}
		}
		else{
			print ("RUnnnning offline");
			if (player != null) {
				RPC_Die (player.GetComponent<Metwork_Object> ().netID);
			} else {
				RPC_Die (0);
			}
		}

	}
	[MRPC]
	public void RPC_Die(int id){
		print ("Net Dying: turret");
		Destroy(Instantiate (explosionEffect, this.transform.position, transform.rotation),5f);
		GameObject destroyedTurret =  Instantiate (destroyedPrefab, this.transform.position, transform.rotation);
		destroyedTurret.transform.parent = this.transform.parent;
		Destroy (destroyedTurret, 5f);
		gameObject.SetActive(false);

		if (player != null) {
			Exit ();
			player = FindObjectOfType<Game_Controller> ().GetPlayerFromNetID (id);
			player.GetComponent<Damage> ().TakeDamage (1000, 0);

			//tmpPlayer.GetComponent<Player_Controller> ().Die ();

		}
		this.GetComponent<Damage> ().Reset();

		this.enabled = false;

	}
	// Update is called once per frame
	void Update () {
		if (player.GetComponent<Metwork_Object> ().isLocal) {
			h = -MInput.GetAxis ("Rotate Y");
			v = -MInput.GetAxis ("Rotate X");
			Look ();
			if (Input.GetButton ("Fire1")) {
				primary1.FireWeapon ();
				primary2.FireWeapon ();
			}
			if (Input.GetButtonDown ("Use Item")) {
				Exit ();
			}
		}

	}
	[MRPC]
	public void RPC_Turn(float turn, float currentTime){
		StartCoroutine (AdjustView (turn,currentTime));

	}
	public IEnumerator AdjustView(float hTime, float time){
		float lookTime = anim.GetCurrentAnimatorStateInfo (0).normalizedTime;
		if (lookTime < time) {
			anim.SetFloat ("Look Speed", 1f);
		} else if(lookTime>time) {
			anim.SetFloat ("Look Speed", -1f);
		}

		yield return new WaitUntil (() => Mathf.Abs (anim.GetCurrentAnimatorStateInfo (0).normalizedTime - time) < 0.05f);
		anim.Play ("Aim Up/Down", 0, time);
		anim.SetFloat ("Look Speed",0f);

		float turnTime = anim.GetCurrentAnimatorStateInfo (1).normalizedTime;
		if (lookTime < hTime) {
			anim.SetFloat ("Turn Speed", 1f);
		} else if(lookTime>hTime) {
			anim.SetFloat ("Turn Speed", -1f);
		}

		yield return new WaitUntil (() => Mathf.Abs (anim.GetCurrentAnimatorStateInfo (1).normalizedTime - hTime) < 0.05f);
		anim.Play ("Turn", 1, hTime);
		anim.SetFloat ("Turn Speed",0f);
	}
	void Look(){
		float lookUpDownTime = anim.GetCurrentAnimatorStateInfo (0).normalizedTime;


		if (v < 0f) {
			if (lookUpDownTime > 0f) {
				anim.SetFloat ("Look Speed", v);
			} else {
				anim.SetFloat ("Look Speed", 0f);
			}
		} else if (v > 0f) {
			if (lookUpDownTime <= 1f) {
				anim.SetFloat ("Look Speed", v);
			} else {
				anim.SetFloat ("Look Speed", 0f);
			}
		} else {
			anim.SetFloat ("Look Speed", 0f);
		}
		anim.SetFloat ("Turn Speed", h);
		float turnTime = anim.GetCurrentAnimatorStateInfo (1).normalizedTime;


		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("RPC_Turn", MRPCMode.Others, new object[]{
				turnTime,
				lookUpDownTime});
		} 
	}
}
