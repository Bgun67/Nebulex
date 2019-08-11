using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour {

	public int team = 0;

	public Transform teamAPosition;
	public Transform teamBPosition;

	public Rigidbody stand;

	public Rigidbody rb;
	public Metwork_Object netObj;
	Game_Controller gameController;
<<<<<<< HEAD
	public BoxCollider boxCollider;
	public Player_Controller _player;
=======
	public Transform target;
	public Player_Controller player;
>>>>>>> Local-Git
	public float droppedTime;
	public float maxDropTime;
	bool vehicleEntered;



	void Start(){
		gameController = GameObject.FindObjectOfType<Game_Controller> ();
		rb = GetComponent<Rigidbody>();
		ReturnFlag ();
		if (gameController.gameMode != Game_Controller.GameType.CTF) {
			this.gameObject.SetActive (false);
			teamAPosition.gameObject.SetActive (false);
			teamBPosition.gameObject.SetActive (false);
			stand.gameObject.SetActive (false);
		}
			
	}

	public void StartGame(){
		gameController = GameObject.FindObjectOfType<Game_Controller> ();
		ReturnFlag ();
		Invoke ("Register", 2f);

	}
	
	void Register(){
		char flagChar;
		if (team == 0) {
			flagChar = 'A';

		} else {
			flagChar = 'B';
		}
		Navigation.RegisterTarget (this.transform, "Flag " + flagChar.ToString(), 5f, Color.green);

	}
	void Update(){
<<<<<<< HEAD
		if (_player == null) {
			if (Metwork.isServer)
			{
				droppedTime += Time.deltaTime;
				if (droppedTime > maxDropTime)
				{
					ReturnFlag();
				}
			}
			return;
		}
		if (joint.connectedBody==null) {
			_player = null;
=======
		if (target==null) {
			
			player = null;
>>>>>>> Local-Git
			stand.transform.position = this.transform.position;

			target = stand.transform;
			return;
		}
		rb.MovePosition(Vector3.Lerp(transform.position,target.position,0.9f));
		rb.MoveRotation(Quaternion.Lerp(transform.rotation,target.rotation,0.9f));
		if (player == null) {
			if (Metwork.isServer)
			{
				droppedTime += Time.deltaTime;
				if (droppedTime > maxDropTime)
				{
					ReturnFlag();
				}
			}
			return;
		}
<<<<<<< HEAD
		
		if (_player.inVehicle) {
=======
	
		
		if (player.inVehicle) {
>>>>>>> Local-Git
			print ("Invehicle");
			if (!vehicleEntered) {
				vehicleEntered = true;
				//shitty workaround
				foreach (Ship_Controller ship in FindObjectsOfType<Ship_Controller>()) {
					if (ship.player == player.gameObject) {
						target = ship.transform;
					}
				}
				foreach (Turret_Controller turret in FindObjectsOfType<Turret_Controller>()) {
					if (turret.player == player.gameObject) {
						target = turret.transform;
					}
				}
			}
		} else
		{
<<<<<<< HEAD
			if (!joint.connectedBody.gameObject.activeInHierarchy)
			{
				_player = null;
				stand.transform.position = this.transform.position;
				boxCollider.enabled = true;

				joint.connectedBody = stand;
=======
			if (!target.gameObject.activeInHierarchy)
			{
				player = null;
				stand.transform.position = this.transform.position;
				target = stand.transform;
>>>>>>> Local-Git
				return;
			}
			if (vehicleEntered)
			{
				vehicleEntered = false;
<<<<<<< HEAD
				this.transform.forward = _player.transform.forward;
				joint.connectedBody = _player.rb;
				this.transform.position = _player.jetpackJets[0].transform.position + _player.jetpackJets[0].transform.forward;
=======
				this.transform.forward = player.transform.forward;
				target = player.flagPosition;
>>>>>>> Local-Git

			}

		}
<<<<<<< HEAD


=======
		



>>>>>>> Local-Git
	}

	public void OnTriggerEnter(Collider other){
		Player_Controller _player = other.transform.root.GetComponent<Player_Controller> ();
		if (player!= null || _player== null) {
			return;
		}
		print("Trigger Enter");

		//Check if the player matches the flag's team
		if (_player.GetTeam() == this.team) {
			//The flag returns to it's rightful owner
			ReturnFlag();

		} else {
			//Pickup the flag
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netObj.netView.RPC ("RPC_PickupFlag", MRPCMode.AllBuffered, new object[]{_player.netObj.netID });
			} else {
				RPC_PickupFlag (_player.netObj.netID);
			}
		}
	}

	[MRPC]
	void RPC_PickupFlag(int _owner){
		print ("Picking up flag");
		player = gameController.GetPlayerFromNetID (_owner).GetComponent<Player_Controller>();
		//The player picks up the opposing team's flag

		target = player.jetpackJets[0].transform;
		this.transform.forward = player.transform.forward;
		netObj.owner = player.netObj.owner;
	}



	void ReturnFlag(){
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netObj.netView.RPC ("RPC_ReturnFlag", MRPCMode.AllBuffered, new object[]{ });
		} else {
			RPC_ReturnFlag ();
		}
	}

	[MRPC]
	void RPC_ReturnFlag(){
		target = stand.transform;

		if (this.team == 0) {
			stand.MovePosition(teamAPosition.position);
			stand.MoveRotation(teamAPosition.rotation);
		} else {
			stand.MovePosition(teamBPosition.position);
			stand.MoveRotation(teamBPosition.rotation);
		}
<<<<<<< HEAD
		_player = null;
		stand.transform.position = this.transform.position;
		boxCollider.enabled = true;
=======
		player = null;
>>>>>>> Local-Git
	}


}
