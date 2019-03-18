using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderController : MonoBehaviour {
	public Player_Controller player;
	Vector3 ladderPosition;
	public bool isTop;
	public Transform top;
	public Transform bottom;
	public float v;

	void Start(){
	}
	/*void Update(){
		if (player != null) {
			if (Input.GetAxis ("Move Y") > 0.1f) {
				LeaveLadder ();


			} else {
				v = Input.GetAxis ("Move Z");
				if (Mathf.Abs (player.transform.position.x - top.position.x) > 0.1f) {
					//player.transform.position.x = top.transform.position.x;

				} 
				if (Mathf.Abs (player.transform.position.z - top.position.z) > 0.1f) {
					//player.transform.position.z = top.transform.position.z;

				}

					//this.transform.position += Vector3.up * v/4f;
				player.GetComponent<Rigidbody> ().velocity = this.transform.root.GetComponent<Rigidbody> ().GetPointVelocity(player.transform.position) + Vector3.up * v*2f;
					player.GetComponent<Player_Controller> ().anim.SetFloat ("Move Speed", v);
				//}

			}
		}
	}*/
	void Update(){
		if (player != null) {
			if (Input.GetAxis ("Move Y") > 0.1f||Input.GetButton("Jump")) {
				LeaveLadder ();


			} else {
				v = Input.GetAxis ("Move Z");
				Vector3 ladderDisplacement = top.position - bottom.position;
				float _distanceAlongLadder = Vector3.Dot(player.transform.position - bottom.position, ladderDisplacement)/ladderDisplacement.sqrMagnitude;
				ladderPosition = bottom.position+ladderDisplacement * (_distanceAlongLadder+v*0.01f);
				player.rb.MovePosition (ladderPosition);
				player.anim.SetFloat ("V Movement", v);
				if (_distanceAlongLadder+v*0.1f <= 0.1f)
				{
					OnReachBottom();
				}
				else if ((ladderDisplacement*(_distanceAlongLadder+v*0.01f)).magnitude > ladderDisplacement.magnitude)
				{
					OnReachTop();
				}
			}
		}
	}
	void OnTriggerEnter(Collider other){

		if (other.tag == "Player") {
			if (other.gameObject == player) {
				return;
			} else if (player == null) {
				player = other.transform.root.GetComponent<Player_Controller>();
				player.inVehicle = true;
				player.onLadder = true;
				player.ladder = this;
				player.rb.useGravity = false;
				if (Metwork.peerType != MetworkPeerType.Disconnected) {
					player.GetComponent<MetworkView> ().RPC ("RPC_ClimbLadder", MRPCMode.AllBuffered, new object[]{ });
				} else {
					player.RPC_ClimbLadder();

				}


				if (isTop) {
					bottom.GetComponent<LadderController> ().player = player;
					
				} else {
					top.GetComponent<LadderController> ().player = player;

				}
				player.rb.MoveRotation(this.transform.rotation);
				player.rb.MovePosition(transform.position);



			}
		}
	}


	public void OnReachTop(){
		
		player.rb.MovePosition(top.transform.position + transform.forward * 2f);
		player.rb.MoveRotation(top.transform.rotation);
		LeaveLadder ();

	}

	public void OnReachBottom(){
		
		player.rb.MovePosition(bottom.transform.position + transform.forward * -2f);
		player.rb.MoveRotation(bottom.transform.rotation);
		LeaveLadder ();
	}
	public void LeaveLadder(){
		Player_Controller _player = player.GetComponent<Player_Controller> ();
		_player.onLadder = false;
		_player.ladder = null;

		_player.inVehicle = false;

		_player.rb.useGravity = true;
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			player.GetComponent<MetworkView> ().RPC ("RPC_LeaveLadder", MRPCMode.AllBuffered, new object[]{ });
		} else {
			player.GetComponent<Player_Controller> ().RPC_LeaveLadder();

		}
		_player.EnterGravity ();
		bottom.GetComponent<LadderController> ().player = null;
		top.GetComponent<LadderController> ().player = null;


	}


}
