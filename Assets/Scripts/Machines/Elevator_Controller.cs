using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Michael Gunther: 2018-02-05
 * Purpose: Controls the cargo elevators
 * Notes: 
 * Improvements/Fixes: Center the position vector of the elevator and create one model for it
 * */

public class Elevator_Controller : MonoBehaviour {


	public enum Elevator_State{
		Ascending,
		Descending,
		Top,
		Bottom,
		Stopped
	}

	public Elevator_State state = Elevator_State.Bottom;

	public bool hasPower = true;

	public Transform top;
	public Transform bottom;
	public Carrier_Controller ship;


	public float rate = 2f;

	void Start(){
		ship = transform.root.GetComponent<Carrier_Controller> ();

	}
	// Use this for initialization
	void Activate () {
		if (state == Elevator_State.Ascending||state == Elevator_State.Top) {
			state = Elevator_State.Descending;
		} else {
			state = Elevator_State.Ascending;

		}
	}
	
	// Update is called once per frame
	void Update () {
		
		if (!ship.hasPower) {
			return;
		}

		if (state == Elevator_State.Ascending) {
			//if the elevator is not pretty close to its top position
			if ((transform.position.y - top.position.y) > 0.05f) {
				state = Elevator_State.Top;
			} else {
				//move the elevator up
				transform.position = Vector3.Lerp(transform.position,transform.position + Vector3.up * 0.02f * Time.deltaTime * 30f * rate,0.5f);
			}

		}

		if (state == Elevator_State.Descending) {
			//if the elevator is not pretty close to its bottom position
			if ((transform.position.y - bottom.position.y) < 0.05f) {
				state = Elevator_State.Bottom;
			} else {
				//move the elevator down
				transform.position = Vector3.Lerp(transform.position,transform.position - Vector3.up * 0.02f * Time.deltaTime * 30f * rate,0.5f);
			}

		}


	}

	void OnCollisionEnter(Collision other){
		other.gameObject.GetComponent<Rigidbody> ().WakeUp ();
	}
}
