using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Michael Gunther: 2018-02-05
 * Purpose: Ship class to be derived from to make the fighter ship, bombers etc
 * Notes: 
 * Improvements/Fixes:
* */

public class Bomber_Controller : MonoBehaviour
{

	public enum WeaponType
	{
		Primary,
		Secondary,
		Tertiary
	}

	WeaponType weaponType = WeaponType.Primary;

	//Components
	Rigidbody rb;
	public Metwork_Object netObj;
	public GameObject mainCamera;
	public GameObject player;
	public List<Vector3> route;

	public GameObject[] destroyedPrefabs;
	public GameObject explosionEffect;

	public float thrust;
	[Tooltip ("Factor of the thrust that can rotate the ship")]
	public float torqueFactor = 0.4f;
	public float upAngle;
	float moveY;
	float moveZ;
	float deltaThrustForce;

	public Fire fireScriptLeft;
	public Fire fireScriptRight;
	public Fire bombFireScript;

	public bool isPilot;
	public bool isAI = false;
	public Transform target;
	public AudioSource engineSound;
	public Animator anim;
	//Engine Exhaust
	[Header ("Engine Exhaust")]
	public ParticleSystem[] bottomThrusters;
	public ParticleSystem[] rearThrusters;
	public Transform[] passengerSeats;
	public GameObject carrierPointer;
	public Transform carrierOne;
	public Transform carrierTwo;

	Coroutine addDamage;
	Damage damageScript;

	// Use this for initialization
	void Start ()
	{
		rb = this.GetComponent<Rigidbody> ();
		netObj = this.GetComponent<Metwork_Object> ();
		anim = this.GetComponent<Animator> ();
		engineSound = this.GetComponent<AudioSource> ();
		InvokeRepeating ("CheckOccupied", 1f, 1f);
		damageScript = this.GetComponent<Damage> ();
		this.enabled = false;
	}
	void CheckOccupied(){
		if (!(Metwork.isServer || Metwork.peerType == MetworkPeerType.Disconnected)) {
			return;
		}

		if (this.player == null && Vector3.SqrMagnitude (damageScript.initialPosition.position - transform.position) > 500f) {
			if (addDamage == null) {
				addDamage = StartCoroutine (AddDamage ());
			}
		}
	}

	IEnumerator AddDamage(){
		yield return new WaitForSeconds (60f);
		while (damageScript.currentHealth > 0) {
			if (this.player == null && Vector3.SqrMagnitude (damageScript.initialPosition.position - transform.position) > 500f) {
				damageScript.TakeDamage (100, 0);
			} else {
				addDamage = null;
				break;
			}
			yield return new WaitForSeconds (5f);
		}

	}

	// Update is called once per frame
	void Update ()
	{
		if (netObj == null) {
			netObj = this.GetComponent<Metwork_Object> ();
		}
		if (!netObj.isLocal) {
			return;
		}

		Fly ();


		try{ SimulateParticles (); } catch{}
		moveY = Input.GetAxis ("Move Y");
		moveZ = Input.GetAxis ("Move Z");
		deltaThrustForce = Time.deltaTime * 45f * thrust;
		rb.velocity = Vector3.ClampMagnitude (rb.velocity, 1000f);
		rb.angularVelocity = Vector3.ClampMagnitude (rb.angularVelocity, 10f);


		if (Input.GetButton ("Fire1")) {
			Fire ();
		}
		if(Input.GetButtonDown("Use Item")){
			Exit ();
		}

		if(Input.GetButton("Switch Weapons")){
			SwitchWeapons ();
		}
		if(Input.GetButtonDown("Jump")){
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				if (anim.GetBool ("Lower Gear")) {
					netObj.netView.RPC ("RPC_LowerGear", MRPCMode.AllBuffered, new object[]{ false });
				} else {
					netObj.netView.RPC ("RPC_LowerGear", MRPCMode.AllBuffered, new object[]{ true });

				}
			} else {
				if (anim.GetBool ("Lower Gear")) {
					RPC_LowerGear (false);
				} else {
					RPC_LowerGear (true);
				}
			}
		}



	}
			[MRPC]
	void RPC_LowerGear(bool lower){
		
		anim.SetBool ("Lower Gear", lower);
	}

	void Fly ()
	{
		rb.AddRelativeForce (0f, moveY * deltaThrustForce * 2f,
			moveZ * deltaThrustForce);
		engineSound.pitch = Mathf.Clamp (Mathf.Abs (moveZ + moveY), 0, 0.3f) + 1f;


		rb.AddRelativeTorque( MInput.GetAxis("Rotate X") *deltaThrustForce* torqueFactor,
			MInput.GetAxis("Rotate Y") *deltaThrustForce* torqueFactor,
			Input.GetAxis("Move X") *deltaThrustForce* -torqueFactor);
	}


	void SimulateParticles ()
	{
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
	void RPC_SimulateParticles (float _moveY, float _moveZ)
	{


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



	public void SwitchWeapons(){
		//Cycle through the weapons
		switch (weaponType) {
			case WeaponType.Primary:
				weaponType = WeaponType.Secondary;
				break;
			case WeaponType.Secondary:
				//TODO: Change this to tertiary when a tertiary weapon becomes available
				weaponType = WeaponType.Primary;
				break;
			case WeaponType.Tertiary:
				weaponType = WeaponType.Primary;
				break;
			default:
				break;
		}
	}
	public virtual void Fire ()
	{
		if (weaponType == WeaponType.Primary) {
			fireScriptLeft.FireWeapon ();
			fireScriptRight.FireWeapon ();
		}
		if (weaponType == WeaponType.Secondary) {
			bombFireScript.FireWeapon ();
		}
	}

	public void Activate(GameObject pilot){

		//Ensure that the gameobject has the netObj set (Due to start() not being called yet)
		if (netObj == null) {
			netObj = this.GetComponent<Metwork_Object> ();
		}

		if (pilot.GetComponent<Metwork_Object> ().isLocal && this.player == null) {
			pilot.GetComponent<Player_Controller> ().GainAir ();
			this.mainCamera.SetActive (true);
			GetComponent<Damage> ().healthShown = true;
			GetComponent<Damage> ().UpdateUI ();

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
		player.GetComponent<Player_Controller> ().inVehicle = true;

		player.SetActive (false);

		if (anim != null) {
			anim.SetBool ("Should Close", true);
		} 

		netObj.owner = _pilot;
		if ( fireScriptLeft!= null && fireScriptRight != null&&bombFireScript !=null) {
			fireScriptLeft.playerID = _pilot;
			fireScriptRight.playerID = _pilot;
			bombFireScript.playerID = _pilot;

		}

	}

	//Exits over the network
	public virtual void Exit(){
		for (int i = 0; i < bottomThrusters.Length; i++) {
			bottomThrusters [i].Stop();
		}

		for (int i = 0; i < rearThrusters.Length; i++) {
			rearThrusters [i].Stop ();
		}
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
		print ("RPC Exiting");
		if (netObj == null) {
			netObj = this.GetComponent<Metwork_Object> ();
		}
		player.SetActive (true);
		player.GetComponent<Rigidbody> ().velocity = this.rb.velocity;
		player.transform.position = this.transform.position+FindExitPoint();
		if (rb.useGravity)
		{
			player.GetComponent<Player_Controller>().StartCoroutine("EnterGravity");
			player.GetComponent<Rigidbody>().useGravity = true;
		}
		else
		{
			player.GetComponent<Player_Controller>().StartCoroutine("ExitGravity");
			player.GetComponent<Rigidbody>().useGravity = false;
		}
		player.GetComponent<Player_Controller> ().inVehicle = false;

		anim.SetBool ("Should Close", false);
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
		if(Metwork.peerType  != MetworkPeerType.Disconnected){
			netObj.netView.RPC ("RPC_Die", MRPCMode.AllBuffered, new object[]{ });
		}
		else{
			RPC_Die ();
		}

	}

	[MRPC]
	public void RPC_Die(){
		print ("Running ship controller die");
		Destroy(Instantiate (explosionEffect, this.transform.position, transform.rotation),3f);
		Destroy(Instantiate (destroyedPrefabs [Random.Range (0, destroyedPrefabs.Length)], this.transform.position, transform.rotation),5f);
		if (player != null) {
			player.SetActive (true);
			//player.GetComponent<Player_Controller> ().Invoke ("Die", 0.1f);
			player.GetComponent<Damage> ().TakeDamage(1000,0);
			Exit ();


		}



		this.GetComponent<Damage> ().Reset();

		this.enabled = false;
	}

}


