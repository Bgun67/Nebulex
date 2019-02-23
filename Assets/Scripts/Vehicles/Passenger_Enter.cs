using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passenger_Enter : MonoBehaviour {	
	float lastTime = 0f;
	float exitWait = 0f;
	Player_Controller player;
	public Transform seat;

	// Use this for initialization
	void Start () {

	}

	void FixedUpdate(){
		if (player== null||!player.netObj.isLocal)
		{
			return;
		}
		player.rb.MovePosition(seat.position);
		player.rb.MoveRotation(seat.rotation);
		if (!player.gameObject.activeInHierarchy)
		{
			Exit();
		}
		if (Input.GetButtonDown ("Use Item") &&Time.time>exitWait) {
			Exit();
		}
	
	}
	void Exit()
	{
		player.airTime = player.suffocationTime;

		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{

			player.netView.RPC("RPC_Sit", MRPCMode.AllBuffered, new object[] { false });
		}
		else
		{
			player.RPC_Sit(false);
		}
		player.inVehicle = false;
		player.rb.isKinematic = false;
		player.ExitGravity();
		CapsuleCollider[] capsules = player.GetComponents<CapsuleCollider>();
		capsules[0].isTrigger = false;
		capsules[1].isTrigger = false;
		player = null;
		lastTime = Time.time;
		print("Disconnecting");
	}

	void Activate(GameObject _player){
		if (Time.time - lastTime < 2f) {
			return;
		}
		if (player == null) {
			player = _player.GetComponent<Player_Controller>();
			player.rb.isKinematic = true;
			player.inVehicle = true;
			player.airTime = 20000f;
			CapsuleCollider[] capsules = player.GetComponents<CapsuleCollider> ();
			capsules[0].isTrigger = true;
			capsules[1].isTrigger = true;
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				player.netObj.netView.RPC ("RPC_Sit", MRPCMode.AllBuffered, new object[]{ true});
			} else {
				player.RPC_Sit (true);
			}
			exitWait = Time.time + 2f;
		}
	}
	

}
