using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
		public Player_Controller _controller;
		public float _lastSpottedTime;
		public float _distance;
		public bool _hasDied;
	}

	public LayerMask physicsMask;
    public Transform[] patrolPositions;
	public int carrierNum;
	public Animator anim;
	public float _angle;

	public Fire fireScript;
	public Damage damageScript;
	Player_Controller[] players;
	public TargetPlayer targetPlayer;
	Transform currentCover;

	public int patrolIndex = 0;
	public float maxSpeed = 7f;
	[Range(0f,1f)]
	public float lockOnRate = 0.4f;
	public Transform head;
	public BotState botState;
    NavMeshAgent agent;

	public MetworkView netView;



	// Use this for initialization
	void Start () {
		Util.ShowMessage("Hi");
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
		fireScript = GetComponentInChildren<Fire>();
		damageScript = GetComponent<Damage>();
		netView = GetComponent<MetworkView>();
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
		if (Metwork.peerType == MetworkPeerType.Disconnected || Metwork.isServer)
		{
			InvokeRepeating("CheckState", Random.Range(0.1f, 0.5f), 0.2f);
			 /* GameObject[] _spawnPoints = GameObject.FindGameObjectsWithTag("Spawn Point " + carrierNum);
			patrolPositions = new Transform[_spawnPoints.Length];
			for (int i = 0; i < patrolPositions.Length; i++)
			{
				patrolPositions[i] = _spawnPoints[i].transform;
			}*/
			damageScript.initialPosition = patrolPositions[0];

			agent.destination = patrolPositions[0].position;

		}
	}


	void Update(){
		if (Metwork.peerType == MetworkPeerType.Disconnected || Metwork.isServer)
		{
			agent.isStopped = false;
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
	}
	void FootstepAnim()
	{

	}
	void Recoil()
	{
		anim.Play ("Recoil 1*", 2, 0.3f);

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
			netView.RPC("RPC_Animate",MRPCMode.Others, new object[]{agent.speed, anim.GetFloat("Head Turn Speed"), anim.GetFloat("Look Speed"), anim.GetFloat("Scope")});
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
		agent.nextPosition = _nextPos;
		agent.speed = _speed;
		if (Vector3.Distance(_position, transform.position) > 1f)
		{
			agent.Warp(_position);
		}
		transform.rotation = _rotation;
	}

	//Checks which state the bot should be in
	void CheckState(){
		//Raycast to check visibility to each player
		players = GameObject.FindObjectsOfType<Player_Controller>();
		RaycastHit _hit;

		//Check if our currently targetted player is still visible
		if (targetPlayer != null && Time.time - targetPlayer._lastSpottedTime < 10f&&Vector3.Distance(transform.position, targetPlayer._transform.position)<50f) {
			if (Vector3.Angle (targetPlayer._transform.position-this.transform.position, head.transform.forward) < 70f) {
				if (!Physics.Linecast (this.transform.position, targetPlayer._transform.position, out _hit, physicsMask, QueryTriggerInteraction.Ignore) || _hit.transform.root.GetComponent<Player_Controller> () != null) {
										
					targetPlayer._lastSpottedTime = Time.time;
				}
			}
		} else {
			targetPlayer = null;
		}
		if (targetPlayer == null)
		{
			for (int i = 0; i < players.Length; i++)
			{
				if (Vector3.Dot((players[i].transform.position-this.transform.position ).normalized, transform.forward) > 0.2f)
				{
					if (!Physics.Linecast(this.transform.position, players[i].transform.position, out _hit, physicsMask, QueryTriggerInteraction.Ignore) || _hit.transform.root.GetComponent<Player_Controller>() != null)
					{
						if (_hit.distance < 50f)
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
							targetPlayer._controller = players[i];
							targetPlayer._lastSpottedTime = Time.time;
							targetPlayer._hasDied = false;

						}


					}
				}
			}
		}
		if (targetPlayer == null)
		{
			targetPlayer = Listen(players);
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
	TargetPlayer Listen(Player_Controller[] players)
	{
		bool heard;
		Player_Controller heardPlayer = null;
		float loudestSound = 0f;
		foreach (Player_Controller player in players)
		{
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
			if ( _volume> loudestSound)
			{
				heardPlayer = player;
			}
		}
		if (heardPlayer != null)
		{
			targetPlayer = new TargetPlayer();
			targetPlayer._transform = heardPlayer.transform;
			targetPlayer._controller = heardPlayer;
			targetPlayer._lastSpottedTime = Time.time;
			targetPlayer._hasDied = false;
			return targetPlayer;
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
		agent.destination = patrolPositions[patrolIndex].position;
		
		agent.CalculatePath(agent.destination, path);

		if (path.status == NavMeshPathStatus.PathPartial||Vector3.Distance(agent.transform.position,patrolPositions[patrolIndex].position) <= 5f) {
			//Switch to the next patrol position
			patrolIndex++;
			if (patrolIndex >= patrolPositions.Length) {
				//Return to the first patrol position
				patrolIndex = 0;
			}
			//Reset the patrol target
			agent.destination = patrolPositions[patrolIndex].position;
		
		}
	}

	//Fights the player
	void Fight()
	{
		float sqrDistance = Vector3.Magnitude(targetPlayer._transform.position - transform.position);
		
		if ( sqrDistance> 20f)
		{
			agent.destination = targetPlayer._transform.position;
			Debug.DrawLine(transform.position, agent.destination, Color.red);
			anim.SetBool("Scope", false);
		}
		else 
		{
			agent.isStopped = true;
			Aim();
		}

	}
	void Aim()
	{
		anim.SetBool("Scope", true);
		//use relative position
			Vector3 _relativePosition = targetPlayer._transform.position - transform.position;
			//Lerp towards player
			transform.Rotate(0f,Vector3.SignedAngle( fireScript.shotSpawn.transform.forward, _relativePosition, Vector3.up)*lockOnRate,0f);
		//animate so that it look up/down follows player
		 _angle = Vector3.SignedAngle(fireScript.shotSpawn.forward, _relativePosition, transform.right)/100f;
		anim.SetFloat("Look Speed", Mathf.Lerp(anim.GetFloat("Look Speed"),Mathf.Clamp01(anim.GetFloat("Look Speed")- _angle),lockOnRate));
		if (Vector3.Angle(fireScript.shotSpawn.transform.forward, _relativePosition) < 10f)
		{
			agent.Stop();
			//fire
			fireScript.FireWeapon();
		}

	}
	void Hide()
	{
		Vector3 _coverPosition;
		if (Time.frameCount % 3 == 0)
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
			agent.SetDestination(_coverPosition);


		}
		else if(currentCover == null||(!agent.pathPending&& !agent.hasPath))
		{
			//Strafe
			agent.SetDestination(transform.position + transform.right);
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
			if (NavMesh.SamplePosition(_hit1.point, out _hit2, 0.05f, NavMesh.AllAreas))
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

	public void Die(){
		
		damageScript.Reset ();
		agent.Warp (damageScript.initialPosition.position);
	}

    
	

}

