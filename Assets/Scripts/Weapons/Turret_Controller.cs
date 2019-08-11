using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret_Controller : MonoBehaviour
{

	public GameObject turretPivot;
	float h;
	float v;
	public Animator anim;
	public Fire primary1;
	public Fire primary2;
	public GameObject mainCamera;
	public GameObject player;
	public MetworkView netView;
	public GameObject explosionEffect;
	public GameObject destroyedPrefab;
	public LayerMask layerMask;
	public bool auto;
	public int team;

	public Ship_Controller[] fighters;
	public Bomber_Controller[] bombers;

	public void Start(){
		netView = this.GetComponent<MetworkView> ();
		
		//Find all the fighters in the scene
		fighters = FindObjectsOfType<Ship_Controller>();
		//Find all the bombbers in the scene
		bombers = FindObjectsOfType<Bomber_Controller>();

		primary1.playerID = 0;
		primary2.playerID = 0;
	}
	public virtual void Activate(GameObject pilot)
	{
		//Switch to the internal camera
		print(pilot.name);
		if (player == null)
		{
			pilot.GetComponent<Player_Controller>().mainCamObj.SetActive(false);
			netView = this.GetComponent<MetworkView>();


			if (pilot.GetComponent<Metwork_Object>().isLocal)
			{
				this.mainCamera.SetActive(true);
				this.GetComponent<Damage>().healthShown = true;
				this.GetComponent<Damage>().UpdateUI();


				if (Metwork.peerType != MetworkPeerType.Disconnected)
				{
					netView.RPC("RPC_Activate", MRPCMode.AllBuffered, pilot.GetComponent<Metwork_Object>().owner);
				}
				else
				{
					RPC_Activate(pilot.GetComponent<Metwork_Object>().owner);
				}
			}
		}


	}
	[MRPC]
	public void RPC_Activate(int _pilot)
	{
		player = FindObjectOfType<Game_Controller>().GetPlayerFromNetID(_pilot);
		player.SetActive(false);
		player.GetComponent<Player_Controller>().inVehicle = true;
		this.enabled = true;

		primary1.playerID = _pilot;
		primary2.playerID = _pilot;

	}
	public void Exit()
	{
		//Switch to the player camera
		player.GetComponent<Player_Controller>().mainCamObj.SetActive(true);
		this.mainCamera.SetActive(false);

		GetComponent<Damage>().healthShown = false;
		GetComponent<Damage>().UpdateUI();




		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			//Make the owner the server
			netView.RPC("RPC_Exit", MRPCMode.AllBuffered, new object[] { });
		}
		else
		{
			RPC_Exit();
		}
		this.enabled = false;

	}
	[MRPC]
	public void RPC_Exit()
	{
		print(player.name);
		GetComponent<Damage>().healthShown = false;
		GetComponent<Damage>().UpdateUI();
		//move player away from the object
		player.transform.position = transform.position + transform.right * 5f;
		player.gameObject.SetActive(true);
		player.GetComponent<Player_Controller>().inVehicle = false;

		player = null;

	}
	public void Die()
	{
		print("Dying: turret");

		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			print("RUnning online thing");
			if (player != null)
			{
				netView.RPC("RPC_Die", MRPCMode.AllBuffered, new object[] { player.GetComponent<Metwork_Object>().netID });
			}
			else
			{
				netView.RPC("RPC_Die", MRPCMode.AllBuffered, new object[] { 0 });
			}
		}
		else
		{
			print("RUnnnning offline");
			if (player != null)
			{
				RPC_Die(player.GetComponent<Metwork_Object>().netID);
			}
			else
			{
				RPC_Die(0);
			}
		}

	}
	[MRPC]
	public void RPC_Die(int id)
	{
		print("Net Dying: turret");
		Destroy(Instantiate(explosionEffect, this.transform.position, transform.rotation), 5f);
		GameObject destroyedTurret = Instantiate(destroyedPrefab, this.transform.position, transform.rotation);
		destroyedTurret.transform.parent = this.transform.parent;
		Destroy(destroyedTurret, 5f);
		gameObject.SetActive(false);

		if (player != null)
		{
			Exit();
			player = FindObjectOfType<Game_Controller>().GetPlayerFromNetID(id);
			player.GetComponent<Damage>().TakeDamage(1000, 0, transform.position);

			//tmpPlayer.GetComponent<Player_Controller> ().Die ();

		}
		this.GetComponent<Damage>().Reset();

		if (!auto)
		{
			this.enabled = false;
		}

	}
	// Update is called once per frame
	void Update()
	{
		if (player == null && auto && (Metwork.isServer || Metwork.peerType == MetworkPeerType.Disconnected))
		{
			AutoAim();
			return;
		}
		if (player != null && player.GetComponent<Metwork_Object>().isLocal)
		{
			if (MInput.useMouse)
			{
				h = -MInput.GetMouseDelta("Mouse X");
				v = MInput.GetMouseDelta("Mouse Y");
			}
			else
			{
				h = -MInput.GetAxis("Rotate Y");
				v = -MInput.GetAxis("Rotate X");
			}
			Look();
			if (Input.GetButton("Fire1"))
			{
				primary1.FireWeapon();
				primary2.FireWeapon();
			}
			if (Input.GetButtonDown("Use Item"))
			{
				Exit();
			}
		}

	}
	[MRPC]
	public void RPC_Turn(float turn, float currentTime)
	{
		//StartCoroutine (AdjustView (turn,currentTime));
		AdjustView(turn, currentTime);
	}
	public void AdjustView(float hTime, float time)
	{
		float lookTime = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
		anim.Play("Aim Up/Down", 0, Mathf.Lerp(lookTime, time, 0.5f));


		float turnTime = anim.GetCurrentAnimatorStateInfo(1).normalizedTime;
		anim.Play("Turn", 1, Mathf.Lerp(turnTime, hTime, 0.5f));
	}

	void Look()
	{
		if (anim == null)
		{
			return;
		}
		float lookUpDownTime = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;


		if (v < 0f)
		{
			if (lookUpDownTime > 0f)
			{
				anim.SetFloat("Look Speed", v);
			}
			else
			{
				anim.SetFloat("Look Speed", 0f);
			}
		}
		else if (v > 0f)
		{
			if (lookUpDownTime <= 1f)
			{
				anim.SetFloat("Look Speed", v);
			}
			else
			{
				anim.SetFloat("Look Speed", 0f);
			}
		}
		else
		{
			anim.SetFloat("Look Speed", 0f);
		}
		anim.SetFloat("Turn Speed", h);
		float turnTime = anim.GetCurrentAnimatorStateInfo(1).normalizedTime;


		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			netView.RPC("RPC_Turn", MRPCMode.Others, new object[]{
				turnTime,
				lookUpDownTime});
		}
	}
	void AutoAim()
	{
		if (Time.time < 5f)
		{
			return;
		}
		
		Transform _bestShip;
		if(fighters.Length > 0){
			_bestShip = fighters[0].transform;
		}
		else{
			return;
		}
		
		//Find the closest fighter
		for(int i = 0; i< fighters.Length; i++){
			if( (_bestShip.transform.position-transform.position).sqrMagnitude > (fighters[i].transform.position-transform.position).sqrMagnitude
			 && fighters[i].player!= null && Game_Controller.GetTeam(fighters[i].player) != team){
				_bestShip = fighters[i].transform;
			}
		}
		
		
		//Find the closest fighter
		for(int i = 0; i< bombers.Length; i++){
			if( (_bestShip.transform.position-transform.position).sqrMagnitude > (bombers[i].transform.position-transform.position).sqrMagnitude
			 && bombers[i].player!= null && Game_Controller.GetTeam(bombers[i].player) != team){
				_bestShip = bombers[i].transform;
			}
		}



		//Project on the relative (to the base) x-z plane to find left right rotation
		float _yAngle = Vector3.SignedAngle(primary1.shotSpawn.forward,_bestShip.transform.position - transform.position,transform.up);
		anim.SetFloat ("Turn Speed", Mathf.Clamp(-_yAngle/200f,-1f,1f));
		//Project on the relative (to the base) x-z plane to find left right rotation
		float _xAngle = Vector3.SignedAngle(primary1.shotSpawn.forward, _bestShip.transform.position - transform.position, transform.right);

		float lookUpDownTime = anim.GetCurrentAnimatorStateInfo (0).normalizedTime;
		if(lookUpDownTime >= 1.0f){
			_xAngle = -10f;
		}
		if(lookUpDownTime <= 0.0f){
			_xAngle = 10f;
		}
		anim.SetFloat ("Look Speed",  Mathf.Clamp(_xAngle/10f,-2f,2f));

		if(Vector3.Dot(primary1.shotSpawn.forward, (_bestShip.transform.position-transform.position).normalized) >0.7){
			RaycastHit _hit;
			Physics.Raycast(transform.position, _bestShip.transform.position-transform.position, out _hit,500f, layerMask, QueryTriggerInteraction.Ignore);
			if(_hit.transform.root == _bestShip.transform){
				primary1.FireWeapon ();
				primary2.FireWeapon ();
			}
		}
	}
	
}
