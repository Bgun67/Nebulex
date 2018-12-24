using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stair_Controller : MonoBehaviour {

	public float stepHeight;



	// Update is called once per frame
	void OnTriggerEnter(Collider other) {
		if (other.transform.root.tag == "Player") {
			other.transform.root.GetComponent<Player_Controller> ().onStairs = true;
			other.transform.root.GetComponent<Player_Controller> ().currentStepHeight = stepHeight;
			//other.gameObject.GetComponent<Player_Controller> ().currentStepHeight = stepHeight;
		}
	}
	void OnTriggerExit(Collider other){
		if (other.transform.root.tag == "Player") {
			//other.gameObject.GetComponent<Player_Controller> ().onStairs = false;

		}


	}
}
