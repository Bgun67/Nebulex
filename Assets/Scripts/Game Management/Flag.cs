using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour {

	public int team = 0;

	public Transform teamAPosition;
	public Transform teamBPosition;

	public Rigidbody stand;

	[HideInInspector] public Rigidbody rb;
	public Metwork_Object netObj;
	Game_Controller gameController;
	public Transform target;
	public Player_Controller player;
	public float droppedTime;
	public float maxDropTime;
	bool vehicleEntered;



	void Start(){
		gameController = Game_Controller.Instance;
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
		if (target==null) {
			
			player = null;
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
	
		
		if (player.inVehicle) {
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
			if (!target.gameObject.activeInHierarchy)
			{
				player = null;
				stand.transform.position = this.transform.position;
				target = stand.transform;
				return;
			}
			if (vehicleEntered)
			{
				vehicleEntered = false;
				this.transform.forward = player.transform.forward;
				target = player.flagPosition;

			}

		}
		



	}

	public void OnTriggerEnter(Collider other){
		Player_Controller _player = other.transform.root.GetComponent<Player_Controller> ();
		if (player!= null || _player== null) {
			return;
		}

		//Check if the player matches the flag's team
		if (_player.GetTeam() == this.team) {
			//The flag returns to it's rightful owner
			ReturnFlag();

		} else {
			//Pickup the flag
			/*TODO
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netObj.netView.RPC ("RPC_PickupFlag", MRPCMode.AllBuffered, new object[]{_player.netObj.netID });
			} else {
				RPC_PickupFlag (_player.netObj.netID);
			}*/
		}
	}

	[MRPC]
	void RPC_PickupFlag(int _owner){
		print ("Picking up flag");
		player = gameController.GetPlayerFromNetID (_owner).GetComponent<Player_Controller>();
		//The player picks up the opposing team's flag

		target = player.jetpackJets[0].transform;
		this.transform.forward = player.transform.forward;
		//TODO
		//netObj.owner = player.netObj.owner;
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
		player = null;
	}


}
