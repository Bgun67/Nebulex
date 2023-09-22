using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;
using Cinemachine;

public class Player_Controller : Player {
	float v;
	float v2;
	float h;
	float h2;
	float z;
	float down;
	float strafeFactor;
	float rollFactor;
	bool jump;

	Vector3 previousNormal = new Vector3(0,1,0);
	

	[Header("UI")]
	#region UI
	bool minimapRunning = false;

	public GameObject minimapUI;
	
	public Blackout_Effects blackoutShader;

	public GameObject helmet;

	#endregion
	[Header("Cameras")]
	#region cameras
	public float m_DOFAdaptRate;
	Vector3 originalCamPosition;
	Quaternion originalCamRotation;
	public GameObject minimapCam;
	public GameObject iconCamera;
	#endregion
	[Header("Sound")]
	#region sound
	public AudioSource breatheSound;

	#endregion
	[Space(5)]
	[Header("Weapons")]
	#region weapons

	public string[] loadoutSettings;
	[SerializeField] AnimationCurve poweredDragCurve;	
	[SerializeField] float poweredDragScale = 1;

	[SerializeField] AnimationCurve unpoweredDragCurve;	
	[SerializeField] float unpoweredDragScale = 1;
	[Range(0, 5)]
	[SerializeField]float unpoweredCutoff = 0.5f;
	[SerializeField] Transform hipTransform;

	#endregion

	Activater[] allActivaters;
	Activater targetActivater;

	Grabbable grabbable;
	[SerializeField] Transform grabbableTransform;

	

	protected override void Awake()
	{
		base.Awake();
		rb = this.GetComponent<Rigidbody> ();
		anim = this.GetComponentInChildren<Animator> ();
		player_IK = GetComponent<Player_IK>();
		wrapper = GetComponent<AudioWrapper>();
		mainCam = FindObjectOfType<CinemachineBrain>().GetComponent<Camera>();
		blackoutShader = mainCam.GetComponent<Blackout_Effects> ();
	}

	// Use this for initialization
	void Start () {
		
		gameController = Game_Controller.Instance;
		
		
		originalCamPosition = virtualCam.transform.localPosition;
		originalCamRotation = virtualCam.transform.localRotation;

		//Feet raycasts
		lfRaycast = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
		rfRaycast = anim.GetBoneTransform(HumanBodyBones.RightFoot);
		
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
			gameController.localPlayer = this;
			gameController.localTeam = gameController.playerStats[playerID].team;
			LoadPlayerData ();
			Cmd_SetPlayerName(playerName);
		}
		
		anim.SetFloat ("Look Speed", 0.5f);
	}

	//TODO: Make sure that the name is the right name
	[Command]
	void Cmd_SetPlayerName(string name){
		Game_Controller.PlayerStat stat = Game_Controller.Instance.playerStats[playerID];
		stat.name = name;
		Game_Controller.Instance.playerStats[playerID] = stat;
	}
	// runs after start basically the same
	//Check remove this function if possible
	public void Setup(){

		airTime = suffocationTime;
		grenadesNum = 4;
		if (MInput.useMouse)
		{
			Cursor.lockState = CursorLockMode.Locked;

		}
		if (isLocalPlayer) {
			SetupWeapons ();
			damageScript = this.GetComponent<Damage> ();
			damageScript.healthShown = true;
			InvokeRepeating (nameof(UpdateUI), 1f, 1f);
			LoadPlayerData ();
		}
		
		
		UpdateUI ();

	}

	void OnEnable(){

		if (isLocalPlayer) {
			if (minimapUI != null) {
				minimapUI.SetActive (true);
			}
			UI_Manager._instance.SetReticleVisibility(true);
		}
		previousNormal = transform.up;
		previousRot = Vector3.zero;

		anim.SetFloat ("Look Speed", 0.5f);

		Invoke (nameof(Setup), 0.2f);
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
		Color _originalColor = icon.material.color;
		_originalColor.a -= Time.deltaTime*0.5f;
		_originalColor.a = Mathf.Clamp01(_originalColor.a);
		icon.material.color = _originalColor;

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
			virtualCam.enabled = false;
			minimapCam.SetActive(false);
			iconCamera.SetActive (false);

			
			if(!helmet.activeSelf) helmet.SetActive(true);

			return;
		} else {
			
			if(helmet.activeSelf)helmet.SetActive(false);
		
		}
		
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
		z = Input.GetAxis ("Move Y") * moveFactor*scopeMoveFactor;
		v = Input.GetAxis ("Move Z")*moveFactor*scopeMoveFactor;
		h = Input.GetAxis ("Move X")*moveFactor*scopeMoveFactor;
		strafeFactor = Input.GetButton("Ctrl") ? 0f:1f;
		rollFactor = Input.GetButton("Ctrl") ? 1f:0f;
		jump = Input.GetButton("Jump");

		muzzleClimb -= Time.deltaTime;
		if(muzzleClimb < 0){
			muzzleClimb = 0f;
		}

		if (Input.GetButtonDown ("Reload")) {
			Reload ();
		}

		if(Input.GetButtonDown("Grab")) {
			StartGrab();
		}

		if (Input.GetButtonUp ("Grab")) {
			ReleaseGrab (false);
		}

		if (Input.GetButtonDown ("Use Item")) {
			OpenUseHUD ();
		}
		if (Input.GetButton ("Use Item")) {
			UpdateUseHUD ();
		}
		if(Input.GetButtonUp("Use Item")){
			print("button up");
			UseItem();
		}
		if (Input.GetKeyDown ("/")) {
			Cmd_KillPlayer();
		}

		if (MInput.GetButtonDown ("Switch Weapons")&&!switchWeapons) {
			Cmd_SwitchWeapons(!primarySelected);			
			UpdateUI ();
		}
			
		if (jetpackFuel< 2f) {
			jetpackFuel += Time.deltaTime/6f;
			UpdateUI ();
		} 	

		if (rb.useGravity) {
			MouseLook();
			if (jump){
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
							//Make some sort of syncvar
							Cmd_Jump( true, jetpackJets [0].isPlaying);
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
							Cmd_Jump(false, jetpackJets [0].isPlaying );
						}
					}
				}
			}
			if (Input.GetButtonDown ("Crouch")) {
				if (walkState == WalkState.Crouching) {
					Cmd_ChangeCrouch(false);

				} else {
					walkState = WalkState.Crouching;
					Cmd_ChangeCrouch(true);

					

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

        } 
		else{
			
			if (!exitingGravity)
			{
				SpaceMove();
			}
			LoseAir ();
		}

		blackoutShader?.ChangeConciousness (Mathf.Clamp01(Mathf.Pow(airTime / suffocationTime,2f)+0.3f) * 10f);
		if(isLocalPlayer){
			breatheSound.volume = Mathf.Clamp01(Mathf.Pow((suffocationTime-airTime) / suffocationTime,2f)-0.3f);
			AdjustDOF();
		}
		else{
			breatheSound.volume = 0f;
		}
		//TODO: Come up with a move elegant solution
		if (Input.GetButton ("Fire1") && !player_IK.IsSprinting() && !UI_Manager._instance.pauseMenu.gameObject.activeSelf) {
			if(grabbable){
				ReleaseGrab (true);
			}
			else{
				Attack ();
			}

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
		player_IK.SetVelocity(rb.velocity, h2);
	}
	void FixedUpdate(){
		if(grabbable&&isServer){
			Grab();
		}

	}

    private void AdjustDOF()
    {
		/*PostProcessingProfile profile = this.mainCamObj.GetComponent<PostProcessingBehaviour>().profile;
		Vector3 camPosition = this.mainCamObj.transform.position;
		Vector3 camForward = this.mainCamObj.transform.forward;
		LayerMask camLayerMask = this.mainCamObj.GetComponent<Camera>().cullingMask;
		RaycastHit hit;
		float focusDistance = 10000.0f;
		if(Physics.Raycast(camPosition, camForward, out hit, 10000.0f, camLayerMask)){
			focusDistance = hit.distance;
		}
		DepthOfFieldModel.Settings settings = profile.depthOfField.settings;
		settings.focusDistance = Mathf.Pow(10, Mathf.Lerp(Mathf.Log10(settings.focusDistance), Mathf.Log10(focusDistance), m_DOFAdaptRate));
		profile.depthOfField.settings = settings;*/
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
				break;
			case 12:
				
				 CallShip();
				print("Calling");
				break;
		}
	}

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

		


        
        /*TODO Do this all on the server
		bestShip.GetComponent<MetworkView>().RPC("RPC_ActivateAI", MRPCMode.AllBuffered, new object[] { netObj.owner });
        
        bestShip.target = this.transform;
        bestShip.GetComponent<Raycaster>().target = this.transform.position;
        bestShip.isAI = true;
        bestShip.enabled = true;
        print("Calling: " + bestShip.name);*/
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

    public override void LoseAir(){
		base.LoseAir();
		//TODO: network the damage in this function

		if((int)(airTime/suffocationTime * 100) == 50){
			WindowsVoice.Speak("Oxygen at 50%");
		}
		if((int)(airTime/suffocationTime * 100) == 10){
			WindowsVoice.Speak("Oxygen at 10%");
		}
		if((int)(airTime/suffocationTime * 100) < 2){
			WindowsVoice.Speak("Oxygen Critical <break />");
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
	public void Cmd_ActivatePlayer(){
		this.damageScript.initialPosition = null;
		this.damageScript.Reactivate();

		Rpc_ActivatePlayer();
	}
	[ClientRpc]
	public void Rpc_ActivatePlayer(){
		this.gameObject.SetActive(true);
		switchWeapons = false;
		//Reload the players ammo
		primaryWeapon.GetComponent<Fire>().RestockAmmo();
		secondaryWeapon.GetComponent<Fire>().RestockAmmo();
		if (isLocalPlayer) {
			virtualCam.enabled = true;
			minimapCam.SetActive(true);
			iconCamera.SetActive (true);
		}
	}

	#region Region1
	Vector3 currentCamVelocity;
	protected override void Aim(){
		if (fireScript == null||grappleActive)
		{
			moveFactor = 0.75f;
			return;
		}
		if (MInput.GetButton ("Left Trigger")&&!player_IK.IsSprinting()&&!fireScript.reloading &&!switchWeapons && !throwingGrenade) {
			Transform _scopeTransform = fireScript.scopePosition;
			Vector3 _scopePosition = _scopeTransform.position - _scopeTransform.forward * 0.22f + _scopeTransform.up * 0.033f;

			float _distance = Vector3.Distance(virtualCam.transform.position, _scopePosition);
			virtualCam.transform.position = Vector3.SmoothDamp(virtualCam.transform.position, _scopePosition, ref currentCamVelocity, 0.07f);

			//mainCam.transform.position = Vector3.Lerp(mainCam.transform.position,_scopePosition,0.7f);//Mathf.Clamp(0.01f/(_distance),0f,0.5f));
			virtualCam.transform.rotation = Quaternion.Lerp(virtualCam.transform.rotation,Quaternion.LookRotation(_scopeTransform.forward, _scopeTransform.up),0.7f);//Mathf.Clamp(0.01f/(_distance),0f,0.5f));

			player_IK.Scope(true);
			Zoom(true);
			lookFactor = 0.3f;
			scopeMoveFactor = 0.75f;
			
			if (Time.frameCount % 3f == 0) {
				//TODO
				/*if (Metwork.peerType != MetworkPeerType.Disconnected) {
					netView.RPC ("RPC_Aim", MRPCMode.Others, new object[]{true });
				} */
			}

		} else {
			virtualCam.transform.localPosition = Vector3.Lerp(virtualCam.transform.localPosition,originalCamPosition,0.2f*Time.deltaTime/0.034f);
			virtualCam.transform.localRotation = Quaternion.Lerp(virtualCam.transform.localRotation, originalCamRotation, 0.1f);

			Zoom(false);
			player_IK.Scope(false);
			scopeMoveFactor = 1f;
			lookFactor = 1f;

			if (Time.frameCount+1 % 3f == 0) {
				/*
				if (Metwork.peerType != MetworkPeerType.Disconnected) {
					netView.RPC ("RPC_Aim", MRPCMode.All, new object[]{false });
				} */
			}

		}
	}
	//TODO:
	//[MRPC]
	//public void RPC_Aim(bool scoped){
		
	//	anim.SetBool ("Scope", scoped);
	//}
	//zoom in to true = zooming in
	//zooms the camera on scope

	public void Zoom(bool zoomIn){
		float i = virtualCam.m_Lens.FieldOfView;


		if (zoomIn == true) {
			virtualCam.m_Lens.FieldOfView = Mathf.Lerp(i,0.5f*Game_Settings.FOV,0.5f);
		}else
		{
			virtualCam.m_Lens.FieldOfView = Mathf.Lerp(i, Game_Settings.FOV, 0.5f);
		}
	}
	
	
	public override void Jump(){
		
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

	
	void OpenUseHUD(){
		allActivaters = FindObjectsOfType<Activater>();
		UI_Manager.GetInstance.activaterUI.Open(allActivaters);
	}

	void UpdateUseHUD(){
		Activater bestActivater = null;
		float bestDistance = Mathf.Infinity;
		float bestAngle = 15f;

		foreach(Activater activater in allActivaters){
			Vector3 delta = activater.Position - virtualCam.transform.position;
			float distance = delta.magnitude;
			float angle = Vector3.Angle(delta, virtualCam.transform.forward);
			Vector3 screenpoint = mainCam.WorldToScreenPoint(activater.Position);
			UI_Manager.GetInstance.activaterUI.SetHighlight(targetActivater, false);
			UI_Manager.GetInstance.activaterUI.UpdateInfo(activater, screenpoint, distance);

			if(distance>activater.maxDistance){
				continue;
			}
			if(angle>bestAngle){
				continue;
			}
			if(activater.raycast){
				if (Physics.Linecast (virtualCam.transform.position, activater.Position)) {
				//TODO Ignore this object and player
				//	continue;
				}
			}
			bestAngle = angle;
			bestDistance = distance;
			bestActivater = activater;
		}

		targetActivater = bestActivater;
		UI_Manager.GetInstance.activaterUI.SetHighlight(targetActivater, true);
	}

	public override void UseItem(){
		if(targetActivater){
			CmdUseItem(targetActivater);
			UI_Manager.GetInstance.activaterUI.Select(targetActivater);
		}
		UI_Manager.GetInstance.activaterUI.Close();
	}

	public void CmdUseItem(Activater targetActivater){
		targetActivater.ActivateScript(gameObject);
	}

	void StartGrab(){
		grabbable = FindObjectsOfType<Grabbable>()[0];
		grabbable.GetComponent<Rigidbody>().isKinematic = true;
		player_IK.lhTarget = grabbable.handle;
	}

	Vector3 smoothVelocity;
	public void Grab(){
		
		grabbable.GetComponent<Rigidbody>().MovePosition(Vector3.Lerp(grabbable.transform.position, grabbableTransform.position, 0.5f));
		grabbable.GetComponent<Rigidbody>().MoveRotation( Quaternion.Slerp(grabbable.transform.rotation, grabbableTransform.rotation, 0.5f));
	}

	public void ReleaseGrab(bool shouldThrow){
		grabbable.GetComponent<Rigidbody>().isKinematic = false;
		if(shouldThrow){
			grabbable.GetComponent<Rigidbody>().velocity = virtualCam.transform.forward*5f;
		}
		else{
			grabbable.GetComponent<Rigidbody>().velocity = rb.velocity;
		}

		grabbable= null;
		player_IK.lhTarget = null;
		Cmd_SwitchWeapons(!primarySelected);
	}

	public void SpaceMove(){

		rb.constraints = RigidbodyConstraints.FreezeRotation;
		
		float factor = Time.deltaTime*  forceFactor;
		
		magBootsLock = false;
		//Also check if the jump key is pressed, in either auto or hold states
		if (z >= 0.05f)
		{
			magBootsOn = false;
		}
		else if (Game_Settings.currGameplaySettings.Get("hold_to_ground_lock", false))
		{
			magBootsOn = Input.GetButton("Jump");
		}
		else if (Input.GetButtonDown("Jump"))
		{
			magBootsOn = !magBootsOn;
		}


		Vector3 velocity = Vector3.zero;
		Vector3 rotation = Vector3.zero;
		float sprintFactor = 1f;
		//Boost
		if (Input.GetButton("Sprint")&&!MInput.GetButton ("Left Trigger")&&!Input.GetButton ("Fire1"))
		{
			if (jetpackFuel > 0.7f && refueling == true)
			{
				refueling = false;

			}
			if (jetpackFuel <= 0f)
			{
				refueling = true;
			}
			if (refueling == false) {
			
				foreach (ParticleSystem jet in jetpackJets) {
					jet.Play ();
				}
				thrusterSoundFactor = 1f;

				jetpackFuel -= Time.deltaTime * 0.7f;
				sprintFactor = 1.75f;
				player_IK.Sprint(true);
			} else {
				foreach (ParticleSystem jet in jetpackJets) {
					jet.Stop ();
				}
				// Always allow sprinting
				sprintFactor = 1.75f;
				player_IK.Sprint(true);
				//player_IK.Sprint(false);
			}
		
			UpdateUI ();
			
			
		}
		else
		{
			player_IK.Sprint(false);
		}

		Vector3 moveInput = Vector3.ClampMagnitude(new Vector3(h*strafeFactor,z,v), 1f);
		Vector3 desiredVelocity = transform.TransformVector(moveInput * moveSpeed * sprintFactor);
		float moveLerp = Mathf.Lerp( unpoweredDragCurve.Evaluate(rb.velocity.magnitude)*unpoweredDragScale, poweredDragCurve.Evaluate(rb.velocity.magnitude)*poweredDragScale, moveInput.magnitude/unpoweredCutoff);
		
		
		//apply normal lerping
		velocity = Vector3.Lerp(rb.velocity, desiredVelocity, moveLerp);

		
			
			
		

		if(magBootsOn){
			RaycastHit _hit = new RaycastHit();
			RaycastHit _hit2 = new RaycastHit();
			RaycastHit _hit3 = new RaycastHit();


			//Raycast from three different spots
			bool hasHit1 = Physics.SphereCast(transform.position - transform.up * 0.7f,0.1f,- transform.up * 5.4f,out _hit,5.4f, magBootsLayer,  QueryTriggerInteraction.Ignore);
			bool hasHit2 = Physics.Linecast(transform.position - transform.right * 0.1f,transform.position+ transform.right * 0.15f- transform.up * 5.4f, out _hit2, magBootsLayer, QueryTriggerInteraction.Ignore);
			bool hasHit3 = Physics.Linecast(transform.position + transform.right * 0.1f,transform.position- transform.right * 0.15f- transform.up * 5.4f, out _hit3, magBootsLayer,  QueryTriggerInteraction.Ignore);
			//Hit distance is zero if no hit
			_hit.distance = !hasHit1 ? 100000f : _hit.distance;
			//_hit2.distance = !hasHit2 ? 100000f : _hit2.distance;
			//_hit3.distance = !hasHit3 ? 100000f : _hit3.distance;

			if( _hit.distance <= 5.4f){// || _hit2.distance <= 5.4f || _hit3.distance <= 5.4f){
				//Take the weighted average of the three distances;
				Vector3 _hitNormal = (_hit.normal+_hit2.normal+_hit3.normal).normalized; //(_hit.normal / _hit.distance + _hit2.normal / _hit2.distance + _hit3.normal / _hit3.distance)/3f; 
				
				if(Vector3.Dot(_hitNormal.normalized, previousNormal.normalized)> 0.4f){

					_hitNormal = Vector3.Slerp(previousNormal,_hitNormal, 0.5f);

				}
				else{
					
					_hitNormal = Vector3.Slerp(_hitNormal, previousNormal, 0.95f);
				}
				
				
				float _hitDistance = 1f / (1f/_hit.distance);// + 1f/_hit2.distance + 1f/_hit3.distance);
				Vector3 _hitPoint = _hit.point;//(_hit.point / _hit.distance + _hit2.point / _hit2.distance + _hit3.point / _hit3.distance) / (_hitDistance); 
				
				//print(_hit.point + " " + _hitPoint);

				Vector3 _lerpedForward = Vector3.Slerp(transform.forward,Vector3.ProjectOnPlane(transform.forward, _hitNormal), 0.3f );//* (1f-_hitDistance/5.4f));
				Vector3 _lerpedUp =  Vector3.Slerp(transform.up,_hitNormal,0.3f);//*( 1f-_hitDistance/5.4f));
				previousNormal = _lerpedUp;
				rb.MoveRotation(Quaternion.LookRotation(_lerpedForward,_lerpedUp));
				Debug.DrawRay(rb.centerOfMass, transform.forward, Color.blue);
				Debug.DrawRay(this.transform.position, Vector3.ProjectOnPlane(transform.forward,_hitNormal), Color.yellow);
				Debug.DrawRay(_hit.point,_hitNormal, Color.green);


				float lookUpDownTime = anim.GetFloat("Look Speed");	
				anim.SetFloat("Look Speed", Mathf.Clamp(lookUpDownTime + v2 * 2f*Time.deltaTime, -1f, 1f));
				if (Time.frameCount % 4==0) {
					/*TODO
					if (Metwork.peerType != MetworkPeerType.Disconnected) {
						netView.RPC ("RPC_Look", MRPCMode.Others, new object[]{ lookUpDownTime });
					}*/
				}

				rotation = new Vector3(0, h2 * 2f, 0) * 5f * Time.deltaTime * 30f;
				rb.transform.Rotate(rotation);
				velocity = transform.TransformVector(Vector3.ClampMagnitude(new Vector3(h,z,v),1) * moveSpeed);
				//TODO: Make leaf fall down
				velocity += transform.up * -9810f /Mathf.Clamp(0.01f, 10000f, (_hitDistance * _hitDistance)) * Time.deltaTime * 60f;

				if(_hit.distance <= 0.2f){
					//TODO: Finishe
					anim.SetBool("Float", false);
					thrusterSoundFactor = 0f;

					magBootsLock = true;
				}
				else{
					magBootsLock = false;
				}
				
				
			}
		}
		
		//Avoid orbiting around the player while the camera is outside the player body (in spawing)
		if (Vector3.Distance(virtualCam.transform.localPosition, originalCamPosition) > 3f)
		{
			velocity = Vector3.zero;
			rotation = Vector3.zero;
		}
		else if(!magBootsLock)
		{	
			anim.SetBool("Float", true);

			//Try to right the player's body and camera (as in exit gravity)
			Vector3 _aimDirection = virtualCam.transform.forward;
			float _originalLookTime = anim.GetFloat("Look Speed");
			Vector3 _originalForward = transform.forward;

			float _counter = Mathf.Abs(0.5f-_originalLookTime);
			if (_counter > 0.03f)
			{
				Vector3 _lerpedForward = Vector3.Slerp(_originalForward, _aimDirection, 0.3f);
				Vector3 _lerpedUp = Vector3.ProjectOnPlane(transform.up, _lerpedForward);
				rb.transform.rotation = Quaternion.LookRotation(_lerpedForward, _lerpedUp);
				anim.SetFloat("Look Speed", 0.5f - 0.1f *(0.5f-_originalLookTime));
			}


			float _timeFactor = 2*30f;
			rotation = new Vector3(
				-v2 *_timeFactor,//*Time.deltaTime*60f,//Mathf.Lerp(previousRot.x, -v2 * 2f*_timeFactor , 0.2f*Time.deltaTime*20f),
				h2 *_timeFactor,//*Time.deltaTime*60f,//Mathf.Lerp(previousRot.y, h2 * 2f*_timeFactor , 0.2f*Time.deltaTime*20f),
				Mathf.Lerp(previousRot.z, -h * rollFactor*5f*Time.deltaTime*60f , 0.1f)
			);
			previousRot = rotation;
			//TODO
			//Vector3 _pivot = rb.position - transform.up * 1.5f;
			//rb.MovePosition(Quaternion.Euler(rotation) * (rb.transform.position - _pivot) + _pivot);
			
			rb.MoveRotation(rb.transform.rotation*Quaternion.Euler(rotation));
			Debug.DrawLine(transform.position + rb.centerOfMass,transform.position + rb.centerOfMass + 10f * transform.forward, Color.blue);
			//rb.transform.RotateAround(mainCam.transform.position, mainCam.transform.up, rotation.y);
			//rb.transform.RotateAround(mainCam.transform.position, mainCam.transform.right, rotation.x);
			//rb.transform.RotateAround(mainCam.transform.position, mainCam.transform.forward, rotation.z);

			thrusterSoundFactor = (velocity - rb.velocity).magnitude/Time.deltaTime>10f?1f:0f;
		}

		rb.velocity = velocity;
		
		if (Time.frameCount % 4 == 0) {
			/*TODO
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netView.RPC ("RPC_AnimateSpace", MRPCMode.Others, new object[]{ thrusterSoundFactor});
			} */
		}
		
	}
	public override void AnimateMovement(){
		anim.SetFloat ("H Movement", h*moveFactor*scopeMoveFactor);
		anim.SetFloat ("V Movement", v*moveFactor*scopeMoveFactor);
		anim.SetInteger ("Walk State", (int)walkState);
		
		float forwardSpeed = v+Mathf.Abs(h2)/2f;//Mathf.Clamp(transform.InverseTransformVector (rb.velocity).z, -5f, 5f);

		if (Time.frameCount % 4 == 0) {
			/*TODO
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netView.RPC ("RPC_AnimateMovement", MRPCMode.Others, new object[]{ (int)walkState, h*moveFactor*scopeMoveFactor,v*moveFactor*scopeMoveFactor, thrusterSoundFactor});
			}*/ 
		}
	}
	//TODO:
	[MRPC]
	public void RPC_AnimateMovement(int state, float hSpeed, float vSpeed, float _thrusterSoundFactor){
		anim.SetFloat ("H Movement", hSpeed);
		anim.SetFloat ("V Movement", vSpeed);
		anim.SetInteger ("Walk State", state);
		thrusterSoundFactor = _thrusterSoundFactor;
		//walkSound.volume = Mathf.Clamp01 (Mathf.Abs (hSpeed + vSpeed));
	}
	//TODO:
	[MRPC]
	public void RPC_AnimateSpace(float _thrusterSoundFactor){
		thrusterSoundFactor = _thrusterSoundFactor;
		//walkSound.volume = Mathf.Clamp01 (Mathf.Abs (hSpeed + vSpeed));
	}
	//TODO:
	[MRPC]
	public override void RPC_ExitVehicle()
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
			Cmd_SetLookTime(lookUpDownTime);
		}

		if(magBootsLock){
			hipTransform.localRotation = Quaternion.Euler(Vector3.right*(lookUpDownTime-0.5f)*90f);
		}

	
	}
	
	public void TurnHead(){
		anim.SetFloat ("Head Turn Speed", -h2*0.5f+0.5f);
		anim.SetBool("Head Turn Enabled", true);

	}

	public override void Attack(){
		if (!grappleActive)
		{
			//Spread of the gun
			fireScript.shotSpawn.transform.forward = this.virtualCam.transform.forward 
													+ muzzleClimb * fireScript.recoilAmount * virtualCam.transform.up * 0.3f
													+ fireScript.recoilAmount * (0.1f + muzzleClimb) * (Vector3)(Random.insideUnitCircle) * (anim.GetBool ("Scope") ? 0.1f : 0.3f)
													;
			if(muzzleClimb < 0.6f){
				muzzleClimb += 0.05f;
			}
			bool fired = fireScript.FireWeapon(fireScript.shotSpawn.transform.position, fireScript.shotSpawn.transform.forward);
			Cmd_FireWeapon(fireScript.shotSpawn.transform.position, fireScript.shotSpawn.transform.forward);

			if (fired) { 
				player_IK.Recoil(fireScript.recoilAmount);
				
				};
		}
		UpdateUI ();
	}
	

	#endregion
	#region Region2
	
	public override void Die(){
		//Since the damage is calculated on the server, this function only
		//runs on the server and has to be passed to every client
		if(throwingGrenade){
			//TODO: Grenades
			SpawnGrenade(0.25f);
		}
		//Invoke the die animations on all clients
		Rpc_Die();
	}

	[ClientRpc]
	public override void Rpc_Die(){
		Vector3 position = this.transform.position;
		Quaternion rotation = this.transform.rotation;
		inVehicle = false;
		
		
		GameObject ragdollGO = (GameObject)Instantiate (ragdoll, position, rotation);
		Destroy (ragdollGO, 10f);
		foreach(Rigidbody _rb in ragdollGO.GetComponentsInChildren<Rigidbody>()){

			_rb.velocity = Vector3.ClampMagnitude(rb.velocity*Random.Range(0.9f, 1.1f), 20f);
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



        //this.transform.position = Vector3.up * 10000f;
        this.gameObject.SetActive (false);

		if(isLocalPlayer){
			ragdollGO.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>().enabled = true;
			virtualCam.enabled = false;
		}

		StartCoroutine(CoDie(ragdollGO));
		
	}
	public override IEnumerator CoDie(GameObject ragdollGO){
		yield return new WaitForSeconds(4f);
		if (isLocalPlayer) {
			ragdollGO.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>().enabled = false;
			UI_Manager._instance.SetReticleVisibility(false);

			if (!SceneManager.GetSceneByName("SpawnScene").isLoaded)
			{
				SceneManager.LoadScene("SpawnScene", LoadSceneMode.Additive);
			}
		}
	}



	//Called by grav controller when entering / exiting gravity;
	public override IEnumerator ExitGravity(){
		exitingGravity = true;
		
		rb.angularDrag = 0f;
		rb.drag = 0.0f;
		rb.constraints = RigidbodyConstraints.None;
		anim.SetBool ("Float", true);
		anim.SetBool ("Jump", false);
		anim.SetInteger("Walk State", 0);


		//Avoid orbiting around the player while the camera is outside the player body (in spawing)
		while(Vector3.Distance(virtualCam.transform.localPosition, originalCamPosition) > 3f){
			yield return new WaitForSeconds(0.5f);
		}

		Vector3 _aimDirection = virtualCam.transform.forward;
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

		//walkSound.Stop ();

		lookFactor = 1f;
		exitingGravity = false;

	}
	public override IEnumerator EnterGravity(){
		if (enteringGravity)
		{
			yield return null;
		}
		enteringGravity = true;
		rb.drag = 0.3f;

		anim.SetBool ("Float", false);
		
		rb.angularVelocity = Vector3.zero;

		float _counter = 0f;

		//Avoid orbiting around the player while the camera is outside the player body (in spawing)
		while(Vector3.Distance(virtualCam.transform.localPosition, originalCamPosition) > 3f){
			yield return new WaitForSeconds(0.5f);
		}

		Vector3 newForwardVector;
		if(shipRB){
			newForwardVector = Vector3.ProjectOnPlane(transform.forward, shipRB.transform.up);//new Vector3(transform.forward.x, 0f, transform.forward.z);
		}else{
			newForwardVector = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
		}
		Vector3 _originalForward = virtualCam.transform.forward;
		float _aimDirection = Mathf.Clamp01(0.5f-Vector3.SignedAngle( newForwardVector,virtualCam.transform.forward, transform.right)/250f);
		
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
	
	public override IEnumerator Co_SwitchWeapons(bool _primary){
		yield return StartCoroutine(base.Co_SwitchWeapons(_primary));

		if(isLocalPlayer){
			UI_Manager._instance.ChangeWeapon (primarySelected);
		}	
		
	}
	
	public void SetupWeapons(){
		try {
			loadoutSettings = Util.LushWatermelon(System.IO.File.ReadAllLines (Application.persistentDataPath+"/Loadout Settings.txt"));
			string loadoutSettingsString = "";
			foreach(string line in loadoutSettings){
				loadoutSettingsString+=line+'/';
			}

			string[] splitString = loadoutSettingsString.Split('/');
			int _primary = GunNameToInt(splitString[0]);
			int _secondary = GunNameToInt(splitString[1]);
			int _primaryScope = ScopeNameToInt(splitString[2]);
			int _secondaryScope = ScopeNameToInt(splitString[3]);

			if(_primary < 0 || _secondary < 0 || _primaryScope < 0 || _secondaryScope < 0){
				throw new System.Exception();
			}

			Cmd_LoadWeaponData(_primary,_secondary,_primaryScope,_secondaryScope);

		} catch{


			print ("Failed to read loadout file, restoring default settings");
			loadoutSettings = Profile.RestoreLoadoutFile ();
			string loadoutSettingsString = "";
			foreach(string line in loadoutSettings){
				loadoutSettingsString+=line+'/';
			}

			string[] splitString = loadoutSettingsString.Split('/');
			int _primary = GunNameToInt(splitString[0]);
			int _secondary = GunNameToInt(splitString[1]);
			int _primaryScope = ScopeNameToInt(splitString[2]);
			int _secondaryScope = ScopeNameToInt(splitString[3]);

			if(_primary < -1 || _secondary < -1 || _primaryScope < -1 || _secondaryScope < -1){
				Debug.LogError("Loadout file could not be properly recreated");
			}

			Cmd_LoadWeaponData(_primary,_secondary,_primaryScope,_secondaryScope);
			
		}
	}
	[Command]
	void Cmd_LoadWeaponData(int _primary, int _secondary, int _primaryScope, int _secondaryScope){
		//The hooked function will probably run 4 times but c'est la vie
		primaryWeaponNum = _primary;
		secondaryWeaponNum = _secondary;
		primaryScopeNum = _primaryScope;
		secondaryScopeNum = _secondaryScope;

		//Check: Not sure whether the server runs hooks or not
		if(isServerOnly) LoadWeaponData(0, 0);
		
	}
	
	protected override void LoadWeaponData(int oldValue, int newValue){
		

		Destroy(this.primaryWeapon);
		Destroy(this.secondaryWeapon);

		primaryWeaponPrefab = Game_Controller.Instance.weaponsCatalog.guns[primaryWeaponNum];//(GameObject)Resources.Load ("Weapons/" + settings [0]);
		secondaryWeaponPrefab = Game_Controller.Instance.weaponsCatalog.guns[secondaryWeaponNum];//(GameObject)Resources.Load ("Weapons/" + settings [1]);

		
		//add the primary weapon to the finger
		primaryWeapon = (GameObject)Instantiate (primaryWeaponPrefab, finger);
		primaryWeapon.transform.localPosition = Vector3.zero;
		primaryWeapon.transform.localRotation = Quaternion.identity;
		primaryWeapon.GetComponent<Fire> ().OnReloadEvent = new Fire.ReloadEvent(Reload);
		

		secondaryWeapon = (GameObject)Instantiate (secondaryWeaponPrefab, finger);
		secondaryWeapon.transform.localPosition = Vector3.zero;
		secondaryWeapon.transform.localRotation = Quaternion.identity;
		secondaryWeapon.SetActive (false);
		secondaryWeapon.GetComponent<Fire> ().OnReloadEvent = new Fire.ReloadEvent(Reload);
		
		
		//scopes now
		if(primaryScopeNum >= 0){
			GameObject primaryScope = Game_Controller.Instance.weaponsCatalog.scopes[primaryScopeNum];
			Instantiate (primaryScope, primaryWeapon.GetComponent<Fire> ().scopePosition);
		}
		if(secondaryScopeNum >= 0){
			GameObject secondaryScope = Game_Controller.Instance.weaponsCatalog.scopes[secondaryScopeNum];
			Instantiate (secondaryScope, secondaryWeapon.GetComponent<Fire> ().scopePosition);
		}


		//CHECK The latency on this switch may be too much to bear, it may be better to do it locally
		SwitchWeapons(true, true);
		
		
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
			gameController.playerStats [playerID].name = playerName;
			Invoke("RegisterAdmin",2f);

		}
		else if (playerName.StartsWith ("%^A#R*7**MODERATOR")) {
			playerName = playerName.Remove (0, 18);
			gameController.playerStats [playerID].name = playerName;
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
			gameController.playerStats [playerID].name = playerName;
			Invoke("RegisterAdmin",2f);
		}
		else if (playerName.StartsWith ("%^A#R*7**MODERATOR")) {
			playerName = playerName.Remove (0, 18);
			gameController.playerStats [playerID].name = playerName;
			Invoke("RegisterModerator",2f);
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
		blackoutShader?.ChangeBlood (Mathf.Clamp01(1-damageScript.currentHealth/100f));


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
