using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mite_Controller : MonoBehaviour {
	Rigidbody rb;
	public float hitRightDistance;
	public float hitLeftDistance;
	public float hitFrontDistance;
	public float hitBackDistance;
	public int turning = 0;
	public Player_Controller[] allPlayers;
	public Transform targetedPlayer;
	// Use this for initialization
	void Start () {
		rb = this.GetComponent<Rigidbody> ();
		allPlayers = GameObject.FindObjectsOfType<Player_Controller> ();
	}
	// Update is called once per frame
	void Update () {
		RaycastHit hitRight;
		RaycastHit hitLeft;
		RaycastHit hitFront;
		Physics.Raycast (this.transform.position, transform.right, out hitRight, 16f);
		Physics.Raycast (this.transform.position, -transform.right, out hitLeft, 16f);
		Physics.Raycast (this.transform.position, transform.forward, out hitFront, 16f);

		hitLeftDistance = hitLeft.distance;
		hitRightDistance = hitRight.distance;
		hitFrontDistance = hitFront.distance;

		targetedPlayer = null;
		RaycastHit playerHit;
		foreach (Player_Controller player in allPlayers) {
			Debug.DrawLine (this.transform.position, player.transform.position);
			if(Physics.Raycast(this.transform.position, player.transform.position-this.transform.position, out playerHit)){
				if (playerHit.collider.GetComponent<Player_Controller> ()) {
					print ("Found");
					targetedPlayer = player.transform;
				} else {
					print(playerHit.collider.name);
				}
			}
		}
		if (targetedPlayer != null) {
			rb.angularVelocity = Vector3.zero;
			Vector3 originalTransform = this.transform.rotation.eulerAngles;
			transform.LookAt (targetedPlayer.position);
			transform.rotation = Quaternion.Euler(originalTransform.x, this.transform.eulerAngles.y, originalTransform.z);
		}
		//distance 0.8
		else if (hitFront.distance < 1.2f) {
			
			if (hitLeft.distance >= hitRight.distance) {
				if (turning == 0 || turning == 1) {
					TurnLeft ();
				}
			} else if (hitLeft.distance < hitRight.distance) {
				if (turning == 0 || turning == 2) {
					TurnRight ();
				}
			} else {
				if (Time.frameCount % 2 == 0) {
					TurnLeft ();
				} else {
					TurnRight ();
				}
			}
		
	
		} else {
			turning = 0;
		}
		//transform.position += transform.forward;
		rb.AddRelativeForce (Vector3.forward * 3f );

	}
	void TurnLeft(){
		//this.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler (0f, -90f, 0f), 1f);
		turning = 1;
		rb.AddRelativeTorque(0f, -5f, 0f);
	}
	void TurnRight(){
		//this.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler (0f, 90f, 0f), 1f);
		turning = 2;
		rb.AddRelativeTorque (0f, 5f, 0f);

	}

	//force at 1-distacne at 1 works
	//drag at 2 force at 2 is good
	void OnTriggerEnter(Collider other){
		if (other.tag == "Player") {
			other.GetComponent<Damage> ().TakeDamage (5,0);
		}
	}
}
