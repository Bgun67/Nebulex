using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class Bot : NetworkBehaviour {

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

	public Fire fireScript;
	public Damage damageScript;
	Player_Controller[] players;
	public TargetPlayer targetPlayer;

	public int patrolIndex = 0;
	public float maxSpeed = 7f;
	[Range(0f,1f)]
	public float lockOnRate = 0.4f;
	public BotState botState;
    NavMeshAgent agent;


    // Use this for initialization
    void Start () {
        agent = GetComponent<NavMeshAgent>();
		GameObject[] _spawnPoints = GameObject.FindGameObjectsWithTag("Spawn Point 1");
		patrolPositions = new Transform[_spawnPoints.Length];
		for (int i = 0; i < patrolPositions.Length; i++)
		{
			patrolPositions[i] = _spawnPoints[i].transform;
		}
		
		agent.destination = patrolPositions[0].position;

		StartCoroutine (CheckState());
	}
		

	void Update(){

		agent.isStopped = false;

		if (botState == BotState.Patrol) {
			Patrol ();
		}
		if (botState == BotState.Alert) {

		}

		if (botState == BotState.Fighting) {
			Fight ();
		}
	}

	//Checks which state the bot should be in
	IEnumerator CheckState(){
		yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
		while (true)
		{
			yield return new WaitForSeconds(0.2f);
			//Raycast to check visibility to each player
			players = GameObject.FindObjectsOfType<Player_Controller>();
			RaycastHit _hit;

			//Check if our currently targetted player is still visible
			if (targetPlayer != null && Time.time - targetPlayer._lastSpottedTime < 5f && Vector3.Distance(transform.position, targetPlayer._transform.position) < 50f)
			{
				if (Vector3.Dot((targetPlayer._transform.position - this.transform.position).normalized, transform.forward) > 0.2f)
				{
					if (!Physics.Linecast(this.transform.position, targetPlayer._transform.position, out _hit, physicsMask) || _hit.transform.root.GetComponent<Player_Controller>() != null)
					{

						targetPlayer._lastSpottedTime = Time.time;
					}
				}
			}
			else
			{
				targetPlayer = null;
			}
			if (targetPlayer == null)
			{
				for (int i = 0; i < players.Length; i++)
				{
					if (Vector3.Dot((players[i].transform.position - this.transform.position).normalized, transform.forward) > 0.2f)
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

			if (targetPlayer != null)
			{
				botState = BotState.Fighting;
			}
			else
			{
				botState = BotState.Patrol;
			}
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
		}
		else 
		{
			//stop agent
			agent.Stop();
			//use relative position
			Vector3 _relativePosition = targetPlayer._transform.position - transform.position;
			//Lerp towards player
			transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(_relativePosition, transform.up),lockOnRate);
			//fire
			fireScript.FireWeapon(fireScript.shotSpawn.transform.position, fireScript.shotSpawn.transform.forward);
			Cmd_FireWeapon(fireScript.shotSpawn.transform.position, fireScript.shotSpawn.transform.forward);
		}

	}
	[Command]
	void Cmd_FireWeapon(Vector3 shotSpawnPosition, Vector3 shotSpawnForward){
		if(isServerOnly) fireScript.FireWeapon(shotSpawnPosition, shotSpawnForward);
		Rpc_FireWeapon(shotSpawnPosition, shotSpawnForward);
	}
	[ClientRpc(includeOwner=false)]
	void Rpc_FireWeapon(Vector3 shotSpawnPosition, Vector3 shotSpawnForward){
		fireScript.FireWeapon(shotSpawnPosition, shotSpawnForward);
	}

	public void Die(){
		
		damageScript.Reset ();
		agent.Warp (damageScript.initialPosition.position);
	}

    
	

}
