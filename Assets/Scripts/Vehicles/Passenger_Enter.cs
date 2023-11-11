﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passenger_Enter : MonoBehaviour {	
	float lastTime = 0f;
	float exitWait = 0f;
	public Player_Controller player;
	public Transform seat;
	public Rigidbody rootRB;

	// Use this for initialization
	void Start () {
		rootRB = seat.root.GetComponent<Rigidbody>();
	}

	void FixedUpdate(){
		
		
		if (player== null||!player.isLocalPlayer)
		{
			return;
		}
		player.rb.MovePosition(seat.position);
		player.rb.velocity = rootRB.GetPointVelocity(seat.position);
		player.rb.MoveRotation(seat.rotation);
		if (!player.gameObject.activeInHierarchy)
		{
			Exit();
		}
		if (Input.GetButtonDown ("Use Item") &&Time.time>exitWait) {
			
			Exit();
		}
		if (Input.GetButton("Fire1")){
			//CHECK
			//player.fireScript.FireWeapon();
		}

	}
	public void Exit()
	{
		if (UI_Manager.Instance.vehicleHealthBox != null)
		{
			UI_Manager.Instance.vehicleHealthBox.gameObject.SetActive(false);
		}
		
		transform.root.SendMessage("Exit");
		/*TODO
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			player.netView.RPC("RPCExitVehicle", MRPCMode.AllBuffered, new object[] { });
		}
		else
		{
			player.RPC_ExitVehicle();
		}*/
		player = null;
		lastTime = Time.time;
		print("Disconnecting");
	}

	public void Activate(GameObject _player){
		if (Time.time - lastTime < 2f) {
			return;
		}
		if (UI_Manager.Instance.vehicleHealthBox != null)
		{
			UI_Manager.Instance.vehicleHealthBox.gameObject.SetActive(true);
		}
		if (player == null||!player.isLocalPlayer) {
			player = _player.GetComponent<Player_Controller>();
			//player.rb.isKinematic = true;
			player.inVehicle = true;
			player.airTime = 20000f;
			CapsuleCollider[] capsules = player.GetComponents<CapsuleCollider> ();
			capsules[0].enabled = false;
			capsules[1].enabled = false;
			/*TODO
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				player.netObj.netView.RPC ("RPC_Sit", MRPCMode.AllBuffered, new object[]{ true});
			} else {
				player.RPC_Sit (true);
			}*/
			exitWait = Time.time + 2f;
		}
	}
	

}
