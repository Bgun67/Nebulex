﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Michael Gunther: 2018-01-25
 * Purpose: Controls the main body of the venom carrier, mostly concerning the engines
 * Notes: 
 * Improvements/Fixes: render a predicted path to the flight deck
 * */

public class Carrier_Controller : MonoBehaviour {

	//This script will have the three engines to see if they are powered up and force at their location
	public Engine_Controller portEngine;
	public Engine_Controller middleEngine;
	public Engine_Controller starboardEngine;
	public bool hasPower = true;
	public Plasma_Cannon cannon;
	//public ParticleSystem plasmaParticles;
	public Transform pilotPosition;

	public GameObject explosionPrefab;
	public GameObject subExplosionPrefab;
	[HideInInspector]
	public Rigidbody rb;

	public float steering = 0f;
	public float thrust = 1E7f;
	public float throttle = 1f;
	public float vertical;
	float exitWait;

	public bool shieldActive;

	Vector3 previousVelocity = Vector3.zero;
	Vector3 acceleration;
	[HideInInspector]
	public GameObject pilot;

	Metwork_Object netObj;
	Metwork_Object playerNetObj;

	public bool threeAxis;
	public bool destroyAfterDeath;
	float lastTime;

	[HideInInspector]
	public PilotAR pilotAR;

	public Light[] lights;

	public void Start(){
		netObj = this.GetComponent<Metwork_Object>();
		pilotAR = GetComponentInChildren<PilotAR>();
		rb = GetComponent<Rigidbody>();
		InvokeRepeating ("SendControls", 0f, 1f);
	}

	public void Activate(GameObject player){
		if (pilot != null) {
			return;
		}
		if (Time.time - lastTime < 2f) {
			return;
		}
		pilot = player;
		playerNetObj = player.GetComponent<Metwork_Object> ();
		player.GetComponent<Player_Controller> ().inVehicle = true;
		exitWait = Time.time + 2f;
		if (playerNetObj.isLocal) {
			pilotAR.gameObject.SetActive( true);
		}

	}
	public void Exit(){
		//exit over the network
		if (playerNetObj.isLocal && Metwork.peerType != MetworkPeerType.Disconnected) {
			netObj.netView.RPC ("RPC_Exit", MRPCMode.OthersBuffered, new object[]{  });
		}
		lastTime = Time.time;
		pilot.GetComponent<Player_Controller> ().inVehicle = false;
		pilot = null;
		playerNetObj = null;
		pilotAR.gameObject.SetActive( false);
		
	}
	[MRPC]
	void RPC_Exit()
	{
		pilot.GetComponent<Player_Controller>().inVehicle = false;
		pilot = null;
		playerNetObj = null;
	}

	public void SendControls(){
		if (pilot != null && playerNetObj.isLocal && !Metwork.isServer && Metwork.peerType != MetworkPeerType.Disconnected) {
			netObj.netView.RPC ("SetControls", MRPCMode.Server, new object[]{ throttle, steering });
		}
	}

	[MRPC]
	public void SetControls(float _throttle, float _steering){
		throttle = _throttle;
		steering = _steering;
	}

	void Update(){
		
		if (pilot != null && playerNetObj.isLocal)
		{
			pilotAR.DrawPredictedPath (rb);
		}
	}

	void FixedUpdate(){

		if (pilot != null && playerNetObj.isLocal)
		{
			pilot.GetComponent<Rigidbody>().velocity = rb.GetPointVelocity(pilotPosition.position);
			pilot.GetComponent<Rigidbody>().MovePosition(pilotPosition.position);
			pilot.GetComponent<Rigidbody>().MoveRotation(pilotPosition.rotation);
			
			if (Input.GetKey(KeyCode.LeftShift))
			{
				thrust = 1.5E7f;
			}
			else
			{
				thrust = 0.75E7f;
			}

			throttle = Mathf.Clamp(throttle + Input.GetAxis("Move Z") / 10f, -1f, 1f);
			steering = Mathf.Clamp(steering + Input.GetAxis("Move X") / 10f, -1f, 1f);
			if (pilotAR)
			{
				pilotAR.ShowGauges(throttle, steering);
			}

			if (threeAxis)
			{
				vertical = Input.GetAxis("Move Y");
			}
			if (Input.GetButtonDown("Use Item") && Time.time > exitWait)
			{
				Exit();
			}
			if (Input.GetButtonDown("Fire1"))
			{
				if (Metwork.peerType != MetworkPeerType.Disconnected)
				{
					netObj.netView.RPC("RPC_Fire", MRPCMode.AllBuffered, new object[] { });
				}
				else
				{
					RPC_Fire();
				}
			}
			if (Input.GetButtonDown("Jump"))
			{
				if (Metwork.peerType != MetworkPeerType.Disconnected)
				{
					netObj.netView.RPC("RPC_ChargeCannon", MRPCMode.AllBuffered, new object[] { });
				}
				else
				{
					RPC_ChargeCannon();
				}
			}

			
		}
		if (!Metwork.isServer && Metwork.peerType != MetworkPeerType.Disconnected) {
			return;
		}

		//Current number of engines firing
		float numEngines = 0f;
		//The engines that provide a rotational force
		float rotEngines = 0f;


		if (portEngine.isRunning) {
			//Add a force to the number 1 engine (Change to force at position)
			numEngines += 1f;
			rotEngines += 1f;
			portEngine.SetThrottle(throttle + steering);
		}

		if (middleEngine!=null&&middleEngine.isRunning) {
			//Add a force to the number 2 engine
			numEngines += 1f;
		}

		if (starboardEngine.isRunning) {
			//Add a force to the number 1 engine (Change to force at position)
			numEngines += 1f;
			rotEngines -= 1f;
			starboardEngine.SetThrottle(throttle - steering);
		}
		if (threeAxis) {
			rb.AddRelativeForce (vertical * thrust * Time.smoothDeltaTime * 30f * Vector3.up);
		}

		//Here we simplify three forces and two moments to one force and one moment to make it easier on our physics engine
		rb.AddRelativeForce (thrust * throttle * numEngines * Time.smoothDeltaTime * 30f * Vector3.forward);
		rb.AddRelativeTorque (thrust * (rotEngines * throttle + steering) * Time.smoothDeltaTime * 50f * Vector3.up);

	}
	[MRPC]
	public void RPC_Fire(){
		cannon.fire = !cannon.fire;
		pilotAR.FireCannon(cannon.fire);
	}
	[MRPC]
	public void RPC_ChargeCannon(){
		cannon.charge = !cannon.charge;
		pilotAR.ChargeCannon(cannon.fire);
	}

	public void OnDie(){
		StartCoroutine (Die ());
	}

	IEnumerator Die(){
		GameObject explosion;
		rb.AddExplosionForce (10000000f, rb.transform.position, 10000f);
		rb.constraints = RigidbodyConstraints.None;

		for (int i = -2; i < 4; i++) {
			explosion = (GameObject)Instantiate (explosionPrefab, this.transform.position + transform.forward * 100f * i, Quaternion.identity);
			explosion.transform.localScale = Vector3.one * 1E8f;
			Destroy (explosion, 3f);
			for (int j = -1; j < 2; j++) {
				explosion = (GameObject)Instantiate (subExplosionPrefab, this.transform.position + transform.forward * (100f * i + 10f * j), Quaternion.identity);
				explosion.transform.parent = this.transform;
				//explosion.transform.localScale = Vector3.one * 0.0001f;
				yield return new WaitForSeconds (0.05f);
			}

			yield return new WaitForSeconds (0.4f);
		}
		yield return new WaitForSeconds (1.5f);
		foreach (Collider collider in Physics.OverlapSphere(this.transform.position, 200f))
		{
			if (collider.GetComponent<Damage>() != null)
			{
				collider.GetComponent<Damage>().TakeDamage(100, 0, transform.position, true);
			}
		}
		if (destroyAfterDeath) {
			yield return new WaitForSeconds (4f);
			//Destroy (this.gameObject);
			gameObject.SetActive(false);
		}
		//gameObject.SetActive (false);
	}

	
	public void ShutdownGravity(){
		this.GetComponentInChildren<Gravity_Controller>().useGravity = false;
		this.GetComponentInChildren<Gravity_Controller>().ignoreInternalObjects = false;
		Time.timeScale = 0.5f;
		hasPower = false;
		Invoke ("IgnoreInternal", 1f);
	}

	public void ReactivateGravity(){
		this.GetComponentInChildren<Gravity_Controller>().useGravity = true;
		this.GetComponentInChildren<Gravity_Controller>().ignoreInternalObjects = false;
		hasPower = true;
		Invoke ("IgnoreInternal", 1f);
	}
	#region shield
	public void ShutdownShield()
	{
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			netObj.netView.RPC("RPC_ShutdownShield", MRPCMode.AllBuffered, new object[] { });
		}
		else
		{
			RPC_ShutdownShield();
		}
	}
	[MRPC]
	public void RPC_ShutdownShield()
	{
		GetComponentInChildren<Poynting_Shield>().StartCoroutine("ShutdownShield");
	}

	public void ReactivateShield()
	{
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			netObj.netView.RPC("RPC_ReactivateShield", MRPCMode.AllBuffered, new object[] { });
		}
		else
		{
			RPC_ReactivateShield();
		}
	}
	[MRPC]
	public void RPC_ReactivateShield()
	{
		GetComponentInChildren<Poynting_Shield>().StartCoroutine("ReactivateShield");
	}
	#endregion
	void IgnoreInternal(){
		Time.timeScale = 1f;
		this.GetComponentInChildren<Gravity_Controller>().ignoreInternalObjects = true;
	}
	public void ShutdownPower(){
		print ("RPC invokes");
		StartCoroutine (TurnOffLights ());
		this.hasPower = false;
		print ("Completed");
	}

	IEnumerator TurnOffLights(){
		lights = transform.GetComponentsInChildren<Light> ();
		for (int i = 0; i<5; i++){
			lights [i].enabled = false;
		}
		yield return new WaitForSeconds (1f);
		for (int j = 5; j<15; j++){
			lights [j].enabled = false;
		}
		yield return new WaitForSeconds (2f);

		for (int k = 15; k<lights.Length; k++){
			lights [k].enabled = false;
		}
	}
	public void ReactivateLights(){
		print ("RPC invokes");
		this.hasPower = true;
		foreach (Light shipLight in lights) {
			shipLight.enabled = true;
		}
		print ("Completed");

	}
	public static void FlashWarningLights(bool _active, Transform carrier)
	{
		GameObject[] _lights = GameObject.FindGameObjectsWithTag("Warning Light");
		foreach (GameObject go in _lights)
		{
			if (carrier == null || go.transform.root == carrier)
			{
				Animator _anim = go.GetComponentInChildren<Animator>();
				if (_anim != null)
				{
					_anim.SetBool("Is Flashing", _active);
				}
			}
		}
	}


}
