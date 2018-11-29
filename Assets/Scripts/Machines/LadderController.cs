using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderController : MonoBehaviour {
	public GameObject player;
	public bool isTop;

	public Transform top;
	public Transform bottom;
	public float v;

	void Start(){
		
	}
	void Update(){
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
	}
	void OnTriggerEnter(Collider other){

		if (other.tag == "Player") {
			if (other.gameObject == player) {
				if (isTop) {
					print ("Reached TOp");
					OnReachTop ();
				} else {
					print ("Reached Bottom");

					OnReachBottom ();
				}
			} else if (player == null) {
				print ("New PLayer");
				player = other.transform.root.gameObject;

				player.GetComponent<Player_Controller> ().inVehicle = true;
				player.GetComponent<Player_Controller> ().onLadder = true;
				player.GetComponent<Player_Controller> ().ladder = this;

				player.GetComponent<Rigidbody> ().velocity = this.transform.root.GetComponent<Rigidbody> ().GetPointVelocity(player.transform.position);
				player.GetComponent<Rigidbody> ().useGravity = false;
				player.GetComponent<Rigidbody> ().drag = 0f;
				if (Metwork.peerType != MetworkPeerType.Disconnected) {
					player.GetComponent<MetworkView> ().RPC ("RPC_ClimbLadder", MRPCMode.AllBuffered, new object[]{ });
				} else {
					player.GetComponent<Player_Controller> ().RPC_ClimbLadder();

				}


				if (isTop) {
					bottom.GetComponent<LadderController> ().player = player;
					player.transform.SetPositionAndRotation (this.transform.position - transform.up * 3f, this.transform.rotation);
				} else {
					top.GetComponent<LadderController> ().player = player;

					player.transform.SetPositionAndRotation (this.transform.position + Vector3.up*2.5f, this.transform.rotation);
				}



			}
		}
	}


	public void OnReachTop(){
		
		player.transform.SetPositionAndRotation (top.transform.position+transform.forward*4f, top.transform.rotation);
		LeaveLadder ();

	}

	public void OnReachBottom(){
		
		player.transform.SetPositionAndRotation (bottom.transform.position+transform.forward*-2f, bottom.transform.rotation);
		LeaveLadder ();
	}
	public void LeaveLadder(){
		Player_Controller _player = player.GetComponent<Player_Controller> ();
		_player.onLadder = false;
		_player.ladder = null;

		_player.inVehicle = false;

		_player.rb.useGravity = true;

		_player.rb.drag = 1f;

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
