﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

public class Player_Controller : NetworkBehaviour {
	float v;
	float v2;
	float h;
	float h2;
	float z;
	float down;
	public Rigidbody rb;
	public Animator anim;

	public LayerMask magBootsLayer;
	//Access to the left and right foot data
	[HideInInspector]
	public RaycastHit lfHit;
	//[HideInInspector]
	public bool lfHitValid;
	[HideInInspector]
	public RaycastHit rfHit;

	Vector3 previousNormal = new Vector3(0,1,0);
	public bool onLadder = false;
	public float forceFactor;
	public bool inVehicle = false;
	Transform lfRaycast;
	Transform rfRaycast;
	public float maxStepHeight;
	public Game_Controller gameController;
	WalkState walkState = WalkState.Walking;
	public bool useGravity;
	//to be used for moving in space
	float moveSpeed = 5f;
	float lookFactor = 1f;
	float moveFactor = 1f;
	public float currentStepHeight;
	public bool enteringGravity = false;
	bool exitingGravity = false;
	float jumpWait;
	float jumpHeldTime;
	//public MetworkView netView;
	//public Metwork_Object netObj;
	public float jetpackFuel = 1f;
	public ParticleSystem[] jetpackJets;

	public bool refueling = false;
	bool magBootsLock = false;

	public float airTime;
	public float suffocationTime;
	public Damage damageScript;
	public LadderController ladder;
	public int kills;
	
	[SyncVar]
	public int playerID;

	public string playerName = "Fred";
	public GameObject ragdoll;
	public float knifePosition;
	public Animator knifeAnim;
	bool counterKnife;
	public Transform flagPosition;
	public Rigidbody shipRB;

	/// <summary>
	/// The player's team. 0 being team A and 1 being team B
	/// </summary>
	public Material[] teamMaterials;
	public SkinnedMeshRenderer[] jerseyMeshes;

	#region CrouchHeights
	float capsule_originalHeight1 = 1f;
	float capsule_originalHeight2 = 1f;
	float capsule_originalY1 = -0.035f;
	float capsule_originalY2 = -0.01f;
	float capsule_crouchHeight1 = 0.8f;
	float capsule_crouchHeight2 = 0.8f;
	float capsule_crouchY1 = -0.132f;
	float capsule_crouchY2 = -0.1f;
	#endregion

	[Header("UI")]
	#region UI

	//GameObject pieIcon;
	public GameObject [] pieQuadrants;
	public int pieNumber = 0;
	public Minimap_Controller mmpController;
	public MeshRenderer playerIcon;
	public LayerMask iconMask;
	bool minimapRunning = false;

	public GameObject minimapUI;
	public TextMesh nameTextMesh;
	public Text UI_ammoText;
	bool keypadPushed;
	
	public Blackout_Effects blackoutShader;

	public GameObject helmet;

	#endregion
	[Header("Cameras")]
	#region cameras
	public GameObject mainCamObj;
	Camera mainCam;
	Vector3 originalCamPosition;
	Quaternion originalCamRotation;
	public GameObject minimapCam;
	public GameObject iconCamera;
	#endregion
	[Header("Sound")]
	#region sound
	AudioWrapper wrapper;
	float thrusterSoundFactor = 0f;
	public AudioSource walkSound;
	public AudioClip[] walkClips;
	public AudioSource breatheSound;
	public AudioClip[] thrusterClips;

	#endregion
	[Space(5)]
	[Header("Weapons")]
	#region weapons

	string recoilString;
	float recoilAmount;
	public Fire fireScript;
	public string[] loadoutSettings;
	public Transform finger;
	public Transform rightHandPosition;
	public GameObject magGO;
	public bool primarySelected = true;
	[Header("Primary")]
	public GameObject primaryWeapon;
	public GameObject primaryWeaponPrefab;
	public Vector3 primaryLocalPosition;
	public Vector3 primaryLocalRotation;
	int primaryNetView = -1;
	float muzzleClimb = 0f;

	[Header("Secondary")]
	public GameObject secondaryWeapon;
	public GameObject secondaryWeaponPrefab;
	public Vector3 secondaryLocalPosition;
	public Vector3 secondaryLocalRotation;
	int secondaryNetView = -1;
	[Header("Grenades")]
	int grenadesNum = 4;
	public GameObject grenadePrefab;
	public GameObject grenadeModelPrefab;
	GameObject grenadeModel;
	bool throwingGrenade = false;

	public Transform grenadeSpawn;
	[Header("Grapple")]
	public bool grappleActive;

	#endregion

	//Used to lerp the space rotation
	Vector3 previousRot = Vector3.zero;
	Vector3 previousVelocity = Vector3.zero;

	public enum WalkState{
		Walking,
		Crouching,
		Running
	}

	// Use this for initialization
	void Start () {
		
		gameController = FindObjectOfType<Game_Controller> ();
		
		rb = this.GetComponent<Rigidbody> ();
		anim = this.GetComponent<Animator> ();
		mainCam = mainCamObj.GetComponent<Camera> ();
		wrapper = GetComponent<AudioWrapper>();
		blackoutShader = mainCamObj.GetComponent<Blackout_Effects> ();
		originalCamPosition = mainCamObj.transform.localPosition;
		originalCamRotation = mainCamObj.transform.localRotation;

		//Feet raycasts
		lfRaycast = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
		rfRaycast = anim.GetBoneTransform(HumanBodyBones.RightFoot);

		if (gameController.localPlayer == null) {
			gameController.GetLocalPlayer ();
		}
		//ICONS
		GameObject[] _icons = GameObject.FindGameObjectsWithTag("Icon");
				
		foreach(GameObject _icon in _icons){
			Material mat = _icon.GetComponent<MeshRenderer> ().material;
			mat.color = new Color (1f, 1f, 0f, 0.0f);
			_icon.GetComponent<MeshRenderer> ().material = mat;
		}

		//UI_Manager.onPieEvent += this.OnPieEvent;
		//CHECK
		if (isLocalPlayer){
			LoadPlayerData ();
			//TODO
			/*if (Metwork.peerType != MetworkPeerType.Disconnected) {
				print (this.name);
				gameController.netView.RPC ("RPC_AddPlayerStat", MRPCMode.AllBuffered, new object[] {
					playerName,
					this.netObj.owner,
					false

				});
			} else {
				gameController.RPC_AddPlayerStat (
					playerName,
					this.netObj.owner,
					false
				);
			}*/
		}
		//TODO
		/*if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("RPC_ShowNameText", MRPCMode.AllBuffered, new object[]{ });
		} else {
			RPC_ShowNameText ();
			sceneCam.enabled = false;
			sceneCam.GetComponent<AudioListener>().enabled = false;
		}*/
		anim.SetFloat ("Look Speed", 0.5f);
	}
	// runs after start basically the same
	//Check remove this function if possible
	public void Setup(){
		gameController = FindObjectOfType<Game_Controller> ();
		//netObj = this.GetComponent<Metwork_Object> ();
		rb = this.GetComponent<Rigidbody> ();
		anim = this.GetComponent<Animator> ();
		mainCam = mainCamObj.GetComponent<Camera> (); 
		blackoutShader = mainCamObj.GetComponent<Blackout_Effects> ();

		airTime = suffocationTime;
		grenadesNum = 4;
		pieQuadrants = UI_Manager._instance.pieQuadrants;
		if (MInput.useMouse)
		{
			Cursor.lockState = CursorLockMode.Locked;

		}

		mainCam.enabled = true;
		
		if (isLocalPlayer) {
			
			SetupWeapons ();
			damageScript = this.GetComponent<Damage> ();
			damageScript.healthShown = true;
			InvokeRepeating ("UpdateUI", 1f, 1f);
			LoadPlayerData ();
		}
		//primarySelected = !primarySelected;
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			//TODO
			//netView.RPC ("RPC_ShowNameText", MRPCMode.AllBuffered, new object[]{ });
			
		} else {
			
			RPC_ShowNameText ();

		}
		
		UpdateUI ();

	}

	void OnEnable(){

		if (isLocalPlayer) {
			if (minimapUI != null) {
				minimapUI.SetActive (true);
			}

		}
		previousNormal = transform.up;
		previousRot = Vector3.zero;

		anim.SetFloat ("Look Speed", 0.5f);

		Invoke ("Setup", 0.2f);
	}
	void OnDisable(){
		try {
			if (isLocalPlayer) {
				minimapUI.SetActive (false);
			}
		} catch {

		}

	}

	
	// Update is called once per frame
	//was update
	void Update () {
		//Gather Raycast data from under the left and right feet
		lfHitValid = Physics.Raycast(lfRaycast.position, -transform.up,out lfHit,1f,magBootsLayer,QueryTriggerInteraction.Ignore);
		if(lfHitValid){
			Debug.DrawLine(lfRaycast.position, lfHit.point);
		}
		
		Physics.Raycast(rfRaycast.position, -transform.up,out rfHit,1f,magBootsLayer,QueryTriggerInteraction.Ignore);
		

		//Play thruster sounds
		//TODO: Modify this so that we can hear other players on rotation
		if(!walkSound.isPlaying && !rb.useGravity){
			float _soundVolume = 0f;
			float _deltaV = (rb.velocity.sqrMagnitude - previousVelocity.sqrMagnitude) / Time.deltaTime;
			float _deltaRot = Mathf.Abs(Input.GetAxis("Move X"));

			if(_deltaV > 0.01f){
				//0.001f
				_soundVolume += 1.0f;//Mathf.Clamp01(0.01f * _deltaV) * 0.6f;
			}
			if(_deltaRot > 0.1f){
				_soundVolume += 0.3f;
			}
			if(thrusterSoundFactor > 0.1f && !magBootsLock){
				wrapper.PlayOneShot(0, thrusterSoundFactor);
			}

			previousVelocity = rb.velocity;
		}

		if (!isLocalPlayer) {
			mainCamObj.SetActive(false);
			minimapCam.SetActive(false);
			iconCamera.SetActive (false);

			nameTextMesh.transform.LookAt (gameController.localPlayer.transform);
			

			return;
		} else {
			mainCamObj.SetActive(true);
			minimapCam.SetActive(true);
			iconCamera.SetActive (true);
		}
		

		mainCamObj.GetComponent<AudioListener> ().enabled = true;
		if (MInput.useMouse)
		{
			h2 = Mathf.Clamp(MInput.GetMouseDelta("Mouse X")* lookFactor*0.1f,-2f,2f);
			v2 = Mathf.Clamp(MInput.GetMouseDelta("Mouse Y") * lookFactor*0.1f,-2f,2f);
		}
		else
		{
			h2 = MInput.GetAxis("Rotate Y") * lookFactor;
			v2 = -MInput.GetAxis("Rotate X") * lookFactor;
		}

		
		OnPieEvent(UI_Manager.GetPieChoice());
		
		

		if (inVehicle) {
			MouseLook();
			TurnHead ();
			return;
		} else {
			anim.SetBool ("Head Turn Enabled", false);
		}
		z = Input.GetAxis ("Move Y") * moveFactor;
		v = Input.GetAxis ("Move Z")*moveFactor;
		h = Input.GetAxis ("Move X")*moveFactor;

		muzzleClimb -= Time.deltaTime;
		if(muzzleClimb < 0){
			muzzleClimb = 0f;
		}


		if (Input.GetButtonDown ("Use Item")) {
			UseItem ();
			
		}
		if (Input.GetKeyDown ("/")) {
			damageScript.TakeDamage (1000, 0, transform.position, true);
		}

		if (MInput.GetButtonDown ("Switch Weapons")) {
			//TODO
			Cmd_SwitchWeapons(primarySelected);
			/*if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netView.RPC ("RPC_SwitchWeapons", MRPCMode.AllBuffered, new object[]{primarySelected });
			} else {
				RPC_SwitchWeapons (primarySelected);
			}*/
			
			UpdateUI ();
		}
			
		if (jetpackFuel< 2f) {
			jetpackFuel += Time.deltaTime/6f;
			UpdateUI ();
		} 	

		if (rb.useGravity) {
			MouseLook();
			if (Input.GetButton("Jump")){
				Hop();
			}
			if (z > 0f) {
				jumpHeldTime+=Time.deltaTime;
				if (jumpHeldTime > 0.1f)
				{
					Jump();
				}
				wrapper.PlayOneShot(0, jumpHeldTime);

				
			} else {
				
				foreach (ParticleSystem jet in jetpackJets) {
					jet.Stop ();
				}
				jumpHeldTime = 0f;

			}
			
			if (rb.velocity.y > 4f) {
				moveFactor = 0.3f;
				anim.SetBool ("Jump", true);
				if (Time.frameCount % 4 == 0) {
					if (Metwork.peerType != MetworkPeerType.Disconnected) {
						if (jetpackJets.Length>1) {
							//TODO:
							//netView.RPC ("RPC_Jump", MRPCMode.Others, new object[]{ true, jetpackJets [0].isPlaying });
						}
					}
				}

			} else if (rb.velocity.y < -4f) {
				moveFactor = 0.3f;
			} else {
				anim.SetBool ("Jump", false);
				if (Time.frameCount % 4 == 0) {
					if (Metwork.peerType != MetworkPeerType.Disconnected) {
						if (jetpackJets.Length>1) {
							//TODO
							//netView.RPC ("RPC_Jump", MRPCMode.Others, new object[]{ false, jetpackJets [0].isPlaying });
						}
					}
				}
			}
			if (Input.GetButtonDown ("Crouch")) {
				if (walkState == WalkState.Crouching) {
					//TODO:
					/*if (Metwork.peerType != MetworkPeerType.Disconnected) {
						netView.RPC ("RPC_UnCrouch", MRPCMode.AllBuffered, new object[]{ });
					} else {
						RPC_UnCrouch ();
					}*/

				} else {
					walkState = WalkState.Crouching;
					//TODO:
					/*
					if (Metwork.peerType != MetworkPeerType.Disconnected) {
						netView.RPC ("RPC_Crouch", MRPCMode.AllBuffered, new object[]{ });
					} else {
						RPC_Crouch ();
					}*/

					

				}
			} 
			else if (Input.GetButton ("Sprint")&&v>0) {
				walkState = WalkState.Running;
			} else if(walkState != WalkState.Crouching) {
				walkState = WalkState.Walking;
			}


			if (!enteringGravity)
			{
				MovePlayer();
			}
			AnimateMovement ();

            if (airTime < suffocationTime)
            {
                GainAir();
            }

        } else {
			if (!exitingGravity)
			{
				SpaceMove();
			}
			LoseAir ();

		}

		blackoutShader.ChangeConciousness (Mathf.Clamp01(Mathf.Pow(airTime / suffocationTime,2f)+0.3f) * 10f);
		if(isLocalPlayer){
			breatheSound.volume = Mathf.Clamp01(Mathf.Pow((suffocationTime-airTime) / suffocationTime,2f)-0.3f);
		}
		else{
			breatheSound.volume = 0f;
		}
		//TODO: Come up with a move elegant solution
		if (Input.GetButton ("Fire1") && !UI_Manager._instance.pauseMenu.gameObject.activeSelf) {
			Attack ();

		}
		if (MInput.GetButtonDown ("Fire2")) {
			if(grenadesNum>0){
				grenadesNum--;
				ThrowGrenade();
			}
		}
		if (Input.GetButtonDown ("Knife")) {
			Knife ();
		}
		Aim ();


	}

	public void OnPieEvent(int _segmentNumber)
	{
		if (_segmentNumber == -1 || !isLocalPlayer || !this.enabled)
		{
			return;
		}
		switch (_segmentNumber)
		{
			case 0:
				break;
			case 1:
				
				break;
			case 2:
				break;
			case 3:
				if (!minimapRunning)
				{
					minimapRunning = true;
					StartCoroutine(ShowMinimap());
				}
				
				break;
			case 4:
				break;
			case 5:
				break;
			case 6:
				if (grenadesNum > 0)
				{
					grenadesNum--;
					ThrowGrenade();
				}
				break;
			case 7:
				break;
			case 8:
				break;
			case 9:

				break;
			case 10:
				break;
			case 11:
			grappleActive = !grappleActive;
				Grapple(grappleActive);
				break;
			case 12:
				
				 CallShip();
				print("Calling");
				break;
		}
	}
	#region Grapple
	void Grapple(bool _enabled)
	{
		print("grapling");
		Harpoon_Gun _grapple = GetComponentInChildren<Harpoon_Gun>();
		_grapple.BreakWire();
		_grapple.anim.SetBool("Enabled", _enabled);
		anim.SetBool("Grapple", _enabled);

	}	
	//signalled from grapple
	[MRPC]
	public void RPC_FireGrapple(int parent1, int parent2, Vector3 localPos1, Vector3 localPos2)
	{
		Harpoon_Gun _grapple = GetComponentInChildren<Harpoon_Gun>();
		_grapple.ConnectGrapple(
			Game_Controller.GetGameObjectFromNetID(parent1).transform,
			Game_Controller.GetGameObjectFromNetID(parent2).transform,
			localPos1,
			localPos2
		);
	}
	[MRPC]
	public void RPC_BreakWire()
	{
		Harpoon_Gun _grapple = GetComponentInChildren<Harpoon_Gun>();
		_grapple.BreakWire();
	}
	#endregion
	public void ShowMag(int shown){
		//TODO
		/*if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("RPC_ShowMag", MRPCMode.AllBuffered, new object[]{  shown!=0 });
		} else {
			RPC_ShowMag (shown!=0 );
		}*/
	}
	[MRPC]
	public void RPC_ShowMag(bool shown){
		magGO.SetActive (shown);
		if (fireScript.magGO != null) {
			fireScript.magGO.SetActive (!shown);
		}
	}
	[MRPC]
	void RPC_Reload(){
		anim.SetTrigger ("Reload");
	}

	void ShowPieQuadrant(int quadrant){
		pieQuadrants [quadrant].SetActive (true);
	}
	void HideQuadrants(){
		try{
			pieQuadrants[0].SetActive(false);
			pieQuadrants[1].SetActive(false);
			pieQuadrants[2].SetActive(false);
			pieQuadrants[3].SetActive(false);
		}
		catch{
		}
	}
	void ThrowGrenade(){
		throwingGrenade = true;
		//TODO:
		/*if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("RPC_ThrowGrenade", MRPCMode.AllBuffered, new object[]{ });
		} else {
			RPC_ThrowGrenade ();
		}*/
	
	}
	[MRPC]
	void RPC_ThrowGrenade(){
		print ("Throwing Grenade");
		anim.SetTrigger ("Throw Grenade");
	}
	public void SpawnGrenadeModel(){
		grenadeModel = (GameObject)Instantiate (grenadeModelPrefab, grenadeSpawn);
	}

	public void SpawnGrenade(float _dropGrenadeFactor = 1f){
		throwingGrenade = false;
		Destroy (grenadeModel);


		if (isLocalPlayer) {
			return;
		}
		
		if(MInput.GetButton ("Ctrl")){
			//If we are scoped, drop the grenade beside us
			_dropGrenadeFactor = 0.05f;
		}

		Rigidbody _grenade = ((GameObject)Instantiate (grenadePrefab, grenadeSpawn.position, grenadeSpawn.rotation)).GetComponent<Rigidbody> ();

		Destroy (_grenade.gameObject, 20f);

		_grenade.AddForce (mainCam.transform.forward * 400f * _dropGrenadeFactor);
		

		int _grenadeView = 0;

		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			_grenadeView = Metwork.AllocateMetworkView (Metwork.player.connectionID);
		} else {
			_grenadeView = Metwork.AllocateMetworkView (1);
		}
		_grenade.GetComponent<MetworkView> ().viewID = _grenadeView;
		_grenade.GetComponent<Metwork_Object> ().owner = _grenadeView /1000;
		_grenade.GetComponent<Metwork_Object> ().netID = _grenadeView;
		_grenade.GetComponent<Metwork_Object>().isLocal = true;

		if (!Metwork.metViews.ContainsKey (_grenadeView)) {
			Metwork.metViews.Add (_grenadeView, _grenade.GetComponent<MetworkView> ());
		} else {
			Metwork.metViews[_grenadeView] = _grenade.GetComponent<MetworkView> ();
		}

		_grenade.GetComponent<MetworkView>().Start();
		_grenade.GetComponent<Frag_Grenade>().fromID = _grenadeView /1000;

		//TODO
		/*if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("RPC_SpawnGrenade", MRPCMode.OthersBuffered, new object[]{_grenadeView, _grenade.velocity, _grenade.position,_grenade.rotation});
		} else {
			RPC_SpawnGrenade (_grenadeView, _grenade.velocity, _grenade.position,_grenade.rotation);
		}*/

	}
	[MRPC]
	void RPC_SpawnGrenade(int _grenadeView, Vector3 _velocity, Vector3 _position, Quaternion _rotation){
		if(grenadeModel != null){
			Destroy (grenadeModel);
		}
		Rigidbody _grenade = ((GameObject)Instantiate (grenadePrefab, _position + _velocity * 0.05f, _rotation)).GetComponent<Rigidbody> ();
		

		_grenade.GetComponent<MetworkView> ().viewID = _grenadeView;
		_grenade.GetComponent<Metwork_Object> ().owner = _grenadeView /1000;
		_grenade.GetComponent<Metwork_Object> ().netID = _grenadeView;

		if (!Metwork.metViews.ContainsKey (_grenadeView)) {
			Metwork.metViews.Add (_grenadeView, _grenade.GetComponent<MetworkView> ());
		} else {
			Metwork.metViews[_grenadeView] = _grenade.GetComponent<MetworkView> ();
		}

		//Grenades MetworkView not start, changing start to awake fucks things up
		_grenade.GetComponent<MetworkView>().Start();
		_grenade.GetComponent<Frag_Grenade>().fromID = _grenadeView /1000;
		_grenade.velocity = _velocity;
	}


    void CallShip()
    {
        bool isAssigned = false;
        Ship_Controller bestShip = GameObject.FindObjectOfType<Ship_Controller>();

        foreach (Ship_Controller ship in GameObject.FindObjectsOfType<Ship_Controller>())
        {
            if (ship.target == this.transform)
            {
                ship.DisableAI();
                continue;
            }
            if (ship.player == null && !ship.isTransport && !ship.isAI)
            {
                if (!isAssigned || (ship.transform.position - this.transform.position).sqrMagnitude < (bestShip.transform.position - this.transform.position).sqrMagnitude)
                {
                    bestShip = ship;
                    isAssigned = true;
                }
            }
        }

        if (!isAssigned)
        {
            print("All ships occupied");
			WindowsVoice.Speak("All fighters occupied");
            return;
        }

		


        if (Metwork.peerType != MetworkPeerType.Disconnected)
        {
			//TODO:
            //bestShip.GetComponent<MetworkView>().RPC("RPC_ActivateAI", MRPCMode.AllBuffered, new object[] { netObj.owner });
        }
        bestShip.target = this.transform;
        bestShip.GetComponent<Raycaster>().target = this.transform.position;
        bestShip.isAI = true;
        bestShip.enabled = true;
        print("Calling: " + bestShip.name);
		WindowsVoice.Speak("Dispatching fighter");
        anim.SetTrigger("Command");
        StartCoroutine(CancelCall(bestShip));

    }

    IEnumerator CancelCall(Ship_Controller _ship)
    {
        Navigation.RegisterTarget(_ship.transform, "Dispatched Ship", 2f, Color.blue);

        Vector3 previousPosition = _ship.transform.position;
        float initialTime = Time.time;

        while (_ship.isAI)
        {
            yield return new WaitForSeconds(5f);
            if ((_ship.transform.position - previousPosition).sqrMagnitude < 25f)
            {
                _ship.DisableAI();
            }
            if (initialTime - Time.time > 120f)
            {
                Navigation.DeregisterTarget(_ship.transform);
                damageScript.TakeDamage(10000, 0, transform.position);
            }
        }

    }

    //Gains air relatively slowly
    public void GainAir()
    {
        StartCoroutine(CoGainAir());
    }
    IEnumerator CoGainAir()
    {
        while (airTime < suffocationTime)
        {
            airTime += Time.deltaTime * 10f;
            yield return new WaitForSeconds(0.1f);
        }
        airTime = suffocationTime;
    }

    public void LoseAir(){
		airTime -= Time.deltaTime;

		if((int)(airTime/suffocationTime * 100) == 50){
			WindowsVoice.Speak("Oxygen at 50%");
		}
		if((int)(airTime/suffocationTime * 100) == 10){
			WindowsVoice.Speak("Oxygen at 10%");
		}
		if((int)(airTime/suffocationTime * 100) < 2){
			WindowsVoice.Speak("Oxygen Critical <break />");
		}		
		if (airTime < 0) {
			airTime = airTime + 1f;
			damageScript.TakeDamage (10,0, transform.position + transform.forward, true);
		}
	}
	[MRPC]
	public void RPC_GetKnifed(int otherPlayerInt)
	{
		print("Getting Knifed" + name);

		GameObject otherPlayer = Game_Controller.GetGameObjectFromNetID(otherPlayerInt);

		print("other player: " + otherPlayer.name);
		//rb.MovePosition(otherPlayer.transform.position + otherPlayer.transform.forward * knifePosition);
		if (counterKnife)
		{
			//TODO:
			/*otherPlayer.GetComponent<Player_Controller>().netView.RPC("RPC_SwitchWeapons", MRPCMode.AllBuffered, new object[] { });
			
			this.netView.RPC("RPC_SwitchWeapons", MRPCMode.AllBuffered, new object[] { });*/
			counterKnife = false;
		}
		else
		{
			damageScript.TakeDamage(100, otherPlayerInt, otherPlayer.transform.position);
		}
	}
	[Command]
	public void Cmd_ShowPlayer(){
		Rpc_ShowPlayer();
	}
	[ClientRpc]
	public void Rpc_ShowPlayer(){
		this.gameObject.SetActive(true);
	}

	[MRPC]
	public void RPC_SetActive(){
		print ("Reactivating player");
		this.gameObject.SetActive (true);
	}
	
	[MRPC]
	public void RPC_Sit(bool sitMode){
		anim.SetBool ("Sitting", sitMode);
	}
	public void OpenKnife()
	{
		knifeAnim.SetBool("Knife", true);
	}
	public void RetractKnife()
	{
		knifeAnim.SetBool("Knife", false);
	}
	public void Knife(){
		print("Attempting to knife");
		if (isLocalPlayer)
		{
			return;
		}
		RaycastHit hit;
		if (Physics.SphereCast (mainCamObj.transform.position,0.01f, mainCamObj.transform.forward, out hit, 2f)) {
			if (hit.transform.root.tag == "Player") {
				if (hit.transform.root.GetComponent<Animator>().GetBool("Knife"))
				{
					counterKnife = true;
					return;
				}
				StartCoroutine(Stab(hit.transform.root.gameObject));
				

			}

		}
		//TODO:
		/*
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			netView.RPC("RPC_Knife", MRPCMode.AllBuffered, new object[] { });
		}
		else
		{
			RPC_Knife();
		}*/
	}
	IEnumerator Stab(GameObject _otherPlayer)
	{
		float i = 0;
		while (i < 1.1f)
		{
			if (!_otherPlayer.activeInHierarchy)
			{
				break;
			}
			rb.MovePosition(Vector3.Lerp(transform.position, _otherPlayer.transform.position-transform.forward*1.1f, 0.3f));
			yield return new WaitForEndOfFrame();
			i += Time.deltaTime;
		}
		//TODO
		/*
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			_otherPlayer.GetComponent<MetworkView>().RPC("RPC_GetKnifed", MRPCMode.AllBuffered, new object[] {
						netObj.owner});
		}
		else
		{
			_otherPlayer.GetComponent<Player_Controller>().RPC_GetKnifed(netObj.owner);
			print("FOund player stabbing now");
		}*/
	}
	[MRPC]
	public void RPC_Knife(){
		anim.SetBool ("Knife",true);
		Invoke("StopKnife", 0.2f);
	}
	public void StopKnife()
	{
		anim.SetBool("Knife", false);

	}
	#region Region1

	public virtual void Aim(){
		if (fireScript == null||grappleActive)
		{
			moveFactor = 0.75f;
			return;
		}
		if (MInput.GetButton ("Left Trigger")&&!fireScript.reloading  && !throwingGrenade) {
			Transform _scopeTransform = fireScript.scopePosition;
			Vector3 _scopePosition = _scopeTransform.position - _scopeTransform.forward * 0.22f + _scopeTransform.up * 0.033f;

			float _distance = Vector3.Distance(mainCam.transform.position, _scopePosition);
			if(mainCam.fieldOfView < 11 && anim.GetCurrentAnimatorStateInfo(3).IsName("Aim"+fireScript.aimAnimNumber.ToString())){
				fireScript.transform.rotation = Quaternion.RotateTowards(fireScript.scopePosition.rotation, mainCam.transform.rotation, 0.7f);
				mainCam.transform.position = Vector3.Lerp(mainCam.transform.position,_scopePosition,0.7f);//Mathf.Clamp(0.01f/(_distance),0f,0.5f));
				//fireScript.transform.rotation *= Quaternion.FromToRotation(fireScript.scopePosition.forward, mainCam.transform.forward);
				// * Quaternion.Inverse((fireScript.scopePosition.rotation * Quaternion.Inverse(fireScript.transform.rotation)));
				//mainCam.transform.rotation = Quaternion.Lerp(mainCam.transform.rotation, fireScript.scopePosition.rotation, 0.7f);
			}
			anim.SetBool ("Scope", true);
			//StartCoroutine (Zoom (true));
			Zoom(true);
			lookFactor = 0.3f;
			if (walkState!=WalkState.Crouching)
			{
				moveFactor = 0.75f;
			}
			else
			{
				moveFactor = 0.5f;

			}
			recoilString = "Recoil" + fireScript.recoilNumber;
			if (Time.frameCount % 3f == 0) {
				//TODO
				/*if (Metwork.peerType != MetworkPeerType.Disconnected) {
					netView.RPC ("RPC_Aim", MRPCMode.Others, new object[]{true });
				} */
			}

		} else {
			mainCam.transform.localPosition = Vector3.Lerp(mainCam.transform.localPosition,originalCamPosition,0.2f*Time.deltaTime/0.034f);
			mainCam.transform.localRotation = Quaternion.Lerp(mainCam.transform.localRotation, originalCamRotation, 0.1f);

			anim.SetBool ("Scope", false);
			Zoom(false);
			if (walkState!=WalkState.Crouching)
			{
				moveFactor = 1f;
			}
			else
			{
				moveFactor = 0.75f;

			}
			lookFactor = 1f;
			recoilString = "Recoil" + fireScript.recoilNumber+"*";
			if (Time.frameCount+1 % 3f == 0) {
				/*
				if (Metwork.peerType != MetworkPeerType.Disconnected) {
					netView.RPC ("RPC_Aim", MRPCMode.All, new object[]{false });
				} */
			}

		}
	}
	[MRPC]
	public void RPC_Aim(bool scoped){
		
		anim.SetBool ("Scope", scoped);
	}
	//zoom in to true = zooming in
	//zooms the camera on scope

	public void Zoom(bool zoomIn){
		float i = mainCam.fieldOfView;


		if (zoomIn == true) {
			mainCam.fieldOfView = Mathf.Lerp(i,10f,0.5f);
		}else
		{
			mainCam.fieldOfView = Mathf.Lerp(i, 90f, 0.5f);
		}
	}
	[MRPC]
	public void RPC_Jump(bool jumping, bool jets){
		if (jets) {
			foreach (ParticleSystem jet in jetpackJets) {
				jet.Play ();
			}
		} else {
			foreach (ParticleSystem jet in jetpackJets) {
				jet.Stop ();
			}
		}
		anim.SetBool ("Jump", jumping);
		


	}
	public void Jump(){
		
		if (jetpackFuel > 0.7f && refueling == true)
		{
			refueling = false;

		}
		if (jetpackFuel <= 0f)
		{
			refueling = true;
		}
		if (refueling == false) {
			//Factor to make realistic rocket effects
			//TODO: Get rid of all this shit
			float _fuelFactor = Mathf.Pow(2f*jetpackFuel - 2.6f, 4) * Mathf.Cos(1f + 2f*jetpackFuel - 2.6f) + 1;
			rb.AddRelativeForce (0f, Time.deltaTime * 30f * forceFactor * z * 2f, 0f);
			//rb.velocity = new Vector3(rb.velocity.x,12f, rb.velocity.z);
			foreach (ParticleSystem jet in jetpackJets) {
				jet.Play ();
			}
			jetpackFuel -= Time.deltaTime * 2.5f;
		} else {
			foreach (ParticleSystem jet in jetpackJets) {
				jet.Stop ();
			}
		}
		
		UpdateUI ();
	}
	void Hop()
	{
		if (Time.time > jumpWait)
		{
			Debug.DrawLine(transform.position+rb.centerOfMass,transform.position- transform.up * 1.2f);
			if (Physics.Linecast(transform.position+rb.centerOfMass,transform.position- transform.up * 1.4f))
			{
				rb.velocity += transform.up*7f;
				jumpWait = Time.time + 1f;
			}

		}
	}

	

	public void UseItem(){
		RaycastHit hit;
		if (Physics.Raycast (mainCamObj.transform.position, mainCamObj.transform.forward, out hit)) {
			
			if (hit.distance < 20f) {
				
				try {
					hit.collider.GetComponent<Activater> ().ActivateScript (this.gameObject);

				} catch {
					try {
						hit.collider.transform.parent.GetComponent<Activater> ().ActivateScript (this.gameObject);
					} catch {
						try{hit.transform.root.GetComponent<Activater> ().ActivateScript (this.gameObject);
						}
						catch{
							fireScript.StartCoroutine ("Reload");
						}
					}
				}
				
			} else {
				fireScript.StartCoroutine ("Reload");
				UpdateUI ();
			}
			

				
		} else {
			fireScript.StartCoroutine ("Reload");
			UpdateUI ();
		}

	}

	public void SpaceMove(){
		
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		
		float factor = Time.deltaTime * forceFactor;
		float strafeFactor = Input.GetButton("Ctrl") ? 0f:1f;
		float rollFactor = Input.GetButton("Ctrl") ? 1f:0f;
		magBootsLock = false;

		//TODO: Tie this into the jetpack fuel
		if (false && Input.GetButtonDown("Sprint"))//&&(Time.time >jumpWait))
		{
			jumpWait = Time.time + 5f;
			rb.AddRelativeForce (new Vector3(0f, z*Time.deltaTime* forceFactor * 20f, v *Time.deltaTime* forceFactor * 20f)*60f);

		}
		else
		{
	
			rb.AddRelativeTorque(-v2 * factor/2f, h2*factor/7f, -h * factor/1.5f * rollFactor);
			Vector3 desiredVelocity = transform.TransformVector(new Vector3(h,z,v) * moveSpeed);
			if(desiredVelocity.sqrMagnitude < previousVelocity.sqrMagnitude*0.5f){
				rb.velocity = Vector3.Lerp(previousVelocity, Vector3.zero, 0.05f);
			}
			else{
				rb.velocity = Vector3.Lerp(previousVelocity, desiredVelocity, 0.7f);
			}
			
		}
		anim.SetBool("Float", true);
		

		//Raycast down to find ground to lock on to
		//Also check if the jump key is pressed
		if(Input.GetButton("Jump") && Input.GetAxis("Move Y") <= 0.05f){
			RaycastHit _hit = new RaycastHit();
			RaycastHit _hit2 = new RaycastHit();
			RaycastHit _hit3 = new RaycastHit();


			//Raycast from three different spots
			bool hasHit1 = Physics.SphereCast(transform.position + transform.up * 0.1f,0.1f,- transform.up * 5.4f,out _hit,5.4f, magBootsLayer,  QueryTriggerInteraction.Ignore);
			bool hasHit2 = Physics.Linecast(transform.position - transform.right * 0.1f,transform.position+ transform.right * 0.15f- transform.up * 5.4f, out _hit2, magBootsLayer, QueryTriggerInteraction.Ignore);
			bool hasHit3 = Physics.Linecast(transform.position + transform.right * 0.1f,transform.position- transform.right * 0.15f- transform.up * 5.4f, out _hit3, magBootsLayer,  QueryTriggerInteraction.Ignore);
			//Hit distance is zero if no hit
			_hit.distance = !hasHit1 ? 100000f : _hit.distance;
			//_hit2.distance = !hasHit2 ? 100000f : _hit2.distance;
			//_hit3.distance = !hasHit3 ? 100000f : _hit3.distance;

			if( _hit.distance <= 5.4f){// || _hit2.distance <= 5.4f || _hit3.distance <= 5.4f){
				//Take the weighted average of the three distances;
				Vector3 _hitNormal = _hit.normal; //(_hit.normal / _hit.distance + _hit2.normal / _hit2.distance + _hit3.normal / _hit3.distance)/3f; 
				
				if(Vector3.Dot(_hitNormal.normalized, previousNormal.normalized)> 0.4f){

					_hitNormal = Vector3.Slerp(previousNormal,_hitNormal, 0.5f);

				}
				else{
					
					_hitNormal = Vector3.Slerp(_hitNormal, previousNormal, 0.95f);
				}
				
				
				float _hitDistance = 1f / (1f/_hit.distance);// + 1f/_hit2.distance + 1f/_hit3.distance);
				Vector3 _hitPoint = _hit.point;//(_hit.point / _hit.distance + _hit2.point / _hit2.distance + _hit3.point / _hit3.distance) / (_hitDistance); 
				
				//print(_hit.point + " " + _hitPoint);

				Vector3 _lerpedForward = Vector3.Slerp(transform.forward,Vector3.ProjectOnPlane(transform.forward, _hitNormal), 0.3f * (1f-_hitDistance/5.4f));
				Vector3 _lerpedUp =  Vector3.Slerp(transform.up,_hitNormal,0.3f*( 1f-_hitDistance/5.4f));
				previousNormal = _lerpedUp;
				rb.transform.rotation = Quaternion.LookRotation(_lerpedForward,_lerpedUp);
				
				Debug.DrawRay(this.transform.position, Vector3.ProjectOnPlane(transform.forward,_hitNormal), Color.yellow);
				Debug.DrawRay(_hit.point,_hitNormal, Color.green);
				rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
				//rb.AddForceAtPosition((transform.up).normalized * -10000000f /Mathf.Clamp(0.01f, 10000f, (_hitDistance * _hitDistance)), lfRaycast.position);
				rb.velocity += transform.up * -98100f /Mathf.Clamp(0.01f, 10000f, (_hitDistance * _hitDistance)) * Time.deltaTime;

				float lookUpDownTime = anim.GetFloat("Look Speed");	
				anim.SetFloat("Look Speed", Mathf.Clamp(lookUpDownTime + v2 * 0.5f*Time.deltaTime, -1f, 1f));
				if (Time.frameCount % 4==0) {
					/*TODO
					if (Metwork.peerType != MetworkPeerType.Disconnected) {
						netView.RPC ("RPC_Look", MRPCMode.Others, new object[]{ lookUpDownTime });
					}*/
				}
				rb.angularDrag = 20f;
				//Add torque to compensate for extra friction
				rb.AddRelativeTorque(0, h2*factor/3f* 20f, 0);
				rb.rotation = new Quaternion(0,1,0,0) * rb.rotation;
				//Add left/right force (to convert the roll force to side-side)
				
				//rb.AddRelativeForce(h * Time.deltaTime * forceFactor * 20f,0,0);
				if(_hit.distance <= 0.2f){
					//TODO: Finishe
					rb.velocity = transform.TransformVector(new Vector3(h,z,v).normalized * moveSpeed * 1.5f);
					magBootsLock = true;
				}
				else{
					magBootsLock = false;
				}
				
				
			}
			else{
				
			}
			
		}
		
		//Avoid orbiting around the player while the camera is outside the player body (in spawing)
		if(Vector3.Distance(mainCam.transform.localPosition, originalCamPosition) < 3f){
			//Return to zero-g defaults
			//rb.constraints = RigidbodyConstraints.None;
			rb.angularDrag = 1f;
			if(!Input.GetButton("Jump")){
				previousNormal = transform.up;
			}

			//Try to right the player's body and camera (as in exit gravity)
			Vector3 _aimDirection = mainCam.transform.forward;
			float _originalLookTime = anim.GetFloat("Look Speed");
			Vector3 _originalForward = transform.forward;
		
			float _counter = Vector3.Dot(_aimDirection.normalized,_originalForward.normalized);
			if(_counter < 0.97f){
				
				//transform.LookAt(transform.position + Vector3.Slerp (_originalForward, _aimDirection,0.5f));
				Vector3 _lerpedForward = Vector3.Slerp (_originalForward, _aimDirection,0.3f);
				Vector3 _lerpedUp = Vector3.ProjectOnPlane(transform.up,_lerpedForward);
				rb.transform.rotation = Quaternion.LookRotation(_lerpedForward, _lerpedUp);
				//anim.SetFloat ("Look Speed",Mathf.Lerp(_originalLookTime ,0.5f,_counter));
				anim.SetFloat ("Look Speed", 0.5f - 0.8f * Vector3.SignedAngle(_originalForward,transform.forward,transform.right)/90f);
				//rb.AddForce(Vector3.Lerp(Vector3.zero,Vector3.up * 9.81f,  _counter), ForceMode.Acceleration);
			}

			

			Vector3 _rotateAmount = Vector3.Lerp(previousRot,new Vector3(-v2 * 2f, h2*2f, -h * 1.5f * rollFactor) * 5f * Time.deltaTime * 30f, 0.05f);
			rb.transform.Rotate(_rotateAmount);
			previousRot = _rotateAmount;

			thrusterSoundFactor = 0.5f * Mathf.Ceil(Mathf.Abs(h) + Mathf.Abs(z)) + 0.3f * Mathf.Ceil(Mathf.Abs(v));

				
			
			
		}
		if (Time.frameCount % 4 == 0) {
			/*TODO
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netView.RPC ("RPC_AnimateSpace", MRPCMode.Others, new object[]{ thrusterSoundFactor});
			} */
		}
		
	}
	public void AnimateMovement(){
		anim.SetFloat ("H Movement", h*moveFactor);
		anim.SetFloat ("V Movement", v*moveFactor);
		anim.SetInteger ("Walk State", (int)walkState);
		
		float forwardSpeed = v+Mathf.Abs(h2)/2f;//Mathf.Clamp(transform.InverseTransformVector (rb.velocity).z, -5f, 5f);

		if (Time.frameCount % 4 == 0) {
			/*TODO
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netView.RPC ("RPC_AnimateMovement", MRPCMode.Others, new object[]{ (int)walkState, h*moveFactor,v*moveFactor, thrusterSoundFactor});
			}*/ 
		}
	}
	public void FootstepAnim(){
		//if (!walkSound.isPlaying) {
			walkSound.PlayOneShot(walkClips[Random.Range(0,walkClips.Length)]);//[Mathf.Clamp(Time.frameCount % 4,0,walkClips.Length-1)]);
		//}
	}
	[MRPC]
	public void RPC_AnimateMovement(int state, float hSpeed, float vSpeed, float _thrusterSoundFactor){
		anim.SetFloat ("H Movement", hSpeed);
		anim.SetFloat ("V Movement", vSpeed);
		anim.SetInteger ("Walk State", state);
		thrusterSoundFactor = _thrusterSoundFactor;
		//walkSound.volume = Mathf.Clamp01 (Mathf.Abs (hSpeed + vSpeed));
	}

	[MRPC]
	public void RPC_AnimateSpace(float _thrusterSoundFactor){
		thrusterSoundFactor = _thrusterSoundFactor;
		//walkSound.volume = Mathf.Clamp01 (Mathf.Abs (hSpeed + vSpeed));
	}

	[MRPC]
	public void RPC_Crouch(){
		CapsuleCollider[] capsules = this.GetComponents<CapsuleCollider> ();
		Vector3 center = capsules [0].center;
		center.y = capsule_crouchY1;
		capsules [0].center = center;
		capsules [0].height = capsule_crouchHeight1;
		Vector3 center2 = capsules [1].center;
		center2.y = capsule_crouchY2;
		capsules [1].center = center2;
		capsules [1].height = capsule_crouchHeight2;
	}
	[MRPC]
	public void RPC_UnCrouch(){
		CapsuleCollider[] capsules = this.GetComponents<CapsuleCollider> ();
		Vector3 center = capsules [0].center;
		center.y = capsule_originalY1;
		capsules [0].center = center;
		capsules [0].height = capsule_originalHeight1;
		Vector3 center2 = capsules [1].center;
		center2.y = capsule_originalY2;
		capsules [1].center = center2;
		capsules [1].height = capsule_originalHeight2;

		walkState = WalkState.Walking;
	}
	[MRPC]
	public void RPC_ExitVehicle()
	{
		airTime = suffocationTime;
		/*TODO
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{

			netView.RPC("RPC_Sit", MRPCMode.AllBuffered, new object[] { false });
		}
		else
		{
			RPC_Sit(false);
		}*/
		inVehicle = false;
		//player.rb.isKinematic = false;
		//TODO: This may suffer an IEnumerator problem
		ExitGravity();
		CapsuleCollider[] capsules = GetComponents<CapsuleCollider>();
		capsules[0].enabled = true;
		capsules[1].enabled = true;
	}


	//Updated to 
	public void MovePlayer()
	{
		Vector3 groundVelocity = Vector3.zero;
		float groundAngle = 0f;
		if (shipRB)
		{
			groundVelocity = Vector3.Project(rb.velocity, shipRB.transform.up);
			groundVelocity += shipRB.GetPointVelocity(rb.position);
			groundAngle = Vector3.Dot(shipRB.angularVelocity, shipRB.transform.up) * Mathf.Rad2Deg * Time.deltaTime;
			//rb.MoveRotation(Quaternion.Euler(shipRB.angularVelocity*Mathf.Rad2Deg * Time.deltaTime));
		}
		else
		{
			groundVelocity = Vector3.Project(rb.velocity, Vector3.up);
		}
		float _speed = 1f;
		switch (walkState)
		{
			case WalkState.Walking:_speed = 1f;
				break;
				case WalkState.Running:_speed = 1.25f;
				break;
				case WalkState.Crouching:_speed = 0.75f;
				break;
		}
		rb.AddRelativeForce(-transform.up * 9.81f, ForceMode.Acceleration);
		rb.velocity = groundVelocity + transform.TransformVector(h * 3f * _speed, 0f, v * _speed * 4f);

		transform.Rotate(Vector3.up, h2 * Time.deltaTime * 180f+groundAngle); //*Quaternion.AngleAxis(h2 * Time.deltaTime*180f,shipRB.transform.up);

		if (shipRB)
		{
			Vector3 previousForward = Vector3.ProjectOnPlane(transform.forward, shipRB.transform.up);
			Vector3 _up =shipRB.transform.up;
			rb.transform.rotation = Quaternion.LookRotation(previousForward, _up);
		}

		if (Time.frameCount % 4 == 0)
		{
			CheckStep();
		}
	}
	void CheckStep()
	{
		if (v > 0.1f)
		{
			Vector3 footPos = lfRaycast.position;
			RaycastHit _hit;
			if (Physics.SphereCast(footPos,0.1f, transform.forward,out _hit,0.3f))
			{
				if (!Physics.Raycast(footPos + transform.up * 0.4f, transform.forward, 0.35f))
				{
					rb.AddRelativeForce(0f, 240f * rb.mass * 9.81f*Time.deltaTime, 0f);
				}
			}
		}
	}

	public void MouseLook(){
		float lookUpDownTime = anim.GetFloat("Look Speed");		
		anim.SetFloat("Look Speed", Mathf.Clamp(lookUpDownTime + v2 * 2f*Time.deltaTime, -1f, 1f));
		
		if (Time.frameCount % 4==0) {
			/*TODO
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netView.RPC ("RPC_Look", MRPCMode.Others, new object[]{ lookUpDownTime });
			}*/
		}

	
	}
	[MRPC]
	public void RPC_Look(float time){
		AdjustView(time);

	}
	public void AdjustView(float time)
	{
		float lookTime = anim.GetFloat("Look Speed");
		anim.SetFloat("Look Speed", Mathf.Lerp(lookTime, time, 0.5f));
	}
	public void TurnHead(){
		anim.SetFloat ("Head Turn Speed", -h2*0.5f+0.5f);
		anim.SetBool("Head Turn Enabled", true);

	}


	[MRPC]
	public void RPC_ClimbLadder(){
		anim.SetBool ("Climb Ladder", true);
	}
	[MRPC]
	public void RPC_LeaveLadder(){
		anim.SetBool ("Climb Ladder", false);
	}

	public virtual void Attack(){
		if (!grappleActive)
		{
			fireScript.shotSpawn.transform.forward = this.mainCam.transform.forward 
													+ muzzleClimb * fireScript.recoilAmount * mainCam.transform.up * 0.3f
													+ fireScript.recoilAmount * (0.2f + muzzleClimb) * (Vector3)(Random.insideUnitCircle) * (anim.GetBool ("Scope") ? 0.1f : 0.3f)
													;
			if(muzzleClimb < 0.6f){
				muzzleClimb += 0.05f;
			}
			fireScript.FireWeapon();
		}
		UpdateUI ();
	}
	public void Recoil(){
		anim.Play (recoilString, 2, 1-Random.Range(recoilAmount*0.7f,recoilAmount));
		//anim.SetFloat("Recoil", 1 - Random.Range(recoilAmount * 0.7f, recoilAmount));
	}
	#endregion
	#region Region2
	public void Die(){
		if(throwingGrenade){
			SpawnGrenade(0.25f);
		}
		//sceneCam.enabled = true;
		/*TODO
		if(Metwork.peerType  != MetworkPeerType.Disconnected){
			netView.RPC ("RPC_Die", MRPCMode.AllBuffered, new object[]{ });
		}
		else{
			RPC_Die ();
		}

		if (netObj == null) {
			netObj = GetComponent<Metwork_Object> ();
		}*/
		

		Invoke("CoDie", 4f);


	}

	public void CoDie(){
		
		Game_Controller.Instance.sceneCam.enabled = true;
		Game_Controller.Instance.GetComponent<AudioListener>().enabled = true;
		
		if (isLocalPlayer) {
			if (!SceneManager.GetSceneByName("SpawnScene").isLoaded)
			{
				SceneManager.LoadScene("SpawnScene", LoadSceneMode.Additive);
			}
		}
	}

	[MRPC]
	public void RPC_Die(){
		Vector3 position = this.transform.position;
		Quaternion rotation = this.transform.rotation;
		inVehicle = false;
		//disconnect all joints
		ConfigurableJoint[] joints = FindObjectsOfType<ConfigurableJoint>();
		foreach (ConfigurableJoint joint in joints) {
			if (joint.connectedBody == rb) {
				joint.connectedBody = null;
			}
		}
		//disable the grapple
		if (GetComponentInChildren<Harpoon_Gun>() != null)
		{
			Grapple(false);
		}
		GameObject _ragdollGO = (GameObject)Instantiate (ragdoll, position, rotation);
		Destroy (_ragdollGO, 5f);
		foreach(Rigidbody _rb in _ragdollGO.GetComponentsInChildren<Rigidbody>()){

			_rb.velocity = Vector3.ClampMagnitude(rb.velocity, 20f);
			_rb.useGravity = rb.useGravity;
		}
		try{
			GameObject droppedWeapon = (GameObject)Instantiate (fireScript.gameObject, position, rotation);
			droppedWeapon.AddComponent<Rigidbody> ().useGravity = rb.useGravity;
			droppedWeapon.GetComponent<Fire> ().enabled = false;
			droppedWeapon.transform.localScale = this.fireScript.gameObject.transform.lossyScale;

			Destroy (droppedWeapon, 20f);
			//droppedWeapon.GetComponent<Activater> ().enabled = true;
		}
		catch{}



        this.transform.position = Vector3.up * 10000f;
        this.gameObject.SetActive (false);

		if(!isLocalPlayer){
			_ragdollGO.GetComponentInChildren<Camera>().gameObject.SetActive(false);
		}


	}



	//Called by grav controller when entering / exiting gravity;
	public IEnumerator ExitGravity(){
		exitingGravity = true;
		//print("Exiting Gravity");
		rb.angularDrag = 1f;
		rb.drag = 0.3f;
		rb.constraints = RigidbodyConstraints.None;
		anim.SetBool ("Float", true);
		anim.SetBool ("Jump", false);
		anim.SetInteger("Walk State", 0);

		//Avoid orbiting around the player while the camera is outside the player body (in spawing)
		while(Vector3.Distance(mainCam.transform.localPosition, originalCamPosition) > 3f){
			yield return new WaitForSeconds(0.5f);
		}

		//yield return new WaitUntil (() => Mathf.Abs (anim.GetCurrentAnimatorStateInfo (1).normalizedTime - 0.5f) < 0.05f);
		Vector3 _aimDirection = mainCam.transform.forward;
		float _originalLookTime = anim.GetFloat("Look Speed");
		Vector3 _originalForward = transform.forward;
	
		float _counter = 0;
		while (_counter < 1f)
		{
			
			transform.forward = Vector3.Slerp (_originalForward, _aimDirection,_counter);
			anim.SetFloat ("Look Speed",Mathf.Lerp(_originalLookTime ,0.5f,_counter));
			rb.AddForce(Vector3.Lerp(Vector3.zero,Vector3.up * 9.81f,  _counter), ForceMode.Acceleration);
			yield return new WaitForEndOfFrame();
			_counter+= 0.1f;
		}
		transform.forward = _aimDirection;

		if(isLocalPlayer){
			breatheSound.Play ();
		}
		//print("Exited");
		//walkSound.Stop ();

		lookFactor = 1f;
		exitingGravity = false;

	}
	public IEnumerator EnterGravity(){
		if (enteringGravity)
		{
			yield return null;
		}
		enteringGravity = true;
		rb.drag = 0f;

		anim.SetBool ("Float", false);
		
		rb.angularVelocity = Vector3.zero;

		float _counter = 0f;

		//Avoid orbiting around the player while the camera is outside the player body (in spawing)
		while(Vector3.Distance(mainCam.transform.localPosition, originalCamPosition) > 3f){
			yield return new WaitForSeconds(0.5f);
		}

		Vector3 newForwardVector;
		if(shipRB){
			newForwardVector = Vector3.ProjectOnPlane(transform.forward, shipRB.transform.up);//new Vector3(transform.forward.x, 0f, transform.forward.z);
		}else{
			newForwardVector = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
		}
		Vector3 _originalForward = mainCam.transform.forward;
		float _aimDirection = Mathf.Clamp01(0.5f-Vector3.SignedAngle( newForwardVector,mainCam.transform.forward, transform.right)/250f);
		
		while (_counter < 1f)
		{
			transform.forward = Vector3.Lerp(_originalForward, newForwardVector, _counter);
			anim.SetFloat ("Look Speed",Mathf.Lerp(0.5f,_aimDirection,_counter));
			if(shipRB){
				rb.AddForce(Vector3.Lerp(shipRB.transform.up * 9.81f, Vector3.zero, _counter), ForceMode.Acceleration);
			}
			else{
				rb.AddForce(Vector3.Lerp(Vector3.up * 9.81f, Vector3.zero, _counter), ForceMode.Acceleration);
			}

			yield return new WaitForEndOfFrame();
			_counter += 0.1f;
		}
		
		transform.forward = newForwardVector;


		breatheSound.Stop ();
		lookFactor = 1f;
		enteringGravity = false;
	}
	[Command]
	public void Cmd_SwitchWeapons(bool _primary){
		Rpc_SwitchWeapons(_primary);
	}
	[ClientRpc]
	public void Rpc_SwitchWeapons(bool _primary){
		StartCoroutine (SwitchWeapons (_primary));
	}
	public IEnumerator SwitchWeapons(bool _primary){
		anim.SetBool ("Switch Weapons", true);
		yield return new WaitForSeconds (0.5f);
		if (_primary) {
			primaryWeapon.SetActive (false);
			secondaryWeapon.SetActive (true);
			fireScript = secondaryWeapon.GetComponent<Fire> ();
			primarySelected = false;
		} else {
			secondaryWeapon.SetActive (false);
			primaryWeapon.SetActive (true);
			fireScript = primaryWeapon.GetComponent<Fire> ();
			primarySelected = true;
		}
		
		if(isLocalPlayer){
			UI_Manager._instance.ChangeWeapon (primarySelected);
		}
		recoilAmount = fireScript.recoilAmount;
		recoilString = "Recoil" + fireScript.recoilNumber;
		
		//TODO
		fireScript.playerID = playerID;
		anim.SetBool ("Switch Weapons", false);
		//TODO
		fireScript.playerID = playerID;

		anim.SetFloat ("Drift", fireScript.bulk);

		anim.Play ("Aim" + fireScript.aimAnimNumber, 3);
		anim.Play (recoilString, 2, 0f);
		//We want to move the right hand target back and forth depending how long the gun is
		this.GetComponent<Player_IK>().rhOffset = fireScript.rhOffset;
		this.GetComponent<Player_IK>().rhTarget = rightHandPosition;
		this.GetComponent<Player_IK>().lhTarget = fireScript.lhTarget;


	}
	public int GetTeam()
	{
		//TODO
		return gameController.statsArray[playerID].team;
	
	}
	public void SetupWeapons(){
		try {
			loadoutSettings = Util.LushWatermelon(System.IO.File.ReadAllLines (Application.persistentDataPath+"/Loadout Settings.txt"));
			string loadoutSettingsString = "";
			foreach(string line in loadoutSettings){
				loadoutSettingsString+=line+'/';
			}

			if(primaryNetView == -1 || secondaryNetView == -1){
				if (Metwork.peerType != MetworkPeerType.Disconnected) {
					primaryNetView = Metwork.AllocateMetworkView (Metwork.player.connectionID);
					secondaryNetView = Metwork.AllocateMetworkView (Metwork.player.connectionID);
				} else {
					primaryNetView = Metwork.AllocateMetworkView (1);
					secondaryNetView = Metwork.AllocateMetworkView (1);
				}
			}
			else{
				//Metwork.metViews.Remove(_primaryNetView);
				//Metwork.metViews.Remove(_secondaryNetView);
			}

			LoadWeaponData(loadoutSettingsString);
			/*TODO
			if(Metwork.peerType  != MetworkPeerType.Disconnected){
				netView.RPC("RPC_LoadWeaponData",MRPCMode.AllBuffered, new object[]{loadoutSettingsString, primaryNetView, secondaryNetView });
			}
			else{
				RPC_LoadWeaponData(loadoutSettingsString, primaryNetView, secondaryNetView);
			}*/




		} catch{


			print ("Failed file creating new");
			loadoutSettings = Profile.RestoreLoadoutFile ();
			string loadoutSettingsString = "";
			foreach(string line in loadoutSettings){
				loadoutSettingsString+=line+'/';
			}

			if (primaryNetView == -1 || secondaryNetView == -1) {
				if (Metwork.peerType != MetworkPeerType.Disconnected) {
					primaryNetView = Metwork.AllocateMetworkView (Metwork.player.connectionID);
					secondaryNetView = Metwork.AllocateMetworkView (Metwork.player.connectionID);
				} else {
					primaryNetView = Metwork.AllocateMetworkView (1);
					secondaryNetView = Metwork.AllocateMetworkView (1);
				}
			}

			LoadWeaponData(loadoutSettingsString);
			/*TODO
			if(Metwork.peerType  != MetworkPeerType.Disconnected){
				
				netView.RPC("RPC_LoadWeaponData",MRPCMode.AllBuffered, new object[]{loadoutSettingsString,primaryNetView,secondaryNetView});
			}
			else{
				RPC_LoadWeaponData(loadoutSettingsString,primaryNetView,secondaryNetView);
			}*/
		}
	}
	//CHECK This may need to be a networked function
	public void LoadWeaponData(string settingsString){//}, int _primaryMetID, int _secondaryMetID){
		
		string[] settings = settingsString.Split ('/');

		Destroy(this.primaryWeapon);
		Destroy(this.secondaryWeapon);

		primaryWeaponPrefab = (GameObject)Resources.Load ("Weapons/" + settings [0]);
		secondaryWeaponPrefab = (GameObject)Resources.Load ("Weapons/" + settings [1]);

		primaryLocalPosition = Loadout_Controller.gunLocalPositions [settings [0]];
		secondaryLocalPosition = Loadout_Controller.gunLocalPositions [settings [1]];

		primaryLocalRotation = Loadout_Controller.gunLocalRotations [settings [0]];
		secondaryLocalRotation = Loadout_Controller.gunLocalRotations [settings [1]];
		//add the primary weapon to the finger
		//TODO Needs to be spawned
		primaryWeapon = (GameObject)Instantiate (primaryWeaponPrefab, finger);
		primaryWeapon.transform.localPosition = primaryLocalPosition;
		primaryWeapon.transform.localRotation = Quaternion.Euler (primaryLocalRotation);
		secondaryWeapon = (GameObject)Instantiate (secondaryWeaponPrefab, finger);
		secondaryWeapon.transform.localPosition = secondaryLocalPosition;
		secondaryWeapon.transform.localRotation = Quaternion.Euler (secondaryLocalRotation);
		secondaryWeapon.SetActive (false);

		//scopes now
		GameObject primaryScope = (GameObject)Resources.Load ("Weapons/Scopes/" + settings [2]);
		Instantiate (primaryScope, primaryWeapon.GetComponent<Fire> ().scopePosition);

		GameObject secondaryScope = (GameObject)Resources.Load ("Weapons/Scopes/" + settings [3]);
		Instantiate (secondaryScope, secondaryWeapon.GetComponent<Fire> ().scopePosition);

		/*TODO Remove
		primaryWeapon.GetComponent<MetworkView> ().viewID = _primaryMetID;
		if (!Metwork.metViews.ContainsKey (_primaryMetID)) {
			Metwork.metViews.Add (_primaryMetID, primaryWeapon.GetComponent<MetworkView> ());
		} else {
			Metwork.metViews[_primaryMetID] = primaryWeapon.GetComponent<MetworkView> ();
		}

		secondaryWeapon.GetComponent<MetworkView> ().viewID = _secondaryMetID;
		if (!Metwork.metViews.ContainsKey (_secondaryMetID)) {
			Metwork.metViews.Add (_secondaryMetID, secondaryWeapon.GetComponent<MetworkView> ());
		} else {
			Metwork.metViews[_secondaryMetID] = secondaryWeapon.GetComponent<MetworkView> ();
		}*/
		//CHECK The latency on this switch may be too much to bear, it may be better to do it locally
		Cmd_SwitchWeapons(false);
		
	}
	public void LoadPlayerData(){
		string[] playerData = new string[10];
		string playerDataString = "";

		try {
			playerData = Util.LushWatermelon(System.IO.File.ReadAllLines (Application.persistentDataPath + "/Player Data.txt"));
		} catch {
			print ("Loding player data failer");
			playerData = Profile.RestoreDataFile ();
		}

		playerName = playerData [0];
		if (playerName.StartsWith ("$132435**ADMIN")) {
			playerName = playerName.Remove (0, 14);
			gameController.statsArray [playerID].name = playerName;
			Invoke("RegisterAdmin",2f);

		}
		else if (playerName.StartsWith ("%^A#R*7**MODERATOR")) {
			playerName = playerName.Remove (0, 18);
			gameController.statsArray [playerID].name = playerName;
			Invoke("RegisterModerator",2f);
		} 
		foreach (string line in playerData) {
			playerDataString += line + '/';
		}
		/*TODO	
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("RPC_LoadPlayerData", MRPCMode.OthersBuffered, new object[]{ playerDataString });
		} else {
			RPC_LoadPlayerData (playerDataString);
		}*/

		if (playerName == "") {
			name = "Unnamed Player";
		}



	}
	[MRPC]
	public void RPC_LoadPlayerData(string dataString){
		gameController = FindObjectOfType<Game_Controller> ();
		if (gameController.localPlayer == null) {
			gameController.GetLocalPlayer ();
		}
		try {
			string[] data = dataString.Split ('/');
			playerName = data [0];
		
		} catch {
			print ("failed To extract name from string");
			playerName = "Unnamed Character";

		}
		if (playerName.StartsWith ("$132435**ADMIN")) {
			Util.ShowMessage("All Units: Admin is online");
			playerName = playerName.Remove (0, 14);
			//TODO gameController.statsArray [netObj.netID].name = playerName;
			Invoke("RegisterAdmin",2f);
		}
		else if (playerName.StartsWith ("%^A#R*7**MODERATOR")) {
			playerName = playerName.Remove (0, 18);
			//TODOgameController.statsArray [netObj.netID].name = playerName;
			Invoke("RegisterModerator",2f);
		} 
		nameTextMesh.text = playerName;

	}
	[MRPC]
	public void RPC_ShowNameText(){
		gameController = FindObjectOfType<Game_Controller> ();
		if (gameController.localPlayer == null) {
			gameController.GetLocalPlayer ();
		}
		int localTeam = gameController.GetLocalTeam();
		if (GetTeam() == localTeam && this.gameObject!=gameController.localPlayer) {
			nameTextMesh.color = new Color (0f, 50f, 255f);
			nameTextMesh.gameObject.SetActive (true);

		} else {
			nameTextMesh.color = new Color (255f, 0f, 0f);
			nameTextMesh.gameObject.SetActive (false);
		}
		if (this.gameObject == gameController.localPlayer)
		{
			helmet.SetActive(false);
		}
	}
	
	public void UpdateUI(){
		if(!isLocalPlayer){
			return;
		}
		if (fireScript != null) {
			UI_Manager._instance.UpdateAmmo(fireScript.magAmmo, fireScript.magSize, fireScript.totalAmmo);
		}
		UI_Manager._instance.fuelBar.localScale = new Vector2(1f, jetpackFuel / 2f);
		blackoutShader.ChangeBlood (Mathf.Clamp01(1-damageScript.currentHealth/100f));


	}
	
	public IEnumerator ShowMinimap(){
		int _iterations = 20;
		for(int i = 0; i < _iterations; i++) {
			GameObject[] _icons = GameObject.FindGameObjectsWithTag("Icon");
				
			foreach(GameObject _icon in _icons){
				Material mat = _icon.GetComponent<MeshRenderer> ().material;
				mat.color = new Color (1f, 1f, 0f, 1f-(float)i/(_iterations-1f));
				_icon.GetComponent<MeshRenderer> ().material = mat;

			}
			yield return new WaitForSeconds (5f/_iterations);
			
		}
		yield return new WaitForSeconds (2f);
		minimapRunning = false;
	}
	//Admin only
	public void RegisterModerator(){
		damageScript.originalHealth = 300;
		damageScript.currentHealth = damageScript.originalHealth;
		primaryWeapon.GetComponent<Fire> ().magSize = 50;
		secondaryWeapon.GetComponent<Fire> ().damagePower = 40;
		secondaryWeapon.GetComponent<Fire> ().fireType = Fire.FireTypes.FullAuto;

	}
	//Admin only
	public void RegisterAdmin(){
		damageScript.originalHealth = 500;
		damageScript.currentHealth = damageScript.originalHealth;
		secondaryWeapon.GetComponent<Fire> ().magSize = 100;
		secondaryWeapon.GetComponent<Fire> ().damagePower = 50;
		secondaryWeapon.GetComponent<Fire> ().fireType = Fire.FireTypes.FullAuto;
		this.transform.localScale = new Vector3( 3f, 3f, 3f);


	}
	#endregion
}
