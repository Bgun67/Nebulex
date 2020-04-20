using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//TODO: Fix lobby scene players
//TODO: Increase volume range for bots
//TODO: Fix the Astroball
//wanted to keep the old bot script
public class Com_Controller : MonoBehaviour {
	public enum BotState{
		Patrol,
		Alert,
		Hiding,
		Fighting,
		Dead
	}

	public class TargetPlayer
	{
		public Transform _transform;
		//public Player_Controller _controller;
		public float _lastSpottedTime;
		public float _distance;
		public bool _hasDied;
	}
	public bool isInSpace = false;
	public LayerMask physicsMask;
    public Transform[] patrolPositions;
	public int carrierNum;
	public Animator anim;
	public float _angle;

	public Fire fireScript;
	public Damage damageScript;
	Player_Controller[] players;
	Com_Controller[] bots;
	public TargetPlayer targetPlayer;
	Transform currentCover;

	public int patrolIndex = 0;
	public float maxSpeed = 5f;
	[Range(0f,1f)]
	public float lockOnRate = 0.4f;
	public Transform head;
	public BotState botState;
    NavMeshAgent agent;

	public MetworkView netView;
	public Vector3 spaceDestination = Vector3.zero;
	public Vector3 lastSpaceDestination = Vector3.zero;
	public Vector3 lastPosition;
	List<Vector3> spaceRoute = new List<Vector3>();
	public Vector3 currentVelocity;
	private Game_Controller gameController;
	public int botID = -1;
	public TextMesh nameTextMesh;
	public GameObject ragdoll;
	



	// Use this for initialization
	void Start () {
		
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
		fireScript = GetComponentInChildren<Fire>();
		damageScript = GetComponent<Damage>();
		netView = GetComponent<MetworkView>();
		gameController = FindObjectOfType<Game_Controller> ();

		lastPosition = this.transform.position;

		gameController.bots.Add(this);
		if (head == null)
		{
			head = GameObject.Find("Main Camera").transform;
		}

		anim.SetFloat("Look Speed", 0.5f);
		anim.SetBool("Head Turn Enabled", true);
		
		ServerStart();

	}
	void ServerStart()
	{
		//TODO: fix name problem,
		//Program in jerseys, smooth lerp, reduce lag, 
		if (Metwork.peerType == MetworkPeerType.Disconnected || Metwork.isServer)
		{
			//InvokeRepeating("CheckState", Random.Range(0.1f, 0.5f), 0.2f);
			
			

			//Add the bots
			//Check if the position is already occupied by a player
			if(gameController.statsArray[this.botID - 64].isFilled == false){
				if (Metwork.peerType != MetworkPeerType.Disconnected) {
					gameController.netView.RPC ("RPC_AddPlayerStat", MRPCMode.AllBuffered, new object[] {
						"Bot " + (this.botID - 64).ToString(),
						//NetObj owner
						this.botID - 64,
						//IS bot
						true

					});
				} else {
					gameController.RPC_AddPlayerStat (
						"Bot " + (this.botID - 64).ToString(),
						this.botID - 64,
						true
					);
				}
				
			}
			else{
				this.gameObject.SetActive(false);
			}

			nameTextMesh.text = "Bot " + (this.botID - 64).ToString();

			List<Spawn_Point> _allSpawns = new List<Spawn_Point>(FindObjectsOfType<Spawn_Point>());
			_allSpawns.RemoveAll(x => x.team != gameController.statsArray[botID - 64].team);

			Spawn_Point[] _spawnPoints = _allSpawns.ToArray();
			patrolIndex = Random.Range(0, (botID - 64) % patrolPositions.Length);
			damageScript.initialPosition = _spawnPoints[(botID - 64) % _spawnPoints.Length].transform;
			//Randomize the starting position, so we don't get bot collision
			if(isInSpace){
				damageScript.initialPosition.position += Random.insideUnitSphere * Random.Range(0f, 10f);
			}
			//Randomize max speed
			maxSpeed += Random.Range(-0.4f, 0.4f);
			
			
			if(!isInSpace){
				agent.Warp(damageScript.initialPosition.position);
				agent.destination = patrolPositions[patrolIndex].position;
			}
			else{
				this.transform.position = damageScript.initialPosition.position;
				spaceDestination = patrolPositions[patrolIndex].position;
			}

		}
	}


	void Update(){
		if(isInSpace){
			agent.updateRotation = false;
			agent.updateUpAxis = false;
			agent.enabled = false;
			anim.SetBool("Float", true);
		}
		else{
			agent.enabled = true;
		}
		if(Time.frameCount % 10 == 0){
			CheckState();
		}
		fireScript.playerID = botID;

		int localTeam = gameController.GetLocalTeam();
		if (gameController.statsArray[botID - 64].team == localTeam) {
			nameTextMesh.color = new Color (0f, 50f, 255f);
			nameTextMesh.gameObject.SetActive (true);

		} else {
			nameTextMesh.color = new Color (255f, 0f, 0f);
			nameTextMesh.gameObject.SetActive (false);
		}
		nameTextMesh.transform.LookAt (gameController.localPlayer.transform);

		if (Metwork.peerType == MetworkPeerType.Disconnected || Metwork.isServer)
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
			if (Metwork.peerType != MetworkPeerType.Disconnected)
			{
				netView.RPC("RPC_SyncTransform", MRPCMode.Others, new object[] { transform.position, transform.rotation, agent.nextPosition, agent.speed });
			}
		}

		
		if((Time.frameCount + botID) % 50 == 0 && (lastSpaceDestination - spaceDestination).magnitude > 5f){
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
			currentVelocity = Vector3.Lerp(currentVelocity, desiredDirection * maxSpeed, 0.1f);
			this.transform.position +=  currentVelocity * Time.deltaTime;
			lastPosition = transform.position;
		}
		
		
		
	}
	void FootstepAnim()
	{

	}
	void Recoil()
	{
		anim.Play ("Recoil1*", 2, 0.3f);
	}

	void AnimateMovement()
	{
		anim.SetFloat("V Movement", agent.velocity.z);
		if (botState == BotState.Patrol)
		{
			anim.SetFloat("Head Turn Speed", Mathf.Clamp01(Mathf.Sin(Time.time*1f)/2f+0.5f));
			anim.SetFloat("Look Speed", 0.5f);
		}
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			netView.RPC("RPC_Animate",MRPCMode.Others, new object[]{agent.speed, anim.GetFloat("Head Turn Speed"), anim.GetFloat("Look Speed"), anim.GetBool("Scope")});
		}

	}
	[MRPC]
	public void RPC_Animate( float _agentSpeed,float _headTurn,float _lookSpeed, bool _scoped)
	{
		anim.SetFloat("Head Turn Speed", _headTurn);
		anim.SetFloat("V Movement", _agentSpeed);
		anim.SetFloat("Look Speed", _lookSpeed);
		anim.SetBool("Scope", _scoped);
	}
	[MRPC]
	public void RPC_SyncTransform(Vector3 _position, Quaternion _rotation, Vector3 _nextPos, float _speed)
	{
		//TODO: Sync over network
		if(!isInSpace){
			agent.nextPosition = _nextPos;
			agent.speed = _speed;
			if (Vector3.Distance(_position, transform.position) > 1f)
			{
				agent.Warp(_position);
			}
			transform.rotation = _rotation;
		}
	}

	//Checks which state the bot should be in
	void CheckState(){
		
		//Raycast to check visibility to each player
		//players = GameObject.FindObjectsOfType<Player_Controller>();

		List<GameObject> _playerObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
		List<Player_Controller> _playerComponents = new List<Player_Controller>();
		List<Com_Controller> _botComponents = new List<Com_Controller>();
		
		for(int i = 0; i < _playerObjects.Count; i++){
			if(_playerObjects[i].GetComponent<Player_Controller>() != null){
				_playerComponents.Add(_playerObjects[i].GetComponent<Player_Controller>());
			}
			else{
				_botComponents.Add(_playerObjects[i].GetComponent<Com_Controller>());
			}
		}
		players = _playerComponents.ToArray();
		bots = _botComponents.ToArray();
		

		//bots = GameObject.FindObjectsOfType<Com_Controller>();
		RaycastHit _hit;

		//Check if our currently targetted player is still visible
		if (targetPlayer != null && Time.time - targetPlayer._lastSpottedTime < 10f&&Vector3.Distance(transform.position, targetPlayer._transform.position)<50f) {
			if (Vector3.Angle (targetPlayer._transform.position-this.transform.position, head.transform.forward) < 70f) {
				if (!Physics.Linecast (this.transform.position, targetPlayer._transform.position, out _hit, physicsMask, QueryTriggerInteraction.Ignore) || _hit.transform.root.GetComponent<Player_Controller> () != null) {
										
					targetPlayer._lastSpottedTime = Time.time;
					targetPlayer._distance = Vector3.Distance(targetPlayer._transform.position, this.transform.position);
				}
			}
		} else {
			targetPlayer = null;
		}
		if (targetPlayer == null)
		{
			for (int i = 0; i < players.Length; i++)
			{
				if(gameController.statsArray[players[i].netObj.owner].team == gameController.statsArray[botID - 64].team){
					continue;
				}
				if (Vector3.Dot((players[i].transform.position-this.transform.position ).normalized, transform.forward) > 0.2f)
				{
					//if (_hit.distance < 50f)
					if(Vector3.Distance(this.transform.position, players[i].transform.position) < 50f)
					{
						if (!Physics.Linecast(this.transform.position, players[i].transform.position, out _hit, physicsMask, QueryTriggerInteraction.Ignore) || _hit.transform.root.GetComponent<Player_Controller>() != null)
						{
						
							
							//Check if the player is HALF of the distance of the previous player
							if (targetPlayer != null)
							{
								if (_hit.distance > targetPlayer._distance - 5f)
								{
									continue;
								}
							}
							targetPlayer = new TargetPlayer();
							targetPlayer._transform = players[i].transform;
							targetPlayer._distance = Vector3.Distance(players[i].transform.position, this.transform.position);
							//targetPlayer._controller = players[i];
							targetPlayer._lastSpottedTime = Time.time;
							targetPlayer._hasDied = false;

						}


					}
				}
			}

			for (int i = 0; i < bots.Length; i++)
			{
				if(gameController.statsArray[bots[i].botID - 64].team == gameController.statsArray[botID - 64].team || bots[i] == this){
					continue;
				}
				if (Vector3.Dot((bots[i].transform.position-this.transform.position ).normalized, transform.forward) > 0.2f)
				{
					//if (_hit.distance < 50f)
					if(Vector3.Distance(this.transform.position, bots[i].transform.position) < 50f)
					{
						if (!Physics.Linecast(this.transform.position, bots[i].transform.position, out _hit, physicsMask, QueryTriggerInteraction.Ignore) || _hit.transform.root.GetComponent<Com_Controller>() != null)
						{
						
							
							
							//Check if the player is HALF of the distance of the previous player
							if (targetPlayer != null)
							{
								if (_hit.distance > targetPlayer._distance - 5f)
								{
									continue;
								}
							}
							targetPlayer = new TargetPlayer();
							targetPlayer._transform = bots[i].transform;
							targetPlayer._distance = Vector3.Distance(bots[i].transform.position, this.transform.position);
							//targetPlayer._controller = players[i];
							targetPlayer._lastSpottedTime = Time.time;
							targetPlayer._hasDied = false;

						}


					}
				}
			}
		}
		
		TargetPlayer listenTargetPlayer = Listen(players, bots);
		
		//Check if the target player found from listening is more desirable than the current target
		if(targetPlayer != null && listenTargetPlayer != null){

			if(listenTargetPlayer._distance < targetPlayer._distance * 0.6f){
			
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
			else
			{
				botState = BotState.Fighting;
			}
		} else {
			botState = BotState.Patrol;
		}
	}
	TargetPlayer Listen(Player_Controller[] _players, Com_Controller[] _bots)
	{
		bool heard;
		TargetPlayer _targetPlayer;
		Transform heardPlayer = null;
		float loudestSound = -1000f;
		foreach (Player_Controller player in _players)
		{
			if(gameController.statsArray[player.netObj.owner].team == gameController.statsArray[botID - 64].team){
				continue;
			}
			float[] samples = new float[2];
			try
			{
				player.fireScript.shootSound.GetOutputData(samples, 0);
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
		foreach (Com_Controller player in _bots)
		{
			if(gameController.statsArray[player.botID - 64].team == gameController.statsArray[botID - 64].team){
				continue;
			}
			float[] samples = new float[2];
			try
			{
				player.fireScript.shootSound.GetOutputData(samples, 0);
			}
			catch
			{
				continue;
			}
			float _volume = samples[0] / Vector3.Distance(player.transform.position, transform.position);
			if ( _volume > loudestSound)
			{
				heardPlayer = player.transform;
			}
		}
		if (heardPlayer != null && Vector3.Distance(heardPlayer.transform.position, this.transform.position) < 50f)
		{
			_targetPlayer = new TargetPlayer();
			_targetPlayer._transform = heardPlayer.transform;
			_targetPlayer._distance = Vector3.Distance(heardPlayer.transform.position, this.transform.position);
			//targetPlayer._controller = heardPlayer;
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
			agent.CalculatePath(agent.destination, path);
		}
		else{
			spaceDestination = patrolPositions[patrolIndex].position;
			this.transform.forward = Vector3.Slerp(this.transform.forward, (spaceDestination-this.transform.position).normalized, 0.3f);
			
		}

		//
		if ((!isInSpace &&!agent.pathPending&& !agent.hasPath) || Vector3.Distance(agent.transform.position,patrolPositions[patrolIndex].position) <= 4f) {
			
			//Switch to the next patrol position
			patrolIndex++;
			if (patrolIndex >= patrolPositions.Length) {
				//Return to the first patrol position
				patrolIndex = 0;
			}
			//Reset the patrol target
			if(!isInSpace){
				agent.destination = patrolPositions[patrolIndex].position;
			}
			else{
				spaceDestination = patrolPositions[patrolIndex].position;
			}
		
		}
	}

	//Fights the player
	void Fight()
	{
		float sqrDistance = Vector3.Magnitude(targetPlayer._transform.position - transform.position);
		
		if ( sqrDistance> 20f)
		{
			if(!isInSpace){
				agent.destination = targetPlayer._transform.position;
				Debug.DrawLine(transform.position, agent.destination, Color.red);
			}
			else{
				spaceDestination = targetPlayer._transform.position;
				Debug.DrawLine(transform.position, spaceDestination, Color.red);
			}
			anim.SetBool("Scope", false);
		}
		else 
		{
			if(!isInSpace){
				agent.isStopped = true;
			}
			else{
				spaceDestination = this.transform.position;
			}
			Aim();
		}
		
		this.transform.forward = Vector3.Slerp(transform.forward,((targetPlayer._transform.position-fireScript.shotSpawn.position).normalized + (transform.forward - fireScript.shotSpawn.transform.forward)).normalized, 0.3f);//Vector3.Slerp(fireScript.shotSpawn.transform.forward, ((targetPlayer._transform.position-transform.position).normalized + (transform.forward - fireScript.shotSpawn.transform.forward)).normalized, 0.3f);

	}
	void Aim()
	{
		anim.SetBool("Scope", true);
		//use relative position
		Vector3 _relativePosition = targetPlayer._transform.position - transform.position;
		//Lerp towards player
		if(!isInSpace){
			transform.Rotate(0f,Vector3.SignedAngle( fireScript.shotSpawn.transform.forward, _relativePosition, Vector3.up)*lockOnRate,0f);
		}
		//animate so that it look up/down follows player
		_angle = Vector3.SignedAngle(fireScript.shotSpawn.forward, _relativePosition, transform.right)/100f;
		if(isInSpace){
			_angle = Vector3.SignedAngle(fireScript.shotSpawn.forward, transform.forward, transform.right)/1000f;
		}
		anim.SetFloat("Look Speed", Mathf.Lerp(anim.GetFloat("Look Speed"),Mathf.Clamp01(anim.GetFloat("Look Speed")- _angle),lockOnRate));
		if (Vector3.Angle(fireScript.shotSpawn.transform.forward, _relativePosition) < 10f)
		{
			if(!isInSpace){
				agent.Stop();
			}
			//TODO: Stop navigation

			//fire
			fireScript.FireWeapon();
		}

	}

	[MRPC]
	void RPC_Reload(){
		anim.SetTrigger ("Reload");
	}

	void UpdateUI(){
		//DO Nothing

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
	public void Die(){
		//This keeps the bots from clumping
		patrolIndex++;
		if (patrolIndex >= patrolPositions.Length) {
			//Return to the first patrol position
			patrolIndex = 0;
		}
		//sceneCam.enabled = true;
		if(Metwork.peerType  != MetworkPeerType.Disconnected){
			netView.RPC ("RPC_Die", MRPCMode.AllBuffered, new object[]{ });
		}
		else{
			RPC_Die ();
		}

		


	}

	public void CoDie(){
		
		
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

	[MRPC]
	public void RPC_Die(){
		Vector3 position = this.transform.position;
		Quaternion rotation = this.transform.rotation;
		
		GameObject _ragdollGO = (GameObject)Instantiate (ragdoll, position, rotation);
		
		_ragdollGO.GetComponentInChildren<Camera>().gameObject.SetActive(false);
		
		Destroy (_ragdollGO, 5f);

		foreach(Rigidbody _rb in _ragdollGO.GetComponentsInChildren<Rigidbody>()){

			_rb.velocity = Vector3.ClampMagnitude(this.transform.forward, 20f);
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
		Invoke("CoDie", 4f);

		


	}

    #endregion
	

}

