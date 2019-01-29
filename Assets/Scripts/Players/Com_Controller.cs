using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//wanted to keep the old bot script
public class Com_Controller : MonoBehaviour {

	public enum BotState{
		Patrol,
		Alert,
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
        agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
		fireScript = GetComponentInChildren<Fire>();
		damageScript = GetComponent<Damage>();
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
			agent.enabled = true;
			GetComponent<Rigidbody>().isKinematic = true;
			GetComponent<Rigidbody>().detectCollisions = false;
			InvokeRepeating("CheckState", Random.Range(0.1f, 0.5f), 0.2f);
			GameObject[] _spawnPoints = GameObject.FindGameObjectsWithTag("Spawn Point " + carrierNum);
			patrolPositions = new Transform[_spawnPoints.Length];
			for (int i = 0; i < patrolPositions.Length; i++)
			{
				patrolPositions[i] = _spawnPoints[i].transform;
			}
			damageScript.initialPosition = patrolPositions[0];

			agent.destination = patrolPositions[0].position;

		}
	}


	void Update(){
		if (Metwork.peerType == MetworkPeerType.Disconnected || Metwork.isServer)
		{
			agent.isStopped = false;
			AnimateMovement();
			if (botState == BotState.Patrol)
			{
				Patrol();
			}
			if (botState == BotState.Alert)
			{

			}

			if (botState == BotState.Fighting)
			{
				Fight();
			}
		}
	}
	void FootstepAnim()
	{

	}

	void AnimateMovement()
	{
		anim.SetFloat("V Movement", agent.speed);
		if (botState == BotState.Patrol)
		{
			anim.SetFloat("Head Turn Speed", Mathf.Lerp(anim.GetFloat("Head Turn Speed"),(Mathf.Sin(Time.time)+0.5f)*Random.Range(0f, 1f),0.5f));
			anim.SetFloat("Look Speed", 0.5f);
		}
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			netView.RPC("RPC_Animate",MRPCMode.All, new object[]{agent.speed, anim.GetFloat("Head Turn Speed"), anim.GetFloat("Look Speed")});
		}

	}
	[MRPC]
	void RPC_Animate( float _agentSpeed,float _headTurn,float _lookSpeed)
	{
		anim.SetFloat("Head Turn Speed", _headTurn);
		anim.SetFloat("V Movement", _agentSpeed);
		anim.SetFloat("Look Speed", _lookSpeed);
	}

	//Checks which state the bot should be in
	void CheckState(){
		//Raycast to check visibility to each player
		players = GameObject.FindObjectsOfType<Player_Controller>();
		RaycastHit _hit;

		//Check if our currently targetted player is still visible
		if (targetPlayer != null && Time.time - targetPlayer._lastSpottedTime < 5f&&Vector3.Distance(transform.position, targetPlayer._transform.position)<50f) {
			if (Vector3.Angle (targetPlayer._transform.position-this.transform.position, head.transform.forward) < 70f) {
				if (!Physics.Linecast (this.transform.position, targetPlayer._transform.position, out _hit, physicsMask) || _hit.transform.root.GetComponent<Player_Controller> () != null) {
										
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
					if (!Physics.Linecast(this.transform.position, players[i].transform.position, out _hit, physicsMask) || _hit.transform.root.GetComponent<Player_Controller>() != null)
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

		if (targetPlayer != null) {
			botState = BotState.Fighting;
		} else {
			botState = BotState.Patrol;
		}
	}

	//Patrols the ship
	void Patrol(){
		Debug.DrawLine(agent.transform.position,agent.destination);
				
		//This will keep the position in sync with the transforms position
		if (Vector3.Distance (agent.destination, patrolPositions [patrolIndex].position) >= 1f) {
			//Reset the patrol target
			agent.destination = patrolPositions[patrolIndex].position;
		}
		if (Vector3.Distance(agent.transform.position,patrolPositions[patrolIndex].position) <= 5f) {
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
		if (Vector3.Magnitude(targetPlayer._transform.position - transform.position) > 20f)
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

	public void Die(){
		
		damageScript.Reset ();
		agent.Warp (damageScript.initialPosition.position);
	}

    
	

}

