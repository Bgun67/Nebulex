using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kill_Area : MonoBehaviour {

	void OnTriggerExit(Collider other){
		try{
			other.GetComponentInParent<Damage>().currentHealth = -10;
			other.GetComponentInParent<Damage>().dieFunction.Invoke();

		}
		catch{
			//TODO: Bullets don't need to be destroyed
			//Destroy (other.gameObject);
		}
	}
}
