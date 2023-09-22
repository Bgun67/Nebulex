﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

//TODO: Fix lobby scene players
//TODO: Increase volume range for bots
//TODO: Fix the Astroball
//wanted to keep the old bot script
public class Com_Controller : Player {
	public enum BotState{
		Patrol,
		Alert,
		Hiding,
		Fighting,
		Dead
	}

	public enum DifficultyLevel
	{
		Easy = 1,
		Good = 2,
		Hard = 3,
	}
	
	
	[System.Serializable]
	public class TargetPlayer
	{
		public Transform _transform;
		//public Player_Controller _controller;
		public float _acquiredTime;
		public float _lastSpottedTime;
		public float _distance;
		public float _angle;
		public bool _hasDied;
	}
	public bool isInSpace = false;
	public LayerMask physicsMask;
    public GameObject[] patrolPositions;
	
	//public Animator anim;
	public float _angle;
	[SerializeField] DifficultyLevel difficultyLevel = DifficultyLevel.Good;
	[SerializeField]
	ComDifficulty[] difficulties;
	ComDifficulty difficultySetting;

	//public Fire fireScript;
	//public Damage damageScript;
	Player[] players;
	Com_Controller[] bots;
	public TargetPlayer targetPlayer;
	Transform currentCover;

	public int patrolIndex = 0;
	[Range(0f,1f)]
	public float lockOnRate = 0.4f;
	//public Transform rightHandPosition;
	public BotState botState;
    NavMeshAgent agent;

	public MetworkView netView;
	public Vector3 spaceDestination = Vector3.zero;
	public Vector3 lastSpaceDestination = Vector3.zero;
	public Vector3 lastPosition;
	List<Vector3> spaceRoute = new List<Vector3>();
	public Vector3 currentVelocity;
	//private Game_Controller gameController;
	//[SyncVar]
	//public int playerID = -1;
	//public GameObject ragdoll;
	float lastCheckFrame;



	// Use this for initialization
	void Start () {
		
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponentInChildren<Animator>();
		fireScript = GetComponentInChildren<Fire>();
		damageScript = GetComponent<Damage>();
		netView = GetComponent<MetworkView>();
		player_IK = GetComponent<Player_IK>();
		gameController = Game_Controller.Instance;
		patrolPositions = GameObject.FindGameObjectsWithTag("Patrol Position");
		difficultySetting = difficulties[(int)difficultyLevel];
		lastPosition = this.transform.position;

		fireScript.totalAmmo = (int)10000;
		fireScript.damagePower = (int)(difficultySetting.DamageFactor*fireScript.damagePower);


		anim.SetFloat("Look Speed", 0.5f);
		anim.SetBool("Head Turn Enabled", true);
		
		ServerStart();

	}
	void ServerStart()
	{
		//TODO: fix name problem,
		//Program in jerseys, smooth lerp, reduce lag, 
		if (CustomNetworkManager.IsServerMachine())
		{

			List<Spawn_Point> _allSpawns = new List<Spawn_Point>(FindObjectsOfType<Spawn_Point>());
			_allSpawns.RemoveAll(x => x.team != gameController.playerStats[playerID].team);

			Spawn_Point[] _spawnPoints = _allSpawns.ToArray();
			patrolIndex = Random.Range(0, (playerID) % patrolPositions.Length);
			damageScript.initialPosition = _spawnPoints[(playerID) % _spawnPoints.Length].transform;
			//Randomize the starting position, so we don't get bot collision
			if(isInSpace){
				damageScript.initialPosition.position += Random.insideUnitSphere * Random.Range(0f, 10f);
			}
			//Randomize max speed
			moveSpeed += Random.Range(-0.4f, 0.4f);
			
			
			if(!isInSpace){
				agent.Warp(damageScript.initialPosition.position);
				agent.destination = patrolPositions[patrolIndex].transform.position;
			}
			else{
				this.transform.position = damageScript.initialPosition.position;
				spaceDestination = patrolPositions[patrolIndex].transform.position;
			}

		}
	}


	void Update(){
		Color _originalColor = icon.material.color;
		_originalColor.a -= Time.deltaTime;
		_originalColor.a = Mathf.Clamp01(_originalColor.a);
		icon.material.color = _originalColor;
		
		if(isInSpace){
			agent.updateRotation = false;
			agent.updateUpAxis = false;
			agent.enabled = false;
			anim.SetBool("Float", true);
		}
		else{
			if(!agent.enabled){
				agent.enabled = true;
			}
		}
		//TODO: Uncouple from frameRate
		if(isServer && Time.frameCount % 13 == 0){
			CheckState();
		}

		player_IK.rhOffset = fireScript.rhOffset;
		player_IK.gunPosition = rightHandPosition;
		player_IK.gripPosition = fireScript.lhTarget;
		fireScript.playerID = playerID;

		int localTeam = gameController.localTeam;
		

		if (isServer)
		{
			if(!isInSpace){
				agent.isStopped = false;
			}
			
			AnimateMovement();
			switch (botState)
			{
				case BotState.Patrol:
					Patrol();
					break;
				case BotState.Alert:
					break;
				case BotState.Hiding:
					Hide();
					break;
				case BotState.Fighting:
					Fight();
					break;
				default:
					break;
			}
			
		}

		
		if(Time.frameCount % Game_Controller.Instance.maxPlayers == playerID && (lastSpaceDestination - spaceDestination).magnitude > 5f){
			Nav_Volume_Builder._instance.FindRoute(this.transform.position, spaceDestination);
			lastSpaceDestination = spaceDestination;
			//Perform Deep copy
			spaceRoute = new List<Vector3>(Nav_Volume_Builder._instance.plannedRoute);	

		}
		if(spaceRoute.Count > 0){
			Debug.DrawLine(spaceRoute[0], this.transform.position, Color.green);
			if((spaceRoute[0]-this.transform.position).sqrMagnitude < 8f){
				spaceRoute.RemoveAt(0);
			}
		}
		if(spaceRoute.Count > 0){
			Vector3 desiredDirection = (spaceRoute[0]-this.transform.position).normalized;
			currentVelocity = Vector3.Lerp(currentVelocity, desiredDirection * moveSpeed, 0.05f);
			this.transform.position +=  currentVelocity * Time.deltaTime;
			lastPosition = transform.position;
		}
		
		//player_IK.SetVelocity(rb.velocity);

		
	}
	
	void Recoil()
	{
		anim.Play ("Recoil1*", 2, 0.3f);
	}

	public override void AnimateMovement()
	{
		anim.SetFloat("V Movement", agent.velocity.magnitude*1.5f/agent.speed);
		if (botState == BotState.Patrol)
		{
			anim.SetFloat("Head Turn Speed", Mathf.Clamp01(Mathf.Sin(Time.time*1f)/2f+0.5f));
			anim.SetFloat("Look Speed", 0.5f);
		}
		

	}
	

	//Checks which state the bot should be in
	void CheckState()
	{

		players = FindObjectsOfType<Player>();




		//bots = GameObject.FindObjectsOfType<Com_Controller>();
		RaycastHit _hit;

		//Check if our currently targetted player is still visible
		if (targetPlayer != null && targetPlayer._transform != null && Time.time - targetPlayer._lastSpottedTime < difficultySetting.AttentionSpan && Vector3.Distance(transform.position, targetPlayer._transform.position) < difficultySetting.ViewDistance)
		{
			if (Vector3.Angle(targetPlayer._transform.position - virtualCam.transform.position, virtualCam.transform.forward) < difficultySetting.ViewAngle)
			{
				if (!Physics.Linecast(virtualCam.transform.position, targetPlayer._transform.position, out _hit, physicsMask, QueryTriggerInteraction.Ignore) || _hit.transform.root.GetComponent<Player_Controller>() != null)
				{

					targetPlayer._lastSpottedTime = Time.time;
					targetPlayer._acquiredTime += (Time.time-lastCheckFrame) / (Vector3.Distance(transform.position, targetPlayer._transform.position) + Vector3.Angle(targetPlayer._transform.position - virtualCam.transform.position, virtualCam.transform.forward));
					targetPlayer._distance = Vector3.Distance(targetPlayer._transform.position, virtualCam.transform.position);
				}
			}
		}
		else
		{
			targetPlayer = null;
		}
		//if (targetPlayer == null)
		//{
		// Find New Target
		for (int i = 0; i < players.Length; i++)
		{
			//TODO players[i].netObj.owner
			if (gameController.playerStats[players[i].playerID].team == gameController.playerStats[playerID].team)
			{
				continue;
			}
			float angle = Vector3.Angle((players[i].transform.position - virtualCam.transform.position).normalized, virtualCam.transform.forward);
			if (angle < difficultySetting.ViewAngle)
			{
				float distance = Vector3.Distance(virtualCam.transform.position, players[i].transform.position);
				if (distance < difficultySetting.ViewDistance)
				{
					if (!Physics.Linecast(virtualCam.transform.position, players[i].transform.position, out _hit, physicsMask, QueryTriggerInteraction.Ignore) || _hit.transform.root.GetComponent<Player_Controller>() != null)
					{


						//Check if the player is less than the distance of the previous player
						if (targetPlayer != null)
						{
							if (distance * 0.2f * angle * 0.8f > targetPlayer._distance * 0.2f + targetPlayer._angle * 0.8f)
							{
								continue;
							}
						}
						targetPlayer = new TargetPlayer();
						targetPlayer._transform = players[i].transform;
						targetPlayer._distance = distance;
						targetPlayer._angle = angle;
						targetPlayer._acquiredTime += (Time.time-lastCheckFrame) / (distance + angle);
						//targetPlayer._controller = players[i];
						targetPlayer._lastSpottedTime = Time.time;
						targetPlayer._hasDied = false;

					}


				}
			}
		}


		//}

		TargetPlayer listenTargetPlayer = Listen(players);

		//Check if the target player found from listening is more desirable than the current target
		if (targetPlayer != null && listenTargetPlayer != null)
		{

			if (listenTargetPlayer._distance < targetPlayer._distance * 0.6f)
			{

				targetPlayer = listenTargetPlayer;
			}
		}
		if (targetPlayer == null)
		{
			targetPlayer = listenTargetPlayer;
		}

		if (targetPlayer != null) {
			if (damageScript.currentHealth < damageScript.originalHealth*0.8f&&Vector3.Distance(targetPlayer._transform.position, transform.position)>5f)
			{
				botState = BotState.Hiding;

			}
			//else if(targetPlayer._acquiredTime*difficultySetting>0.1f)
			//{
			//	botState = BotState.Fighting;
			//}
			else 
			{
				botState = BotState.Fighting;
			}
		} else {
			botState = BotState.Patrol;
		}
		lastCheckFrame = Time.time;
	}
	TargetPlayer Listen(Player[] _players)
	{
		TargetPlayer _targetPlayer;
		Transform heardPlayer = null;
		float loudestSound = -1000f;
		foreach (Player player in _players)
		{
			//TODO player.netObj.owner
			if(gameController.playerStats[player.playerID].team == gameController.playerStats[playerID].team){
				continue;
			}
			float[] samples = new float[2];
			try
			{
				player.fireScript.source.GetOutputData(samples, 0);
			}
			catch
			{
				continue;
			}
			float _volume = samples[0] / Vector3.Distance(player.transform.position, transform.position) * 100f;
			
			if ( _volume > loudestSound)
			{
				heardPlayer = player.transform;
			}
		}
		
		if (heardPlayer != null && Vector3.Distance(heardPlayer.transform.position, this.transform.position) < difficultySetting.ListenDistance)
		{
			_targetPlayer = new TargetPlayer();
			_targetPlayer._transform = heardPlayer.transform;
			_targetPlayer._distance = Vector3.Distance(heardPlayer.transform.position, this.transform.position);
			//targetPlayer._controller = heardPlayer;
			_targetPlayer._acquiredTime += (Time.time-lastCheckFrame) / (Vector3.Distance(heardPlayer.transform.position, this.transform.position));

			_targetPlayer._lastSpottedTime = Time.time;
			_targetPlayer._hasDied = false;
			return _targetPlayer;
		}
		else
		{
			return null;
		}
	}
	//Patrols the ship
	void Patrol(){
		Debug.DrawLine(agent.transform.position,agent.destination);
		//This will keep the position in sync with the transforms position
		NavMeshPath path = new NavMeshPath();

		//Reset the patrol target
		if(!isInSpace){
			//agent.destination = patrolPositions[patrolIndex].position;
			//TEST: Move the agent calculate position farther
			//agent.CalculatePath(agent.destination, path);
		}
		else{
			spaceDestination = patrolPositions[patrolIndex].transform.position;
			Vector3 _rotation = Vector3.RotateTowards(this.transform.forward, (spaceDestination-this.transform.position).normalized, difficultySetting.PatrolRotationSpeed, 1.5f);
			transform.rotation = Quaternion.LookRotation(_rotation);
		}

		//
		if ((!isInSpace &&!agent.pathPending&& !agent.hasPath) || Vector3.Distance(agent.transform.position,patrolPositions[patrolIndex].transform.position) <= 4f) {
			
			//Switch to the next patrol position
			patrolIndex++;
			if (patrolIndex >= patrolPositions.Length) {
				//Return to the first patrol position
				patrolIndex = 0;
			}
			//Reset the patrol target
			if(!isInSpace){
				agent.destination = patrolPositions[patrolIndex].transform.position;
				agent.CalculatePath(agent.destination, path);
			}
			else{
				spaceDestination = patrolPositions[patrolIndex].transform.position;
			}
		
		}
	}
	

	//Fights the player
	void Fight()
	{
		float distance = Vector3.Magnitude(targetPlayer._transform.position - transform.position);
		RaycastHit _hit;
		bool _canSeePlayer = !Physics.Linecast(virtualCam.transform.position, targetPlayer._transform.position, out _hit, physicsMask, QueryTriggerInteraction.Ignore);
		//target out of range, move forward before firing
		if ( distance>difficultySetting.FiringRange || !_canSeePlayer)
		{
			if(!isInSpace){
				agent.destination = targetPlayer._transform.position;
				Debug.DrawLine(transform.position, agent.destination, Color.red);
			}
			else{
				spaceDestination = targetPlayer._transform.position;
				Debug.DrawLine(transform.position, spaceDestination, Color.red);
			}
		}
		//move forward while firing
		else if ( distance> difficultySetting.AdvanceDistance)
		{
			if(!isInSpace){
				agent.destination = targetPlayer._transform.position;
				Debug.DrawLine(transform.position, agent.destination, Color.red);
			}
			else{
				spaceDestination = targetPlayer._transform.position;
				Debug.DrawLine(transform.position, spaceDestination, Color.red);
			}
			player_IK.Scope(true);

			Aim();
		}
		
		else 
		{
			if(!isInSpace){
				agent.isStopped = true;
			}
			else{
				spaceDestination = this.transform.position;
			}
			player_IK.Scope(false);
			Aim();
		}
		Vector3 _rotation = Vector3.RotateTowards(transform.forward, (targetPlayer._transform.position-transform.position).normalized, 0.4f/*difficultySetting.FightingRotationSpeed*Time.deltaTime*/, 1f);
		transform.rotation = Quaternion.LookRotation(_rotation);
		
	}
	protected override void Aim()
	{

		//use relative position
		Vector3 _relativePosition = targetPlayer._transform.position - virtualCam.transform.position;
		//Lerp towards player
		if(!isInSpace){
			transform.Rotate(0f,Vector3.SignedAngle( virtualCam.transform.forward, _relativePosition, Vector3.up)*lockOnRate,0f);
		}
		//animate so that it look up/down follows player
		_angle = Vector3.SignedAngle(virtualCam.transform.forward, _relativePosition, transform.right)/100f;
		if(isInSpace){
			_angle = Vector3.SignedAngle(virtualCam.transform.forward, transform.forward, transform.right)/1000f;
		}
		anim.SetFloat("Look Speed", Mathf.Lerp(anim.GetFloat("Look Speed"),Mathf.Clamp01(anim.GetFloat("Look Speed")- _angle),lockOnRate));
		Debug.DrawLine(transform.position, transform.position+_relativePosition, Color.white);
		if (Vector3.Angle(fireScript.shotSpawn.transform.forward, _relativePosition) < difficultySetting.MaxAimOffsetBeforeFiring)
		{
			
			if(!isInSpace){
				agent.Stop();
			}
			//TODO: Stop navigation
			float varianceFactor = difficultySetting.AccuracyOverTime.Evaluate(targetPlayer._acquiredTime);
			Vector3 variance = new Vector3(
				Mathf.Sin(Mathf.Deg2Rad*Random.Range(-varianceFactor, varianceFactor)),
				Mathf.Sin(Mathf.Deg2Rad*Random.Range(-varianceFactor, varianceFactor)),
				1f);
			//fire
			fireScript.FireWeapon(virtualCam.transform.position+virtualCam.transform.forward*0.5f, virtualCam.transform.TransformDirection(variance));
			Debug.DrawRay(virtualCam.transform.position+virtualCam.transform.forward*0.5f, virtualCam.transform.TransformDirection(variance)*100f, Color.magenta);

			//Bots only exist on the server
			Rpc_FireWeapon(virtualCam.transform.position+virtualCam.transform.forward*0.5f, virtualCam.transform.TransformDirection(variance));
		}

	}

	void Hide()
	{
		Vector3 _coverPosition;
		if (Time.frameCount % 10 == 0)
		{
			currentCover = FindCover();
		}
		if (currentCover != null)
		{
			Vector3 _displacement = currentCover.transform.position - targetPlayer._transform.position;
			_coverPosition = currentCover.transform.position+_displacement.normalized * 2f;

			
			//crouch and allow to fire
			if (Vector3.Distance(transform.position, _coverPosition) < 2f)
			{
				if (fireScript.magAmmo < 5)
				{
					anim.SetBool("Crouching", true);
					fireScript.Reload();
				}
				Aim();
			}
			else
			{
				anim.SetBool("Crouching", false);
			}
			if(!isInSpace){
				agent.SetDestination(_coverPosition);
			}
			else{
				Aim();
				spaceDestination = _coverPosition;
			}

		}
		else if(currentCover == null||(!isInSpace && !agent.pathPending&& !agent.hasPath))
		{
			//Strafe
			
			if(!isInSpace){
				agent.SetDestination(transform.position + transform.right);
			}
			else{
				spaceDestination = transform.position + transform.right*10f;
			}
			Aim();
		}

	}


	Transform FindCover()
	{
		Transform cover = null;
		//find distance to check a circle for cover - we don't really want the com to be forced to move forward 
		float _distance = Vector3.Distance(targetPlayer._transform.position, transform.position)*2f;
		Collider[] _colliders = Physics.OverlapSphere(targetPlayer._transform.position, _distance, physicsMask, QueryTriggerInteraction.Ignore);
		float _minCoverDistance = 100;
		foreach (Collider collider in _colliders)
		{
			if (collider.transform.root == this.transform)
			{
				continue;
			}
			Vector3 _displacement = collider.transform.position - targetPlayer._transform.position;
			Vector3 _testPosition = collider.transform.position+_displacement.normalized * 2f;
			//raycast down to make sure we're checking a floor and not a ceiling
			RaycastHit _hit1;
			if (Physics.Raycast(_testPosition, Vector3.down, out _hit1))
			{
				_testPosition = _hit1.point;
			}
			else
			{
				continue;
			}
			//check to make sure there is a collider between where the com would be and target
			if (!Physics.Linecast(_hit1.point + Vector3.up * 0.4f, targetPlayer._transform.position,physicsMask, QueryTriggerInteraction.Ignore))
			{
				continue;
			}

			NavMeshHit _hit2;
			if (isInSpace || NavMesh.SamplePosition(_hit1.point, out _hit2, 0.05f, NavMesh.AllAreas))
			{
				float _distanceToCover = Vector3.Distance(transform.position, _testPosition);
				if ( _distanceToCover< _minCoverDistance)
				{
					_minCoverDistance = _distanceToCover;
					cover = collider.transform;
					Debug.DrawLine(targetPlayer._transform.position, cover.position, Color.green);

				}
			}
		}
		return cover;

	}


	#region Region2
	public override void Die(){
		//This keeps the bots from clumping
		patrolIndex++;
		if (patrolIndex >= patrolPositions.Length) {
			//Return to the first patrol position
			patrolIndex = 0;
		}
		Rpc_Die();
	}

	public override void CoDie(){
		
		
		if (Metwork.peerType == MetworkPeerType.Disconnected || Metwork.isServer)
		{
			damageScript.Reset ();
			if(!isInSpace){
				agent.Warp (damageScript.initialPosition.position);
			}
			else{
				this.transform.position = damageScript.initialPosition.position;
			}
			
		}
		this.gameObject.SetActive(true);
	}

	[ClientRpc]
	public override void Rpc_Die(){
		Vector3 position = this.transform.position;
		Quaternion rotation = this.transform.rotation;
		
		GameObject _ragdollGO = (GameObject)Instantiate (ragdoll, position, rotation);
				
		Destroy (_ragdollGO, 5f);

		foreach(Rigidbody _rb in _ragdollGO.GetComponentsInChildren<Rigidbody>()){

			_rb.velocity = Vector3.ClampMagnitude(Random.insideUnitSphere*Random.Range(0.9f, 1.1f), 20f);
			_rb.useGravity = !isInSpace;
		}
		
		try{
			GameObject droppedWeapon = (GameObject)Instantiate (fireScript.gameObject, position, rotation);
			droppedWeapon.AddComponent<Rigidbody> ().useGravity = !isInSpace;
			droppedWeapon.GetComponent<Fire> ().enabled = false;
			droppedWeapon.transform.localScale = this.fireScript.gameObject.transform.lossyScale;

			Destroy (droppedWeapon, 20f);
		
		}
		catch{}



        this.transform.position = Vector3.up * 10000f;
        this.gameObject.SetActive (false);
		Invoke(nameof(CoDie), 4f);

	}

    #endregion

}

