using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag_Port : MonoBehaviour {

	public int team;
	Game_Controller gameController;

	void Start(){
		gameController = GameObject.FindObjectOfType<Game_Controller> ();
	}

	void OnTriggerEnter(Collider other){
		Flag _flag = other.GetComponent<Flag> ();

		//Check if the object is a flag
		if (_flag == null) {
			return;
		}
		//If I have been brought the flag of the opposite team
		if (_flag.team != this.team) {
			//Swallow it forever
			gameController.overrideWinner = this.team;
			gameController.EndGame ();
		}
	}
}
