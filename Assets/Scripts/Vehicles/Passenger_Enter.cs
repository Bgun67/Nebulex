using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passenger_Enter : MonoBehaviour {
	[System.Serializable]
	public struct Seat{
		public ConfigurableJoint joint;
		public Transform _transform;
	}

	float lastTime = 0f;
	float exitWait = 0f;

	public Seat seat;

	// Use this for initialization
	void Start () {

	}

	void Update(){
		if (Input.GetButtonDown ("Use Item") && seat.joint.connectedBody != null&&Time.time>exitWait) {
			Player_Controller player = seat.joint.connectedBody.GetComponent<Player_Controller>();
			player.airTime = player.suffocationTime;

			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				
				player.netView.RPC ("RPC_Sit", MRPCMode.AllBuffered, new object[]{ false});
			} else {
				player.RPC_Sit (false);
			}
			seat.joint.connectedBody = null;
			player.transform.position =player.transform.position+ seat._transform.forward * 1f + seat._transform.up * 2f;
			player.inVehicle = false;
			if (player.GetComponent<Rigidbody> ().useGravity) {
				player.EnterGravity ();
				player.transform.forward =  new Vector3 (player.transform.forward.x, 0f, player.transform.forward.z);
			} else {
				player.ExitGravity ();

			}


			lastTime = Time.time;
			print ("Disconnecting");

		}
	}

	void Activate(GameObject _player){
		if (Time.time - lastTime < 2f) {
			return;
		}

		if (seat.joint.connectedBody == null) {

			_player.GetComponent<Rigidbody> ().isKinematic = true;
			_player.transform.position = seat._transform.position;
			_player.transform.rotation = seat._transform.rotation;
			_player.GetComponent<Rigidbody> ().isKinematic = false;
			_player.GetComponent<Player_Controller> ().inVehicle = true;

			_player.GetComponent<Player_Controller> ().airTime = 20000f;
			seat.joint.connectedBody = _player.GetComponent<Rigidbody> ();

			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				_player.GetComponent<MetworkView>().RPC ("RPC_Sit", MRPCMode.AllBuffered, new object[]{ true});
			} else {
				_player.GetComponent<Player_Controller>().RPC_Sit (true);
			}
			exitWait = Time.time + 2f;
				
		
		}
	}
	

}
