using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Vehicle : NetworkBehaviour
{
	protected Rigidbody rb;
	protected Animator anim;

	//Components
	public GameObject mainCamera;

	protected float emptyTime;
	protected float getInTime = -1f;

	protected Damage damageScript;
	public GameObject[] destroyedPrefabs;
	public GameObject explosionEffect;

	public Player_Controller player;
	[Header("Engine Exhaust")]
	public ParticleSystem[] bottomThrusters;
	public ParticleSystem[] rearThrusters;

	protected float moveX;
	protected float moveY;
	protected float moveZ;
	public Metwork_Object netObj;

	public Fire fireScriptLeft;
	public Fire fireScriptRight;

    protected void CheckOccupied(){
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

	protected void AddDamage(){
		if (damageScript.currentHealth > 0) {
			damageScript.TakeDamage (100, 0, transform.position);
		}

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

	public virtual void DisableAI(){
		
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
	public virtual void RPC_Activate(int _pilot){
		if (netObj == null) {
			netObj = this.GetComponent<Metwork_Object> ();
		}

		this.DisableAI();
		anim = this.GetComponent<Animator> ();

		

		player = FindObjectOfType<Game_Controller>().GetPlayerFromNetID (_pilot);
		player.inVehicle = true;
		player.gameObject.SetActive (false);


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
	public virtual void RPC_Exit(){
		
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

		
		player = null;


	}
	protected Vector3 FindExitPoint(){
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

    
}
