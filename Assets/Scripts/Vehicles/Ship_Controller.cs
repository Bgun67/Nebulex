﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/*Michael Gunther: 2018-02-05
 * Purpose: Ship class to be derived from to make the fighter ship, bombers etc
 * Notes: 
 * Improvements/Fixes:
 * */

public class Ship_Controller : NetworkBehaviour {

	//Components
	protected Rigidbody rb;
	public Metwork_Object netObj;
	public GameObject mainCamera;
	public Player_Controller player;
	public List<Vector3> route;

	public GameObject[] destroyedPrefabs;
	public GameObject explosionEffect;

	public float thrust;
	[Tooltip("Factor of the thrust that can rotate the ship")]
	public float torqueFactor = 0.4f;
	public float upAngle;
	protected float moveX;
	protected float moveY;
	protected float moveZ;
	protected float deltaThrustForce;

	public Fire fireScriptLeft;
	public Fire fireScriptRight;
	public bool isAI = false;
	public bool isTransport = false;
	public Transform target;
	public AudioSource engineSound;
	protected float previousEnginePitch;
	public Animator anim;
	//Engine Exhaust
	[Header("Engine Exhaust")]
	public ParticleSystem[] bottomThrusters;
	public ParticleSystem[] rearThrusters;
	public Transform[] passengerSeats;
	bool holdPosition = false;
	Vector3 holdLocation;
	bool distanceNotified = false;

	 float emptyTime; 
	Damage damageScript;
	bool landMode = false;
	float getInTime = -1f;


	// Use this for initialization
	void Start () {
		rb = this.GetComponent<Rigidbody> ();
		netObj = this.GetComponent<Metwork_Object> ();
		anim = this.GetComponent<Animator> ();
		engineSound = this.GetComponent<AudioSource> ();
		InvokeRepeating ("FindDirection", 0f, 2f);
		InvokeRepeating ("CheckOccupied", 1f, 1f);
		damageScript = this.GetComponent<Damage> ();
		this.enabled = false;
		engineSound.pitch = Mathf.Clamp(Mathf.Abs(moveZ+moveX+moveY),0,0.1f) + (Time.frameCount % 5f)*0.003f  + 0.85f;
		previousEnginePitch = engineSound.pitch;

	}

	void CheckOccupied(){
		if (!(Metwork.isServer || Metwork.peerType == MetworkPeerType.Disconnected)) {
			return;
		}
		if (this.player == null && Vector3.SqrMagnitude (damageScript.initialPosition.position - transform.position) > 200f) {
			emptyTime += 1f;
			if (emptyTime > 120f)
			{
				AddDamage();
			}
		}
		else
		{
			emptyTime = 0f;
		}
	}

	void AddDamage(){
		if (damageScript.currentHealth > 0) {
			damageScript.TakeDamage (100, 0, transform.position);
		}

	}
	
	// Update is called once per frame
	void Update () {
		if (netObj == null) {
			netObj = this.GetComponent<Metwork_Object> ();
		}
		if (!netObj.isLocal) {
			return;
		}
		if (isAI) {
			AI ();
		} else {
			Fly ();
		}

		try{ SimulateParticles (); } catch{}
		moveY = Input.GetAxis ("Move Y");
		moveZ = Input.GetAxis ("Move Z");
		deltaThrustForce = Time.deltaTime * 35f * thrust;
		rb.velocity = Vector3.ClampMagnitude (rb.velocity, 1000f);
		rb.angularVelocity = Vector3.ClampMagnitude (rb.angularVelocity, 10f);


		if (Input.GetButton ("Fire1")) {
			Fire ();
		}
		if(Input.GetButtonDown("Use Item") && Time.time - getInTime > 1f){
			Exit ();
		}
		if (Input.GetButtonDown("Jump"))
		{
			if (isTransport)
			{
				LowerRamp();
			}
			else
			{
				EnableNightVision();
			}
		}



	}
	void EnableNightVision()
	{
		Night_Vision_Effects nightVision = mainCamera.GetComponent<Night_Vision_Effects>();
		if (nightVision != null)
		{
			nightVision.enabled = !nightVision.enabled;

		}
	}
	void LowerRamp()
	{
		transform.GetComponentInChildren<Door_Controller>().GetComponent<Activater>().ActivateScript(player.gameObject);
	}
	

	protected virtual void Fly(){
		rb.AddRelativeForce (0f,moveY *deltaThrustForce* 2f,
			moveZ *deltaThrustForce);
		
		if(rb.useGravity){
			if(Vector3.Project(rb.velocity, transform.forward).sqrMagnitude < 300f){
				//Balancing
				//entering gravity, activate landing
				if (!landMode)
				{
					if (Metwork.peerType != MetworkPeerType.Disconnected)
					{
						netObj.netView.RPC("RPC_LandMode", MRPCMode.All, new object[] { true });
					}
					else
					{
						RPC_LandMode(true);
					}
					landMode = true;

				}
				rb.AddForce(rb.mass * 9.81f * Mathf.Clamp(1f/Vector3.Dot(Vector3.up, transform.up), 0.1f, 1f)*transform.up * 50f * Time.deltaTime);
				rb.AddTorque(Vector3.Cross(-transform.up,(transform.up - Vector3.up)*Vector3.Magnitude(transform.up - Vector3.up)*rb.mass*10f)*rb.mass/1000f);
			}
			else{
				rb.AddRelativeForce(rb.mass * 9.81f * Mathf.Clamp(Vector3.Project(rb.velocity, transform.forward).sqrMagnitude * 0.002f,0, 1.5f) * 50f * Time.deltaTime * Vector3.up);
			}
		}
		else
		{
			//exiting gravity, deactivate landing
			if (landMode)
			{
				if (Metwork.peerType != MetworkPeerType.Disconnected)
				{
					netObj.netView.RPC("RPC_LandMode", MRPCMode.All, new object[] { false });
				}
				else
				{
					RPC_LandMode(false);
				}
				landMode = false;
			}
			rb.AddRelativeForce(0f, 0f, deltaThrustForce*0.4f);
		}

		engineSound.pitch = Mathf.Lerp(previousEnginePitch,Mathf.Clamp(Mathf.Abs(moveZ+moveX+moveY),0,0.1f) + (Time.frameCount % 5f)*0.003f  + 0.85f, 0.3f);
		previousEnginePitch = engineSound.pitch;
		if (MInput.useMouse)
		{
			rb.AddRelativeTorque(MInput.GetMouseDelta("Mouse Y") * -0.2f * deltaThrustForce * torqueFactor,
				MInput.GetMouseDelta("Mouse X") * 0.2f  * deltaThrustForce * torqueFactor,
				Input.GetAxis("Move X") * deltaThrustForce * -torqueFactor);
		}
		else
		{
			rb.AddRelativeTorque(MInput.GetAxis("Rotate X") * deltaThrustForce * torqueFactor,
				MInput.GetAxis("Rotate Y")  * deltaThrustForce * torqueFactor,
				Input.GetAxis("Move X") * deltaThrustForce * -torqueFactor);
		}
	}
	public void RPC_LandMode(bool on)
	{
		if (on)
		{
			anim.SetBool("Land Mode", true);
			rb.angularDrag = 1f;
			rb.drag = 0.2f;
		}
		else
		{
			anim.SetBool("Land Mode", false);
			rb.angularDrag = 0.5f;
			rb.drag = 0.1f;
		}
	}


	void SimulateParticles(){
		if (!netObj.isLocal) {
			return;
		}
		if (moveY != 1f) {
			for (int i = 0; i < bottomThrusters.Length; i++) {
				bottomThrusters [i].emissionRate = 150f * (Mathf.Clamp (moveY, 0f, 1f) * 1f);
			}
		}
		if (moveZ != 1f) {
			for (int i = 0; i < rearThrusters.Length; i++) {
				rearThrusters [i].emissionRate = 150f * (Mathf.Clamp (moveZ, 0.2f, 1f) * 1.3f);
			}
		}
		if ((int)(Time.deltaTime * 10000f) % 2 == 0) {
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netObj.netView.RPC ("RPC_SimulateParticles", MRPCMode.Others, new object[]{ moveY, moveZ });
			}
		}

	}

	[MRPC]
	void RPC_SimulateParticles(float _moveY, float _moveZ){


		if (_moveY != 1f) {
			for (int i = 0; i < bottomThrusters.Length; i++) {
				bottomThrusters [i].emissionRate = 150f * (Mathf.Clamp (_moveY, 0f, 1f) * 1f);
			}
		}
		if (_moveZ != 1f) {
			for (int i = 0; i < rearThrusters.Length; i++) {
				rearThrusters [i].emissionRate = 150f * (Mathf.Clamp (_moveZ, 0.2f, 1f) * 1.3f);
			}
		}
	}

	public void DisableAI(){
		this.isAI = false;
		holdPosition = false;
		distanceNotified = true;
		this.target = null;
		Navigation.DeregisterTarget(this.transform);
	}

	void AI(){
		Vector3 nextPosition;
		if (route == null || route.Count <= 0) {
			if(GetComponent<Raycaster>().target != null){
				nextPosition = GetComponent<Raycaster> ().target;
			}
			else{
				return;
			}
		} else {
			Vector3 closest = route [0];//new Vector3 (float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 middle = route [0];//new Vector3 (float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 farthest = route [0];//new Vector3 (float.MaxValue, float.MaxValue, float.MaxValue);



			for (int i = 0; i < route.Count; i++) {
				if ((route [i] - this.transform.position).sqrMagnitude < (closest - this.transform.position).sqrMagnitude) {
					farthest = middle;
					middle = closest;
					closest = route [i];
				} else if ((route [i] - this.transform.position).sqrMagnitude < (middle - this.transform.position).sqrMagnitude) {
					farthest = middle;
					middle = route [i];
				} else if ((route [i] - this.transform.position).sqrMagnitude < (farthest - this.transform.position).sqrMagnitude) {
					farthest = route [i];
				}
			}


			if(route.Count > route.IndexOf (closest)+1){
				nextPosition = route [route.IndexOf (closest) + 1];
			}
			else{
				nextPosition = route [route.IndexOf (closest)];
			}
		}

		float angle = Vector2.SignedAngle (new Vector2 (transform.forward.x, transform.forward.z), new Vector2 ((nextPosition - transform.position).x, (nextPosition - transform.position).z));//Vector3.SignedAngle ((nextPosition-this.transform.position), transform.forward, Vector3.forward);


		float horizontal = Mathf.Clamp (angle / -100f, -1f, 1f) - Mathf.Clamp((rb.angularVelocity.y/ angle), -1f,1f);



		rb.AddRelativeTorque(horizontal * thrust * 2f *  torqueFactor * Vector3.up);


		float anglePitch = Vector3.SignedAngle (nextPosition,this.transform.position, Vector3.right) + 90f;

		float gravityFactor = 0;
		if (rb.useGravity) {
			gravityFactor = 1f;
		}

		float vertical = Mathf.Clamp (angle / -100f, -1f, 1f);
		//rb.AddRelativeTorque(vertical * thrust * torqueFactor * Vector3.right);

		rb.AddForce (Mathf.Clamp((nextPosition - transform.position).y/90f, -1,1) * thrust * 2f * Vector3.up);


		float forward = 1f - (Mathf.Clamp(new Vector2(rb.velocity.x, rb.velocity.z).magnitude / (new Vector2((this.transform.position - nextPosition).x, (this.transform.position - nextPosition).z).magnitude), -1f,1f));

		//Slow down when approaching the target
		float slowFactor = 1f;
		if(Vector3.Distance(route[route.Count - 1],this.transform.position) < 15f){
			slowFactor = 0.3f;
			rb.velocity = Vector3.Lerp (rb.velocity, Vector3.zero, 0.05f);
			if(!distanceNotified){
				distanceNotified = true;
				WindowsVoice.Speak("Ten meters out.");
			}
		}
		rb.AddForce (slowFactor * forward * thrust*Time.deltaTime * 60f * (nextPosition - transform.position).normalized);
		rb.AddForce (slowFactor * gravityFactor * thrust * Vector3.up * Time.deltaTime * 45f);


		float upAngle = Vector2.SignedAngle (new Vector2 (transform.up.x, transform.up.y), Vector2.up);
		rb.AddRelativeTorque (0f, 0f, upAngle*upAngle*Time.deltaTime * 60f*Mathf.Sign(upAngle)*0.1f);

		float rightAngle = Vector2.SignedAngle (new Vector2 (transform.up.z, transform.up.y), Vector2.up);
		rb.AddRelativeTorque (rightAngle*rightAngle*Time.deltaTime * 60f*Mathf.Sign(rightAngle)*-0.1f,0f, 0f);

		

		rb.velocity = Vector3.Lerp (rb.velocity, Vector3.zero, 0.005f);

		/*if ((GetComponent<Raycaster>().target - this.transform.position).sqrMagnitude < 225f) {
			rb.velocity = Vector3.zero;
			isAI = false;
			this.enabled = false;
			Navigation.DeregisterTarget (this.transform);
		}*/


	}

	void FindDirection(){
		if (isAI == false) {
			return;
		}

		Raycaster caster = this.GetComponent<Raycaster> ();
		caster.origin = this.transform.position;
		if(Vector3.Distance(this.transform.position,target.position) <6f && !holdPosition){
			holdPosition = true;
			holdLocation = this.transform.position;
		}
		if(holdPosition){
			caster.target = holdLocation;
		}
		else{
			caster.target = target.position;
		}
		caster.resolution = Mathf.Ceil(Mathf.Max(new float[]{Mathf.Abs((caster.target - caster.origin).x),Mathf.Abs((caster.target - caster.origin).y),Mathf.Abs((caster.target - caster.origin).z)}) /4f);
		caster.Cast();

		//caster.Raycast (Vector3.zero, new Queue<Vector3>());
		caster.Visualize (caster.route.ToArray ());

		route = new List<Vector3>(caster.route); 

	}

	void CorrectPitch(){
		upAngle = Vector3.SignedAngle (Vector3.up, transform.up, transform.forward);

		rb.AddRelativeTorque (0f, 0f, upAngle*-1000f);
	}

	public virtual void Fire (){
		fireScriptLeft.FireWeapon(fireScriptLeft.shotSpawn.transform.position, fireScriptLeft.shotSpawn.transform.forward);
		Cmd_FireWeapon(fireScriptLeft.shotSpawn.transform.position, fireScriptLeft.shotSpawn.transform.forward, 0);
		fireScriptRight.FireWeapon (fireScriptRight.shotSpawn.transform.position, fireScriptRight.shotSpawn.transform.forward);
		Cmd_FireWeapon(fireScriptRight.shotSpawn.transform.position, fireScriptRight.shotSpawn.transform.forward, 1);
	}
	[Command]
	void Cmd_FireWeapon(Vector3 shotSpawnPosition, Vector3 shotSpawnForward, int gunNum){
		Fire fireScript;
		switch(gunNum){
			case 0:
				fireScript = fireScriptLeft;
				break;
			case 1:
				fireScript = fireScriptRight;
				break;
			default:
				fireScript = fireScriptLeft;
				break;
		}
		if(isServerOnly) fireScript.FireWeapon(shotSpawnPosition, shotSpawnForward);
		Rpc_FireWeapon(shotSpawnPosition, shotSpawnForward, gunNum);
	}
	[ClientRpc(includeOwner=false)]
	void Rpc_FireWeapon(Vector3 shotSpawnPosition, Vector3 shotSpawnForward, int gunNum){
		Fire fireScript;
		switch(gunNum){
			case 0:
				fireScript = fireScriptLeft;
				break;
			case 1:
				fireScript = fireScriptRight;
				break;
			default:
				fireScript = fireScriptLeft;
				break;
		}
		fireScript.FireWeapon(shotSpawnPosition, shotSpawnForward);
	}

	public void Activate(Player_Controller pilot){
		//Ensure that the gameobject has the netObj set (Due to start() not being called yet)
		if (netObj == null) {
			netObj = this.GetComponent<Metwork_Object> ();
		}

		if (pilot.GetComponent<Metwork_Object> ().isLocal && this.player == null) {
			this.DisableAI();
			//TODO: Find a better way of doing this maybe?
			pilot.GainAir ();
			this.mainCamera.SetActive (true);
			GetComponent<Damage> ().healthShown = true;
			GetComponent<Damage> ().UpdateUI ();
			getInTime = Time.time;
			//carrierPointer.SetActive (true);
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netObj.netView.RPC ("RPC_Activate", MRPCMode.AllBuffered, pilot.GetComponent<Metwork_Object> ().owner);
			} else {
				RPC_Activate (pilot.GetComponent<Metwork_Object> ().owner);
			}

		}


	}


	[MRPC]
	public void RPC_Activate(int _pilot){
		if (netObj == null) {
			netObj = this.GetComponent<Metwork_Object> ();
		}

		this.DisableAI();
		anim = this.GetComponent<Animator> ();

		try{
			for (int i = 0; i < bottomThrusters.Length; i++) {
				bottomThrusters [i].Play();
			}

			for (int i = 0; i < rearThrusters.Length; i++) {
				rearThrusters [i].Play ();
			}

		}
		catch{
		}

		player = FindObjectOfType<Game_Controller>().GetPlayerFromNetID (_pilot);
		player.inVehicle = true;
		player.gameObject.SetActive (false);

		if (anim != null&&!isTransport) {
			anim.SetBool ("Should Close", true);
		} 

		netObj.owner = _pilot;
		if (fireScriptLeft != null && fireScriptRight != null) {
			fireScriptLeft.playerID = _pilot;
			fireScriptRight.playerID = _pilot;
		}

	}

	public virtual void Exit(){
		
		//Switch to the internal camera
		this.mainCamera.SetActive(false);


		GetComponent<Damage> ().healthShown = false;
		GetComponent<Damage> ().UpdateUI ();

		if (netObj == null) {
			netObj = this.GetComponent<Metwork_Object> ();
		}


		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			//Make the owner the server
			netObj.netView.RPC ("RPC_Exit", MRPCMode.AllBuffered, new object[]{});

		} else {
			RPC_Exit ();

		}

		this.enabled = false;
	}


	//Exits over the network
	[MRPC]
	public void RPC_Exit(){
		for (int i = 0; i < bottomThrusters.Length; i++) {
			bottomThrusters [i].Stop();
		}

		for (int i = 0; i < rearThrusters.Length; i++) {
			rearThrusters [i].Stop ();
		}
		print ("RPC Exiting");
		if (netObj == null) {
			netObj = this.GetComponent<Metwork_Object> ();
		}
		player.gameObject.SetActive (true);
		player.GetComponent<Rigidbody> ().velocity = this.rb.velocity;
		player.transform.position = this.transform.position+FindExitPoint();
		if (rb.useGravity)
		{
			player.StartCoroutine("EnterGravity");
			player.rb.useGravity = true;
		}
		else
		{
			player.StartCoroutine("ExitGravity");
			player.rb.useGravity = false;
		}
		player.inVehicle = false;

		if (!isTransport && anim != null)
		{
			anim.SetBool("Should Close", false);
		}
		player = null;




	}
	Vector3 FindExitPoint(){
		Vector3 returnPosition = Vector3.zero;

		if(!Physics.Raycast(this.transform.position,transform.up, 6f)){
			returnPosition = transform.up * 3f;
			print ("UP");
		}
		else if(!Physics.Raycast(this.transform.position,transform.right, 6f)){
			returnPosition = transform.right * 4f;
			print ("RIGHT");

		}
		else if(!Physics.Raycast(this.transform.position,-transform.right, 6f)){
			returnPosition = -transform.right * 4f;
			print ("Left");

		}
		else if(!Physics.Raycast(this.transform.position,transform.forward, 6f)){
			returnPosition = transform.forward * 5f;
			print ("FOrward");

		}
		else if(!Physics.Raycast(this.transform.position,-transform.forward, 6f)){
			returnPosition = -transform.forward * 5f;
			print ("Back");
		}
		else {
			returnPosition = -transform.up * 3f;
			print ("Failed, going down");

		}
	
		return returnPosition;
	}

	public void Die(){

		if(Metwork.peerType != MetworkPeerType.Disconnected){
			if (player != null) {
				netObj.netView.RPC ("RPC_Die", MRPCMode.AllBuffered, new object[]{ player.GetComponent<Metwork_Object> ().netID });
			} else {
				netObj.netView.RPC ("RPC_Die", MRPCMode.AllBuffered, new object[]{0});
			}
		}
		else{
			if (player != null) {
				RPC_Die (player.GetComponent<Metwork_Object> ().netID);
			} else {
				RPC_Die (0);
			}
		}

	}

	//This function should essentially make the ship "like new"
	[MRPC]
	public void RPC_Die(int id){
		print ("Running ship controller die");
		Navigation.DeregisterTarget (this.transform);
		DisableAI();

		Destroy(Instantiate (explosionEffect, this.transform.position, transform.rotation),5f);
		Destroy(Instantiate (destroyedPrefabs [Random.Range (0, destroyedPrefabs.Length)], this.transform.position, transform.rotation),5f);
		if (player != null) {
			Exit ();
			player = FindObjectOfType<Game_Controller> ().GetPlayerFromNetID (id);
			player.GetComponent<Damage> ().TakeDamage(1000,0, transform.position, true);
		}



		this.GetComponent<Damage> ().Reset();

		this.enabled = false;
	}

}
