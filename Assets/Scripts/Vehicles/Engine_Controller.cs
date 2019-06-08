using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine_Controller : MonoBehaviour {
	public bool isRunning = true;
	public ParticleSystem engineFlame;
	public ParticleSystem.MainModule main;
	float originalLifeTime = 5f;

	void Start(){
		main = engineFlame.main;
		originalLifeTime = main.startLifetime.constant;
	}

	public void SetThrottle(float _throttle){
		main.startLifetime = originalLifeTime * Mathf.Clamp01(_throttle);
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
