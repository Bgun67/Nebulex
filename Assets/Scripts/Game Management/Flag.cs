using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour {

	public int team = 0;

	public Transform teamAPosition;
	public Transform teamBPosition;

	public Rigidbody stand;

	public ConfigurableJoint joint;
	public Metwork_Object netObj;
	Game_Controller gameController;
	public BoxCollider boxCollider;
	public Player_Controller _player;
	public float droppedTime;
	public float maxDropTime;
	bool vehicleEntered;



	void Start(){
		gameController = GameObject.FindObjectOfType<Game_Controller> ();
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
			stand.transform.position = this.transform.position;
			boxCollider.enabled = true;

			joint.connectedBody = stand;
			return;
		}
		if (!joint.connectedBody.gameObject.activeInHierarchy) {
			_player = null;
			stand.transform.position = this.transform.position;
			boxCollider.enabled = true;

			joint.connectedBody = stand;
			return;
		}
		if (_player.inVehicle) {
			print ("Invehicle");
			if (!vehicleEntered) {
				vehicleEntered = true;
				//shitty workaround
				foreach (Ship_Controller ship in FindObjectsOfType<Ship_Controller>()) {
					if (ship.player == _player.gameObject) {
						this.transform.position = ship.transform.position;
						this.transform.rotation = ship.transform.rotation;
						joint.connectedBody = ship.GetComponent<Rigidbody> ();
					}
				}
				foreach (Turret_Controller turret in FindObjectsOfType<Turret_Controller>()) {
					if (turret.player == _player.gameObject) {
						this.transform.position = turret.transform.position;
						this.transform.rotation = turret.transform.rotation;
						joint.connectedBody = turret.transform.root.GetComponent<Rigidbody> ();
					}
				}
			}
		} else {
			if (vehicleEntered) {
				vehicleEntered = false;
				this.transform.position = _player.jetpackJets[0].transform.position + new Vector3(0,-1,0);
				this.transform.forward = _player.transform.forward;
				joint.connectedBody = _player.rb;
			}

		}
		
	}

	public void OnTriggerEnter(Collider other){
		if (other.transform.root.GetComponent<Player_Controller> () != null) {
			_player = other.transform.root.GetComponent<Player_Controller> ();
		} else {
			return;
		}

		//Check if the player matches the flag's team
		if (_player.team == this.team) {
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
		_player = gameController.GetPlayerFromNetID (_owner).GetComponent<Player_Controller>();
		//The player picks up the opposing team's flag
		boxCollider.enabled = false;

		this.transform.position = _player.jetpackJets[0].transform.position + new Vector3(0,-1,0);
		this.transform.forward = _player.transform.forward;
		GetComponent<Rigidbody> ().useGravity = false;
		joint.connectedBody = _player.rb;
		netObj.owner = _player.netObj.owner;
	}



	void ReturnFlag(){
		print ("Returning flag");
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netObj.netView.RPC ("RPC_ReturnFlag", MRPCMode.AllBuffered, new object[]{ });
		} else {
			RPC_ReturnFlag ();
		}
	}

	[MRPC]
	void RPC_ReturnFlag(){
		joint.connectedBody = stand;

		if (this.team == 0) {
			this.transform.position = teamAPosition.position;
			this.transform.rotation = teamAPosition.rotation;
		} else {
			this.transform.position = teamBPosition.position;
			this.transform.rotation = teamBPosition.rotation;
		}
		_player = null;
		stand.transform.position = this.transform.position;
		boxCollider.enabled = true;
	}


}
