using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine_Controller : MonoBehaviour {
	public bool isRunning = true;
	public ParticleSystem engineFlame;

	void Start(){

	}

	public void ShutdownEngine(){
		print ("RPC invokes");
		isRunning = false;
		engineFlame.Stop ();
		print ("Completed");

	}
	public void ReactivateEngine(){
		print ("RPC invokes");
		this.isRunning = true;
		engineFlame.Play ();

	}


}
