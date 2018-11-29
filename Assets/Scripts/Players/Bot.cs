using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour {

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
		public bool _hasDied;
	}

	public LayerMask physicsMask;
    public Transform[] patrolPositions;

	public Fire fireScript;
	public Damage damageScript;
	Player_Controller[] players;
	public TargetPlayer targetPlayer;

	public int patrolIndex = 0;

	public BotState botState;
    NavMeshAgent agent;


    // Use this for initialization
    void Start () {
        agent = GetComponent<NavMeshAgent>();
		agent.destination = patrolPositions[0].position;

		InvokeRepeating ("CheckState", Random.Range(0.1f,0.5f), 0.2f);
	}
		

	void Update(){

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
	void CheckState(){
		//Raycast to check visibility to each player
		players = GameObject.FindObjectsOfType<Player_Controller>();
		RaycastHit _hit;

		//Check if our currently targetted player is still visible
		if (targetPlayer != null && Time.time - targetPlayer._lastSpottedTime < 5f) {
			if (Vector3.Dot (this.transform.position.normalized, transform.forward) > 0.2f) {
				if (!Physics.Linecast (this.transform.position, targetPlayer._transform.position, out _hit, physicsMask) || _hit.transform.root.GetComponent<Player_Controller> () != null) {
					targetPlayer._lastSpottedTime = Time.time;
				}
			}
		} else {
			targetPlayer = null;
		}

		for (int i = 0; targetPlayer == null && i < players.Length; i++) {
			if (Vector3.Dot (this.transform.position.normalized, transform.forward) > 0.2f) {
				if (!Physics.Linecast (this.transform.position, players [i].transform.position, out _hit, physicsMask) || _hit.transform.root.GetComponent<Player_Controller> () != null) {
					//Check if the player is HALF of the distance of the previous player
					targetPlayer = new TargetPlayer ();
					targetPlayer._transform = players [i].transform;
					targetPlayer._controller = players [i];
					targetPlayer._lastSpottedTime = Time.time;
					targetPlayer._hasDied = false;


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
	void Fight(){
		agent.destination = targetPlayer._transform.position;
		fireScript.FireWeapon ();
	}

	public void Die(){
		
		damageScript.Reset ();
		agent.Warp (damageScript.initialPosition.position);
	}

    
	

}
